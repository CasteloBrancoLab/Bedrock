using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.PhoneNumbers;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Customers.Interfaces;

namespace ShopDemo.Orders.Domain.Entities.Customers;

public class IndividualCustomer
    : CustomerBase,
    IIndividualCustomer
{

    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;
    public BirthDate? DateOfBirth { get; }
    public PhoneNumber? PersonalPhoneNumber { get; }

    public override IEntity<CustomerBase> Clone()
    {
        return default!;
    }

    protected override bool IsValidInternal(ExecutionContext executionContext)
    {
        return true;
    }
}
