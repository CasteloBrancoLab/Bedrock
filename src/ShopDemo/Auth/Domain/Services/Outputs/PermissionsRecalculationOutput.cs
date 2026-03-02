using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Services.Outputs;

[ExcludeFromCodeCoverage(Justification = "Record class com propriedade de colecao — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public sealed record PermissionsRecalculationOutput(
    int ChangedClaimsCount
);
