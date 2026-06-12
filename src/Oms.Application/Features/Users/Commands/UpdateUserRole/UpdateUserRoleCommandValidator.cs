using FluentValidation;

namespace Oms.Application.Features.Users.Commands.UpdateUserRole;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotNull();
        RuleFor(x => x.RoleNames).NotEmpty();
    }
}