using Bedrock.BuildingBlocks.Testing.Attributes;
using Bedrock.BuildingBlocks.Testing.Integration;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.IntegrationTests.Auth.Application.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.IntegrationTests.Auth.Application.UseCases;

[Collection("AuthApplication")]
[Feature("AuthenticateUserUseCase", "Autenticacao com Argon2 real")]
public class AuthenticateUserUseCaseIntegrationTests : IntegrationTestBase
{
    private readonly AuthApplicationFixture _fixture;

    public AuthenticateUserUseCaseIntegrationTests(
        AuthApplicationFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AuthenticateUser_WithValidCredentials_ShouldReturnOutput()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-application"]);
        LogArrange("Registrando usuario e autenticando com credenciais validas");
        var tenantCode = Guid.NewGuid();
        var email = $"auth_valid_{Guid.NewGuid():N}@example.com";
        var password = "ValidAuth1!xxxxx";

        // Register user via domain service (bypassing use case to avoid outbox dependency)
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regService = _fixture.CreateAuthenticationService(regRepo);
        var user = await regService.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        user.ShouldNotBeNull();

        // Act
        LogAct("Autenticando via AuthenticateUserUseCase");
        var authCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var authUow = _fixture.CreateAppUserUnitOfWork();
        var authRepo = _fixture.CreateUserRepository(authUow);
        var authService = _fixture.CreateAuthenticationService(authRepo);
        var useCase = _fixture.CreateAuthenticateUserUseCase(authService);
        var input = new AuthenticateUserInput(email, password);

        var result = await useCase.ExecuteAsync(authCtx, input, CancellationToken.None);

        // Assert
        LogAssert("Verificando que autenticacao retornou dados corretos do usuario");
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(user.EntityInfo.Id.Value);
        result.Email.ShouldBe(email);
    }

    [Fact]
    public async Task AuthenticateUser_WithWrongPassword_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-application"]);
        LogArrange("Registrando usuario e tentando autenticar com senha errada");
        var tenantCode = Guid.NewGuid();
        var email = $"auth_wrong_{Guid.NewGuid():N}@example.com";
        var password = "CorrectPass1!xxx";

        // Register user
        var regCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var regUow = _fixture.CreateAppUserUnitOfWork();
        var regRepo = _fixture.CreateUserRepository(regUow);
        var regService = _fixture.CreateAuthenticationService(regRepo);
        var user = await regService.RegisterUserAsync(regCtx, email, password, CancellationToken.None);
        user.ShouldNotBeNull();

        // Act
        LogAct("Autenticando com senha incorreta");
        var authCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var authUow = _fixture.CreateAppUserUnitOfWork();
        var authRepo = _fixture.CreateUserRepository(authUow);
        var authService = _fixture.CreateAuthenticationService(authRepo);
        var useCase = _fixture.CreateAuthenticateUserUseCase(authService);
        var input = new AuthenticateUserInput(email, "WrongPassword1!x");

        var result = await useCase.ExecuteAsync(authCtx, input, CancellationToken.None);

        // Assert
        LogAssert("Verificando que autenticacao retornou null para senha incorreta");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AuthenticateUser_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        UseEnvironment(_fixture.Environments["auth-application"]);
        LogArrange("Tentando autenticar email inexistente");
        var tenantCode = Guid.NewGuid();
        var email = $"nonexistent_{Guid.NewGuid():N}@example.com";

        // Act
        LogAct("Autenticando com email que nao existe no banco");
        var authCtx = _fixture.CreateExecutionContext(tenantCode);
        await using var authUow = _fixture.CreateAppUserUnitOfWork();
        var authRepo = _fixture.CreateUserRepository(authUow);
        var authService = _fixture.CreateAuthenticationService(authRepo);
        var useCase = _fixture.CreateAuthenticateUserUseCase(authService);
        var input = new AuthenticateUserInput(email, "AnyPassword1!xxx");

        var result = await useCase.ExecuteAsync(authCtx, input, CancellationToken.None);

        // Assert
        LogAssert("Verificando que autenticacao retornou null para email inexistente");
        result.ShouldBeNull();
    }
}
