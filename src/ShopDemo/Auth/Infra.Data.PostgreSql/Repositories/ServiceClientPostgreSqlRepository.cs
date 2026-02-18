using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;

public sealed class ServiceClientPostgreSqlRepository
    : IServiceClientPostgreSqlRepository
{
    private readonly IServiceClientDataModelRepository _dataModelRepository;

    public ServiceClientPostgreSqlRepository(
        IServiceClientDataModelRepository dataModelRepository)
    {
        ArgumentNullException.ThrowIfNull(dataModelRepository);

        _dataModelRepository = dataModelRepository;
    }

    public async Task<ServiceClient?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        ServiceClientDataModel? dataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (dataModel is null)
            return null;

        return ServiceClientFactory.Create(dataModel);
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
        ServiceClient aggregateRoot,
        CancellationToken cancellationToken)
    {
        ServiceClientDataModel dataModel = ServiceClientDataModelFactory.Create(aggregateRoot);

        return _dataModelRepository.InsertAsync(
            executionContext,
            dataModel,
            cancellationToken);
    }

    public Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<ServiceClient> handler,
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
        EnumerateModifiedSinceItemHandler<ServiceClient> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateModifiedSinceAsync(
            executionContext,
            since,
            CreateEnumerateModifiedSinceDataModelHandler(executionContext, timeProvider, since, handler),
            cancellationToken);
    }

    public async Task<ServiceClient?> GetByClientIdAsync(
        ExecutionContext executionContext,
        string clientId,
        CancellationToken cancellationToken)
    {
        ServiceClientDataModel? dataModel = await _dataModelRepository.GetByClientIdAsync(
            executionContext,
            clientId,
            cancellationToken);

        if (dataModel is null)
            return null;

        return ServiceClientFactory.Create(dataModel);
    }

    public async Task<IReadOnlyList<ServiceClient>> GetByCreatorUserIdAsync(
        ExecutionContext executionContext,
        Id createdByUserId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceClientDataModel> dataModels = await _dataModelRepository.GetByCreatorUserIdAsync(
            executionContext,
            createdByUserId.Value,
            cancellationToken);

        return dataModels.Select(ServiceClientFactory.Create).ToList();
    }

    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        ServiceClient aggregateRoot,
        CancellationToken cancellationToken)
    {
        ServiceClientDataModel? existingDataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            aggregateRoot.EntityInfo.Id,
            cancellationToken);

        if (existingDataModel is null)
            return false;

        long expectedVersion = existingDataModel.EntityVersion;

        ServiceClientDataModelAdapter.Adapt(existingDataModel, aggregateRoot);

        return await _dataModelRepository.UpdateAsync(
            executionContext,
            existingDataModel,
            expectedVersion,
            cancellationToken);
    }

    // Stryker disable all : Delegates internos capturados pelo mock - testados via callback nos testes de EnumerateAllAsync
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateAllAsync")]
    private static DataModelItemHandler<ServiceClientDataModel> CreateEnumerateAllDataModelHandler(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<ServiceClient> handler)
    {
        var adapter = new EnumerateAllHandlerAdapter(executionContext, paginationInfo, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all
    // Stryker disable all : Delegates internos - requer captura via callback mock com DataModelItemHandler
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateModifiedSinceAsync")]
    private static DataModelItemHandler<ServiceClientDataModel> CreateEnumerateModifiedSinceDataModelHandler(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<ServiceClient> handler)
    {
        var adapter = new EnumerateModifiedSinceHandlerAdapter(executionContext, timeProvider, since, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateAllHandlerAdapter(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<ServiceClient> handler)
    {
        public async Task<bool> InvokeAsync(ServiceClientDataModel dataModel, CancellationToken cancellationToken)
        {
            ServiceClient entity = ServiceClientFactory.Create(dataModel);
            return await handler(executionContext, entity, paginationInfo, cancellationToken);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateModifiedSinceHandlerAdapter(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<ServiceClient> handler)
    {
        public async Task<bool> InvokeAsync(ServiceClientDataModel dataModel, CancellationToken cancellationToken)
        {
            ServiceClient entity = ServiceClientFactory.Create(dataModel);
            return await handler(executionContext, entity, timeProvider, since, cancellationToken);
        }
    }
}
