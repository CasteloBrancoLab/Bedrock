using Bedrock.BuildingBlocks.Core.Extensions;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Extensions;

public class StringExtensionsTests : TestBase
{
    public StringExtensionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Constants Tests

    [Fact]
    public void KebabCaseSeparator_ShouldBeHyphen()
    {
        // Arrange
        LogArrange("Getting KebabCaseSeparator constant");

        // Act
        LogAct("Reading constant value");
        var separator = StringExtensions.KebabCaseSeparator;

        // Assert
        LogAssert("Verifying value is hyphen");
        separator.ShouldBe('-');
    }

    [Fact]
    public void SnakeCaseSeparator_ShouldBeUnderscore()
    {
        // Arrange
        LogArrange("Getting SnakeCaseSeparator constant");

        // Act
        LogAct("Reading constant value");
        var separator = StringExtensions.SnakeCaseSeparator;

        // Assert
        LogAssert("Verifying value is underscore");
        separator.ShouldBe('_');
    }

    #endregion

    #region ToKebabCase Tests

    [Fact]
    public void ToKebabCase_Null_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        string? input = null;

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result is null");
        result.ShouldBeNull();
    }

    [Fact]
    public void ToKebabCase_EmptyString_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Preparing empty string");
        string input = string.Empty;

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result is empty");
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("helloWorld", "hello-world")]
    [InlineData("Hello", "hello")]
    [InlineData("hello", "hello")]
    [InlineData("HELLO", "hello")]
    [InlineData("ABCdef", "abcdef")]
    [InlineData("XMLParser", "xmlparser")]
    [InlineData("myXMLParser", "my-xmlparser")]
    public void ToKebabCase_PascalOrCamelCase_ShouldConvertCorrectly(string input, string expected)
    {
        // Arrange
        LogArrange("Preparing input");
        LogInfo("Input: {0}", input);

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe(expected);
        LogInfo("Result: {0}", result);
    }

    [Theory]
    [InlineData("Hello-World", "hello-world")]
    [InlineData("hello-world", "hello-world")]
    [InlineData("Hello--World", "hello-world")]
    [InlineData("-Hello-World-", "hello-world")]
    public void ToKebabCase_WithExistingHyphens_ShouldHandleCorrectly(string input, string expected)
    {
        // Arrange
        LogArrange("Preparing input with hyphens");
        LogInfo("Input: {0}", input);

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe(expected);
        LogInfo("Result: {0}", result);
    }

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Hello_World", "hello-world")]
    [InlineData("Hello.World", "hello-world")]
    public void ToKebabCase_WithOtherSeparators_ShouldRemoveAndAddSeparatorOnCaseChange(string input, string expected)
    {
        // Arrange
        LogArrange("Preparing input with other separators");
        LogInfo("Input: {0}", input);

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe(expected);
        LogInfo("Result: {0}", result);
    }

    [Theory]
    [InlineData("Test123", "test123")]
    [InlineData("Test123Value", "test123-value")]
    [InlineData("123Test", "123-test")]
    public void ToKebabCase_WithDigits_ShouldPreserveDigits(string input, string expected)
    {
        // Arrange
        LogArrange("Preparing input with digits");
        LogInfo("Input: {0}", input);

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe(expected);
        LogInfo("Result: {0}", result);
    }

    [Fact]
    public void ToKebabCase_SingleUpperCase_ShouldReturnLowerCase()
    {
        // Arrange
        LogArrange("Preparing single uppercase");
        string input = "A";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe("a");
    }

    [Fact]
    public void ToKebabCase_SingleLowerCase_ShouldReturnSame()
    {
        // Arrange
        LogArrange("Preparing single lowercase");
        string input = "a";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe("a");
    }

    [Fact]
    public void ToKebabCase_TrailingHyphen_ShouldRemoveIt()
    {
        // Arrange
        LogArrange("Preparing input with trailing hyphen");
        string input = "Hello-";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying trailing hyphen removed");
        result.ShouldBe("hello");
    }

    [Fact]
    public void ToKebabCase_LeadingHyphen_ShouldRemoveIt()
    {
        // Arrange
        LogArrange("Preparing input with leading hyphen");
        string input = "-Hello";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying leading hyphen removed");
        result.ShouldBe("hello");
    }

    [Fact]
    public void ToKebabCase_OnlyHyphens_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Preparing input with only hyphens");
        string input = "---";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying result is empty");
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void ToKebabCase_ConsecutiveUppercase_ShouldNotSeparate()
    {
        // Arrange
        LogArrange("Preparing consecutive uppercase");
        string input = "ABC";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying no separation between uppercase");
        result.ShouldBe("abc");
    }

    [Fact]
    public void ToKebabCase_MixedWithSpecialChars_ShouldIgnoreNonHyphen()
    {
        // Arrange
        LogArrange("Preparing input with special characters");
        string input = "Hello!World";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying special chars removed and case transition handled");
        result.ShouldBe("hello-world");
    }

    [Fact]
    public void ToKebabCase_DigitAfterLowercase_ShouldNotAddSeparator()
    {
        // Arrange
        LogArrange("Preparing digit after lowercase");
        string input = "test1";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying no separator before digit");
        result.ShouldBe("test1");
    }

    [Fact]
    public void ToKebabCase_LowercaseHyphenUppercase_ShouldKeepHyphen()
    {
        // Arrange - kills mutant on line 40 (lastCharWasSeparator = false initialization)
        // If lastCharWasSeparator starts as true, the first hyphen would be ignored
        LogArrange("Preparing lowercase-hyphen-uppercase sequence");
        string input = "a-B";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying hyphen is preserved");
        result.ShouldBe("a-b");
    }

    [Fact]
    public void ToKebabCase_UppercaseHyphenLowercase_ShouldKeepHyphen()
    {
        // Arrange - kills mutant on line 63 (lastCharWasSeparator = false after uppercase)
        // If lastCharWasSeparator becomes true after uppercase, subsequent hyphen would be ignored
        LogArrange("Preparing uppercase-hyphen-lowercase sequence");
        string input = "A-b";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying hyphen after uppercase is preserved");
        result.ShouldBe("a-b");
    }

    #endregion

    #region ToSnakeCase Tests

    [Fact]
    public void ToSnakeCase_Null_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        string input = null!;

        // Act
        LogAct("Calling ToSnakeCase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying result is null");
        result.ShouldBeNull();
    }

    [Fact]
    public void ToSnakeCase_EmptyString_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Preparing empty string");
        string input = string.Empty;

        // Act
        LogAct("Calling ToSnakeCase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying result is empty");
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("helloWorld", "hello_world")]
    [InlineData("Hello", "hello")]
    [InlineData("hello", "hello")]
    [InlineData("HELLO", "h_e_l_l_o")]
    [InlineData("ABCdef", "a_b_cdef")]
    [InlineData("XMLParser", "x_m_l_parser")]
    public void ToSnakeCase_PascalOrCamelCase_ShouldConvertCorrectly(string input, string expected)
    {
        // Arrange
        LogArrange("Preparing input");
        LogInfo("Input: {0}", input);

        // Act
        LogAct("Calling ToSnakeCase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe(expected);
        LogInfo("Result: {0}", result);
    }

    [Theory]
    [InlineData("Test123", "test123")]
    [InlineData("Test123Value", "test123_value")]
    [InlineData("123Test", "123_test")]
    public void ToSnakeCase_WithDigits_ShouldPreserveDigits(string input, string expected)
    {
        // Arrange
        LogArrange("Preparing input with digits");
        LogInfo("Input: {0}", input);

        // Act
        LogAct("Calling ToSnakeCase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe(expected);
        LogInfo("Result: {0}", result);
    }

    [Fact]
    public void ToSnakeCase_SingleUpperCase_ShouldReturnLowerCase()
    {
        // Arrange
        LogArrange("Preparing single uppercase");
        string input = "A";

        // Act
        LogAct("Calling ToSnakeCase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe("a");
    }

    [Fact]
    public void ToSnakeCase_SingleLowerCase_ShouldReturnSame()
    {
        // Arrange
        LogArrange("Preparing single lowercase");
        string input = "a";

        // Act
        LogAct("Calling ToSnakeCase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe("a");
    }

    [Fact]
    public void ToSnakeCase_WithSpecialChars_ShouldPreserve()
    {
        // Arrange
        LogArrange("Preparing input with special characters");
        string input = "Hello_World";

        // Act
        LogAct("Calling ToSnakeCase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying special chars preserved");
        result.ShouldBe("hello__world");
    }

    [Fact]
    public void ToSnakeCase_ConsecutiveUppercase_ShouldSeparateEach()
    {
        // Arrange
        LogArrange("Preparing consecutive uppercase");
        string input = "ABC";

        // Act
        LogAct("Calling ToSnakeCase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying each letter separated");
        result.ShouldBe("a_b_c");
    }

    #endregion

    #region OnlyLettersAndDigits Tests

    [Fact]
    public void OnlyLettersAndDigits_Null_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        string input = null!;

        // Act
        LogAct("Calling OnlyLettersAndDigits");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying result is null");
        result.ShouldBeNull();
    }

    [Fact]
    public void OnlyLettersAndDigits_EmptyString_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Preparing empty string");
        string input = string.Empty;

        // Act
        LogAct("Calling OnlyLettersAndDigits");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying result is empty");
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("Hello123", "Hello123")]
    [InlineData("Hello World", "HelloWorld")]
    [InlineData("Hello-World", "HelloWorld")]
    [InlineData("Hello_World", "HelloWorld")]
    [InlineData("Hello.World!", "HelloWorld")]
    [InlineData("@#$%^&*()", "")]
    [InlineData("Test!@#123", "Test123")]
    public void OnlyLettersAndDigits_ShouldFilterCorrectly(string input, string expected)
    {
        // Arrange
        LogArrange("Preparing input");
        LogInfo("Input: {0}", input);

        // Act
        LogAct("Calling OnlyLettersAndDigits");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying result");
        result.ShouldBe(expected);
        LogInfo("Result: {0}", result);
    }

    [Fact]
    public void OnlyLettersAndDigits_OnlyLetters_ShouldReturnSame()
    {
        // Arrange
        LogArrange("Preparing only letters");
        string input = "HelloWorld";

        // Act
        LogAct("Calling OnlyLettersAndDigits");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying result unchanged");
        result.ShouldBe("HelloWorld");
    }

    [Fact]
    public void OnlyLettersAndDigits_OnlyDigits_ShouldReturnSame()
    {
        // Arrange
        LogArrange("Preparing only digits");
        string input = "12345";

        // Act
        LogAct("Calling OnlyLettersAndDigits");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying result unchanged");
        result.ShouldBe("12345");
    }

    [Fact]
    public void OnlyLettersAndDigits_MixedWithUnicode_ShouldPreserveLetters()
    {
        // Arrange
        LogArrange("Preparing input with unicode");
        string input = "Héllo123";

        // Act
        LogAct("Calling OnlyLettersAndDigits");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying unicode letters preserved");
        result.ShouldBe("Héllo123");
    }

    #endregion
}
