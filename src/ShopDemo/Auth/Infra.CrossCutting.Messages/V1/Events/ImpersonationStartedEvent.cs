using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when an impersonation session is started.
/// Self-contained: carries the original input and the resulting state.
/// </summary>
public sealed record ImpersonationStartedEvent(
    MessageMetadata Metadata,
    StartImpersonationInputModel Input,
    ImpersonationSessionModel NewState
) : EventBase(Metadata);
