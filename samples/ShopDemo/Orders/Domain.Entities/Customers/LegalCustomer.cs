using Bedrock.BuildingBlocks.Core.PhoneNumbers;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Customers.Interfaces;

namespace ShopDemo.Orders.Domain.Entities.Customers;

public class LegalCustomer
    : CustomerBase,
    ILegalCustomer
{

    // Properties
    public string LegalName { get; } = string.Empty;
    public string TradeName { get; } = string.Empty;
    public PhoneNumber? CommercialPhoneNumber { get; }

    public override IEntity<CustomerBase> Clone()
    {
        return default!;
    }
    protected override bool IsValidInternal(ExecutionContext executionContext)
    {
        return true;
    }
}
