namespace ShopDemo.Auth.Domain.Entities.SigningKeys;

public readonly struct Kid : IEquatable<Kid>
{
    public string Value { get; }

    private Kid(string value)
    {
        Value = value;
    }

    public static Kid CreateNew(string value)
    {
        return new Kid(value);
    }

    public static Kid CreateFromExistingInfo(string value)
    {
        return new Kid(value);
    }

    public bool Equals(Kid other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is Kid other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode(StringComparison.Ordinal) ?? 0;
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(Kid left, Kid right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Kid left, Kid right)
    {
        return !left.Equals(right);
    }
}
