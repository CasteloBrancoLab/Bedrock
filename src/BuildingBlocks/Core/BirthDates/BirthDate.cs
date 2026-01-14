namespace Bedrock.BuildingBlocks.Core.BirthDates;

public readonly struct BirthDate
    : IEquatable<BirthDate>, IComparable<BirthDate>
{
    public DateTimeOffset Value { get; }

    private BirthDate(DateTimeOffset value)
    {
        Value = value;
    }

    public static BirthDate CreateNew(DateTimeOffset value)
    {
        return new BirthDate(value);
    }

    public static BirthDate CreateNew(DateOnly value)
    {
        return new BirthDate(
            value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
        );
    }

    public int CalculateAgeInYears(DateTimeOffset referenceDate)
    {
        int age = referenceDate.Year - Value.Year;

        if (referenceDate.Month < Value.Month ||
            (referenceDate.Month == Value.Month && referenceDate.Day < Value.Day))
        {
            age--;
        }

        return age;
    }

    public int CalculateAgeInYears(TimeProvider timeProvider)
    {
        return CalculateAgeInYears(timeProvider.GetUtcNow());
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is BirthDate other && Equals(other);
    }

    public bool Equals(BirthDate other)
    {
        return Value == other.Value;
    }

    public int CompareTo(BirthDate other)
    {
        return Value.CompareTo(other.Value);
    }

    public static implicit operator DateTimeOffset(BirthDate birthDate)
    {
        return birthDate.Value;
    }

    public static implicit operator BirthDate(DateTimeOffset value)
    {
        return CreateNew(value);
    }

    public static bool operator ==(BirthDate left, BirthDate right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(BirthDate left, BirthDate right)
    {
        return left.Value != right.Value;
    }

    public static bool operator <(BirthDate left, BirthDate right)
    {
        return left.Value.CompareTo(right.Value) < 0;
    }

    public static bool operator >(BirthDate left, BirthDate right)
    {
        return left.Value.CompareTo(right.Value) > 0;
    }

    public static bool operator <=(BirthDate left, BirthDate right)
    {
        return left.Value.CompareTo(right.Value) <= 0;
    }

    public static bool operator >=(BirthDate left, BirthDate right)
    {
        return left.Value.CompareTo(right.Value) >= 0;
    }
}
