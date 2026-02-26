using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Validators;
using ShopDemo.Auth.Domain.Validators.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Validators;

public class DPoPProofValidatorTests : TestBase
{
    private readonly Mock<IDPoPKeyRepository> _dPoPKeyRepositoryMock;
    private readonly Mock<IDPoPProofVerifier> _proofVerifierMock;
    private readonly DPoPProofValidator _sut;

    public DPoPProofValidatorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _dPoPKeyRepositoryMock = new Mock<IDPoPKeyRepository>();
        _proofVerifierMock = new Mock<IDPoPProofVerifier>();
        _sut = new DPoPProofValidator(_dPoPKeyRepositoryMock.Object, _proofVerifierMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDPoPKeyRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null DPoP key repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new DPoPProofValidator(null!, _proofVerifierMock.Object));
    }

    [Fact]
    public void Constructor_WithNullProofVerifier_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null proof verifier");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new DPoPProofValidator(_dPoPKeyRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIDPoPProofValidator()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IDPoPProofValidator>();
    }

    #endregion

    #region ValidateProofAsync Tests

    [Fact]
    public async Task ValidateProofAsync_WhenProofIsInvalid_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up verifier to return null (invalid proof)");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _proofVerifierMock
            .Setup(x => x.ParseAndVerifyProof("invalid-jwt"))
            .Returns((DPoPProofInfo?)null);

        // Act
        LogAct("Validating invalid DPoP proof");
        var result = await _sut.ValidateProofAsync(executionContext, userId, "invalid-jwt", "GET", "https://api.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateProofAsync_WhenKeyNotRegistered_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up valid proof but unregistered key");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var thumbprint = JwkThumbprint.CreateNew("test-thumbprint");

        _proofVerifierMock
            .Setup(x => x.ParseAndVerifyProof("valid-jwt"))
            .Returns(new DPoPProofInfo(thumbprint, "jwk", "GET", "https://api.example.com", DateTimeOffset.UtcNow));

        _dPoPKeyRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndThumbprintAsync(executionContext, userId, thumbprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DPoPKey?)null);

        // Act
        LogAct("Validating proof with unregistered key");
        var result = await _sut.ValidateProofAsync(executionContext, userId, "valid-jwt", "GET", "https://api.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with unregistered key error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateProofAsync_WhenHttpMethodMismatch_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up valid proof and registered key but mismatched HTTP method");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var thumbprint = JwkThumbprint.CreateNew("test-thumbprint");

        _proofVerifierMock
            .Setup(x => x.ParseAndVerifyProof("valid-jwt"))
            .Returns(new DPoPProofInfo(thumbprint, "jwk", "POST", "https://api.example.com", DateTimeOffset.UtcNow));

        var dPoPKey = CreateTestDPoPKey(userId, thumbprint);
        _dPoPKeyRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndThumbprintAsync(executionContext, userId, thumbprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dPoPKey);

        // Act
        LogAct("Validating proof with mismatched HTTP method (POST vs GET)");
        var result = await _sut.ValidateProofAsync(executionContext, userId, "valid-jwt", "GET", "https://api.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with HTTP method mismatch error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateProofAsync_WhenHttpUriMismatch_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up valid proof and registered key but mismatched HTTP URI");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var thumbprint = JwkThumbprint.CreateNew("test-thumbprint");

        _proofVerifierMock
            .Setup(x => x.ParseAndVerifyProof("valid-jwt"))
            .Returns(new DPoPProofInfo(thumbprint, "jwk", "GET", "https://api.example.com/wrong", DateTimeOffset.UtcNow));

        var dPoPKey = CreateTestDPoPKey(userId, thumbprint);
        _dPoPKeyRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndThumbprintAsync(executionContext, userId, thumbprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dPoPKey);

        // Act
        LogAct("Validating proof with mismatched HTTP URI");
        var result = await _sut.ValidateProofAsync(executionContext, userId, "valid-jwt", "GET", "https://api.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with HTTP URI mismatch error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateProofAsync_WhenAllChecksPass_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up valid proof, registered key, matching method and URI");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var thumbprint = JwkThumbprint.CreateNew("test-thumbprint");

        _proofVerifierMock
            .Setup(x => x.ParseAndVerifyProof("valid-jwt"))
            .Returns(new DPoPProofInfo(thumbprint, "jwk", "GET", "https://api.example.com", DateTimeOffset.UtcNow));

        var dPoPKey = CreateTestDPoPKey(userId, thumbprint);
        _dPoPKeyRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndThumbprintAsync(executionContext, userId, thumbprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dPoPKey);

        // Act
        LogAct("Validating proof with all matching parameters");
        var result = await _sut.ValidateProofAsync(executionContext, userId, "valid-jwt", "GET", "https://api.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true (all checks passed)");
        result.ShouldBeTrue();
        executionContext.HasErrorMessages.ShouldBeFalse();
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

    private static DPoPKey CreateTestDPoPKey(Id userId, JwkThumbprint thumbprint)
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

        return DPoPKey.CreateFromExistingInfo(new CreateFromExistingInfoDPoPKeyInput(
            entityInfo,
            userId,
            thumbprint,
            "test-public-key-jwk",
            DateTimeOffset.UtcNow.AddHours(1),
            DPoPKeyStatus.Active,
            null));
    }

    #endregion
}
