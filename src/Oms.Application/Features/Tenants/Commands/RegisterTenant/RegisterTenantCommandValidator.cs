using FluentValidation;

namespace Oms.Application.Features.Tenants.Commands.RegisterTenant;

public class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.OwnerFirstName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.OwnerLastName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.OwnerPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters long.") 
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Must((command, password) => !password.Contains(command.OwnerFirstName, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password must not contain your first name.")
            .Must((command, password) => !password.Contains(command.OwnerLastName, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password must not contain your last name.")
            .Must((command, password) => 
            {
                var emailUsername = command.OwnerEmail.Split('@')[0];
                return string.IsNullOrEmpty(emailUsername) || !password.Contains(emailUsername, StringComparison.OrdinalIgnoreCase);
            })
            .WithMessage("Password must not contain parts of your email address.");
    }
}