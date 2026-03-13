using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a signing key is rotated.
/// Self-contained: carries the input, previous state, and resulting state.
/// Note: encrypted private key is never included in the model for security.
/// </summary>
public sealed record SigningKeyRotatedEvent(
    MessageMetadata Metadata,
    RotateSigningKeyInputModel Input,
    SigningKeyModel OldState,
    SigningKeyModel NewState
) : EventBase(Metadata);
