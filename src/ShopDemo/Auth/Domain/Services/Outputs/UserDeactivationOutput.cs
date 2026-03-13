using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Services.Outputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct UserDeactivationOutput(
    int RevokedRefreshTokenCount,
    int RevokedServiceClientCount,
    int RevokedApiKeyCount
);
