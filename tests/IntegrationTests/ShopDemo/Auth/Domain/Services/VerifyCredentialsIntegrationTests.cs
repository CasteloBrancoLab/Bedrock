using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.IntegrationTests.Auth.Domain.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Domain.Services;

[Collection("AuthDomain")]
[Feature("AuthenticationService VerifyCredentials", "Verificacao de credenciais com Argon2 real e PostgreSQL")]
public class VerifyCredentialsIntegrationTests : IntegrationTestBase
{
    private readonly AuthDomainFixture _fixture;

    public VerifyCredentialsIntegrationTests(
        AuthDomainFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithCorrectPassword_ShouldReturnUser()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando usuario e verificando com senha correta");
        var tenantCode = Guid.NewGuid();
        var email = $"verify_ok_{Guid.NewGuid():N}@example.com";
        var password = "CorrectPass1!xxx";

        // Register
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        await regUow.OpenConnectionAsync(regCtx, CancellationToken.None);
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regSvc = _fixture.CreateStandardAuthenticationService(regRepo);
        var registered = await regSvc.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        registered.ShouldNotBeNull();

        // Act
        LogAct("Verificando credenciais com senha correta em contexto separado");
        var verifyCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUow = _fixture.CreateAppUserUnitOfWork();
        await verifyUow.OpenConnectionAsync(verifyCtx, CancellationToken.None);
        var verifyRepo = _fixture.CreateUserRepository(verifyUow);
        var verifySvc = _fixture.CreateStandardAuthenticationService(verifyRepo);
        var result = await verifySvc.VerifyCredentialsAsync(verifyCtx, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi retornado");
        result.ShouldNotBeNull();
        result.Email.Value.ShouldBe(email);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithWrongPassword_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando usuario e verificando com senha errada");
        var tenantCode = Guid.NewGuid();
        var email = $"verify_wrong_{Guid.NewGuid():N}@example.com";
        var password = "CorrectPass1!xxx";

        // Register
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        await regUow.OpenConnectionAsync(regCtx, CancellationToken.None);
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regSvc = _fixture.CreateStandardAuthenticationService(regRepo);
        var registered = await regSvc.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        registered.ShouldNotBeNull();

        // Act
        LogAct("Verificando credenciais com senha errada");
        var verifyCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUow = _fixture.CreateAppUserUnitOfWork();
        await verifyUow.OpenConnectionAsync(verifyCtx, CancellationToken.None);
        var verifyRepo = _fixture.CreateUserRepository(verifyUow);
        var verifySvc = _fixture.CreateStandardAuthenticationService(verifyRepo);
        var result = await verifySvc.VerifyCredentialsAsync(verifyCtx, email, "WrongPassword1!x", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna null e tem mensagem de erro");
        result.ShouldBeNull();
        verifyCtx.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Verificando credenciais com email nunca registrado (anti-enumeracao)");
        var tenantCode = Guid.NewGuid();
        var email = $"ghost_{Guid.NewGuid():N}@example.com";

        // Act
        LogAct("Chamando VerifyCredentialsAsync para email inexistente");
        var verifyCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUow = _fixture.CreateAppUserUnitOfWork();
        await verifyUow.OpenConnectionAsync(verifyCtx, CancellationToken.None);
        var verifyRepo = _fixture.CreateUserRepository(verifyUow);
        var verifySvc = _fixture.CreateStandardAuthenticationService(verifyRepo);
        var result = await verifySvc.VerifyCredentialsAsync(verifyCtx, email, "AnyPassword1!xxx", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna null e tem mensagem de erro");
        result.ShouldBeNull();
        verifyCtx.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyCredentialsAsync_EndToEnd_RegisterThenVerifyInSeparateContexts()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando e verificando com ExecutionContexts completamente separados");
        var tenantCode = Guid.NewGuid();
        var email = $"e2e_{Guid.NewGuid():N}@example.com";
        var password = "E2EPassword1!xxx";

        // Register with context 1
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        await regUow.OpenConnectionAsync(regCtx, CancellationToken.None);
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regSvc = _fixture.CreateStandardAuthenticationService(regRepo);
        var registered = await regSvc.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        registered.ShouldNotBeNull();

        // Act - Verify with completely separate context, UoW, repo and service
        LogAct("Verificando com contexto, UnitOfWork, repositorio e servico separados");
        var verifyCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUow = _fixture.CreateAppUserUnitOfWork();
        await verifyUow.OpenConnectionAsync(verifyCtx, CancellationToken.None);
        var verifyRepo = _fixture.CreateUserRepository(verifyUow);
        var verifySvc = _fixture.CreateStandardAuthenticationService(verifyRepo);
        var result = await verifySvc.VerifyCredentialsAsync(verifyCtx, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi retornado corretamente");
        result.ShouldNotBeNull();
        result.Email.Value.ShouldBe(email);
        result.EntityInfo.Id.Value.ShouldBe(registered.EntityInfo.Id.Value);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_CaseSensitivePassword_ShouldFail()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando com senha case-sensitive e verificando com case diferente");
        var tenantCode = Guid.NewGuid();
        var email = $"case_sens_{Guid.NewGuid():N}@example.com";
        var password = "MyPassword1!xxxx";

        // Register
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        await regUow.OpenConnectionAsync(regCtx, CancellationToken.None);
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regSvc = _fixture.CreateStandardAuthenticationService(regRepo);
        var registered = await regSvc.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        registered.ShouldNotBeNull();

        // Act
        LogAct("Verificando com senha em uppercase");
        var verifyCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUow = _fixture.CreateAppUserUnitOfWork();
        await verifyUow.OpenConnectionAsync(verifyCtx, CancellationToken.None);
        var verifyRepo = _fixture.CreateUserRepository(verifyUow);
        var verifySvc = _fixture.CreateStandardAuthenticationService(verifyRepo);
        var result = await verifySvc.VerifyCredentialsAsync(verifyCtx, email, "MYPASSWORD1!XXXX", CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna null para senha com case diferente");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task VerifyCredentialsAsync_MultipleUsers_ShouldVerifyEachCorrectly()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando dois usuarios e verificando cada um com sua senha");
        var tenantCode = Guid.NewGuid();
        var emailA = $"multi_a_{Guid.NewGuid():N}@example.com";
        var emailB = $"multi_b_{Guid.NewGuid():N}@example.com";
        var passwordA = "PasswordUserA1!x";
        var passwordB = "PasswordUserB2@x";

        // Register User A
        var regCtxA = _fixture.CreateExecutionContext(tenantCode);
        await using var regUowA = _fixture.CreateAppUserUnitOfWork();
        await regUowA.OpenConnectionAsync(regCtxA, CancellationToken.None);
        var regRepoA = _fixture.CreateUserRepository(regUowA);
        var regSvcA = _fixture.CreateStandardAuthenticationService(regRepoA);
        var registeredA = await regSvcA.RegisterUserAsync(regCtxA, emailA, passwordA, CancellationToken.None);
        registeredA.ShouldNotBeNull();

        // Register User B
        var regCtxB = _fixture.CreateExecutionContext(tenantCode);
        await using var regUowB = _fixture.CreateAppUserUnitOfWork();
        await regUowB.OpenConnectionAsync(regCtxB, CancellationToken.None);
        var regRepoB = _fixture.CreateUserRepository(regUowB);
        var regSvcB = _fixture.CreateStandardAuthenticationService(regRepoB);
        var registeredB = await regSvcB.RegisterUserAsync(regCtxB, emailB, passwordB, CancellationToken.None);
        registeredB.ShouldNotBeNull();

        // Act - Verify User A
        LogAct("Verificando credenciais de cada usuario individualmente");
        var verifyCtxA = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUowA = _fixture.CreateAppUserUnitOfWork();
        await verifyUowA.OpenConnectionAsync(verifyCtxA, CancellationToken.None);
        var verifyRepoA = _fixture.CreateUserRepository(verifyUowA);
        var verifySvcA = _fixture.CreateStandardAuthenticationService(verifyRepoA);
        var resultA = await verifySvcA.VerifyCredentialsAsync(verifyCtxA, emailA, passwordA, CancellationToken.None);

        // Act - Verify User B
        var verifyCtxB = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUowB = _fixture.CreateAppUserUnitOfWork();
        await verifyUowB.OpenConnectionAsync(verifyCtxB, CancellationToken.None);
        var verifyRepoB = _fixture.CreateUserRepository(verifyUowB);
        var verifySvcB = _fixture.CreateStandardAuthenticationService(verifyRepoB);
        var resultB = await verifySvcB.VerifyCredentialsAsync(verifyCtxB, emailB, passwordB, CancellationToken.None);

        // Assert
        LogAssert("Verificando que cada usuario foi verificado corretamente");
        resultA.ShouldNotBeNull();
        resultA.Email.Value.ShouldBe(emailA);
        resultB.ShouldNotBeNull();
        resultB.Email.Value.ShouldBe(emailB);
    }
}
