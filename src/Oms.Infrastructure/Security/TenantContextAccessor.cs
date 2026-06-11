using Oms.Application.Common.Interfaces;

namespace Oms.Infrastructure.Security;

public class TenantContextAccessor: ITenantContextAccessor
{
    public Guid? TenantId { get; private set; }
    
    public void SetTenantId(Guid tenantId)
    {
        // Fail-safe check: prevent tenant context swapping mid-request
        if (TenantId.HasValue && TenantId.Value != tenantId)
        {
            throw new InvalidOperationException("Tenant context is already established and cannot be changed.");
        }
        TenantId = tenantId;
    }
}