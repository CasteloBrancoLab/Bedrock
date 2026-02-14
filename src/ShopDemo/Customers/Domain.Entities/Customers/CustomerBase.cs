using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Domain.Entities;
using ShopDemo.Core.Entities.Customers.Interfaces;
using ShopDemo.Core.Entities.Persons.Enums;
using ShopDemo.Core.Entities.Persons.ValueObjects;

namespace ShopDemo.Customers.Domain.Entities.Customers;

// ArchRule disable DE051_IsValidHierarchyInAbstractClasses : sample placeholder — CustomerBase é demonstrativa e não implementa hierarquia completa de validação
// ArchRule disable DE055_RegisterNewBaseInAbstractClasses : sample placeholder — CustomerBase é demonstrativa e não implementa RegisterNewBase
public abstract class CustomerBase
    : EntityBase<CustomerBase>,
    ICustomer
{
    // Properties
    public PersonType PersonType { get; }
    public string DisplayName
    {
        get { return GetDisplayName(); }
    }
    public EmailAddress EmailAddress { get; }
    public IPersonIdentifier[] PersonIdentifierCollection { get; } = [];

    // Constructors
    protected CustomerBase(PersonType personType)
    {
        PersonType = personType;
    }

    // Protected Methods
    protected abstract string GetDisplayName();
}
