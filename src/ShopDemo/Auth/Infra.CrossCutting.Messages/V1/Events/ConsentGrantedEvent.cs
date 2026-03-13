using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a user grants consent to a consent term.
/// Self-contained: carries the original input and the resulting state.
/// </summary>
public sealed record ConsentGrantedEvent(
    MessageMetadata Metadata,
    GrantConsentInputModel Input,
    UserConsentModel NewState
) : EventBase(Metadata);
