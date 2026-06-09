using MediatR;

namespace Oms.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken, string IpAddress) : IRequest<LogoutResult>;
public record LogoutResult(bool IsSuccess, string Message);