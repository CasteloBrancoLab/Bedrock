using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a user revokes consent to a consent term.
/// Self-contained: carries the input, previous state, and resulting state.
/// </summary>
public sealed record ConsentRevokedEvent(
    MessageMetadata Metadata,
    RevokeConsentInputModel Input,
    UserConsentModel OldState,
    UserConsentModel NewState
) : EventBase(Metadata);
