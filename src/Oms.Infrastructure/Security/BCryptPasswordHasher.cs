using Oms.Application.Common.Interfaces;

namespace Oms.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    // Work factor of 12 represents 2^12 hashing iterations.
    // It takes roughly 250-300ms, which is perfect for blocking brute-force searches
    // without impacting user experience too heavily.
    private const int WorkFactor = 12;
    
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }

    public bool Verify(string hash, string password)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(hash, password);
    }
}