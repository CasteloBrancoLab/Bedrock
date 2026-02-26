using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.MfaSetups.Inputs;
using ShopDemo.Auth.Domain.Entities.MfaSetups.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.MfaSetups;

public sealed class MfaSetup
    : EntityBase<MfaSetup>,
    IMfaSetup
{
    // Properties
    public Id UserId { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string EncryptedSharedSecret { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public DateTimeOffset? EnabledAt { get; private set; }

    // Constructors
    private MfaSetup()
    {
    }

    private MfaSetup(
        EntityInfo entityInfo,
        Id userId,
        string encryptedSharedSecret,
        bool isEnabled,
        DateTimeOffset? enabledAt
    ) : base(entityInfo)
    {
        UserId = userId;
        EncryptedSharedSecret = encryptedSharedSecret;
        IsEnabled = isEnabled;
        EnabledAt = enabledAt;
    }

    // Public Business Methods
    public static MfaSetup? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewMfaSetupInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new MfaSetup(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetEncryptedSharedSecret(executionContext, input.EncryptedSharedSecret)
                    & instance.SetIsEnabled(false)
                    & instance.SetEnabledAt(null);
            }
        );
    }

    public static MfaSetup CreateFromExistingInfo(
        CreateFromExistingInfoMfaSetupInput input
    )
    {
        return new MfaSetup(
            input.EntityInfo,
            input.UserId,
            input.EncryptedSharedSecret,
            input.IsEnabled,
            input.EnabledAt
        );
    }

    public MfaSetup? Enable(
        ExecutionContext executionContext,
        EnableMfaSetupInput input
    )
    {
        return RegisterChangeInternal<MfaSetup, EnableMfaSetupInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.EnableInternal(executionContext);
            }
        );
    }

    public MfaSetup? Disable(
        ExecutionContext executionContext,
        DisableMfaSetupInput input
    )
    {
        return RegisterChangeInternal<MfaSetup, DisableMfaSetupInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.DisableInternal(executionContext);
            }
        );
    }

    public override MfaSetup Clone()
    {
        return new MfaSetup(
            EntityInfo,
            UserId,
            EncryptedSharedSecret,
            IsEnabled,
            EnabledAt
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool EnableInternal(
        ExecutionContext executionContext
    )
    {
        if (IsEnabled)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<MfaSetup>(propertyName: MfaSetupMetadata.IsEnabledPropertyName)}.AlreadyEnabled");
            return false;
        }

        IsEnabled = true;
        EnabledAt = executionContext.Timestamp;

        return true;
    }
    // Stryker restore all

    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool DisableInternal(
        ExecutionContext executionContext
    )
    {
        if (!IsEnabled)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<MfaSetup>(propertyName: MfaSetupMetadata.IsEnabledPropertyName)}.AlreadyDisabled");
            return false;
        }

        IsEnabled = false;
        EnabledAt = null;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        string? encryptedSharedSecret
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateEncryptedSharedSecret(executionContext, encryptedSharedSecret);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            UserId,
            EncryptedSharedSecret
        );
    }

    public static bool ValidateUserId(
        ExecutionContext executionContext,
        Id? userId
    )
    {
        bool userIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<MfaSetup>(propertyName: MfaSetupMetadata.UserIdPropertyName),
            isRequired: MfaSetupMetadata.UserIdIsRequired,
            value: userId
        );

        if (!userIdIsRequiredValidation)
            return false;

        if (userId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<MfaSetup>(propertyName: MfaSetupMetadata.UserIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateEncryptedSharedSecret(
        ExecutionContext executionContext,
        string? encryptedSharedSecret
    )
    {
        bool encryptedSharedSecretIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<MfaSetup>(propertyName: MfaSetupMetadata.EncryptedSharedSecretPropertyName),
            isRequired: MfaSetupMetadata.EncryptedSharedSecretIsRequired,
            value: encryptedSharedSecret
        );

        if (!encryptedSharedSecretIsRequiredValidation)
            return false;

        bool encryptedSharedSecretMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<MfaSetup>(propertyName: MfaSetupMetadata.EncryptedSharedSecretPropertyName),
            minLength: 1,
            value: encryptedSharedSecret!.Length
        );

        if (!encryptedSharedSecretMinLengthValidation)
            return false;

        bool encryptedSharedSecretMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<MfaSetup>(propertyName: MfaSetupMetadata.EncryptedSharedSecretPropertyName),
            maxLength: MfaSetupMetadata.EncryptedSharedSecretMaxLength,
            value: encryptedSharedSecret!.Length
        );

        return encryptedSharedSecretMaxLengthValidation;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetUserId(
        ExecutionContext executionContext,
        Id userId
    )
    {
        bool isValid = ValidateUserId(
            executionContext,
            userId
        );

        if (!isValid)
            return false;

        UserId = userId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetEncryptedSharedSecret(
        ExecutionContext executionContext,
        string encryptedSharedSecret
    )
    {
        bool isValid = ValidateEncryptedSharedSecret(
            executionContext,
            encryptedSharedSecret
        );

        if (!isValid)
            return false;

        EncryptedSharedSecret = encryptedSharedSecret;

        return true;
    }
    // Stryker restore all

    private bool SetIsEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return true;
    }

    private bool SetEnabledAt(DateTimeOffset? enabledAt)
    {
        EnabledAt = enabledAt;
        return true;
    }

    // Metadata
    public static class MfaSetupMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // EncryptedSharedSecret
        public static readonly string EncryptedSharedSecretPropertyName = "EncryptedSharedSecret";
        public static bool EncryptedSharedSecretIsRequired { get; private set; } = true;
        public static int EncryptedSharedSecretMaxLength { get; private set; } = 1024;

        // IsEnabled
        public static readonly string IsEnabledPropertyName = "IsEnabled";

        public static void ChangeUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                UserIdIsRequired = isRequired;
            }
        }

        public static void ChangeEncryptedSharedSecretMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                EncryptedSharedSecretIsRequired = isRequired;
                EncryptedSharedSecretMaxLength = maxLength;
            }
        }
    }
}
