using Angelus.Application.Characters.DTOs;
using Angelus.Application.Common;
using Angelus.Domain.Entities;
using Angelus.Domain.Interfaces;

namespace Angelus.Application.Characters.Commands;

public record CreateCharacterCommand(Guid UserId, string Name, string AngelType);

public class CreateCharacterCommandHandler(ICharacterRepository characterRepository)
{
    private static readonly string[] ValidAngelTypes = ["sol", "lua", "rosa"];

    public async Task<Result<CharacterResponse>> HandleAsync(CreateCharacterCommand command)
    {
        if (!ValidAngelTypes.Contains(command.AngelType))
            return Result<CharacterResponse>.Failure(
                Error.Validation(
                    "Tipo de anjinho inválido. Use: sol, lua ou rosa.",
                    "INVALID_ANGEL_TYPE"
                )
            );

        if (await characterRepository.UserHasCharacterAsync(command.UserId))
            return Result<CharacterResponse>.Failure(
                Error.Conflict("Você já possui um personagem.", "CHARACTER_ALREADY_EXISTS")
            );

        if (await characterRepository.ExistsByNameAsync(command.Name))
            return Result<CharacterResponse>.Failure(
                Error.Conflict("Este nome já está em uso.", "NAME_ALREADY_TAKEN")
            );

        var character = new Character
        {
            UserId = command.UserId,
            Name = command.Name,
            AngelType = command.AngelType,
        };

        await characterRepository.AddAsync(character);

        return Result<CharacterResponse>.Success(
            new CharacterResponse(
                character.Id,
                character.Name,
                character.AngelType,
                character.CreatedAt
            )
        );
    }
}
