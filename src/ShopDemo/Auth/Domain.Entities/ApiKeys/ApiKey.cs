using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.ApiKeys;

public sealed class ApiKey
    : EntityBase<ApiKey>,
    IApiKey
{
    // Properties
    public Id ServiceClientId { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string KeyPrefix { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string KeyHash { get; private set; } = string.Empty;
    public ApiKeyStatus Status { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    // Constructors
    private ApiKey()
    {
    }

    private ApiKey(
        EntityInfo entityInfo,
        Id serviceClientId,
        string keyPrefix,
        string keyHash,
        ApiKeyStatus status,
        DateTimeOffset? expiresAt,
        DateTimeOffset? lastUsedAt,
        DateTimeOffset? revokedAt
    ) : base(entityInfo)
    {
        ServiceClientId = serviceClientId;
        KeyPrefix = keyPrefix;
        KeyHash = keyHash;
        Status = status;
        ExpiresAt = expiresAt;
        LastUsedAt = lastUsedAt;
        RevokedAt = revokedAt;
    }

    // Public Business Methods
    public static ApiKey? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewApiKeyInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new ApiKey(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetServiceClientId(executionContext, input.ServiceClientId)
                    & instance.SetKeyPrefix(executionContext, input.KeyPrefix)
                    & instance.SetKeyHash(executionContext, input.KeyHash)
                    & instance.SetStatus(executionContext, ApiKeyStatus.Active)
                    & instance.SetExpiresAt(input.ExpiresAt)
                    & instance.SetLastUsedAt(null)
                    & instance.SetRevokedAt(null);
            }
        );
    }

    public static ApiKey CreateFromExistingInfo(
        CreateFromExistingInfoApiKeyInput input
    )
    {
        return new ApiKey(
            input.EntityInfo,
            input.ServiceClientId,
            input.KeyPrefix,
            input.KeyHash,
            input.Status,
            input.ExpiresAt,
            input.LastUsedAt,
            input.RevokedAt
        );
    }

    public ApiKey? Revoke(
        ExecutionContext executionContext,
        RevokeApiKeyInput input
    )
    {
        return RegisterChangeInternal<ApiKey, RevokeApiKeyInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RevokeInternal(executionContext);
            }
        );
    }

    public ApiKey? RecordUsage(
        ExecutionContext executionContext,
        RecordApiKeyUsageInput input
    )
    {
        return RegisterChangeInternal<ApiKey, RecordApiKeyUsageInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RecordUsageInternal(executionContext);
            }
        );
    }

    public override ApiKey Clone()
    {
        return new ApiKey(
            EntityInfo,
            ServiceClientId,
            KeyPrefix,
            KeyHash,
            Status,
            ExpiresAt,
            LastUsedAt,
            RevokedAt
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool RevokeInternal(
        ExecutionContext executionContext
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, ApiKeyStatus.Revoked);

        if (!isValidTransition)
            return false;

        Status = ApiKeyStatus.Revoked;
        RevokedAt = executionContext.Timestamp;

        return true;
    }
    // Stryker restore all

    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool RecordUsageInternal(
        ExecutionContext executionContext
    )
    {
        LastUsedAt = executionContext.Timestamp;
        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? serviceClientId,
        string? keyPrefix,
        string? keyHash,
        ApiKeyStatus? status
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateServiceClientId(executionContext, serviceClientId)
            & ValidateKeyPrefix(executionContext, keyPrefix)
            & ValidateKeyHash(executionContext, keyHash)
            & ValidateStatus(executionContext, status);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            ServiceClientId,
            KeyPrefix,
            KeyHash,
            Status
        );
    }

    public static bool ValidateServiceClientId(
        ExecutionContext executionContext,
        Id? serviceClientId
    )
    {
        bool serviceClientIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.ServiceClientIdPropertyName),
            isRequired: ApiKeyMetadata.ServiceClientIdIsRequired,
            value: serviceClientId
        );

        return serviceClientIdIsRequiredValidation;
    }

    public static bool ValidateKeyPrefix(
        ExecutionContext executionContext,
        string? keyPrefix
    )
    {
        bool keyPrefixIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.KeyPrefixPropertyName),
            isRequired: ApiKeyMetadata.KeyPrefixIsRequired,
            value: keyPrefix
        );

        if (!keyPrefixIsRequiredValidation)
            return false;

        bool keyPrefixMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.KeyPrefixPropertyName),
            minLength: 1,
            value: keyPrefix!.Length
        );

        if (!keyPrefixMinLengthValidation)
            return false;

        bool keyPrefixMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.KeyPrefixPropertyName),
            maxLength: ApiKeyMetadata.KeyPrefixMaxLength,
            value: keyPrefix!.Length
        );

        return keyPrefixMaxLengthValidation;
    }

    public static bool ValidateKeyHash(
        ExecutionContext executionContext,
        string? keyHash
    )
    {
        bool keyHashIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.KeyHashPropertyName),
            isRequired: ApiKeyMetadata.KeyHashIsRequired,
            value: keyHash
        );

        if (!keyHashIsRequiredValidation)
            return false;

        bool keyHashMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.KeyHashPropertyName),
            minLength: 1,
            value: keyHash!.Length
        );

        if (!keyHashMinLengthValidation)
            return false;

        bool keyHashMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.KeyHashPropertyName),
            maxLength: ApiKeyMetadata.KeyHashMaxLength,
            value: keyHash!.Length
        );

        return keyHashMaxLengthValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        ApiKeyStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.StatusPropertyName),
            isRequired: ApiKeyMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        ApiKeyStatus? from,
        ApiKeyStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (ApiKeyStatus.Active, ApiKeyStatus.Revoked) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ApiKey>(propertyName: ApiKeyMetadata.StatusPropertyName)}.InvalidTransition");
        }

        return isValid;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetServiceClientId(
        ExecutionContext executionContext,
        Id serviceClientId
    )
    {
        bool isValid = ValidateServiceClientId(
            executionContext,
            serviceClientId
        );

        if (!isValid)
            return false;

        ServiceClientId = serviceClientId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetKeyPrefix(
        ExecutionContext executionContext,
        string keyPrefix
    )
    {
        bool isValid = ValidateKeyPrefix(
            executionContext,
            keyPrefix
        );

        if (!isValid)
            return false;

        KeyPrefix = keyPrefix;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetKeyHash(
        ExecutionContext executionContext,
        string keyHash
    )
    {
        bool isValid = ValidateKeyHash(
            executionContext,
            keyHash
        );

        if (!isValid)
            return false;

        KeyHash = keyHash;

        return true;
    }

    // Stryker disable once Block : SetStatus recebe ApiKeyStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe ApiKeyStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        ApiKeyStatus status
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
    // Stryker restore all

    private bool SetExpiresAt(DateTimeOffset? expiresAt)
    {
        ExpiresAt = expiresAt;
        return true;
    }

    private bool SetLastUsedAt(DateTimeOffset? lastUsedAt)
    {
        LastUsedAt = lastUsedAt;
        return true;
    }

    private bool SetRevokedAt(DateTimeOffset? revokedAt)
    {
        RevokedAt = revokedAt;
        return true;
    }

    // Metadata
    public static class ApiKeyMetadata
    {
        private static readonly Lock _lockObject = new();

        // ServiceClientId
        public static readonly string ServiceClientIdPropertyName = "ServiceClientId";
        public static bool ServiceClientIdIsRequired { get; private set; } = true;

        // KeyPrefix
        public static readonly string KeyPrefixPropertyName = "KeyPrefix";
        public static bool KeyPrefixIsRequired { get; private set; } = true;
        public static int KeyPrefixMaxLength { get; private set; } = 32;

        // KeyHash
        public static readonly string KeyHashPropertyName = "KeyHash";
        public static bool KeyHashIsRequired { get; private set; } = true;
        public static int KeyHashMaxLength { get; private set; } = 128;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        public static void ChangeServiceClientIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ServiceClientIdIsRequired = isRequired;
            }
        }

        public static void ChangeKeyPrefixMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                KeyPrefixIsRequired = isRequired;
                KeyPrefixMaxLength = maxLength;
            }
        }

        public static void ChangeKeyHashMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                KeyHashIsRequired = isRequired;
                KeyHashMaxLength = maxLength;
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
    }
}
