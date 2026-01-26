namespace Bedrock.BuildingBlocks.Core.TenantInfos;

public readonly struct TenantInfo
    : IEquatable<TenantInfo>
    , ISpanFormattable
{
    private const int GuidLength = 36;

    public Guid Code { get; }
    public string? Name { get; }

    private TenantInfo(Guid code, string? name)
    {
        Code = code;
        Name = name;
    }

    public static TenantInfo Create(Guid code, string? name = null)
    {
        return new TenantInfo(code, name);
    }

    public TenantInfo WithName(string? name)
    {
        return new TenantInfo(Code, name);
    }

    public override string ToString()
    {
        return ToString(null, null);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return Name is not null
            ? string.Concat(Name, " (", Code.ToString(), ")")
            : Code.ToString();
    }

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (Name is null)
        {
            return Code.TryFormat(destination, out charsWritten);
        }

        var nameSpan = Name.AsSpan();
        var requiredLength = nameSpan.Length + 3 + GuidLength;

        if (destination.Length < requiredLength)
        {
            charsWritten = 0;
            return false;
        }

        nameSpan.CopyTo(destination);
        var position = nameSpan.Length;

        destination[position++] = ' ';
        destination[position++] = '(';

        Code.TryFormat(destination.Slice(position), out var guidCharsWritten);
        position += guidCharsWritten;

        destination[position++] = ')';

        charsWritten = position;
        return true;
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is TenantInfo other && Equals(other);
    }

    public bool Equals(TenantInfo other)
    {
        return Code == other.Code;
    }

    public static bool operator ==(TenantInfo left, TenantInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TenantInfo left, TenantInfo right)
    {
        return !left.Equals(right);
    }
}
