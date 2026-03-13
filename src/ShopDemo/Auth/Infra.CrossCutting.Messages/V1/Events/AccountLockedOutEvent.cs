using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when an account is locked out due to excessive failed login attempts.
/// Activity event: carries the input and the lockout result.
/// </summary>
public sealed record AccountLockedOutEvent(
    MessageMetadata Metadata,
    AccountLockedOutInputModel Input,
    AccountLockoutOutputModel Output
) : EventBase(Metadata);
