using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Repository;

[Collection("AuthPostgreSql")]
[Feature("UserPostgreSqlRepository CRUD", "Round-trip completo Domain Entity <-> DataModel <-> SQL")]
public class UserPostgreSqlRepositoryCrudIntegrationTests : IntegrationTestBase
{
    private readonly AuthPostgreSqlFixture _fixture;

    public UserPostgreSqlRepositoryCrudIntegrationTests(
        AuthPostgreSqlFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnUserEntity_WhenExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario via raw SQL e buscando como entity");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync no repositorio de dominio");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade User foi construida corretamente");
        result.ShouldNotBeNull();
        result.EntityInfo.Id.Value.ShouldBe(dataModel.Id);
        result.Username.ShouldBe(dataModel.Username);
        result.Email.Value.ShouldBe(dataModel.Email);
        result.PasswordHash.Value.ToArray().ShouldBe(dataModel.PasswordHash);
        result.Status.ShouldBe((ShopDemo.Core.Entities.Users.Enums.UserStatus)dataModel.Status);
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
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando GetByIdAsync para ID inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.GetByIdAsync(
            executionContext,
            Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que null e retornado");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterNewAsync_Should_PersistUserEntity()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando User entity via RegisterNew");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var passwordHash = PasswordHash.CreateNew(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        var email = EmailAddress.CreateNew("newuser@example.com");
        var input = new RegisterNewInput(email, passwordHash);
        var user = User.RegisterNew(executionContext, input);
        user.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando RegisterNewAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.RegisterNewAsync(executionContext, user, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a entidade foi persistida via raw SQL");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetUserDirectlyAsync(user.EntityInfo.Id.Value, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Email.ShouldBe("newuser@example.com");
        persisted.Username.ShouldBe("newuser@example.com");
        persisted.PasswordHash.ShouldBe(passwordHash.Value.ToArray());
        persisted.Status.ShouldBe((short)ShopDemo.Core.Entities.Users.Enums.UserStatus.Active);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenUserExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario e verificando existencia via repositorio de dominio");
        var tenantCode = Guid.NewGuid();
        var dataModel = _fixture.CreateTestUserDataModel(tenantCode: tenantCode);
        await _fixture.InsertUserDirectlyAsync(dataModel);

        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando ExistsAsync");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var result = await repository.ExistsAsync(
            executionContext,
            Bedrock.BuildingBlocks.Core.Ids.Id.CreateFromExistingInfo(dataModel.Id),
            CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Inserindo usuario, recuperando como entity e atualizando");
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

        var changedUser = user.ChangeUsername(
            executionContext,
            new ShopDemo.Auth.Domain.Entities.Users.Inputs.ChangeUsernameInput("updated_name"));
        changedUser.ShouldNotBeNull();

        // Act
        LogAct("Chamando UpdateAsync");
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, changedUser, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que a atualizacao foi persistida");
        result.ShouldBeTrue();

        var persisted = await _fixture.GetUserDirectlyAsync(dataModel.Id, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.Username.ShouldBe("updated_name");
        persisted.EntityVersion.ShouldNotBe(1);
        persisted.EntityVersion.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnFalse_WhenUserNotExists()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando usuario sem persistir e tentando update");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var passwordHash = PasswordHash.CreateNew(new byte[] { 1, 2, 3, 4, 5 });
        var email = EmailAddress.CreateNew("ghost@example.com");
        var user = User.RegisterNew(executionContext, new RegisterNewInput(email, passwordHash));
        user.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Chamando UpdateAsync para usuario inexistente");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        var result = await repository.UpdateAsync(executionContext, user, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna false");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task RegisterNewAsync_Should_PersistPasswordHashAsBytea()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-repository"]);
        LogArrange("Criando User com password hash de 64 bytes");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);

        var hashBytes = new byte[64];
        for (int i = 0; i < hashBytes.Length; i++)
            hashBytes[i] = (byte)(i % 256);

        var passwordHash = PasswordHash.CreateNew(hashBytes);
        var email = EmailAddress.CreateNew("bytea_test@example.com");
        var user = User.RegisterNew(executionContext, new RegisterNewInput(email, passwordHash));
        user.ShouldNotBeNull();

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var dataModelRepo = _fixture.CreateDataModelRepository(unitOfWork);
        var repository = _fixture.CreatePostgreSqlRepository(dataModelRepo);

        // Act
        LogAct("Persistindo e recuperando via raw SQL");
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(executionContext, CancellationToken.None);
        await repository.RegisterNewAsync(executionContext, user, CancellationToken.None);
        await unitOfWork.CommitAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o password hash foi persistido corretamente como BYTEA");
        var persisted = await _fixture.GetUserDirectlyAsync(user.EntityInfo.Id.Value, tenantCode);
        persisted.ShouldNotBeNull();
        persisted.PasswordHash.ShouldBe(hashBytes);
    }
}
