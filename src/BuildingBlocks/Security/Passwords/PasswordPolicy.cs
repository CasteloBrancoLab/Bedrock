using Bedrock.BuildingBlocks.Core.Validations;

namespace Bedrock.BuildingBlocks.Security.Passwords;

public static class PasswordPolicy
{
    private const string MessageCodePrefix = "PasswordPolicy.Password";

    public static bool ValidatePassword(
        ExecutionContext executionContext,
        string? password
    )
    {
        bool isRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: MessageCodePrefix,
            isRequired: true,
            value: password
        );

        if (!isRequiredValidation)
            return false;

        bool minLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: MessageCodePrefix,
            minLength: PasswordPolicyMetadata.MinLength,
            value: password!.Length
        );

        bool maxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: MessageCodePrefix,
            maxLength: PasswordPolicyMetadata.MaxLength,
            value: password!.Length
        );

        bool allowSpacesValidation = ValidateAllowSpaces(executionContext, password);
        bool requireUppercaseValidation = ValidateRequireUppercase(executionContext, password);
        bool requireLowercaseValidation = ValidateRequireLowercase(executionContext, password);
        bool requireDigitValidation = ValidateRequireDigit(executionContext, password);
        bool requireSpecialCharacterValidation = ValidateRequireSpecialCharacter(executionContext, password);
        bool minUniqueCharactersValidation = ValidateMinUniqueCharacters(executionContext, password);

        return minLengthValidation
            & maxLengthValidation
            & allowSpacesValidation
            & requireUppercaseValidation
            & requireLowercaseValidation
            & requireDigitValidation
            & requireSpecialCharacterValidation
            & minUniqueCharactersValidation;
    }

    private static bool ValidateAllowSpaces(
        ExecutionContext executionContext,
        string password)
    {
        if (PasswordPolicyMetadata.AllowSpaces)
            return true;

        if (password.Contains(' '))
        {
            executionContext.AddErrorMessage(
                code: $"{MessageCodePrefix}.AllowSpaces");
            return false;
        }

        return true;
    }

    private static bool ValidateRequireUppercase(
        ExecutionContext executionContext,
        string password)
    {
        if (!PasswordPolicyMetadata.RequireUppercase)
            return true;

        if (password.Any(char.IsUpper))
            return true;

        executionContext.AddErrorMessage(
            code: $"{MessageCodePrefix}.RequireUppercase");
        return false;
    }

    private static bool ValidateRequireLowercase(
        ExecutionContext executionContext,
        string password)
    {
        if (!PasswordPolicyMetadata.RequireLowercase)
            return true;

        if (password.Any(char.IsLower))
            return true;

        executionContext.AddErrorMessage(
            code: $"{MessageCodePrefix}.RequireLowercase");
        return false;
    }

    private static bool ValidateRequireDigit(
        ExecutionContext executionContext,
        string password)
    {
        if (!PasswordPolicyMetadata.RequireDigit)
            return true;

        if (password.Any(char.IsDigit))
            return true;

        executionContext.AddErrorMessage(
            code: $"{MessageCodePrefix}.RequireDigit");
        return false;
    }

    private static bool ValidateRequireSpecialCharacter(
        ExecutionContext executionContext,
        string password)
    {
        if (!PasswordPolicyMetadata.RequireSpecialCharacter)
            return true;

        if (password.Any(c => !char.IsLetterOrDigit(c) && c != ' '))
            return true;

        executionContext.AddErrorMessage(
            code: $"{MessageCodePrefix}.RequireSpecialCharacter");
        return false;
    }

    private static bool ValidateMinUniqueCharacters(
        ExecutionContext executionContext,
        string password)
    {
        // Stryker disable once Equality : <= 0 e < 0 sao equivalentes - HashSet.Count nunca e negativo
        if (PasswordPolicyMetadata.MinUniqueCharacters <= 0)
            return true;

        var uniqueChars = new HashSet<char>(password);
        if (uniqueChars.Count >= PasswordPolicyMetadata.MinUniqueCharacters)
            return true;

        executionContext.AddErrorMessage(
            code: $"{MessageCodePrefix}.MinUniqueCharacters");
        return false;
    }
}
