using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("TokenExchangePostgreSqlRepository Enumeration", "EnumerateAllAsync e EnumerateModifiedSinceAsync")]
public class TokenExchangePostgreSqlRepositoryEnumerationIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public TokenExchangePostgreSqlRepositoryEnumerationIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_EnumerateAllTokenExchanges()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 3 token exchanges no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 3; i++)
        {
            var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
            await _fixture.InsertTokenExchangeDirectlyAsync(exchange);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        var enumerated = new List<TokenExchange>();

        // Act
        LogAct("Chamando EnumerateAllAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, tokenExchange, pagination, ct) =>
            {
                enumerated.Add(tokenExchange);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 3 token exchanges foram enumerados");
        result.ShouldBeTrue();
        enumerated.Count.ShouldBe(3);
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_StopOnFalse()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 5 token exchanges no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
            await _fixture.InsertTokenExchangeDirectlyAsync(exchange);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        var count = 0;

        // Act
        LogAct("Chamando EnumerateAllAsync com handler que para apos 2 itens");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, tokenExchange, pagination, ct) =>
            {
                count++;
                return Task.FromResult(count < 2);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a enumeracao parou apos 2 itens");
        result.ShouldBeTrue();
        count.ShouldBe(2);
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_ConvertToTokenExchangeEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo token exchange com dados conhecidos");
        var tenantCode = Guid.NewGuid();
        var knownUserId = Guid.NewGuid();
        var knownSubjectTokenJti = "subj_enum_convert";
        var knownRequestedAudience = "aud_enum_convert";
        var knownIssuedTokenJti = "iss_enum_convert";

        var dataModel = _fixture.CreateTestTokenExchangeDataModel(
            tenantCode: tenantCode,
            userId: knownUserId,
            subjectTokenJti: knownSubjectTokenJti,
            requestedAudience: knownRequestedAudience,
            issuedTokenJti: knownIssuedTokenJti);
        await _fixture.InsertTokenExchangeDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        TokenExchange? capturedExchange = null;

        // Act
        LogAct("Chamando EnumerateAllAsync e capturando entidade");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, tokenExchange, pagination, ct) =>
            {
                capturedExchange = tokenExchange;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade TokenExchange foi convertida corretamente");
        capturedExchange.ShouldNotBeNull();
        capturedExchange.UserId.Value.ShouldBe(knownUserId);
        capturedExchange.SubjectTokenJti.ShouldBe(knownSubjectTokenJti);
        capturedExchange.RequestedAudience.ShouldBe(knownRequestedAudience);
        capturedExchange.IssuedTokenJti.ShouldBe(knownIssuedTokenJti);
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_FilterByTimestamp()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo token exchange com last_changed_at definido");
        var tenantCode = Guid.NewGuid();
        var sinceTime = DateTimeOffset.UtcNow.AddMinutes(-5);

        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
        exchange.LastChangedBy = "modifier";
        exchange.LastChangedAt = DateTimeOffset.UtcNow;
        exchange.LastChangedExecutionOrigin = "TestOrigin";
        exchange.LastChangedCorrelationId = Guid.NewGuid();
        exchange.LastChangedBusinessOperationCode = "TEST_OP";
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        var enumerated = new List<TokenExchange>();

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateModifiedSinceAsync(
            executionContext,
            TimeProvider.System,
            sinceTime,
            (ctx, entity, timeProvider, since, ct) =>
            {
                enumerated.Add(entity);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que o token exchange modificado foi enumerado");
        result.ShouldBeTrue();
        enumerated.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_NotReturnUnmodifiedExchanges()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo token exchange sem last_changed_at e buscando modificados");
        var tenantCode = Guid.NewGuid();
        var exchange = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
        await _fixture.InsertTokenExchangeDirectlyAsync(exchange);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        var enumerated = new List<TokenExchange>();

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync com timestamp futuro");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateModifiedSinceAsync(
            executionContext,
            TimeProvider.System,
            DateTimeOffset.UtcNow.AddMinutes(10),
            (ctx, entity, timeProvider, since, ct) =>
            {
                enumerated.Add(entity);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum token exchange foi enumerado");
        result.ShouldBeTrue();
        enumerated.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_ConvertToTokenExchangeEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo token exchange modificado com dados conhecidos");
        var tenantCode = Guid.NewGuid();
        var knownUserId = Guid.NewGuid();
        var knownSubjectTokenJti = "subj_mod_convert";
        var knownRequestedAudience = "aud_mod_convert";
        var knownIssuedTokenJti = "iss_mod_convert";

        var dataModel = _fixture.CreateTestTokenExchangeDataModel(
            tenantCode: tenantCode,
            userId: knownUserId,
            subjectTokenJti: knownSubjectTokenJti,
            requestedAudience: knownRequestedAudience,
            issuedTokenJti: knownIssuedTokenJti);
        dataModel.LastChangedBy = "modifier";
        dataModel.LastChangedAt = DateTimeOffset.UtcNow;
        dataModel.LastChangedExecutionOrigin = "TestOrigin";
        dataModel.LastChangedCorrelationId = Guid.NewGuid();
        dataModel.LastChangedBusinessOperationCode = "TEST_OP";
        await _fixture.InsertTokenExchangeDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        TokenExchange? capturedExchange = null;

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateModifiedSinceAsync(
            executionContext,
            TimeProvider.System,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            (ctx, entity, timeProvider, since, ct) =>
            {
                capturedExchange = entity;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade TokenExchange foi convertida corretamente");
        capturedExchange.ShouldNotBeNull();
        capturedExchange.UserId.Value.ShouldBe(knownUserId);
        capturedExchange.SubjectTokenJti.ShouldBe(knownSubjectTokenJti);
        capturedExchange.RequestedAudience.ShouldBe(knownRequestedAudience);
        capturedExchange.IssuedTokenJti.ShouldBe(knownIssuedTokenJti);
    }
}
