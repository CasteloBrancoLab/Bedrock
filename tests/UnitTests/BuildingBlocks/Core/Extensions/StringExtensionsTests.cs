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

    [Fact]
    public void ToKebabCase_LeadingSpecialCharFollowedByLetter_ShouldNotAddLeadingSeparator()
    {
        // Arrange - kills mutant on line 109 (position > 0 vs position >= 0)
        // If mutated to >= 0, a separator would be added at position 0
        LogArrange("Preparing input starting with special char");
        string input = "!hello";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying no leading separator");
        result.ShouldBe("hello");
        result.ShouldNotStartWith("-");
    }

    [Fact]
    public void ToKebabCase_SpecialCharFollowedByUppercase_ShouldNotAddExtraSeparator()
    {
        // Arrange - kills mutant on line 115 (lastCharWasLowerCaseOrDigit = false vs true)
        // If mutated to true, an extra separator would be added before uppercase after special char
        LogArrange("Preparing special char followed by uppercase");
        string input = "!A";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying no extra separator before uppercase");
        result.ShouldBe("a");
    }

    [Fact]
    public void ToKebabCase_MultipleSpecialCharsFollowedByUppercase_ShouldNotAddExtraSeparator()
    {
        // Arrange - additional coverage for line 115 mutation
        LogArrange("Preparing multiple special chars followed by uppercase");
        string input = "!!!ABC";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying no extra separators");
        result.ShouldBe("abc");
    }

    [Fact]
    public void ToKebabCase_LowercaseSpecialCharUppercase_ShouldAddOneSeparator()
    {
        // Arrange - ensures proper behavior: lowercase, then special, then uppercase
        // The separator should come from the special char, not from the uppercase detection
        LogArrange("Preparing lowercase-special-uppercase sequence");
        string input = "a!B";

        // Act
        LogAct("Calling ToKebabCase");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying exactly one separator");
        result.ShouldBe("a-b");
        result.Count(c => c == '-').ShouldBe(1);
    }

    [Fact]
    public void ToKebabCase_LargeString_ShouldUseArrayPoolPath()
    {
        // Arrange - string > 128 chars to trigger ArrayPool path (maxLength = length * 2 > 256)
        LogArrange("Preparing large string (>128 chars to ensure maxLength > 256)");
        string input = new string('a', 100) + "HelloWorld" + new string('b', 30);

        // Act
        LogAct("Calling ToKebabCase on large string");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying conversion worked via ArrayPool path");
        result.ShouldContain("hello-world");
        // HelloWorld becomes hello-world (adds 1 separator between Hello and World)
        // But since it's all lowercase 'a' and 'b' around it, only 1 separator is added
        result.Length.ShouldBe(input.Length + 2); // two separators: a...a-hello-world-b...b
    }

    [Fact]
    public void ToKebabCase_LargeStringAllLowercase_ShouldReturnCorrectly()
    {
        // Arrange - large string with no uppercase
        LogArrange("Preparing large lowercase string");
        string input = new string('a', 150);

        // Act
        LogAct("Calling ToKebabCase on large lowercase string");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying no change for all lowercase");
        result.ShouldBe(input);
    }

    [Fact]
    public void ToKebabCase_LargeStringWithTrailingSeparator_ShouldTrimIt()
    {
        // Arrange - large string ending with special char
        LogArrange("Preparing large string with trailing special char");
        string input = new string('a', 150) + "-";

        // Act
        LogAct("Calling ToKebabCase on large string with trailing separator");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying trailing separator removed");
        result.ShouldBe(new string('a', 150));
        result.ShouldNotEndWith("-");
    }

    [Fact]
    public void ToKebabCase_ExactlyAtThreshold_ShouldUseStackAlloc()
    {
        // Arrange - exactly at threshold boundary (128 chars * 2 = 256 = threshold)
        LogArrange("Preparing string at exact threshold boundary");
        string input = new string('a', 128);

        // Act
        LogAct("Calling ToKebabCase at threshold");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying correct conversion at threshold");
        result.ShouldBe(input);
    }

    [Fact]
    public void ToKebabCase_JustAboveThreshold_ShouldUseArrayPool()
    {
        // Arrange - just above threshold (129 chars * 2 = 258 > 256)
        LogArrange("Preparing string just above threshold");
        string input = new string('a', 129);

        // Act
        LogAct("Calling ToKebabCase just above threshold");
        var result = input.ToKebabCase();

        // Assert
        LogAssert("Verifying correct conversion above threshold");
        result.ShouldBe(input);
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

    #region Large String Tests (ArrayPool path)

    [Fact]
    public void ToSnakeCase_LargeString_ShouldUseArrayPoolPath()
    {
        // Arrange - string > 256 chars to trigger ArrayPool path (maxLength = length * 2 > 256)
        LogArrange("Preparing large string (>128 chars to ensure maxLength > 256)");
        string input = new string('A', 129) + "bcd"; // 132 chars, maxLength = 264 > 256

        // Act
        LogAct("Calling ToSnakeCase on large string");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying conversion worked via ArrayPool path");
        result.ShouldStartWith("a_");
        result.ShouldEndWith("bcd");
        result.Length.ShouldBeGreaterThan(input.Length); // underscores added
    }

    [Fact]
    public void ToSnakeCase_LargeStringAllLowercase_ShouldReturnCorrectly()
    {
        // Arrange - large string with no uppercase
        LogArrange("Preparing large lowercase string");
        string input = new string('a', 150);

        // Act
        LogAct("Calling ToSnakeCase on large lowercase string");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying no change for all lowercase");
        result.ShouldBe(input);
    }

    [Fact]
    public void ToSnakeCase_LargeStringWithUppercase_ShouldAddUnderscores()
    {
        // Arrange - large string with uppercase letters
        LogArrange("Preparing large string with uppercase");
        string input = string.Concat(Enumerable.Range(0, 150).Select(i => i % 10 == 0 ? 'A' : 'b'));

        // Act
        LogAct("Calling ToSnakeCase on large string with uppercase");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying underscores added before uppercase");
        result.ShouldContain("_");
        result.ShouldBe(result.ToLowerInvariant()); // all converted to lowercase
    }

    [Fact]
    public void ToSnakeCase_ExactlyAtThreshold_ShouldUseStackAlloc()
    {
        // Arrange - exactly at threshold boundary (128 chars * 2 = 256 = threshold)
        LogArrange("Preparing string at exact threshold boundary");
        string input = new string('a', 128); // maxLength = 256 = StackAllocThreshold

        // Act
        LogAct("Calling ToSnakeCase at threshold");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying correct conversion at threshold");
        result.ShouldBe(input);
    }

    [Fact]
    public void ToSnakeCase_JustAboveThreshold_ShouldUseArrayPool()
    {
        // Arrange - just above threshold (129 chars * 2 = 258 > 256)
        LogArrange("Preparing string just above threshold");
        string input = new string('a', 129);

        // Act
        LogAct("Calling ToSnakeCase just above threshold");
        var result = input.ToSnakeCase();

        // Assert
        LogAssert("Verifying correct conversion above threshold");
        result.ShouldBe(input);
    }

    [Fact]
    public void OnlyLettersAndDigits_LargeString_ShouldUseArrayPoolPath()
    {
        // Arrange - string > 256 chars to trigger ArrayPool path
        LogArrange("Preparing large string with special chars (>256 chars)");
        string input = new string('a', 257) + "!@#" + new string('b', 50);

        // Act
        LogAct("Calling OnlyLettersAndDigits on large string");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying special chars removed via ArrayPool path");
        result.ShouldBe(new string('a', 257) + new string('b', 50));
        result.Length.ShouldBe(307);
    }

    [Fact]
    public void OnlyLettersAndDigits_LargeStringAllSpecialChars_ShouldReturnEmpty()
    {
        // Arrange - large string with only special characters
        LogArrange("Preparing large string with only special chars");
        string input = new string('!', 300);

        // Act
        LogAct("Calling OnlyLettersAndDigits on special chars only");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying empty string returned");
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void OnlyLettersAndDigits_LargeStringAllLetters_ShouldReturnSame()
    {
        // Arrange - large string with only letters
        LogArrange("Preparing large string with only letters");
        string input = new string('x', 300);

        // Act
        LogAct("Calling OnlyLettersAndDigits on letters only");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying same string returned");
        result.ShouldBe(input);
    }

    [Fact]
    public void OnlyLettersAndDigits_ExactlyAtThreshold_ShouldUseStackAlloc()
    {
        // Arrange - exactly at threshold (256 chars)
        LogArrange("Preparing string at exact threshold");
        string input = new string('a', 256);

        // Act
        LogAct("Calling OnlyLettersAndDigits at threshold");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying correct filtering at threshold");
        result.ShouldBe(input);
    }

    [Fact]
    public void OnlyLettersAndDigits_JustAboveThreshold_ShouldUseArrayPool()
    {
        // Arrange - just above threshold (257 chars)
        LogArrange("Preparing string just above threshold");
        string input = new string('a', 257);

        // Act
        LogAct("Calling OnlyLettersAndDigits just above threshold");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying correct filtering above threshold");
        result.ShouldBe(input);
    }

    [Fact]
    public void OnlyLettersAndDigits_LargeStringMixedContent_ShouldFilterCorrectly()
    {
        // Arrange - large string with mixed content
        LogArrange("Preparing large mixed content string");
        var letters = new string('a', 200);
        var digits = new string('1', 100);
        var specials = new string('@', 50);
        string input = letters + specials + digits;

        // Act
        LogAct("Calling OnlyLettersAndDigits on mixed content");
        var result = input.OnlyLettersAndDigits();

        // Assert
        LogAssert("Verifying only letters and digits kept");
        result.ShouldBe(letters + digits);
        result.Length.ShouldBe(300);
    }

    #endregion
}
