namespace Oms.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    
    // Every user belongs to exactly one tenant
    public Guid TenantId { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   
    
    // Navigation property
    public Tenant Tenant { get; set; } = null!;
}