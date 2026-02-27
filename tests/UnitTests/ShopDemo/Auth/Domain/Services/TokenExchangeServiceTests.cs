using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class TokenExchangeServiceTests : TestBase
{
    private readonly Mock<ITokenExchangeRepository> _tokenExchangeRepositoryMock;
    private readonly Mock<IDenyListService> _denyListServiceMock;
    private readonly TokenExchangeService _sut;

    public TokenExchangeServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _tokenExchangeRepositoryMock = new Mock<ITokenExchangeRepository>();
        _denyListServiceMock = new Mock<IDenyListService>();
        _sut = new TokenExchangeService(_tokenExchangeRepositoryMock.Object, _denyListServiceMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullTokenExchangeRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null token exchange repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new TokenExchangeService(null!, _denyListServiceMock.Object));
    }

    [Fact]
    public void Constructor_WithNullDenyListService_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null deny list service");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new TokenExchangeService(_tokenExchangeRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementITokenExchangeService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<ITokenExchangeService>();
    }

    #endregion

    #region ExchangeTokenAsync Tests

    [Fact]
    public async Task ExchangeTokenAsync_WhenImpersonationToken_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up exchange with impersonation token");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        // Act
        LogAct("Exchanging impersonation token");
        var result = await _sut.ExchangeTokenAsync(
            executionContext, userId, "subject-jti", "internal-services", true, CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned with error message");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ExchangeTokenAsync_WhenAudienceNotAllowed_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up exchange with disallowed audience");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        // Act
        LogAct("Exchanging token with invalid audience");
        var result = await _sut.ExchangeTokenAsync(
            executionContext, userId, "subject-jti", "unknown-audience", false, CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned with audience error");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ExchangeTokenAsync_WhenUserIsDenied_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up exchange with denied user");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _denyListServiceMock
            .Setup(x => x.IsUserRevokedAsync(executionContext, userId.Value.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Exchanging token for denied user");
        var result = await _sut.ExchangeTokenAsync(
            executionContext, userId, "subject-jti", "internal-services", false, CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned with user denied error");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ExchangeTokenAsync_WhenRegisterNewReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with SubjectTokenJti exceeding max length to trigger RegisterNew failure");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _denyListServiceMock
            .Setup(x => x.IsUserRevokedAsync(executionContext, userId.Value.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Exchanging token with SubjectTokenJti exceeding max length (36)");
        var result = await _sut.ExchangeTokenAsync(
            executionContext, userId, new string('x', 37), "internal-services", false, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned when TokenExchange.RegisterNew fails validation");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExchangeTokenAsync_WhenRegistrationFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up exchange where repository registration fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _denyListServiceMock
            .Setup(x => x.IsUserRevokedAsync(executionContext, userId.Value.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tokenExchangeRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<TokenExchange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Exchanging token when repository fails to register");
        var result = await _sut.ExchangeTokenAsync(
            executionContext, userId, "subject-jti", "internal-services", false, CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned when persistence fails");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExchangeTokenAsync_WhenAllChecksPass_ShouldReturnTokenExchange()
    {
        // Arrange
        LogArrange("Setting up successful token exchange");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _denyListServiceMock
            .Setup(x => x.IsUserRevokedAsync(executionContext, userId.Value.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tokenExchangeRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<TokenExchange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Exchanging token successfully");
        var result = await _sut.ExchangeTokenAsync(
            executionContext, userId, "subject-jti", "internal-services", false, CancellationToken.None);

        // Assert
        LogAssert("Verifying token exchange entity is returned");
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.SubjectTokenJti.ShouldBe("subject-jti");
        result.RequestedAudience.ShouldBe("internal-services");
    }

    [Fact]
    public async Task ExchangeTokenAsync_WithPublicApiAudience_ShouldSucceed()
    {
        // Arrange
        LogArrange("Setting up token exchange with public-api audience");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _denyListServiceMock
            .Setup(x => x.IsUserRevokedAsync(executionContext, userId.Value.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tokenExchangeRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<TokenExchange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Exchanging token with public-api audience");
        var result = await _sut.ExchangeTokenAsync(
            executionContext, userId, "subject-jti", "public-api", false, CancellationToken.None);

        // Assert
        LogAssert("Verifying public-api audience is accepted");
        result.ShouldNotBeNull();
        result.RequestedAudience.ShouldBe("public-api");
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    #endregion
}
