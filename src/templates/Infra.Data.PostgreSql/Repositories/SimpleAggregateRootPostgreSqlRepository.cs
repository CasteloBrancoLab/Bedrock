using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories;
using Templates.Domain.Entities.SimpleAggregateRoots;
using Templates.Infra.Data.PostgreSql.DataModels;
using Templates.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using Templates.Infra.Data.PostgreSql.Factories;
using Templates.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace Templates.Infra.Data.PostgreSql.Repositories;

public class SimpleAggregateRootPostgreSqlRepository
    : ISimpleAggregateRootPostgreSqlRepository
{
    private readonly ISimpleAggregateRootDataModelRepository _dataModelRepository;

    public SimpleAggregateRootPostgreSqlRepository(
        ISimpleAggregateRootDataModelRepository dataModelRepository)
    {
        ArgumentNullException.ThrowIfNull(dataModelRepository);

        _dataModelRepository = dataModelRepository;
    }

    public async Task<SimpleAggregateRoot?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        SimpleAggregateRootDataModel? dataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (dataModel is null)
            return null;

        return SimpleAggregateRootFactory.Create(dataModel);
    }

    public Task<bool> ExistsAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.ExistsAsync(
            executionContext,
            id,
            cancellationToken);
    }

    public Task<bool> RegisterNewAsync(
        ExecutionContext executionContext,
        SimpleAggregateRoot aggregateRoot,
        CancellationToken cancellationToken)
    {
        SimpleAggregateRootDataModel dataModel = SimpleAggregateRootDataModelFactory.Create(aggregateRoot);

        return _dataModelRepository.InsertAsync(
            executionContext,
            dataModel,
            cancellationToken);
    }

    public Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<SimpleAggregateRoot> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateAllAsync(
            executionContext,
            paginationInfo,
            CreateEnumerateAllDataModelHandler(executionContext, paginationInfo, handler),
            cancellationToken);
    }

    public Task<bool> EnumerateModifiedSinceAsync(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<SimpleAggregateRoot> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateModifiedSinceAsync(
            executionContext,
            since,
            CreateEnumerateModifiedSinceDataModelHandler(executionContext, timeProvider, since, handler),
            cancellationToken);
    }

    private static DataModelItemHandler<SimpleAggregateRootDataModel> CreateEnumerateAllDataModelHandler(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<SimpleAggregateRoot> handler)
    {
        return async (dataModel, cancellationToken) =>
        {
            SimpleAggregateRoot entity = SimpleAggregateRootFactory.Create(dataModel);
            return await handler(executionContext, entity, paginationInfo, cancellationToken);
        };
    }

    private static DataModelItemHandler<SimpleAggregateRootDataModel> CreateEnumerateModifiedSinceDataModelHandler(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<SimpleAggregateRoot> handler)
    {
        return async (dataModel, cancellationToken) =>
        {
            SimpleAggregateRoot entity = SimpleAggregateRootFactory.Create(dataModel);
            return await handler(executionContext, entity, timeProvider, since, cancellationToken);
        };
    }
}
