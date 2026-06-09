using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;

namespace Oms.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtProvider jwtProvider)
    {
        _context = context;
        _jwtProvider = jwtProvider;
    }
    
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await  _context.RefreshTokens.Include(u => u.User).SingleOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (existingToken == null || !existingToken.IsActive)
        {
            return new RefreshTokenResult(false, "Invalid or expired refresh token.", null, null);
        }
        
        var newAccessToken =  _jwtProvider.GenerateAccessToken(existingToken.User);
        var newRefreshToken =  _jwtProvider.GenerateRefreshToken();
        
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedByIp = request.IpAddress;
        existingToken.ReplacedByToken = newRefreshToken;
        
        var newRefreshTokenEntity = new Oms.Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7-day lifetime
            UserId = existingToken.UserId,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.IpAddress
        };
        _context.RefreshTokens.Add(newRefreshTokenEntity);
        
        await _context.SaveChangesAsync(cancellationToken);
        return new RefreshTokenResult(true, "Success", newAccessToken, newRefreshToken);
    }
}