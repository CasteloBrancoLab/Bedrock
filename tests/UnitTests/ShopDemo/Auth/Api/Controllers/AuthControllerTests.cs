using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.ExecutionContexts.Interfaces;
using Bedrock.BuildingBlocks.Web.WebApi.Envelope.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopDemo.Auth.Api.Controllers.V1.Auth;
using ShopDemo.Auth.Api.Controllers.V1.Auth.Models;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Models;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Api.Controllers;

public class AuthControllerTests : TestBase
{
    private readonly Mock<IRegisterUserUseCase> _registerMock;
    private readonly Mock<IAuthenticateUserUseCase> _authenticateMock;
    private readonly ExecutionContextFactory _executionContextFactory;
    private readonly AuthController _sut;

    public AuthControllerTests(ITestOutputHelper output) : base(output)
    {
        _registerMock = new Mock<IRegisterUserUseCase>();
        _authenticateMock = new Mock<IAuthenticateUserUseCase>();
        _executionContextFactory = new ExecutionContextFactory(TimeProvider.System);
        _sut = new AuthController(_executionContextFactory, _registerMock.Object, _authenticateMock.Object);
        SetupHttpContext();
    }

    private void SetupHttpContext(
        string? correlationId = null,
        string? tenantId = null,
        string? userName = null,
        string businessOperationCode = "TEST_OPERATION")
    {
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers[ExecutionContextFactory.BusinessOperationCodeHeaderName] = businessOperationCode;

        if (correlationId is not null)
            httpContext.Request.Headers[ExecutionContextFactory.CorrelationIdHeaderName] = correlationId;

        if (tenantId is not null)
            httpContext.Request.Headers[ExecutionContextFactory.TenantIdHeaderName] = tenantId;

        if (userName is not null)
        {
            var identity = new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, userName)],
                "test");
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);
        }

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullExecutionContextFactory_ShouldThrow()
    {
        LogAct("Creating controller with null execution context factory");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthController(null!, _registerMock.Object, _authenticateMock.Object));
    }

    [Fact]
    public void Constructor_WithNullRegisterUseCase_ShouldThrow()
    {
        LogAct("Creating controller with null register use case");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthController(_executionContextFactory, null!, _authenticateMock.Object));
    }

    [Fact]
    public void Constructor_WithNullAuthenticateUseCase_ShouldThrow()
    {
        LogAct("Creating controller with null authenticate use case");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() =>
            new AuthController(_executionContextFactory, _registerMock.Object, null!));
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_WhenSucceeds_ShouldReturnCreated()
    {
        // Arrange
        LogArrange("Setting up successful registration");
        var payload = new RegisterPayload("test@example.com", "SecurePassword123!");
        var expectedOutput = new RegisterUserOutput(Guid.NewGuid(), "test@example.com");

        _registerMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<RegisterUserInput>(i => i.Email == payload.Email && i.Password == payload.Password),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOutput);

        // Act
        LogAct("Calling Register endpoint");
        var result = await _sut.Register(payload, CancellationToken.None);

        // Assert
        LogAssert("Verifying 201 result with ApiResponse containing RegisterResponse");
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(201);
        var apiResponse = objectResult.Value.ShouldBeOfType<ApiResponse<RegisterResponse>>();
        apiResponse.Data.ShouldNotBeNull();
        apiResponse.Data.UserId.ShouldBe(expectedOutput.UserId);
        apiResponse.Data.Email.ShouldBe(expectedOutput.Email);
    }

    [Fact]
    public async Task Register_WhenFails_ShouldReturnDomainError()
    {
        // Arrange
        LogArrange("Setting up failed registration");
        var payload = new RegisterPayload("test@example.com", "SecurePassword123!");

        _registerMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegisterUserOutput?)null);

        // Act
        LogAct("Calling Register endpoint");
        var result = await _sut.Register(payload, CancellationToken.None);

        // Assert
        LogAssert("Verifying 422 result with ApiResponse");
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(422);
        objectResult.Value.ShouldBeOfType<ApiResponse<RegisterResponse>>();
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WhenSucceeds_ShouldReturnOk()
    {
        // Arrange
        LogArrange("Setting up successful login");
        var payload = new LoginPayload("test@example.com", "SecurePassword123!");
        var expectedOutput = new AuthenticateUserOutput(Guid.NewGuid(), "test@example.com");

        _authenticateMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.Is<AuthenticateUserInput>(i => i.Email == payload.Email && i.Password == payload.Password),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOutput);

        // Act
        LogAct("Calling Login endpoint");
        var result = await _sut.Login(payload, CancellationToken.None);

        // Assert
        LogAssert("Verifying 200 result with ApiResponse containing LoginResponse");
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(200);
        var apiResponse = objectResult.Value.ShouldBeOfType<ApiResponse<LoginResponse>>();
        apiResponse.Data.ShouldNotBeNull();
        apiResponse.Data.UserId.ShouldBe(expectedOutput.UserId);
        apiResponse.Data.Email.ShouldBe(expectedOutput.Email);
    }

    [Fact]
    public async Task Login_WhenFails_ShouldReturnUnauthorized()
    {
        // Arrange
        LogArrange("Setting up failed login");
        var payload = new LoginPayload("test@example.com", "WrongPassword!");

        _authenticateMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<ExecutionContext>(),
                It.IsAny<AuthenticateUserInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthenticateUserOutput?)null);

        // Act
        LogAct("Calling Login endpoint");
        var result = await _sut.Login(payload, CancellationToken.None);

        // Assert
        LogAssert("Verifying 401 result with ApiResponse");
        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(401);
        objectResult.Value.ShouldBeOfType<ApiResponse<LoginResponse>>();
    }

    #endregion

    #region CreateExecutionContext Tests

    [Fact]
    public async Task Register_WithCorrelationIdHeader_ShouldUseProvidedValue()
    {
        // Arrange
        LogArrange("Setting up with X-Correlation-Id header");
        var correlationId = Guid.NewGuid();
        SetupHttpContext(correlationId: correlationId.ToString());

        _registerMock
            .Setup(x => x.ExecuteAsync(
                It.Is<ExecutionContext>(ctx => ctx.CorrelationId == correlationId),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterUserOutput(Guid.NewGuid(), "a@b.com"));

        // Act
        LogAct("Calling Register with correlation header");
        await _sut.Register(new RegisterPayload("a@b.com", "ValidPassword12!"), CancellationToken.None);

        // Assert
        LogAssert("Verifying use case received the correlation id from header");
        _registerMock.Verify(
            x => x.ExecuteAsync(
                It.Is<ExecutionContext>(ctx => ctx.CorrelationId == correlationId),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_WithTenantIdHeader_ShouldUseProvidedValue()
    {
        // Arrange
        LogArrange("Setting up with X-Tenant-Id header");
        var tenantId = Guid.NewGuid();
        SetupHttpContext(tenantId: tenantId.ToString());

        _registerMock
            .Setup(x => x.ExecuteAsync(
                It.Is<ExecutionContext>(ctx => ctx.TenantInfo.Code == tenantId),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterUserOutput(Guid.NewGuid(), "a@b.com"));

        // Act
        LogAct("Calling Register with tenant id header");
        await _sut.Register(new RegisterPayload("a@b.com", "ValidPassword12!"), CancellationToken.None);

        // Assert
        LogAssert("Verifying use case received the tenant id from header");
        _registerMock.Verify(
            x => x.ExecuteAsync(
                It.Is<ExecutionContext>(ctx => ctx.TenantInfo.Code == tenantId),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_WithAuthenticatedUser_ShouldUseUserName()
    {
        // Arrange
        LogArrange("Setting up with authenticated user");
        SetupHttpContext(userName: "john.doe");

        _registerMock
            .Setup(x => x.ExecuteAsync(
                It.Is<ExecutionContext>(ctx => ctx.ExecutionUser == "john.doe"),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterUserOutput(Guid.NewGuid(), "a@b.com"));

        // Act
        LogAct("Calling Register with authenticated user");
        await _sut.Register(new RegisterPayload("a@b.com", "ValidPassword12!"), CancellationToken.None);

        // Assert
        LogAssert("Verifying use case received the user name");
        _registerMock.Verify(
            x => x.ExecuteAsync(
                It.Is<ExecutionContext>(ctx => ctx.ExecutionUser == "john.doe"),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_WithNoAuthenticatedUser_ShouldUseAnonymous()
    {
        // Arrange
        LogArrange("Setting up without authenticated user");

        _registerMock
            .Setup(x => x.ExecuteAsync(
                It.Is<ExecutionContext>(ctx => ctx.ExecutionUser == "anonymous"),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterUserOutput(Guid.NewGuid(), "a@b.com"));

        // Act
        LogAct("Calling Register without authenticated user");
        await _sut.Register(new RegisterPayload("a@b.com", "ValidPassword12!"), CancellationToken.None);

        // Assert
        LogAssert("Verifying use case received 'anonymous'");
        _registerMock.Verify(
            x => x.ExecuteAsync(
                It.Is<ExecutionContext>(ctx => ctx.ExecutionUser == "anonymous"),
                It.IsAny<RegisterUserInput>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
