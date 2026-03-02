using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a user's password is changed.
/// Self-contained: carries the input, previous state, and resulting state.
/// Note: password hash is never included in the user model for security.
/// </summary>
public sealed record PasswordChangedEvent(
    MessageMetadata Metadata,
    ChangePasswordInputModel Input,
    UserModel OldState,
    UserModel NewState
) : EventBase(Metadata);
