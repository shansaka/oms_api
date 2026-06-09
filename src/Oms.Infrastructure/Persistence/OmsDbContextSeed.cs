using Microsoft.EntityFrameworkCore;
using Oms.Domain.Entities;

namespace Oms.Infrastructure.Persistence;

public class OmsDbContextSeed
{
    public static async Task SeedAsync(OmsDbContext context)
    {
        // 1. Seed Permissions
        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Name = "orders:read", Description = "Read order details" },
            new() { Id = Guid.NewGuid(), Name = "orders:write", Description = "Create/Modify orders" },
            new() { Id = Guid.NewGuid(), Name = "users:manage", Description = "Create/Modify/Delete users" },
            new() { Id = Guid.NewGuid(), Name = "tenant:manage", Description = "Manage tenant configurations" }
        };
        foreach (var perm in permissions)
        {
            if (!await context.Permissions.AnyAsync(p => p.Name == perm.Name))
            {
                context.Permissions.Add(perm);
            }
        }
        await context.SaveChangesAsync();
        
        // 2. Fetch seeded tracked permissions
        var dbPermissions = await context.Permissions.ToListAsync();
        var readOrder = dbPermissions.First(p => p.Name == "orders:read");
        var writeOrder = dbPermissions.First(p => p.Name == "orders:write");
        var manageUsers = dbPermissions.First(p => p.Name == "users:manage");
        var manageTenant = dbPermissions.First(p => p.Name == "tenant:manage");
        
        // 3. Seed Roles and map their permission lists
        var rolesToSeed = new List<(string Name, string Description, List<Permission> Permissions)>
        {
            ("Owner", "Owner with absolute controls", new() { readOrder, writeOrder, manageUsers, manageTenant }),
            ("Admin", "Administrator access", new() { readOrder, writeOrder, manageUsers }),
            ("Staff", "Standard worker access", new() { readOrder, writeOrder })
        };
        
        foreach (var roleData in rolesToSeed)
        {
            var existingRole = await context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Name == roleData.Name);
            if (existingRole == null)
            {
                var newRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleData.Name,
                    Description = roleData.Description,
                    Permissions = roleData.Permissions
                };
                context.Roles.Add(newRole);
            }
        }
        await context.SaveChangesAsync();
    }
}