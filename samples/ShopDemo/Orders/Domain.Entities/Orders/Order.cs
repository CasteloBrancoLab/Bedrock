using System;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Orders;
using ShopDemo.Core.Entities.Orders.Enums;
using ShopDemo.Orders.Domain.Entities.Customers;

namespace ShopDemo.Orders.Domain.Entities.Orders;

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
