using FluentValidation;

namespace Oms.Application.Features.Users.Commands.CreateStaff;

public class CreateStaffUserCommandValidator : AbstractValidator<CreateStaffUserCommand>
{
    public CreateStaffUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters long.") 
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Must((command, password) => !password.Contains(command.FirstName, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password must not contain your first name.")
            .Must((command, password) => !password.Contains(command.LastName, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password must not contain your last name.")
            .Must((command, password) => 
            {
                var emailUsername = command.Email.Split('@')[0];
                return string.IsNullOrEmpty(emailUsername) || !password.Contains(emailUsername, StringComparison.OrdinalIgnoreCase);
            })
            .WithMessage("Password must not contain parts of your email address.");
    }
}