using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.BirthDates;

public class BirthDateTests : TestBase
{
    public BirthDateTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void CreateNew_WithDateTimeOffset_ShouldPreserveValue()
    {
        // Arrange
        LogArrange("Creating a DateTimeOffset for birth date");
        var expectedDate = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Creating BirthDate from DateTimeOffset");
        var birthDate = BirthDate.CreateNew(expectedDate);

        // Assert
        LogAssert("Verifying value is preserved");
        birthDate.Value.ShouldBe(expectedDate);
        LogInfo("BirthDate value: {0}", birthDate.Value);
    }

    [Fact]
    public void CreateNew_WithDateOnly_ShouldConvertToUtcDateTime()
    {
        // Arrange
        LogArrange("Creating a DateOnly for birth date");
        var dateOnly = new DateOnly(1985, 12, 25);

        // Act
        LogAct("Creating BirthDate from DateOnly");
        var birthDate = BirthDate.CreateNew(dateOnly);

        // Assert
        LogAssert("Verifying conversion to UTC DateTime");
        birthDate.Value.Year.ShouldBe(1985);
        birthDate.Value.Month.ShouldBe(12);
        birthDate.Value.Day.ShouldBe(25);
        birthDate.Value.Hour.ShouldBe(0);
        birthDate.Value.Minute.ShouldBe(0);
        birthDate.Value.Second.ShouldBe(0);
        LogInfo("BirthDate from DateOnly: {0}", birthDate.Value);
    }

