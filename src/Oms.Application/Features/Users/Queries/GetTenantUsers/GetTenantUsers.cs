using MediatR;

namespace Oms.Application.Features.Users.Queries.GetTenantUsers;

public record GetTenantUsersQuery() : IRequest<List<UserDto>>;
public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    List<string> Roles,
    DateTime CreatedAt
);