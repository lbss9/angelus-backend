using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Principados.Application.Interfaces;
using Principados.Domain.Interfaces;
using Principados.Infrastructure.Data;
using Principados.Infrastructure.Repositories;
using Principados.Infrastructure.Services;

namespace Principados.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
