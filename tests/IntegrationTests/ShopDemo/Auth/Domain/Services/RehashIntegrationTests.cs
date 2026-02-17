using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using Microsoft.Extensions.Logging;
using ShopDemo.IntegrationTests.Auth.Domain.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Domain.Services;

[Collection("AuthDomain")]
[Feature("AuthenticationService Rehash", "Re-hashing de senha apos rotacao de pepper com Argon2 real")]
public class RehashIntegrationTests : IntegrationTestBase
{
    private readonly AuthDomainFixture _fixture;

    public RehashIntegrationTests(
        AuthDomainFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithOldPepper_ShouldReturnUserWithNewHash()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando com pepper v1 e verificando com servico que tem pepper v2 ativo");
        var tenantCode = Guid.NewGuid();
        var email = $"rehash_new_{Guid.NewGuid():N}@example.com";
        var password = "RehashPass1!xxxx";

        // Register with v1 pepper
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        await regUow.OpenConnectionAsync(regCtx, CancellationToken.None);
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regSvc = _fixture.CreateStandardAuthenticationService(regRepo);
        var registered = await regSvc.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        registered.ShouldNotBeNull();

        // Verify stored hash uses pepper v1
        var dbUserBefore = await _fixture.GetUserDirectlyAsync(registered.EntityInfo.Id.Value, tenantCode);
        dbUserBefore.ShouldNotBeNull();
        dbUserBefore.PasswordHash[0].ShouldBe((byte)1);

        // Act - verify with rotated pepper config (v2 active, v1 retained)
        LogAct("Verificando credenciais com servico que tem pepper v2 ativo");
        var verifyCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUow = _fixture.CreateAppUserUnitOfWork();
        await verifyUow.OpenConnectionAsync(verifyCtx, CancellationToken.None);
        var verifyRepo = _fixture.CreateUserRepository(verifyUow);
        var verifySvc = _fixture.CreateAuthenticationService(_fixture.RotatedPepperConfig, verifyRepo);
        var result = await verifySvc.VerifyCredentialsAsync(verifyCtx, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o usuario foi retornado com hash novo usando pepper v2");
        result.ShouldNotBeNull();
        result.PasswordHash.Value.Span[0].ShouldBe((byte)2);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_WithCurrentPepper_ShouldNotRehash()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Registrando com pepper v1 e verificando com servico que tem pepper v1 ativo (sem rehash)");
        var tenantCode = Guid.NewGuid();
        var email = $"rehash_no_{Guid.NewGuid():N}@example.com";
        var password = "NoRehashPass1!xx";

        // Register with v1 pepper
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        await regUow.OpenConnectionAsync(regCtx, CancellationToken.None);
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regSvc = _fixture.CreateStandardAuthenticationService(regRepo);
        var registered = await regSvc.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        registered.ShouldNotBeNull();

        // Act - verify with same pepper config (v1 active)
        LogAct("Verificando credenciais com servico que tem pepper v1 ativo");
        var verifyCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUow = _fixture.CreateAppUserUnitOfWork();
        await verifyUow.OpenConnectionAsync(verifyCtx, CancellationToken.None);
        var verifyRepo = _fixture.CreateUserRepository(verifyUow);
        var verifySvc = _fixture.CreateStandardAuthenticationService(verifyRepo);
        var result = await verifySvc.VerifyCredentialsAsync(verifyCtx, email, password, CancellationToken.None);

        // Assert
        LogAssert("Verificando que o hash permanece com pepper v1 (sem rehash)");
        result.ShouldNotBeNull();
        result.PasswordHash.Value.Span[0].ShouldBe((byte)1);
    }

    [Fact]
    public async Task VerifyCredentialsAsync_AfterRehash_FullLifecycle_ShouldSucceed()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-domain"]);
        LogArrange("Ciclo completo: registrar com v1, rehash para v2, persistir e verificar novamente");
        var tenantCode = Guid.NewGuid();
        var email = $"rehash_full_{Guid.NewGuid():N}@example.com";
        var password = "FullCyclePass1!x";

        // Step 1: Register with v1 pepper
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        await regUow.OpenConnectionAsync(regCtx, CancellationToken.None);
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regSvc = _fixture.CreateStandardAuthenticationService(regRepo);
        var registered = await regSvc.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        registered.ShouldNotBeNull();

        // Step 2: Verify with rotated pepper (triggers rehash in memory)
        LogAct("Verificando com pepper v2 (rehash em memoria) e persistindo via UpdateAsync");
        var verifyCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var verifyUow = _fixture.CreateAppUserUnitOfWork();
        await verifyUow.OpenConnectionAsync(verifyCtx, CancellationToken.None);
        var verifyPgRepo = _fixture.CreatePostgreSqlRepository(verifyUow);
        var verifyRepo = new ShopDemo.Auth.Infra.Data.Repositories.UserRepository(
            _fixture.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()
                .CreateLogger<ShopDemo.Auth.Infra.Data.Repositories.UserRepository>(),
            verifyPgRepo);
        var verifySvc = _fixture.CreateAuthenticationService(_fixture.RotatedPepperConfig, verifyRepo);
        var rehashed = await verifySvc.VerifyCredentialsAsync(verifyCtx, email, password, CancellationToken.None);
        rehashed.ShouldNotBeNull();
        rehashed.PasswordHash.Value.Span[0].ShouldBe((byte)2);

        // Step 3: Persist the rehashed user via UpdateAsync on the PostgreSqlRepository
        var updateResult = await verifyPgRepo.UpdateAsync(verifyCtx, rehashed, CancellationToken.None);
        updateResult.ShouldBeTrue();

        // Step 4: Verify again with rotated pepper - should work with the persisted v2 hash
        LogAssert("Verificando que a nova hash v2 foi persistida e funciona");
        var finalCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var finalUow = _fixture.CreateAppUserUnitOfWork();
        await finalUow.OpenConnectionAsync(finalCtx, CancellationToken.None);
        var finalRepo = _fixture.CreateUserRepository(finalUow);
        var finalSvc = _fixture.CreateAuthenticationService(_fixture.RotatedPepperConfig, finalRepo);
        var finalResult = await finalSvc.VerifyCredentialsAsync(finalCtx, email, password, CancellationToken.None);

        finalResult.ShouldNotBeNull();
        finalResult.Email.Value.ShouldBe(email);

        // Verify in DB that pepper version is now 2
        var dbUser = await _fixture.GetUserDirectlyAsync(finalResult.EntityInfo.Id.Value, tenantCode);
        dbUser.ShouldNotBeNull();
        dbUser.PasswordHash[0].ShouldBe((byte)2);
    }
}
