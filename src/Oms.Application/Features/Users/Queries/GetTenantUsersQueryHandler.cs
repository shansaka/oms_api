using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;

namespace Oms.Application.Features.Users.Queries;

public class GetTenantUsersQueryHandler : IRequestHandler<GetTenantUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;
    public GetTenantUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<UserDto>> Handle(GetTenantUsersQuery request, CancellationToken cancellationToken)
    {
        // Global query filter automatic logic is active because User implements ITenantEntity.
        // It fetches only the users belonging to the caller's TenantId.
        return await _context.Users
            .AsNoTracking() // 👈 Optimization: No change tracking needed for read-only query
            .Select(u => new UserDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Roles.Select(r => r.Name).ToList(),
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}