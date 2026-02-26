using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;

namespace ShopDemo.Auth.Domain.Entities.SigningKeys.Interfaces;

public interface ISigningKey
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Kid Kid { get; }
    string Algorithm { get; }
    string PublicKey { get; }
    string EncryptedPrivateKey { get; }
    SigningKeyStatus Status { get; }
    DateTimeOffset? RotatedAt { get; }
    DateTimeOffset ExpiresAt { get; }
}
