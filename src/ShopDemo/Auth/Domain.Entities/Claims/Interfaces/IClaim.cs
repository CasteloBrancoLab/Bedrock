namespace ShopDemo.Auth.Domain.Entities.Claims.Interfaces;

public interface IClaim
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    string Name { get; }
    string? Description { get; }
}
