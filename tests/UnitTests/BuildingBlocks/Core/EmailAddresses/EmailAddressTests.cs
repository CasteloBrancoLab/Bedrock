using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.EmailAddresses;

public class EmailAddressTests : TestBase
{
    public EmailAddressTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void CreateNew_WithValidEmail_ShouldPreserveValue()
    {
        // Arrange
        LogArrange("Creating email address string");
        const string emailValue = "test@example.com";

        // Act
        LogAct("Creating EmailAddress from string");
        var email = EmailAddress.CreateNew(emailValue);

        // Assert
        LogAssert("Verifying value is preserved");
        email.Value.ShouldBe(emailValue);
        LogInfo("EmailAddress value: {0}", email.Value);
    }

    [Fact]
    public void GetLocalPart_WithValidEmail_ShouldReturnLocalPart()
    {
        // Arrange
        LogArrange("Creating email address");
        var email = EmailAddress.CreateNew("user@domain.com");

        // Act
        LogAct("Getting local part");
        var localPart = email.GetLocalPart();

        // Assert
        LogAssert("Verifying local part is correct");
        localPart.ShouldBe("user");
        LogInfo("Local part: {0}", localPart);
    }

    [Fact]
    public void GetLocalPart_WithNoAtSymbol_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating email address without @ symbol");
        var email = EmailAddress.CreateNew("invalidemail");

        // Act
        LogAct("Getting local part");
        var localPart = email.GetLocalPart();

