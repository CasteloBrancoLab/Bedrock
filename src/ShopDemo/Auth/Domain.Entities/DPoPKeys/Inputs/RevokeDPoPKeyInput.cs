using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RevokeDPoPKeyInput;
