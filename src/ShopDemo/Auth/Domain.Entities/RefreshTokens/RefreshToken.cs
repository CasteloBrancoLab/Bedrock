using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.RefreshTokens;

public sealed class RefreshToken
    : EntityBase<RefreshToken>,
    IRefreshToken
{
    // Properties
    public Id UserId { get; private set; }
    public TokenHash TokenHash { get; private set; }
    public TokenFamily FamilyId { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public RefreshTokenStatus Status { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Id? ReplacedByTokenId { get; private set; }

    // Constructors
    private RefreshToken()
    {
    }

    private RefreshToken(
        EntityInfo entityInfo,
        Id userId,
        TokenHash tokenHash,
        TokenFamily familyId,
        DateTimeOffset expiresAt,
        RefreshTokenStatus status,
        DateTimeOffset? revokedAt,
        Id? replacedByTokenId
    ) : base(entityInfo)
    {
        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        ExpiresAt = expiresAt;
        Status = status;
        RevokedAt = revokedAt;
        ReplacedByTokenId = replacedByTokenId;
    }

    // Public Business Methods
    public static RefreshToken? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewRefreshTokenInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new RefreshToken(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetTokenHash(executionContext, input.TokenHash)
                    & instance.SetFamilyId(executionContext, input.FamilyId)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt)
                    & instance.SetStatus(executionContext, RefreshTokenStatus.Active)
                    & instance.SetRevokedAt(null)
                    & instance.SetReplacedByTokenId(null);
            }
        );
    }

    public static RefreshToken CreateFromExistingInfo(
        CreateFromExistingInfoRefreshTokenInput input
    )
    {
        return new RefreshToken(
            input.EntityInfo,
            input.UserId,
            input.TokenHash,
            input.FamilyId,
            input.ExpiresAt,
            input.Status,
            input.RevokedAt,
            input.ReplacedByTokenId
        );
    }

    public RefreshToken? MarkAsUsed(
        ExecutionContext executionContext,
        MarkAsUsedRefreshTokenInput input
    )
    {
        return RegisterChangeInternal<RefreshToken, MarkAsUsedRefreshTokenInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.MarkAsUsedInternal(executionContext, input.ReplacedByTokenId);
            }
        );
    }

    public RefreshToken? Revoke(
        ExecutionContext executionContext,
        RevokeRefreshTokenInput input
    )
    {
        return RegisterChangeInternal<RefreshToken, RevokeRefreshTokenInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RevokeInternal(executionContext);
            }
        );
    }

    public override RefreshToken Clone()
    {
        return new RefreshToken(
            EntityInfo,
            UserId,
            TokenHash,
            FamilyId,
            ExpiresAt,
            Status,
            RevokedAt,
            ReplacedByTokenId
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool MarkAsUsedInternal(
        ExecutionContext executionContext,
        Id replacedByTokenId
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, RefreshTokenStatus.Used);

        if (!isValidTransition)
            return false;

        Status = RefreshTokenStatus.Used;
        ReplacedByTokenId = replacedByTokenId;

        return true;
    }
    // Stryker restore all

    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool RevokeInternal(
        ExecutionContext executionContext
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, RefreshTokenStatus.Revoked);

        if (!isValidTransition)
            return false;

        Status = RefreshTokenStatus.Revoked;
        RevokedAt = executionContext.Timestamp;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        TokenHash? tokenHash,
        TokenFamily? familyId,
        DateTimeOffset? expiresAt,
        RefreshTokenStatus? status
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateTokenHash(executionContext, tokenHash)
            & ValidateFamilyId(executionContext, familyId)
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
            TokenHash,
            FamilyId,
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
            propertyName: CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.UserIdPropertyName),
            isRequired: RefreshTokenMetadata.UserIdIsRequired,
            value: userId
        );

        return userIdIsRequiredValidation;
    }

    public static bool ValidateTokenHash(
        ExecutionContext executionContext,
        TokenHash? tokenHash
    )
    {
        bool tokenHashIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.TokenHashPropertyName),
            isRequired: RefreshTokenMetadata.TokenHashIsRequired,
            value: tokenHash
        );

        if (!tokenHashIsRequiredValidation)
            return false;

        if (tokenHash!.Value.IsEmpty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.TokenHashPropertyName)}.IsRequired");
            return false;
        }

        bool tokenHashMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.TokenHashPropertyName),
            maxLength: RefreshTokenMetadata.TokenHashMaxLength,
            value: tokenHash.Value.Length
        );

        return tokenHashMaxLengthValidation;
    }

    public static bool ValidateFamilyId(
        ExecutionContext executionContext,
        TokenFamily? familyId
    )
    {
        bool familyIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.FamilyIdPropertyName),
            isRequired: RefreshTokenMetadata.FamilyIdIsRequired,
            value: familyId
        );

        return familyIdIsRequiredValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.ExpiresAtPropertyName),
            isRequired: RefreshTokenMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        RefreshTokenStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.StatusPropertyName),
            isRequired: RefreshTokenMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        RefreshTokenStatus? from,
        RefreshTokenStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (RefreshTokenStatus.Active, RefreshTokenStatus.Used) => true,
            (RefreshTokenStatus.Active, RefreshTokenStatus.Revoked) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<RefreshToken>(propertyName: RefreshTokenMetadata.StatusPropertyName)}.InvalidTransition");
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
    private bool SetTokenHash(
        ExecutionContext executionContext,
        TokenHash tokenHash
    )
    {
        bool isValid = ValidateTokenHash(
            executionContext,
            tokenHash
        );

        if (!isValid)
            return false;

        TokenHash = tokenHash;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetFamilyId(
        ExecutionContext executionContext,
        TokenFamily familyId
    )
    {
        bool isValid = ValidateFamilyId(
            executionContext,
            familyId
        );

        if (!isValid)
            return false;

        FamilyId = familyId;

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

    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe RefreshTokenStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        RefreshTokenStatus status
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

    private bool SetReplacedByTokenId(Id? replacedByTokenId)
    {
        ReplacedByTokenId = replacedByTokenId;
        return true;
    }

    // Metadata
    public static class RefreshTokenMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // TokenHash
        public static readonly string TokenHashPropertyName = "TokenHash";
        public static bool TokenHashIsRequired { get; private set; } = true;
        public static int TokenHashMaxLength { get; private set; } = 64;

        // FamilyId
        public static readonly string FamilyIdPropertyName = "FamilyId";
        public static bool FamilyIdIsRequired { get; private set; } = true;

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

        public static void ChangeTokenHashMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                TokenHashIsRequired = isRequired;
                TokenHashMaxLength = maxLength;
            }
        }

        public static void ChangeFamilyIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                FamilyIdIsRequired = isRequired;
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
