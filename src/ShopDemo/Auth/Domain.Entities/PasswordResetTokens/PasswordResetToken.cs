using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens.Inputs;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.PasswordResetTokens;

public sealed class PasswordResetToken
    : EntityBase<PasswordResetToken>,
    IPasswordResetToken
{
    // Properties
    public Id UserId { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }

    // Constructors
    private PasswordResetToken()
    {
    }

    private PasswordResetToken(
        EntityInfo entityInfo,
        Id userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        bool isUsed,
        DateTimeOffset? usedAt
    ) : base(entityInfo)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        IsUsed = isUsed;
        UsedAt = usedAt;
    }

    // Public Business Methods
    public static PasswordResetToken? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewPasswordResetTokenInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new PasswordResetToken(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetTokenHash(executionContext, input.TokenHash)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt)
                    & instance.SetIsUsed(false)
                    & instance.SetUsedAt(null);
            }
        );
    }

    public static PasswordResetToken CreateFromExistingInfo(
        CreateFromExistingInfoPasswordResetTokenInput input
    )
    {
        return new PasswordResetToken(
            input.EntityInfo,
            input.UserId,
            input.TokenHash,
            input.ExpiresAt,
            input.IsUsed,
            input.UsedAt
        );
    }

    public PasswordResetToken? MarkUsed(
        ExecutionContext executionContext,
        MarkUsedPasswordResetTokenInput input
    )
    {
        return RegisterChangeInternal<PasswordResetToken, MarkUsedPasswordResetTokenInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.MarkUsedInternal(executionContext);
            }
        );
    }

    public override PasswordResetToken Clone()
    {
        return new PasswordResetToken(
            EntityInfo,
            UserId,
            TokenHash,
            ExpiresAt,
            IsUsed,
            UsedAt
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool MarkUsedInternal(
        ExecutionContext executionContext
    )
    {
        if (IsUsed)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<PasswordResetToken>(propertyName: PasswordResetTokenMetadata.IsUsedPropertyName)}.AlreadyUsed");
            return false;
        }

        IsUsed = true;
        UsedAt = executionContext.Timestamp;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        string? tokenHash,
        DateTimeOffset? expiresAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateTokenHash(executionContext, tokenHash)
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
            TokenHash,
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
            propertyName: CreateMessageCode<PasswordResetToken>(propertyName: PasswordResetTokenMetadata.UserIdPropertyName),
            isRequired: PasswordResetTokenMetadata.UserIdIsRequired,
            value: userId
        );

        if (!userIdIsRequiredValidation)
            return false;

        if (userId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<PasswordResetToken>(propertyName: PasswordResetTokenMetadata.UserIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateTokenHash(
        ExecutionContext executionContext,
        string? tokenHash
    )
    {
        bool tokenHashIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<PasswordResetToken>(propertyName: PasswordResetTokenMetadata.TokenHashPropertyName),
            isRequired: PasswordResetTokenMetadata.TokenHashIsRequired,
            value: tokenHash
        );

        if (!tokenHashIsRequiredValidation)
            return false;

        bool tokenHashMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<PasswordResetToken>(propertyName: PasswordResetTokenMetadata.TokenHashPropertyName),
            minLength: 1,
            value: tokenHash!.Length
        );

        if (!tokenHashMinLengthValidation)
            return false;

        bool tokenHashMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<PasswordResetToken>(propertyName: PasswordResetTokenMetadata.TokenHashPropertyName),
            maxLength: PasswordResetTokenMetadata.TokenHashMaxLength,
            value: tokenHash!.Length
        );

        return tokenHashMaxLengthValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<PasswordResetToken>(propertyName: PasswordResetTokenMetadata.ExpiresAtPropertyName),
            isRequired: PasswordResetTokenMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
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
        string tokenHash
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

    private bool SetIsUsed(bool isUsed)
    {
        IsUsed = isUsed;
        return true;
    }

    private bool SetUsedAt(DateTimeOffset? usedAt)
    {
        UsedAt = usedAt;
        return true;
    }

    // Metadata
    public static class PasswordResetTokenMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // TokenHash
        public static readonly string TokenHashPropertyName = "TokenHash";
        public static bool TokenHashIsRequired { get; private set; } = true;
        public static int TokenHashMaxLength { get; private set; } = 128;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        // IsUsed
        public static readonly string IsUsedPropertyName = "IsUsed";

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
