using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when an impersonation session is ended.
/// Self-contained: carries the input, previous state, and resulting state.
/// </summary>
public sealed record ImpersonationEndedEvent(
    MessageMetadata Metadata,
    EndImpersonationInputModel Input,
    ImpersonationSessionModel OldState,
    ImpersonationSessionModel NewState
) : EventBase(Metadata);
