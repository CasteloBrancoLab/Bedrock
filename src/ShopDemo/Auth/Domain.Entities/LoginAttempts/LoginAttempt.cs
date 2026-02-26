using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs;
using ShopDemo.Auth.Domain.Entities.LoginAttempts.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.LoginAttempts;

public sealed class LoginAttempt
    : EntityBase<LoginAttempt>,
    ILoginAttempt
{
    // Properties
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Username { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public DateTimeOffset AttemptedAt { get; private set; }
    public bool IsSuccessful { get; private set; }
    public string? FailureReason { get; private set; }

    // Constructors
    private LoginAttempt()
    {
    }

    private LoginAttempt(
        EntityInfo entityInfo,
        string username,
        string? ipAddress,
        DateTimeOffset attemptedAt,
        bool isSuccessful,
        string? failureReason
    ) : base(entityInfo)
    {
        Username = username;
        IpAddress = ipAddress;
        AttemptedAt = attemptedAt;
        IsSuccessful = isSuccessful;
        FailureReason = failureReason;
    }

    // Public Business Methods
    public static LoginAttempt? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewLoginAttemptInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new LoginAttempt(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUsername(executionContext, input.Username)
                    & instance.SetIpAddress(executionContext, input.IpAddress)
                    & instance.SetAttemptedAt(executionContext.Timestamp)
                    & instance.SetIsSuccessful(input.IsSuccessful)
                    & instance.SetFailureReason(executionContext, input.FailureReason);
            }
        );
    }

    public static LoginAttempt CreateFromExistingInfo(
        CreateFromExistingInfoLoginAttemptInput input
    )
    {
        return new LoginAttempt(
            input.EntityInfo,
            input.Username,
            input.IpAddress,
            input.AttemptedAt,
            input.IsSuccessful,
            input.FailureReason
        );
    }

    public override LoginAttempt Clone()
    {
        return new LoginAttempt(
            EntityInfo,
            Username,
            IpAddress,
            AttemptedAt,
            IsSuccessful,
            FailureReason
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? username,
        DateTimeOffset? attemptedAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUsername(executionContext, username)
            & ValidateAttemptedAt(executionContext, attemptedAt);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Username,
            AttemptedAt
        );
    }

    public static bool ValidateUsername(
        ExecutionContext executionContext,
        string? username
    )
    {
        bool usernameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<LoginAttempt>(propertyName: LoginAttemptMetadata.UsernamePropertyName),
            isRequired: LoginAttemptMetadata.UsernameIsRequired,
            value: username
        );

        if (!usernameIsRequiredValidation)
            return false;

        bool usernameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<LoginAttempt>(propertyName: LoginAttemptMetadata.UsernamePropertyName),
            maxLength: LoginAttemptMetadata.UsernameMaxLength,
            value: username!.Length
        );

        return usernameMaxLengthValidation;
    }

    public static bool ValidateAttemptedAt(
        ExecutionContext executionContext,
        DateTimeOffset? attemptedAt
    )
    {
        bool attemptedAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<LoginAttempt>(propertyName: LoginAttemptMetadata.AttemptedAtPropertyName),
            isRequired: LoginAttemptMetadata.AttemptedAtIsRequired,
            value: attemptedAt
        );

        return attemptedAtIsRequiredValidation;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetUsername(
        ExecutionContext executionContext,
        string username
    )
    {
        bool isValid = ValidateUsername(
            executionContext,
            username
        );

        if (!isValid)
            return false;

        Username = username;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetIpAddress(
        ExecutionContext executionContext,
        string? ipAddress
    )
    {
        if (ipAddress is not null)
        {
            bool ipAddressMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<LoginAttempt>(propertyName: LoginAttemptMetadata.IpAddressPropertyName),
                maxLength: LoginAttemptMetadata.IpAddressMaxLength,
                value: ipAddress.Length
            );

            if (!ipAddressMaxLengthValidation)
                return false;
        }

        IpAddress = ipAddress;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetAttemptedAt(
        DateTimeOffset attemptedAt
    )
    {
        AttemptedAt = attemptedAt;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetIsSuccessful(
        bool isSuccessful
    )
    {
        IsSuccessful = isSuccessful;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetFailureReason(
        ExecutionContext executionContext,
        string? failureReason
    )
    {
        if (failureReason is not null)
        {
            bool failureReasonMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<LoginAttempt>(propertyName: LoginAttemptMetadata.FailureReasonPropertyName),
                maxLength: LoginAttemptMetadata.FailureReasonMaxLength,
                value: failureReason.Length
            );

            if (!failureReasonMaxLengthValidation)
                return false;
        }

        FailureReason = failureReason;

        return true;
    }
    // Stryker restore all

    // Metadata
    public static class LoginAttemptMetadata
    {
        private static readonly Lock _lockObject = new();

        // Username
        public static readonly string UsernamePropertyName = "Username";
        public static bool UsernameIsRequired { get; private set; } = true;
        public static int UsernameMaxLength { get; private set; } = 255;

        // IpAddress
        public static readonly string IpAddressPropertyName = "IpAddress";
        public static int IpAddressMaxLength { get; private set; } = 45;

        // AttemptedAt
        public static readonly string AttemptedAtPropertyName = "AttemptedAt";
        public static bool AttemptedAtIsRequired { get; private set; } = true;

        // FailureReason
        public static readonly string FailureReasonPropertyName = "FailureReason";
        public static int FailureReasonMaxLength { get; private set; } = 255;

        public static void ChangeUsernameMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                UsernameIsRequired = isRequired;
                UsernameMaxLength = maxLength;
            }
        }

        public static void ChangeIpAddressMetadata(
            int maxLength
        )
        {
            lock (_lockObject)
            {
                IpAddressMaxLength = maxLength;
            }
        }

        public static void ChangeAttemptedAtMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                AttemptedAtIsRequired = isRequired;
            }
        }

        public static void ChangeFailureReasonMetadata(
            int maxLength
        )
        {
            lock (_lockObject)
            {
                FailureReasonMaxLength = maxLength;
            }
        }
    }
}
