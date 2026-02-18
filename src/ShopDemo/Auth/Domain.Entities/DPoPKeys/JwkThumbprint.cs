namespace ShopDemo.Auth.Domain.Entities.DPoPKeys;

public readonly struct JwkThumbprint : IEquatable<JwkThumbprint>
{
    public string Value { get; }

    private JwkThumbprint(string value)
    {
        Value = value;
    }

    public static JwkThumbprint CreateNew(string value)
    {
        return new JwkThumbprint(value);
    }

    public static JwkThumbprint CreateFromExistingInfo(string value)
    {
        return new JwkThumbprint(value);
    }

    public bool Equals(JwkThumbprint other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is JwkThumbprint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode(StringComparison.Ordinal) ?? 0;
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(JwkThumbprint left, JwkThumbprint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(JwkThumbprint left, JwkThumbprint right)
    {
        return !left.Equals(right);
    }
}
