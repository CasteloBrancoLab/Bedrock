using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.PhoneNumbers;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Customers.Interfaces;

namespace ShopDemo.Orders.Domain.Entities.Customers;

// ArchRule disable DE001_SealedClass : sample placeholder — entidade demonstrativa, sealed será adicionado quando for implementada
// ArchRule disable DE004_InvalidStateNeverExists : sample placeholder — RegisterNew será implementado quando a entidade for completa
// ArchRule disable DE017_RegisterNewAndCreateFromExistingInfo : sample placeholder — CreateFromExistingInfo será implementado quando a entidade for completa
// ArchRule disable DE020_TwoPrivateConstructors : sample placeholder — construtores corretos serão implementados quando a entidade for completa
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
