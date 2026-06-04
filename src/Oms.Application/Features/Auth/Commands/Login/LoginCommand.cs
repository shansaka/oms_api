using MediatR;

namespace Oms.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;
public record LoginResult(bool IsSuccess, string? Message, Guid? UserId);