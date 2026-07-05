using Angelus.Application.Auth.DTOs;
using Angelus.Application.Common;
using Angelus.Application.Interfaces;
using Angelus.Domain.Entities;
using Angelus.Domain.Interfaces;

namespace Angelus.Application.Auth.Commands;

public record RegisterCommand(string Email, string Password);

public class RegisterCommandHandler(IUserRepository userRepository, IJwtService jwtService)
{
    private static readonly RegisterCommandValidator _validator = new();

    public async Task<Result<AuthResponse>> HandleAsync(RegisterCommand command)
    {
        var validation = await _validator.ValidateAsync(command);
        if (!validation.IsValid)
            return Result<AuthResponse>.Failure(
                Error.Validation(validation.Errors[0].ErrorMessage)
            );

        if (await userRepository.ExistsByEmailAsync(command.Email))
            return Result<AuthResponse>.Failure(
                Error.Conflict("Email já cadastrado.", "EMAIL_ALREADY_EXISTS")
            );

        var user = new User
        {
            Email = command.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password),
        };

        await userRepository.AddAsync(user);

        var token = jwtService.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(token, user.Id));
    }
}
