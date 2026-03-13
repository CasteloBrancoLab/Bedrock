using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a session is revoked.
/// Self-contained: carries the input, previous state, and resulting state.
/// </summary>
public sealed record SessionRevokedEvent(
    MessageMetadata Metadata,
    RevokeSessionInputModel Input,
    SessionModel OldState,
    SessionModel NewState
) : EventBase(Metadata);
