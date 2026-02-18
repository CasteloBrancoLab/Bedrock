using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.ServiceClientScopes.Interfaces;

public interface IServiceClientScope
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id ServiceClientId { get; }
    string Scope { get; }
}
