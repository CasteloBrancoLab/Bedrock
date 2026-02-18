using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;
using ShopDemo.Auth.Domain.Entities.KeyChains.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.KeyChains;

public sealed class KeyChain
    : EntityBase<KeyChain>,
    IKeyChain
{
    // Properties
    public Id UserId { get; private set; }
    public KeyId KeyId { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string PublicKey { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string EncryptedSharedSecret { get; private set; } = string.Empty;
    public KeyChainStatus Status { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    // Constructors
    private KeyChain()
    {
    }

    private KeyChain(
        EntityInfo entityInfo,
        Id userId,
        KeyId keyId,
        string publicKey,
        string encryptedSharedSecret,
        KeyChainStatus status,
        DateTimeOffset expiresAt
    ) : base(entityInfo)
    {
        UserId = userId;
        KeyId = keyId;
        PublicKey = publicKey;
        EncryptedSharedSecret = encryptedSharedSecret;
        Status = status;
        ExpiresAt = expiresAt;
    }

    // Public Business Methods
    public static KeyChain? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewKeyChainInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new KeyChain(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetKeyId(executionContext, input.KeyId)
                    & instance.SetPublicKey(executionContext, input.PublicKey)
                    & instance.SetEncryptedSharedSecret(executionContext, input.EncryptedSharedSecret)
                    & instance.SetStatus(executionContext, KeyChainStatus.Active)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt);
            }
        );
    }

    public static KeyChain CreateFromExistingInfo(
        CreateFromExistingInfoKeyChainInput input
    )
    {
        return new KeyChain(
            input.EntityInfo,
            input.UserId,
            input.KeyId,
            input.PublicKey,
            input.EncryptedSharedSecret,
            input.Status,
            input.ExpiresAt
        );
    }

    public KeyChain? Deactivate(
        ExecutionContext executionContext,
        DeactivateKeyChainInput input
    )
    {
        return RegisterChangeInternal<KeyChain, DeactivateKeyChainInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.DeactivateInternal(executionContext);
            }
        );
    }

    public override KeyChain Clone()
    {
        return new KeyChain(
            EntityInfo,
            UserId,
            KeyId,
            PublicKey,
            EncryptedSharedSecret,
            Status,
            ExpiresAt
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool DeactivateInternal(
        ExecutionContext executionContext
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, KeyChainStatus.DecryptOnly);

        if (!isValidTransition)
            return false;

        Status = KeyChainStatus.DecryptOnly;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        KeyId? keyId,
        string? publicKey,
        string? encryptedSharedSecret,
        KeyChainStatus? status,
        DateTimeOffset? expiresAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateKeyId(executionContext, keyId)
            & ValidatePublicKey(executionContext, publicKey)
            & ValidateEncryptedSharedSecret(executionContext, encryptedSharedSecret)
            & ValidateStatus(executionContext, status)
            & ValidateExpiresAt(executionContext, expiresAt);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            UserId,
            KeyId,
            PublicKey,
            EncryptedSharedSecret,
            Status,
            ExpiresAt
        );
    }

    public static bool ValidateUserId(
        ExecutionContext executionContext,
        Id? userId
    )
    {
        bool userIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.UserIdPropertyName),
            isRequired: KeyChainMetadata.UserIdIsRequired,
            value: userId
        );

        return userIdIsRequiredValidation;
    }

    public static bool ValidateKeyId(
        ExecutionContext executionContext,
        KeyId? keyId
    )
    {
        bool keyIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.KeyIdPropertyName),
            isRequired: KeyChainMetadata.KeyIdIsRequired,
            value: keyId
        );

        return keyIdIsRequiredValidation;
    }

    public static bool ValidatePublicKey(
        ExecutionContext executionContext,
        string? publicKey
    )
    {
        bool publicKeyIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.PublicKeyPropertyName),
            isRequired: KeyChainMetadata.PublicKeyIsRequired,
            value: publicKey
        );

        if (!publicKeyIsRequiredValidation)
            return false;

        bool publicKeyMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.PublicKeyPropertyName),
            maxLength: KeyChainMetadata.PublicKeyMaxLength,
            value: publicKey!.Length
        );

        return publicKeyMaxLengthValidation;
    }

    public static bool ValidateEncryptedSharedSecret(
        ExecutionContext executionContext,
        string? encryptedSharedSecret
    )
    {
        bool encryptedSharedSecretIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.EncryptedSharedSecretPropertyName),
            isRequired: KeyChainMetadata.EncryptedSharedSecretIsRequired,
            value: encryptedSharedSecret
        );

        if (!encryptedSharedSecretIsRequiredValidation)
            return false;

        bool encryptedSharedSecretMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.EncryptedSharedSecretPropertyName),
            maxLength: KeyChainMetadata.EncryptedSharedSecretMaxLength,
            value: encryptedSharedSecret!.Length
        );

        return encryptedSharedSecretMaxLengthValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        KeyChainStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.StatusPropertyName),
            isRequired: KeyChainMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.ExpiresAtPropertyName),
            isRequired: KeyChainMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        KeyChainStatus? from,
        KeyChainStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (KeyChainStatus.Active, KeyChainStatus.DecryptOnly) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<KeyChain>(propertyName: KeyChainMetadata.StatusPropertyName)}.InvalidTransition");
        }

        return isValid;
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
    private bool SetKeyId(
        ExecutionContext executionContext,
        KeyId keyId
    )
    {
        bool isValid = ValidateKeyId(
            executionContext,
            keyId
        );

        if (!isValid)
            return false;

        KeyId = keyId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetPublicKey(
        ExecutionContext executionContext,
        string publicKey
    )
    {
        bool isValid = ValidatePublicKey(
            executionContext,
            publicKey
        );

        if (!isValid)
            return false;

        PublicKey = publicKey;

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

    // Stryker disable once Block : SetStatus recebe KeyChainStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe KeyChainStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        KeyChainStatus status
    )
    {
        bool isValid = ValidateStatus(
            executionContext,
            status
        );

        if (!isValid)
            return false;

        Status = status;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset expiresAt
    )
    {
        bool isValid = ValidateExpiresAt(
            executionContext,
            expiresAt
        );

        if (!isValid)
            return false;

        ExpiresAt = expiresAt;

        return true;
    }
    // Stryker restore all

    // Metadata
    public static class KeyChainMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // KeyId
        public static readonly string KeyIdPropertyName = "KeyId";
        public static bool KeyIdIsRequired { get; private set; } = true;

        // PublicKey
        public static readonly string PublicKeyPropertyName = "PublicKey";
        public static bool PublicKeyIsRequired { get; private set; } = true;
        public static int PublicKeyMaxLength { get; private set; } = 512;

        // EncryptedSharedSecret
        public static readonly string EncryptedSharedSecretPropertyName = "EncryptedSharedSecret";
        public static bool EncryptedSharedSecretIsRequired { get; private set; } = true;
        public static int EncryptedSharedSecretMaxLength { get; private set; } = 1024;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        public static void ChangeUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                UserIdIsRequired = isRequired;
            }
        }

        public static void ChangeKeyIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                KeyIdIsRequired = isRequired;
            }
        }

        public static void ChangePublicKeyMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                PublicKeyIsRequired = isRequired;
                PublicKeyMaxLength = maxLength;
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

        public static void ChangeStatusMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                StatusIsRequired = isRequired;
            }
        }

        public static void ChangeExpiresAtMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ExpiresAtIsRequired = isRequired;
            }
        }
    }
}
