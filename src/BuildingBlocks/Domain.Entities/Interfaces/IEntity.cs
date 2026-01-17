using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace Bedrock.BuildingBlocks.Domain.Entities.Interfaces;

/// <summary>
/// Base contract for all domain entities.
/// </summary>
/// <remarks>
/// Every entity in the domain must have:
/// - A unique identifier (Id)
/// - Tenant information for multi-tenancy support
/// - Audit information (creation and modification tracking)
/// - Version for optimistic locking
/// </remarks>
public interface IEntity
{
    /// <summary>
    /// Gets the entity metadata including Id, TenantInfo, audit data, and version.
    /// </summary>
    EntityInfo EntityInfo { get; }

    /// <summary>
    /// Sets the entity information with validation.
    /// </summary>
    /// <param name="executionContext">The execution context for validation messages.</param>
    /// <param name="entityInfo">The new entity information.</param>
    /// <returns>true if the entity info was set successfully; false if validation failed.</returns>
    internal bool SetEntityInfo(ExecutionContext executionContext, EntityInfo entityInfo);
}

/// <summary>
/// Generic entity contract that adds cloning capability.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IEntity<TEntity> : IEntity
{
    /// <summary>
    /// Creates a deep clone of this entity.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    IEntity<TEntity> Clone();
}
