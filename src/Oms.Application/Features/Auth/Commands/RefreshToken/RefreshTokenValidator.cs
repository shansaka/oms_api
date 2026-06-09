using FluentValidation;

namespace Oms.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(c => c.RefreshToken).NotNull().NotEmpty().WithMessage("RefreshToken is required");
    }
}