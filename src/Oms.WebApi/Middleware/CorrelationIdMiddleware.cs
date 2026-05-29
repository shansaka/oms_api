using Serilog.Context;

namespace Oms.WebApi.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderKey = "X-Correlation-ID";
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // 1. Check if the client sent a Correlation ID in their headers; if not, generate a new one
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }
        
        // 2. Add the correlation ID to the response headers so the client knows it
        context.Response.Headers[CorrelationIdHeaderKey] = correlationId;
        
        // 3. Push the correlation ID onto the Serilog Log Context.
        // This ensures ANY log statement inside this request block will automatically include the "CorrelationId" property.
        using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
        {
            await _next(context);
        }
    }
}