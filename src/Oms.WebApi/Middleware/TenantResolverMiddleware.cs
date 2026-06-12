using System.Security.Claims;
using Oms.Application.Common.Interfaces;

namespace Oms.WebApi.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // ITenantContextAccessor is injected here because middleware is a Singleton under the hood,
    // but Accessor is Scoped. Injected services in InvokeAsync resolve dynamically per request.
    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantContextAccessor)
    {
        // 1. Check authenticated JWT claims (Tenant ID)
        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;

        if (!string.IsNullOrEmpty(tenantIdClaim))
        {
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                tenantContextAccessor.SetTenantId(tenantId);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid tenant claim format." });
                return;
            }
        }
        
        // 2. Fallback: Check request header (useful for developer testing or server-to-server calls)
        else if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue))
        {
            if (Guid.TryParse(headerValue, out var tenantId))
            {
                tenantContextAccessor.SetTenantId(tenantId);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid X-Tenant-ID header format." });
                return;
            }
        }

        await _next(context);
    }
}