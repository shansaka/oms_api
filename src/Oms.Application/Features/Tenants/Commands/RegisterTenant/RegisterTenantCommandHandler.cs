using MediatR;
using Microsoft.EntityFrameworkCore;
using Oms.Application.Common.Interfaces;
using Oms.Domain.Entities;

namespace Oms.Application.Features.Tenants.Commands.RegisterTenant;

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, TenantRegistrationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterTenantCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher =  passwordHasher;
    }
    
    public async Task<TenantRegistrationResult> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var slug = Tenant.GenerateSlug(request.CompanyName);
        var slugExists = await _context.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken: cancellationToken);
        if (slugExists)
        {
            throw new InvalidOperationException($"The company name '{request.CompanyName}' generates a duplicate slug '{slug}'.");
        }

        var emailExists = await _context.Users.AnyAsync(x => x.Email == request.OwnerEmail, cancellationToken: cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException($"The owner email '{request.OwnerEmail}' already exists.");
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        var tenantSettings = new TenantSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Tenant = tenant,
            Theme = "Light",
            AllowedEmailDomains = null,
        };
        tenant.Settings = tenantSettings;

        var owner = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = request.OwnerEmail,
            PasswordHash = _passwordHasher.Hash(request.OwnerPassword),
            FirstName = request.OwnerFirstName,
            LastName = request.OwnerLastName,
            CreatedAt = DateTime.UtcNow,
            Tenant = tenant

        };
        
        //adding owner role
        var ownerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Owner", cancellationToken);
        if (ownerRole != null)
        {
            owner.Roles.Add(ownerRole);
        }
        
        tenant.Users.Add(owner);
        
        _context.Tenants.Add(tenant);
        _context.Users.Add(owner);
        _context.TenantSettings.Add(tenantSettings);
        
        await  _context.SaveChangesAsync(cancellationToken);
        
        return new TenantRegistrationResult(tenant.Id,  tenant.Slug, owner.Id);
    }
}