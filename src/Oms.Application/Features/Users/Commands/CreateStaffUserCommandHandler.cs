using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;
using Oms.Domain.Entities;

namespace Oms.Application.Features.Users.Commands;

public class CreateStaffUserCommandHandler : IRequestHandler<CreateStaffUserCommand, CreateStaffUserResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContextAccessor _tenantAccessor;
    public CreateStaffUserCommandHandler(
        IApplicationDbContext context, 
        IPasswordHasher passwordHasher, 
        ITenantContextAccessor tenantAccessor)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tenantAccessor = tenantAccessor;
    }
    public async Task<CreateStaffUserResult> Handle(CreateStaffUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce BOLA protection: Ensure tenant ID exists in request context
        if (!_tenantAccessor.TenantId.HasValue)
        {
            throw new InvalidOperationException("Tenant context is required to create user profiles.");
        }
        
        // 2. Validate user uniqueness
        var emailExists = await _context.Users.AnyAsync(
            u => u.Email == request.Email, 
            cancellationToken);
        
        if (emailExists)
        {
            throw new InvalidOperationException($"The email address '{request.Email}' is already registered.");
        }
        
        // 3. Find the standard "Staff" role to associate
        var staffRole = await _context.Roles.FirstOrDefaultAsync(
            r => r.Name == "Staff", 
            cancellationToken);
        
        if (staffRole == null)
        {
            throw new InvalidOperationException("Default 'Staff' role is not seeded in the database.");
        }
        
        // 4. Create entity (TenantId will be auto-injected by OmsDbContext SaveChangesAsync)
        var newStaff = new User
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantAccessor.TenantId.Value, // Set explicitly for clear code, though DbContext will enforce it
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };
        
        // 5. Establish the relationship
        newStaff.Roles.Add(staffRole);
        
        // 6. Save records inside transaction boundary
        _context.Users.Add(newStaff);
        await _context.SaveChangesAsync(cancellationToken);
        return new CreateStaffUserResult(newStaff.Id, newStaff.Email, staffRole.Name);
    }
}