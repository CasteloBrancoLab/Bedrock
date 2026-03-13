using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RevokeConsentInputModel(
    Guid UserId,
    Guid ConsentTermId
);
