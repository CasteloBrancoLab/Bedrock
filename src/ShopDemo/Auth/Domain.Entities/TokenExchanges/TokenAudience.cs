using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Entities.TokenExchanges;

[ExcludeFromCodeCoverage(Justification = "Value object simples — testado indiretamente via TokenExchangeService")]
public readonly struct TokenAudience : IEquatable<TokenAudience>
{
    public string Value { get; }

    private TokenAudience(string value)
    {
        Value = value;
    }

    public static TokenAudience CreateNew(string value) => new(value);
    public static TokenAudience CreateFromExistingInfo(string value) => new(value);

    public static bool IsValidValue(string? value)
        => !string.IsNullOrWhiteSpace(value) && value.Length <= 255;

    public bool Equals(TokenAudience other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is TokenAudience other && Equals(other);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
    public override string ToString() => Value;

    public static bool operator ==(TokenAudience left, TokenAudience right) => left.Equals(right);
    public static bool operator !=(TokenAudience left, TokenAudience right) => !left.Equals(right);
}
