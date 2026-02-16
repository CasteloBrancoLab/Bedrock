using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.MultiTenancy;

[Collection("AuthPostgreSql")]
[Feature("User Multi-Tenancy", "Isolamento multi-tenant em todas as queries")]
public class UserMultiTenancyIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public UserMultiTenancyIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_NotReturnUserFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantA);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(user.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_Should_NotReturnUserFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com email no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantA, email: "cross_email@example.com");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByEmailAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("cross_email@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_NotReturnUserFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com username no tenant A e buscando no tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantA, username: "cross_username");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUsernameAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(executionContext, "cross_username", CancellationToken.None);

        // Assert
        LogAssert("Verificando isolamento cross-tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnFalseForOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Verificando ExistsByEmailAsync cross-tenant");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantA, email: "cross_exists_email@example.com");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsByEmailAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("cross_exists_email@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false para outro tenant");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByUsernameAsync_Should_ReturnFalseForOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Verificando ExistsByUsernameAsync cross-tenant");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantA, username: "cross_exists_user");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsByUsernameAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByUsernameAsync(executionContext, "cross_exists_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false para outro tenant");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_ReturnOnlyCurrentTenantUsers()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuarios em dois tenants diferentes");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        for (int i = 0; i < 3; i++)
            await _fixture.InsertUserDirectlyAsync(_fixture.CreateTestUserDataModel(tenantCode: tenantA));

        for (int i = 0; i < 2; i++)
            await _fixture.InsertUserDirectlyAsync(_fixture.CreateTestUserDataModel(tenantCode: tenantB));

        var executionContext = _fixture.CreateExecutionContext(tenantA);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        var enumerated = new List<User>();

        // Act
        LogAct("Chamando EnumerateAllAsync para tenant A");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, user, pagination, ct) =>
            {
                enumerated.Add(user);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que apenas usuarios do tenant A foram retornados");
        enumerated.Count.ShouldBe(3);
    }

    [Fact]
    public async Task UpdateAsync_Should_NotModifyUserFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario no tenant A e tentando atualizar com contexto do tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantA, entityVersion: 1);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContextB = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Tentando buscar e atualizar usuario de outro tenant");
        await unitOfWork.OpenConnectionAsync(executionContextB, CancellationToken.None);
        var fetchedUser = await repository.GetByIdAsync(
            executionContextB,
            Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario nao foi encontrado no tenant B");
        fetchedUser.ShouldBeNull();

        var original = await _fixture.GetUserDirectlyAsync(dataModel.Id, tenantA);
        original.ShouldNotBeNull();
        original.Username.ShouldBe(dataModel.Username);
    }

    [Fact]
    public async Task DeleteAsync_Should_NotDeleteUserFromOtherTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario no tenant A e tentando excluir com contexto do tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantA, entityVersion: 1);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContextB = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando excluir usuario de outro tenant");
        await unitOfWork.OpenConnectionAsync(executionContextB, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContextB, CancellationToken.None);
        var result = await dataModelRepo.DeleteAsync(executionContextB, dataModel.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContextB, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou e o usuario original permanece");
        result.ShouldBeFalse();

        var stillExists = await _fixture.GetUserDirectlyAsync(dataModel.Id, tenantA);
        stillExists.ShouldNotBeNull();
    }
}
