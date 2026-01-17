using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;

namespace Bedrock.BuildingBlocks.Domain.Models;

/// <summary>
/// Immutable record containing all metadata for a domain entity.
/// </summary>
/// <remarks>
/// EntityInfo aggregates:
/// - Id: Unique identifier (UUIDv7)
/// - TenantInfo: Multi-tenancy support
/// - EntityChangeInfo: Complete audit trail
/// - EntityVersion: Optimistic locking support
/// </remarks>
public readonly record struct EntityInfo
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public Id Id { get; }

    /// <summary>
    /// Gets the tenant information for multi-tenancy support.
    /// </summary>
    public TenantInfo TenantInfo { get; }

    /// <summary>
    /// Gets the audit information (creation and modification tracking).
    /// </summary>
    public EntityChangeInfo EntityChangeInfo { get; }

    /// <summary>
    /// Gets the version for optimistic locking.
    /// </summary>
    public RegistryVersion EntityVersion { get; }

    private EntityInfo(
        Id id,
        TenantInfo tenantInfo,
        EntityChangeInfo entityChangeInfo,
        RegistryVersion entityVersion)
    {
        Id = id;
        TenantInfo = tenantInfo;
        EntityChangeInfo = entityChangeInfo;
        EntityVersion = entityVersion;
    }

    /// <summary>
    /// Creates a new EntityInfo for a newly created entity.
    /// </summary>
    /// <param name="executionContext">The current execution context.</param>
    /// <param name="tenantInfo">The tenant this entity belongs to.</param>
    /// <param name="createdBy">The user creating the entity.</param>
    /// <returns>A new EntityInfo with generated Id and version.</returns>
    public static EntityInfo RegisterNew(
        ExecutionContext executionContext,
        TenantInfo tenantInfo,
        string createdBy)
    {
        return new EntityInfo(
            id: Id.GenerateNewId(executionContext.TimeProvider),
            tenantInfo: tenantInfo,
            entityChangeInfo: EntityChangeInfo.RegisterNew(
                executionContext,
                createdBy: createdBy),
            entityVersion: RegistryVersion.GenerateNewVersion(executionContext.TimeProvider));
    }

    /// <summary>
    /// Creates an EntityInfo from existing EntityChangeInfo.
    /// </summary>
    /// <remarks>
    /// Use this when reconstructing an entity from persistence.
    /// </remarks>
    public static EntityInfo CreateFromExistingInfo(
        Id id,
        TenantInfo tenantInfo,
        EntityChangeInfo entityChangeInfo,
        RegistryVersion entityVersion)
    {
        return new EntityInfo(
            id: id,
            tenantInfo: tenantInfo,
            entityChangeInfo: entityChangeInfo,
            entityVersion: entityVersion);
    }

    /// <summary>
    /// Creates an EntityInfo from individual field values.
    /// </summary>
    /// <remarks>
    /// Use this when reconstructing an entity from persistence with raw values.
    /// </remarks>
    public static EntityInfo CreateFromExistingInfo(
        Id id,
        TenantInfo tenantInfo,
        DateTimeOffset createdAt,
        string createdBy,
        Guid createdCorrelationId,
        string createdExecutionOrigin,
        string createdBusinessOperationCode,
        DateTimeOffset? lastChangedAt,
        string? lastChangedBy,
        Guid? lastChangedCorrelationId,
        string? lastChangedExecutionOrigin,
        string? lastChangedBusinessOperationCode,
        RegistryVersion entityVersion)
    {
        return new EntityInfo(
            id: id,
            tenantInfo: tenantInfo,
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: createdAt,
                createdBy: createdBy,
                createdCorrelationId: createdCorrelationId,
                createdExecutionOrigin: createdExecutionOrigin,
                createdBusinessOperationCode: createdBusinessOperationCode,
                lastChangedAt: lastChangedAt,
                lastChangedBy: lastChangedBy,
                lastChangedCorrelationId: lastChangedCorrelationId,
                lastChangedExecutionOrigin: lastChangedExecutionOrigin,
                lastChangedBusinessOperationCode: lastChangedBusinessOperationCode),
            entityVersion: entityVersion);
    }

    /// <summary>
    /// Creates a new EntityInfo recording a change to the entity.
    /// </summary>
    /// <param name="executionContext">The current execution context.</param>
    /// <param name="changedBy">The user modifying the entity.</param>
    /// <returns>A new EntityInfo with updated change info and new version.</returns>
    public EntityInfo RegisterChange(
        ExecutionContext executionContext,
        string changedBy)
    {
        return new EntityInfo(
            id: Id,
            tenantInfo: TenantInfo,
            entityChangeInfo: EntityChangeInfo.RegisterChange(
                executionContext,
                changedBy: changedBy),
            entityVersion: RegistryVersion.GenerateNewVersion(executionContext.TimeProvider));
    }
}
