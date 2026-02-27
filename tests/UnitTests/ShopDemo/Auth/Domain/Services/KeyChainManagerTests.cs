using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class KeyChainManagerTests : TestBase
{
    private readonly Mock<IKeyChainRepository> _keyChainRepositoryMock;
    private readonly Mock<IKeyAgreementService> _keyAgreementServiceMock;
    private readonly KeyChainManager _sut;

    public KeyChainManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _keyChainRepositoryMock = new Mock<IKeyChainRepository>();
        _keyAgreementServiceMock = new Mock<IKeyAgreementService>();
        _sut = new KeyChainManager(_keyChainRepositoryMock.Object, _keyAgreementServiceMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullKeyChainRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating KeyChainManager with null repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new KeyChainManager(null!, _keyAgreementServiceMock.Object));
    }

    [Fact]
    public void Constructor_WithNullKeyAgreementService_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating KeyChainManager with null key agreement service");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new KeyChainManager(_keyChainRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIKeyChainManager()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IKeyChainManager>();
    }

    #endregion

    #region RotateKeyAsync Tests

    [Fact]
    public async Task RotateKeyAsync_WithNoExistingKeys_ShouldCreateNewKey()
    {
        // Arrange
        LogArrange("Setting up with no existing keys");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var clientPublicKey = "dGVzdC1rZXk="; // base64 placeholder

        _keyChainRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KeyChain>());

        _keyAgreementServiceMock
            .Setup(x => x.NegotiateKey(clientPublicKey))
            .Returns(new KeyAgreementResult("c2VydmVyLWtleQ==", new byte[32]));

        _keyChainRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<KeyChain>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Rotating key with no existing keys");
        var result = await _sut.RotateKeyAsync(executionContext, userId, clientPublicKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying new key was created");
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task RotateKeyAsync_WithExistingActiveKey_ShouldDeactivateOldAndCreateNew()
    {
        // Arrange
        LogArrange("Setting up with existing active key");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var clientPublicKey = "dGVzdC1rZXk=";

        var existingKey = KeyChain.RegisterNew(executionContext,
            new RegisterNewKeyChainInput(userId, KeyId.CreateNew("v1"),
                "c2VydmVyLWtleQ==", Convert.ToBase64String(new byte[32]),
                executionContext.Timestamp.AddDays(30)));

        _keyChainRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KeyChain> { existingKey! });

        _keyChainRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<KeyChain>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _keyAgreementServiceMock
            .Setup(x => x.NegotiateKey(clientPublicKey))
            .Returns(new KeyAgreementResult("bmV3LWtleQ==", new byte[32]));

        _keyChainRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<KeyChain>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Rotating key with existing active key");
        var result = await _sut.RotateKeyAsync(executionContext, userId, clientPublicKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying new key created and old key deactivated");
        result.ShouldNotBeNull();
        _keyChainRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<KeyChain>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RotateKeyAsync_WhenRegistrationFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up where registration fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var clientPublicKey = "dGVzdC1rZXk=";

        _keyChainRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KeyChain>());

        _keyAgreementServiceMock
            .Setup(x => x.NegotiateKey(clientPublicKey))
            .Returns(new KeyAgreementResult("c2VydmVyLWtleQ==", new byte[32]));

        _keyChainRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<KeyChain>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Rotating key when registration fails");
        var result = await _sut.RotateKeyAsync(executionContext, userId, clientPublicKey, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
    }

    #endregion

    #region ResolveKeyForDecryptionAsync Tests

    [Fact]
    public async Task ResolveKeyForDecryptionAsync_ShouldDelegateToRepository()
    {
        // Arrange
        LogArrange("Setting up repository to return null for key");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var keyId = KeyId.CreateNew("v1");

        _keyChainRepositoryMock
            .Setup(x => x.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((KeyChain?)null);

        // Act
        LogAct("Resolving key for decryption");
        var result = await _sut.ResolveKeyForDecryptionAsync(executionContext, userId, keyId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
        _keyChainRepositoryMock.Verify(
            x => x.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CleanupExpiredAsync Tests

    [Fact]
    public async Task CleanupExpiredAsync_ShouldDelegateToRepository()
    {
        // Arrange
        LogArrange("Setting up repository to return cleanup count");
        var executionContext = CreateTestExecutionContext();

        _keyChainRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, executionContext.Timestamp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        LogAct("Cleaning up expired keys");
        var result = await _sut.CleanupExpiredAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying count returned");
        result.ShouldBe(3);
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
