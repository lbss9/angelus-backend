using Angelus.Application.Auth.Commands;
using Angelus.Application.Interfaces;
using Angelus.Domain.Entities;
using Angelus.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Angelus.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userRepo.Object, _jwtService.Object);
    }

    [Fact]
    public async Task Handle_NewEmail_ReturnsToken()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync("angel@test.com")).ReturnsAsync(false);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _jwtService.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var result = await _handler.HandleAsync(
            new RegisterCommand("angel@test.com", "Password@123")
        );

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Handle_ExistingEmail_ReturnsFailure()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync("angel@test.com")).ReturnsAsync(true);

        var result = await _handler.HandleAsync(
            new RegisterCommand("angel@test.com", "Password@123")
        );

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email já cadastrado.");
    }
}
