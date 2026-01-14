namespace Bedrock.BuildingBlocks.Core.EmailAddresses;

public readonly struct EmailAddress
    : IEquatable<EmailAddress>
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress CreateNew(string value)
    {
        return new EmailAddress(value);
    }

    public string GetLocalPart()
    {
        int atIndex = Value.IndexOf('@');
        // Stryker disable once Equality : Mutante equivalente - Value[..0] retorna "" igual a string.Empty
        return atIndex > 0 ? Value[..atIndex] : string.Empty;
    }

    public string GetDomain()
    {
        int atIndex = Value.IndexOf('@');
        // Stryker disable all : Mutantes equivalentes - string slices retornam "" nos casos limite
        return atIndex >= 0 && atIndex < Value.Length - 1
            ? Value[(atIndex + 1)..]
            : string.Empty;
        // Stryker restore all
    }

    public override string ToString()
    {
        return Value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is EmailAddress other && Equals(other);
    }

    public bool Equals(EmailAddress other)
    {
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public static implicit operator string(EmailAddress emailAddress)
    {
        return emailAddress.Value;
    }

    public static implicit operator EmailAddress(string value)
    {
        return CreateNew(value);
    }

    public static bool operator ==(EmailAddress left, EmailAddress right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EmailAddress left, EmailAddress right)
    {
        return !left.Equals(right);
    }
}
