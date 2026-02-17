using System.Security.Cryptography;

namespace ShopDemo.Auth.Domain.Entities.RefreshTokens;

public readonly struct TokenHash : IEquatable<TokenHash>
{
    public ReadOnlyMemory<byte> Value { get; }

    public bool IsEmpty => Value.IsEmpty;

    public int Length => Value.Length;

    private TokenHash(ReadOnlyMemory<byte> value)
    {
        Value = value;
    }

    public static TokenHash CreateNew(byte[] value)
    {
        var copy = new byte[value.Length];
        value.CopyTo(copy.AsSpan());
        return new TokenHash(copy);
    }

    public bool Equals(TokenHash other)
    {
        return CryptographicOperations.FixedTimeEquals(Value.Span, other.Value.Span);
    }

    public override bool Equals(object? obj)
    {
        return obj is TokenHash other && Equals(other);
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

    public static bool operator ==(TokenHash left, TokenHash right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TokenHash left, TokenHash right)
    {
        return !left.Equals(right);
    }
}
