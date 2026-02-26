namespace ShopDemo.Auth.Domain.Entities.KeyChains;

public readonly struct KeyId : IEquatable<KeyId>
{
    public string Value { get; }

    private KeyId(string value)
    {
        Value = value;
    }

    public static KeyId CreateNew(string value)
    {
        return new KeyId(value);
    }

    public static KeyId CreateFromExistingInfo(string value)
    {
        return new KeyId(value);
    }

    public bool Equals(KeyId other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is KeyId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode(StringComparison.Ordinal) ?? 0;
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(KeyId left, KeyId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(KeyId left, KeyId right)
    {
        return !left.Equals(right);
    }
}
