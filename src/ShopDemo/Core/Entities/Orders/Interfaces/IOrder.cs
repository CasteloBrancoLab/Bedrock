using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Customers.Interfaces;
using ShopDemo.Core.Entities.Orders.Enums;

namespace ShopDemo.Core.Entities.Orders.Interfaces;

public interface IOrder
    : IEntity
{
    public string OrderNumber { get; }
    public DateTime OrderDate { get; }
    public OrderStatus Status { get; }
}

public interface IOrder<TCustomer>
    : IOrder
    where TCustomer : ICustomer
{
    public TCustomer Customer { get; }
}