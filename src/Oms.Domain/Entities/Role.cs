namespace Oms.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // "Owner", "Admin", "Staff"
    public string Description { get; set; } = string.Empty;
    
    // Many-to-many relationships
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}