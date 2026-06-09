using FluentValidation;

namespace Oms.Application.Features.Auth.Commands.Logout;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty().NotNull().WithMessage("Refresh token is required");
    }
}