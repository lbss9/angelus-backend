using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Angelus.Tests.Integration;

public class CharacterIntegrationTests(AngelusWebAppFactory factory)
    : IClassFixture<AngelusWebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync() => await factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<string> RegisterAndLoginAsync(string email = "char@angelus.com")
    {
        var payload = new { email, password = "senha123" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return body!.Token;
    }

    [Fact]
    public async Task GetCharacters_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/characters");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCharacter_ValidData_Returns201()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/characters", new
        {
            name = "Serafim",
            angelType = "sol"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CharacterResponse>();
        body!.Name.Should().Be("Serafim");
        body.AngelType.Should().Be("sol");
    }

    [Fact]
    public async Task CreateCharacter_InvalidAngelType_Returns409()
    {
        var token = await RegisterAndLoginAsync("invalid@angelus.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/characters", new
        {
            name = "Serafim",
            angelType = "fogo"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetCharacters_AfterCreate_ReturnsList()
    {
        var token = await RegisterAndLoginAsync("list@angelus.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/characters", new { name = "Luna", angelType = "lua" });

        var response = await _client.GetAsync("/api/characters");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<List<CharacterResponse>>();
        list.Should().HaveCount(1);
        list![0].Name.Should().Be("Luna");
    }

    [Fact]
    public async Task DeleteCharacter_OwnCharacter_Returns204()
    {
        var token = await RegisterAndLoginAsync("delete@angelus.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await _client.PostAsJsonAsync("/api/characters", new { name = "Rosa", angelType = "rosa" });
        var character = await create.Content.ReadFromJsonAsync<CharacterResponse>();

        var response = await _client.DeleteAsync($"/api/characters/{character!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private record TokenResponse(string Token, Guid UserId);
    private record CharacterResponse(Guid Id, string Name, string AngelType, DateTime CreatedAt);
}
