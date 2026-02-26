namespace ShopDemo.Auth.Domain.Entities.Claims;

public readonly struct ClaimValue : IEquatable<ClaimValue>
{
    public short Value { get; }

    public static readonly ClaimValue Granted = new(1);
    public static readonly ClaimValue Denied = new(-1);
    public static readonly ClaimValue Inherited = new(0);

    public bool IsGranted => Value == 1;
    public bool IsDenied => Value == -1;
    public bool IsInherited => Value == 0;

    private ClaimValue(short value)
    {
        Value = value;
    }

    public static ClaimValue CreateNew(short value)
    {
        return new ClaimValue(value);
    }

    public static ClaimValue CreateFromExistingInfo(short value)
    {
        return new ClaimValue(value);
    }

    public static bool IsValidValue(short value)
    {
        return value is 1 or -1 or 0;
    }

    public bool Equals(ClaimValue other)
    {
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is ClaimValue other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value switch
        {
            1 => "Granted",
            -1 => "Denied",
            0 => "Inherited",
            _ => Value.ToString()
        };
    }

    public static bool operator ==(ClaimValue left, ClaimValue right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ClaimValue left, ClaimValue right)
    {
        return !left.Equals(right);
    }
}
