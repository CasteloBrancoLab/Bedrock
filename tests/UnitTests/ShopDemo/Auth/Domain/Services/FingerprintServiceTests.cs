using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Fingerprints;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class FingerprintServiceTests : TestBase
{
    private readonly FingerprintService _sut;

    public FingerprintServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _sut = new FingerprintService();
    }

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIFingerprintService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IFingerprintService>();
    }

    #endregion

    #region Generate Tests

    [Fact]
    public void Generate_ShouldReturnNonEmptyFingerprintAndHash()
    {
        // Act
        LogAct("Generating fingerprint");
        var (fingerprint, hash) = _sut.Generate();

        // Assert
        LogAssert("Verifying fingerprint and hash are not empty");
        fingerprint.Value.ShouldNotBeNullOrWhiteSpace();
        hash.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void Generate_ShouldReturnUniqueFingerprints()
    {
        // Act
        LogAct("Generating two fingerprints");
        var (fp1, _) = _sut.Generate();
        var (fp2, _) = _sut.Generate();

        // Assert
        LogAssert("Verifying fingerprints are different");
        fp1.Value.ShouldNotBe(fp2.Value);
    }

    #endregion

    #region Validate Tests

    [Fact]
    public void Validate_WithMatchingHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Generating a fingerprint and its hash");
        var (fingerprint, hash) = _sut.Generate();

        // Act
        LogAct("Validating fingerprint against its hash");
        var result = _sut.Validate(fingerprint, hash);

        // Assert
        LogAssert("Verifying validation returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithDifferentFingerprint_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Generating two different fingerprints");
        var (_, hash1) = _sut.Generate();
        var (fp2, _) = _sut.Generate();

        // Act
        LogAct("Validating different fingerprint against original hash");
        var result = _sut.Validate(fp2, hash1);

        // Assert
        LogAssert("Verifying validation returns false");
        result.ShouldBeFalse();
    }

    #endregion
}
