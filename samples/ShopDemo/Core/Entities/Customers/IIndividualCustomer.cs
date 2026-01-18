using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.PhoneNumbers;

namespace ShopDemo.Core.Entities.Customers;

public interface IIndividualCustomer
    : ICustomer
{
    public string FirstName { get; }
    public string LastName { get; }
    public BirthDate? DateOfBirth { get; }
    public PhoneNumber? PersonalPhoneNumber { get; }
}
