namespace Oms.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // The slug is used for routing (e.g., veesal.oms.com or oms.com/veesal)
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public TenantSettings Settings { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
}