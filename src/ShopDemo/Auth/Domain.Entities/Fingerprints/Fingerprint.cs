using System.Security.Cryptography;
using System.Text;

namespace ShopDemo.Auth.Domain.Entities.Fingerprints;

public readonly struct Fingerprint : IEquatable<Fingerprint>
{
    public string Value { get; }

    private Fingerprint(string value)
    {
        Value = value;
    }

    public static Fingerprint CreateNew()
    {
        return new Fingerprint(Guid.NewGuid().ToString("N"));
    }

    public static Fingerprint CreateFromExistingInfo(string value)
    {
        return new Fingerprint(value);
    }

    public FingerprintHash ComputeHash()
    {
        byte[] fingerprintBytes = Encoding.UTF8.GetBytes(Value);
        byte[] hashBytes = SHA256.HashData(fingerprintBytes);
        return FingerprintHash.CreateNew(hashBytes);
    }

    public bool Equals(Fingerprint other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is Fingerprint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode(StringComparison.Ordinal) ?? 0;
    }

    public override string ToString()
    {
        return "[REDACTED]";
    }

    public static bool operator ==(Fingerprint left, Fingerprint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Fingerprint left, Fingerprint right)
    {
        return !left.Equals(right);
    }
}
