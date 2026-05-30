using MediatR;

namespace Oms.Application.Features.Tenants.Commands.RegisterTenant;

public record RegisterTenantCommand(
    string CompanyName,
    string OwnerFirstName,
    string OwnerLastName,
    string OwnerEmail
) : IRequest<TenantRegistrationResult>;

public record TenantRegistrationResult(Guid TenantId, string Slug, Guid OwnerId);
