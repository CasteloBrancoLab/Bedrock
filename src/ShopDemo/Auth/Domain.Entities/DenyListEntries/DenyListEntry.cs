using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Inputs;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.DenyListEntries;

public sealed class DenyListEntry
    : EntityBase<DenyListEntry>,
    IDenyListEntry
{
    // Properties
    public DenyListEntryType Type { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Value { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public string? Reason { get; private set; }

    // Constructors
    private DenyListEntry()
    {
    }

    private DenyListEntry(
        EntityInfo entityInfo,
        DenyListEntryType type,
        string value,
        DateTimeOffset expiresAt,
        string? reason
    ) : base(entityInfo)
    {
        Type = type;
        Value = value;
        ExpiresAt = expiresAt;
        Reason = reason;
    }

    // Public Business Methods
    public static DenyListEntry? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewDenyListEntryInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new DenyListEntry(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetType(executionContext, input.Type)
                    & instance.SetValue(executionContext, input.Value)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt)
                    & instance.SetReason(input.Reason);
            }
        );
    }

    public static DenyListEntry CreateFromExistingInfo(
        CreateFromExistingInfoDenyListEntryInput input
    )
    {
        return new DenyListEntry(
            input.EntityInfo,
            input.Type,
            input.Value,
            input.ExpiresAt,
            input.Reason
        );
    }

    public override DenyListEntry Clone()
    {
        return new DenyListEntry(
            EntityInfo,
            Type,
            Value,
            ExpiresAt,
            Reason
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        DenyListEntryType? type,
        string? value,
        DateTimeOffset? expiresAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateType(executionContext, type)
            & ValidateValue(executionContext, value)
            & ValidateExpiresAt(executionContext, expiresAt);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Type,
            Value,
            ExpiresAt
        );
    }

    public static bool ValidateType(
        ExecutionContext executionContext,
        DenyListEntryType? type
    )
    {
        bool typeIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DenyListEntry>(propertyName: DenyListEntryMetadata.TypePropertyName),
            isRequired: DenyListEntryMetadata.TypeIsRequired,
            value: type
        );

        return typeIsRequiredValidation;
    }

    public static bool ValidateValue(
        ExecutionContext executionContext,
        string? value
    )
    {
        bool valueIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DenyListEntry>(propertyName: DenyListEntryMetadata.ValuePropertyName),
            isRequired: DenyListEntryMetadata.ValueIsRequired,
            value: value
        );

        if (!valueIsRequiredValidation)
            return false;

        bool valueMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<DenyListEntry>(propertyName: DenyListEntryMetadata.ValuePropertyName),
            maxLength: DenyListEntryMetadata.ValueMaxLength,
            value: value!.Length
        );

        return valueMaxLengthValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<DenyListEntry>(propertyName: DenyListEntryMetadata.ExpiresAtPropertyName),
            isRequired: DenyListEntryMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetType(
        ExecutionContext executionContext,
        DenyListEntryType type
    )
    {
        bool isValid = ValidateType(
            executionContext,
            type
        );

        if (!isValid)
            return false;

        Type = type;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetValue(
        ExecutionContext executionContext,
        string value
    )
    {
        bool isValid = ValidateValue(
            executionContext,
            value
        );

        if (!isValid)
            return false;

        Value = value;

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

    private bool SetReason(string? reason)
    {
        Reason = reason;
        return true;
    }

    // Metadata
    public static class DenyListEntryMetadata
    {
        private static readonly Lock _lockObject = new();

        // Type
        public static readonly string TypePropertyName = "Type";
        public static bool TypeIsRequired { get; private set; } = true;

        // Value
        public static readonly string ValuePropertyName = "Value";
        public static bool ValueIsRequired { get; private set; } = true;
        public static int ValueMaxLength { get; private set; } = 1024;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        // Reason
        public static readonly string ReasonPropertyName = "Reason";
        public static int ReasonMaxLength { get; private set; } = 1000;

        public static void ChangeTypeMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                TypeIsRequired = isRequired;
            }
        }

        public static void ChangeValueMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ValueIsRequired = isRequired;
                ValueMaxLength = maxLength;
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

        public static void ChangeReasonMetadata(
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ReasonMaxLength = maxLength;
            }
        }
    }
}
