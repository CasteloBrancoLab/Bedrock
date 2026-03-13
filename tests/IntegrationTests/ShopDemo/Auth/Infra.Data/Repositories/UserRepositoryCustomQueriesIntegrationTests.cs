using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.Repositories;

[Collection("AuthData")]
[Feature("UserRepository Custom Queries", "Facade Infra.Data: GetByEmail, ExistsByEmail, GetByUsername, ExistsByUsername")]
public class UserRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthDataFixture _fixture;

    public UserRepositoryCustomQueriesIntegrationTests(
        AuthDataFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnUser_WhenEmailExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-data"]);
        LogArrange("Inserindo usuario e buscando por email via facade");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(
            tenantCode: tenantCode,
            email: "email_query@example.com");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateUserRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByEmailAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("email_query@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi retornado");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(dataModel.Id);
        result.Email.Value.ShouldBe("email_query@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnNull_WhenEmailNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-data"]);
        LogArrange("Buscando email inexistente via facade");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateUserRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByEmailAsync para email inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("nonexistent@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnTrue_WhenEmailExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-data"]);
        LogArrange("Inserindo usuario e verificando existencia por email");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(
            tenantCode: tenantCode,
            email: "exists_email@example.com");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateUserRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsByEmailAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("exists_email@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnFalse_WhenEmailNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-data"]);
        LogArrange("Verificando existencia de email inexistente");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateUserRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsByEmailAsync para email inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByEmailAsync(
            executionContext,
            EmailAddress.CreateNew("ghost@example.com"),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_ReturnUser_WhenUsernameExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-data"]);
        LogArrange("Inserindo usuario e buscando por username via facade");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(
            tenantCode: tenantCode,
            username: "facade_user_query");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateUserRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUsernameAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(
            executionContext,
            "facade_user_query",
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi retornado");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(dataModel.Id);
        result.Username.ShouldBe("facade_user_query");
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_ReturnNull_WhenUsernameNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-data"]);
        LogArrange("Buscando username inexistente via facade");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateUserRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUsernameAsync para username inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(
            executionContext,
            "nonexistent_username",
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExistsByUsernameAsync_Should_ReturnTrue_WhenUsernameExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-data"]);
        LogArrange("Inserindo usuario e verificando existencia por username");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(
            tenantCode: tenantCode,
            username: "exists_username_facade");
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateUserRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsByUsernameAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByUsernameAsync(
            executionContext,
            "exists_username_facade",
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByUsernameAsync_Should_ReturnFalse_WhenUsernameNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-data"]);
        LogArrange("Verificando existencia de username inexistente");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateUserRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsByUsernameAsync para username inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByUsernameAsync(
            executionContext,
            "ghost_username",
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }
}
