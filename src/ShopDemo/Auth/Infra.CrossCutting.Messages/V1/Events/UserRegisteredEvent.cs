using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a new user is registered in the Auth bounded context.
/// Self-contained: carries the original input and the resulting state.
/// </summary>
public sealed record UserRegisteredEvent(
    MessageMetadata Metadata,
    RegisterUserInputModel Input,
    UserModel NewState
) : EventBase(Metadata);
