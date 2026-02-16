using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.DataModelRepository;

[Collection("AuthPostgreSql")]
[Feature("UserDataModel Custom Queries", "Queries customizadas do repositorio de UserDataModel")]
public class UserDataModelRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public UserDataModelRepositoryCustomQueriesIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByEmailAsync_Should_ReturnUser_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com email especifico");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, email: "findme@example.com");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByEmailAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(executionContext, "findme@example.com", CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi encontrado pelo email");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Email.ShouldBe("findme@example.com");
        result.Username.ShouldBe(user.Username);
        result.PasswordHash.ShouldBe(user.PasswordHash);
        result.Status.ShouldBe(user.Status);
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
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByEmailAsync para email inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(executionContext, "nonexistent@example.com", CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantA, email: "tenant_filter@example.com");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByEmailAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(executionContext, "tenant_filter@example.com", CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra usuario de outro tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_Should_PopulateAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com todos os campos preenchidos");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        user.LastChangedBy = "modifier_user";
        user.LastChangedAt = DateTimeOffset.UtcNow;
        user.LastChangedExecutionOrigin = "TestOrigin";
        user.LastChangedCorrelationId = Guid.NewGuid();
        user.LastChangedBusinessOperationCode = "UPDATE_OP";
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByEmailAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByEmailAsync(executionContext, user.Email, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram populados");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.TenantCode.ShouldBe(user.TenantCode);
        result.Username.ShouldBe(user.Username);
        result.Email.ShouldBe(user.Email);
        result.PasswordHash.ShouldBe(user.PasswordHash);
        result.Status.ShouldBe(user.Status);
        result.CreatedBy.ShouldBe(user.CreatedBy);
        result.EntityVersion.ShouldBe(user.EntityVersion);
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_ReturnUser_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com username especifico");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, username: "findme_user");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUsernameAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(executionContext, "findme_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi encontrado pelo username");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Username.ShouldBe("findme_user");
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
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUsernameAsync para username inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(executionContext, "nonexistent_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantA, username: "cross_tenant_user");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUsernameAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(executionContext, "cross_tenant_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra usuario de outro tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_PopulateAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com todos os campos preenchidos");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUsernameAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUsernameAsync(executionContext, user.Username, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram populados");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Email.ShouldBe(user.Email);
        result.PasswordHash.ShouldBe(user.PasswordHash);
        result.Status.ShouldBe(user.Status);
    }

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com email especifico");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, email: "exists@example.com");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsByEmailAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByEmailAsync(executionContext, "exists@example.com", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para email inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsByEmailAsync para email inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByEmailAsync(executionContext, "missing@example.com", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByUsernameAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario com username especifico");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, username: "exists_user");
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsByUsernameAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByUsernameAsync(executionContext, "exists_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByUsernameAsync_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para username inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsByUsernameAsync para username inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsByUsernameAsync(executionContext, "missing_user", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }
}
