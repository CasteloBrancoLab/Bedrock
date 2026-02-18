using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes.Inputs;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.RecoveryCodes;

public sealed class RecoveryCode
    : EntityBase<RecoveryCode>,
    IRecoveryCode
{
    // Properties
    public Id UserId { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string CodeHash { get; private set; } = string.Empty;
    public bool IsUsed { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }

    // Constructors
    private RecoveryCode()
    {
    }

    private RecoveryCode(
        EntityInfo entityInfo,
        Id userId,
        string codeHash,
        bool isUsed,
        DateTimeOffset? usedAt
    ) : base(entityInfo)
    {
        UserId = userId;
        CodeHash = codeHash;
        IsUsed = isUsed;
        UsedAt = usedAt;
    }

    // Public Business Methods
    public static RecoveryCode? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewRecoveryCodeInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new RecoveryCode(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetCodeHash(executionContext, input.CodeHash)
                    & instance.SetIsUsed(false)
                    & instance.SetUsedAt(null);
            }
        );
    }

    public static RecoveryCode CreateFromExistingInfo(
        CreateFromExistingInfoRecoveryCodeInput input
    )
    {
        return new RecoveryCode(
            input.EntityInfo,
            input.UserId,
            input.CodeHash,
            input.IsUsed,
            input.UsedAt
        );
    }

    public RecoveryCode? MarkUsed(
        ExecutionContext executionContext,
        MarkUsedRecoveryCodeInput input
    )
    {
        return RegisterChangeInternal<RecoveryCode, MarkUsedRecoveryCodeInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.MarkUsedInternal(executionContext);
            }
        );
    }

    public override RecoveryCode Clone()
    {
        return new RecoveryCode(
            EntityInfo,
            UserId,
            CodeHash,
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
                code: $"{CreateMessageCode<RecoveryCode>(propertyName: RecoveryCodeMetadata.IsUsedPropertyName)}.AlreadyUsed");
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
        string? codeHash
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateCodeHash(executionContext, codeHash);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            UserId,
            CodeHash
        );
    }

    public static bool ValidateUserId(
        ExecutionContext executionContext,
        Id? userId
    )
    {
        bool userIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RecoveryCode>(propertyName: RecoveryCodeMetadata.UserIdPropertyName),
            isRequired: RecoveryCodeMetadata.UserIdIsRequired,
            value: userId
        );

        return userIdIsRequiredValidation;
    }

    public static bool ValidateCodeHash(
        ExecutionContext executionContext,
        string? codeHash
    )
    {
        bool codeHashIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<RecoveryCode>(propertyName: RecoveryCodeMetadata.CodeHashPropertyName),
            isRequired: RecoveryCodeMetadata.CodeHashIsRequired,
            value: codeHash
        );

        if (!codeHashIsRequiredValidation)
            return false;

        bool codeHashMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<RecoveryCode>(propertyName: RecoveryCodeMetadata.CodeHashPropertyName),
            maxLength: RecoveryCodeMetadata.CodeHashMaxLength,
            value: codeHash!.Length
        );

        return codeHashMaxLengthValidation;
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
    private bool SetCodeHash(
        ExecutionContext executionContext,
        string codeHash
    )
    {
        bool isValid = ValidateCodeHash(
            executionContext,
            codeHash
        );

        if (!isValid)
            return false;

        CodeHash = codeHash;

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
    public static class RecoveryCodeMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // CodeHash
        public static readonly string CodeHashPropertyName = "CodeHash";
        public static bool CodeHashIsRequired { get; private set; } = true;
        public static int CodeHashMaxLength { get; private set; } = 128;

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

        public static void ChangeCodeHashMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                CodeHashIsRequired = isRequired;
                CodeHashMaxLength = maxLength;
            }
        }
    }
}
