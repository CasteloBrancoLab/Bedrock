using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Domain.Entities;
using ShopDemo.Core.Entities.Customers;
using ShopDemo.Core.Entities.Persons.Enums;
using ShopDemo.Core.Entities.Persons.ValueObjects;

namespace ShopDemo.Orders.Domain.Entities.Customers;

public abstract class CustomerBase
    : EntityBase<CustomerBase>,
    ICustomer
{
    // Properties
    public PersonType PersonType { get; }
    public string DisplayName { get; } = string.Empty;
    public EmailAddress EmailAddress { get; }
    public IPersonIdentifier[] PersonIdentifierCollection { get; } = [];
}
