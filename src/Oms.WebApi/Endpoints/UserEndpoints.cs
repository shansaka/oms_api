using MediatR;
using Oms.Application.Features.Users.Commands;
using Oms.Application.Features.Users.Queries;

namespace Oms.WebApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");
        
        // GET /api/users - Lists users of caller's tenant
        group.MapGet("/", GetUsers)
            .WithName("GetTenantUsers")
            .Produces<List<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("users:manage"); // 👈 Enforce the permission guard
        
        // POST /api/users/staff - Creates a new staff member for caller's tenant
        group.MapPost("/staff", CreateStaff)
            .WithName("CreateStaffUser")
            .Produces<CreateStaffUserResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("users:manage"); // 👈 Enforce the permission guard
    }
    private static async Task<IResult> GetUsers(
        ISender sender, 
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTenantUsersQuery(), cancellationToken);
        return Results.Ok(result);
    }
    private static async Task<IResult> CreateStaff(
        CreateStaffUserCommand command, 
        ISender sender, 
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(command, cancellationToken);
            // Returns 201 Created and sends a deterministic response payload
            return Results.Created($"/api/users/{result.UserId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}