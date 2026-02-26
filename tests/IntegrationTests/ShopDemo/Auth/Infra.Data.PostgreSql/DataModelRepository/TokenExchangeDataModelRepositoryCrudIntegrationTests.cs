using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.DataModelRepository;

[Collection("AuthPostgreSql")]
[Feature("TokenExchangeDataModel CRUD", "Operacoes CRUD do repositorio de TokenExchangeDataModel")]
public class TokenExchangeDataModelRepositoryCrudIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public TokenExchangeDataModelRepositoryCrudIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnTokenExchange_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange de teste e inserindo diretamente");
        var tenantCode = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Abrindo conexao e chamando GetByIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, exchange.Id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o token exchange foi recuperado corretamente");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(exchange.Id);
        result.TenantCode.ShouldBe(exchange.TenantCode);
        result.UserId.ShouldBe(exchange.UserId);
        result.SubjectTokenJti.ShouldBe(exchange.SubjectTokenJti);
        result.RequestedAudience.ShouldBe(exchange.RequestedAudience);
        result.IssuedTokenJti.ShouldBe(exchange.IssuedTokenJti);
        result.IssuedAt.ShouldBe(exchange.IssuedAt);
        result.ExpiresAt.ShouldBe(exchange.ExpiresAt);
        result.EntityVersion.ShouldBe(exchange.EntityVersion);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto com ID de token exchange inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByIdAsync para token exchange inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo token exchange de teste");
        var tenantCode = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, exchange.Id, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o token exchange existe");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto com ID inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando ExistsAsync para token exchange inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que o token exchange nao existe");
        result.ShouldBeFalse();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task InsertAsync_Should_PersistAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange de teste com todos os campos");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var subjectTokenJti = Guid.NewGuid().ToString();
        var requestedAudience = "https://api.example.com";
        var issuedTokenJti = Guid.NewGuid().ToString();
        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var exchange = _fixture.CreateTestTokenExchangeDataModel(
            tenantCode: tenantCode,
            userId: userId,
            subjectTokenJti: subjectTokenJti,
            requestedAudience: requestedAudience,
            issuedTokenJti: issuedTokenJti,
            issuedAt: issuedAt,
            expiresAt: expiresAt);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Inserindo token exchange pelo repositorio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, exchange, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram persistidos");
        result.ShouldBeTrue();
        executionContext.HasExceptions.ShouldBeFalse();

        var persisted = await _fixture.GetTokenExchangeDirectlyAsync(exchange.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldBe(exchange.Id);
        persisted.TenantCode.ShouldBe(exchange.TenantCode);
        persisted.UserId.ShouldBe(userId);
        persisted.SubjectTokenJti.ShouldBe(subjectTokenJti);
        persisted.RequestedAudience.ShouldBe(requestedAudience);
        persisted.IssuedTokenJti.ShouldBe(issuedTokenJti);
        persisted.IssuedAt.ShouldBe(exchange.IssuedAt);
        persisted.ExpiresAt.ShouldBe(exchange.ExpiresAt);
        persisted.CreatedBy.ShouldBe(exchange.CreatedBy);
    }

    [Fact]
    public async Task InsertAsync_Should_HandleNullableBaseFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange de teste com campos base opcionais nulos");
        var tenantCode = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Inserindo token exchange com campos base opcionais nulos");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.InsertAsync(executionContext, exchange, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que campos anulaveis do DataModelBase sao armazenados como null");
        result.ShouldBeTrue();
        var persisted = await _fixture.GetTokenExchangeDirectlyAsync(exchange.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.LastChangedBy.ShouldBeNull();
        persisted.LastChangedAt.ShouldBeNull();
        persisted.LastChangedExecutionOrigin.ShouldBeNull();
        persisted.LastChangedCorrelationId.ShouldBeNull();
        persisted.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveTokenExchange_WhenVersionMatches()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo token exchange de teste");
        var tenantCode = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(
            tenantCode: tenantCode,
            entityVersion: 1);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Excluindo token exchange com versao correspondente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, exchange.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o token exchange foi excluido");
        result.ShouldBeTrue();
        var deleted = await _fixture.GetTokenExchangeDirectlyAsync(exchange.Id, tenantCode);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenVersionMismatch()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando e inserindo token exchange com versao 3");
        var tenantCode = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(
            tenantCode: tenantCode,
            entityVersion: 3);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando exclusao com versao obsoleta");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.DeleteAsync(executionContext, exchange.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou");
        result.ShouldBeFalse();
        var stillExists = await _fixture.GetTokenExchangeDirectlyAsync(exchange.Id, tenantCode);
        stillExists.ShouldNotBeNull();
    }
}
