using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a user successfully authenticates.
/// Activity event: carries the input and the user snapshot at authentication time.
/// </summary>
public sealed record UserAuthenticatedEvent(
    MessageMetadata Metadata,
    AuthenticateUserInputModel Input,
    UserModel UserState
) : EventBase(Metadata);
