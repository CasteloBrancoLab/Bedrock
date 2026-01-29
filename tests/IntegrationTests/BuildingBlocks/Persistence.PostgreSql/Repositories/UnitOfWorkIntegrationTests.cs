using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Integration tests for PostgreSqlUnitOfWorkBase transaction management.
/// </summary>
[Collection("PostgresRepository")]
[Feature("Unit of Work", "Gerenciamento de transações via UnitOfWork")]
public class UnitOfWorkIntegrationTests : IntegrationTestBase
{
    private readonly PostgresRepositoryFixture _fixture;

    public UnitOfWorkIntegrationTests(
        PostgresRepositoryFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExecuteAsync_Should_CommitTransaction_OnSuccess()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork e Repository");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Executando handler que retorna true (sucesso)");
        var result = await unitOfWork.ExecuteAsync(
            executionContext,
            entity,
            async (ctx, ent, ct) =>
            {
                var insertResult = await repository.InsertAsync(ctx, ent, ct);
                return insertResult;
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a transação foi commitada e os dados persistidos");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var persistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        persistedEntity.ShouldNotBeNull();
        persistedEntity.Name.ShouldBe(entity.Name);
        LogInfo("Transaction committed successfully");
    }

    [Fact]
    public async Task ExecuteAsync_Should_RollbackTransaction_OnHandlerFailure()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork e Repository");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Executando handler que retorna false (falha)");
        var result = await unitOfWork.ExecuteAsync(
            executionContext,
            entity,
            async (ctx, ent, ct) =>
            {
                await repository.InsertAsync(ctx, ent, ct);
                return false; // Simulate failure
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a transação foi revertida e os dados não persistidos");
        result.ShouldBeFalse();

        var notPersistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        notPersistedEntity.ShouldBeNull();
        LogInfo("Transaction rolled back correctly on handler failure");
    }

    [Fact]
    public async Task ExecuteAsync_Should_RollbackTransaction_OnException()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork e Repository");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        // Act
        LogAct("Executando handler que lança exceção");
        var result = await unitOfWork.ExecuteAsync(
            executionContext,
            entity,
            async (ctx, ent, ct) =>
            {
                await repository.InsertAsync(ctx, ent, ct);
                throw new InvalidOperationException("Test exception");
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a transação foi revertida, exceção registrada e dados não persistidos");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeTrue();

        var notPersistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        notPersistedEntity.ShouldBeNull();
        LogInfo("Transaction rolled back correctly on exception");
    }

    [Fact]
    public async Task BeginTransactionAsync_Should_CreateTransaction()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();

        // Act
        LogAct("Abrindo conexão e iniciando transação");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a transação foi criada");
        result.ShouldBeTrue();
        unitOfWork.GetCurrentTransaction().ShouldNotBeNull();
        LogInfo("Transaction created successfully");
    }

    [Fact]
    public async Task BeginTransactionAsync_Should_BeIdempotent()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork com conexão aberta");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var firstTransaction = unitOfWork.GetCurrentTransaction();

        // Act
        LogAct("Chamando BeginTransactionAsync novamente");
        var result = await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a segunda chamada retorna true e mesma transação");
        result.ShouldBeTrue();
        unitOfWork.GetCurrentTransaction().ShouldBe(firstTransaction);
        LogInfo("BeginTransactionAsync is idempotent");
    }

    [Fact]
    public async Task CommitAsync_Should_PersistChanges()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork, Repository e inserindo entidade");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.InsertAsync(executionContext, entity, CancellationToken.None);

        // Act
        LogAct("Commitando transação");
        var result = await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que as alterações foram persistidas após commit");
        result.ShouldBeTrue();

        var persistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        persistedEntity.ShouldNotBeNull();
        LogInfo("Commit persisted changes successfully");
    }

    [Fact]
    public async Task RollbackAsync_Should_DiscardChanges()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork, Repository e inserindo entidade");
        var tenantCode = Guid.NewGuid();
        var entity = _fixture.CreateTestEntity(tenantCode: tenantCode);
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateRepository(unitOfWork);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.InsertAsync(executionContext, entity, CancellationToken.None);

        // Act
        LogAct("Revertendo transação");
        var result = await unitOfWork.RollbackAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que as alterações foram descartadas após rollback");
        result.ShouldBeTrue();

        var notPersistedEntity = await _fixture.GetTestEntityDirectlyAsync(entity.Id, tenantCode);
        notPersistedEntity.ShouldBeNull();
        LogInfo("Rollback discarded changes successfully");
    }

    [Fact]
    public async Task CreateNpgsqlCommand_Should_AttachConnectionAndTransaction()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork com conexão e transação");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Criando NpgsqlCommand");
        using var command = unitOfWork.CreateNpgsqlCommand("SELECT 1");

        // Assert
        LogAssert("Verificando que o comando possui conexão e transação associados");
        command.ShouldNotBeNull();
        command.Connection.ShouldBe(unitOfWork.GetCurrentConnection());
        command.Transaction.ShouldBe(unitOfWork.GetCurrentTransaction());
        LogInfo("Command created with correct connection and transaction");
    }

    [Fact]
    public async Task CloseConnectionAsync_Should_CloseAndDisposeTransaction()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork com conexão e transação abertas");
        var executionContext = _fixture.CreateExecutionContext();
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Fechando conexão");
        var result = await unitOfWork.CloseConnectionAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a conexão está fechada e a transação descartada");
        result.ShouldBeTrue();
        unitOfWork.GetCurrentTransaction().ShouldBeNull();
        unitOfWork.GetCurrentConnection().ShouldBeNull();
        LogInfo("Connection closed and transaction disposed correctly");
    }

    [Fact]
    public async Task DisposeAsync_Should_CleanupResources()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["repository"]);
        LogArrange("Configurando UnitOfWork com conexão aberta");
        var executionContext = _fixture.CreateExecutionContext();
        var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);

        // Act
        LogAct("Descartando UnitOfWork");
        await unitOfWork.DisposeAsync();

        // Assert
        LogAssert("Verificando que os recursos foram limpos");
        unitOfWork.GetCurrentTransaction().ShouldBeNull();
        unitOfWork.GetCurrentConnection().ShouldBeNull();
        LogInfo("DisposeAsync cleaned up resources correctly");
    }
}
