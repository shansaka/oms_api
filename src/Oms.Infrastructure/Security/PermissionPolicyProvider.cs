using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Oms.Infrastructure.Security;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) 
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if the policy is already registered (like default fallback/authenticated policies)
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null) return policy;

        // If not registered, create a new dynamic policy using our PermissionRequirement
        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}