using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace Oms.WebApi;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            _logger.LogWarning(exception, "Validation failed for request: {Path}", httpContext.Request.Path);
            // Group validation errors by property name for cleaner API client responses
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Detail = "Please review the errors list for details.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Instance = httpContext.Request.Path
            };
            
            // Add custom validation errors to the payload extensions dictionary
            problemDetails.Extensions.Add("errors", errors);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            
            return true; // Tells ASP.NET that this exception is handled
        }
        
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var serverErrorDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error, Please contact your system administrator",
            Detail = exception.Message,
            Type = "https://httpstatuses.com/500"
        };

        httpContext.Response.StatusCode = serverErrorDetails.Status.Value;
        
        await httpContext.Response.WriteAsJsonAsync(serverErrorDetails, cancellationToken);
        
        return true;
    }
}