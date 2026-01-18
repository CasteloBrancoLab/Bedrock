using Bedrock.BuildingBlocks.Core.Paginations;

namespace Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories;

/// <summary>
/// Marker interface for data model repositories at the persistence layer.
/// </summary>
public interface IDataModelRepository
{
}

/// <summary>
/// Handler delegate for processing data model items during enumeration.
/// </summary>
/// <typeparam name="TDataModel">The data model type.</typeparam>
/// <param name="item">The current data model item.</param>
/// <param name="cancellationToken">A token to cancel the operation.</param>
/// <returns>
/// True to continue iteration; false to stop iteration.
/// This allows the caller to interrupt enumeration when needed.
/// </returns>
/// <remarks>
/// The handler pattern replaces IAsyncEnumerable to avoid leaky abstractions.
/// Exceptions during iteration are caught and handled in the repository,
/// not propagated to the caller. This keeps infrastructure concerns
/// (like SqlException) hidden from the domain/service layer.
/// </remarks>
public delegate Task<bool> DataModelItemHandler<in TDataModel>(
    TDataModel item,
    CancellationToken cancellationToken);

/// <summary>
/// Generic interface for data model repositories that provide CRUD operations
/// for persistence layer data models.
/// </summary>
/// <typeparam name="TDataModel">The data model type for persistence.</typeparam>
public interface IDataModelRepository<TDataModel> : IDataModelRepository
    where TDataModel : class
{
    /// <summary>
    /// Retrieves a data model by its unique identifier.
    /// </summary>
    /// <param name="tenantCode">The tenant code for multi-tenancy filtering.</param>
    /// <param name="id">The unique identifier of the data model.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The data model if found; otherwise, null.</returns>
    Task<TDataModel?> GetByIdAsync(Guid tenantCode, Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Enumerates all data models with pagination support, calling the handler for each item.
    /// </summary>
    /// <param name="tenantCode">The tenant code for multi-tenancy filtering.</param>
    /// <param name="paginationInfo">The pagination information.</param>
    /// <param name="handler">The handler to call for each data model. Return false to stop iteration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if enumeration completed successfully; false if an error occurred.</returns>
    /// <remarks>
    /// Uses the handler pattern instead of IAsyncEnumerable to avoid leaky abstractions.
    /// Exceptions are caught and handled internally, returning false on failure.
    /// </remarks>
    Task<bool> EnumerateAllAsync(
        Guid tenantCode,
        PaginationInfo paginationInfo,
        DataModelItemHandler<TDataModel> handler,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a data model exists by its unique identifier.
    /// </summary>
    /// <param name="tenantCode">The tenant code for multi-tenancy filtering.</param>
    /// <param name="id">The unique identifier of the data model.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the data model exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid tenantCode, Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts a new data model.
    /// </summary>
    /// <param name="dataModel">The data model to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the insert was successful; otherwise, false.</returns>
    Task<bool> InsertAsync(TDataModel dataModel, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing data model.
    /// </summary>
    /// <param name="dataModel">The data model to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateAsync(TDataModel dataModel, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a data model by its unique identifier.
    /// </summary>
    /// <param name="tenantCode">The tenant code for multi-tenancy filtering.</param>
    /// <param name="id">The unique identifier of the data model.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the delete was successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(Guid tenantCode, Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Enumerates data models modified since a specific timestamp, calling the handler for each item.
    /// </summary>
    /// <param name="tenantCode">The tenant code for multi-tenancy filtering.</param>
    /// <param name="since">The timestamp to filter modifications.</param>
    /// <param name="handler">The handler to call for each data model. Return false to stop iteration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if enumeration completed successfully; false if an error occurred.</returns>
    /// <remarks>
    /// Uses the handler pattern instead of IAsyncEnumerable to avoid leaky abstractions.
    /// Exceptions are caught and handled internally, returning false on failure.
    /// </remarks>
    Task<bool> EnumerateModifiedSinceAsync(
        Guid tenantCode,
        DateTimeOffset since,
        DataModelItemHandler<TDataModel> handler,
        CancellationToken cancellationToken);
}
