using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.DataModelRepository;

[Collection("AuthPostgreSql")]
[Feature("UserDataModel CRUD", "Operacoes CRUD do repositorio de UserDataModel")]
public class UserDataModelRepositoryCrudIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public UserDataModelRepositoryCrudIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnUser_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario de teste e inserindo diretamente");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Abrindo conexao e chamando GetByIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, user.Id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi recuperado corretamente");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.TenantCode.ShouldBe(user.TenantCode);
        result.Username.ShouldBe(user.Username);
        result.Email.ShouldBe(user.Email);
        result.PasswordHash.ShouldBe(user.PasswordHash);
        result.Status.ShouldBe(user.Status);
        result.EntityVersion.ShouldBe(user.EntityVersion);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto com ID de usuario inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByIdAsync para usuario inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenUserExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo usuario de teste");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, user.Id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario existe");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_WhenUserNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto com ID inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsAsync para usuario inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario nao existe");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task InsertAsync_Should_PersistAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario de teste com todos os campos");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Inserindo usuario pelo repositorio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, user, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram persistidos");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var persisted = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(user.Id);
        persisted.TenantCode.ShouldBe(user.TenantCode);
        persisted.Username.ShouldBe(user.Username);
        persisted.Email.ShouldBe(user.Email);
        persisted.PasswordHash.ShouldBe(user.PasswordHash);
        persisted.Status.ShouldBe(user.Status);
        persisted.CreatedBy.ShouldBe(user.CreatedBy);
    }

    [Fact]
    public async Task InsertAsync_Should_HandleNullableFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario de teste com campos opcionais nulos");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Inserindo usuario com campos opcionais nulos");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, user, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que campos anulaveis sao armazenados como null");
        result.ShouldBeTrue();
        var persisted = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.LastChangedBy.ShouldBeNull();
        persisted.LastChangedAt.ShouldBeNull();
        persisted.LastChangedExecutionOrigin.ShouldBeNull();
        persisted.LastChangedCorrelationId.ShouldBeNull();
        persisted.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyFields_WhenVersionMatches()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo usuario de teste");
        var tenantCode = Guid.NewGuid();
        var originalVersion = 1L;
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: originalVersion);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        user.Username = "updated_username";
        user.Email = "updated@example.com";
        user.EntityVersion = originalVersion + 1;

        // Act
        LogAct("Atualizando usuario com versao correspondente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, user, expectedVersion: originalVersion, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi atualizado");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var updated = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
        updated.ShouldNotBeNull();
        updated.Username.ShouldBe("updated_username");
        updated.Email.ShouldBe("updated@example.com");
        updated.EntityVersion.ShouldBe(2);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo usuario de teste com versao 5");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: 5);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        user.Username = "should_not_update";
        user.EntityVersion = 6;

        // Act
        LogAct("Tentando atualizacao com versao obsoleta");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, user, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a atualizacao falhou por incompatibilidade de versao");
        result.ShouldBeFalse();

        var unchanged = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
        unchanged.ShouldNotBeNull();
        unchanged.Username.ShouldNotBe("should_not_update");
        unchanged.EntityVersion.ShouldBe(5);
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveUser_WhenVersionMatches()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo usuario de teste");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Excluindo usuario com versao correspondente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, user.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi excluido");
        result.ShouldBeTrue();
        var deleted = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo usuario com versao 3");
        var tenantCode = Guid.NewGuid();
        var user = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: 3);
        await _fixture.InsertUserDirectlyAsync(user);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando exclusao com versao obsoleta");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, user.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou");
        result.ShouldBeFalse();
        var stillExists = await _fixture.GetUserDirectlyAsync(user.Id, tenantCode);
        stillExists.ShouldNotBeNull();
    }
}
