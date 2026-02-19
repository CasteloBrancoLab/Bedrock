using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
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
