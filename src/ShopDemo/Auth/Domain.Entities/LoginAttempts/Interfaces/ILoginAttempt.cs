namespace ShopDemo.Auth.Domain.Entities.LoginAttempts.Interfaces;

public interface ILoginAttempt
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    string Username { get; }
    string? IpAddress { get; }
    DateTimeOffset AttemptedAt { get; }
    bool IsSuccessful { get; }
    string? FailureReason { get; }
}
