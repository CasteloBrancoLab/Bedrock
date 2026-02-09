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

        bool complexityValidation = ValidateAllowSpaces(executionContext, password)
            & ValidateRequireUppercase(executionContext, password)
            & ValidateRequireLowercase(executionContext, password)
            & ValidateRequireDigit(executionContext, password)
            & ValidateRequireSpecialCharacter(executionContext, password)
            & ValidateMinUniqueCharacters(executionContext, password);

        return minLengthValidation
            & maxLengthValidation
            & complexityValidation;
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

        foreach (char c in password)
        {
            if (char.IsUpper(c))
                return true;
        }

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

        foreach (char c in password)
        {
            if (char.IsLower(c))
                return true;
        }

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

        foreach (char c in password)
        {
            if (char.IsDigit(c))
                return true;
        }

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

        foreach (char c in password)
        {
            if (!char.IsLetterOrDigit(c) && c != ' ')
                return true;
        }

        executionContext.AddErrorMessage(
            code: $"{MessageCodePrefix}.RequireSpecialCharacter");
        return false;
    }

    private static bool ValidateMinUniqueCharacters(
        ExecutionContext executionContext,
        string password)
    {
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
