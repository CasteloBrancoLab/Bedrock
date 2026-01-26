using Bedrock.BuildingBlocks.Core.PhoneNumbers;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Core.PhoneNumbers;

public class PhoneNumberTests : TestBase
{
    public PhoneNumberTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void CreateNew_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up phone number components");
        var countryCode = "55";
        var areaCode = "11";
        var number = "999998888";

        // Act
        LogAct("Creating PhoneNumber");
        var phone = PhoneNumber.CreateNew(countryCode, areaCode, number);

        // Assert
        LogAssert("Verifying all properties are set");
        phone.CountryCode.ShouldBe(countryCode);
        phone.AreaCode.ShouldBe(areaCode);
        phone.Number.ShouldBe(number);
        LogInfo("PhoneNumber created: {0}", phone);
    }

    [Fact]
    public void CreateNew_WithUSNumber_ShouldWork()
    {
        // Arrange
        LogArrange("Setting up US phone number");
        var countryCode = "1";
        var areaCode = "212";
        var number = "5551234";

        // Act
        LogAct("Creating US PhoneNumber");
        var phone = PhoneNumber.CreateNew(countryCode, areaCode, number);

        // Assert
        LogAssert("Verifying US phone number");
        phone.CountryCode.ShouldBe("1");
        phone.AreaCode.ShouldBe("212");
        phone.Number.ShouldBe("5551234");
        LogInfo("US PhoneNumber: {0}", phone);
    }

    [Fact]
    public void ToFormattedString_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for formatting");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act
        LogAct("Calling ToFormattedString");
        var formatted = phone.ToFormattedString();

        // Assert
        LogAssert("Verifying formatted string");
        formatted.ShouldBe("+55 (11) 999998888");
        LogInfo("Formatted: {0}", formatted);
    }

    [Fact]
    public void ToFormattedString_WithUSNumber_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating US PhoneNumber for formatting");
        var phone = PhoneNumber.CreateNew("1", "212", "5551234");

        // Act
        LogAct("Calling ToFormattedString");
        var formatted = phone.ToFormattedString();

        // Assert
        LogAssert("Verifying formatted string");
        formatted.ShouldBe("+1 (212) 5551234");
        LogInfo("Formatted: {0}", formatted);
    }

    [Fact]
    public void ToE164String_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for E.164");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act
        LogAct("Calling ToE164String");
        var e164 = phone.ToE164String();

        // Assert
        LogAssert("Verifying E.164 string");
        e164.ShouldBe("+5511999998888");
        LogInfo("E.164: {0}", e164);
    }

    [Fact]
    public void ToE164String_WithUSNumber_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating US PhoneNumber for E.164");
        var phone = PhoneNumber.CreateNew("1", "212", "5551234");

        // Act
        LogAct("Calling ToE164String");
        var e164 = phone.ToE164String();

        // Assert
        LogAssert("Verifying E.164 string");
        e164.ShouldBe("+12125551234");
        LogInfo("E.164: {0}", e164);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        LogArrange("Creating PhoneNumber");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act
        LogAct("Calling ToString");
        var result = phone.ToString();

        // Assert
        LogAssert("Verifying ToString returns formatted string");
        result.ShouldBe("+55 (11) 999998888");
        result.ShouldBe(phone.ToFormattedString());
        LogInfo("ToString: {0}", result);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two PhoneNumbers with same values");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act
        LogAct("Comparing for equality");
        var areEqual = phone1.Equals(phone2);

        // Assert
        LogAssert("Verifying equality");
        areEqual.ShouldBeTrue();
        LogInfo("PhoneNumbers are equal");
    }

    [Fact]
    public void Equals_WithDifferentCountryCode_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PhoneNumbers with different country codes");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("1", "11", "999998888");

        // Act
        LogAct("Comparing for equality");
        var areEqual = phone1.Equals(phone2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
        LogInfo("PhoneNumbers with different country codes are not equal");
    }

    [Fact]
    public void Equals_WithDifferentAreaCode_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PhoneNumbers with different area codes");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "21", "999998888");

        // Act
        LogAct("Comparing for equality");
        var areEqual = phone1.Equals(phone2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
        LogInfo("PhoneNumbers with different area codes are not equal");
    }

    [Fact]
    public void Equals_WithDifferentNumber_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PhoneNumbers with different numbers");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "11", "888887777");

        // Act
        LogAct("Comparing for equality");
        var areEqual = phone1.Equals(phone2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
        LogInfo("PhoneNumbers with different numbers are not equal");
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWork()
    {
        // Arrange
        LogArrange("Creating PhoneNumber and object for equality test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        object objSame = PhoneNumber.CreateNew("55", "11", "999998888");
        object objDifferent = PhoneNumber.CreateNew("55", "11", "888887777");
        object? objNull = null;
        object objWrongType = "not a phone";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        phone.Equals(objSame).ShouldBeTrue("Equal PhoneNumber objects should be equal");
        phone.Equals(objDifferent).ShouldBeFalse("Different PhoneNumber objects should not be equal");
        phone.Equals(objNull).ShouldBeFalse("Null should not be equal");
        phone.Equals(objWrongType).ShouldBeFalse("Wrong type should not be equal");

        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void EqualityOperator_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two PhoneNumbers with same values");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act & Assert
        LogAct("Testing equality operator");
        (phone1 == phone2).ShouldBeTrue();
        LogAssert("Equality operator works correctly");
    }

    [Fact]
    public void EqualityOperator_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two different PhoneNumbers");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "21", "888887777");

        // Act & Assert
        LogAct("Testing equality operator");
        (phone1 == phone2).ShouldBeFalse();
        LogAssert("Equality operator correctly returns false for different values");
    }

    [Fact]
    public void InequalityOperator_WithSameValues_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating two PhoneNumbers with same values");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act & Assert
        LogAct("Testing inequality operator");
        (phone1 != phone2).ShouldBeFalse();
        LogAssert("Inequality operator correctly returns false for same values");
    }

    [Fact]
    public void InequalityOperator_WithDifferentValues_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two different PhoneNumbers");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "21", "888887777");

        // Act & Assert
        LogAct("Testing inequality operator");
        (phone1 != phone2).ShouldBeTrue();
        LogAssert("Inequality operator works correctly");
    }

    [Fact]
    public void GetHashCode_SameValues_ShouldReturnSameHash()
    {
        // Arrange
        LogArrange("Creating two PhoneNumbers with same values");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act
        LogAct("Getting hash codes");
        var hash1 = phone1.GetHashCode();
        var hash2 = phone2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal");
        hash1.ShouldBe(hash2);
        LogInfo("Hash codes: {0} == {1}", hash1, hash2);
    }

    [Fact]
    public void GetHashCode_DifferentValues_ShouldReturnDifferentHash()
    {
        // Arrange
        LogArrange("Creating two different PhoneNumbers");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("55", "21", "888887777");

        // Act
        LogAct("Getting hash codes");
        var hash1 = phone1.GetHashCode();
        var hash2 = phone2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are different");
        hash1.ShouldNotBe(hash2);
        LogInfo("Hash codes: {0} != {1}", hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        LogArrange("Creating PhoneNumber");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act
        LogAct("Getting hash code multiple times");
        var hash1 = phone.GetHashCode();
        var hash2 = phone.GetHashCode();

        // Assert
        LogAssert("Verifying hash code is consistent");
        hash1.ShouldBe(hash2);
        LogInfo("Hash code is consistent: {0}", hash1);
    }

    [Fact]
    public void GetHashCode_ShouldCombineAllFields()
    {
        // Arrange - criar phones que diferem apenas em um campo
        LogArrange("Creating PhoneNumbers differing in single fields");
        var basePhone = PhoneNumber.CreateNew("55", "11", "999998888");
        var diffCountry = PhoneNumber.CreateNew("1", "11", "999998888");
        var diffArea = PhoneNumber.CreateNew("55", "21", "999998888");
        var diffNumber = PhoneNumber.CreateNew("55", "11", "888887777");

        // Act
        LogAct("Getting hash codes");
        var baseHash = basePhone.GetHashCode();
        var countryHash = diffCountry.GetHashCode();
        var areaHash = diffArea.GetHashCode();
        var numberHash = diffNumber.GetHashCode();

        // Assert
        LogAssert("Verifying all fields contribute to hash");
        baseHash.ShouldNotBe(countryHash, "CountryCode should affect hash");
        baseHash.ShouldNotBe(areaHash, "AreaCode should affect hash");
        baseHash.ShouldNotBe(numberHash, "Number should affect hash");
        LogInfo("All fields contribute to hash code");
    }

    [Fact]
    public void CreateNew_WithEmptyStrings_ShouldWork()
    {
        // Arrange
        LogArrange("Creating PhoneNumber with empty strings");

        // Act
        LogAct("Creating PhoneNumber");
        var phone = PhoneNumber.CreateNew("", "", "");

        // Assert
        LogAssert("Verifying empty strings are accepted");
        phone.CountryCode.ShouldBe("");
        phone.AreaCode.ShouldBe("");
        phone.Number.ShouldBe("");
        LogInfo("Empty strings accepted");
    }

    [Fact]
    public void ToFormattedString_WithEmptyStrings_ShouldReturnFormat()
    {
        // Arrange
        LogArrange("Creating PhoneNumber with empty strings");
        var phone = PhoneNumber.CreateNew("", "", "");

        // Act
        LogAct("Calling ToFormattedString");
        var formatted = phone.ToFormattedString();

        // Assert
        LogAssert("Verifying format with empty strings");
        formatted.ShouldBe("+ () ");
        LogInfo("Formatted with empty: '{0}'", formatted);
    }

    [Fact]
    public void ToE164String_WithEmptyStrings_ShouldReturnPlusOnly()
    {
        // Arrange
        LogArrange("Creating PhoneNumber with empty strings");
        var phone = PhoneNumber.CreateNew("", "", "");

        // Act
        LogAct("Calling ToE164String");
        var e164 = phone.ToE164String();

        // Assert
        LogAssert("Verifying E.164 with empty strings");
        e164.ShouldBe("+");
        LogInfo("E.164 with empty: '{0}'", e164);
    }

    [Fact]
    public void Equals_AllFieldsMustMatch()
    {
        // Este teste mata mutantes que removem condições do Equals

        // Arrange
        LogArrange("Criando PhoneNumbers para verificar todas as condições do Equals");
        var reference = PhoneNumber.CreateNew("55", "11", "999998888");

        // Testar cada campo individualmente
        var diffCountryOnly = PhoneNumber.CreateNew("1", "11", "999998888");
        var diffAreaOnly = PhoneNumber.CreateNew("55", "21", "999998888");
        var diffNumberOnly = PhoneNumber.CreateNew("55", "11", "111111111");

        // Act & Assert
        LogAct("Verificando que cada campo afeta igualdade");
        reference.Equals(diffCountryOnly).ShouldBeFalse("CountryCode diferente deve retornar false");
        reference.Equals(diffAreaOnly).ShouldBeFalse("AreaCode diferente deve retornar false");
        reference.Equals(diffNumberOnly).ShouldBeFalse("Number diferente deve retornar false");

        LogAssert("Todas as condicoes do Equals verificadas");
    }

    [Fact]
    public void InequalityOperator_ShouldNegateEquals()
    {
        // Mata mutante: !left.Equals(right) -> left.Equals(right)

        // Arrange
        LogArrange("Criando PhoneNumbers para verificar operador !=");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2Same = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone3Diff = PhoneNumber.CreateNew("55", "21", "888887777");

        // Act & Assert
        LogAct("Verificando que != e a negacao de ==");

        // Para valores iguais: == true, != false
        (phone1 == phone2Same).ShouldBeTrue();
        (phone1 != phone2Same).ShouldBeFalse();

        // Para valores diferentes: == false, != true
        (phone1 == phone3Diff).ShouldBeFalse();
        (phone1 != phone3Diff).ShouldBeTrue();

        LogAssert("Operador != corretamente nega ==");
    }

    [Fact]
    public void ToStringWithFormat_WithEFormat_ShouldReturnE164()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for format test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act
        LogAct("Calling ToString with E format");
        var resultE = phone.ToString("E", null);
        var resultLowerE = phone.ToString("e", null);

        // Assert
        LogAssert("Verifying E format returns E.164");
        resultE.ShouldBe("+5511999998888");
        resultLowerE.ShouldBe("+5511999998888");
        LogInfo("E format: {0}", resultE);
    }

    [Fact]
    public void ToStringWithFormat_WithNullOrOtherFormat_ShouldReturnFormatted()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for format test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");

        // Act
        LogAct("Calling ToString with null and other formats");
        var resultNull = phone.ToString(null, null);
        var resultG = phone.ToString("G", null);
        var resultOther = phone.ToString("X", null);

        // Assert
        LogAssert("Verifying non-E formats return formatted string");
        resultNull.ShouldBe("+55 (11) 999998888");
        resultG.ShouldBe("+55 (11) 999998888");
        resultOther.ShouldBe("+55 (11) 999998888");
        LogInfo("Default format: {0}", resultNull);
    }

    [Fact]
    public void TryFormat_WithSufficientBuffer_ShouldReturnTrueAndWriteFormatted()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for TryFormat test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        Span<char> buffer = stackalloc char[50];

        // Act
        LogAct("Calling TryFormat with sufficient buffer");
        var result = phone.TryFormat(buffer, out var charsWritten, default, null);

        // Assert
        LogAssert("Verifying TryFormat succeeds");
        result.ShouldBeTrue();
        charsWritten.ShouldBe(18); // "+55 (11) 999998888".Length
        buffer[..charsWritten].ToString().ShouldBe("+55 (11) 999998888");
        LogInfo("TryFormat result: {0}", buffer[..charsWritten].ToString());
    }

    [Fact]
    public void TryFormat_WithEFormat_ShouldReturnE164()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for TryFormat E.164 test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        Span<char> buffer = stackalloc char[50];

        // Act
        LogAct("Calling TryFormat with E format");
        var resultE = phone.TryFormat(buffer, out var charsWrittenE, "E".AsSpan(), null);
        var resultLowerE = phone.TryFormat(buffer, out var charsWrittenLowerE, "e".AsSpan(), null);

        // Assert
        LogAssert("Verifying TryFormat E format");
        resultE.ShouldBeTrue();
        charsWrittenE.ShouldBe(14); // "+5511999998888".Length
        buffer[..charsWrittenE].ToString().ShouldBe("+5511999998888");

        resultLowerE.ShouldBeTrue();
        charsWrittenLowerE.ShouldBe(14);
        buffer[..charsWrittenLowerE].ToString().ShouldBe("+5511999998888");
        LogInfo("TryFormat E.164: {0}", buffer[..charsWrittenE].ToString());
    }

    [Fact]
    public void TryFormat_WithInsufficientBuffer_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for insufficient buffer test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        Span<char> smallBuffer = stackalloc char[5];

        // Act
        LogAct("Calling TryFormat with insufficient buffer");
        var result = phone.TryFormat(smallBuffer, out var charsWritten, default, null);

        // Assert
        LogAssert("Verifying TryFormat fails with small buffer");
        result.ShouldBeFalse();
        charsWritten.ShouldBe(0);
        LogInfo("TryFormat correctly failed with insufficient buffer");
    }

    [Fact]
    public void TryFormat_WithInsufficientBufferForE164_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for insufficient E.164 buffer test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        Span<char> smallBuffer = stackalloc char[10];

        // Act
        LogAct("Calling TryFormat with E format and insufficient buffer");
        var result = phone.TryFormat(smallBuffer, out var charsWritten, "E".AsSpan(), null);

        // Assert
        LogAssert("Verifying TryFormat E.164 fails with small buffer");
        result.ShouldBeFalse();
        charsWritten.ShouldBe(0);
        LogInfo("TryFormat E.164 correctly failed with insufficient buffer");
    }

    [Fact]
    public void TryFormat_WithExactBufferSize_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for exact buffer size test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        var expectedFormatted = "+55 (11) 999998888";
        var expectedE164 = "+5511999998888";
        Span<char> exactFormattedBuffer = stackalloc char[expectedFormatted.Length];
        Span<char> exactE164Buffer = stackalloc char[expectedE164.Length];

        // Act
        LogAct("Calling TryFormat with exact buffer sizes");
        var resultFormatted = phone.TryFormat(exactFormattedBuffer, out var charsWrittenFormatted, default, null);
        var resultE164 = phone.TryFormat(exactE164Buffer, out var charsWrittenE164, "E".AsSpan(), null);

        // Assert
        LogAssert("Verifying TryFormat succeeds with exact buffer");
        resultFormatted.ShouldBeTrue();
        charsWrittenFormatted.ShouldBe(expectedFormatted.Length);
        exactFormattedBuffer.ToString().ShouldBe(expectedFormatted);

        resultE164.ShouldBeTrue();
        charsWrittenE164.ShouldBe(expectedE164.Length);
        exactE164Buffer.ToString().ShouldBe(expectedE164);
        LogInfo("TryFormat works with exact buffer sizes");
    }

    [Fact]
    public void TryFormat_WithEmptyStrings_ShouldWork()
    {
        // Arrange
        LogArrange("Creating PhoneNumber with empty strings for TryFormat");
        var phone = PhoneNumber.CreateNew("", "", "");
        Span<char> buffer = stackalloc char[10];

        // Act
        LogAct("Calling TryFormat with empty phone");
        var resultFormatted = phone.TryFormat(buffer, out var charsWrittenFormatted, default, null);
        var resultE164 = phone.TryFormat(buffer, out var charsWrittenE164, "E".AsSpan(), null);

        // Assert
        LogAssert("Verifying TryFormat with empty strings");
        resultFormatted.ShouldBeTrue();
        charsWrittenFormatted.ShouldBe(5); // "+ () ".Length
        buffer[..charsWrittenFormatted].ToString().ShouldBe("+ () ");

        resultE164.ShouldBeTrue();
        charsWrittenE164.ShouldBe(1); // "+".Length
        buffer[..charsWrittenE164].ToString().ShouldBe("+");
        LogInfo("TryFormat works with empty strings");
    }

    [Fact]
    public void TryFormat_ShouldWriteCorrectCharacters()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for character verification");
        var phone = PhoneNumber.CreateNew("1", "212", "5551234");
        Span<char> buffer = stackalloc char[50];

        // Act
        LogAct("Calling TryFormat and verifying each character");
        var result = phone.TryFormat(buffer, out var charsWritten, default, null);
        var formatted = buffer[..charsWritten].ToString();

        // Assert
        LogAssert("Verifying each character position");
        result.ShouldBeTrue();
        formatted.ShouldBe("+1 (212) 5551234");

        // Verify specific characters to kill mutants
        formatted[0].ShouldBe('+');
        formatted[1].ShouldBe('1');
        formatted[2].ShouldBe(' ');
        formatted[3].ShouldBe('(');
        formatted[4].ShouldBe('2');
        formatted[5].ShouldBe('1');
        formatted[6].ShouldBe('2');
        formatted[7].ShouldBe(')');
        formatted[8].ShouldBe(' ');
        LogInfo("All characters verified: {0}", formatted);
    }

    [Fact]
    public void TryFormat_E164_ShouldWriteCorrectCharacters()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for E.164 character verification");
        var phone = PhoneNumber.CreateNew("1", "212", "5551234");
        Span<char> buffer = stackalloc char[50];

        // Act
        LogAct("Calling TryFormat E.164 and verifying");
        var result = phone.TryFormat(buffer, out var charsWritten, "E".AsSpan(), null);
        var e164 = buffer[..charsWritten].ToString();

        // Assert
        LogAssert("Verifying E.164 format");
        result.ShouldBeTrue();
        e164.ShouldBe("+12125551234");

        // Verify there are no spaces or parentheses
        e164.ShouldNotContain(" ");
        e164.ShouldNotContain("(");
        e164.ShouldNotContain(")");
        LogInfo("E.164 verified: {0}", e164);
    }

    [Fact]
    public void TryFormat_CharsWritten_ShouldMatchActualLength()
    {
        // Arrange
        LogArrange("Creating PhoneNumbers for charsWritten verification");
        var phone1 = PhoneNumber.CreateNew("55", "11", "999998888");
        var phone2 = PhoneNumber.CreateNew("1", "2", "3");
        Span<char> buffer = stackalloc char[50];

        // Act & Assert
        LogAct("Verifying charsWritten matches actual content");

        phone1.TryFormat(buffer, out var chars1, default, null);
        chars1.ShouldBe(buffer[..chars1].ToString().Length);

        phone1.TryFormat(buffer, out var chars1E164, "E".AsSpan(), null);
        chars1E164.ShouldBe(buffer[..chars1E164].ToString().Length);

        phone2.TryFormat(buffer, out var chars2, default, null);
        chars2.ShouldBe(buffer[..chars2].ToString().Length);

        phone2.TryFormat(buffer, out var chars2E164, "E".AsSpan(), null);
        chars2E164.ShouldBe(buffer[..chars2E164].ToString().Length);

        LogAssert("charsWritten correctly reports actual length");
    }

    [Fact]
    public void TryFormat_ShouldProduceConsistentResultWithToString()
    {
        // Arrange
        LogArrange("Creating PhoneNumber for consistency test");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        Span<char> buffer = stackalloc char[50];

        // Act
        LogAct("Comparing TryFormat with ToString methods");
        phone.TryFormat(buffer, out var charsFormatted, default, null);
        var tryFormatResult = buffer[..charsFormatted].ToString();

        phone.TryFormat(buffer, out var charsE164, "E".AsSpan(), null);
        var tryFormatE164Result = buffer[..charsE164].ToString();

        // Assert
        LogAssert("Verifying consistency");
        tryFormatResult.ShouldBe(phone.ToFormattedString());
        tryFormatResult.ShouldBe(phone.ToString());
        tryFormatE164Result.ShouldBe(phone.ToE164String());
        tryFormatE164Result.ShouldBe(phone.ToString("E", null));
        LogInfo("TryFormat is consistent with ToString methods");
    }

    [Fact]
    public void TryFormat_FormatLength_ShouldOnlyAcceptSingleChar()
    {
        // Testa que format.Length == 1 é verificado
        // Arrange
        LogArrange("Testing format length validation");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        Span<char> bufferEmpty = stackalloc char[50];
        Span<char> bufferSingle = stackalloc char[50];
        Span<char> bufferMultiple = stackalloc char[50];

        // Act
        LogAct("Testing various format lengths");
        phone.TryFormat(bufferEmpty, out var charsEmpty, "".AsSpan(), null);
        phone.TryFormat(bufferSingle, out var charsSingle, "E".AsSpan(), null);
        phone.TryFormat(bufferMultiple, out var charsMultiple, "EE".AsSpan(), null);

        // Assert
        LogAssert("Verifying format length behavior");
        // Empty format should use default (formatted)
        bufferEmpty[..charsEmpty].ToString().ShouldBe("+55 (11) 999998888");
        // Single char E should use E.164
        bufferSingle[..charsSingle].ToString().ShouldBe("+5511999998888");
        // Multiple chars should use default (formatted)
        bufferMultiple[..charsMultiple].ToString().ShouldBe("+55 (11) 999998888");
        LogInfo("Format length validation works correctly");
    }

    [Fact]
    public void TryFormat_WithOneLessThanRequired_ShouldReturnFalse()
    {
        // Teste para matar mutantes aritméticos na linha 153
        // O cálculo: 1 + CountryCode.Length + 2 + AreaCode.Length + 2 + Number.Length
        // Para "+55 (11) 999998888" = 1 + 2 + 2 + 2 + 2 + 9 = 18 caracteres
        // Arrange
        LogArrange("Testing buffer one char smaller than required");
        var phone = PhoneNumber.CreateNew("55", "11", "999998888");
        var expectedFormatted = "+55 (11) 999998888";
        var expectedE164 = "+5511999998888";

        // Buffer exatamente 1 menor que o necessário deve falhar
        Span<char> bufferFormattedSmall = stackalloc char[expectedFormatted.Length - 1];
        Span<char> bufferE164Small = stackalloc char[expectedE164.Length - 1];

        // Act
        LogAct("Testing with buffers one char too small");
        var resultFormatted = phone.TryFormat(bufferFormattedSmall, out var charsFormatted, default, null);
        var resultE164 = phone.TryFormat(bufferE164Small, out var charsE164, "E".AsSpan(), null);

        // Assert
        LogAssert("Verifying TryFormat fails with insufficient buffer");
        resultFormatted.ShouldBeFalse("Buffer one char smaller should fail for formatted");
        charsFormatted.ShouldBe(0);
        resultE164.ShouldBeFalse("Buffer one char smaller should fail for E.164");
        charsE164.ShouldBe(0);
        LogInfo("TryFormat correctly rejects buffers one char too small");
    }

    [Fact]
    public void TryFormat_RequiredLengthCalculation_ShouldBeExact()
    {
        // Teste detalhado para validar o cálculo exato de requiredLength
        // Mata mutantes que alterem qualquer parte do cálculo aritmético
        // Arrange
        LogArrange("Testing exact required length calculation");

        // Casos com diferentes tamanhos para verificar cada componente
        var cases = new[]
        {
            (country: "1", area: "2", number: "3", expectedFormatted: 8, expectedE164: 4),      // 1+1+2+1+2+1 = 8, 1+1+1+1 = 4
            (country: "55", area: "11", number: "9", expectedFormatted: 10, expectedE164: 6),   // 1+2+2+2+2+1 = 10, 1+2+2+1 = 6
            (country: "1", area: "212", number: "5551234", expectedFormatted: 16, expectedE164: 12), // 1+1+2+3+2+7 = 16, 1+1+3+7 = 12
        };

        foreach (var (country, area, number, expectedFormatted, expectedE164) in cases)
        {
            var phone = PhoneNumber.CreateNew(country, area, number);

            // Buffer exato deve funcionar
            Span<char> exactBufferFormatted = stackalloc char[expectedFormatted];
            Span<char> exactBufferE164 = stackalloc char[expectedE164];

            // Act
            LogAct($"Testing phone {country}/{area}/{number}");
            var resultFormatted = phone.TryFormat(exactBufferFormatted, out var charsFormatted, default, null);
            var resultE164 = phone.TryFormat(exactBufferE164, out var charsE164, "E".AsSpan(), null);

            // Assert
            resultFormatted.ShouldBeTrue($"Exact buffer should work for formatted ({country}/{area}/{number})");
            charsFormatted.ShouldBe(expectedFormatted, $"Chars written should be exact for formatted ({country}/{area}/{number})");

            resultE164.ShouldBeTrue($"Exact buffer should work for E.164 ({country}/{area}/{number})");
            charsE164.ShouldBe(expectedE164, $"Chars written should be exact for E.164 ({country}/{area}/{number})");

            // Buffer 1 menor deve falhar
            Span<char> smallBufferFormatted = stackalloc char[expectedFormatted - 1];
            Span<char> smallBufferE164 = stackalloc char[expectedE164 - 1];

            var failFormatted = phone.TryFormat(smallBufferFormatted, out _, default, null);
            var failE164 = phone.TryFormat(smallBufferE164, out _, "E".AsSpan(), null);

            failFormatted.ShouldBeFalse($"Buffer -1 should fail for formatted ({country}/{area}/{number})");
            failE164.ShouldBeFalse($"Buffer -1 should fail for E.164 ({country}/{area}/{number})");
        }

        LogAssert("Required length calculation verified for all cases");
    }
}
