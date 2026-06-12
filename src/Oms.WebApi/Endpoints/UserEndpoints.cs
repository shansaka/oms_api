using MediatR;
using Oms.Application.Features.Users.Commands;
using Oms.Application.Features.Users.Commands.CreateStaff;
using Oms.Application.Features.Users.Commands.ToggleUserActive;
using Oms.Application.Features.Users.Commands.UpdateUserRole;
using Oms.Application.Features.Users.Queries;
using Oms.Application.Features.Users.Queries.GetTenantUsers;

namespace Oms.WebApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");
        
        // GET /api/users - Lists users of caller's tenant
        group.MapGet("/", GetTenantUsers)
            .WithName("GetTenantUsers")
            .Produces<List<UserDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("users:manage"); 
        
        // POST /api/users/staff - Creates a new staff member for caller's tenant
        group.MapPost("/staff", CreateStaffUser)
            .WithName("CreateStaffUser")
            .Produces<CreateStaffUserResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("users:manage"); 
        
        group.MapPut("/{userId:guid}/active", ToggleUserActive)
            .WithName("ToggleUserActive")
            .RequireAuthorization("users:manage"); 
        
        group.MapPut("/{userId:guid}/roles", UpdateUserRoles)
            .WithName("UpdateUserRoles")
            .RequireAuthorization("users:manage"); 
    }
    private static async Task<IResult> GetTenantUsers(
        ISender sender, 
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTenantUsersQuery(), cancellationToken);
        return Results.Ok(result);
    }
    
    private static async Task<IResult> CreateStaffUser(
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
    
    private static async Task<IResult> ToggleUserActive(
        Guid userId,
        ToggleActiveRequest request,
        HttpContext context,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new ToggleUserActiveCommand(userId, request.IsActive, ipAddress);
        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(new { success = result, message = $"User has been {(request.IsActive ? "activated" : "deactivated")}." });
    }

    private static async Task<IResult> UpdateUserRoles(
        Guid userId,
        UpdateRolesRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserRoleCommand(userId, request.RoleNames);
        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(new { success = result });
    }
    
    public record ToggleActiveRequest(bool IsActive = true);
    public record UpdateRolesRequest(List<string> RoleNames);
}