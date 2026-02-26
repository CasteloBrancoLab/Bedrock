using System.Security.Cryptography;

namespace ShopDemo.Auth.Domain.Entities.Fingerprints;

public readonly struct FingerprintHash : IEquatable<FingerprintHash>
{
    public ReadOnlyMemory<byte> Value { get; }

    public bool IsEmpty => Value.IsEmpty;

    public int Length => Value.Length;

    private FingerprintHash(ReadOnlyMemory<byte> value)
    {
        Value = value;
    }

    public static FingerprintHash CreateNew(byte[] value)
    {
        var copy = new byte[value.Length];
        value.CopyTo(copy.AsSpan());
        return new FingerprintHash(copy);
    }

    public static FingerprintHash CreateFromExistingInfo(byte[] value)
    {
        var copy = new byte[value.Length];
        value.CopyTo(copy.AsSpan());
        return new FingerprintHash(copy);
    }

    public string ToBase64Url()
    {
        return Convert.ToBase64String(Value.Span)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public bool Equals(FingerprintHash other)
    {
        return CryptographicOperations.FixedTimeEquals(Value.Span, other.Value.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is FingerprintHash other && Equals(other);
    }

    public override int GetHashCode()
    {
        var span = Value.Span;
        var hash = new HashCode();
        for (int i = 0; i < span.Length; i++)
        {
            hash.Add(span[i]);
        }
        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return "[REDACTED]";
    }

    public static bool operator ==(FingerprintHash left, FingerprintHash right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FingerprintHash left, FingerprintHash right)
    {
        return !left.Equals(right);
    }
}
