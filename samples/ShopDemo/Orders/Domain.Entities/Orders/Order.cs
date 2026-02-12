using System;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Orders.Interfaces;
using ShopDemo.Core.Entities.Orders.Enums;
using ShopDemo.Orders.Domain.Entities.Customers;

namespace ShopDemo.Orders.Domain.Entities.Orders;

// ArchRule disable DE001_SealedClass : sample placeholder — entidade demonstrativa, sealed será adicionado quando for implementada
// ArchRule disable DE004_InvalidStateNeverExists : sample placeholder — RegisterNew será implementado quando a entidade for completa
// ArchRule disable DE017_RegisterNewAndCreateFromExistingInfo : sample placeholder — CreateFromExistingInfo será implementado quando a entidade for completa
// ArchRule disable DE020_TwoPrivateConstructors : sample placeholder — construtores corretos serão implementados quando a entidade for completa
// ArchRule disable DE058_ProcessValidateSetForAssociatedAggregateRoots : sample placeholder — Process*Internal será implementado quando a entidade for completa
public class Order
    : EntityBase<Order>,
    IOrder<CustomerBase>
{
    public CustomerBase Customer { get; } = null!;
    public string OrderNumber { get; } = string.Empty;
    public DateTime OrderDate { get; }
    public OrderStatus Status { get; }

    public override IEntity<Order> Clone()
    {
        return default!;
    }

    protected override bool IsValidInternal(ExecutionContext executionContext)
    {
        return true;
    }
}
