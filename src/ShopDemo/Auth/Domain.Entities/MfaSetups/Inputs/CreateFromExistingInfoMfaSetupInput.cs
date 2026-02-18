using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace ShopDemo.Auth.Domain.Entities.MfaSetups.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoMfaSetupInput(
    EntityInfo EntityInfo,
    Id UserId,
    string EncryptedSharedSecret,
    bool IsEnabled,
    DateTimeOffset? EnabledAt
);
