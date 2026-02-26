namespace ShopDemo.Auth.Domain.Entities.ExternalLogins;

public readonly struct LoginProvider : IEquatable<LoginProvider>
{
    public string Value { get; }

    public static readonly LoginProvider Google = new("google");
    public static readonly LoginProvider GitHub = new("github");
    public static readonly LoginProvider Microsoft = new("microsoft");
    public static readonly LoginProvider Apple = new("apple");

    private LoginProvider(string value)
    {
        Value = value;
    }

    public static LoginProvider CreateNew(string value)
    {
        return new LoginProvider(value);
    }

    public static LoginProvider CreateFromExistingInfo(string value)
    {
        return new LoginProvider(value);
    }

    public bool Equals(LoginProvider other)
    {
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is LoginProvider other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(LoginProvider left, LoginProvider right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LoginProvider left, LoginProvider right)
    {
        return !left.Equals(right);
    }
}
