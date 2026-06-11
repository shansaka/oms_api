namespace Oms.Application.Common.Interfaces;

public interface ITenantContextAccessor
{
    Guid? TenantId { get; }
    void SetTenantId(Guid tenantId);
}