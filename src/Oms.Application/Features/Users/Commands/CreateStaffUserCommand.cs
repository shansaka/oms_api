using MediatR;

namespace Oms.Application.Features.Users.Commands;

public record CreateStaffUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<CreateStaffUserResult>;

public record CreateStaffUserResult(
    Guid UserId,
    string Email,
    string Role
);