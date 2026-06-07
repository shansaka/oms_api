using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;
using Oms.Domain.Entities;

namespace Oms.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.SingleOrDefaultAsync(
            u => u.Email == request.Email, cancellationToken);

        // Security best practice: If user is not found, we still execute the verify method
        // with a dummy hash to prevent timing attacks that verify if an email exists.
        if (user == null)
        {
            _passwordHasher.Verify(request.Password, "$2a$12$DummyHashDummyHashDummyHashDummyHashDummyHashDummy");
            return new LoginResult(false, "Invalid credentials.", null, null);
        }
        
        var isCorrectPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isCorrectPassword)
        {
            return new LoginResult(false, "Invalid credentials.", null, null);
        }
        
        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();
        
        // Save refresh token to db
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);
        return new LoginResult(true, "Success", accessToken, newRefreshToken);
    }
}