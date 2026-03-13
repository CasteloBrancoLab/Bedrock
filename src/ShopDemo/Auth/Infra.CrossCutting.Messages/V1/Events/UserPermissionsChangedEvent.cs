using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;

/// <summary>
/// Event raised when a user's API token permissions are recalculated.
/// Activity event: carries the input and the result of the recalculation.
/// </summary>
public sealed record UserPermissionsChangedEvent(
    MessageMetadata Metadata,
    RecalculatePermissionsInputModel Input,
    PermissionsChangedOutputModel Output
) : EventBase(Metadata);
