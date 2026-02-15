using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;

public sealed class UserPostgreSqlRepository
    : IUserPostgreSqlRepository
{
    private readonly IUserDataModelRepository _dataModelRepository;

    public UserPostgreSqlRepository(
        IUserDataModelRepository dataModelRepository)
    {
        ArgumentNullException.ThrowIfNull(dataModelRepository);

        _dataModelRepository = dataModelRepository;
    }

    public async Task<User?> GetByIdAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        UserDataModel? dataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (dataModel is null)
            return null;

        return UserFactory.Create(dataModel);
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
        User aggregateRoot,
        CancellationToken cancellationToken)
    {
        UserDataModel dataModel = UserDataModelFactory.Create(aggregateRoot);

        return _dataModelRepository.InsertAsync(
            executionContext,
            dataModel,
            cancellationToken);
    }

    public Task<bool> EnumerateAllAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<User> handler,
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
        EnumerateModifiedSinceItemHandler<User> handler,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.EnumerateModifiedSinceAsync(
            executionContext,
            since,
            CreateEnumerateModifiedSinceDataModelHandler(executionContext, timeProvider, since, handler),
            cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken)
    {
        UserDataModel? dataModel = await _dataModelRepository.GetByEmailAsync(
            executionContext,
            email.Value,
            cancellationToken);

        if (dataModel is null)
            return null;

        return UserFactory.Create(dataModel);
    }

    public async Task<User?> GetByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken)
    {
        UserDataModel? dataModel = await _dataModelRepository.GetByUsernameAsync(
            executionContext,
            username,
            cancellationToken);

        if (dataModel is null)
            return null;

        return UserFactory.Create(dataModel);
    }

    public Task<bool> ExistsByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.ExistsByEmailAsync(
            executionContext,
            email.Value,
            cancellationToken);
    }

    public Task<bool> ExistsByUsernameAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken)
    {
        return _dataModelRepository.ExistsByUsernameAsync(
            executionContext,
            username,
            cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        ExecutionContext executionContext,
        User aggregateRoot,
        CancellationToken cancellationToken)
    {
        UserDataModel? existingDataModel = await _dataModelRepository.GetByIdAsync(
            executionContext,
            aggregateRoot.EntityInfo.Id,
            cancellationToken);

        if (existingDataModel is null)
            return false;

        UserDataModelAdapter.Adapt(existingDataModel, aggregateRoot);

        return await _dataModelRepository.UpdateAsync(
            executionContext,
            existingDataModel,
            aggregateRoot.EntityInfo.EntityVersion,
            cancellationToken);
    }

    // Stryker disable all : Delegates internos capturados pelo mock - testados via callback nos testes de EnumerateAllAsync
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateAllAsync")]
    private static DataModelItemHandler<UserDataModel> CreateEnumerateAllDataModelHandler(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<User> handler)
    {
        return (dataModel, cancellationToken) =>
            HandleEnumerateAllItemAsync(executionContext, paginationInfo, handler, dataModel, cancellationToken);
    }

    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateAllAsync")]
    private static async Task<bool> HandleEnumerateAllItemAsync(
        ExecutionContext executionContext,
        PaginationInfo paginationInfo,
        EnumerateAllItemHandler<User> handler,
        UserDataModel dataModel,
        CancellationToken cancellationToken)
    {
        User entity = UserFactory.Create(dataModel);
        return await handler(executionContext, entity, paginationInfo, cancellationToken);
    }

    // Stryker restore all
    // Stryker disable all : Delegates internos - requer captura via callback mock com DataModelItemHandler
    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateModifiedSinceAsync")]
    private static DataModelItemHandler<UserDataModel> CreateEnumerateModifiedSinceDataModelHandler(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<User> handler)
    {
        return (dataModel, cancellationToken) =>
            HandleEnumerateModifiedSinceItemAsync(executionContext, timeProvider, since, handler, dataModel, cancellationToken);
    }

    [ExcludeFromCodeCoverage(Justification = "Delegate interno capturado pelo mock - testado via callback nos testes de EnumerateModifiedSinceAsync")]
    private static async Task<bool> HandleEnumerateModifiedSinceItemAsync(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DateTimeOffset since,
        EnumerateModifiedSinceItemHandler<User> handler,
        UserDataModel dataModel,
        CancellationToken cancellationToken)
    {
        User entity = UserFactory.Create(dataModel);
        return await handler(executionContext, entity, timeProvider, since, cancellationToken);
    }
    // Stryker restore all
}
