using Oms.Domain.Entities;

namespace Oms.Application.Common.Interfaces;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}