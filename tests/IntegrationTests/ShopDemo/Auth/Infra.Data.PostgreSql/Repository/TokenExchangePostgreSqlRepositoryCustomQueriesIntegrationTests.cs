using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("TokenExchangePostgreSqlRepository Custom Queries", "Queries customizadas de dominio")]
public class TokenExchangePostgreSqlRepositoryCustomQueriesIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public TokenExchangePostgreSqlRepositoryCustomQueriesIntegrationTests(
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
        LogArrange("Inserindo 2 token exchanges para o mesmo userId");
        var tenantCode = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (int i = 0; i < 2; i++)
        {
            var dataModel = _fixture.CreateTestTokenExchangeDataModel(
                tenantCode: tenantCode,
                userId: userId);
            await _fixture.InsertTokenExchangeDirectlyAsync(dataModel);
        }

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByUserIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByUserIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(userId),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que 2 entidades TokenExchange foram retornadas");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldAllBe(te => te.UserId.Value == userId);
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
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

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
    public async Task GetByIssuedTokenJtiAsync_Should_ReturnEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo token exchange com issuedTokenJti conhecido e buscando por jti");
        var tenantCode = Guid.NewGuid();
        var knownIssuedTokenJti = "iss_known_jti_test";
        var dataModel = _fixture.CreateTestTokenExchangeDataModel(
            tenantCode: tenantCode,
            issuedTokenJti: knownIssuedTokenJti);
        await _fixture.InsertTokenExchangeDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIssuedTokenJtiAsync(
            executionContext,
            knownIssuedTokenJti,
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade TokenExchange foi retornada com jti correto");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(dataModel.Id);
        result.IssuedTokenJti.ShouldBe(knownIssuedTokenJti);
    }

    [Fact]
    public async Task GetByIssuedTokenJtiAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para issuedTokenJti inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIssuedTokenJtiAsync para jti inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIssuedTokenJtiAsync(
            executionContext,
            "iss_nonexistent_jti_value",
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }
}
