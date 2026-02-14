using Bedrock.BuildingBlocks.Core.EmailAddresses;

namespace ShopDemo.Auth.Domain.Entities.Users.Inputs;

public readonly record struct RegisterNewInput(
    EmailAddress Email,
    PasswordHash PasswordHash
);
