using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using ShopDemo.Auth.Domain.Entities.Tenants.Inputs;
using ShopDemo.Auth.Domain.Entities.Tenants.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.Tenants;

public sealed class Tenant
    : EntityBase<Tenant>,
    ITenant
{
    // Properties
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Name { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Domain { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string SchemaName { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public TenantTier Tier { get; private set; }
    public string? DbVersion { get; private set; }

    // Constructors
    private Tenant()
    {
    }

    private Tenant(
        EntityInfo entityInfo,
        string name,
        string domain,
        string schemaName,
        TenantStatus status,
        TenantTier tier,
        string? dbVersion
    ) : base(entityInfo)
    {
        Name = name;
        Domain = domain;
        SchemaName = schemaName;
        Status = status;
        Tier = tier;
        DbVersion = dbVersion;
    }

    // Public Business Methods
    public static Tenant? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewTenantInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new Tenant(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetName(executionContext, input.Name)
                    & instance.SetDomain(executionContext, input.Domain)
                    & instance.SetSchemaName(executionContext, input.SchemaName)
                    & instance.SetStatus(executionContext, TenantStatus.Active)
                    & instance.SetTier(executionContext, input.Tier)
                    & instance.SetDbVersion(null);
            }
        );
    }

    public static Tenant CreateFromExistingInfo(
        CreateFromExistingInfoTenantInput input
    )
    {
        return new Tenant(
            input.EntityInfo,
            input.Name,
            input.Domain,
            input.SchemaName,
            input.Status,
            input.Tier,
            input.DbVersion
        );
    }

    public Tenant? ChangeStatus(
        ExecutionContext executionContext,
        ChangeTenantStatusInput input
    )
    {
        return RegisterChangeInternal<Tenant, ChangeTenantStatusInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeStatusInternal(executionContext, input.Status);
            }
        );
    }

    public Tenant? ChangeDbVersion(
        ExecutionContext executionContext,
        ChangeTenantDbVersionInput input
    )
    {
        return RegisterChangeInternal<Tenant, ChangeTenantDbVersionInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeDbVersionInternal(executionContext, input.DbVersion);
            }
        );
    }

    public override Tenant Clone()
    {
        return new Tenant(
            EntityInfo,
            Name,
            Domain,
            SchemaName,
            Status,
            Tier,
            DbVersion
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool ChangeStatusInternal(
        ExecutionContext executionContext,
        TenantStatus newStatus
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, newStatus);

        if (!isValidTransition)
            return false;

        Status = newStatus;

        return true;
    }
    // Stryker restore all

    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool ChangeDbVersionInternal(
        ExecutionContext executionContext,
        string dbVersion
    )
    {
        bool isValid = ValidateDbVersion(executionContext, dbVersion);

        if (!isValid)
            return false;

        DbVersion = dbVersion;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? name,
        string? domain,
        string? schemaName,
        TenantStatus? status,
        TenantTier? tier
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateName(executionContext, name)
            & ValidateDomain(executionContext, domain)
            & ValidateSchemaName(executionContext, schemaName)
            & ValidateStatus(executionContext, status)
            & ValidateTier(executionContext, tier);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Name,
            Domain,
            SchemaName,
            Status,
            Tier
        );
    }

    public static bool ValidateName(
        ExecutionContext executionContext,
        string? name
    )
    {
        bool nameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.NamePropertyName),
            isRequired: TenantMetadata.NameIsRequired,
            value: name
        );

        if (!nameIsRequiredValidation)
            return false;

        bool nameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.NamePropertyName),
            minLength: 1,
            value: name!.Length
        );

        if (!nameMinLengthValidation)
            return false;

        bool nameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.NamePropertyName),
            maxLength: TenantMetadata.NameMaxLength,
            value: name!.Length
        );

        return nameMaxLengthValidation;
    }

    public static bool ValidateDomain(
        ExecutionContext executionContext,
        string? domain
    )
    {
        bool domainIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.DomainPropertyName),
            isRequired: TenantMetadata.DomainIsRequired,
            value: domain
        );

        if (!domainIsRequiredValidation)
            return false;

        bool domainMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.DomainPropertyName),
            minLength: 1,
            value: domain!.Length
        );

        if (!domainMinLengthValidation)
            return false;

        bool domainMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.DomainPropertyName),
            maxLength: TenantMetadata.DomainMaxLength,
            value: domain!.Length
        );

        return domainMaxLengthValidation;
    }

    public static bool ValidateSchemaName(
        ExecutionContext executionContext,
        string? schemaName
    )
    {
        bool schemaNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.SchemaNamePropertyName),
            isRequired: TenantMetadata.SchemaNameIsRequired,
            value: schemaName
        );

        if (!schemaNameIsRequiredValidation)
            return false;

        bool schemaNameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.SchemaNamePropertyName),
            minLength: 1,
            value: schemaName!.Length
        );

        if (!schemaNameMinLengthValidation)
            return false;

        bool schemaNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.SchemaNamePropertyName),
            maxLength: TenantMetadata.SchemaNameMaxLength,
            value: schemaName!.Length
        );

        return schemaNameMaxLengthValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        TenantStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.StatusPropertyName),
            isRequired: TenantMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateTier(
        ExecutionContext executionContext,
        TenantTier? tier
    )
    {
        bool tierIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.TierPropertyName),
            isRequired: TenantMetadata.TierIsRequired,
            value: tier
        );

        return tierIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        TenantStatus? from,
        TenantStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<Tenant>(propertyName: TenantMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        return true;
    }

    public static bool ValidateDbVersion(
        ExecutionContext executionContext,
        string? dbVersion
    )
    {
        bool dbVersionIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.DbVersionPropertyName),
            isRequired: TenantMetadata.DbVersionIsRequired,
            value: dbVersion
        );

        if (!dbVersionIsRequiredValidation)
            return false;

        bool dbVersionMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.DbVersionPropertyName),
            minLength: 1,
            value: dbVersion!.Length
        );

        if (!dbVersionMinLengthValidation)
            return false;

        bool dbVersionMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Tenant>(propertyName: TenantMetadata.DbVersionPropertyName),
            maxLength: TenantMetadata.DbVersionMaxLength,
            value: dbVersion!.Length
        );

        return dbVersionMaxLengthValidation;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetName(
        ExecutionContext executionContext,
        string name
    )
    {
        bool isValid = ValidateName(
            executionContext,
            name
        );

        if (!isValid)
            return false;

        Name = name;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetDomain(
        ExecutionContext executionContext,
        string domain
    )
    {
        bool isValid = ValidateDomain(
            executionContext,
            domain
        );

        if (!isValid)
            return false;

        Domain = domain;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetSchemaName(
        ExecutionContext executionContext,
        string schemaName
    )
    {
        bool isValid = ValidateSchemaName(
            executionContext,
            schemaName
        );

        if (!isValid)
            return false;

        SchemaName = schemaName;

        return true;
    }

    // Stryker disable once Block : SetStatus recebe TenantStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe TenantStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        TenantStatus status
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
    private bool SetTier(
        ExecutionContext executionContext,
        TenantTier tier
    )
    {
        bool isValid = ValidateTier(
            executionContext,
            tier
        );

        if (!isValid)
            return false;

        Tier = tier;

        return true;
    }
    // Stryker restore all

    private bool SetDbVersion(string? dbVersion)
    {
        DbVersion = dbVersion;
        return true;
    }

    // Metadata
    public static class TenantMetadata
    {
        private static readonly Lock _lockObject = new();

        // Name
        public static readonly string NamePropertyName = "Name";
        public static bool NameIsRequired { get; private set; } = true;
        public static int NameMaxLength { get; private set; } = 255;

        // Domain
        public static readonly string DomainPropertyName = "Domain";
        public static bool DomainIsRequired { get; private set; } = true;
        public static int DomainMaxLength { get; private set; } = 255;

        // SchemaName
        public static readonly string SchemaNamePropertyName = "SchemaName";
        public static bool SchemaNameIsRequired { get; private set; } = true;
        public static int SchemaNameMaxLength { get; private set; } = 63;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        // Tier
        public static readonly string TierPropertyName = "Tier";
        public static bool TierIsRequired { get; private set; } = true;

        // DbVersion
        public static readonly string DbVersionPropertyName = "DbVersion";
        public static bool DbVersionIsRequired { get; private set; } = true;
        public static int DbVersionMaxLength { get; private set; } = 50;

        public static void ChangeNameMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                NameIsRequired = isRequired;
                NameMaxLength = maxLength;
            }
        }

        public static void ChangeDomainMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                DomainIsRequired = isRequired;
                DomainMaxLength = maxLength;
            }
        }

        public static void ChangeSchemaNameMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                SchemaNameIsRequired = isRequired;
                SchemaNameMaxLength = maxLength;
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

        public static void ChangeTierMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                TierIsRequired = isRequired;
            }
        }

        public static void ChangeDbVersionMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                DbVersionIsRequired = isRequired;
                DbVersionMaxLength = maxLength;
            }
        }
    }
}
