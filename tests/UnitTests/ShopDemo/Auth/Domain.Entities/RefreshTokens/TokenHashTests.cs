using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RefreshTokens;

public class TokenHashTests : TestBase
{
    public TokenHashTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region CreateNew Tests

    [Fact]
    public void CreateNew_WithValidBytes_ShouldPreserveValue()
    {
        // Arrange
        LogArrange("Creating byte array for token hash");
        byte[] hashBytes = [1, 2, 3, 4, 5];

        // Act
        LogAct("Creating TokenHash via CreateNew");
        var tokenHash = TokenHash.CreateNew(hashBytes);

        // Assert
        LogAssert("Verifying value is preserved");
        tokenHash.Value.Span.SequenceEqual(hashBytes).ShouldBeTrue();
    }

    [Fact]
    public void CreateNew_ShouldMakeDefensiveCopy()
    {
        // Arrange
        LogArrange("Creating mutable byte array");
        byte[] hashBytes = [1, 2, 3, 4, 5];

        // Act
        LogAct("Creating TokenHash and modifying original array");
        var tokenHash = TokenHash.CreateNew(hashBytes);
        hashBytes[0] = 99;

        // Assert
        LogAssert("Verifying TokenHash was not affected by mutation");
        tokenHash.Value.Span[0].ShouldBe((byte)1);
    }

    [Fact]
    public void CreateNew_WithEmptyArray_ShouldCreateEmptyHash()
    {
        // Arrange
        LogArrange("Creating empty byte array");
        byte[] hashBytes = [];

        // Act
        LogAct("Creating TokenHash from empty array");
        var tokenHash = TokenHash.CreateNew(hashBytes);

        // Assert
        LogAssert("Verifying hash is empty");
        tokenHash.IsEmpty.ShouldBeTrue();
        tokenHash.Length.ShouldBe(0);
    }

    #endregion

    #region IsEmpty Tests

