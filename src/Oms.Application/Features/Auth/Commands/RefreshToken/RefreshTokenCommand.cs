using MediatR;

namespace Oms.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string IpAddress) : IRequest<RefreshTokenResult>;
public record RefreshTokenResult(bool IsSuccess, string? Message, string? AccessToken, string? RefreshToken);