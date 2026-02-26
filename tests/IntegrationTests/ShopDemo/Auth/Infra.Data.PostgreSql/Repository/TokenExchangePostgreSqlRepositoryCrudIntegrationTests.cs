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
[Feature("TokenExchangePostgreSqlRepository CRUD", "Round-trip completo Domain Entity <-> DataModel <-> SQL")]
public class TokenExchangePostgreSqlRepositoryCrudIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public TokenExchangePostgreSqlRepositoryCrudIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnTokenExchangeEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo token exchange via raw SQL e buscando como entity");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
        await _fixture.InsertTokenExchangeDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade TokenExchange foi construida corretamente");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(dataModel.Id);
        result.UserId.Value.ShouldBe(dataModel.UserId);
        result.SubjectTokenJti.ShouldBe(dataModel.SubjectTokenJti);
        result.RequestedAudience.ShouldBe(dataModel.RequestedAudience);
        result.IssuedTokenJti.ShouldBe(dataModel.IssuedTokenJti);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Configurando contexto para ID inexistente");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync para ID inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterNewAsync_Should_PersistTokenExchangeEntity()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando TokenExchange entity via RegisterNew");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var subjectTokenJti = "subj_persist_test";
        var requestedAudience = "aud_persist_test";
        var issuedTokenJti = "iss_persist_test";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var input = new RegisterNewTokenExchangeInput(userId, subjectTokenJti, requestedAudience, issuedTokenJti, expiresAt);
        var tokenExchange = TokenExchange.RegisterNew(executionContext, input);
        tokenExchange.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando RegisterNewAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.RegisterNewAsync(executionContext, tokenExchange, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade foi persistida via raw SQL");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetTokenExchangeDirectlyAsync(tokenExchange.EntityInfo.Id.Value, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.UserId.ShouldBe(userId.Value);
        persisted.SubjectTokenJti.ShouldBe(subjectTokenJti);
        persisted.RequestedAudience.ShouldBe(requestedAudience);
        persisted.IssuedTokenJti.ShouldBe(issuedTokenJti);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo token exchange e verificando existencia via repositorio de dominio");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestTokenExchangeDataModel(tenantCode: tenantCode);
        await _fixture.InsertTokenExchangeDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(
            executionContext,
            Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task RegisterNewAsync_Should_PersistAllStringFields()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando TokenExchange com strings especificas para round-trip");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var userId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var subjectTokenJti = "subj_abcdef123456";
        var requestedAudience = "https://api.example.com/v2";
        var issuedTokenJti = "iss_xyz789012345";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

        var input = new RegisterNewTokenExchangeInput(userId, subjectTokenJti, requestedAudience, issuedTokenJti, expiresAt);
        var tokenExchange = TokenExchange.RegisterNew(executionContext, input);
        tokenExchange.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateTokenExchangeDataModelRepository(unitOfWork);
        var repository = _fixture.CreateTokenExchangePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Persistindo e recuperando via raw SQL para verificar round-trip de strings");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.RegisterNewAsync(executionContext, tokenExchange, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que SubjectTokenJti, RequestedAudience e IssuedTokenJti foram persistidos corretamente");
        var persisted = await _fixture.GetTokenExchangeDirectlyAsync(tokenExchange.EntityInfo.Id.Value, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.SubjectTokenJti.ShouldBe(subjectTokenJti);
        persisted.RequestedAudience.ShouldBe(requestedAudience);
        persisted.IssuedTokenJti.ShouldBe(issuedTokenJti);
    }
}
