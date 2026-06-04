using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;

namespace Oms.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
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
            return new LoginResult(false, "Invalid credentials.", null);
        }
        
        var isCorrectPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isCorrectPassword)
        {
            return new LoginResult(false, "Invalid credentials.", null);
        }
        
        return new LoginResult(true, "Authenticated successfully.", user.Id);
    }
}