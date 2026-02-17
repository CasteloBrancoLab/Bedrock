using Bedrock.BuildingBlocks.Security.Passwords;
using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Core.Entities.Users.Enums;
using ShopDemo.IntegrationTests.Auth.Domain.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Domain.Services;

[Collection("AuthDomain")]
[Feature("AuthenticationService RegisterUser", "Registro de usuarios com Argon2 real e PostgreSQL")]
public class RegisterUserIntegrationTests : IntegrationTestBase
{
    private readonly AuthDomainFixture _fixture;

    public RegisterUserIntegrationTests(
        AuthDomainFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RegisterUserAsync_WithValidCredentials_ShouldPersistUserWithArgon2Hash()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando usuario com credenciais validas e verificando dados no banco");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"register_hash_{Guid.NewGuid():N}@example.com";
        var password = "MySecurePass1!";

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var repository = _fixture.CreateUserRepository(unitOfWork);
        var service = _fixture.CreateStandardAuthenticationService(repository);

        // Act
        LogAct("Chamando RegisterUserAsync com credenciais validas");
        var result = await service.RegisterUserAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi persistido com hash Argon2 de 49 bytes");
        result.ShouldNotBeNull();

        var dbUser = await _fixture.GetUserDirectlyAsync(result.EntityInfo.Id.Value, tenantCode);
        dbUser.ShouldNotBeNull();
        dbUser.PasswordHash.Length.ShouldBe(49);
        dbUser.PasswordHash[0].ShouldBe((byte)1); // pepper version = 1
        dbUser.Email.ShouldBe(email);
        dbUser.Status.ShouldBe((short)UserStatus.Active);
        dbUser.Username.ShouldBe(email.ToLowerInvariant());
    }

    [Fact]
    public async Task RegisterUserAsync_StoredHash_ShouldBeVerifiableWithSamePasswordHasher()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando usuario e verificando hash armazenado com PasswordHasher");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"register_verify_{Guid.NewGuid():N}@example.com";
        var password = "VerifyHash1!pass";

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var repository = _fixture.CreateUserRepository(unitOfWork);
        var service = _fixture.CreateStandardAuthenticationService(repository);

        // Act
        LogAct("Registrando usuario e lendo hash do banco");
        var result = await service.RegisterUserAsync(executionContext, email, password, CancellationToken.None);
        result.ShouldNotBeNull();

        var dbUser = await _fixture.GetUserDirectlyAsync(result.EntityInfo.Id.Value, tenantCode);
        dbUser.ShouldNotBeNull();

        // Assert
        LogAssert("Verificando hash com PasswordHasher.VerifyPassword retorna IsValid=true");
        var hasher = _fixture.CreatePasswordHasher(_fixture.StandardPepperConfig);
        var verifyContext = _fixture.CreateExecutionContext(tenantCode);
        var verification = hasher.VerifyPassword(verifyContext, password, dbUser.PasswordHash);
        verification.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task RegisterUserAsync_WithPasswordTooShort_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Tentando registrar usuario com senha muito curta (4 chars < 12 min)");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"short_pass_{Guid.NewGuid():N}@example.com";
        var password = "Ab1!";

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var repository = _fixture.CreateUserRepository(unitOfWork);
        var service = _fixture.CreateStandardAuthenticationService(repository);

