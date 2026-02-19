using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class PasswordResetTokenServiceTests : TestBase
{
    private readonly PasswordResetTokenService _sut;

    public PasswordResetTokenServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _sut = new PasswordResetTokenService();
    }

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIPasswordResetTokenService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IPasswordResetTokenService>();
    }

    #endregion

    #region GenerateToken Tests

    [Fact]
    public void GenerateToken_ShouldReturnNonEmptyString()
    {
        // Act
        LogAct("Generating token");
        var token = _sut.GenerateToken();

        // Assert
        LogAssert("Verifying token is not empty");
        token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ShouldReturnUrlSafeToken()
    {
        // Act
        LogAct("Generating token");
        var token = _sut.GenerateToken();

        // Assert
        LogAssert("Verifying token is URL-safe");
        token.ShouldNotContain("+");
        token.ShouldNotContain("/");
        token.ShouldNotContain("=");
    }

    [Fact]
    public void GenerateToken_ShouldReturnUniqueTokens()
    {
        // Act
        LogAct("Generating two tokens");
        var token1 = _sut.GenerateToken();
        var token2 = _sut.GenerateToken();

        // Assert
        LogAssert("Verifying tokens are different");
        token1.ShouldNotBe(token2);
    }

    #endregion

    #region HashToken Tests

    [Fact]
    public void HashToken_ShouldReturnSha256HexString()
    {
        // Arrange
        LogArrange("Creating a test token");
        var token = "test-token";

        // Act
        LogAct("Hashing token");
        var hash = _sut.HashToken(token);

        // Assert
        LogAssert("Verifying hash is 64-char hex string (SHA256)");
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.Length.ShouldBe(64);
    }

    [Fact]
    public void HashToken_ShouldBeDeterministic()
    {
        // Arrange
        LogArrange("Creating a test token");
        var token = "same-token";

        // Act
        LogAct("Hashing token twice");
        var hash1 = _sut.HashToken(token);
        var hash2 = _sut.HashToken(token);

        // Assert
        LogAssert("Verifying hashes are identical");
        hash1.ShouldBe(hash2);
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_WithCorrectHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Generating and hashing a token");
        var token = _sut.GenerateToken();
        var hash = _sut.HashToken(token);

        // Act
        LogAct("Validating token against its hash");
        var result = _sut.ValidateToken(token, hash);

        // Assert
        LogAssert("Verifying validation returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateToken_WithIncorrectHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Generating a token and using wrong hash");
        var token = _sut.GenerateToken();
        var wrongHash = _sut.HashToken("wrong-token");

        // Act
        LogAct("Validating token against wrong hash");
        var result = _sut.ValidateToken(token, wrongHash);

        // Assert
        LogAssert("Verifying validation returns false");
        result.ShouldBeFalse();
    }

    #endregion
}
