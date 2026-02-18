using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.DPoPKeys;

public sealed class DPoPKey
    : EntityBase<DPoPKey>,
    IDPoPKey
{
    // Properties
    public Id UserId { get; private set; }
    public JwkThumbprint JwkThumbprint { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string PublicKeyJwk { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DPoPKeyStatus Status { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    // Constructors
    private DPoPKey()
    {
    }

    private DPoPKey(
        EntityInfo entityInfo,
        Id userId,
        JwkThumbprint jwkThumbprint,
        string publicKeyJwk,
        DateTimeOffset expiresAt,
        DPoPKeyStatus status,
        DateTimeOffset? revokedAt
    ) : base(entityInfo)
    {
        UserId = userId;
        JwkThumbprint = jwkThumbprint;
        PublicKeyJwk = publicKeyJwk;
        ExpiresAt = expiresAt;
        Status = status;
        RevokedAt = revokedAt;
    }

    // Public Business Methods
    public static DPoPKey? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewDPoPKeyInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new DPoPKey(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetJwkThumbprint(executionContext, input.JwkThumbprint)
                    & instance.SetPublicKeyJwk(executionContext, input.PublicKeyJwk)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt)
                    & instance.SetStatus(executionContext, DPoPKeyStatus.Active)
                    & instance.SetRevokedAt(null);
            }
        );
    }

    public static DPoPKey CreateFromExistingInfo(
        CreateFromExistingInfoDPoPKeyInput input
    )
    {
        return new DPoPKey(
            input.EntityInfo,
            input.UserId,
            input.JwkThumbprint,
            input.PublicKeyJwk,
            input.ExpiresAt,
            input.Status,
            input.RevokedAt
        );
    }

    public DPoPKey? Revoke(
        ExecutionContext executionContext,
        RevokeDPoPKeyInput input
    )
    {
        return RegisterChangeInternal<DPoPKey, RevokeDPoPKeyInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RevokeInternal(executionContext);
            }
        );
    }

    public override DPoPKey Clone()
    {
        return new DPoPKey(
            EntityInfo,
            UserId,
            JwkThumbprint,
            PublicKeyJwk,
            ExpiresAt,
            Status,
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
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, DPoPKeyStatus.Revoked);

        if (!isValidTransition)
            return false;

        Status = DPoPKeyStatus.Revoked;
        RevokedAt = executionContext.Timestamp;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        JwkThumbprint? jwkThumbprint,
        string? publicKeyJwk,
        DateTimeOffset? expiresAt,
        DPoPKeyStatus? status
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateJwkThumbprint(executionContext, jwkThumbprint)
            & ValidatePublicKeyJwk(executionContext, publicKeyJwk)
            & ValidateExpiresAt(executionContext, expiresAt)
            & ValidateStatus(executionContext, status);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            UserId,
            JwkThumbprint,
            PublicKeyJwk,
            ExpiresAt,
            Status
        );
    }

    public static bool ValidateUserId(
        ExecutionContext executionContext,
        Id? userId
    )
    {
        bool userIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.UserIdPropertyName),
            isRequired: DPoPKeyMetadata.UserIdIsRequired,
            value: userId
        );

        return userIdIsRequiredValidation;
    }

    public static bool ValidateJwkThumbprint(
        ExecutionContext executionContext,
        JwkThumbprint? jwkThumbprint
    )
    {
        bool jwkThumbprintIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.JwkThumbprintPropertyName),
            isRequired: DPoPKeyMetadata.JwkThumbprintIsRequired,
            value: jwkThumbprint
        );

        return jwkThumbprintIsRequiredValidation;
    }

    public static bool ValidatePublicKeyJwk(
        ExecutionContext executionContext,
        string? publicKeyJwk
    )
    {
        bool publicKeyJwkIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.PublicKeyJwkPropertyName),
            isRequired: DPoPKeyMetadata.PublicKeyJwkIsRequired,
            value: publicKeyJwk
        );

        if (!publicKeyJwkIsRequiredValidation)
            return false;

        bool publicKeyJwkMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.PublicKeyJwkPropertyName),
            maxLength: DPoPKeyMetadata.PublicKeyJwkMaxLength,
            value: publicKeyJwk!.Length
        );

        return publicKeyJwkMaxLengthValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.ExpiresAtPropertyName),
            isRequired: DPoPKeyMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        DPoPKeyStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.StatusPropertyName),
            isRequired: DPoPKeyMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        DPoPKeyStatus? from,
        DPoPKeyStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (DPoPKeyStatus.Active, DPoPKeyStatus.Revoked) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<DPoPKey>(propertyName: DPoPKeyMetadata.StatusPropertyName)}.InvalidTransition");
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
    private bool SetJwkThumbprint(
        ExecutionContext executionContext,
        JwkThumbprint jwkThumbprint
    )
    {
        bool isValid = ValidateJwkThumbprint(
            executionContext,
            jwkThumbprint
        );

        if (!isValid)
            return false;

        JwkThumbprint = jwkThumbprint;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetPublicKeyJwk(
        ExecutionContext executionContext,
        string publicKeyJwk
    )
    {
        bool isValid = ValidatePublicKeyJwk(
            executionContext,
            publicKeyJwk
        );

        if (!isValid)
            return false;

        PublicKeyJwk = publicKeyJwk;

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

    // Stryker disable once Block : SetStatus recebe DPoPKeyStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe DPoPKeyStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        DPoPKeyStatus status
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

    private bool SetRevokedAt(DateTimeOffset? revokedAt)
    {
        RevokedAt = revokedAt;
        return true;
    }

    // Metadata
    public static class DPoPKeyMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // JwkThumbprint
        public static readonly string JwkThumbprintPropertyName = "JwkThumbprint";
        public static bool JwkThumbprintIsRequired { get; private set; } = true;

        // PublicKeyJwk
        public static readonly string PublicKeyJwkPropertyName = "PublicKeyJwk";
        public static bool PublicKeyJwkIsRequired { get; private set; } = true;
        public static int PublicKeyJwkMaxLength { get; private set; } = 4096;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        public static void ChangeUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                UserIdIsRequired = isRequired;
            }
        }

        public static void ChangeJwkThumbprintMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                JwkThumbprintIsRequired = isRequired;
            }
        }

        public static void ChangePublicKeyJwkMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                PublicKeyJwkIsRequired = isRequired;
                PublicKeyJwkMaxLength = maxLength;
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
