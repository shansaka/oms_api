using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;

namespace Oms.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IApplicationDbContext _context;

    public LogoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _context.RefreshTokens
            .SingleOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (existingToken == null || !existingToken.IsActive)
        {
            // Idempotent behavior: return success even if already logged out/not found.
            // This prevents leaking whether a token exists in our database.
            return new LogoutResult(true, "Successfully logged out.");
        }

        // Revoke the token
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedByIp = request.IpAddress;

        await _context.SaveChangesAsync(cancellationToken);

        return new LogoutResult(true, "Successfully logged out.");
    }
}