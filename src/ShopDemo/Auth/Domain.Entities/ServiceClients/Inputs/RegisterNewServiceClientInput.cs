using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewServiceClientInput(
    string ClientId,
    byte[] ClientSecretHash,
    string Name,
    Id CreatedByUserId,
    DateTimeOffset? ExpiresAt
);
