using Microsoft.EntityFrameworkCore;
using Oms.Domain.Entities;

namespace Oms.Infrastructure.Persistence;

public class OmsDbContext : DbContext
{
    public OmsDbContext(DbContextOptions<OmsDbContext> options) : base(options)
    {
    }
    
    public DbSet<Order> Orders => Set<Order>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // and automatically applies all configurations that implement IEntityTypeConfiguration<T>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OmsDbContext).Assembly);
    }
    
}