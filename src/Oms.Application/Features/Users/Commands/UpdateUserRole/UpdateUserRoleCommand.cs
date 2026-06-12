using MediatR;

namespace Oms.Application.Features.Users.Commands.UpdateUserRole;

public record UpdateUserRoleCommand(Guid UserId, List<string> RoleNames) : IRequest<bool>;