using Bedrock.BuildingBlocks.Core.ExecutionContexts;

namespace Bedrock.BuildingBlocks.Domain.Entities.Models;

/// <summary>
/// Immutable record containing audit information about entity creation and modification.
/// </summary>
/// <remarks>
/// This record captures complete audit trail for an entity:
/// - Creation: When, by whom, correlation ID, origin, and business operation
/// - Last change: Same fields, populated only after first modification
///
/// All fields are immutable. Use factory methods to create new instances.
/// </remarks>
public readonly record struct EntityChangeInfo
{
    /// <summary>
    /// Gets the timestamp when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the identifier of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; }

    /// <summary>
    /// Gets the correlation ID of the operation that created the entity.
    /// </summary>
    public Guid CreatedCorrelationId { get; }

    /// <summary>
    /// Gets the origin (system/service) that created the entity.
    /// </summary>
    public string CreatedExecutionOrigin { get; }

    /// <summary>
    /// Gets the business operation code that created the entity.
    /// </summary>
    public string CreatedBusinessOperationCode { get; }

    /// <summary>
    /// Gets the timestamp when the entity was last modified, if ever.
    /// </summary>
    public DateTimeOffset? LastChangedAt { get; }

    /// <summary>
    /// Gets the identifier of the user who last modified the entity, if ever.
    /// </summary>
    public string? LastChangedBy { get; }

    /// <summary>
    /// Gets the correlation ID of the operation that last modified the entity, if ever.
    /// </summary>
    public Guid? LastChangedCorrelationId { get; }

    /// <summary>
    /// Gets the origin (system/service) that last modified the entity, if ever.
    /// </summary>
    public string? LastChangedExecutionOrigin { get; }

    /// <summary>
    /// Gets the business operation code that last modified the entity, if ever.
    /// </summary>
    public string? LastChangedBusinessOperationCode { get; }

    private EntityChangeInfo(
        DateTimeOffset createdAt,
        string createdBy,
        Guid createdCorrelationId,
        string createdExecutionOrigin,
        string createdBusinessOperationCode,
        DateTimeOffset? lastChangedAt,
        string? lastChangedBy,
        Guid? lastChangedCorrelationId,
        string? lastChangedExecutionOrigin,
        string? lastChangedBusinessOperationCode)
    {
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        CreatedCorrelationId = createdCorrelationId;
        CreatedExecutionOrigin = createdExecutionOrigin;
        CreatedBusinessOperationCode = createdBusinessOperationCode;
        LastChangedAt = lastChangedAt;
        LastChangedBy = lastChangedBy;
        LastChangedCorrelationId = lastChangedCorrelationId;
        LastChangedExecutionOrigin = lastChangedExecutionOrigin;
        LastChangedBusinessOperationCode = lastChangedBusinessOperationCode;
    }

    /// <summary>
    /// Creates a new EntityChangeInfo for a newly created entity.
    /// </summary>
    /// <param name="executionContext">The current execution context.</param>
    /// <param name="createdBy">The user creating the entity.</param>
    /// <returns>A new EntityChangeInfo with creation fields populated and change fields null.</returns>
    public static EntityChangeInfo RegisterNew(
        ExecutionContext executionContext,
        string createdBy)
    {
        return new EntityChangeInfo(
            createdAt: executionContext.TimeProvider.GetUtcNow(),
            createdBy: createdBy,
            createdCorrelationId: executionContext.CorrelationId,
            createdExecutionOrigin: executionContext.ExecutionOrigin,
            createdBusinessOperationCode: executionContext.BusinessOperationCode,
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);
    }

    /// <summary>
    /// Creates an EntityChangeInfo from existing/persisted values.
    /// </summary>
    /// <remarks>
    /// Use this when reconstructing an entity from persistence (database, cache, etc.).
    /// </remarks>
    public static EntityChangeInfo CreateFromExistingInfo(
        DateTimeOffset createdAt,
        string createdBy,
        Guid createdCorrelationId,
        string createdExecutionOrigin,
        string createdBusinessOperationCode,
        DateTimeOffset? lastChangedAt,
        string? lastChangedBy,
        Guid? lastChangedCorrelationId,
        string? lastChangedExecutionOrigin,
        string? lastChangedBusinessOperationCode)
    {
        return new EntityChangeInfo(
            createdAt: createdAt,
            createdBy: createdBy,
            createdCorrelationId: createdCorrelationId,
            createdExecutionOrigin: createdExecutionOrigin,
            createdBusinessOperationCode: createdBusinessOperationCode,
            lastChangedAt: lastChangedAt,
            lastChangedBy: lastChangedBy,
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: lastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: lastChangedBusinessOperationCode);
    }

    /// <summary>
    /// Creates a new EntityChangeInfo recording a change to the entity.
    /// </summary>
    /// <param name="executionContext">The current execution context.</param>
    /// <param name="changedBy">The user modifying the entity.</param>
    /// <returns>A new EntityChangeInfo with change fields populated from the context.</returns>
    public EntityChangeInfo RegisterChange(
        ExecutionContext executionContext,
        string changedBy)
    {
        return new EntityChangeInfo(
            createdAt: CreatedAt,
            createdBy: CreatedBy,
            createdCorrelationId: CreatedCorrelationId,
            createdExecutionOrigin: CreatedExecutionOrigin,
            createdBusinessOperationCode: CreatedBusinessOperationCode,
            lastChangedAt: executionContext.TimeProvider.GetUtcNow(),
            lastChangedBy: changedBy,
            lastChangedCorrelationId: executionContext.CorrelationId,
            lastChangedExecutionOrigin: executionContext.ExecutionOrigin,
            lastChangedBusinessOperationCode: executionContext.BusinessOperationCode);
    }
}
