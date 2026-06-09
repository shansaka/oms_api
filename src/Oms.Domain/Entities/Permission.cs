namespace Oms.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty;
    
    // Many-to-many relationship
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}