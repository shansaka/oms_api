using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;

namespace Oms.Application.Features.Users.Commands.ToggleUserActive;

public class ToggleUserActiveCommandHandler: IRequestHandler<ToggleUserActiveCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public ToggleUserActiveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
    {
        // ITenantEntity query filter automatically isolates this request to the caller's Tenant!
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found in your tenant.");
        }
        
        user.IsActive = request.IsActive;
        
        // Cascade revocation of active refresh tokens on deactivation
        if (!request.IsActive)
        {
            var activeTokens = user.RefreshTokens.Where(t => t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = request.IpAddress;
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}