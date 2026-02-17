namespace ShopDemo.Auth.Domain.Entities.RefreshTokens;

public readonly struct TokenFamily : IEquatable<TokenFamily>
{
    public Guid Value { get; }

    private TokenFamily(Guid value)
    {
        Value = value;
    }

    public static TokenFamily CreateNew()
    {
        return new TokenFamily(Guid.NewGuid());
    }

    public static TokenFamily CreateFromExistingInfo(Guid value)
    {
        return new TokenFamily(value);
    }

    public bool Equals(TokenFamily other)
    {
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is TokenFamily other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static bool operator ==(TokenFamily left, TokenFamily right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TokenFamily left, TokenFamily right)
    {
        return !left.Equals(right);
    }
}
