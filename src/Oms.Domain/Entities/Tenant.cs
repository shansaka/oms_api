using System.Text.RegularExpressions;

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
    
    public static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        
        // Convert to lowercase
        string slug = name.ToLowerInvariant();
        
        // Replace invalid characters with spaces
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        
        // Convert multiple spaces/hyphens into a single hyphen
        slug = Regex.Replace(slug, @"[\s-]+", "-");
        
        // Trim leading and trailing hyphens
        return slug.Trim('-');
    }
}