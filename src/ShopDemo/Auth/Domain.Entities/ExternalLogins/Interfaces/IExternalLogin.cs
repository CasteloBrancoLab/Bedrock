using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.ExternalLogins.Interfaces;

public interface IExternalLogin
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    LoginProvider Provider { get; }
    string ProviderUserId { get; }
    string? Email { get; }
}
