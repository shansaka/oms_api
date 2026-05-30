using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oms.Domain.Entities;

namespace Oms.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
        
        builder.Property(p => p.Slug).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();
        
        builder.Property(p => p.CreatedAt).IsRequired();
        
    }
}