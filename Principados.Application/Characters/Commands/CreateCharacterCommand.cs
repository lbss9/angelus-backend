using Principados.Application.Characters.DTOs;
using Principados.Application.Common;
using Principados.Domain.Entities;
using Principados.Domain.Interfaces;

namespace Principados.Application.Characters.Commands;

public record CreateCharacterCommand(Guid UserId, string Name, string AngelType);

public class CreateCharacterCommandHandler(ICharacterRepository characterRepository)
{
    private static readonly string[] ValidAngelTypes = ["sol", "lua", "rosa"];

    public async Task<Result<CharacterResponse>> HandleAsync(CreateCharacterCommand command)
    {
        if (!ValidAngelTypes.Contains(command.AngelType))
            return Result<CharacterResponse>.Failure("Tipo de anjinho inválido. Use: sol, lua ou rosa.");

        if (await characterRepository.UserHasCharacterAsync(command.UserId))
            return Result<CharacterResponse>.Failure("Você já possui um personagem.");

        if (await characterRepository.ExistsByNameAsync(command.Name))
            return Result<CharacterResponse>.Failure("Este nome já está em uso.");

        var character = new Character
        {
            UserId = command.UserId,
            Name = command.Name,
            AngelType = command.AngelType
        };

        await characterRepository.AddAsync(character);

        return Result<CharacterResponse>.Success(
            new CharacterResponse(character.Id, character.Name, character.AngelType, character.CreatedAt));
    }
}
