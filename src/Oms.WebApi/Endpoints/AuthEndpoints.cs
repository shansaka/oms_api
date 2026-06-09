using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Oms.Application.Features.Auth.Commands.Login;
using Oms.Application.Features.Auth.Commands.Logout;
using Oms.Application.Features.Auth.Commands.RefreshToken;

namespace Oms.WebApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");
        
        group.MapPost("/login", LoginUser).WithName("LoginUser").Produces<LoginResult>(StatusCodes.Status200OK);
        
        group.MapPost("/refreshToken",  RefreshToken).WithName("RefreshToken").Produces<RefreshTokenResult>(StatusCodes.Status200OK);
        
        group.MapPost("/logout",  LogoutUser).WithName("LogoutUser").Produces<LogoutResult>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> LoginUser(LoginCommand loginCommand, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(loginCommand, cancellationToken);
        if (!result.IsSuccess)
        {
            return Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
        }
        
        return Results.Ok(result);
    }

    private static async Task<IResult> RefreshToken(RefreshRequest request, HttpContext context, ISender sender,
        CancellationToken cancellationToken)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new RefreshTokenCommand(request.RefreshToken, ipAddress);
        var result = await sender.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
        }
        
        return Results.Ok(result);
    }

    private static async Task<IResult> LogoutUser(
        LogoutRequest request, 
        HttpContext context, 
        ISender sender, 
        CancellationToken cancellationToken)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new LogoutCommand(request.RefreshToken, ipAddress);
        var result = await sender.Send(command, cancellationToken);
        
        return Results.Ok(result);
    }
    
    // Request payloads
    public record RefreshRequest(string RefreshToken);

    public record LogoutRequest(string RefreshToken);
}