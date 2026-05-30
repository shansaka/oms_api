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
    }
}