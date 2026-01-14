namespace Bedrock.BuildingBlocks.Core.TenantInfos;

public readonly struct TenantInfo
    : IEquatable<TenantInfo>
{
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
        return Name is not null ? $"{Name} ({Code})" : Code.ToString();
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
