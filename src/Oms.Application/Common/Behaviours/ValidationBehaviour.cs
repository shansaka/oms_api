using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Oms.Application.Common.Interfaces;

namespace Oms.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ITenantContextAccessor _tenantContextAccessor; // 👈 1. Inject Context Accessor

    public ValidationBehaviour(
        IEnumerable<IValidator<TRequest>> validators, 
        ITenantContextAccessor tenantContextAccessor)
    {
        _validators = validators;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // 👈 2. Guard against cross-tenant identifier overrides
        if (request is ITenantScopedRequest tenantScopedRequest)
        {
            var authenticatedTenantId = _tenantContextAccessor.TenantId;
            
            // If the user is authenticated with a tenant, but the payload has a different tenant, fail fast!
            if (authenticatedTenantId.HasValue && tenantScopedRequest.TenantId != authenticatedTenantId.Value)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(
                        nameof(ITenantScopedRequest.TenantId), 
                        "Cross-tenant operations are strictly prohibited. The tenant identifier does not match your authenticated context.")
                });
            }
        }

        // 3. Continue running standard FluentValidation rules
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }
        
        return await next();
    }
}