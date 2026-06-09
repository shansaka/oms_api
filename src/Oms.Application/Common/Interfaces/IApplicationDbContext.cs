using Microsoft.EntityFrameworkCore;
using Oms.Domain.Entities;

namespace Oms.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<TenantSettings> TenantSettings { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    
    // This method is required so the Application layer can persist changes
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}