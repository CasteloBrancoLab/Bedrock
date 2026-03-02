using System.Text;
using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Moq;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Models;
using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;
using ShopDemo.IntegrationTests.Auth.Application.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Application.UseCases;

[Collection("AuthApplication")]
[Feature("RegisterUserUseCase", "Registro de usuario com outbox transacional")]
public class RegisterUserUseCaseIntegrationTests : IntegrationTestBase
{
    private readonly AuthApplicationFixture _fixture;

    public RegisterUserUseCaseIntegrationTests(
        AuthApplicationFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RegisterUser_Success_ShouldPersistUserAndOutboxEntry()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-application"]);
        LogArrange("Registrando usuario e verificando persistencia no banco e outbox");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"register_outbox_{Guid.NewGuid():N}@example.com";
        var input = new RegisterUserInput(email, "SecurePass1!xxxx");

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var userRepo = _fixture.CreateUserRepository(unitOfWork);
        var authService = _fixture.CreateAuthenticationService(userRepo);
        var outboxRepo = _fixture.CreateAuthOutboxRepository(unitOfWork);
        var outboxWriter = _fixture.CreateAuthOutboxWriter(outboxRepo);
        var useCase = _fixture.CreateRegisterUserUseCase(unitOfWork, authService, outboxWriter);

        // Act
        LogAct("Executando RegisterUserUseCase");
        var result = await useCase.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verificando usuario e outbox entry persistidos");
        result.ShouldNotBeNull();
        result.Email.ShouldBe(email);

        var dbUser = await _fixture.GetUserDirectlyAsync(result.UserId, tenantCode);
        dbUser.ShouldNotBeNull();
        dbUser.Email.ShouldBe(email);

        var entries = await _fixture.GetOutboxEntriesDirectlyAsync(tenantCode);
        entries.Count.ShouldBe(1);
        entries[0].PayloadType.ShouldContain("UserRegisteredEvent");
        entries[0].Status.ShouldBe(OutboxEntryStatus.Pending);
        entries[0].ContentType.ShouldBe("application/json");
    }

    [Fact]
    public async Task RegisterUser_Success_OutboxPayload_ShouldContainUserData()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-application"]);
        LogArrange("Registrando usuario e verificando conteudo do payload no outbox");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"payload_check_{Guid.NewGuid():N}@example.com";
        var input = new RegisterUserInput(email, "PayloadCheck1!xx");

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var userRepo = _fixture.CreateUserRepository(unitOfWork);
        var authService = _fixture.CreateAuthenticationService(userRepo);
        var outboxRepo = _fixture.CreateAuthOutboxRepository(unitOfWork);
        var outboxWriter = _fixture.CreateAuthOutboxWriter(outboxRepo);
        var useCase = _fixture.CreateRegisterUserUseCase(unitOfWork, authService, outboxWriter);

        // Act
        LogAct("Executando RegisterUserUseCase e lendo payload do outbox");
        var result = await useCase.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verificando que payload JSON contem email e userId");
        result.ShouldNotBeNull();

        var entries = await _fixture.GetOutboxEntriesDirectlyAsync(tenantCode);
        entries.Count.ShouldBe(1);

