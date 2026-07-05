using Angelus.Application.Characters.DTOs;
using Angelus.Domain.Interfaces;

namespace Angelus.Application.Characters.Queries;

public record GetCharactersQuery(Guid UserId);

public class GetCharactersQueryHandler(ICharacterRepository characterRepository)
{
    public async Task<List<CharacterResponse>> HandleAsync(GetCharactersQuery query)
    {
        var characters = await characterRepository.GetByUserIdAsync(query.UserId);
        return characters.Select(c => new CharacterResponse(c.Id, c.Name, c.AngelType, c.CreatedAt)).ToList();
    }
}
