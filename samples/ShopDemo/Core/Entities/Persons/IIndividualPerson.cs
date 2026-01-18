using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.PhoneNumbers;

namespace ShopDemo.Core.Entities.Persons;

public interface IIndividualPerson
    : IPerson
{
    public string FirstName { get; }
    public string LastName { get; }
    public BirthDate? DateOfBirth { get; }
    public PhoneNumber? PersonalPhoneNumber { get; }
}
