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
}
