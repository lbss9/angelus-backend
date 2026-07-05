using Angelus.Application.Auth.Commands;
using Angelus.Application.Interfaces;
using Angelus.Domain.Entities;
using Angelus.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Angelus.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepo.Object, _jwtService.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        var user = new User
        {
            Email = "angel@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
        };
        _userRepo.Setup(r => r.GetByEmailAsync("angel@test.com")).ReturnsAsync(user);
        _jwtService.Setup(j => j.GenerateToken(user)).Returns("jwt-token");

        var result = await _handler.HandleAsync(new LoginCommand("angel@test.com", "password123"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFailure()
    {
        var user = new User
        {
            Email = "angel@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
        };
        _userRepo.Setup(r => r.GetByEmailAsync("angel@test.com")).ReturnsAsync(user);

        var result = await _handler.HandleAsync(
            new LoginCommand("angel@test.com", "wrong-password")
        );

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email ou senha inválidos.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("ghost@test.com")).ReturnsAsync((User?)null);

        var result = await _handler.HandleAsync(new LoginCommand("ghost@test.com", "any"));

        result.IsSuccess.Should().BeFalse();
    }
}
