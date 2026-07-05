using Angelus.Application.Auth.Commands;
using FluentValidation;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório")
            .EmailAddress()
            .WithMessage("Email inválido")
            .MaximumLength(255)
            .WithMessage("Email muito longo");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Senha é obrigatória")
            .MinimumLength(8)
            .WithMessage("Senha deve ter no mínimo 8 caracteres")
            .Matches("[A-Z]")
            .WithMessage("Senha deve ter pelo menos uma letra maiúscula")
            .Matches("[a-z]")
            .WithMessage("Senha deve ter pelo menos uma letra minúscula")
            .Matches("[0-9]")
            .WithMessage("Senha deve ter pelo menos um número")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Senha deve ter pelo menos um caractere especial");
    }
}
