using System.Text;
using Angelus.Api.Common;
using Angelus.Api.Hubs;
using Angelus.Application.Auth.Commands;
using Angelus.Application.Characters.Commands;
using Angelus.Application.Characters.Queries;
using Angelus.Infrastructure;
using Angelus.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (ctx, services, config) =>
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
);

builder.Services.AddInfrastructure(builder.Configuration);

// Tratamento global de exceções → contrato ApiError
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Handlers CQRS
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<GetCharactersQueryHandler>();
builder.Services.AddScoped<CreateCharacterCommandHandler>();
builder.Services.AddScoped<DeleteCharacterCommandHandler>();

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/gamehub"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins("http://localhost:5500", "http://127.0.0.1:5500", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    )
);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (doc, ctx, ct) =>
        {
            foreach (var server in doc.Servers ?? [])
                server.Url = server.Url?.TrimEnd('/') ?? server.Url;
            return Task.CompletedTask;
        }
    );
});

builder
    .Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("angelus-api"))
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation())
    .WithMetrics(m =>
        m.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddPrometheusExporter()
    );

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Exceções não tratadas → ApiError com status 500 (deve vir primeiro no pipeline)
app.UseExceptionHandler();

// Status gerados pelo framework sem corpo (401 sem token, 404 rota inexistente) → ApiError
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    // Só formata respostas de erro que ainda não têm corpo próprio
    if (response.HasStarted || !string.IsNullOrEmpty(response.ContentType))
        return;

    var error = ApiError.FromStatus(response.StatusCode, context.HttpContext.Request.Path);
    await response.WriteAsJsonAsync(error);
});

app.MapOpenApi();
app.MapPrometheusScrapingEndpoint(); // /metrics para Prometheus/Grafana
app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();

public partial class Program { }
