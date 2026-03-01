using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a user is deactivated in the Auth bounded context.
/// Self-contained: carries the original input and the deactivation result.
/// </summary>
public sealed record UserDeactivatedEvent(
    MessageMetadata Metadata,
    DeactivateUserInputModel Input,
    UserDeactivatedModel Result
) : EventBase(Metadata);
