using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("RefreshTokenPostgreSqlRepository Enumeration", "EnumerateAllAsync e EnumerateModifiedSinceAsync")]
public class RefreshTokenPostgreSqlRepositoryEnumerationIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public RefreshTokenPostgreSqlRepositoryEnumerationIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_EnumerateAllRefreshTokens()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 3 refresh tokens no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 3; i++)
        {
            var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode);
            await _fixture.InsertRefreshTokenDirectlyAsync(token);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        var enumerated = new List<RefreshToken>();

        // Act
        LogAct("Chamando EnumerateAllAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, refreshToken, pagination, ct) =>
            {
                enumerated.Add(refreshToken);
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 3 refresh tokens foram enumerados");
        result.ShouldBeTrue();
        enumerated.Count.ShouldBe(3);
    }

    [Fact]
    public async Task EnumerateAllAsync_Should_StopOnFalse()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 5 refresh tokens no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
        {
            var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode);
            await _fixture.InsertRefreshTokenDirectlyAsync(token);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        var count = 0;

        // Act
        LogAct("Chamando EnumerateAllAsync com handler que para apos 2 itens");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, refreshToken, pagination, ct) =>
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
    public async Task EnumerateAllAsync_Should_ConvertToRefreshTokenEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh token com dados conhecidos");
        var tenantCode = Guid.NewGuid();
        var knownUserId = Guid.NewGuid();
        var knownFamilyId = Guid.NewGuid();
        var knownTokenHash = new byte[] { 11, 22, 33, 44, 55, 66, 77, 88 };

        var dataModel = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            userId: knownUserId,
            familyId: knownFamilyId,
            tokenHash: knownTokenHash);
        await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        RefreshToken? capturedToken = null;

        // Act
        LogAct("Chamando EnumerateAllAsync e capturando entidade");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateAllAsync(
            executionContext,
            PaginationInfo.All,
            (ctx, refreshToken, pagination, ct) =>
            {
                capturedToken = refreshToken;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade RefreshToken foi convertida corretamente");
        capturedToken.ShouldNotBeNull();
        capturedToken.UserId.Value.ShouldBe(knownUserId);
        capturedToken.FamilyId.Value.ShouldBe(knownFamilyId);
        capturedToken.TokenHash.Value.ToArray().ShouldBe(knownTokenHash);
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_FilterByTimestamp()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh token com last_changed_at definido");
        var tenantCode = Guid.NewGuid();
        var sinceTime = DateTimeOffset.UtcNow.AddMinutes(-5);

        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode);
        token.LastChangedBy = "modifier";
        token.LastChangedAt = DateTimeOffset.UtcNow;
        token.LastChangedExecutionOrigin = "TestOrigin";
        token.LastChangedCorrelationId = Guid.NewGuid();
        token.LastChangedBusinessOperationCode = "TEST_OP";
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        var enumerated = new List<RefreshToken>();

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
        LogAssert("Verificando que o refresh token modificado foi enumerado");
        result.ShouldBeTrue();
        enumerated.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_NotReturnUnmodifiedTokens()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh token sem last_changed_at e buscando modificados");
        var tenantCode = Guid.NewGuid();
        var token = _fixture.CreateTestRefreshTokenDataModel(tenantCode: tenantCode);
        await _fixture.InsertRefreshTokenDirectlyAsync(token);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        var enumerated = new List<RefreshToken>();

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
        LogAssert("Verificando que nenhum refresh token foi enumerado");
        result.ShouldBeTrue();
        enumerated.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnumerateModifiedSinceAsync_Should_ConvertToRefreshTokenEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh token modificado com dados conhecidos");
        var tenantCode = Guid.NewGuid();
        var knownUserId = Guid.NewGuid();
        var knownFamilyId = Guid.NewGuid();
        var knownTokenHash = new byte[] { 21, 22, 23, 24, 25, 26, 27, 28 };

        var dataModel = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            userId: knownUserId,
            familyId: knownFamilyId,
            tokenHash: knownTokenHash);
        dataModel.LastChangedBy = "modifier";
        dataModel.LastChangedAt = DateTimeOffset.UtcNow;
        dataModel.LastChangedExecutionOrigin = "TestOrigin";
        dataModel.LastChangedCorrelationId = Guid.NewGuid();
        dataModel.LastChangedBusinessOperationCode = "TEST_OP";
        await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        RefreshToken? capturedToken = null;

        // Act
        LogAct("Chamando EnumerateModifiedSinceAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await repository.EnumerateModifiedSinceAsync(
            executionContext,
            TimeProvider.System,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            (ctx, entity, timeProvider, since, ct) =>
            {
                capturedToken = entity;
                return Task.FromResult(true);
            },
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade RefreshToken foi convertida corretamente");
        capturedToken.ShouldNotBeNull();
        capturedToken.UserId.Value.ShouldBe(knownUserId);
        capturedToken.FamilyId.Value.ShouldBe(knownFamilyId);
        capturedToken.TokenHash.Value.ToArray().ShouldBe(knownTokenHash);
    }
}
