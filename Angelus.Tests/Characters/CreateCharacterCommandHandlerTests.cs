using Angelus.Application.Characters.Commands;
using Angelus.Domain.Entities;
using Angelus.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Angelus.Tests.Characters;

public class CreateCharacterCommandHandlerTests
{
    private readonly Mock<ICharacterRepository> _characterRepo = new();
    private readonly CreateCharacterCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateCharacterCommandHandlerTests()
    {
        _handler = new CreateCharacterCommandHandler(_characterRepo.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCharacter()
    {
        _characterRepo.Setup(r => r.UserHasCharacterAsync(_userId)).ReturnsAsync(false);
        _characterRepo.Setup(r => r.ExistsByNameAsync("Serafim")).ReturnsAsync(false);
        _characterRepo.Setup(r => r.AddAsync(It.IsAny<Character>())).Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(
            new CreateCharacterCommand(_userId, "Serafim", "sol")
        );

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Serafim");
        result.Value.AngelType.Should().Be("sol");
    }

    [Fact]
    public async Task Handle_InvalidAngelType_ReturnsFailure()
    {
        var result = await _handler.HandleAsync(
            new CreateCharacterCommand(_userId, "Serafim", "fogo")
        );

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("inválido");
    }

    [Fact]
    public async Task Handle_UserAlreadyHasCharacter_ReturnsFailure()
    {
        _characterRepo.Setup(r => r.UserHasCharacterAsync(_userId)).ReturnsAsync(true);

        var result = await _handler.HandleAsync(
            new CreateCharacterCommand(_userId, "Serafim", "lua")
        );

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("já possui");
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsFailure()
    {
        _characterRepo.Setup(r => r.UserHasCharacterAsync(_userId)).ReturnsAsync(false);
        _characterRepo.Setup(r => r.ExistsByNameAsync("Serafim")).ReturnsAsync(true);

        var result = await _handler.HandleAsync(
            new CreateCharacterCommand(_userId, "Serafim", "rosa")
        );

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("nome");
    }
}
