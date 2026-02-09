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

        return isRequiredValidation
            && minLengthValidation
            && maxLengthValidation;
    }
}
