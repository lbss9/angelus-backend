using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Principados.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Principados.Api.Hubs;

[Authorize]
public class GameHub(AppDbContext db) : Hub
{
    private static readonly ConcurrentDictionary<string, PlayerState> Players = new();

    public async Task JoinWorld(Guid characterId)
    {
        var userId = Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var character = await db.Characters.FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId);

        if (character is null)
        {
            await Clients.Caller.SendAsync("Error", "Personagem não encontrado.");
            return;
        }

        var player = new PlayerState
        {
            ConnectionId = Context.ConnectionId,
            CharacterId = character.Id,
            Name = character.Name,
            AngelType = character.AngelType,
            X = 0, Y = 0, Z = 0, RotY = 0
        };

        Players[Context.ConnectionId] = player;

        await Clients.Caller.SendAsync("WorldState", new { players = Players.Values });
        await Clients.Others.SendAsync("PlayerJoined", player);
    }

    public async Task Move(float x, float y, float z, float rotY)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;

        player.X = x; player.Y = y; player.Z = z; player.RotY = rotY;

        await Clients.Others.SendAsync("PlayerMoved", new
        {
            id = player.CharacterId,
            x = player.X, y = player.Y, z = player.Z, rotY = player.RotY
        });
    }

    public async Task SendChat(string message)
    {
        if (!Players.TryGetValue(Context.ConnectionId, out var player)) return;
        if (string.IsNullOrWhiteSpace(message)) return;

        message = message.Length > 200 ? message[..200] : message;

        await Clients.All.SendAsync("ChatMessage", new
        {
            characterName = player.Name,
            angelType = player.AngelType,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Players.TryRemove(Context.ConnectionId, out var player))
            await Clients.Others.SendAsync("PlayerLeft", new { id = player.CharacterId });

        await base.OnDisconnectedAsync(exception);
    }
}

public class PlayerState
{
    public string ConnectionId { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AngelType { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float RotY { get; set; }
}
