using Principados.Application.Auth.DTOs;
using Principados.Application.Common;
using Principados.Application.Interfaces;
using Principados.Domain.Interfaces;

namespace Principados.Application.Auth.Commands;

public record LoginCommand(string Email, string Password);

public class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService)
{
    public async Task<Result<AuthResponse>> HandleAsync(LoginCommand command)
    {
        var user = await userRepository.GetByEmailAsync(command.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("Email ou senha inválidos.");

        var token = jwtService.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(token, user.Id));
    }
}
