using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class SigningKeyManagerTests : TestBase
{
    private readonly Mock<ISigningKeyRepository> _signingKeyRepositoryMock;
    private readonly SigningKeyManager _sut;

    public SigningKeyManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _signingKeyRepositoryMock = new Mock<ISigningKeyRepository>();
        _sut = new SigningKeyManager(_signingKeyRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null signing key repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new SigningKeyManager(null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementISigningKeyManager()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<ISigningKeyManager>();
    }

    #endregion

    #region RotateKeyAsync Tests

    [Fact]
    public async Task RotateKeyAsync_WhenNoCurrentKey_ShouldCreateNewKeySuccessfully()
    {
        // Arrange
        LogArrange("Setting up repository with no active key and successful registration");
        var executionContext = CreateTestExecutionContext();

        _signingKeyRepositoryMock
            .Setup(x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        _signingKeyRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<SigningKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Rotating key when no current key exists");
        var result = await _sut.RotateKeyAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying new key was created");
        result.ShouldNotBeNull();
        result.Algorithm.ShouldBe("ES256");
        result.Status.ShouldBe(SigningKeyStatus.Active);
    }

    [Fact]
    public async Task RotateKeyAsync_WhenCurrentKeyExists_ShouldCreateNewKey()
    {
        // Arrange
        LogArrange("Setting up repository with existing active key");
        var executionContext = CreateTestExecutionContext();
        var currentKey = CreateTestSigningKey(SigningKeyStatus.Active);

        _signingKeyRepositoryMock
            .Setup(x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentKey);

        _signingKeyRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<SigningKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _signingKeyRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<SigningKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Rotating key when current key exists");
        var result = await _sut.RotateKeyAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying new key was created regardless of old key rotation result");
        result.ShouldNotBeNull();
        result.Algorithm.ShouldBe("ES256");
        _signingKeyRepositoryMock.Verify(
            x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RotateKeyAsync_WhenRegistrationFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository where registration fails");
        var executionContext = CreateTestExecutionContext();

        _signingKeyRepositoryMock
            .Setup(x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        _signingKeyRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<SigningKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Rotating key when registration fails");
        var result = await _sut.RotateKeyAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned when persistence fails");
        result.ShouldBeNull();
    }

    #endregion

    #region GetCurrentKeyAsync Tests

    [Fact]
    public async Task GetCurrentKeyAsync_ShouldDelegateToRepository()
    {
        // Arrange
        LogArrange("Setting up repository to return active key");
        var executionContext = CreateTestExecutionContext();
        var expectedKey = CreateTestSigningKey(SigningKeyStatus.Active);

        _signingKeyRepositoryMock
            .Setup(x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedKey);

        // Act
        LogAct("Getting current key");
        var result = await _sut.GetCurrentKeyAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying repository was called and key returned");
        result.ShouldBeSameAs(expectedKey);
    }

    [Fact]
    public async Task GetCurrentKeyAsync_WhenNoActiveKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository with no active key");
        var executionContext = CreateTestExecutionContext();

        _signingKeyRepositoryMock
            .Setup(x => x.GetActiveAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        // Act
        LogAct("Getting current key when none exists");
        var result = await _sut.GetCurrentKeyAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned");
        result.ShouldBeNull();
    }

    #endregion

    #region GetKeyByKidAsync Tests

    [Fact]
    public async Task GetKeyByKidAsync_ShouldDelegateToRepository()
    {
        // Arrange
        LogArrange("Setting up repository to return key by kid");
        var executionContext = CreateTestExecutionContext();
        var kid = Kid.CreateNew("test-kid");
        var expectedKey = CreateTestSigningKey(SigningKeyStatus.Active);

        _signingKeyRepositoryMock
            .Setup(x => x.GetByKidAsync(executionContext, kid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedKey);

        // Act
        LogAct("Getting key by kid");
        var result = await _sut.GetKeyByKidAsync(executionContext, kid, CancellationToken.None);

        // Assert
        LogAssert("Verifying repository was called and key returned");
        result.ShouldBeSameAs(expectedKey);
    }

    [Fact]
    public async Task GetKeyByKidAsync_WhenKeyNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository with no key for kid");
        var executionContext = CreateTestExecutionContext();
        var kid = Kid.CreateNew("unknown-kid");

        _signingKeyRepositoryMock
            .Setup(x => x.GetByKidAsync(executionContext, kid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SigningKey?)null);

        // Act
        LogAct("Getting key by unknown kid");
        var result = await _sut.GetKeyByKidAsync(executionContext, kid, CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned");
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

    private static SigningKey CreateTestSigningKey(SigningKeyStatus status)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow,
                createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(),
                createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null,
                lastChangedBy: null,
                lastChangedCorrelationId: null,
                lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return SigningKey.CreateFromExistingInfo(new CreateFromExistingInfoSigningKeyInput(
            entityInfo,
            Kid.CreateNew("test-kid"),
            "ES256",
            "test-public-key",
            "test-encrypted-private-key",
            status,
            status == SigningKeyStatus.Rotated ? DateTimeOffset.UtcNow : null,
            DateTimeOffset.UtcNow.AddDays(90)));
    }

    #endregion
}
