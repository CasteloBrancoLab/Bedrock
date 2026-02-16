using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("UserPostgreSqlRepository Custom Queries", "Queries customizadas no nivel de entidade")]
public class UserPostgreSqlRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public UserPostgreSqlRepositoryCustomQueriesIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnUserEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario e buscando por email no nivel de dominio");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, email: "entity_email@example.com");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByEmailAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("entity_email@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade User foi retornada");
        result.ShouldNotBeNull();
        result.Email.Value.ShouldBe("entity_email@example.com");
        result.Username.ShouldBe(dataModel.Username);
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para email inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByEmailAsync para email inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("ghost@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_ReturnUserEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario e buscando por username no nivel de dominio");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, username: "entity_username");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUsernameAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(executionContext, "entity_username", CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade User foi retornada");
        result.ShouldNotBeNull();
        result.Username.ShouldBe("entity_username");
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para username inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUsernameAsync para username inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(executionContext, "ghost_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario e verificando existencia por email");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, email: "exists_entity@example.com");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsByEmailAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("exists_entity@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Verificando existencia por email inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsByEmailAsync para email inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("missing_entity@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByUsernameAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario e verificando existencia por username");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, username: "exists_entity_user");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsByUsernameAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByUsernameAsync(executionContext, "exists_entity_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByUsernameAsync_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Verificando existencia por username inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsByUsernameAsync para username inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByUsernameAsync(executionContext, "missing_entity_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }
}
