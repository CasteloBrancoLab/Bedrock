namespace ShopDemo.Auth.Domain.Entities.Roles.Interfaces;

public interface IRole
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    string Name { get; }
    string? Description { get; }
}
