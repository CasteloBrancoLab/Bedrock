using Bedrock.BuildingBlocks.Core.Sortings;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using SortDirection = Bedrock.BuildingBlocks.Core.Sortings.Enums.SortDirection;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Sortings;

public class SortInfoTests : TestBase
{
    public SortInfoTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Create Tests

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up sort info components");
        var field = "LastName";
        var direction = SortDirection.Ascending;

        // Act
        LogAct("Creating SortInfo");
        var sort = SortInfo.Create(field, direction);

        // Assert
        LogAssert("Verifying all properties are set");
        sort.Field.ShouldBe(field);
        sort.Direction.ShouldBe(direction);
        LogInfo("SortInfo created: {0}", sort);
    }

    [Fact]
    public void Create_WithDescending_ShouldWork()
    {
        // Arrange
        LogArrange("Setting up descending sort");
        var field = "CreatedAt";
        var direction = SortDirection.Descending;

        // Act
        LogAct("Creating SortInfo with Descending");
        var sort = SortInfo.Create(field, direction);

        // Assert
        LogAssert("Verifying descending sort");
        sort.Field.ShouldBe("CreatedAt");
        sort.Direction.ShouldBe(SortDirection.Descending);
        LogInfo("Descending SortInfo: {0}", sort);
    }

    [Fact]
    public void Create_WithNullField_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing null field");

        // Act & Assert
        LogAct("Creating SortInfo with null field");
        var exception = Should.Throw<ArgumentException>(() =>
            SortInfo.Create(null!, SortDirection.Ascending));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("field");
        LogInfo("Exception thrown: {0}", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyField_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing empty field");

        // Act & Assert
        LogAct("Creating SortInfo with empty field");
        var exception = Should.Throw<ArgumentException>(() =>
            SortInfo.Create("", SortDirection.Ascending));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("field");
        LogInfo("Exception thrown: {0}", exception.Message);
    }

    [Fact]
    public void Create_WithWhitespaceField_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing whitespace field");

        // Act & Assert
        LogAct("Creating SortInfo with whitespace field");
        var exception = Should.Throw<ArgumentException>(() =>
            SortInfo.Create("   ", SortDirection.Ascending));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("field");
        LogInfo("Exception thrown: {0}", exception.Message);
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up existing info");
        var field = "Email";
        var direction = SortDirection.Descending;

        // Act
        LogAct("Creating from existing info");
        var sort = SortInfo.CreateFromExistingInfo(field, direction);

        // Assert
        LogAssert("Verifying properties");
        sort.Field.ShouldBe(field);
        sort.Direction.ShouldBe(direction);
        LogInfo("SortInfo from existing: {0}", sort);
    }

    [Fact]
    public void CreateFromExistingInfo_WithEmptyField_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing empty field for existing info");

        // Act
        LogAct("Creating from existing info with empty field");
        var sort = SortInfo.CreateFromExistingInfo("", SortDirection.Ascending);

        // Assert
        LogAssert("Verifying no exception and empty field");
        sort.Field.ShouldBe("");
        LogInfo("Empty field accepted in CreateFromExistingInfo");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two SortInfos with same values");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Name", SortDirection.Ascending);

        // Act
        LogAct("Comparing for equality");
        var areEqual = sort1.Equals(sort2);

        // Assert
        LogAssert("Verifying equality");
        areEqual.ShouldBeTrue();
        LogInfo("SortInfos are equal");
    }

    [Fact]
    public void Equals_WithDifferentField_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating SortInfos with different fields");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Email", SortDirection.Ascending);

        // Act
        LogAct("Comparing for equality");
        var areEqual = sort1.Equals(sort2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
        LogInfo("SortInfos with different fields are not equal");
    }

    [Fact]
    public void Equals_WithDifferentDirection_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating SortInfos with different directions");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Name", SortDirection.Descending);

        // Act
        LogAct("Comparing for equality");
        var areEqual = sort1.Equals(sort2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
        LogInfo("SortInfos with different directions are not equal");
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWork()
    {
        // Arrange
        LogArrange("Creating SortInfo and objects for equality test");
        var sort = SortInfo.Create("Name", SortDirection.Ascending);
        object objSame = SortInfo.Create("Name", SortDirection.Ascending);
        object objDifferent = SortInfo.Create("Email", SortDirection.Ascending);
        object? objNull = null;
        object objWrongType = "not a sort";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        sort.Equals(objSame).ShouldBeTrue();
        sort.Equals(objDifferent).ShouldBeFalse();
        sort.Equals(objNull).ShouldBeFalse();
        sort.Equals(objWrongType).ShouldBeFalse();
        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void EqualityOperator_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two SortInfos with same values");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Name", SortDirection.Ascending);

        // Act & Assert
        LogAct("Testing equality operator");
        (sort1 == sort2).ShouldBeTrue();
        LogAssert("Equality operator works correctly");
    }

    [Fact]
    public void EqualityOperator_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two different SortInfos");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Email", SortDirection.Descending);

        // Act & Assert
        LogAct("Testing equality operator");
        (sort1 == sort2).ShouldBeFalse();
        LogAssert("Equality operator correctly returns false");
    }

    [Fact]
    public void InequalityOperator_WithSameValues_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two SortInfos with same values");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Name", SortDirection.Ascending);

        // Act & Assert
        LogAct("Testing inequality operator");
        (sort1 != sort2).ShouldBeFalse();
        LogAssert("Inequality operator correctly returns false");
    }

    [Fact]
    public void InequalityOperator_WithDifferentValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two different SortInfos");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Email", SortDirection.Descending);

        // Act & Assert
        LogAct("Testing inequality operator");
        (sort1 != sort2).ShouldBeTrue();
        LogAssert("Inequality operator works correctly");
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameValues_ShouldReturnSameHash()
    {
        // Arrange
        LogArrange("Creating two SortInfos with same values");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Name", SortDirection.Ascending);

        // Act
        LogAct("Getting hash codes");
        var hash1 = sort1.GetHashCode();
        var hash2 = sort2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal");
        hash1.ShouldBe(hash2);
        LogInfo("Hash codes: {0} == {1}", hash1, hash2);
    }

    [Fact]
    public void GetHashCode_DifferentValues_ShouldReturnDifferentHash()
    {
        // Arrange
        LogArrange("Creating two different SortInfos");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2 = SortInfo.Create("Email", SortDirection.Descending);

        // Act
        LogAct("Getting hash codes");
        var hash1 = sort1.GetHashCode();
        var hash2 = sort2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are different");
        hash1.ShouldNotBe(hash2);
        LogInfo("Hash codes: {0} != {1}", hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ShouldCombineAllFields()
    {
        // Arrange
        LogArrange("Creating SortInfos differing in single fields");
        var baseSort = SortInfo.Create("Name", SortDirection.Ascending);
        var diffField = SortInfo.Create("Email", SortDirection.Ascending);
        var diffDirection = SortInfo.Create("Name", SortDirection.Descending);

        // Act
        LogAct("Getting hash codes");
        var baseHash = baseSort.GetHashCode();
        var fieldHash = diffField.GetHashCode();
        var directionHash = diffDirection.GetHashCode();

        // Assert
        LogAssert("Verifying all fields contribute to hash");
        baseHash.ShouldNotBe(fieldHash, "Field should affect hash");
        baseHash.ShouldNotBe(directionHash, "Direction should affect hash");
        LogInfo("All fields contribute to hash code");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating SortInfo");
        var sort = SortInfo.Create("LastName", SortDirection.Ascending);

        // Act
        LogAct("Calling ToString");
        var result = sort.ToString();

        // Assert
        LogAssert("Verifying format");
        result.ShouldBe("LastName Ascending");
        LogInfo("ToString: {0}", result);
    }

    [Fact]
    public void ToString_WithDescending_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating descending SortInfo");
        var sort = SortInfo.Create("CreatedAt", SortDirection.Descending);

        // Act
        LogAct("Calling ToString");
        var result = sort.ToString();

        // Assert
        LogAssert("Verifying format");
        result.ShouldBe("CreatedAt Descending");
        LogInfo("ToString: {0}", result);
    }

    #endregion

    #region ISpanFormattable Tests

    [Fact]
    public void ToStringWithFormatProvider_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating SortInfo for IFormattable test");
        var sort = SortInfo.Create("Name", SortDirection.Ascending);

        // Act
        LogAct("Calling ToString with format and provider");
        var result = sort.ToString(null, null);

        // Assert
        LogAssert("Verifying format matches");
        result.ShouldBe("Name Ascending");
        LogInfo("IFormattable ToString: {0}", result);
    }

    [Fact]
    public void TryFormat_WithSufficientBuffer_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating SortInfo and buffer");
        var sort = SortInfo.Create("Email", SortDirection.Ascending);
        Span<char> buffer = stackalloc char[50];

        // Act
        LogAct("Calling TryFormat");
        var success = sort.TryFormat(buffer, out var charsWritten, default, null);

        // Assert
        LogAssert("Verifying success and content");
        success.ShouldBeTrue();
        charsWritten.ShouldBe(15); // "Email Ascending" = 15 chars
        buffer[..charsWritten].ToString().ShouldBe("Email Ascending");
        LogInfo("TryFormat wrote {0} chars: {1}", charsWritten, buffer[..charsWritten].ToString());
    }

    [Fact]
    public void TryFormat_WithDescending_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating descending SortInfo and buffer");
        var sort = SortInfo.Create("Date", SortDirection.Descending);
        Span<char> buffer = stackalloc char[50];

        // Act
        LogAct("Calling TryFormat with Descending");
        var success = sort.TryFormat(buffer, out var charsWritten, default, null);

        // Assert
        LogAssert("Verifying success and content");
        success.ShouldBeTrue();
        charsWritten.ShouldBe(15); // "Date Descending" = 15 chars
        buffer[..charsWritten].ToString().ShouldBe("Date Descending");
        LogInfo("TryFormat wrote {0} chars: {1}", charsWritten, buffer[..charsWritten].ToString());
    }

    [Fact]
    public void TryFormat_WithExactBuffer_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating SortInfo with exact buffer size");
        var sort = SortInfo.Create("Id", SortDirection.Ascending);
        var expectedLength = 12; // "Id Ascending" = 12 chars
        Span<char> buffer = stackalloc char[expectedLength];

        // Act
        LogAct("Calling TryFormat with exact buffer");
        var success = sort.TryFormat(buffer, out var charsWritten, default, null);

        // Assert
        LogAssert("Verifying success with exact buffer");
        success.ShouldBeTrue();
        charsWritten.ShouldBe(expectedLength);
        buffer.ToString().ShouldBe("Id Ascending");
        LogInfo("TryFormat succeeded with exact buffer");
    }

    [Fact]
    public void TryFormat_WithInsufficientBuffer_ShouldFail()
    {
        // Arrange
        LogArrange("Creating SortInfo with small buffer");
        var sort = SortInfo.Create("Name", SortDirection.Ascending);
        Span<char> buffer = stackalloc char[5]; // Too small

        // Act
        LogAct("Calling TryFormat with insufficient buffer");
        var success = sort.TryFormat(buffer, out var charsWritten, default, null);

        // Assert
        LogAssert("Verifying failure");
        success.ShouldBeFalse();
        charsWritten.ShouldBe(0);
        LogInfo("TryFormat correctly failed with insufficient buffer");
    }

    [Fact]
    public void TryFormat_WithEmptyBuffer_ShouldFail()
    {
        // Arrange
        LogArrange("Creating SortInfo with empty buffer");
        var sort = SortInfo.Create("Field", SortDirection.Ascending);
        Span<char> buffer = Span<char>.Empty;

        // Act
        LogAct("Calling TryFormat with empty buffer");
        var success = sort.TryFormat(buffer, out var charsWritten, default, null);

        // Assert
        LogAssert("Verifying failure");
        success.ShouldBeFalse();
        charsWritten.ShouldBe(0);
        LogInfo("TryFormat correctly failed with empty buffer");
    }

    [Fact]
    public void TryFormat_WithBufferOneCharShort_ShouldFail()
    {
        // Arrange
        LogArrange("Creating SortInfo with buffer one char short");
        var sort = SortInfo.Create("Test", SortDirection.Ascending);
        var requiredLength = 14; // "Test Ascending" = 14 chars
        Span<char> buffer = stackalloc char[requiredLength - 1];

        // Act
        LogAct("Calling TryFormat with buffer one char short");
        var success = sort.TryFormat(buffer, out var charsWritten, default, null);

        // Assert
        LogAssert("Verifying failure");
        success.ShouldBeFalse();
        charsWritten.ShouldBe(0);
        LogInfo("TryFormat correctly failed when buffer is one char short");
    }

    [Fact]
    public void InterpolatedStringHandler_ShouldWorkCorrectly()
    {
        // Arrange
        LogArrange("Creating SortInfo for interpolation test");
        var sort = SortInfo.Create("Column", SortDirection.Descending);

        // Act
        LogAct("Using string interpolation");
        var result = $"{sort}";

        // Assert
        LogAssert("Verifying interpolation result");
        result.ShouldBe("Column Descending");
        LogInfo("Interpolation result: {0}", result);
    }

    [Fact]
    public void TryFormat_AscendingAndDescending_ShouldHaveCorrectLengths()
    {
        // Arrange
        LogArrange("Creating ascending and descending SortInfos with same field");
        var ascending = SortInfo.Create("X", SortDirection.Ascending);
        var descending = SortInfo.Create("X", SortDirection.Descending);
        Span<char> buffer = stackalloc char[50];

        // Act
        LogAct("Calling TryFormat for both directions");
        ascending.TryFormat(buffer, out var ascChars, default, null);
        var ascResult = buffer[..ascChars].ToString();

        descending.TryFormat(buffer, out var descChars, default, null);
        var descResult = buffer[..descChars].ToString();

        // Assert
        LogAssert("Verifying correct lengths for both directions");
        ascChars.ShouldBe(11); // "X Ascending" = 11
        descChars.ShouldBe(12); // "X Descending" = 12
        ascResult.ShouldBe("X Ascending");
        descResult.ShouldBe("X Descending");
        LogInfo("Ascending: {0} ({1} chars), Descending: {2} ({3} chars)", ascResult, ascChars, descResult, descChars);
    }

    #endregion

    #region Mutation Killing Tests

    [Fact]
    public void Equals_AllFieldsMustMatch()
    {
        // Arrange
        LogArrange("Creating SortInfos for comprehensive equality check");
        var reference = SortInfo.Create("Name", SortDirection.Ascending);

        var diffFieldOnly = SortInfo.Create("Email", SortDirection.Ascending);
        var diffDirectionOnly = SortInfo.Create("Name", SortDirection.Descending);

        // Act & Assert
        LogAct("Verifying each field affects equality");
        reference.Equals(diffFieldOnly).ShouldBeFalse("Field must affect equality");
        reference.Equals(diffDirectionOnly).ShouldBeFalse("Direction must affect equality");
        LogAssert("All equality conditions verified");
    }

    [Fact]
    public void InequalityOperator_ShouldNegateEquals()
    {
        // Arrange
        LogArrange("Creating SortInfos for inequality check");
        var sort1 = SortInfo.Create("Name", SortDirection.Ascending);
        var sort2Same = SortInfo.Create("Name", SortDirection.Ascending);
        var sort3Diff = SortInfo.Create("Email", SortDirection.Descending);

        // Act & Assert
        LogAct("Verifying != negates ==");
        (sort1 == sort2Same).ShouldBeTrue();
        (sort1 != sort2Same).ShouldBeFalse();
        (sort1 == sort3Diff).ShouldBeFalse();
        (sort1 != sort3Diff).ShouldBeTrue();
        LogAssert("Inequality operator correctly negates equality");
    }

    #endregion
}
