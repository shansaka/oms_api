using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Oms.Domain.Entities;
using Oms.Application.Common.Interfaces;

namespace Oms.Infrastructure.Persistence;


public class OmsDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContextAccessor _tenantContextAccessor;
    
    public OmsDbContext(DbContextOptions<OmsDbContext> options, ITenantContextAccessor tenantContextAccessor) : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }
    
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    
    private static readonly MethodInfo ConfigureTenantFilterMethod = typeof(OmsDbContext)
        .GetMethod(nameof(ConfigureTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // and automatically applies all configurations that implement IEntityTypeConfiguration<T>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OmsDbContext).Assembly);
        
        // Dynamically find and apply query filters to all ITenantEntity implementations
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = ConfigureTenantFilterMethod.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }
    }
    
    private void ConfigureTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        // 🚨 Automatically injects WHERE TenantId = @CurrentTenantId to queries
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _tenantContextAccessor.TenantId);
    }
    // Automatically assign TenantId and block tenant updates on modified records
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (_tenantContextAccessor.TenantId.HasValue)
                    {
                        entry.Entity.TenantId = _tenantContextAccessor.TenantId.Value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot save a tenant-scoped entity without a resolved Tenant ID.");
                    }
                    break;
                case EntityState.Modified:
                    // Block modifying the TenantId of an existing record (Strict isolation protection)
                    entry.Property(x => x.TenantId).IsModified = false;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
    
}