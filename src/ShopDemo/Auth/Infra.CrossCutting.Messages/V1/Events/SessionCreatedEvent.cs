using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a new session is created.
/// Self-contained: carries the original input and the resulting state.
/// </summary>
public sealed record SessionCreatedEvent(
    MessageMetadata Metadata,
    CreateSessionInputModel Input,
    SessionModel NewState
) : EventBase(Metadata);
