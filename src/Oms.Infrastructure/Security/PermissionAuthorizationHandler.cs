using Microsoft.AspNetCore.Authorization;

namespace Oms.Infrastructure.Security;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        // Extract all 'permissions' claims from the user's JWT
        var permissions = context.User.Claims
            .Where(c => c.Type == "permissions")
            .Select(c => c.Value)
            .ToHashSet();
        
        // If the user has the required permission, pass the check
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}