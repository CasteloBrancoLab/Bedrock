using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when all sessions for a user are revoked.
/// Activity event: carries the input and the result with revocation count.
/// </summary>
public sealed record AllSessionsRevokedEvent(
    MessageMetadata Metadata,
    RevokeAllSessionsInputModel Input,
    AllSessionsRevokedOutputModel Output
) : EventBase(Metadata);
