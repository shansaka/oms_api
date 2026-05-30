using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oms.Domain.Entities;

namespace Oms.Infrastructure.Persistence.Configurations;

public class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.ToTable("TenantSettings");
        
        builder.HasKey(p => p.Id);

        builder.Property(p => p.AllowedEmailDomains);
        builder.Property(p => p.Theme).IsRequired().HasMaxLength(30).HasDefaultValue("Light");
        
        builder.HasOne(p => p.Tenant).WithOne(p => p.Settings).HasForeignKey<TenantSettings>(p => p.TenantId).OnDelete(DeleteBehavior.Cascade);
        
    }
}