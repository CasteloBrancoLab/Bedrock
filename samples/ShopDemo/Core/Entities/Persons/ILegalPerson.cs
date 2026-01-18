using Bedrock.BuildingBlocks.Core.PhoneNumbers;

namespace ShopDemo.Core.Entities.Persons;

public interface ILegalPerson
    : IPerson
{
    public string LegalName { get; }
    public string TradeName { get; }
    public PhoneNumber? CommercialPhoneNumber { get; }
}