        // Act
        LogAct("Chamando RegisterUserAsync com senha muito curta");
        var result = await service.RegisterUserAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna null para senha muito curta");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_WithPasswordNoUppercase_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Tentando registrar usuario com senha sem letra maiuscula");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"no_upper_{Guid.NewGuid():N}@example.com";
        var password = "nouppercase1!xx";

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var repository = _fixture.CreateUserRepository(unitOfWork);
        var service = _fixture.CreateStandardAuthenticationService(repository);

        // Act
        LogAct("Chamando RegisterUserAsync com senha sem maiuscula");
        var result = await service.RegisterUserAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna null para senha sem maiuscula");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_WithPasswordNoDigit_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Tentando registrar usuario com senha sem digito");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"no_digit_{Guid.NewGuid():N}@example.com";
        var password = "NoDigitHere!!xx";

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var repository = _fixture.CreateUserRepository(unitOfWork);
        var service = _fixture.CreateStandardAuthenticationService(repository);

        // Act
        LogAct("Chamando RegisterUserAsync com senha sem digito");
        var result = await service.RegisterUserAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna null para senha sem digito");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_WithPasswordNoSpecialChar_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Tentando registrar usuario com senha sem caractere especial");
        var tenantCode = Guid.NewGuid();
        var executionContext = _fixture.CreateExecutionContext(tenantCode);
        var email = $"no_special_{Guid.NewGuid():N}@example.com";
        var password = "NoSpecialChar1x";

        await using var unitOfWork = _fixture.CreateAppUserUnitOfWork();
        await unitOfWork.OpenConnectionAsync(executionContext, CancellationToken.None);
        var repository = _fixture.CreateUserRepository(unitOfWork);
        var service = _fixture.CreateStandardAuthenticationService(repository);

        // Act
        LogAct("Chamando RegisterUserAsync com senha sem caractere especial");
        var result = await service.RegisterUserAsync(executionContext, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que retorna null para senha sem caractere especial");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_TwoDifferentUsers_ShouldBothSucceed()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando dois usuarios diferentes no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        var email1 = $"user_a_{Guid.NewGuid():N}@example.com";
        var email2 = $"user_b_{Guid.NewGuid():N}@example.com";
        var password = "SharedPass1!xxxx";

        // Act - User 1
        LogAct("Registrando primeiro usuario");
        var ctx1 = _fixture.CreateExecutionContext(tenantCode);
        await using var uow1 = _fixture.CreateAppUserUnitOfWork();
        await uow1.OpenConnectionAsync(ctx1, CancellationToken.None);
        var repo1 = _fixture.CreateUserRepository(uow1);
        var svc1 = _fixture.CreateStandardAuthenticationService(repo1);
        var result1 = await svc1.RegisterUserAsync(ctx1, email1, password, CancellationToken.None);

        // Act - User 2
        LogAct("Registrando segundo usuario");
        var ctx2 = _fixture.CreateExecutionContext(tenantCode);
        await using var uow2 = _fixture.CreateAppUserUnitOfWork();
        await uow2.OpenConnectionAsync(ctx2, CancellationToken.None);
        var repo2 = _fixture.CreateUserRepository(uow2);
        var svc2 = _fixture.CreateStandardAuthenticationService(repo2);
        var result2 = await svc2.RegisterUserAsync(ctx2, email2, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que ambos os usuarios foram registrados com sucesso");
        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result1.EntityInfo.Id.Value.ShouldNotBe(result2.EntityInfo.Id.Value);
    }

    [Fact]
    public async Task RegisterUserAsync_DuplicateEmail_SameTenant_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Tentando registrar email duplicado no mesmo tenant");
        var tenantCode = Guid.NewGuid();
        var email = $"duplicate_{Guid.NewGuid():N}@example.com";
        var password = "DuplicatePass1!x";

        // Register first user
        var ctx1 = _fixture.CreateExecutionContext(tenantCode);
        await using var uow1 = _fixture.CreateAppUserUnitOfWork();
        await uow1.OpenConnectionAsync(ctx1, CancellationToken.None);
        var repo1 = _fixture.CreateUserRepository(uow1);
        var svc1 = _fixture.CreateStandardAuthenticationService(repo1);
        var result1 = await svc1.RegisterUserAsync(ctx1, email, password, CancellationToken.None);
        result1.ShouldNotBeNull();

        // Act - try to register same email
        LogAct("Tentando registrar mesmo email no mesmo tenant");
        var ctx2 = _fixture.CreateExecutionContext(tenantCode);
        await using var uow2 = _fixture.CreateAppUserUnitOfWork();
        await uow2.OpenConnectionAsync(ctx2, CancellationToken.None);
        var repo2 = _fixture.CreateUserRepository(uow2);
        var svc2 = _fixture.CreateStandardAuthenticationService(repo2);
        var result2 = await svc2.RegisterUserAsync(ctx2, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que segundo registro retorna null (unique constraint)");
        result2.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_SameEmail_DifferentTenants_ShouldBothSucceed()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando mesmo email em tenants diferentes (isolamento multi-tenant)");
        var tenantCode1 = Guid.NewGuid();
        var tenantCode2 = Guid.NewGuid();
        var email = $"multitenant_{Guid.NewGuid():N}@example.com";
        var password = "MultiTenant1!xxx";

        // Act - Tenant 1
        LogAct("Registrando usuario no primeiro tenant");
        var ctx1 = _fixture.CreateExecutionContext(tenantCode1);
        await using var uow1 = _fixture.CreateAppUserUnitOfWork();
        await uow1.OpenConnectionAsync(ctx1, CancellationToken.None);
        var repo1 = _fixture.CreateUserRepository(uow1);
        var svc1 = _fixture.CreateStandardAuthenticationService(repo1);
        var result1 = await svc1.RegisterUserAsync(ctx1, email, password, CancellationToken.None);

        // Act - Tenant 2
        LogAct("Registrando mesmo email no segundo tenant");
        var ctx2 = _fixture.CreateExecutionContext(tenantCode2);
        await using var uow2 = _fixture.CreateAppUserUnitOfWork();
        await uow2.OpenConnectionAsync(ctx2, CancellationToken.None);
        var repo2 = _fixture.CreateUserRepository(uow2);
        var svc2 = _fixture.CreateStandardAuthenticationService(repo2);
        var result2 = await svc2.RegisterUserAsync(ctx2, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que ambos foram registrados com sucesso em tenants diferentes");
        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result1.EntityInfo.Id.Value.ShouldNotBe(result2.EntityInfo.Id.Value);
    }
}
