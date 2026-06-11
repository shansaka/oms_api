namespace Oms.Application.Common.Interfaces;

public interface ITenantScopedRequest
{
    Guid TenantId { get; }
}