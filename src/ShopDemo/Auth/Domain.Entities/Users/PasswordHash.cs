using System.Security.Cryptography;

namespace ShopDemo.Auth.Domain.Entities.Users;

public readonly struct PasswordHash : IEquatable<PasswordHash>
{
    public ReadOnlyMemory<byte> Value { get; }

    public bool IsEmpty => Value.IsEmpty;

    public int Length => Value.Length;

    private PasswordHash(ReadOnlyMemory<byte> value)
    {
        Value = value;
    }

    public static PasswordHash CreateNew(byte[] value)
    {
        var copy = new byte[value.Length];
        value.CopyTo(copy.AsSpan());
        return new PasswordHash(copy);
    }

    public bool Equals(PasswordHash other)
    {
        return CryptographicOperations.FixedTimeEquals(Value.Span, other.Value.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is PasswordHash other && Equals(other);
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

    public static bool operator ==(PasswordHash left, PasswordHash right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PasswordHash left, PasswordHash right)
    {
        return !left.Equals(right);
    }
}
