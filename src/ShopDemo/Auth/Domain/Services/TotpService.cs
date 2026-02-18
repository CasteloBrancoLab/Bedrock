using System.Security.Cryptography;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class TotpService : ITotpService
{
    private const int SecretLength = 20;
    private const int CodeDigits = 6;
    private const int TimeStepSeconds = 30;
    private const int WindowSize = 1;
    private static readonly int CodeModulus = (int)Math.Pow(10, CodeDigits);

    public byte[] GenerateSecret()
    {
        return RandomNumberGenerator.GetBytes(SecretLength);
    }

    public string GenerateQrCodeUri(
        byte[] secret,
        string issuer,
        string accountName)
    {
        string base32Secret = ToBase32(secret);
        string encodedIssuer = Uri.EscapeDataString(issuer);
        string encodedAccountName = Uri.EscapeDataString(accountName);

        return $"otpauth://totp/{encodedIssuer}:{encodedAccountName}?secret={base32Secret}&issuer={encodedIssuer}&algorithm=SHA1&digits={CodeDigits}&period={TimeStepSeconds}";
    }

    public bool ValidateCode(
        byte[] secret,
        string code,
        DateTimeOffset timestamp)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != CodeDigits)
            return false;

        if (!int.TryParse(code, out _))
            return false;

        long timeStep = GetTimeStep(timestamp);

        for (int i = -WindowSize; i <= WindowSize; i++)
        {
            string expectedCode = ComputeTotp(secret, timeStep + i);

            if (CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(expectedCode),
                System.Text.Encoding.UTF8.GetBytes(code)))
            {
                return true;
            }
        }

        return false;
    }

    private static long GetTimeStep(DateTimeOffset timestamp)
    {
        return timestamp.ToUnixTimeSeconds() / TimeStepSeconds;
    }

    private static string ComputeTotp(byte[] secret, long timeStep)
    {
        Span<byte> timeBytes = stackalloc byte[8];
        WriteBigEndianInt64(timeBytes, timeStep);

        Span<byte> hash = stackalloc byte[HMACSHA1.HashSizeInBytes];
        HMACSHA1.HashData(secret, timeBytes, hash);

        int offset = hash[^1] & 0x0F;

        int binaryCode =
            ((hash[offset] & 0x7F) << 24)
            | ((hash[offset + 1] & 0xFF) << 16)
            | ((hash[offset + 2] & 0xFF) << 8)
            | (hash[offset + 3] & 0xFF);

        int otp = binaryCode % CodeModulus;

        return otp.ToString().PadLeft(CodeDigits, '0');
    }

    private static void WriteBigEndianInt64(Span<byte> buffer, long value)
    {
        buffer[0] = (byte)(value >> 56);
        buffer[1] = (byte)(value >> 48);
        buffer[2] = (byte)(value >> 40);
        buffer[3] = (byte)(value >> 32);
        buffer[4] = (byte)(value >> 24);
        buffer[5] = (byte)(value >> 16);
        buffer[6] = (byte)(value >> 8);
        buffer[7] = (byte)value;
    }

    private static string ToBase32(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        int outputLength = (int)Math.Ceiling(data.Length * 8.0 / 5);
        Span<char> result = stackalloc char[outputLength];

        int bitBuffer = 0;
        int bitsInBuffer = 0;
        int index = 0;

        for (int i = 0; i < data.Length; i++)
        {
            bitBuffer = (bitBuffer << 8) | data[i];
            bitsInBuffer += 8;

            while (bitsInBuffer >= 5)
            {
                bitsInBuffer -= 5;
                result[index++] = alphabet[(bitBuffer >> bitsInBuffer) & 0x1F];
            }
        }

        if (bitsInBuffer > 0)
        {
            result[index++] = alphabet[(bitBuffer << (5 - bitsInBuffer)) & 0x1F];
        }

        return new string(result[..index]);
    }
}
