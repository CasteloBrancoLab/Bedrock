using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.PhoneNumbers;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Customers.Interfaces;
using ShopDemo.Core.Entities.Persons.Enums;

namespace ShopDemo.Customers.Domain.Entities.Customers;

public class IndividualCustomer
    : CustomerBase,
    IIndividualCustomer
{
    // Properties
    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;
    public BirthDate? DateOfBirth { get; }
    public PhoneNumber? PersonalPhoneNumber { get; }

    // Constructors
    private IndividualCustomer(PersonType personType)
        : base(personType)
    {
    }

    protected override string GetDisplayName()
    {
        return $"{FirstName} {LastName}";
    }

    public override IEntity<CustomerBase> Clone()
    {
        return default!;
    }

    protected override bool IsValidInternal(ExecutionContext executionContext)
    {
        return true;
    }
}
