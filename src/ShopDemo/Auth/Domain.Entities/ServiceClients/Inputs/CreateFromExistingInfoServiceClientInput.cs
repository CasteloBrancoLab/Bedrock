using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;

namespace ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoServiceClientInput(
    EntityInfo EntityInfo,
    string ClientId,
    byte[] ClientSecretHash,
    string Name,
    ServiceClientStatus Status,
    Id CreatedByUserId,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? RevokedAt
);
