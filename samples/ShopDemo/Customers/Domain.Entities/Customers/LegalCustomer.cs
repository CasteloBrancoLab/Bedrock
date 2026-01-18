using Bedrock.BuildingBlocks.Core.PhoneNumbers;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Customers;
using ShopDemo.Core.Entities.Persons.Enums;

namespace ShopDemo.Customers.Domain.Entities.Customers;

public class LegalCustomer
    : CustomerBase,
    ILegalCustomer
{
    public string LegalName { get; } = string.Empty;
    public string TradeName { get; } = string.Empty;
    public PhoneNumber? CommercialPhoneNumber { get; }

    private LegalCustomer()
        : base(PersonType.Individual)
    {
    }

    public override IEntity<CustomerBase> Clone()
    {
        return default!;
    }
    protected override bool IsValidInternal(ExecutionContext executionContext)
    {
        return true;
    }

    protected override string GetDisplayName()
    {
        return LegalName;
    }
}
