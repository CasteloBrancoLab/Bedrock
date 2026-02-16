using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Concurrency;

[Collection("AuthPostgreSql")]
[Feature("User Optimistic Concurrency", "Concorrencia otimista no nivel de entidade")]
public class UserOptimisticConcurrencyIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public UserOptimisticConcurrencyIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpdateAsync_Should_Succeed_WhenVersionIsCorrect()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario e atualizando com versao correta");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);

        var user = await repository.GetByIdAsync(
            executionContext,
            Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);
        user.ShouldNotBeNull();

        var changed = user.ChangeUsername(executionContext, new ChangeUsernameInput("concurrency_ok"));
        changed.ShouldNotBeNull();

        // Act
        LogAct("Chamando UpdateAsync com versao correta");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, changed, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o update teve sucesso");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetUserDirectlyAsync(dataModel.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Username.ShouldBe("concurrency_ok");
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenDeletedBetweenReadAndUpdate()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario, lendo como entity, e excluindo por fora antes do update");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);

        var user = await repository.GetByIdAsync(
            executionContext,
            Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);
        user.ShouldNotBeNull();

        // Simulate deletion by another process
        await _fixture.DeleteUserDirectlyAsync(dataModel.Id, tenantCode);

        var changed = user.ChangeUsername(executionContext, new ChangeUsernameInput("should_not_persist"));
        changed.ShouldNotBeNull();

        // Act
        LogAct("Chamando UpdateAsync apos exclusao por outro processo");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, changed, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o update retornou false porque o registro nao existe mais");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task SequentialUpdates_Should_BothSucceed_BecauseDomainRepoReReads()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Simulando dois updates sequenciais - domain repo re-le do DB em cada UpdateAsync");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext1 = _fixture.CreateExecutionContext(tenantCode);
        var executionContext2 = _fixture.CreateExecutionContext(tenantCode);
        var id = Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(dataModel.Id);

        // First "session" reads user
        await using var unitOfWork1 = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo1 = _fixture.CreateDataModelRepository(unitOfWork1);
        var repository1 = _fixture.CreatePostgreSqlRepository(dataModelRepo1);
        await unitOfWork1.OpenConnectionAsync(executionContext1, CancellationToken.None);
        var user1 = await repository1.GetByIdAsync(executionContext1, id, CancellationToken.None);
        user1.ShouldNotBeNull();

        // Second "session" reads user
        await using var unitOfWork2 = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo2 = _fixture.CreateDataModelRepository(unitOfWork2);
        var repository2 = _fixture.CreatePostgreSqlRepository(dataModelRepo2);
        await unitOfWork2.OpenConnectionAsync(executionContext2, CancellationToken.None);
        var user2 = await repository2.GetByIdAsync(executionContext2, id, CancellationToken.None);
        user2.ShouldNotBeNull();

        // Both change username
        var changed1 = user1.ChangeUsername(executionContext1, new ChangeUsernameInput("first_name"));
        changed1.ShouldNotBeNull();
        var changed2 = user2.ChangeUsername(executionContext2, new ChangeUsernameInput("second_name"));
        changed2.ShouldNotBeNull();

        // Act - Both updates succeed because domain repo re-reads current version from DB
        LogAct("Primeiro update");
        await unitOfWork1.BeginTransactionAsync(executionContext1, CancellationToken.None);
        var result1 = await repository1.UpdateAsync(executionContext1, changed1, CancellationToken.None);
        await unitOfWork1.CommitAsync(executionContext1, CancellationToken.None);

        LogAct("Segundo update (tambem deve ter sucesso - domain repo re-le a versao atual do DB)");
        await unitOfWork2.BeginTransactionAsync(executionContext2, CancellationToken.None);
        var result2 = await repository2.UpdateAsync(executionContext2, changed2, CancellationToken.None);
        await unitOfWork2.CommitAsync(executionContext2, CancellationToken.None);

        // Assert - Domain repo re-reads from DB before each update,
        // so both succeed and last writer wins
        LogAssert("Verificando que ambos os updates tiveram sucesso (last writer wins)");
        result1.ShouldBeTrue();
        result2.ShouldBeTrue();

        var persisted = await _fixture.GetUserDirectlyAsync(dataModel.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Username.ShouldBe("second_name");
    }

    [Fact]
    public async Task DeleteAsync_Should_Fail_AfterConcurrentUpdate()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario, atualizando versao, e tentando excluir com versao antiga");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        // Simulate another process updating the entity
        await _fixture.UpdateEntityVersionDirectlyAsync(dataModel.Id, tenantCode, 3);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);

        // Act
        LogAct("Tentando excluir com versao desatualizada");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await dataModelRepo.DeleteAsync(executionContext, dataModel.Id, expectedVersion: 1, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a exclusao falhou por concorrencia");
        result.ShouldBeFalse();

        var stillExists = await _fixture.GetUserDirectlyAsync(dataModel.Id, tenantCode);
        stillExists.ShouldNotBeNull();
        stillExists.EntityVersion.ShouldBe(3);
    }

    [Fact]
    public async Task UpdateAsync_Should_GenerateNewTimestampVersion()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario com versao 1 e fazendo update");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode, entityVersion: 1);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);

        var user = await repository.GetByIdAsync(
            executionContext,
            Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);
        user.ShouldNotBeNull();

        var changed = user.ChangeUsername(executionContext, new ChangeUsernameInput("version_test"));
        changed.ShouldNotBeNull();

        // Act
        LogAct("Executando update e verificando que a versao mudou");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.UpdateAsync(executionContext, changed, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert - RegistryVersion gera versao baseada em timestamp, nao incremental
        LogAssert("Verificando que a versao mudou de 1 para um timestamp-based value");
        var persisted = await _fixture.GetUserDirectlyAsync(dataModel.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.EntityVersion.ShouldNotBe(1);
        persisted.EntityVersion.ShouldBeGreaterThan(0);
    }
}
