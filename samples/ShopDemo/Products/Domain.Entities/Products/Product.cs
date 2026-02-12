using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Products.Interfaces;

namespace ShopDemo.Products.Domain.Entities.Products;

public class Product
    : EntityBase<Product>,
    IProduct
{
    // Properties
    public string Code { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public string? Description { get; }

    public override IEntity<Product> Clone()
    {
        return default!;
    }

    protected override bool IsValidInternal(ExecutionContext executionContext)
    {
        return true;
    }
}
