using MediatR;
using Oms.Application.Features.Tenants.Commands.RegisterTenant;

namespace Oms.WebApi.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        // Group all tenant routes under '/api/tenants' for clean structure
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants"); // Groups them nicely in Swagger/OpenAPI
        
        // Map POST /api/tenants/register
        group.MapPost("/register", RegisterTenant)
            .WithName("RegisterTenant")
            .Produces<TenantRegistrationResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }
    
    // The endpoint handler method
    private static async Task<IResult> RegisterTenant(
        RegisterTenantCommand command, 
        ISender sender, // 👈 Using ISender (MediatR interface for sending commands)
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(command, cancellationToken);
            
            // Returns 201 Created with the location header pointing to the new tenant's URL
            return Results.Created($"/api/tenants/{result.Slug}", result);
        }
        catch (InvalidOperationException ex)
        {
            // Returns 400 Bad Request with a friendly error payload
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}