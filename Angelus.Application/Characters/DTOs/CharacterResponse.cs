namespace Angelus.Application.Characters.DTOs;

public record CharacterResponse(Guid Id, string Name, string AngelType, DateTime CreatedAt);
