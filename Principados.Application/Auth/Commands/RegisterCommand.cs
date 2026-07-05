using Principados.Application.Auth.DTOs;
using Principados.Application.Common;
using Principados.Application.Interfaces;
using Principados.Domain.Entities;
using Principados.Domain.Interfaces;

namespace Principados.Application.Auth.Commands;

public record RegisterCommand(string Email, string Password);

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService)
{
    public async Task<Result<AuthResponse>> HandleAsync(RegisterCommand command)
    {
        if (await userRepository.ExistsByEmailAsync(command.Email))
            return Result<AuthResponse>.Failure("Email já cadastrado.");

        var user = new User
        {
            Email = command.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password)
        };

        await userRepository.AddAsync(user);

        var token = jwtService.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(token, user.Id));
    }
}
