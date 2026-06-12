using MediatR;

namespace Oms.Application.Features.Users.Commands.ToggleUserActive;

public record ToggleUserActiveCommand(Guid UserId, bool IsActive, string IpAddress) : IRequest<bool>;