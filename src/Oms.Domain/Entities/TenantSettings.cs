namespace Oms.Domain.Entities;

public class TenantSettings
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    // Custom configurations per tenant
    public string Theme { get; set; } = "Light";
    public string? AllowedEmailDomains { get; set; } 
    
    // Navigation property back to Tenant
    public Tenant Tenant { get; set; } = null!;
}