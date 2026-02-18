using System.Security.Cryptography;
using System.Text;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class PasswordResetTokenService : IPasswordResetTokenService
{
    private const int TokenSizeInBytes = 32;

    public string GenerateToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);
        return Convert.ToBase64String(tokenBytes)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
    }

    public string HashToken(string token)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(hash);
    }

    public bool ValidateToken(string token, string tokenHash)
    {
        string computedHash = HashToken(token);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(tokenHash));
    }
}