    [Fact]
    public void CalculateAgeInYears_WithReferenceDate_ShouldReturnCorrectAge()
    {
        // Arrange
        LogArrange("Creating birth date and reference date");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(1990, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var referenceDate = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Calculating age in years");
        var age = birthDate.CalculateAgeInYears(referenceDate);

        // Assert
        LogAssert("Verifying age is correct");
        age.ShouldBe(34);
        LogInfo("Age on birthday: {0} years", age);
    }

    [Fact]
    public void CalculateAgeInYears_BeforeBirthday_ShouldReturnOneYearLess()
    {
        // Arrange
        LogArrange("Creating birth date and reference date before birthday");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(1990, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var referenceDate = new DateTimeOffset(2024, 6, 14, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Calculating age before birthday");
        var age = birthDate.CalculateAgeInYears(referenceDate);

        // Assert
        LogAssert("Verifying age is one year less");
        age.ShouldBe(33);
        LogInfo("Age before birthday: {0} years", age);
    }

    [Fact]
    public void CalculateAgeInYears_DifferentMonthBeforeBirthday_ShouldReturnOneYearLess()
    {
        // Arrange
        LogArrange("Creating birth date and reference date in earlier month");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(1990, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var referenceDate = new DateTimeOffset(2024, 5, 20, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Calculating age with reference in earlier month");
        var age = birthDate.CalculateAgeInYears(referenceDate);

        // Assert
        LogAssert("Verifying age is one year less");
        age.ShouldBe(33);
        LogInfo("Age before birthday month: {0} years", age);
    }

    [Fact]
    public void CalculateAgeInYears_AfterBirthday_ShouldReturnFullAge()
    {
        // Arrange
        LogArrange("Creating birth date and reference date after birthday");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(1990, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var referenceDate = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Calculating age after birthday");
        var age = birthDate.CalculateAgeInYears(referenceDate);

        // Assert
        LogAssert("Verifying full age is returned");
        age.ShouldBe(34);
        LogInfo("Age after birthday: {0} years", age);
    }

    [Fact]
    public void CalculateAgeInYears_WithTimeProvider_ShouldUseProviderTime()
    {
        // Arrange
        LogArrange("Creating birth date and time provider");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var fixedTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(fixedTime);

        // Act
        LogAct("Calculating age using time provider");
        var age = birthDate.CalculateAgeInYears(timeProvider);

        // Assert
        LogAssert("Verifying age uses provider time");
        age.ShouldBe(25);
        LogInfo("Age using TimeProvider: {0} years", age);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two BirthDates with same value");
        var date = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var birthDate1 = BirthDate.CreateNew(date);
        var birthDate2 = BirthDate.CreateNew(date);

        // Act
        LogAct("Comparing BirthDates for equality");
        var areEqual = birthDate1.Equals(birthDate2);

        // Assert
        LogAssert("Verifying BirthDates are equal");
        areEqual.ShouldBeTrue();
        LogInfo("BirthDates with same value are equal");
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two BirthDates with different values");
        var birthDate1 = BirthDate.CreateNew(new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero));
        var birthDate2 = BirthDate.CreateNew(new DateTimeOffset(1991, 5, 15, 0, 0, 0, TimeSpan.Zero));

        // Act
        LogAct("Comparing BirthDates for equality");
        var areEqual = birthDate1.Equals(birthDate2);

        // Assert
        LogAssert("Verifying BirthDates are not equal");
        areEqual.ShouldBeFalse();
        LogInfo("BirthDates with different values are not equal");
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWork()
    {
        // Arrange
        LogArrange("Creating BirthDate and object for equality test");
        var date = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var birthDate = BirthDate.CreateNew(date);
        object objSame = BirthDate.CreateNew(date);
        object objDifferent = BirthDate.CreateNew(new DateTimeOffset(1991, 5, 15, 0, 0, 0, TimeSpan.Zero));
        object? objNull = null;
        object objWrongType = "not a BirthDate";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        birthDate.Equals(objSame).ShouldBeTrue("Equal BirthDate objects should be equal");
        birthDate.Equals(objDifferent).ShouldBeFalse("Different BirthDate objects should not be equal");
        birthDate.Equals(objNull).ShouldBeFalse("Null should not be equal");
        birthDate.Equals(objWrongType).ShouldBeFalse("Wrong type should not be equal");

        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two BirthDates with same value");
        var date = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var birthDate1 = BirthDate.CreateNew(date);
        var birthDate2 = BirthDate.CreateNew(date);

        // Act & Assert
        LogAct("Testing equality operator");
        (birthDate1 == birthDate2).ShouldBeTrue();
        LogAssert("Equality operator works correctly");
    }

    [Fact]
    public void InequalityOperator_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two BirthDates with different values");
        var birthDate1 = BirthDate.CreateNew(new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero));
        var birthDate2 = BirthDate.CreateNew(new DateTimeOffset(1991, 5, 15, 0, 0, 0, TimeSpan.Zero));

        // Act & Assert
        LogAct("Testing inequality operator");
        (birthDate1 != birthDate2).ShouldBeTrue();
        LogAssert("Inequality operator works correctly");
    }

    [Fact]
    public void ComparisonOperators_ShouldWork()
    {
        // Arrange
        LogArrange("Creating two BirthDates for comparison");
        var earlier = BirthDate.CreateNew(new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero));
        var later = BirthDate.CreateNew(new DateTimeOffset(1991, 5, 15, 0, 0, 0, TimeSpan.Zero));

        // Act & Assert
        LogAct("Testing comparison operators");
        (earlier < later).ShouldBeTrue("Earlier should be less than later");
        (later > earlier).ShouldBeTrue("Later should be greater than earlier");
        (earlier <= later).ShouldBeTrue("Earlier should be less than or equal to later");
        (later >= earlier).ShouldBeTrue("Later should be greater than or equal to earlier");
#pragma warning disable CS1718 // Comparison to same variable - intentional test of reflexive comparison operators
        (earlier <= earlier).ShouldBeTrue("BirthDate should be less than or equal to itself");
        (earlier >= earlier).ShouldBeTrue("BirthDate should be greater than or equal to itself");
#pragma warning restore CS1718
        LogAssert("All comparison operators work correctly");
    }

    [Fact]
    public void ComparisonOperators_LessThan_WithEqualValues_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two BirthDates with same value");
        var date = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var birthDate1 = BirthDate.CreateNew(date);
        var birthDate2 = BirthDate.CreateNew(date);

        // Act & Assert
        LogAct("Testing less than operator with equal values");
        (birthDate1 < birthDate2).ShouldBeFalse("Equal BirthDates should not be less than each other");
        (birthDate2 < birthDate1).ShouldBeFalse("Equal BirthDates should not be less than each other");
        LogAssert("Less than operator returns false for equal BirthDates");
    }

    [Fact]
    public void ComparisonOperators_GreaterThan_WithEqualValues_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two BirthDates with same value");
        var date = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var birthDate1 = BirthDate.CreateNew(date);
        var birthDate2 = BirthDate.CreateNew(date);

        // Act & Assert
        LogAct("Testing greater than operator with equal values");
        (birthDate1 > birthDate2).ShouldBeFalse("Equal BirthDates should not be greater than each other");
        (birthDate2 > birthDate1).ShouldBeFalse("Equal BirthDates should not be greater than each other");
        LogAssert("Greater than operator returns false for equal BirthDates");
    }

    [Fact]
    public void CompareTo_ShouldReturnCorrectValues()
    {
        // Arrange
        LogArrange("Creating BirthDates for CompareTo test");
        var date = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var earlier = BirthDate.CreateNew(date.AddYears(-1));
        var same = BirthDate.CreateNew(date);
        var later = BirthDate.CreateNew(date.AddYears(1));
        var birthDate = BirthDate.CreateNew(date);

        // Act & Assert
        LogAct("Testing CompareTo method");
        birthDate.CompareTo(earlier).ShouldBeGreaterThan(0, "Should be greater than earlier date");
        birthDate.CompareTo(same).ShouldBe(0, "Should be equal to same date");
        birthDate.CompareTo(later).ShouldBeLessThan(0, "Should be less than later date");
        LogAssert("CompareTo returns correct values");
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        LogArrange("Creating a BirthDate");
        var date = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var birthDate = BirthDate.CreateNew(date);

        // Act
        LogAct("Getting hash code multiple times");
        var hash1 = birthDate.GetHashCode();
        var hash2 = birthDate.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are consistent");
        hash1.ShouldBe(hash2);
        hash1.ShouldBe(date.GetHashCode());
        LogInfo("Hash code is consistent: {0}", hash1);
    }

    [Fact]
    public void ImplicitConversion_ToDateTimeOffset_ShouldWork()
    {
        // Arrange
        LogArrange("Creating a BirthDate");
        var expectedDate = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
        var birthDate = BirthDate.CreateNew(expectedDate);

        // Act
        LogAct("Implicitly converting BirthDate to DateTimeOffset");
        DateTimeOffset result = birthDate;

        // Assert
        LogAssert("Verifying conversion preserves value");
        result.ShouldBe(expectedDate);
        LogInfo("Implicit conversion to DateTimeOffset successful");
    }

    [Fact]
    public void ImplicitConversion_FromDateTimeOffset_ShouldWork()
    {
        // Arrange
        LogArrange("Creating a DateTimeOffset");
        var dateTimeOffset = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Implicitly converting DateTimeOffset to BirthDate");
        BirthDate birthDate = dateTimeOffset;

        // Assert
        LogAssert("Verifying conversion preserves value");
        birthDate.Value.ShouldBe(dateTimeOffset);
        LogInfo("Implicit conversion from DateTimeOffset successful");
    }

    [Fact]
    public void CalculateAgeInYears_SameMonthDifferentDay_BeforeBirthday_ShouldReturnOneYearLess()
    {
        // Arrange
        LogArrange("Creating birth date June 15, reference June 10");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(1990, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var referenceDate = new DateTimeOffset(2024, 6, 10, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Calculating age in same month but before birth day");
        var age = birthDate.CalculateAgeInYears(referenceDate);

        // Assert
        LogAssert("Verifying age is one year less when in same month but before birthday");
        age.ShouldBe(33);
        LogInfo("Age in same month before birthday: {0} years", age);
    }

    [Fact]
    public void CalculateAgeInYears_SameMonthSameDay_ShouldReturnFullAge()
    {
        // Arrange
        LogArrange("Creating birth date and reference date on same day");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(1990, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var referenceDate = new DateTimeOffset(2024, 6, 15, 12, 30, 0, TimeSpan.Zero);

        // Act
        LogAct("Calculating age on birthday");
        var age = birthDate.CalculateAgeInYears(referenceDate);

        // Assert
        LogAssert("Verifying full age on birthday");
        age.ShouldBe(34);
        LogInfo("Age on birthday: {0} years", age);
    }

    [Fact]
    public void CalculateAgeInYears_SameMonthDayAfter_ShouldReturnFullAge()
    {
        // Arrange
        LogArrange("Creating birth date June 15, reference June 20");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(1990, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var referenceDate = new DateTimeOffset(2024, 6, 20, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Calculating age in same month after birthday");
        var age = birthDate.CalculateAgeInYears(referenceDate);

        // Assert
        LogAssert("Verifying full age when after birthday in same month");
        age.ShouldBe(34);
        LogInfo("Age after birthday in same month: {0} years", age);
    }

    [Fact]
    public void CalculateAgeInYears_ReferenceDateBeforeBirthDate_ShouldReturnNegative()
    {
        // Arrange
        LogArrange("Creating birth date after reference date");
        var birthDate = BirthDate.CreateNew(new DateTimeOffset(2000, 6, 15, 0, 0, 0, TimeSpan.Zero));
        var referenceDate = new DateTimeOffset(1990, 6, 15, 0, 0, 0, TimeSpan.Zero);

        // Act
        LogAct("Calculating age with reference before birth");
        var age = birthDate.CalculateAgeInYears(referenceDate);

        // Assert
        LogAssert("Verifying negative age is returned");
        age.ShouldBeLessThan(0);
        age.ShouldBe(-10);
        LogInfo("Age before birth: {0} years", age);
    }

    private class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _fixedTime;

        public FixedTimeProvider(DateTimeOffset fixedTime)
        {
            _fixedTime = fixedTime;
        }

        public override DateTimeOffset GetUtcNow() => _fixedTime;
    }
}
