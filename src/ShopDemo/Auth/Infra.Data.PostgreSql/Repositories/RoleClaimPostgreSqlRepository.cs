using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;

public sealed class RoleClaimPostgreSqlRepository
    : IRoleClaimPostgreSqlRepository
{
    private readonly IRoleClaimDataModelRepository _dataModelRepository;

    public RoleClaimPostgreSqlRepository(
        IRoleClaimDataModelRepository dataModelRepository)
    {
        ArgumentNullException.ThrowIfNull(dataModelRepository);

        _dataModelRepository = dataModelRepository;
    }

    public async Task<RoleClaim?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        RoleClaimDataModel? dataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (dataModel is null)
            return null;

        return RoleClaimFactory.Create(dataModel);
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
        RoleClaim aggregateRoot,
        CancellationToken cancellationToken)
    {
        RoleClaimDataModel dataModel = RoleClaimDataModelFactory.Create(aggregateRoot);

        return _dataModelRepository.InsertAsync(
            executionContext,
            dataModel,
            cancellationToken);
    }

    public Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RoleClaim> handler,
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
        EnumerateModifiedSinceItemHandler<RoleClaim> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateModifiedSinceAsync(
            executionContext,
            since,
            CreateEnumerateModifiedSinceDataModelHandler(executionContext, timeProvider, since, handler),
            cancellationToken);
    }

    public async Task<IReadOnlyList<RoleClaim>> GetByRoleIdAsync(
        ExecutionContext executionContext,
        Id roleId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RoleClaimDataModel> dataModels = await _dataModelRepository.GetByRoleIdAsync(
            executionContext,
            roleId.Value,
            cancellationToken);

        return dataModels.Select(RoleClaimFactory.Create).ToList();
    }

    public async Task<RoleClaim?> GetByRoleIdAndClaimIdAsync(
        ExecutionContext executionContext,
        Id roleId,
        Id claimId,
        CancellationToken cancellationToken)
    {
        RoleClaimDataModel? dataModel = await _dataModelRepository.GetByRoleIdAndClaimIdAsync(
            executionContext,
            roleId.Value,
            claimId.Value,
            cancellationToken);

        if (dataModel is null)
            return null;

        return RoleClaimFactory.Create(dataModel);
    }

    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        RoleClaim roleClaim,
        CancellationToken cancellationToken)
    {
        RoleClaimDataModel? existingDataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            roleClaim.EntityInfo.Id,
            cancellationToken);

        if (existingDataModel is null)
            return false;

        long expectedVersion = existingDataModel.EntityVersion;

        RoleClaimDataModelAdapter.Adapt(existingDataModel, roleClaim);

        return await _dataModelRepository.UpdateAsync(
            executionContext,
            existingDataModel,
            expectedVersion,
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        ExecutionContext executionContext,
        RoleClaim roleClaim,
        CancellationToken cancellationToken)
    {
        RoleClaimDataModel? existingDataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            roleClaim.EntityInfo.Id,
            cancellationToken);

        if (existingDataModel is null)
            return false;

        long expectedVersion = existingDataModel.EntityVersion;

        return await _dataModelRepository.DeleteAsync(
            executionContext,
            roleClaim.EntityInfo.Id,
            expectedVersion,
            cancellationToken);
    }

    // Stryker disable all : Delegates internos capturados pelo mock - testados via callback nos testes de EnumerateAllAsync
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateAllAsync")]
    private static DataModelItemHandler<RoleClaimDataModel> CreateEnumerateAllDataModelHandler(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RoleClaim> handler)
    {
        var adapter = new EnumerateAllHandlerAdapter(executionContext, paginationInfo, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all
    // Stryker disable all : Delegates internos - requer captura via callback mock com DataModelItemHandler
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateModifiedSinceAsync")]
    private static DataModelItemHandler<RoleClaimDataModel> CreateEnumerateModifiedSinceDataModelHandler(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<RoleClaim> handler)
    {
        var adapter = new EnumerateModifiedSinceHandlerAdapter(executionContext, timeProvider, since, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateAllHandlerAdapter(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RoleClaim> handler)
    {
        public async Task<bool> InvokeAsync(RoleClaimDataModel dataModel, CancellationToken cancellationToken)
        {
            RoleClaim entity = RoleClaimFactory.Create(dataModel);
            return await handler(executionContext, entity, paginationInfo, cancellationToken);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateModifiedSinceHandlerAdapter(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<RoleClaim> handler)
    {
        public async Task<bool> InvokeAsync(RoleClaimDataModel dataModel, CancellationToken cancellationToken)
        {
            RoleClaim entity = RoleClaimFactory.Create(dataModel);
            return await handler(executionContext, entity, timeProvider, since, cancellationToken);
        }
    }
}