        var payloadJson = Encoding.UTF8.GetString(entries[0].Payload);
        payloadJson.ShouldContain(email);
        payloadJson.ShouldContain(result.UserId.ToString());
    }

    [Fact]
    public async Task RegisterUser_WithInvalidPassword_ShouldNotPersistAnything()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-application"]);
        LogArrange("Tentando registrar usuario com senha invalida (curta)");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"short_pass_{Guid.NewGuid():N}@example.com";
        var input = new RegisterUserInput(email, "Ab1!");

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var userRepo = _fixture.CreateUserRepository(unitOfWork);
        var authService = _fixture.CreateAuthenticationService(userRepo);
        var outboxRepo = _fixture.CreateAuthOutboxRepository(unitOfWork);
        var outboxWriter = _fixture.CreateAuthOutboxWriter(outboxRepo);
        var useCase = _fixture.CreateRegisterUserUseCase(unitOfWork, authService, outboxWriter);

        // Act
        LogAct("Executando RegisterUserUseCase com senha invalida");
        var result = await useCase.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum usuario ou outbox entry foi persistido");
        result.ShouldBeNull();

        var userCount = await _fixture.CountUsersDirectlyAsync(tenantCode);
        userCount.ShouldBe(0);

        var entries = await _fixture.GetOutboxEntriesDirectlyAsync(tenantCode);
        entries.Count.ShouldBe(0);
    }

    [Fact]
    public async Task RegisterUser_DuplicateEmail_ShouldNotPersistSecondOutboxEntry()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-application"]);
        LogArrange("Registrando email duplicado e verificando que segundo outbox entry nao foi criado");
        var tenantCode = Guid.NewGuid();
        var email = $"duplicate_{Guid.NewGuid():N}@example.com";
        var password = "DuplicatePass1!x";

        // First registration
        var ctx1 = _fixture.CreateExecutionContext(tenantCode);
        await using var uow1 = _fixture.CreateAppUserUnitOfWork();
        var repo1 = _fixture.CreateUserRepository(uow1);
        var svc1 = _fixture.CreateAuthenticationService(repo1);
        var outboxRepo1 = _fixture.CreateAuthOutboxRepository(uow1);
        var writer1 = _fixture.CreateAuthOutboxWriter(outboxRepo1);
        var uc1 = _fixture.CreateRegisterUserUseCase(uow1, svc1, writer1);

        var result1 = await uc1.ExecuteAsync(ctx1, new RegisterUserInput(email, password), CancellationToken.None);
        result1.ShouldNotBeNull();

        // Act - Second registration (duplicate)
        LogAct("Tentando registrar mesmo email novamente");
        var ctx2 = _fixture.CreateExecutionContext(tenantCode);
        await using var uow2 = _fixture.CreateAppUserUnitOfWork();
        var repo2 = _fixture.CreateUserRepository(uow2);
        var svc2 = _fixture.CreateAuthenticationService(repo2);
        var outboxRepo2 = _fixture.CreateAuthOutboxRepository(uow2);
        var writer2 = _fixture.CreateAuthOutboxWriter(outboxRepo2);
        var uc2 = _fixture.CreateRegisterUserUseCase(uow2, svc2, writer2);

        var result2 = await uc2.ExecuteAsync(ctx2, new RegisterUserInput(email, password), CancellationToken.None);

        // Assert
        LogAssert("Verificando que segundo registro falhou e apenas 1 outbox entry existe");
        result2.ShouldBeNull();

        var entries = await _fixture.GetOutboxEntriesDirectlyAsync(tenantCode);
        entries.Count.ShouldBe(1);
    }

    [Fact]
    public async Task RegisterUser_OutboxFailure_ShouldRollbackUserPersistence()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-application"]);
        LogArrange("Simulando falha no outbox writer e verificando rollback do usuario");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"rollback_{Guid.NewGuid():N}@example.com";
        var input = new RegisterUserInput(email, "RollbackTest1!xx");

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        var userRepo = _fixture.CreateUserRepository(unitOfWork);
        var authService = _fixture.CreateAuthenticationService(userRepo);

        var failingWriter = new Mock<IAuthOutboxWriter>();
        failingWriter
            .Setup(w => w.EnqueueAsync(It.IsAny<MessageBase>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated outbox failure"));

        var useCase = _fixture.CreateRegisterUserUseCase(unitOfWork, authService, failingWriter.Object);

        // Act
        LogAct("Executando RegisterUserUseCase com outbox writer que falha");
        var result = await useCase.ExecuteAsync(executionContext, input, CancellationToken.None);

        // Assert
        LogAssert("Verificando que nenhum usuario foi persistido (rollback transacional)");
        result.ShouldBeNull();

        var userCount = await _fixture.CountUsersDirectlyAsync(tenantCode);
        userCount.ShouldBe(0);

        var entries = await _fixture.GetOutboxEntriesDirectlyAsync(tenantCode);
        entries.Count.ShouldBe(0);
    }
}
