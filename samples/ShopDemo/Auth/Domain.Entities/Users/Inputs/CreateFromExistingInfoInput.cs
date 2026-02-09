using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Users.Enums;

namespace ShopDemo.Auth.Domain.Entities.Users.Inputs;

public readonly record struct CreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string Username,
    EmailAddress Email,
    PasswordHash PasswordHash,
    UserStatus Status
);
