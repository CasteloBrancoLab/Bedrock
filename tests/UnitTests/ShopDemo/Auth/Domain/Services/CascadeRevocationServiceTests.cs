using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.Claims.Inputs;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims.Inputs;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;
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

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithActiveRefreshTokens_ShouldRevokeTokens()
    {
        // Arrange
        LogArrange("Setting up with active refresh tokens");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var refreshToken = CreateTestRefreshToken(executionContext, userId, RefreshTokenStatus.Active);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken });

        _refreshTokenRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient>());

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking all tokens for user with active refresh tokens");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying event with revoked count");
        result.ShouldNotBeNull();
        result.Value.RevokedRefreshTokenCount.ShouldBe(1);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithActiveServiceClientsAndApiKeys_ShouldRevokeCascade()
    {
        // Arrange
        LogArrange("Setting up with active service client and API key");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);
        var serviceClientId = serviceClient.EntityInfo.Id;
        var apiKey = CreateTestApiKey(executionContext, serviceClientId, ApiKeyStatus.Active);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<ServiceClient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _apiKeyRepositoryMock
            .Setup(x => x.GetByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApiKey> { apiKey });

        _apiKeyRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceClientClaimRepositoryMock
            .Setup(x => x.DeleteByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking all tokens including service clients and API keys");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying cascade revocation counts");
        result.ShouldNotBeNull();
        result.Value.RevokedServiceClientCount.ShouldBe(1);
        result.Value.RevokedApiKeyCount.ShouldBe(1);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WhenRefreshTokenUpdateFails_ShouldNotCount()
    {
        // Arrange
        LogArrange("Setting up with active refresh token where update fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var refreshToken = CreateTestRefreshToken(executionContext, userId, RefreshTokenStatus.Active);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken });

        _refreshTokenRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient>());

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens when refresh token update fails");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying refresh token not counted when update fails");
        result.ShouldNotBeNull();
        result.Value.RevokedRefreshTokenCount.ShouldBe(0);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WhenServiceClientUpdateFails_ShouldNotCascadeToApiKeys()
    {
        // Arrange
        LogArrange("Setting up with active service client where update fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<ServiceClient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens when service client update fails");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying service client not counted and API keys not touched");
        result.ShouldNotBeNull();
        result.Value.RevokedServiceClientCount.ShouldBe(0);
        _apiKeyRepositoryMock.Verify(
            x => x.GetByServiceClientIdAsync(executionContext, It.IsAny<Id>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WhenApiKeyUpdateFails_ShouldNotCountApiKey()
    {
        // Arrange
        LogArrange("Setting up with active service client and API key where API key update fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);
        var serviceClientId = serviceClient.EntityInfo.Id;
        var apiKey = CreateTestApiKey(executionContext, serviceClientId, ApiKeyStatus.Active);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<ServiceClient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _apiKeyRepositoryMock
            .Setup(x => x.GetByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApiKey> { apiKey });

        _apiKeyRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _serviceClientClaimRepositoryMock
            .Setup(x => x.DeleteByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens when API key update fails");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying service client counted but API key not counted");
        result.ShouldNotBeNull();
        result.Value.RevokedServiceClientCount.ShouldBe(1);
        result.Value.RevokedApiKeyCount.ShouldBe(0);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WhenRefreshTokenRevokeReturnsNull_ShouldSkip()
    {
        // Arrange
        LogArrange("Setting up with active refresh token from different tenant");
        var executionContext = CreateTestExecutionContext();
        var differentContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var refreshToken = CreateTestRefreshToken(differentContext, userId, RefreshTokenStatus.Active);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken });

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient>());

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens when refresh token Revoke returns null due to tenant mismatch");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying refresh token skipped (Revoke returned null)");
        result.ShouldNotBeNull();
        result.Value.RevokedRefreshTokenCount.ShouldBe(0);
        _refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithNonActiveServiceClient_ShouldSkipInRevocation()
    {
        // Arrange
        LogArrange("Setting up with revoked service client in the revocation path");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var revokedClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Revoked);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { revokedClient });

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens with non-Active service client");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying non-Active service client skipped");
        result.ShouldNotBeNull();
        result.Value.RevokedServiceClientCount.ShouldBe(0);
        _serviceClientRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<ServiceClient>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WhenServiceClientRevokeReturnsNull_ShouldSkip()
    {
        // Arrange
        LogArrange("Setting up with active service client from different tenant");
        var executionContext = CreateTestExecutionContext();
        var differentContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var serviceClient = CreateTestServiceClient(differentContext, userId, ServiceClientStatus.Active);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens when service client Revoke returns null due to tenant mismatch");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying service client skipped (Revoke returned null)");
        result.ShouldNotBeNull();
        result.Value.RevokedServiceClientCount.ShouldBe(0);
        _serviceClientRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<ServiceClient>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithNonActiveApiKey_ShouldSkipInRevocation()
    {
        // Arrange
        LogArrange("Setting up with active service client and revoked API key");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);
        var serviceClientId = serviceClient.EntityInfo.Id;
        var revokedApiKey = CreateTestApiKey(executionContext, serviceClientId, ApiKeyStatus.Revoked);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<ServiceClient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _apiKeyRepositoryMock
            .Setup(x => x.GetByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApiKey> { revokedApiKey });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.DeleteByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens with non-Active API key");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying non-Active API key skipped");
        result.ShouldNotBeNull();
        result.Value.RevokedServiceClientCount.ShouldBe(1);
        result.Value.RevokedApiKeyCount.ShouldBe(0);
        _apiKeyRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WhenApiKeyRevokeReturnsNull_ShouldSkip()
    {
        // Arrange
        LogArrange("Setting up with active service client and different-tenant API key");
        var executionContext = CreateTestExecutionContext();
        var differentContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);
        var serviceClientId = serviceClient.EntityInfo.Id;
        var differentTenantApiKey = CreateTestApiKey(differentContext, serviceClientId, ApiKeyStatus.Active);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<ServiceClient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _apiKeyRepositoryMock
            .Setup(x => x.GetByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApiKey> { differentTenantApiKey });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.DeleteByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens when API key Revoke returns null due to tenant mismatch");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying API key skipped (Revoke returned null)");
        result.ShouldNotBeNull();
        result.Value.RevokedServiceClientCount.ShouldBe(1);
        result.Value.RevokedApiKeyCount.ShouldBe(0);
        _apiKeyRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithRevokedRefreshToken_ShouldSkipAlreadyRevoked()
    {
        // Arrange
        LogArrange("Setting up with already-revoked refresh token");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var revokedToken = CreateTestRefreshToken(executionContext, userId, RefreshTokenStatus.Revoked);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { revokedToken });

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient>());

        _denyListServiceMock
            .Setup(x => x.RevokeUserAsync(executionContext, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking tokens when refresh token already revoked");
        var result = await _sut.RevokeAllUserTokensAsync(executionContext, userId, "test", CancellationToken.None);

        // Assert
        LogAssert("Verifying already-revoked tokens are skipped");
        result.ShouldNotBeNull();
        result.Value.RevokedRefreshTokenCount.ShouldBe(0);
        _refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
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

    [Fact]
    public async Task RecalculateApiTokenPermissionsAsync_WithChangedPermissions_ShouldReturnEvent()
    {
        // Arrange
        LogArrange("Setting up with service client that has claim exceeding user permission");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();

        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);
        var serviceClientId = serviceClient.EntityInfo.Id;

        var claim = CreateTestClaim(claimId, "admin_access");

        var serviceClientClaim = CreateTestServiceClientClaim(serviceClientId, claimId, ClaimValue.Granted);

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue> { ["admin_access"] = ClaimValue.Denied });

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claim });

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.GetByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClientClaim> { serviceClientClaim });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.DeleteByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceClientClaimRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<ServiceClientClaim>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Recalculating permissions with changed claims");
        var result = await _sut.RecalculateApiTokenPermissionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying event with changed claims");
        result.ShouldNotBeNull();
        result.ChangedClaims.Count.ShouldBe(1);
    }

    [Fact]
    public async Task RecalculateApiTokenPermissionsAsync_WithNoChanges_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with service client where permissions match");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();

        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);
        var serviceClientId = serviceClient.EntityInfo.Id;

        var claim = CreateTestClaim(claimId, "read_access");

        var serviceClientClaim = CreateTestServiceClientClaim(serviceClientId, claimId, ClaimValue.Granted);

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue> { ["read_access"] = ClaimValue.Granted });

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claim });

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.GetByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClientClaim> { serviceClientClaim });

        // Act
        LogAct("Recalculating permissions with no changes");
        var result = await _sut.RecalculateApiTokenPermissionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned (no changes)");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RecalculateApiTokenPermissionsAsync_WhenClaimNotInMap_ShouldDenyByDefault()
    {
        // Arrange
        LogArrange("Setting up with service client claim whose claimId is not in the claims map");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var unknownClaimId = Id.GenerateNewId();

        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);
        var serviceClientId = serviceClient.EntityInfo.Id;

        var serviceClientClaim = CreateTestServiceClientClaim(serviceClientId, unknownClaimId, ClaimValue.Granted);

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.GetByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClientClaim> { serviceClientClaim });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.DeleteByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceClientClaimRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<ServiceClientClaim>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Recalculating permissions when claim not in claims map");
        var result = await _sut.RecalculateApiTokenPermissionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying claim downgraded to Denied (claimId not found in map)");
        result.ShouldNotBeNull();
        result.ChangedClaims.Count.ShouldBe(1);
    }

    [Fact]
    public async Task RecalculateApiTokenPermissionsAsync_WhenUserLacksClaim_ShouldDenyByDefault()
    {
        // Arrange
        LogArrange("Setting up with claim in map but user lacks the claim");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();

        var serviceClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Active);
        var serviceClientId = serviceClient.EntityInfo.Id;

        var claim = CreateTestClaim(claimId, "admin_access");
        var serviceClientClaim = CreateTestServiceClientClaim(serviceClientId, claimId, ClaimValue.Granted);

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claim });

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { serviceClient });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.GetByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClientClaim> { serviceClientClaim });

        _serviceClientClaimRepositoryMock
            .Setup(x => x.DeleteByServiceClientIdAsync(executionContext, serviceClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceClientClaimRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<ServiceClientClaim>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Recalculating permissions when user lacks the claim");
        var result = await _sut.RecalculateApiTokenPermissionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying claim downgraded to Denied (user lacks claim)");
        result.ShouldNotBeNull();
        result.ChangedClaims.Count.ShouldBe(1);
    }

    [Fact]
    public async Task RecalculateApiTokenPermissionsAsync_WithRevokedServiceClient_ShouldSkip()
    {
        // Arrange
        LogArrange("Setting up with revoked service client");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var revokedClient = CreateTestServiceClient(executionContext, userId, ServiceClientStatus.Revoked);

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());

        _serviceClientRepositoryMock
            .Setup(x => x.GetByCreatorUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceClient> { revokedClient });

        // Act
        LogAct("Recalculating permissions with revoked service client");
        var result = await _sut.RecalculateApiTokenPermissionsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned (revoked client skipped)");
        result.ShouldBeNull();
        _serviceClientClaimRepositoryMock.Verify(
            x => x.GetByServiceClientIdAsync(executionContext, It.IsAny<Id>(), It.IsAny<CancellationToken>()),
            Times.Never);
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

    private static EntityInfo CreateTestEntityInfo()
    {
        return EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow, createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(), createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null, lastChangedBy: null,
                lastChangedCorrelationId: null, lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));
    }

    private static RefreshToken CreateTestRefreshToken(ExecutionContext executionContext, Id userId, RefreshTokenStatus status)
    {
        var token = RefreshToken.RegisterNew(executionContext,
            new ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs.RegisterNewRefreshTokenInput(
                userId,
                ShopDemo.Auth.Domain.Entities.RefreshTokens.TokenHash.CreateNew(new byte[32]),
                ShopDemo.Auth.Domain.Entities.RefreshTokens.TokenFamily.CreateNew(),
                DateTimeOffset.UtcNow.AddDays(7)));

        if (status == RefreshTokenStatus.Revoked && token is not null)
        {
            var revoked = token.Revoke(executionContext, new RevokeRefreshTokenInput());
            return revoked ?? token;
        }

        return token!;
    }

    private static ServiceClient CreateTestServiceClient(ExecutionContext executionContext, Id createdByUserId, ServiceClientStatus status)
    {
        var client = ServiceClient.RegisterNew(executionContext,
            new ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs.RegisterNewServiceClientInput(
                "test-client-" + Guid.NewGuid().ToString("N")[..8],
                new byte[32], "Test Client", createdByUserId,
                DateTimeOffset.UtcNow.AddDays(90)));

        if (status == ServiceClientStatus.Revoked && client is not null)
        {
            var revoked = client.Revoke(executionContext, new RevokeServiceClientInput());
            return revoked ?? client;
        }

        return client!;
    }

    private static ApiKey CreateTestApiKey(ExecutionContext executionContext, Id serviceClientId, ApiKeyStatus status)
    {
        var apiKey = ApiKey.RegisterNew(executionContext,
            new ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs.RegisterNewApiKeyInput(
                serviceClientId, "tk_test", "hash-value",
                DateTimeOffset.UtcNow.AddDays(90)));

        if (status == ApiKeyStatus.Revoked && apiKey is not null)
        {
            var revoked = apiKey.Revoke(executionContext, new RevokeApiKeyInput());
            return revoked ?? apiKey;
        }

        return apiKey!;
    }

    private static Claim CreateTestClaim(Id claimId, string name)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(claimId.Value),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow, createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(), createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null, lastChangedBy: null,
                lastChangedCorrelationId: null, lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return Claim.CreateFromExistingInfo(new CreateFromExistingInfoClaimInput(entityInfo, name, null));
    }

    private static ServiceClientClaim CreateTestServiceClientClaim(Id serviceClientId, Id claimId, ClaimValue value)
    {
        return ServiceClientClaim.CreateFromExistingInfo(new CreateFromExistingInfoServiceClientClaimInput(
            CreateTestEntityInfo(), serviceClientId, claimId, value));
    }

    #endregion
}
