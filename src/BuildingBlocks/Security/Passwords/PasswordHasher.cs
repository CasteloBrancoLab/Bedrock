using System.Security.Cryptography;
using Konscious.Security.Cryptography;

namespace Bedrock.BuildingBlocks.Security.Passwords;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const int PepperVersionLength = 1;
    private const int TotalHashLength = PepperVersionLength + SaltLength + HashLength; // 49

    // OWASP recommended Argon2id parameters
    private const int MemorySize = 19 * 1024; // 19 MiB in KiB
    private const int Iterations = 2;
    private const int DegreeOfParallelism = 1;

    private readonly PepperConfiguration _pepperConfiguration;

    public PasswordHasher(PepperConfiguration pepperConfiguration)
    {
        _pepperConfiguration = pepperConfiguration ?? throw new ArgumentNullException(nameof(pepperConfiguration));
    }

    public PasswordHashResult HashPassword(
        ExecutionContext executionContext,
        string password
    )
    {
        int activePepperVersion = _pepperConfiguration.ActivePepperVersion;
        byte[] pepper = _pepperConfiguration.Peppers[activePepperVersion];

        byte[] pepperedPassword = ApplyPepper(password, pepper);
        byte[] salt = RandomNumberGenerator.GetBytes(SaltLength);
        byte[] hash = ComputeArgon2idHash(pepperedPassword, salt);

        byte[] result = new byte[TotalHashLength];
        result[0] = (byte)activePepperVersion;
        salt.CopyTo(result.AsSpan(PepperVersionLength));
        hash.CopyTo(result.AsSpan(PepperVersionLength + SaltLength));

        return new PasswordHashResult(result, activePepperVersion);
    }

    public PasswordVerificationResult VerifyPassword(
        ExecutionContext executionContext,
        string password,
        byte[] storedHash
    )
    {
        if (storedHash is null || storedHash.Length != TotalHashLength)
            return new PasswordVerificationResult(false, false);

        int pepperVersion = storedHash[0];

        if (!_pepperConfiguration.Peppers.TryGetValue(pepperVersion, out byte[]? pepper))
            return new PasswordVerificationResult(false, false);

        byte[] salt = storedHash.AsSpan(PepperVersionLength, SaltLength).ToArray();
        byte[] storedHashPart = storedHash.AsSpan(PepperVersionLength + SaltLength, HashLength).ToArray();

        byte[] pepperedPassword = ApplyPepper(password, pepper);
        byte[] computedHash = ComputeArgon2idHash(pepperedPassword, salt);

        bool isValid = CryptographicOperations.FixedTimeEquals(computedHash, storedHashPart);
        bool needsRehash = pepperVersion != _pepperConfiguration.ActivePepperVersion;

        return new PasswordVerificationResult(isValid, needsRehash);
    }

    public bool NeedsRehash(byte[] storedHash)
    {
        if (storedHash is null || storedHash.Length < PepperVersionLength)
            return true;

        int pepperVersion = storedHash[0];

        return pepperVersion != _pepperConfiguration.ActivePepperVersion;
    }

    private static byte[] ApplyPepper(string password, byte[] pepper)
    {
        byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

        using var hmac = new HMACSHA256(pepper);

        return hmac.ComputeHash(passwordBytes);
    }

    private static byte[] ComputeArgon2idHash(byte[] pepperedPassword, byte[] salt)
    {
        using var argon2 = new Argon2id(pepperedPassword);
        argon2.Salt = salt;
        argon2.MemorySize = MemorySize;
        argon2.Iterations = Iterations;
        argon2.DegreeOfParallelism = DegreeOfParallelism;

        return argon2.GetBytes(HashLength);
    }
}
