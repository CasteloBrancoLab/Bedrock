using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.PasswordHistories.Inputs;
using ShopDemo.Auth.Domain.Entities.PasswordHistories.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.PasswordHistories;

public sealed class PasswordHistory
    : EntityBase<PasswordHistory>,
    IPasswordHistory
{
    // Properties
    public Id UserId { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; private set; }

    // Constructors
    private PasswordHistory()
    {
    }

    private PasswordHistory(
        EntityInfo entityInfo,
        Id userId,
        string passwordHash,
        DateTimeOffset changedAt
    ) : base(entityInfo)
    {
        UserId = userId;
        PasswordHash = passwordHash;
        ChangedAt = changedAt;
    }

    // Public Business Methods
    public static PasswordHistory? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewPasswordHistoryInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new PasswordHistory(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetPasswordHash(executionContext, input.PasswordHash)
                    & instance.SetChangedAt(executionContext.Timestamp);
            }
        );
    }

    public static PasswordHistory CreateFromExistingInfo(
        CreateFromExistingInfoPasswordHistoryInput input
    )
    {
        return new PasswordHistory(
            input.EntityInfo,
            input.UserId,
            input.PasswordHash,
            input.ChangedAt
        );
    }

    public override PasswordHistory Clone()
    {
        return new PasswordHistory(
            EntityInfo,
            UserId,
            PasswordHash,
            ChangedAt
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        string? passwordHash,
        DateTimeOffset? changedAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidatePasswordHash(executionContext, passwordHash)
            & ValidateChangedAt(executionContext, changedAt);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            UserId,
            PasswordHash,
            ChangedAt
        );
    }

    public static bool ValidateUserId(
        ExecutionContext executionContext,
        Id? userId
    )
    {
        bool userIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<PasswordHistory>(propertyName: PasswordHistoryMetadata.UserIdPropertyName),
            isRequired: PasswordHistoryMetadata.UserIdIsRequired,
            value: userId
        );

        return userIdIsRequiredValidation;
    }

    public static bool ValidatePasswordHash(
        ExecutionContext executionContext,
        string? passwordHash
    )
    {
        bool passwordHashIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<PasswordHistory>(propertyName: PasswordHistoryMetadata.PasswordHashPropertyName),
            isRequired: PasswordHistoryMetadata.PasswordHashIsRequired,
            value: passwordHash
        );

        if (!passwordHashIsRequiredValidation)
            return false;

        bool passwordHashMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<PasswordHistory>(propertyName: PasswordHistoryMetadata.PasswordHashPropertyName),
            maxLength: PasswordHistoryMetadata.PasswordHashMaxLength,
            value: passwordHash!.Length
        );

        return passwordHashMaxLengthValidation;
    }

    public static bool ValidateChangedAt(
        ExecutionContext executionContext,
        DateTimeOffset? changedAt
    )
    {
        bool changedAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<PasswordHistory>(propertyName: PasswordHistoryMetadata.ChangedAtPropertyName),
            isRequired: PasswordHistoryMetadata.ChangedAtIsRequired,
            value: changedAt
        );

        return changedAtIsRequiredValidation;
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
    private bool SetPasswordHash(
        ExecutionContext executionContext,
        string passwordHash
    )
    {
        bool isValid = ValidatePasswordHash(
            executionContext,
            passwordHash
        );

        if (!isValid)
            return false;

        PasswordHash = passwordHash;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetChangedAt(
        DateTimeOffset changedAt
    )
    {
        ChangedAt = changedAt;
        return true;
    }
    // Stryker restore all

    // Metadata
    public static class PasswordHistoryMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // PasswordHash
        public static readonly string PasswordHashPropertyName = "PasswordHash";
        public static bool PasswordHashIsRequired { get; private set; } = true;
        public static int PasswordHashMaxLength { get; private set; } = 1024;

        // ChangedAt
        public static readonly string ChangedAtPropertyName = "ChangedAt";
        public static bool ChangedAtIsRequired { get; private set; } = true;

        public static void ChangeUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                UserIdIsRequired = isRequired;
            }
        }

        public static void ChangePasswordHashMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                PasswordHashIsRequired = isRequired;
                PasswordHashMaxLength = maxLength;
            }
        }

        public static void ChangeChangedAtMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ChangedAtIsRequired = isRequired;
            }
        }
    }
}
