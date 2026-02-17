using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Core.Entities.Users.Enums;

namespace ShopDemo.Auth.Domain.Entities.Users.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string Username,
    EmailAddress Email,
    PasswordHash PasswordHash,
    UserStatus Status
);
