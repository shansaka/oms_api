using MediatR;
using Oms.Application.Features.Auth.Commands.Login;

namespace Oms.WebApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");
        
        group.MapPost("/login", LoginUser).WithName("LoginUser").Produces<LoginResult>(StatusCodes.Status200OK);
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
}