    [Fact]
    public void IsEmpty_WithDefaultStruct_ShouldBeTrue()
    {
        // Arrange & Act
        LogAct("Creating default TokenHash");
        var tokenHash = default(TokenHash);

        // Assert
        LogAssert("Verifying default is empty");
        tokenHash.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void IsEmpty_WithNonEmptyHash_ShouldBeFalse()
    {
        // Arrange
        LogArrange("Creating non-empty hash");
        var tokenHash = TokenHash.CreateNew([1, 2, 3]);

        // Act
        LogAct("Checking IsEmpty");
        bool isEmpty = tokenHash.IsEmpty;

        // Assert
        LogAssert("Verifying not empty");
        isEmpty.ShouldBeFalse();
    }

    #endregion

    #region Length Tests

    [Fact]
    public void Length_ShouldReturnCorrectByteCount()
    {
        // Arrange
        LogArrange("Creating hash with 32 bytes (SHA-256)");
        byte[] hashBytes = new byte[32];
        var tokenHash = TokenHash.CreateNew(hashBytes);

        // Act
        LogAct("Getting length");
        int length = tokenHash.Length;

        // Assert
        LogAssert("Verifying length is 32");
        length.ShouldBe(32);
    }

    [Fact]
    public void Length_WithDefaultStruct_ShouldBeZero()
    {
        // Arrange & Act
        LogAct("Creating default TokenHash");
        var tokenHash = default(TokenHash);

        // Assert
        LogAssert("Verifying length is 0");
        tokenHash.Length.ShouldBe(0);
    }

    #endregion

    #region Equals Tests

    [Fact]
    public void Equals_WithSameBytes_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two hashes with same bytes");
        byte[] bytes = [1, 2, 3, 4, 5];
        var hash1 = TokenHash.CreateNew(bytes);
        var hash2 = TokenHash.CreateNew(bytes);

        // Act
        LogAct("Comparing hashes");
        bool result = hash1.Equals(hash2);

        // Assert
        LogAssert("Verifying equality");
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentBytes_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two hashes with different bytes");
        var hash1 = TokenHash.CreateNew([1, 2, 3]);
        var hash2 = TokenHash.CreateNew([4, 5, 6]);

        // Act
        LogAct("Comparing hashes");
        bool result = hash1.Equals(hash2);

        // Assert
        LogAssert("Verifying inequality");
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentLengths_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two hashes with different lengths");
        var hash1 = TokenHash.CreateNew([1, 2, 3]);
        var hash2 = TokenHash.CreateNew([1, 2, 3, 4]);

        // Act
        LogAct("Comparing hashes");
        bool result = hash1.Equals(hash2);

        // Assert
        LogAssert("Verifying inequality due to different lengths");
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithObjectOfSameType_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating hash and boxing to object");
        byte[] bytes = [10, 20, 30];
        var hash1 = TokenHash.CreateNew(bytes);
        object hash2 = TokenHash.CreateNew(bytes);

        // Act
        LogAct("Comparing via object Equals");
        bool result = hash1.Equals(hash2);

        // Assert
        LogAssert("Verifying equality via object");
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithObjectOfDifferentType_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating hash and a different type");
        var hash = TokenHash.CreateNew([1, 2, 3]);
        object other = "not a hash";

        // Act
        LogAct("Comparing with different type");
        bool result = hash.Equals(other);

        // Assert
        LogAssert("Verifying false for different type");
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating hash");
        var hash = TokenHash.CreateNew([1, 2, 3]);

        // Act
        LogAct("Comparing with null");
        bool result = hash.Equals(null);

        // Assert
        LogAssert("Verifying false for null");
        result.ShouldBeFalse();
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void EqualityOperator_WithSameBytes_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two equal hashes");
        byte[] bytes = [5, 10, 15];
        var hash1 = TokenHash.CreateNew(bytes);
        var hash2 = TokenHash.CreateNew(bytes);

        // Act
        LogAct("Using == operator");
        bool result = hash1 == hash2;

        // Assert
        LogAssert("Verifying == returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferentBytes_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two different hashes");
        var hash1 = TokenHash.CreateNew([1, 2, 3]);
        var hash2 = TokenHash.CreateNew([4, 5, 6]);

        // Act
        LogAct("Using != operator");
        bool result = hash1 != hash2;

        // Assert
        LogAssert("Verifying != returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSameBytes_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two equal hashes");
        byte[] bytes = [5, 10, 15];
        var hash1 = TokenHash.CreateNew(bytes);
        var hash2 = TokenHash.CreateNew(bytes);

        // Act
        LogAct("Using != operator");
        bool result = hash1 != hash2;

        // Assert
        LogAssert("Verifying != returns false for equal hashes");
        result.ShouldBeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithSameBytes_ShouldReturnSameHash()
    {
        // Arrange
        LogArrange("Creating two hashes with same bytes");
        byte[] bytes = [1, 2, 3, 4, 5];
        var hash1 = TokenHash.CreateNew(bytes);
        var hash2 = TokenHash.CreateNew(bytes);

        // Act
        LogAct("Getting hash codes");
        int hashCode1 = hash1.GetHashCode();
        int hashCode2 = hash2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal");
        hashCode1.ShouldBe(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentBytes_ShouldReturnDifferentHash()
    {
        // Arrange
        LogArrange("Creating two hashes with different bytes");
        var hash1 = TokenHash.CreateNew([1, 2, 3]);
        var hash2 = TokenHash.CreateNew([4, 5, 6]);

        // Act
        LogAct("Getting hash codes");
        int hashCode1 = hash1.GetHashCode();
        int hashCode2 = hash2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes differ (statistically)");
        hashCode1.ShouldNotBe(hashCode2);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnRedacted()
    {
        // Arrange
        LogArrange("Creating token hash");
        var tokenHash = TokenHash.CreateNew([1, 2, 3, 4, 5]);

        // Act
        LogAct("Calling ToString");
        string result = tokenHash.ToString();

        // Assert
        LogAssert("Verifying [REDACTED] is returned");
        result.ShouldBe("[REDACTED]");
    }

    [Fact]
    public void ToString_WithDefaultStruct_ShouldReturnRedacted()
    {
        // Arrange & Act
        LogAct("Calling ToString on default TokenHash");
        string result = default(TokenHash).ToString();

        // Assert
        LogAssert("Verifying [REDACTED] is returned for default");
        result.ShouldBe("[REDACTED]");
    }

    #endregion
}
