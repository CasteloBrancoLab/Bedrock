namespace ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Interfaces;

public interface IIdempotencyRecord
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    string IdempotencyKey { get; }
    string RequestHash { get; }
    string? ResponseBody { get; }
    int StatusCode { get; }
    DateTimeOffset ExpiresAt { get; }
}
