using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Products.Interfaces;

namespace ShopDemo.Orders.Domain.Entities.Products;

// ArchRule disable DE001_SealedClass : sample placeholder — entidade demonstrativa, sealed será adicionado quando for implementada
// ArchRule disable DE004_InvalidStateNeverExists : sample placeholder — RegisterNew será implementado quando a entidade for completa
// ArchRule disable DE017_RegisterNewAndCreateFromExistingInfo : sample placeholder — CreateFromExistingInfo será implementado quando a entidade for completa
// ArchRule disable DE020_TwoPrivateConstructors : sample placeholder — construtores corretos serão implementados quando a entidade for completa
public class Product
    : EntityBase<Product>,
    IProduct
{
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
