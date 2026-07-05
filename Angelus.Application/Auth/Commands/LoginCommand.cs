using Angelus.Application.Auth.DTOs;
using Angelus.Application.Common;
using Angelus.Application.Interfaces;
using Angelus.Domain.Interfaces;

namespace Angelus.Application.Auth.Commands;

public record LoginCommand(string Email, string Password);

public class LoginCommandHandler(IUserRepository userRepository, IJwtService jwtService)
{
    public async Task<Result<AuthResponse>> HandleAsync(LoginCommand command)
    {
        var user = await userRepository.GetByEmailAsync(command.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure(
                Error.Unauthorized("Email ou senha inválidos.", "INVALID_CREDENTIALS")
            );

        var token = jwtService.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(token, user.Id));
    }
}
