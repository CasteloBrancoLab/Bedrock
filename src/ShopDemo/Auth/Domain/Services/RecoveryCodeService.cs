using System.Security.Cryptography;
using System.Text;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class RecoveryCodeService : IRecoveryCodeService
{
    private const int CodePartLength = 4;
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public IReadOnlyList<string> GenerateCodes(int count = 10)
    {
        var codes = new List<string>(count);

        for (int i = 0; i < count; i++)
        {
            codes.Add(GenerateSingleCode());
        }

        return codes;
    }

    public string HashCode(string code)
    {
        string normalized = code.Replace("-", "", StringComparison.Ordinal).ToUpperInvariant();
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexStringLower(hash);
    }

    public bool ValidateCode(string code, string codeHash)
    {
        string computedHash = HashCode(code);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(codeHash));
    }

    private static string GenerateSingleCode()
    {
        Span<char> part1 = stackalloc char[CodePartLength];
        Span<char> part2 = stackalloc char[CodePartLength];

        FillRandomChars(part1);
        FillRandomChars(part2);

        return $"{part1}-{part2}";
    }

    private static void FillRandomChars(Span<char> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = AllowedChars[RandomNumberGenerator.GetInt32(AllowedChars.Length)];
        }
    }
}
