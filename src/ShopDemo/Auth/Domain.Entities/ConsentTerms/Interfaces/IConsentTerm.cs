using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;

namespace ShopDemo.Auth.Domain.Entities.ConsentTerms.Interfaces;

public interface IConsentTerm
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    ConsentTermType Type { get; }
    string Version { get; }
    string Content { get; }
    DateTimeOffset PublishedAt { get; }
}
