using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Resolvers.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class CascadeRevocationServiceTests : TestBase
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IServiceClientRepository> _serviceClientRepositoryMock;
    private readonly Mock<IApiKeyRepository> _apiKeyRepositoryMock;
    private readonly Mock<IServiceClientClaimRepository> _serviceClientClaimRepositoryMock;
    private readonly Mock<IDenyListService> _denyListServiceMock;
    private readonly Mock<IClaimResolver> _claimResolverMock;
    private readonly Mock<IClaimRepository> _claimRepositoryMock;
    private readonly CascadeRevocationService _sut;

    public CascadeRevocationServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _serviceClientRepositoryMock = new Mock<IServiceClientRepository>();
        _apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        _serviceClientClaimRepositoryMock = new Mock<IServiceClientClaimRepository>();
        _denyListServiceMock = new Mock<IDenyListService>();
        _claimResolverMock = new Mock<IClaimResolver>();
        _claimRepositoryMock = new Mock<IClaimRepository>();
        _sut = new CascadeRevocationService(
            _refreshTokenRepositoryMock.Object,
            _serviceClientRepositoryMock.Object,
            _apiKeyRepositoryMock.Object,
            _serviceClientClaimRepositoryMock.Object,
            _denyListServiceMock.Object,
            _claimResolverMock.Object,
            _claimRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRefreshTokenRepository_ShouldThrow()
    {
        LogAct("Creating with null refresh token repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new CascadeRevocationService(
            null!, _serviceClientRepositoryMock.Object, _apiKeyRepositoryMock.Object,
            _serviceClientClaimRepositoryMock.Object, _denyListServiceMock.Object,
            _claimResolverMock.Object, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullServiceClientRepository_ShouldThrow()
    {
        LogAct("Creating with null service client repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new CascadeRevocationService(
            _refreshTokenRepositoryMock.Object, null!, _apiKeyRepositoryMock.Object,
            _serviceClientClaimRepositoryMock.Object, _denyListServiceMock.Object,
            _claimResolverMock.Object, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullApiKeyRepository_ShouldThrow()
    {
        LogAct("Creating with null API key repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new CascadeRevocationService(
            _refreshTokenRepositoryMock.Object, _serviceClientRepositoryMock.Object, null!,
            _serviceClientClaimRepositoryMock.Object, _denyListServiceMock.Object,
            _claimResolverMock.Object, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullServiceClientClaimRepository_ShouldThrow()
    {
        LogAct("Creating with null service client claim repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new CascadeRevocationService(
            _refreshTokenRepositoryMock.Object, _serviceClientRepositoryMock.Object,
            _apiKeyRepositoryMock.Object, null!, _denyListServiceMock.Object,
            _claimResolverMock.Object, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullDenyListService_ShouldThrow()
    {
        LogAct("Creating with null deny list service");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new CascadeRevocationService(
            _refreshTokenRepositoryMock.Object, _serviceClientRepositoryMock.Object,
            _apiKeyRepositoryMock.Object, _serviceClientClaimRepositoryMock.Object,
            null!, _claimResolverMock.Object, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullClaimResolver_ShouldThrow()
    {
        LogAct("Creating with null claim resolver");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new CascadeRevocationService(
            _refreshTokenRepositoryMock.Object, _serviceClientRepositoryMock.Object,
            _apiKeyRepositoryMock.Object, _serviceClientClaimRepositoryMock.Object,
            _denyListServiceMock.Object, null!, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullClaimRepository_ShouldThrow()
    {
        LogAct("Creating with null claim repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new CascadeRevocationService(
            _refreshTokenRepositoryMock.Object, _serviceClientRepositoryMock.Object,
            _apiKeyRepositoryMock.Object, _serviceClientClaimRepositoryMock.Object,
            _denyListServiceMock.Object, _claimResolverMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementICascadeRevocationService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<ICascadeRevocationService>();
    }

    #endregion

    #region RevokeAllUserTokensAsync Tests

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithNoTokensOrClients_ShouldReturnEvent()
    {
        // Arrange
        LogArrange("Setting up with no tokens or clients");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient>());

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking all tokens for user with none");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test reason", CancellationToken.None);

        // Assert
        LogAssert("Verifying event returned with zero counts");
        result.ShouldNotBeNull();
        _denyListServiceMock.Verify(
            x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), "test reason", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RecalculateApiTokenPermissionsAsync Tests

    [Fact]
    public async Task RecalculateApiTokenPermissionsAsync_WithNoServiceClients_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with no service clients");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient>());

        // Act
        LogAct("Recalculating permissions with no service clients");
        var result = await _sut.RecalculateApiTokenPermissionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned (no changes)");
        result.ShouldBeNull();
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
