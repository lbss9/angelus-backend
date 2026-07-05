using Principados.Application.Common;
using Principados.Domain.Interfaces;

namespace Principados.Application.Characters.Commands;

public record DeleteCharacterCommand(Guid UserId, Guid CharacterId);

public class DeleteCharacterCommandHandler(ICharacterRepository characterRepository)
{
    public async Task<Result<bool>> HandleAsync(DeleteCharacterCommand command)
    {
        var character = await characterRepository.GetByIdAsync(command.CharacterId);

        if (character is null || character.UserId != command.UserId)
            return Result<bool>.Failure("Personagem não encontrado.");

        await characterRepository.DeleteAsync(character);
        return Result<bool>.Success(true);
    }
}
