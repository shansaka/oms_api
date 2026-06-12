using FluentValidation;

namespace Oms.Application.Features.Users.Commands.ToggleUserActive;

public class ToggleUserActiveCommandValidator : AbstractValidator<ToggleUserActiveCommand>
{
    public ToggleUserActiveCommandValidator()
    {
        RuleFor(c => c.UserId).NotNull();
        RuleFor(c => c.IsActive).NotNull();
    }
}