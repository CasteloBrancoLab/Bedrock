using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("RefreshTokenPostgreSqlRepository Custom Queries", "Queries customizadas de dominio")]
public class RefreshTokenPostgreSqlRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public RefreshTokenPostgreSqlRepositoryCustomQueriesIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnEntities_WhenExist()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 2 refresh tokens para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (int i = 0; i < 2; i++)
        {
            var dataModel = _fixture.CreateTestRefreshTokenDataModel(
                tenantCode: tenantCode,
                userId: userId);
            await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUserIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 2 entidades RefreshToken foram retornadas");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldAllBe(rt => rt.UserId.Value == userId);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_ReturnEmpty_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para userId inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUserIdAsync para userId inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que lista vazia e retornada");
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByTokenHashAsync_Should_ReturnEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo refresh token com tokenHash conhecido e buscando por hash");
        var tenantCode = Guid.NewGuid();
        var knownHash = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
        var dataModel = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            tokenHash: knownHash);
        await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByTokenHashAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByTokenHashAsync(
            executionContext,
            TokenHash.CreateNew(knownHash),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade RefreshToken foi retornada com hash correto");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(dataModel.Id);
        result.TokenHash.Value.ToArray().ShouldBe(knownHash);
    }

    [Fact]
    public async Task GetByTokenHashAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para tokenHash inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByTokenHashAsync para hash inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByTokenHashAsync(
            executionContext,
            TokenHash.CreateNew(new byte[] { 99, 98, 97, 96, 95 }),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveByFamilyIdAsync_Should_ReturnActiveEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 2 refresh tokens Active com mesmo familyId");
        var tenantCode = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        for (int i = 0; i < 2; i++)
        {
            var dataModel = _fixture.CreateTestRefreshTokenDataModel(
                tenantCode: tenantCode,
                familyId: familyId,
                status: (short)RefreshTokenStatus.Active);
            await _fixture.InsertRefreshTokenDirectlyAsync(dataModel);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByFamilyIdAsync(
            executionContext,
            TokenFamily.CreateFromExistingInfo(familyId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 2 entidades Active foram retornadas");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldAllBe(rt => rt.FamilyId.Value == familyId);
        result.ShouldAllBe(rt => rt.Status == RefreshTokenStatus.Active);
    }

    [Fact]
    public async Task GetActiveByFamilyIdAsync_Should_NotReturnRevokedEntities()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo 1 Active e 1 Revoked com mesmo familyId");
        var tenantCode = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        var activeToken = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            familyId: familyId,
            status: (short)RefreshTokenStatus.Active);
        await _fixture.InsertRefreshTokenDirectlyAsync(activeToken);

        var revokedToken = _fixture.CreateTestRefreshTokenDataModel(
            tenantCode: tenantCode,
            familyId: familyId,
            status: (short)RefreshTokenStatus.Revoked,
            revokedAt: DateTimeOffset.UtcNow);
        await _fixture.InsertRefreshTokenDirectlyAsync(revokedToken);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateRefreshTokenDataModelRepository(unitOfWork);
        var repository = _fixture.CreateRefreshTokenPostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetActiveByFamilyIdAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetActiveByFamilyIdAsync(
            executionContext,
            TokenFamily.CreateFromExistingInfo(familyId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que apenas o token Active foi retornado, excluindo o Revoked");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].EntityInfo.Id.Value.ShouldBe(activeToken.Id);
        result[0].Status.ShouldBe(RefreshTokenStatus.Active);
    }
}
