using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.EmailAddresses;

namespace ShopDemo.Auth.Domain.Entities.Users.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewInput(
    EmailAddress Email,
    PasswordHash PasswordHash
);
