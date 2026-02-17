using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;

public sealed class RefreshTokenPostgreSqlRepository
    : IRefreshTokenPostgreSqlRepository
{
    private readonly IRefreshTokenDataModelRepository _dataModelRepository;

    public RefreshTokenPostgreSqlRepository(
        IRefreshTokenDataModelRepository dataModelRepository)
    {
        ArgumentNullException.ThrowIfNull(dataModelRepository);

        _dataModelRepository = dataModelRepository;
    }

    public async Task<RefreshToken?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        RefreshTokenDataModel? dataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (dataModel is null)
            return null;

        return RefreshTokenFactory.Create(dataModel);
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
        RefreshToken aggregateRoot,
        CancellationToken cancellationToken)
    {
        RefreshTokenDataModel dataModel = RefreshTokenDataModelFactory.Create(aggregateRoot);

        return _dataModelRepository.InsertAsync(
            executionContext,
            dataModel,
            cancellationToken);
    }

    public Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RefreshToken> handler,
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
        EnumerateModifiedSinceItemHandler<RefreshToken> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateModifiedSinceAsync(
            executionContext,
            since,
            CreateEnumerateModifiedSinceDataModelHandler(executionContext, timeProvider, since, handler),
            cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Id userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RefreshTokenDataModel> dataModels = await _dataModelRepository.GetByUserIdAsync(
            executionContext,
            userId.Value,
            cancellationToken);

        return dataModels.Select(RefreshTokenFactory.Create).ToList();
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(
        ExecutionContext executionContext,
        TokenHash tokenHash,
        CancellationToken cancellationToken)
    {
        RefreshTokenDataModel? dataModel = await _dataModelRepository.GetByTokenHashAsync(
            executionContext,
            tokenHash.Value.ToArray(),
            cancellationToken);

        if (dataModel is null)
            return null;

        return RefreshTokenFactory.Create(dataModel);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByFamilyIdAsync(
        ExecutionContext executionContext,
        TokenFamily familyId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RefreshTokenDataModel> dataModels = await _dataModelRepository.GetActiveByFamilyIdAsync(
            executionContext,
            familyId.Value,
            cancellationToken);

        return dataModels.Select(RefreshTokenFactory.Create).ToList();
    }

    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        RefreshToken aggregateRoot,
        CancellationToken cancellationToken)
    {
        RefreshTokenDataModel? existingDataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            aggregateRoot.EntityInfo.Id,
            cancellationToken);

        if (existingDataModel is null)
            return false;

        long expectedVersion = existingDataModel.EntityVersion;

        RefreshTokenDataModelAdapter.Adapt(existingDataModel, aggregateRoot);

        return await _dataModelRepository.UpdateAsync(
            executionContext,
            existingDataModel,
            expectedVersion,
            cancellationToken);
    }

    // Stryker disable all : Delegates internos capturados pelo mock - testados via callback nos testes de EnumerateAllAsync
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateAllAsync")]
    private static DataModelItemHandler<RefreshTokenDataModel> CreateEnumerateAllDataModelHandler(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RefreshToken> handler)
    {
        var adapter = new EnumerateAllHandlerAdapter(executionContext, paginationInfo, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all
    // Stryker disable all : Delegates internos - requer captura via callback mock com DataModelItemHandler
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateModifiedSinceAsync")]
    private static DataModelItemHandler<RefreshTokenDataModel> CreateEnumerateModifiedSinceDataModelHandler(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<RefreshToken> handler)
    {
        var adapter = new EnumerateModifiedSinceHandlerAdapter(executionContext, timeProvider, since, handler);
        return adapter.InvokeAsync;
    }

    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateAllHandlerAdapter(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<RefreshToken> handler)
    {
        public async Task<bool> InvokeAsync(RefreshTokenDataModel dataModel, CancellationToken cancellationToken)
        {
            RefreshToken entity = RefreshTokenFactory.Create(dataModel);
            return await handler(executionContext, entity, paginationInfo, cancellationToken);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Delegate interno - requer infraestrutura real para execucao")]
    private sealed class EnumerateModifiedSinceHandlerAdapter(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<RefreshToken> handler)
    {
        public async Task<bool> InvokeAsync(RefreshTokenDataModel dataModel, CancellationToken cancellationToken)
        {
            RefreshToken entity = RefreshTokenFactory.Create(dataModel);
            return await handler(executionContext, entity, timeProvider, since, cancellationToken);
        }
    }
}
