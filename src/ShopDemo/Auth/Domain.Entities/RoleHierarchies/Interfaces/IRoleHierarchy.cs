using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.RoleHierarchies.Interfaces;

public interface IRoleHierarchy
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id RoleId { get; }
    Id ParentRoleId { get; }
}
