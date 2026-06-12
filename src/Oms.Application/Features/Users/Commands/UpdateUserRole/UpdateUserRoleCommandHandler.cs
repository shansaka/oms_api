using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;

namespace Oms.Application.Features.Users.Commands.UpdateUserRole;

public class UpdateUserRoleCommandHandler: IRequestHandler<UpdateUserRoleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public UpdateUserRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch user (scoped to current Tenant)
        var user = await _context.Users
            .Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found in your tenant.");
        }
        
        // 2. Fetch roles to assign from Db
        var rolesToAssign = await _context.Roles
            .Where(r => request.RoleNames.Contains(r.Name))
            .ToListAsync(cancellationToken);
        
        if (rolesToAssign.Count != request.RoleNames.Count)
        {
            throw new InvalidOperationException("One or more roles do not exist.");
        }
        
        // 3. Update the user's role collection
        user.Roles.Clear();
        
        foreach (var role in rolesToAssign)
        {
            user.Roles.Add(role);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}