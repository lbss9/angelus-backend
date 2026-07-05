using Principados.Application.Characters.DTOs;
using Principados.Domain.Interfaces;

namespace Principados.Application.Characters.Queries;

public record GetCharactersQuery(Guid UserId);

public class GetCharactersQueryHandler(ICharacterRepository characterRepository)
{
    public async Task<List<CharacterResponse>> HandleAsync(GetCharactersQuery query)
    {
        var characters = await characterRepository.GetByUserIdAsync(query.UserId);
        return characters.Select(c => new CharacterResponse(c.Id, c.Name, c.AngelType, c.CreatedAt)).ToList();
    }
}
