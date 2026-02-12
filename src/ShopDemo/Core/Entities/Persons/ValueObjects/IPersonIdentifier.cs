namespace ShopDemo.Core.Entities.Persons.ValueObjects;

public interface IPersonIdentifier
{
    public string Value { get; }
    public string Type { get; }
    public string CountryCode { get; }
    public bool IsDefaultForCountryCode { get; }
}
