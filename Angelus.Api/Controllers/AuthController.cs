using Microsoft.AspNetCore.Mvc;
using Angelus.Application.Auth.Commands;

namespace Angelus.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    RegisterCommandHandler registerHandler,
    LoginCommandHandler loginHandler) : ControllerBase
{
    public record AuthRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        var result = await registerHandler.HandleAsync(new RegisterCommand(request.Email, request.Password));
        if (!result.IsSuccess)
            return Conflict(new { message = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest request)
    {
        var result = await loginHandler.HandleAsync(new LoginCommand(request.Email, request.Password));
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.Error });

        return Ok(result.Value);
    }
}
