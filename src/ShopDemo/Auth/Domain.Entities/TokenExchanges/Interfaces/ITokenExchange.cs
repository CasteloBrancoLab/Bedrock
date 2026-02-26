using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.TokenExchanges.Interfaces;

public interface ITokenExchange
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    string SubjectTokenJti { get; }
    string RequestedAudience { get; }
    string IssuedTokenJti { get; }
    DateTimeOffset IssuedAt { get; }
    DateTimeOffset ExpiresAt { get; }
}
