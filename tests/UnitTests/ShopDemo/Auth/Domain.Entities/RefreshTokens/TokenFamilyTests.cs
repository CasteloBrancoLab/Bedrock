using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.RefreshTokens;

public class TokenFamilyTests : TestBase
{
    public TokenFamilyTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region CreateNew Tests

    [Fact]
    public void CreateNew_ShouldGenerateNonEmptyGuid()
    {
        // Arrange & Act
        LogAct("Creating new TokenFamily");
        var family = TokenFamily.CreateNew();

        // Assert
        LogAssert("Verifying Value is not empty");
        family.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void CreateNew_ShouldGenerateUniqueValues()
    {
        // Arrange & Act
        LogAct("Creating two TokenFamily instances");
        var family1 = TokenFamily.CreateNew();
        var family2 = TokenFamily.CreateNew();

        // Assert
        LogAssert("Verifying distinct values");
        family1.Value.ShouldNotBe(family2.Value);
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldPreserveGuid()
    {
        // Arrange
        LogArrange("Creating known Guid");
        var guid = Guid.NewGuid();

        // Act
        LogAct("Creating TokenFamily from existing Guid");
        var family = TokenFamily.CreateFromExistingInfo(guid);

        // Assert
        LogAssert("Verifying Guid is preserved");
        family.Value.ShouldBe(guid);
    }

    [Fact]
    public void CreateFromExistingInfo_WithEmptyGuid_ShouldPreserve()
    {
        // Arrange
        LogArrange("Using empty Guid");

        // Act
        LogAct("Creating TokenFamily from Guid.Empty");
        var family = TokenFamily.CreateFromExistingInfo(Guid.Empty);

        // Assert
        LogAssert("Verifying empty Guid is preserved");
        family.Value.ShouldBe(Guid.Empty);
    }

    #endregion

    #region Equals Tests

    [Fact]
    public void Equals_WithSameGuid_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two families with same Guid");
        var guid = Guid.NewGuid();
        var family1 = TokenFamily.CreateFromExistingInfo(guid);
        var family2 = TokenFamily.CreateFromExistingInfo(guid);

        // Act
        LogAct("Comparing families");
        bool result = family1.Equals(family2);

        // Assert
        LogAssert("Verifying equality");
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentGuid_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two families with different Guids");
        var family1 = TokenFamily.CreateNew();
        var family2 = TokenFamily.CreateNew();

        // Act
        LogAct("Comparing families");
        bool result = family1.Equals(family2);

        // Assert
        LogAssert("Verifying inequality");
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithObjectOfSameType_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating family and boxing to object");
        var guid = Guid.NewGuid();
        var family1 = TokenFamily.CreateFromExistingInfo(guid);
        object family2 = TokenFamily.CreateFromExistingInfo(guid);

        // Act
        LogAct("Comparing via object Equals");
        bool result = family1.Equals(family2);

        // Assert
        LogAssert("Verifying equality via object");
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithObjectOfDifferentType_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating family and a different type");
        var family = TokenFamily.CreateNew();
        object other = "not a family";

        // Act
        LogAct("Comparing with different type");
        bool result = family.Equals(other);

        // Assert
        LogAssert("Verifying false for different type");
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating family");
        var family = TokenFamily.CreateNew();

        // Act
        LogAct("Comparing with null");
        bool result = family.Equals(null);

        // Assert
        LogAssert("Verifying false for null");
        result.ShouldBeFalse();
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void EqualityOperator_WithSameGuid_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two equal families");
        var guid = Guid.NewGuid();
        var family1 = TokenFamily.CreateFromExistingInfo(guid);
        var family2 = TokenFamily.CreateFromExistingInfo(guid);

        // Act
        LogAct("Using == operator");
        bool result = family1 == family2;

        // Assert
        LogAssert("Verifying == returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferentGuid_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two different families");
        var family1 = TokenFamily.CreateNew();
        var family2 = TokenFamily.CreateNew();

        // Act
        LogAct("Using != operator");
        bool result = family1 != family2;

        // Assert
        LogAssert("Verifying != returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSameGuid_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two equal families");
        var guid = Guid.NewGuid();
        var family1 = TokenFamily.CreateFromExistingInfo(guid);
        var family2 = TokenFamily.CreateFromExistingInfo(guid);

        // Act
        LogAct("Using != operator");
        bool result = family1 != family2;

        // Assert
        LogAssert("Verifying != returns false for equal families");
        result.ShouldBeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithSameGuid_ShouldReturnSameHash()
    {
        // Arrange
        LogArrange("Creating two families with same Guid");
        var guid = Guid.NewGuid();
        var family1 = TokenFamily.CreateFromExistingInfo(guid);
        var family2 = TokenFamily.CreateFromExistingInfo(guid);

        // Act
        LogAct("Getting hash codes");
        int hashCode1 = family1.GetHashCode();
        int hashCode2 = family2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal");
        hashCode1.ShouldBe(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentGuid_ShouldReturnDifferentHash()
    {
        // Arrange
        LogArrange("Creating two families with different Guids");
        var family1 = TokenFamily.CreateNew();
        var family2 = TokenFamily.CreateNew();

        // Act
        LogAct("Getting hash codes");
        int hashCode1 = family1.GetHashCode();
        int hashCode2 = family2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes differ (statistically)");
        hashCode1.ShouldNotBe(hashCode2);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        LogArrange("Creating family with known Guid");
        var guid = Guid.NewGuid();
        var family = TokenFamily.CreateFromExistingInfo(guid);

        // Act
        LogAct("Calling ToString");
        string result = family.ToString();

        // Assert
        LogAssert("Verifying ToString returns Guid string");
        result.ShouldBe(guid.ToString());
    }

    [Fact]
    public void ToString_WithDefaultStruct_ShouldReturnEmptyGuidString()
    {
        // Arrange & Act
        LogAct("Calling ToString on default TokenFamily");
        string result = default(TokenFamily).ToString();

        // Assert
        LogAssert("Verifying ToString returns empty Guid string");
        result.ShouldBe(Guid.Empty.ToString());
    }

    #endregion
}
