using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;

namespace ShopDemo.Core.Entities.Products;

public interface IProduct
    : IEntity
{
    public string Code { get; }
    public string Name { get; }
    public string? Description { get; }
}
