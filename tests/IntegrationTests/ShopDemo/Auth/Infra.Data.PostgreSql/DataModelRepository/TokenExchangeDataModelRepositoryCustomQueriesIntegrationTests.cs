using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.DataModelRepository;

[Collection("AuthPostgreSql")]
[Feature("TokenExchangeDataModel Custom Queries", "Queries customizadas do repositorio de TokenExchangeDataModel")]
public class TokenExchangeDataModelRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public TokenExchangeDataModelRepositoryCustomQueriesIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnExchanges_WhenExist()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando 3 token exchanges para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var exchange1 = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode, userId: userId);
        var exchange2 = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode, userId: userId);
        var exchange3 = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode, userId: userId);

        await _fixture.InsertTokenExchangeDirectlyAsync(exchange1);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange2);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange3);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que 3 exchanges foram retornados");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldAllBe(t => t.UserId == userId);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnEmpty_WhenNoExchanges()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para userId sem token exchanges");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync para userId sem exchanges");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, Guid.NewGuid(), CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista vazia e retornada");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantA, userId: userId);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra exchanges de outro tenant");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_PopulateAllFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange com todos os campos preenchidos");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode, userId: userId);
        exchange.LastChangedBy = "modifier_user";
        exchange.LastChangedAt = DateTimeOffset.UtcNow;
        exchange.LastChangedExecutionOrigin = "TestOrigin";
        exchange.LastChangedCorrelationId = Guid.NewGuid();
        exchange.LastChangedBusinessOperationCode = "EXCHANGE_OP";
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByUserIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verificando que todos os campos foram populados");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        var returned = result[0];
        returned.Id.ShouldBe(exchange.Id);
        returned.TenantCode.ShouldBe(exchange.TenantCode);
        returned.UserId.ShouldBe(exchange.UserId);
        returned.SubjectTokenJti.ShouldBe(exchange.SubjectTokenJti);
        returned.RequestedAudience.ShouldBe(exchange.RequestedAudience);
        returned.IssuedTokenJti.ShouldBe(exchange.IssuedTokenJti);
        returned.IssuedAt.ShouldBe(exchange.IssuedAt);
        returned.ExpiresAt.ShouldBe(exchange.ExpiresAt);
        returned.CreatedBy.ShouldBe(exchange.CreatedBy);
        returned.EntityVersion.ShouldBe(exchange.EntityVersion);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIssuedTokenJtiAsync_Should_ReturnExchange_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange com issuedTokenJti especifico");
        var tenantCode = Guid.NewGuid();
        var issuedTokenJti = Guid.NewGuid().ToString();

        var exchange = _fixture.CreateTestTokenExchangeDataModel(
            tenantCode: tenantCode,
            issuedTokenJti: issuedTokenJti);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIssuedTokenJtiAsync(executionContext, issuedTokenJti, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o exchange foi encontrado pelo issuedTokenJti");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(exchange.Id);
        result.IssuedTokenJti.ShouldBe(issuedTokenJti);
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIssuedTokenJtiAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para issuedTokenJti inexistente");
        var tenantCode = Guid.NewGuid();
        var nonExistentJti = Guid.NewGuid().ToString();

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync para jti inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIssuedTokenJtiAsync(executionContext, nonExistentJti, CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
        executionContext.HasExceptions.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIssuedTokenJtiAsync_Should_FilterByTenant()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando token exchange em tenant A e buscando em tenant B");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var issuedTokenJti = Guid.NewGuid().ToString();

        var exchange = _fixture.CreateTestTokenExchangeDataModel(
            tenantCode: tenantA,
            issuedTokenJti: issuedTokenJti);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantB);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var repository = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync com tenant diferente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIssuedTokenJtiAsync(executionContext, issuedTokenJti, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nao encontra exchange de outro tenant");
        result.ShouldBeNull();
        executionContext.HasExceptions.ShouldBeFalse();
    }
}
