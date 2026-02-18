using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;

public sealed class RoleHierarchyPostgreSqlRepository
    : IRoleHierarchyPostgreSqlRepository
{
    private readonly IRoleHierarchyDataModelRepository _dataModelRepository;

    public RoleHierarchyPostgreSqlRepository(
        IRoleHierarchyDataModelRepository dataModelRepository)
    {
        ArgumentNullException.ThrowIfNull(dataModelRepository);

        _dataModelRepository = dataModelRepository;
    }

    public async Task<RoleHierarchy?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        RoleHierarchyDataModel? dataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (dataModel is null)
            return null;

        return RoleHierarchyFactory.Create(dataModel);
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
        RoleHierarchy aggregateRoot,
        CancellationToken cancellationToken)
    {
        RoleHierarchyDataModel dataModel = RoleHierarchyDataModelFactory.Create(aggregateRoot);

        return _dataModelRepository.InsertAsync(
            executionContext,
            dataModel,
            cancellationToken);
    }

    public Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RoleHierarchy> handler,
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
        EnumerateModifiedSinceItemHandler<RoleHierarchy> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateModifiedSinceAsync(
            executionContext,
            since,
            CreateEnumerateModifiedSinceDataModelHandler(executionContext, timeProvider, since, handler),
            cancellationToken);
    }

    public async Task<IReadOnlyList<RoleHierarchy>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Id roleId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RoleHierarchyDataModel> dataModels = await _dataModelRepository.GetByRoleIdAsync(
            executionContext,
            roleId.Value,
            cancellationToken);

        return dataModels.Select(RoleHierarchyFactory.Create).ToList();
    }

    public async Task<IReadOnlyList<RoleHierarchy>> GetByParentRoleIdAsync(
        ExecutionContext executionContext,
        Id parentRoleId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RoleHierarchyDataModel> dataModels = await _dataModelRepository.GetByParentRoleIdAsync(
            executionContext,
            parentRoleId.Value,
            cancellationToken);

        return dataModels.Select(RoleHierarchyFactory.Create).ToList();
    }

    public async Task<IReadOnlyList<RoleHierarchy>> GetAllAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RoleHierarchyDataModel> dataModels = await _dataModelRepository.GetAllAsync(
            executionContext,
            cancellationToken);

        return dataModels.Select(RoleHierarchyFactory.Create).ToList();
    }

    public async Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        RoleHierarchy roleHierarchy,
        CancellationToken cancellationToken)
    {
        RoleHierarchyDataModel? existingDataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            roleHierarchy.EntityInfo.Id,
            cancellationToken);

        if (existingDataModel is null)
            return false;

        long expectedVersion = existingDataModel.EntityVersion;

        return await _dataModelRepository.DeleteAsync(
            executionContext,
            roleHierarchy.EntityInfo.Id,
            expectedVersion,
            cancellationToken);
    }

    // Stryker disable all : Delegates internos capturados pelo mock - testados via callback nos testes de EnumerateAllAsync
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateAllAsync")]
    private static DataModelItemHandler<RoleHierarchyDataModel> CreateEnumerateAllDataModelHandler(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RoleHierarchy> handler)
    {
        var adapter = new EnumerateAllHandlerAdapter(executionContext, paginationInfo, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all
    // Stryker disable all : Delegates internos - requer captura via callback mock com DataModelItemHandler
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateModifiedSinceAsync")]
    private static DataModelItemHandler<RoleHierarchyDataModel> CreateEnumerateModifiedSinceDataModelHandler(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<RoleHierarchy> handler)
    {
        var adapter = new EnumerateModifiedSinceHandlerAdapter(executionContext, timeProvider, since, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateAllHandlerAdapter(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RoleHierarchy> handler)
    {
        public async Task<bool> InvokeAsync(RoleHierarchyDataModel dataModel, CancellationToken cancellationToken)
        {
            RoleHierarchy entity = RoleHierarchyFactory.Create(dataModel);
            return await handler(executionContext, entity, paginationInfo, cancellationToken);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateModifiedSinceHandlerAdapter(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<RoleHierarchy> handler)
    {
        public async Task<bool> InvokeAsync(RoleHierarchyDataModel dataModel, CancellationToken cancellationToken)
        {
            RoleHierarchy entity = RoleHierarchyFactory.Create(dataModel);
            return await handler(executionContext, entity, timeProvider, since, cancellationToken);
        }
    }
}