        // Assert
        LogAssert("Verifying empty string is returned");
        localPart.ShouldBeEmpty();
        LogInfo("Local part for invalid email: '{0}'", localPart);
    }

    [Fact]
    public void GetLocalPart_WithAtAtStart_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating email address with @ at start");
        var email = EmailAddress.CreateNew("@domain.com");

        // Act
        LogAct("Getting local part");
        var localPart = email.GetLocalPart();

        // Assert - Critical: must return empty, not substring before index 0
        LogAssert("Verifying empty string is returned (@ is at index 0)");
        localPart.ShouldBe(string.Empty);
        localPart.Length.ShouldBe(0);
        LogInfo("Local part when @ at start: '{0}'", localPart);
    }

    [Fact]
    public void GetLocalPart_WithSingleCharBeforeAt_ShouldReturnChar()
    {
        // Arrange
        LogArrange("Creating email address with single char before @");
        var email = EmailAddress.CreateNew("a@domain.com");

        // Act
        LogAct("Getting local part");
        var localPart = email.GetLocalPart();

        // Assert - Critical: @ at index 1 should return "a"
        LogAssert("Verifying single char local part is returned");
        localPart.ShouldBe("a");
        localPart.Length.ShouldBe(1);
        LogInfo("Local part with single char: '{0}'", localPart);
    }

    [Fact]
    public void GetDomain_WithValidEmail_ShouldReturnDomain()
    {
        // Arrange
        LogArrange("Creating email address");
        var email = EmailAddress.CreateNew("user@domain.com");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert
        LogAssert("Verifying domain is correct");
        domain.ShouldBe("domain.com");
        LogInfo("Domain: {0}", domain);
    }

    [Fact]
    public void GetDomain_WithNoAtSymbol_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating email address without @ symbol");
        var email = EmailAddress.CreateNew("invalidemail");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert
        LogAssert("Verifying empty string is returned");
        domain.ShouldBeEmpty();
        LogInfo("Domain for invalid email: '{0}'", domain);
    }

    [Fact]
    public void GetDomain_WithAtAtEnd_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating email address with @ at end");
        var email = EmailAddress.CreateNew("user@");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert
        LogAssert("Verifying empty string is returned");
        domain.ShouldBeEmpty();
        LogInfo("Domain when @ at end: '{0}'", domain);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        LogArrange("Creating email address");
        const string emailValue = "test@example.com";
        var email = EmailAddress.CreateNew(emailValue);

        // Act
        LogAct("Calling ToString");
        var result = email.ToString();

        // Assert
        LogAssert("Verifying ToString returns value");
        result.ShouldBe(emailValue);
        LogInfo("ToString result: {0}", result);
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        LogArrange("Creating email address");
        var email = EmailAddress.CreateNew("test@example.com");

        // Act
        LogAct("Getting hash code multiple times");
        var hash1 = email.GetHashCode();
        var hash2 = email.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are consistent");
        hash1.ShouldBe(hash2);
        LogInfo("Hash code is consistent: {0}", hash1);
    }

    [Fact]
    public void GetHashCode_ForSameEmailDifferentCase_ShouldBeEqual()
    {
        // Arrange
        LogArrange("Creating email addresses with different case");
        var email1 = EmailAddress.CreateNew("Test@Example.COM");
        var email2 = EmailAddress.CreateNew("test@example.com");

        // Act
        LogAct("Getting hash codes");
        var hash1 = email1.GetHashCode();
        var hash2 = email2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal for case-insensitive match");
        hash1.ShouldBe(hash2);
        LogInfo("Hash codes match: {0} == {1}", hash1, hash2);
    }

    [Fact]
    public void GetHashCode_WithNullValue_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Creating EmailAddress with null value");
        var email = EmailAddress.CreateNew(null!);

        // Act
        LogAct("Getting hash code for null value");
        var hashCode = email.GetHashCode();

        // Assert
        LogAssert("Verifying hash code is 0 for null");
        hashCode.ShouldBe(0);
        LogInfo("Hash code for null: {0}", hashCode);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two EmailAddresses with same value");
        var email1 = EmailAddress.CreateNew("test@example.com");
        var email2 = EmailAddress.CreateNew("test@example.com");

        // Act
        LogAct("Comparing EmailAddresses for equality");
        var areEqual = email1.Equals(email2);

        // Assert
        LogAssert("Verifying EmailAddresses are equal");
        areEqual.ShouldBeTrue();
        LogInfo("EmailAddresses with same value are equal");
    }

    [Fact]
    public void Equals_WithDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two EmailAddresses with different case");
        var email1 = EmailAddress.CreateNew("Test@Example.COM");
        var email2 = EmailAddress.CreateNew("test@example.com");

        // Act
        LogAct("Comparing EmailAddresses for case-insensitive equality");
        var areEqual = email1.Equals(email2);

        // Assert
        LogAssert("Verifying EmailAddresses are equal (case-insensitive)");
        areEqual.ShouldBeTrue();
        LogInfo("EmailAddresses with different case are equal");
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two EmailAddresses with different values");
        var email1 = EmailAddress.CreateNew("test1@example.com");
        var email2 = EmailAddress.CreateNew("test2@example.com");

        // Act
        LogAct("Comparing EmailAddresses for equality");
        var areEqual = email1.Equals(email2);

        // Assert
        LogAssert("Verifying EmailAddresses are not equal");
        areEqual.ShouldBeFalse();
        LogInfo("EmailAddresses with different values are not equal");
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWork()
    {
        // Arrange
        LogArrange("Creating EmailAddress and object for equality test");
        var email = EmailAddress.CreateNew("test@example.com");
        object objSame = EmailAddress.CreateNew("test@example.com");
        object objDifferent = EmailAddress.CreateNew("other@example.com");
        object? objNull = null;
        object objWrongType = "not an EmailAddress";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        email.Equals(objSame).ShouldBeTrue("Equal EmailAddress objects should be equal");
        email.Equals(objDifferent).ShouldBeFalse("Different EmailAddress objects should not be equal");
        email.Equals(objNull).ShouldBeFalse("Null should not be equal");
        email.Equals(objWrongType).ShouldBeFalse("Wrong type should not be equal");

        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two EmailAddresses with same value");
        var email1 = EmailAddress.CreateNew("test@example.com");
        var email2 = EmailAddress.CreateNew("test@example.com");

        // Act & Assert
        LogAct("Testing equality operator");
        (email1 == email2).ShouldBeTrue();
        LogAssert("Equality operator works correctly");
    }

    [Fact]
    public void EqualityOperator_WithDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two EmailAddresses with different case");
        var email1 = EmailAddress.CreateNew("TEST@EXAMPLE.COM");
        var email2 = EmailAddress.CreateNew("test@example.com");

        // Act & Assert
        LogAct("Testing equality operator with different case");
        (email1 == email2).ShouldBeTrue();
        LogAssert("Equality operator is case-insensitive");
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two EmailAddresses with different values");
        var email1 = EmailAddress.CreateNew("test1@example.com");
        var email2 = EmailAddress.CreateNew("test2@example.com");

        // Act & Assert
        LogAct("Testing inequality operator");
        (email1 != email2).ShouldBeTrue();
        LogAssert("Inequality operator works correctly");
    }

    [Fact]
    public void InequalityOperator_WithSameValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two EmailAddresses with same value");
        var email1 = EmailAddress.CreateNew("test@example.com");
        var email2 = EmailAddress.CreateNew("test@example.com");

        // Act & Assert
        LogAct("Testing inequality operator with same value");
        (email1 != email2).ShouldBeFalse();
        LogAssert("Inequality operator returns false for equal values");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        LogArrange("Creating an EmailAddress");
        const string expectedValue = "test@example.com";
        var email = EmailAddress.CreateNew(expectedValue);

        // Act
        LogAct("Implicitly converting EmailAddress to string");
        string result = email;

        // Assert
        LogAssert("Verifying conversion preserves value");
        result.ShouldBe(expectedValue);
        LogInfo("Implicit conversion to string successful");
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        // Arrange
        LogArrange("Creating a string");
        const string emailString = "test@example.com";

        // Act
        LogAct("Implicitly converting string to EmailAddress");
        EmailAddress email = emailString;

        // Assert
        LogAssert("Verifying conversion preserves value");
        email.Value.ShouldBe(emailString);
        LogInfo("Implicit conversion from string successful");
    }

    [Fact]
    public void GetLocalPart_WithMultipleAtSymbols_ShouldReturnPartBeforeFirst()
    {
        // Arrange
        LogArrange("Creating email address with multiple @ symbols");
        var email = EmailAddress.CreateNew("user@name@domain.com");

        // Act
        LogAct("Getting local part");
        var localPart = email.GetLocalPart();

        // Assert
        LogAssert("Verifying returns part before first @");
        localPart.ShouldBe("user");
        LogInfo("Local part with multiple @: {0}", localPart);
    }

    [Fact]
    public void GetDomain_WithMultipleAtSymbols_ShouldReturnPartAfterFirst()
    {
        // Arrange
        LogArrange("Creating email address with multiple @ symbols");
        var email = EmailAddress.CreateNew("user@name@domain.com");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert
        LogAssert("Verifying returns part after first @");
        domain.ShouldBe("name@domain.com");
        LogInfo("Domain with multiple @: {0}", domain);
    }

    [Fact]
    public void GetLocalPart_WithEmptyString_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating email address with empty string");
        var email = EmailAddress.CreateNew(string.Empty);

        // Act
        LogAct("Getting local part");
        var localPart = email.GetLocalPart();

        // Assert
        LogAssert("Verifying empty string is returned");
        localPart.ShouldBeEmpty();
        LogInfo("Local part for empty: '{0}'", localPart);
    }

    [Fact]
    public void GetDomain_WithEmptyString_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating email address with empty string");
        var email = EmailAddress.CreateNew(string.Empty);

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert
        LogAssert("Verifying empty string is returned");
        domain.ShouldBeEmpty();
        LogInfo("Domain for empty: '{0}'", domain);
    }

    [Fact]
    public void GetDomain_WithOnlyAtSymbol_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating email address with only @ symbol");
        var email = EmailAddress.CreateNew("@");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert
        LogAssert("Verifying empty string is returned (nothing after @)");
        domain.ShouldBeEmpty();
        LogInfo("Domain for '@': '{0}'", domain);
    }

    [Fact]
    public void GetDomain_WithAtAtExactlyLastPosition_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Creating email address with @ at last position");
        var email = EmailAddress.CreateNew("user@");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert
        LogAssert("Verifying empty when @ is at last position");
        domain.ShouldBeEmpty();
        LogInfo("Domain with @ at end: '{0}'", domain);
    }

    [Fact]
    public void GetDomain_WithSingleCharDomain_ShouldReturnDomain()
    {
        // Arrange
        LogArrange("Creating email address with single char domain");
        var email = EmailAddress.CreateNew("user@a");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert
        LogAssert("Verifying single char domain is returned");
        domain.ShouldBe("a");
        LogInfo("Domain with single char: '{0}'", domain);
    }

    [Fact]
    public void GetHashCode_WithDefaultStruct_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Creating default EmailAddress struct");
        var email = default(EmailAddress);

        // Act
        LogAct("Getting hash code for default struct");
        var hashCode = email.GetHashCode();

        // Assert
        LogAssert("Verifying hash code is 0 for default (null Value)");
        hashCode.ShouldBe(0);
        LogInfo("Hash code for default struct: {0}", hashCode);
    }

    [Fact]
    public void GetHashCode_WithNonNullValue_ShouldNotReturnZero()
    {
        // Arrange
        LogArrange("Creating EmailAddress with non-null value");
        var email = EmailAddress.CreateNew("test@example.com");

        // Act
        LogAct("Getting hash code");
        var hashCode = email.GetHashCode();

        // Assert - Critical: non-null value should produce non-zero hash
        LogAssert("Verifying hash code is not 0 for valid email");
        hashCode.ShouldNotBe(0);
        LogInfo("Hash code for valid email: {0}", hashCode);
    }

    [Fact]
    public void GetDomain_WithAtAtIndexZeroAndContent_ShouldReturnDomain()
    {
        // Arrange
        LogArrange("Creating email with @ at index 0 followed by content");
        var email = EmailAddress.CreateNew("@test");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert - Critical: atIndex >= 0 condition, when @ at index 0
        LogAssert("Verifying domain is returned when @ is at index 0");
        domain.ShouldBe("test");
        LogInfo("Domain when @ at index 0: '{0}'", domain);
    }

    [Fact]
    public void GetDomain_WithAtBeforeLastChar_ShouldReturnDomain()
    {
        // Arrange
        LogArrange("Creating email with @ one position before end");
        var email = EmailAddress.CreateNew("u@x");

        // Act
        LogAct("Getting domain");
        var domain = email.GetDomain();

        // Assert - Critical: tests atIndex < Value.Length - 1 boundary
        LogAssert("Verifying domain is returned");
        domain.ShouldBe("x");
        domain.Length.ShouldBe(1);
        LogInfo("Domain: '{0}'", domain);
    }
}
