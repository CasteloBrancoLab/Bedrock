namespace Bedrock.BuildingBlocks.Domain.Entities.Interfaces;

/// <summary>
/// Marker interface for aggregate roots in Domain-Driven Design.
/// </summary>
/// <remarks>
/// An aggregate root is the main entry point for an aggregate.
/// It controls access to the aggregate's internal entities and ensures
/// consistency within the aggregate boundary.
///
/// Rules for aggregate roots:
/// - External objects can only hold references to the aggregate root
/// - All changes to the aggregate must go through the root
/// - The root is responsible for enforcing invariants across the aggregate
/// - Repositories only work with aggregate roots
/// </remarks>
public interface IAggregateRoot : IEntity
{
}
