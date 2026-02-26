using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class RecoveryCodeServiceTests : TestBase
{
    private readonly RecoveryCodeService _sut;

    public RecoveryCodeServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _sut = new RecoveryCodeService();
    }

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIRecoveryCodeService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IRecoveryCodeService>();
    }

    #endregion

    #region GenerateCodes Tests

    [Fact]
    public void GenerateCodes_WithDefaultCount_ShouldReturn10Codes()
    {
        // Act
        LogAct("Generating codes with default count");
        var codes = _sut.GenerateCodes();

        // Assert
        LogAssert("Verifying 10 codes returned");
        codes.Count.ShouldBe(10);
    }

    [Fact]
    public void GenerateCodes_WithCustomCount_ShouldReturnRequestedCount()
    {
        // Act
        LogAct("Generating 5 codes");
        var codes = _sut.GenerateCodes(5);

        // Assert
        LogAssert("Verifying 5 codes returned");
        codes.Count.ShouldBe(5);
    }

    [Fact]
    public void GenerateCodes_ShouldReturnCodesInXxxxXxxxFormat()
    {
        // Act
        LogAct("Generating codes");
        var codes = _sut.GenerateCodes(5);

        // Assert
        LogAssert("Verifying XXXX-XXXX format");
        foreach (var code in codes)
        {
            code.Length.ShouldBe(9);
            code[4].ShouldBe('-');
        }
    }

    [Fact]
    public void GenerateCodes_ShouldReturnUniqueCodes()
    {
        // Act
        LogAct("Generating codes");
        var codes = _sut.GenerateCodes(10);

        // Assert
        LogAssert("Verifying all codes are unique");
        codes.Distinct().Count().ShouldBe(codes.Count);
    }

    #endregion

    #region HashCode Tests

    [Fact]
    public void HashCode_ShouldReturnSha256HexString()
    {
        // Arrange
        LogArrange("Creating a test code");
        var code = "ABCD-1234";

        // Act
        LogAct("Hashing code");
        var hash = _sut.HashCode(code);

        // Assert
        LogAssert("Verifying hash is 64-char hex string");
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.Length.ShouldBe(64);
    }

    [Fact]
    public void HashCode_ShouldBeDeterministic()
    {
        // Arrange
        LogArrange("Creating a test code");
        var code = "ABCD-1234";

        // Act
        LogAct("Hashing code twice");
        var hash1 = _sut.HashCode(code);
        var hash2 = _sut.HashCode(code);

        // Assert
        LogAssert("Verifying hashes are identical");
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void HashCode_ShouldNormalizeDashes()
    {
        // Act
        LogAct("Hashing with and without dash");
        var hashWithDash = _sut.HashCode("ABCD-1234");
        var hashWithoutDash = _sut.HashCode("ABCD1234");

        // Assert
        LogAssert("Verifying normalized hashes match");
        hashWithDash.ShouldBe(hashWithoutDash);
    }

    #endregion

    #region ValidateCode Tests

    [Fact]
    public void ValidateCode_WithCorrectHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Generating and hashing a code");
        var codes = _sut.GenerateCodes(1);
        var code = codes[0];
        var hash = _sut.HashCode(code);

        // Act
        LogAct("Validating code against its hash");
        var result = _sut.ValidateCode(code, hash);

        // Assert
        LogAssert("Verifying validation returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateCode_WithIncorrectHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Generating a code and using wrong hash");
        var codes = _sut.GenerateCodes(1);
        var code = codes[0];
        var wrongHash = _sut.HashCode("ZZZZ-9999");

        // Act
        LogAct("Validating code against wrong hash");
        var result = _sut.ValidateCode(code, wrongHash);

        // Assert
        LogAssert("Verifying validation returns false");
        result.ShouldBeFalse();
    }

    #endregion
}
