using System.Diagnostics.CodeAnalysis;
using ShopDemo.Core.Entities.Users.Enums;

namespace ShopDemo.Auth.Domain.Entities.Users.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct ChangeStatusInput(
    UserStatus NewStatus
);
