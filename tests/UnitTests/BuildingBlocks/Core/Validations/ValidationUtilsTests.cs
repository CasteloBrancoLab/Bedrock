using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Core.Validations.Enums;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Validations;

public class ValidationUtilsTests : TestBase
{
    private readonly TimeProvider _timeProvider;
    private readonly TenantInfo _tenantInfo;

    public ValidationUtilsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _timeProvider = TimeProvider.System;
        _tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
    }

    private ExecutionContext CreateContext()
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: _tenantInfo,
            executionUser: "test-user",
            executionOrigin: "test-origin",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: _timeProvider
        );
    }

    #region ValidateIsRequired Tests

    [Fact]
    public void ValidateIsRequired_NotRequired_WithNullValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for non-required null validation");
        var context = CreateContext();
        string? value = null;

        // Act
        LogAct("Validating non-required null value");
        var result = ValidationUtils.ValidateIsRequired(context, "Email", isRequired: false, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Non-required null value is valid");
    }

    [Fact]
    public void ValidateIsRequired_Required_WithNullValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context for required null validation");
        var context = CreateContext();
        string? value = null;

        // Act
        LogAct("Validating required null value");
        var result = ValidationUtils.ValidateIsRequired(context, "Email", isRequired: true, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.First().Code.ShouldBe("Email.IsRequired");
        LogInfo("Required null value is invalid");
    }

    [Fact]
    public void ValidateIsRequired_Required_WithDefaultInt_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context for required default int validation");
        var context = CreateContext();
        int value = 0;

        // Act
        LogAct("Validating required default int");
        var result = ValidationUtils.ValidateIsRequired(context, "Age", isRequired: true, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.First().Code.ShouldBe("Age.IsRequired");
        LogInfo("Required default int is invalid");
    }

    [Fact]
    public void ValidateIsRequired_Required_WithNonDefaultInt_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for required non-default int validation");
        var context = CreateContext();
        int value = 25;

        // Act
        LogAct("Validating required non-default int");
        var result = ValidationUtils.ValidateIsRequired(context, "Age", isRequired: true, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Required non-default int is valid");
    }

    [Fact]
    public void ValidateIsRequired_Required_WithEmptyGuid_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context for required empty Guid validation");
        var context = CreateContext();
        Guid value = Guid.Empty;

        // Act
        LogAct("Validating required empty Guid");
        var result = ValidationUtils.ValidateIsRequired(context, "UserId", isRequired: true, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.First().Code.ShouldBe("UserId.IsRequired");
        LogInfo("Required empty Guid is invalid");
    }

    [Fact]
    public void ValidateIsRequired_Required_WithValidGuid_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for required valid Guid validation");
        var context = CreateContext();
        Guid value = Guid.NewGuid();

        // Act
        LogAct("Validating required valid Guid");
        var result = ValidationUtils.ValidateIsRequired(context, "UserId", isRequired: true, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Required valid Guid is valid");
    }

    [Fact]
    public void ValidateIsRequired_Required_WithNonEmptyString_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for required non-empty string validation");
        var context = CreateContext();
        string value = "test@example.com";

        // Act
        LogAct("Validating required non-empty string");
        var result = ValidationUtils.ValidateIsRequired(context, "Email", isRequired: true, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Required non-empty string is valid");
    }

    [Fact]
    public void ValidateIsRequired_NotRequired_WithDefaultValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for non-required default value validation");
        var context = CreateContext();
        int value = 0;

        // Act
        LogAct("Validating non-required default value");
        var result = ValidationUtils.ValidateIsRequired(context, "Count", isRequired: false, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Non-required default value is valid");
    }

    #endregion

    #region ValidateMinLength Tests

    [Fact]
    public void ValidateMinLength_WithNullValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for null min length validation");
        var context = CreateContext();
        string? value = null;

        // Act
        LogAct("Validating null value for min length");
        var result = ValidationUtils.ValidateMinLength(context, "Name", minLength: "A", value);

        // Assert
        LogAssert("Verifying validation passed (null is valid)");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Null value passes min length validation");
    }

    [Fact]
    public void ValidateMinLength_WithValueBelowMin_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context for below min validation");
        var context = CreateContext();
        int value = 15;

        // Act
        LogAct("Validating value below minimum");
        var result = ValidationUtils.ValidateMinLength(context, "Age", minLength: 18, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.First().Code.ShouldBe("Age.MinLength");
        LogInfo("Value below minimum is invalid");
    }

    [Fact]
    public void ValidateMinLength_WithValueEqualToMin_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for equal to min validation");
        var context = CreateContext();
        int value = 18;

        // Act
        LogAct("Validating value equal to minimum");
        var result = ValidationUtils.ValidateMinLength(context, "Age", minLength: 18, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Value equal to minimum is valid");
    }

    [Fact]
    public void ValidateMinLength_WithValueAboveMin_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for above min validation");
        var context = CreateContext();
        int value = 25;

        // Act
        LogAct("Validating value above minimum");
        var result = ValidationUtils.ValidateMinLength(context, "Age", minLength: 18, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Value above minimum is valid");
    }

    [Fact]
    public void ValidateMinLength_WithStringLength_ShouldWork()
    {
        // Arrange
        LogArrange("Creating context for string length min validation");
        var context = CreateContext();
        string name = "Jo";

        // Act
        LogAct("Validating string length below minimum");
        var result = ValidationUtils.ValidateMinLength(context, "Name", minLength: 3, name.Length);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.First().Code.ShouldBe("Name.MinLength");
        LogInfo("String too short is invalid");
    }

    #endregion

    #region ValidateMaxLength Tests

    [Fact]
    public void ValidateMaxLength_WithNullValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for null max length validation");
        var context = CreateContext();
        string? value = null;

        // Act
        LogAct("Validating null value for max length");
        var result = ValidationUtils.ValidateMaxLength(context, "Name", maxLength: "Z", value);

        // Assert
        LogAssert("Verifying validation passed (null is valid)");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Null value passes max length validation");
    }

    [Fact]
    public void ValidateMaxLength_WithValueAboveMax_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context for above max validation");
        var context = CreateContext();
        int value = 150;

        // Act
        LogAct("Validating value above maximum");
        var result = ValidationUtils.ValidateMaxLength(context, "Age", maxLength: 120, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.First().Code.ShouldBe("Age.MaxLength");
        LogInfo("Value above maximum is invalid");
    }

    [Fact]
    public void ValidateMaxLength_WithValueEqualToMax_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for equal to max validation");
        var context = CreateContext();
        int value = 120;

        // Act
        LogAct("Validating value equal to maximum");
        var result = ValidationUtils.ValidateMaxLength(context, "Age", maxLength: 120, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Value equal to maximum is valid");
    }

    [Fact]
    public void ValidateMaxLength_WithValueBelowMax_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for below max validation");
        var context = CreateContext();
        int value = 50;

        // Act
        LogAct("Validating value below maximum");
        var result = ValidationUtils.ValidateMaxLength(context, "Age", maxLength: 120, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Value below maximum is valid");
    }

    [Fact]
    public void ValidateMaxLength_WithStringLength_ShouldWork()
    {
        // Arrange
        LogArrange("Creating context for string length max validation");
        var context = CreateContext();
        string name = "This is a very long name that exceeds the maximum allowed length";

        // Act
        LogAct("Validating string length above maximum");
        var result = ValidationUtils.ValidateMaxLength(context, "Name", maxLength: 20, name.Length);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.First().Code.ShouldBe("Name.MaxLength");
        LogInfo("String too long is invalid");
    }

    #endregion

    #region Combined Validation Tests

    [Fact]
    public void CombinedValidations_AllValid_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for combined validations");
        var context = CreateContext();
        string name = "John Doe";

        // Act
        LogAct("Running combined validations");
        bool isValid = true;
        isValid &= ValidationUtils.ValidateIsRequired(context, "Name", isRequired: true, name);
        isValid &= ValidationUtils.ValidateMinLength(context, "Name", minLength: 3, name.Length);
        isValid &= ValidationUtils.ValidateMaxLength(context, "Name", maxLength: 50, name.Length);

        // Assert
        LogAssert("Verifying all validations passed");
        isValid.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("All combined validations passed");
    }

    [Fact]
    public void CombinedValidations_OneInvalid_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating context for combined validations with one failure");
        var context = CreateContext();
        string name = "Jo";

        // Act
        LogAct("Running combined validations");
        bool isValid = true;
        isValid &= ValidationUtils.ValidateIsRequired(context, "Name", isRequired: true, name);
        isValid &= ValidationUtils.ValidateMinLength(context, "Name", minLength: 3, name.Length);
        isValid &= ValidationUtils.ValidateMaxLength(context, "Name", maxLength: 50, name.Length);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.Count().ShouldBe(1);
        context.Messages.First().Code.ShouldBe("Name.MinLength");
        LogInfo("Combined validation detected min length failure");
    }

    [Fact]
    public void CombinedValidations_MultipleInvalid_ShouldAddMultipleErrors()
    {
        // Arrange
        LogArrange("Creating context for multiple validation failures");
        var context = CreateContext();
        string? name = null;
        int age = 10;

        // Act
        LogAct("Running multiple validations");
        bool isValid = true;
        isValid &= ValidationUtils.ValidateIsRequired(context, "Name", isRequired: true, name);
        isValid &= ValidationUtils.ValidateMinLength(context, "Age", minLength: 18, age);

        // Assert
        LogAssert("Verifying multiple errors added");
        isValid.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.Count().ShouldBe(2);
        context.Messages.Select(m => m.Code).ShouldContain("Name.IsRequired");
        context.Messages.Select(m => m.Code).ShouldContain("Age.MinLength");
        LogInfo("Multiple validation errors detected");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ValidateIsRequired_WithFalseBoolean_ShouldReturnFalse()
    {
        // Arrange - false is default for bool
        LogArrange("Creating context for required false boolean validation");
        var context = CreateContext();
        bool value = false;

        // Act
        LogAct("Validating required false boolean");
        var result = ValidationUtils.ValidateIsRequired(context, "IsActive", isRequired: true, value);

        // Assert
        LogAssert("Verifying validation failed (false == default(bool))");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        LogInfo("Required false boolean is invalid (default)");
    }

    [Fact]
    public void ValidateIsRequired_WithTrueBoolean_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context for required true boolean validation");
        var context = CreateContext();
        bool value = true;

        // Act
        LogAct("Validating required true boolean");
        var result = ValidationUtils.ValidateIsRequired(context, "IsActive", isRequired: true, value);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
        LogInfo("Required true boolean is valid");
    }

    [Fact]
    public void ValidateMinLength_WithDecimal_ShouldWork()
    {
        // Arrange
        LogArrange("Creating context for decimal min validation");
        var context = CreateContext();
        decimal value = 9.99m;

        // Act
        LogAct("Validating decimal below minimum");
        var result = ValidationUtils.ValidateMinLength(context, "Price", minLength: 10.00m, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        LogInfo("Decimal below minimum is invalid");
    }

    [Fact]
    public void ValidateMaxLength_WithDecimal_ShouldWork()
    {
        // Arrange
        LogArrange("Creating context for decimal max validation");
        var context = CreateContext();
        decimal value = 1000.01m;

        // Act
        LogAct("Validating decimal above maximum");
        var result = ValidationUtils.ValidateMaxLength(context, "Price", maxLength: 1000.00m, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        LogInfo("Decimal above maximum is invalid");
    }

    [Fact]
    public void ValidateMinLength_WithDateTime_ShouldWork()
    {
        // Arrange
        LogArrange("Creating context for DateTime min validation");
        var context = CreateContext();
        var minDate = new DateTime(2020, 1, 1);
        var value = new DateTime(2019, 6, 15);

        // Act
        LogAct("Validating DateTime below minimum");
        var result = ValidationUtils.ValidateMinLength(context, "StartDate", minLength: minDate, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        LogInfo("DateTime below minimum is invalid");
    }

    [Fact]
    public void ValidateMaxLength_WithDateTime_ShouldWork()
    {
        // Arrange
        LogArrange("Creating context for DateTime max validation");
        var context = CreateContext();
        var maxDate = new DateTime(2025, 12, 31);
        var value = new DateTime(2026, 1, 1);

        // Act
        LogAct("Validating DateTime above maximum");
        var result = ValidationUtils.ValidateMaxLength(context, "EndDate", maxLength: maxDate, value);

        // Assert
        LogAssert("Verifying validation failed");
        result.ShouldBeFalse();
        context.HasErrorMessages.ShouldBeTrue();
        LogInfo("DateTime above maximum is invalid");
    }

    #endregion

    #region Mutation Killing Tests

    [Fact]
    public void ValidateIsRequired_ConditionMustCheckBothNullAndDefault()
    {
        // Mata mutante: value is null || value.Equals(default) -> apenas um dos dois

        // Arrange
        LogArrange("Verificando ambas as condicoes do IsRequired");
        var context1 = CreateContext();
        var context2 = CreateContext();

        // Act & Assert - null
        LogAct("Testando condicao null");
        string? nullValue = null;
        ValidationUtils.ValidateIsRequired(context1, "Field", true, nullValue).ShouldBeFalse();
        context1.Messages.First().Code.ShouldBe("Field.IsRequired");

        // Act & Assert - default (mas nao null)
        LogAct("Testando condicao default");
        int defaultInt = 0;
        ValidationUtils.ValidateIsRequired(context2, "Number", true, defaultInt).ShouldBeFalse();
        context2.Messages.First().Code.ShouldBe("Number.IsRequired");

        LogAssert("Ambas as condicoes verificadas");
    }

    [Fact]
    public void ValidateMinLength_MustReturnTrueForNull()
    {
        // Mata mutante: if (value is null) return true -> return false

        // Arrange
        LogArrange("Verificando que null retorna true");
        var context = CreateContext();
        string? nullValue = null;

        // Act
        LogAct("Validando null");
        var result = ValidationUtils.ValidateMinLength(context, "Value", "A", nullValue);

        // Assert
        LogAssert("Verificando retorno true para null");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
    }

    [Fact]
    public void ValidateMaxLength_MustReturnTrueForNull()
    {
        // Mata mutante: if (value is null) return true -> return false

        // Arrange
        LogArrange("Verificando que null retorna true");
        var context = CreateContext();
        string? nullValue = null;

        // Act
        LogAct("Validando null");
        var result = ValidationUtils.ValidateMaxLength(context, "Value", "Z", nullValue);

        // Assert
        LogAssert("Verificando retorno true para null");
        result.ShouldBeTrue();
        context.HasErrorMessages.ShouldBeFalse();
    }

    [Fact]
    public void ValidateMinLength_CompareToMustBeLessThanZero()
    {
        // Mata mutante: CompareTo < 0 -> CompareTo <= 0 ou > 0

        // Arrange
        LogArrange("Verificando comparacao estrita");
        var contextEqual = CreateContext();
        var contextBelow = CreateContext();

        // Act - valor igual ao minimo deve passar
        LogAct("Testando valor igual ao minimo");
        var resultEqual = ValidationUtils.ValidateMinLength(contextEqual, "V", 10, 10);
        resultEqual.ShouldBeTrue();
        contextEqual.HasErrorMessages.ShouldBeFalse();

        // Act - valor abaixo do minimo deve falhar
        LogAct("Testando valor abaixo do minimo");
        var resultBelow = ValidationUtils.ValidateMinLength(contextBelow, "V", 10, 9);
        resultBelow.ShouldBeFalse();
        contextBelow.HasErrorMessages.ShouldBeTrue();

        LogAssert("Comparacao < verificada");
    }

    [Fact]
    public void ValidateMaxLength_CompareToMustBeGreaterThanZero()
    {
        // Mata mutante: CompareTo > 0 -> CompareTo >= 0 ou < 0

        // Arrange
        LogArrange("Verificando comparacao estrita");
        var contextEqual = CreateContext();
        var contextAbove = CreateContext();

        // Act - valor igual ao maximo deve passar
        LogAct("Testando valor igual ao maximo");
        var resultEqual = ValidationUtils.ValidateMaxLength(contextEqual, "V", 100, 100);
        resultEqual.ShouldBeTrue();
        contextEqual.HasErrorMessages.ShouldBeFalse();

        // Act - valor acima do maximo deve falhar
        LogAct("Testando valor acima do maximo");
        var resultAbove = ValidationUtils.ValidateMaxLength(contextAbove, "V", 100, 101);
        resultAbove.ShouldBeFalse();
        contextAbove.HasErrorMessages.ShouldBeTrue();

        LogAssert("Comparacao > verificada");
    }

    [Fact]
    public void ValidateIsRequired_MustReturnFalseOnFailure()
    {
        // Mata mutante: return false -> return true

        // Arrange
        LogArrange("Verificando retorno false em falha");
        var context = CreateContext();

        // Act
        LogAct("Validando valor invalido");
        var result = ValidationUtils.ValidateIsRequired(context, "Field", true, (string?)null);

        // Assert
        LogAssert("Verificando retorno false");
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidateMinLength_MustReturnFalseOnFailure()
    {
        // Mata mutante: return false -> return true

        // Arrange
        LogArrange("Verificando retorno false em falha");
        var context = CreateContext();

        // Act
        LogAct("Validando valor invalido");
        var result = ValidationUtils.ValidateMinLength(context, "Field", 10, 5);

        // Assert
        LogAssert("Verificando retorno false");
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidateMaxLength_MustReturnFalseOnFailure()
    {
        // Mata mutante: return false -> return true

        // Arrange
        LogArrange("Verificando retorno false em falha");
        var context = CreateContext();

        // Act
        LogAct("Validando valor invalido");
        var result = ValidationUtils.ValidateMaxLength(context, "Field", 10, 15);

        // Assert
        LogAssert("Verificando retorno false");
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidateIsRequired_MustAddErrorMessage()
    {
        // Mata mutante: AddErrorMessage statement removal

        // Arrange
        LogArrange("Verificando que mensagem de erro e adicionada");
        var context = CreateContext();

        // Act
        LogAct("Validando valor invalido");
        ValidationUtils.ValidateIsRequired(context, "TestField", true, (string?)null);

        // Assert
        LogAssert("Verificando mensagem adicionada");
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.ShouldContain(m => m.Code == "TestField.IsRequired");
    }

    [Fact]
    public void ValidateMinLength_MustAddErrorMessage()
    {
        // Mata mutante: AddErrorMessage statement removal

        // Arrange
        LogArrange("Verificando que mensagem de erro e adicionada");
        var context = CreateContext();

        // Act
        LogAct("Validando valor invalido");
        ValidationUtils.ValidateMinLength(context, "TestField", 10, 5);

        // Assert
        LogAssert("Verificando mensagem adicionada");
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.ShouldContain(m => m.Code == "TestField.MinLength");
    }

    [Fact]
    public void ValidateMaxLength_MustAddErrorMessage()
    {
        // Mata mutante: AddErrorMessage statement removal

        // Arrange
        LogArrange("Verificando que mensagem de erro e adicionada");
        var context = CreateContext();

        // Act
        LogAct("Validando valor invalido");
        ValidationUtils.ValidateMaxLength(context, "TestField", 10, 15);

        // Assert
        LogAssert("Verificando mensagem adicionada");
        context.HasErrorMessages.ShouldBeTrue();
        context.Messages.ShouldContain(m => m.Code == "TestField.MaxLength");
    }

    #endregion

    #region Cache Consistency Tests

    [Fact]
    public void ValidateIsRequired_SamePropertyName_ShouldReturnSameErrorCode()
    {
        // Verifica que o cache retorna a mesma instância de string para a mesma propriedade

        // Arrange
        LogArrange("Criando dois contextos para validar cache");
        var context1 = CreateContext();
        var context2 = CreateContext();

        // Act
        LogAct("Validando mesma propriedade duas vezes");
        ValidationUtils.ValidateIsRequired(context1, "CachedField", true, (string?)null);
        ValidationUtils.ValidateIsRequired(context2, "CachedField", true, (string?)null);

        // Assert
        LogAssert("Verificando que os códigos são iguais");
        var code1 = context1.Messages.First().Code;
        var code2 = context2.Messages.First().Code;
        code1.ShouldBe("CachedField.IsRequired");
        code2.ShouldBe("CachedField.IsRequired");
        ReferenceEquals(code1, code2).ShouldBeTrue("Códigos devem ser a mesma instância de string (cache)");
    }

    [Fact]
    public void ValidateMinLength_SamePropertyName_ShouldReturnSameErrorCode()
    {
        // Verifica que o cache retorna a mesma instância de string para a mesma propriedade

        // Arrange
        LogArrange("Criando dois contextos para validar cache");
        var context1 = CreateContext();
        var context2 = CreateContext();

        // Act
        LogAct("Validando mesma propriedade duas vezes");
        ValidationUtils.ValidateMinLength(context1, "CachedMinField", 10, 5);
        ValidationUtils.ValidateMinLength(context2, "CachedMinField", 10, 5);

        // Assert
        LogAssert("Verificando que os códigos são iguais");
        var code1 = context1.Messages.First().Code;
        var code2 = context2.Messages.First().Code;
        code1.ShouldBe("CachedMinField.MinLength");
        code2.ShouldBe("CachedMinField.MinLength");
        ReferenceEquals(code1, code2).ShouldBeTrue("Códigos devem ser a mesma instância de string (cache)");
    }

    [Fact]
    public void ValidateMaxLength_SamePropertyName_ShouldReturnSameErrorCode()
    {
        // Verifica que o cache retorna a mesma instância de string para a mesma propriedade

        // Arrange
        LogArrange("Criando dois contextos para validar cache");
        var context1 = CreateContext();
        var context2 = CreateContext();

        // Act
        LogAct("Validando mesma propriedade duas vezes");
        ValidationUtils.ValidateMaxLength(context1, "CachedMaxField", 10, 15);
        ValidationUtils.ValidateMaxLength(context2, "CachedMaxField", 10, 15);

        // Assert
        LogAssert("Verificando que os códigos são iguais");
        var code1 = context1.Messages.First().Code;
        var code2 = context2.Messages.First().Code;
        code1.ShouldBe("CachedMaxField.MaxLength");
        code2.ShouldBe("CachedMaxField.MaxLength");
        ReferenceEquals(code1, code2).ShouldBeTrue("Códigos devem ser a mesma instância de string (cache)");
    }

    [Fact]
    public void DifferentValidationTypes_SamePropertyName_ShouldReturnDifferentCodes()
    {
        // Verifica que diferentes tipos de validação geram códigos diferentes

        // Arrange
        LogArrange("Criando contextos para diferentes tipos de validação");
        var context1 = CreateContext();
        var context2 = CreateContext();
        var context3 = CreateContext();

        // Act
        LogAct("Validando com diferentes tipos");
        ValidationUtils.ValidateIsRequired(context1, "MultiField", true, (string?)null);
        ValidationUtils.ValidateMinLength(context2, "MultiField", 10, 5);
        ValidationUtils.ValidateMaxLength(context3, "MultiField", 10, 15);

        // Assert
        LogAssert("Verificando códigos diferentes");
        var codeRequired = context1.Messages.First().Code;
        var codeMin = context2.Messages.First().Code;
        var codeMax = context3.Messages.First().Code;

        codeRequired.ShouldBe("MultiField.IsRequired");
        codeMin.ShouldBe("MultiField.MinLength");
        codeMax.ShouldBe("MultiField.MaxLength");

        codeRequired.ShouldNotBe(codeMin);
        codeRequired.ShouldNotBe(codeMax);
        codeMin.ShouldNotBe(codeMax);
    }

    #endregion
}
