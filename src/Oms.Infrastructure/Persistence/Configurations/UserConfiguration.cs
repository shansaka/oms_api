using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oms.Domain.Entities;

namespace Oms.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(p => p.Email).IsUnique();
        
        builder.Property(p => p.FirstName).HasMaxLength(256).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(256).IsRequired();
        
        builder.Property(p => p.CreatedAt).IsRequired();
        
        builder.HasOne(p => p.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(p => p.PasswordHash).HasMaxLength(500).IsRequired();
        
        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<Dictionary<string, object>>(
                "UserRoles",
                r => r.HasOne<Role>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                u => u.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade)
            );
    }
}