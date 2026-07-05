using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Angelus.Tests.Integration;

public class AuthIntegrationTests(AngelusWebAppFactory factory)
    : IClassFixture<AngelusWebAppFactory>,
        IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync() => await factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ValidData_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "serafim@angelus.com", password = "senha123" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var payload = new { email = "duplicado@angelus.com", password = "senha123" };
        await _client.PostAsJsonAsync("/api/auth/register", payload);

        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var payload = new { email = "login@angelus.com", password = "senha123" };
        await _client.PostAsJsonAsync("/api/auth/register", payload);

        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "teste@angelus.com", password = "certa" }
        );

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "teste@angelus.com", password = "errada" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record TokenResponse(string Token, Guid UserId);
}
