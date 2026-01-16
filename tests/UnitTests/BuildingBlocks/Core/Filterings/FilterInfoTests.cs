using Bedrock.BuildingBlocks.Core.Filterings;
using Bedrock.BuildingBlocks.Core.Filterings.Enums;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Filterings;

public class FilterInfoTests : TestBase
{
    public FilterInfoTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Create Tests

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up filter info components");
        var field = "LastName";
        var op = FilterOperator.Contains;
        var value = "Silva";

        // Act
        LogAct("Creating FilterInfo");
        var filter = FilterInfo.Create(field, op, value);

        // Assert
        LogAssert("Verifying all properties are set");
        filter.Field.ShouldBe(field);
        filter.Operator.ShouldBe(op);
        filter.Value.ShouldBe(value);
        filter.ValueEnd.ShouldBeNull();
        filter.Values.ShouldBeNull();
        LogInfo("FilterInfo created: {0}", filter);
    }

    [Fact]
    public void Create_WithNullValue_ShouldWork()
    {
        // Arrange
        LogArrange("Setting up filter with null value");

        // Act
        LogAct("Creating FilterInfo with null value");
        var filter = FilterInfo.Create("Status", FilterOperator.Equals, null);

        // Assert
        LogAssert("Verifying null value accepted");
        filter.Value.ShouldBeNull();
        LogInfo("FilterInfo with null value: {0}", filter);
    }

    [Fact]
    public void Create_WithNullField_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing null field");

        // Act & Assert
        LogAct("Creating FilterInfo with null field");
        var exception = Should.Throw<ArgumentException>(() =>
            FilterInfo.Create(null!, FilterOperator.Equals, "value"));

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
        LogAct("Creating FilterInfo with empty field");
        var exception = Should.Throw<ArgumentException>(() =>
            FilterInfo.Create("", FilterOperator.Equals, "value"));

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
        LogAct("Creating FilterInfo with whitespace field");
        var exception = Should.Throw<ArgumentException>(() =>
            FilterInfo.Create("   ", FilterOperator.Contains, "value"));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("field");
        LogInfo("Exception thrown: {0}", exception.Message);
    }

    #endregion

    #region CreateBetween Tests

    [Fact]
    public void CreateBetween_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up between filter");
        var field = "CreatedAt";
        var valueStart = "2024-01-01";
        var valueEnd = "2024-12-31";

        // Act
        LogAct("Creating Between FilterInfo");
        var filter = FilterInfo.CreateBetween(field, valueStart, valueEnd);

        // Assert
        LogAssert("Verifying properties");
        filter.Field.ShouldBe(field);
        filter.Operator.ShouldBe(FilterOperator.Between);
        filter.Value.ShouldBe(valueStart);
        filter.ValueEnd.ShouldBe(valueEnd);
        filter.Values.ShouldBeNull();
        LogInfo("Between filter: {0}", filter);
    }

    [Fact]
    public void CreateBetween_WithNullField_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing null field for between");

        // Act & Assert
        LogAct("Creating Between with null field");
        var exception = Should.Throw<ArgumentException>(() =>
            FilterInfo.CreateBetween(null!, "start", "end"));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("field");
    }

    [Fact]
    public void CreateBetween_WithNullValues_ShouldWork()
    {
        // Arrange
        LogArrange("Creating between with null values");

        // Act
        LogAct("Creating Between with nulls");
        var filter = FilterInfo.CreateBetween("Date", null, null);

        // Assert
        LogAssert("Verifying null values accepted");
        filter.Value.ShouldBeNull();
        filter.ValueEnd.ShouldBeNull();
        LogInfo("Between with nulls: {0}", filter);
    }

    #endregion

    #region CreateIn Tests

    [Fact]
    public void CreateIn_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up In filter");
        var field = "Status";
        var values = new[] { "Active", "Pending" };

        // Act
        LogAct("Creating In FilterInfo");
        var filter = FilterInfo.CreateIn(field, values);

        // Assert
        LogAssert("Verifying properties");
        filter.Field.ShouldBe(field);
        filter.Operator.ShouldBe(FilterOperator.In);
        filter.Value.ShouldBeNull();
        filter.ValueEnd.ShouldBeNull();
        filter.Values.ShouldBe(values);
        LogInfo("In filter: {0}", filter);
    }

    [Fact]
    public void CreateIn_WithNullField_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing null field for In");

        // Act & Assert
        LogAct("Creating In with null field");
        var exception = Should.Throw<ArgumentException>(() =>
            FilterInfo.CreateIn(null!, new[] { "value" }));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("field");
    }

    #endregion

    #region CreateNotIn Tests

    [Fact]
    public void CreateNotIn_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up NotIn filter");
        var field = "Status";
        var values = new[] { "Deleted", "Archived" };

        // Act
        LogAct("Creating NotIn FilterInfo");
        var filter = FilterInfo.CreateNotIn(field, values);

        // Assert
        LogAssert("Verifying properties");
        filter.Field.ShouldBe(field);
        filter.Operator.ShouldBe(FilterOperator.NotIn);
        filter.Value.ShouldBeNull();
        filter.ValueEnd.ShouldBeNull();
        filter.Values.ShouldBe(values);
        LogInfo("NotIn filter: {0}", filter);
    }

    [Fact]
    public void CreateNotIn_WithNullField_ShouldThrowArgumentException()
    {
        // Arrange
        LogArrange("Preparing null field for NotIn");

        // Act & Assert
        LogAct("Creating NotIn with null field");
        var exception = Should.Throw<ArgumentException>(() =>
            FilterInfo.CreateNotIn(null!, new[] { "value" }));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("field");
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up existing info");
        var field = "Field";
        var op = FilterOperator.Contains;
        var value = "value";
        var valueEnd = "end";
        var values = new[] { "a", "b" };

        // Act
        LogAct("Creating from existing info");
        var filter = FilterInfo.CreateFromExistingInfo(field, op, value, valueEnd, values);

        // Assert
        LogAssert("Verifying all properties");
        filter.Field.ShouldBe(field);
        filter.Operator.ShouldBe(op);
        filter.Value.ShouldBe(value);
        filter.ValueEnd.ShouldBe(valueEnd);
        filter.Values.ShouldBe(values);
        LogInfo("Filter from existing: {0}", filter);
    }

    [Fact]
    public void CreateFromExistingInfo_WithEmptyField_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing empty field for existing info");

        // Act
        LogAct("Creating from existing with empty field");
        var filter = FilterInfo.CreateFromExistingInfo("", FilterOperator.Equals, null, null, null);

        // Assert
        LogAssert("Verifying no exception");
        filter.Field.ShouldBe("");
        LogInfo("Empty field accepted");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two FilterInfos with same values");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter2 = FilterInfo.Create("Name", FilterOperator.Contains, "test");

        // Act
        LogAct("Comparing for equality");
        var areEqual = filter1.Equals(filter2);

        // Assert
        LogAssert("Verifying equality");
        areEqual.ShouldBeTrue();
        LogInfo("FilterInfos are equal");
    }

    [Fact]
    public void Equals_WithDifferentField_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating FilterInfos with different fields");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter2 = FilterInfo.Create("Email", FilterOperator.Contains, "test");

        // Act
        LogAct("Comparing for equality");
        var areEqual = filter1.Equals(filter2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentOperator_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating FilterInfos with different operators");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter2 = FilterInfo.Create("Name", FilterOperator.Equals, "test");

        // Act
        LogAct("Comparing for equality");
        var areEqual = filter1.Equals(filter2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating FilterInfos with different values");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test1");
        var filter2 = FilterInfo.Create("Name", FilterOperator.Contains, "test2");

        // Act
        LogAct("Comparing for equality");
        var areEqual = filter1.Equals(filter2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentValueEnd_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating Between filters with different ValueEnd");
        var filter1 = FilterInfo.CreateBetween("Date", "2024-01-01", "2024-06-30");
        var filter2 = FilterInfo.CreateBetween("Date", "2024-01-01", "2024-12-31");

        // Act
        LogAct("Comparing for equality");
        var areEqual = filter1.Equals(filter2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWork()
    {
        // Arrange
        LogArrange("Creating FilterInfo and objects for equality test");
        var filter = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        object objSame = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        object objDifferent = FilterInfo.Create("Email", FilterOperator.Contains, "test");
        object? objNull = null;
        object objWrongType = "not a filter";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        filter.Equals(objSame).ShouldBeTrue();
        filter.Equals(objDifferent).ShouldBeFalse();
        filter.Equals(objNull).ShouldBeFalse();
        filter.Equals(objWrongType).ShouldBeFalse();
        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void EqualityOperator_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two FilterInfos with same values");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter2 = FilterInfo.Create("Name", FilterOperator.Contains, "test");

        // Act & Assert
        LogAct("Testing equality operator");
        (filter1 == filter2).ShouldBeTrue();
        LogAssert("Equality operator works correctly");
    }

    [Fact]
    public void InequalityOperator_WithDifferentValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two different FilterInfos");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter2 = FilterInfo.Create("Email", FilterOperator.Equals, "other");

        // Act & Assert
        LogAct("Testing inequality operator");
        (filter1 != filter2).ShouldBeTrue();
        LogAssert("Inequality operator works correctly");
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameValues_ShouldReturnSameHash()
    {
        // Arrange
        LogArrange("Creating two FilterInfos with same values");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter2 = FilterInfo.Create("Name", FilterOperator.Contains, "test");

        // Act
        LogAct("Getting hash codes");
        var hash1 = filter1.GetHashCode();
        var hash2 = filter2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal");
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void GetHashCode_DifferentValues_ShouldReturnDifferentHash()
    {
        // Arrange
        LogArrange("Creating two different FilterInfos");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter2 = FilterInfo.Create("Email", FilterOperator.Equals, "other");

        // Act
        LogAct("Getting hash codes");
        var hash1 = filter1.GetHashCode();
        var hash2 = filter2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are different");
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void GetHashCode_ShouldCombineAllRelevantFields()
    {
        // Arrange
        LogArrange("Creating FilterInfos differing in single fields");
        var baseFilter = FilterInfo.CreateBetween("Date", "2024-01-01", "2024-12-31");
        var diffField = FilterInfo.CreateBetween("Time", "2024-01-01", "2024-12-31");
        var diffValue = FilterInfo.CreateBetween("Date", "2023-01-01", "2024-12-31");
        var diffValueEnd = FilterInfo.CreateBetween("Date", "2024-01-01", "2025-12-31");

        // Act
        LogAct("Getting hash codes");
        var baseHash = baseFilter.GetHashCode();
        var fieldHash = diffField.GetHashCode();
        var valueHash = diffValue.GetHashCode();
        var valueEndHash = diffValueEnd.GetHashCode();

        // Assert
        LogAssert("Verifying all fields contribute to hash");
        baseHash.ShouldNotBe(fieldHash, "Field should affect hash");
        baseHash.ShouldNotBe(valueHash, "Value should affect hash");
        baseHash.ShouldNotBe(valueEndHash, "ValueEnd should affect hash");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_SimpleFilter_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating simple filter");
        var filter = FilterInfo.Create("Name", FilterOperator.Contains, "Silva");

        // Act
        LogAct("Calling ToString");
        var result = filter.ToString();

        // Assert
        LogAssert("Verifying format");
        result.ShouldBe("Name Contains Silva");
        LogInfo("ToString: {0}", result);
    }

    [Fact]
    public void ToString_BetweenFilter_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating between filter");
        var filter = FilterInfo.CreateBetween("CreatedAt", "2024-01-01", "2024-12-31");

        // Act
        LogAct("Calling ToString");
        var result = filter.ToString();

        // Assert
        LogAssert("Verifying format");
        result.ShouldBe("CreatedAt Between [2024-01-01, 2024-12-31]");
        LogInfo("ToString: {0}", result);
    }

    [Fact]
    public void ToString_InFilter_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating In filter");
        var filter = FilterInfo.CreateIn("Status", new[] { "Active", "Pending" });

        // Act
        LogAct("Calling ToString");
        var result = filter.ToString();

        // Assert
        LogAssert("Verifying format");
        result.ShouldBe("Status In [Active, Pending]");
        LogInfo("ToString: {0}", result);
    }

    [Fact]
    public void ToString_NotInFilter_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating NotIn filter");
        var filter = FilterInfo.CreateNotIn("Status", new[] { "Deleted" });

        // Act
        LogAct("Calling ToString");
        var result = filter.ToString();

        // Assert
        LogAssert("Verifying format");
        result.ShouldBe("Status NotIn [Deleted]");
        LogInfo("ToString: {0}", result);
    }

    [Fact]
    public void ToString_InFilterWithEmptyValues_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating In filter with empty values via CreateFromExistingInfo");
        var filter = FilterInfo.CreateFromExistingInfo("Status", FilterOperator.In, null, null, null);

        // Act
        LogAct("Calling ToString");
        var result = filter.ToString();

        // Assert
        LogAssert("Verifying format with empty values");
        result.ShouldBe("Status In []");
        LogInfo("ToString: {0}", result);
    }

    #endregion

    #region Mutation Killing Tests

    [Fact]
    public void Equals_AllFieldsMustMatch()
    {
        // Arrange
        LogArrange("Creating FilterInfos for comprehensive equality check");
        var reference = FilterInfo.CreateBetween("Date", "start", "end");

        var diffFieldOnly = FilterInfo.CreateBetween("Time", "start", "end");
        var diffValueOnly = FilterInfo.CreateBetween("Date", "other", "end");
        var diffValueEndOnly = FilterInfo.CreateBetween("Date", "start", "other");

        // Act & Assert
        LogAct("Verifying each field affects equality");
        reference.Equals(diffFieldOnly).ShouldBeFalse("Field must affect equality");
        reference.Equals(diffValueOnly).ShouldBeFalse("Value must affect equality");
        reference.Equals(diffValueEndOnly).ShouldBeFalse("ValueEnd must affect equality");
        LogAssert("All equality conditions verified");
    }

    [Fact]
    public void Equals_OperatorMustMatch()
    {
        // Arrange
        LogArrange("Creating FilterInfos with different operators");
        var filter1 = FilterInfo.Create("Field", FilterOperator.Equals, "value");
        var filter2 = FilterInfo.Create("Field", FilterOperator.NotEquals, "value");

        // Act & Assert
        LogAct("Verifying operator affects equality");
        filter1.Equals(filter2).ShouldBeFalse("Operator must affect equality");
        LogAssert("Operator equality verified");
    }

    [Fact]
    public void InequalityOperator_ShouldNegateEquals()
    {
        // Arrange
        LogArrange("Creating FilterInfos for inequality check");
        var filter1 = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter2Same = FilterInfo.Create("Name", FilterOperator.Contains, "test");
        var filter3Diff = FilterInfo.Create("Email", FilterOperator.Equals, "other");

        // Act & Assert
        LogAct("Verifying != negates ==");
        (filter1 == filter2Same).ShouldBeTrue();
        (filter1 != filter2Same).ShouldBeFalse();
        (filter1 == filter3Diff).ShouldBeFalse();
        (filter1 != filter3Diff).ShouldBeTrue();
        LogAssert("Inequality operator correctly negates equality");
    }

    #endregion
}
