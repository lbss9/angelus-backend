using System.Security.Claims;
using Angelus.Application.Characters.Commands;
using Angelus.Application.Characters.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Angelus.Api.Controllers;

[ApiController]
[Route("api/characters")]
[Authorize]
public class CharacterController(
    GetCharactersQueryHandler getCharactersHandler,
    CreateCharacterCommandHandler createCharacterHandler,
    DeleteCharacterCommandHandler deleteCharacterHandler
) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await getCharactersHandler.HandleAsync(new GetCharactersQuery(UserId)));

    public record CreateCharacterRequest(string Name, string AngelType);

    [HttpPost]
    public async Task<IActionResult> Create(CreateCharacterRequest request)
    {
        var result = await createCharacterHandler.HandleAsync(
            new CreateCharacterCommand(UserId, request.Name, request.AngelType)
        );

        if (!result.IsSuccess)
            return Conflict(new { message = result.Error });

        return Created($"/api/characters/{result.Value!.Id}", result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await deleteCharacterHandler.HandleAsync(
            new DeleteCharacterCommand(UserId, id)
        );
        if (!result.IsSuccess)
            return NotFound(new { message = result.Error });

        return NoContent();
    }
}
