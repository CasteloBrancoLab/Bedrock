using Bedrock.BuildingBlocks.Core.PhoneNumbers;

namespace ShopDemo.Core.Entities.Customers;

public interface ILegalCustomer
    : ICustomer
{
    public string LegalName { get; }
    public string TradeName { get; }
    public PhoneNumber? CommercialPhoneNumber { get; }
}
