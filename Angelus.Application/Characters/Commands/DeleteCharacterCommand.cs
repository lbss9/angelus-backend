using Angelus.Application.Common;
using Angelus.Domain.Interfaces;

namespace Angelus.Application.Characters.Commands;

public record DeleteCharacterCommand(Guid UserId, Guid CharacterId);

public class DeleteCharacterCommandHandler(ICharacterRepository characterRepository)
{
    public async Task<Result<bool>> HandleAsync(DeleteCharacterCommand command)
    {
        var character = await characterRepository.GetByIdAsync(command.CharacterId);

        if (character is null || character.UserId != command.UserId)
            return Result<bool>.Failure(
                Error.NotFound("Personagem não encontrado.", "CHARACTER_NOT_FOUND")
            );

        await characterRepository.DeleteAsync(character);
        return Result<bool>.Success(true);
    }
}
