using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a token exchange is performed.
/// Self-contained: carries the original input and the resulting state.
/// </summary>
public sealed record TokenExchangedEvent(
    MessageMetadata Metadata,
    ExchangeTokenInputModel Input,
    TokenExchangeModel NewState
) : EventBase(Metadata);
