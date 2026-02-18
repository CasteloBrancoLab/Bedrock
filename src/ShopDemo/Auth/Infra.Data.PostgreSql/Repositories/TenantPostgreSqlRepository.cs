using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;

public sealed class TenantPostgreSqlRepository
    : ITenantPostgreSqlRepository
{
    private readonly ITenantDataModelRepository _dataModelRepository;

    public TenantPostgreSqlRepository(
        ITenantDataModelRepository dataModelRepository)
    {
        ArgumentNullException.ThrowIfNull(dataModelRepository);

        _dataModelRepository = dataModelRepository;
    }

    public async Task<Tenant?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        TenantDataModel? dataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (dataModel is null)
            return null;

        return TenantFactory.Create(dataModel);
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
        Tenant aggregateRoot,
        CancellationToken cancellationToken)
    {
        TenantDataModel dataModel = TenantDataModelFactory.Create(aggregateRoot);

        return _dataModelRepository.InsertAsync(
            executionContext,
            dataModel,
            cancellationToken);
    }

    public Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<Tenant> handler,
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
        EnumerateModifiedSinceItemHandler<Tenant> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateModifiedSinceAsync(
            executionContext,
            since,
            CreateEnumerateModifiedSinceDataModelHandler(executionContext, timeProvider, since, handler),
            cancellationToken);
    }

    public async Task<Tenant?> GetByDomainAsync(
        ExecutionContext executionContext,
        string domain,
        CancellationToken cancellationToken)
    {
        TenantDataModel? dataModel = await _dataModelRepository.GetByDomainAsync(
            executionContext,
            domain,
            cancellationToken);

        if (dataModel is null)
            return null;

        return TenantFactory.Create(dataModel);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TenantDataModel> dataModels = await _dataModelRepository.GetAllAsync(
            executionContext,
            cancellationToken);

        return dataModels.Select(TenantFactory.Create).ToList();
    }

    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        Tenant aggregateRoot,
        CancellationToken cancellationToken)
    {
        TenantDataModel? existingDataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            aggregateRoot.EntityInfo.Id,
            cancellationToken);

        if (existingDataModel is null)
            return false;

        long expectedVersion = existingDataModel.EntityVersion;

        TenantDataModelAdapter.Adapt(existingDataModel, aggregateRoot);

        return await _dataModelRepository.UpdateAsync(
            executionContext,
            existingDataModel,
            expectedVersion,
            cancellationToken);
    }

    // Stryker disable all : Delegates internos capturados pelo mock - testados via callback nos testes de EnumerateAllAsync
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateAllAsync")]
    private static DataModelItemHandler<TenantDataModel> CreateEnumerateAllDataModelHandler(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<Tenant> handler)
    {
        var adapter = new EnumerateAllHandlerAdapter(executionContext, paginationInfo, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all
    // Stryker disable all : Delegates internos - requer captura via callback mock com DataModelItemHandler
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateModifiedSinceAsync")]
    private static DataModelItemHandler<TenantDataModel> CreateEnumerateModifiedSinceDataModelHandler(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<Tenant> handler)
    {
        var adapter = new EnumerateModifiedSinceHandlerAdapter(executionContext, timeProvider, since, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateAllHandlerAdapter(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<Tenant> handler)
    {
        public async Task<bool> InvokeAsync(TenantDataModel dataModel, CancellationToken cancellationToken)
        {
            Tenant entity = TenantFactory.Create(dataModel);
            return await handler(executionContext, entity, paginationInfo, cancellationToken);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateModifiedSinceHandlerAdapter(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<Tenant> handler)
    {
        public async Task<bool> InvokeAsync(TenantDataModel dataModel, CancellationToken cancellationToken)
        {
            Tenant entity = TenantFactory.Create(dataModel);
            return await handler(executionContext, entity, timeProvider, since, cancellationToken);
        }
    }
}
