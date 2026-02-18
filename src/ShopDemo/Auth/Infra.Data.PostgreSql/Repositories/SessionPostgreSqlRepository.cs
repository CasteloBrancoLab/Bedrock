using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;

public sealed class SessionPostgreSqlRepository
    : ISessionPostgreSqlRepository
{
    private readonly ISessionDataModelRepository _dataModelRepository;

    public SessionPostgreSqlRepository(
        ISessionDataModelRepository dataModelRepository)
    {
        ArgumentNullException.ThrowIfNull(dataModelRepository);

        _dataModelRepository = dataModelRepository;
    }

    public async Task<Session?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        SessionDataModel? dataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (dataModel is null)
            return null;

        return SessionFactory.Create(dataModel);
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
        Session aggregateRoot,
        CancellationToken cancellationToken)
    {
        SessionDataModel dataModel = SessionDataModelFactory.Create(aggregateRoot);

        return _dataModelRepository.InsertAsync(
            executionContext,
            dataModel,
            cancellationToken);
    }

    public Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<Session> handler,
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
        EnumerateModifiedSinceItemHandler<Session> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateModifiedSinceAsync(
            executionContext,
            since,
            CreateEnumerateModifiedSinceDataModelHandler(executionContext, timeProvider, since, handler),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Session>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SessionDataModel> dataModels = await _dataModelRepository.GetByUserIdAsync(
            executionContext,
            userId.Value,
            cancellationToken);

        return dataModels.Select(SessionFactory.Create).ToList();
    }

    public async Task<IReadOnlyList<Session>> GetActiveByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SessionDataModel> dataModels = await _dataModelRepository.GetActiveByUserIdAsync(
            executionContext,
            userId.Value,
            cancellationToken);

        return dataModels.Select(SessionFactory.Create).ToList();
    }

    public Task<int> CountActiveByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.CountActiveByUserIdAsync(
            executionContext,
            userId.Value,
            cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        Session aggregateRoot,
        CancellationToken cancellationToken)
    {
        SessionDataModel? existingDataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            aggregateRoot.EntityInfo.Id,
            cancellationToken);

        if (existingDataModel is null)
            return false;

        long expectedVersion = existingDataModel.EntityVersion;

        SessionDataModelAdapter.Adapt(existingDataModel, aggregateRoot);

        return await _dataModelRepository.UpdateAsync(
            executionContext,
            existingDataModel,
            expectedVersion,
            cancellationToken);
    }

    // Stryker disable all : Delegates internos capturados pelo mock - testados via callback nos testes de EnumerateAllAsync
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateAllAsync")]
    private static DataModelItemHandler<SessionDataModel> CreateEnumerateAllDataModelHandler(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<Session> handler)
    {
        var adapter = new EnumerateAllHandlerAdapter(executionContext, paginationInfo, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all
    // Stryker disable all : Delegates internos - requer captura via callback mock com DataModelItemHandler
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateModifiedSinceAsync")]
    private static DataModelItemHandler<SessionDataModel> CreateEnumerateModifiedSinceDataModelHandler(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<Session> handler)
    {
        var adapter = new EnumerateModifiedSinceHandlerAdapter(executionContext, timeProvider, since, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateAllHandlerAdapter(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<Session> handler)
    {
        public async Task<bool> InvokeAsync(SessionDataModel dataModel, CancellationToken cancellationToken)
        {
            Session entity = SessionFactory.Create(dataModel);
            return await handler(executionContext, entity, paginationInfo, cancellationToken);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateModifiedSinceHandlerAdapter(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<Session> handler)
    {
        public async Task<bool> InvokeAsync(SessionDataModel dataModel, CancellationToken cancellationToken)
        {
            Session entity = SessionFactory.Create(dataModel);
            return await handler(executionContext, entity, timeProvider, since, cancellationToken);
        }
    }
}
