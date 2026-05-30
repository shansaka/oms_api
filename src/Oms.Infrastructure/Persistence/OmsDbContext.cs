using Microsoft.EntityFrameworkCore;
using Oms.Domain.Entities;
using Oms.Application.Common.Interfaces;

namespace Oms.Infrastructure.Persistence;


public class OmsDbContext : DbContext, IApplicationDbContext
{
    public OmsDbContext(DbContextOptions<OmsDbContext> options) : base(options)
    {
    }
    
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // and automatically applies all configurations that implement IEntityTypeConfiguration<T>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OmsDbContext).Assembly);
    }
    
}