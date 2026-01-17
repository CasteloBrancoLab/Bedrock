using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Interfaces;
using Bedrock.BuildingBlocks.Domain.Models;

namespace Bedrock.BuildingBlocks.Domain;

/// <summary>
/// Abstract base class for all domain entities.
/// </summary>
/// <remarks>
/// Provides:
/// - Entity metadata management (Id, TenantInfo, audit, version)
/// - Validation infrastructure with configurable metadata rules
/// - Multi-tenancy validation
/// - Message code generation for validation errors
/// </remarks>
public abstract class EntityBase : IEntity
{
    /// <summary>
    /// Message code for tenant mismatch errors.
    /// </summary>
    public const string TenantMismatchMessageCode = "TenantMismatch";

    /// <summary>
    /// Provides configurable metadata for entity validation rules.
    /// </summary>
    public static class EntityBaseMetadata
    {
        // Id
        public static string IdPropertyName { get; } = nameof(EntityInfo.Id);
        public static bool IdIsRequired { get; private set; } = true;

        // TenantCode
        public static string TenantCodePropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(TenantInfo.Code)}";
        public static bool TenantCodeIsRequired { get; private set; } = true;

        // Creation Info
        public static string CreatedAtPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(EntityChangeInfo.CreatedAt)}";
        public static bool CreatedAtIsRequired { get; private set; } = true;

        public static string CreatedByPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(EntityChangeInfo.CreatedBy)}";
        public static bool CreatedByIsRequired { get; private set; } = true;
        public static int CreatedByMinLength { get; private set; } = 1;
        public static int CreatedByMaxLength { get; private set; } = 255;

        // Update Info
        public static string LastChangedAtPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(EntityChangeInfo.LastChangedAt)}";
        public static bool LastChangedAtIsRequired { get; private set; }

        public static string LastChangedByPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(EntityChangeInfo.LastChangedBy)}";
        public static bool LastChangedByIsRequired { get; private set; }
        public static int LastChangedByMinLength { get; private set; } = 1;
        public static int LastChangedByMaxLength { get; private set; } = 255;

        // Correlation Info
        public static string CreatedCorrelationIdPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(EntityChangeInfo.CreatedCorrelationId)}";
        public static bool CreatedCorrelationIdIsRequired { get; private set; } = true;

        public static string LastChangedCorrelationIdPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(EntityChangeInfo.LastChangedCorrelationId)}";
        public static bool LastChangedCorrelationIdIsRequired { get; private set; }

        // ExecutionOrigin Info
        public static string CreatedExecutionOriginPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(EntityChangeInfo.CreatedExecutionOrigin)}";
        public static bool CreatedExecutionOriginIsRequired { get; private set; } = true;
        public static int CreatedExecutionOriginMinLength { get; private set; } = 1;
        public static int CreatedExecutionOriginMaxLength { get; private set; } = 255;

        public static string LastChangedExecutionOriginPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityChangeInfo)}.{nameof(EntityChangeInfo.LastChangedExecutionOrigin)}";
        public static bool LastChangedExecutionOriginIsRequired { get; private set; }
        public static int LastChangedExecutionOriginMinLength { get; private set; } = 1;
        public static int LastChangedExecutionOriginMaxLength { get; private set; } = 255;

        // Registry Version
        public static string EntityVersionPropertyName { get; } =
            $"{nameof(EntityInfo)}.{nameof(EntityInfo.EntityVersion)}";
        public static bool EntityVersionIsRequired { get; private set; } = true;

        /// <summary>
        /// Changes the Id metadata validation rules.
        /// </summary>
        public static void ChangeIdMetadata(bool isRequired)
        {
            IdIsRequired = isRequired;
        }

        /// <summary>
        /// Changes the TenantCode metadata validation rules.
        /// </summary>
        public static void ChangeTenantCodeMetadata(bool isRequired)
        {
            TenantCodeIsRequired = isRequired;
        }

        /// <summary>
        /// Changes the creation info metadata validation rules.
        /// </summary>
        public static void ChangeCreationInfoMetadata(
            bool createdAtIsRequired,
            bool createdByIsRequired,
            int createdByMinLength,
            int createdByMaxLength)
        {
            CreatedAtIsRequired = createdAtIsRequired;
            CreatedByIsRequired = createdByIsRequired;
            CreatedByMinLength = createdByMinLength;
            CreatedByMaxLength = createdByMaxLength;
        }

        /// <summary>
        /// Changes the update info metadata validation rules.
        /// </summary>
        public static void ChangeUpdateInfoMetadata(
            bool lastChangedAtIsRequired,
            bool lastChangedByIsRequired,
            int lastChangedByMinLength,
            int lastChangedByMaxLength)
        {
            LastChangedAtIsRequired = lastChangedAtIsRequired;
            LastChangedByIsRequired = lastChangedByIsRequired;
            LastChangedByMinLength = lastChangedByMinLength;
            LastChangedByMaxLength = lastChangedByMaxLength;
        }

        /// <summary>
        /// Changes the EntityVersion metadata validation rules.
        /// </summary>
        public static void ChangeEntityVersionMetadata(bool isRequired)
        {
            EntityVersionIsRequired = isRequired;
        }

        /// <summary>
        /// Changes the correlation ID metadata validation rules.
        /// </summary>
        public static void ChangeCorrelationIdMetadata(
            bool createdCorrelationIdIsRequired,
            bool lastChangedCorrelationIdIsRequired)
        {
            CreatedCorrelationIdIsRequired = createdCorrelationIdIsRequired;
            LastChangedCorrelationIdIsRequired = lastChangedCorrelationIdIsRequired;
        }

        /// <summary>
        /// Changes the execution origin metadata validation rules.
        /// </summary>
        public static void ChangeExecutionOriginMetadata(
            bool createdExecutionOriginIsRequired,
            int createdExecutionOriginMinLength,
            int createdExecutionOriginMaxLength,
            bool lastChangedExecutionOriginIsRequired,
            int lastChangedExecutionOriginMinLength,
            int lastChangedExecutionOriginMaxLength)
        {
            CreatedExecutionOriginIsRequired = createdExecutionOriginIsRequired;
            CreatedExecutionOriginMinLength = createdExecutionOriginMinLength;
            CreatedExecutionOriginMaxLength = createdExecutionOriginMaxLength;
            LastChangedExecutionOriginIsRequired = lastChangedExecutionOriginIsRequired;
            LastChangedExecutionOriginMinLength = lastChangedExecutionOriginMinLength;
            LastChangedExecutionOriginMaxLength = lastChangedExecutionOriginMaxLength;
        }
    }

    /// <summary>
    /// Gets the entity metadata.
    /// </summary>
    public EntityInfo EntityInfo { get; private set; }

    /// <summary>
    /// Initializes a new instance of the EntityBase class.
    /// </summary>
    protected EntityBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the EntityBase class with entity info.
    /// </summary>
    protected EntityBase(EntityInfo entityInfo)
    {
        EntityInfo = entityInfo;
    }

    /// <summary>
    /// Validates the entity base metadata.
    /// </summary>
    protected static bool EntityBaseIsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo)
    {
        return ValidateEntityInfo(executionContext, entityInfo);
    }

    /// <summary>
    /// Validates the complete entity including base and derived class validations.
    /// </summary>
    public bool IsValid(ExecutionContext executionContext)
    {
        return EntityBaseIsValid(executionContext, EntityInfo)
            & IsValidInternal(executionContext);
    }

    /// <summary>
    /// When overridden in a derived class, validates entity-specific rules.
    /// </summary>
    protected abstract bool IsValidInternal(ExecutionContext executionContext);

    /// <summary>
    /// Validates all EntityInfo fields according to metadata rules.
    /// </summary>
    public static bool ValidateEntityInfo(
        ExecutionContext executionContext,
        EntityInfo entityInfo)
    {
        bool idIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: nameof(EntityInfo.Id)),
            isRequired: EntityBaseMetadata.IdIsRequired,
            entityInfo.Id);

        bool tenantCodeIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.TenantCodePropertyName),
            isRequired: EntityBaseMetadata.TenantCodeIsRequired,
            entityInfo.TenantInfo.Code);

        bool createdAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.CreatedAtPropertyName),
            isRequired: EntityBaseMetadata.CreatedAtIsRequired,
            entityInfo.EntityChangeInfo.CreatedAt);

        bool createdByIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.CreatedByPropertyName),
            isRequired: EntityBaseMetadata.CreatedByIsRequired,
            entityInfo.EntityChangeInfo.CreatedBy);

        // Stryker disable once Boolean : Equivalent mutant - value is always overwritten in the if block below when CreatedBy is not null
        bool createdByMinLengthValidation = true;
        // Stryker disable once Boolean : Equivalent mutant - value is always overwritten in the if block below when CreatedBy is not null
        bool createdByMaxLengthValidation = true;

        if (entityInfo.EntityChangeInfo.CreatedBy is not null)
        {
            createdByMinLengthValidation = ValidationUtils.ValidateMinLength(
                executionContext,
                propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.CreatedByPropertyName),
                minLength: EntityBaseMetadata.CreatedByMinLength,
                entityInfo.EntityChangeInfo.CreatedBy.Length);
            createdByMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.CreatedByPropertyName),
                maxLength: EntityBaseMetadata.CreatedByMaxLength,
                entityInfo.EntityChangeInfo.CreatedBy.Length);
        }

        bool lastChangedAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.LastChangedAtPropertyName),
            isRequired: EntityBaseMetadata.LastChangedAtIsRequired,
            entityInfo.EntityChangeInfo.LastChangedAt);

        bool lastChangedByIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.LastChangedByPropertyName),
            isRequired: EntityBaseMetadata.LastChangedByIsRequired,
            entityInfo.EntityChangeInfo.LastChangedBy);

        bool lastChangedByMinLengthValidation = true;
        bool lastChangedByMaxLengthValidation = true;

        if (entityInfo.EntityChangeInfo.LastChangedBy is not null)
        {
            lastChangedByMinLengthValidation = ValidationUtils.ValidateMinLength(
                executionContext,
                propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.LastChangedByPropertyName),
                minLength: EntityBaseMetadata.LastChangedByMinLength,
                entityInfo.EntityChangeInfo.LastChangedBy.Length);
            lastChangedByMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.LastChangedByPropertyName),
                maxLength: EntityBaseMetadata.LastChangedByMaxLength,
                entityInfo.EntityChangeInfo.LastChangedBy.Length);
        }

        bool entityVersionIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.EntityVersionPropertyName),
            isRequired: EntityBaseMetadata.EntityVersionIsRequired,
            entityInfo.EntityVersion);

        bool createdCorrelationIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.CreatedCorrelationIdPropertyName),
            isRequired: EntityBaseMetadata.CreatedCorrelationIdIsRequired,
            entityInfo.EntityChangeInfo.CreatedCorrelationId);

        bool lastChangedCorrelationIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.LastChangedCorrelationIdPropertyName),
            isRequired: EntityBaseMetadata.LastChangedCorrelationIdIsRequired,
            entityInfo.EntityChangeInfo.LastChangedCorrelationId);

        bool createdExecutionOriginIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.CreatedExecutionOriginPropertyName),
            isRequired: EntityBaseMetadata.CreatedExecutionOriginIsRequired,
            entityInfo.EntityChangeInfo.CreatedExecutionOrigin);

        // Stryker disable once Boolean : Equivalent mutant - value is always overwritten in the if block below when CreatedExecutionOrigin is not null
        bool createdExecutionOriginMinLengthValidation = true;
        // Stryker disable once Boolean : Equivalent mutant - value is always overwritten in the if block below when CreatedExecutionOrigin is not null
        bool createdExecutionOriginMaxLengthValidation = true;

        if (entityInfo.EntityChangeInfo.CreatedExecutionOrigin is not null)
        {
            createdExecutionOriginMinLengthValidation = ValidationUtils.ValidateMinLength(
                executionContext,
                propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.CreatedExecutionOriginPropertyName),
                minLength: EntityBaseMetadata.CreatedExecutionOriginMinLength,
                entityInfo.EntityChangeInfo.CreatedExecutionOrigin.Length);
            createdExecutionOriginMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.CreatedExecutionOriginPropertyName),
                maxLength: EntityBaseMetadata.CreatedExecutionOriginMaxLength,
                entityInfo.EntityChangeInfo.CreatedExecutionOrigin.Length);
        }

        bool lastChangedExecutionOriginIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.LastChangedExecutionOriginPropertyName),
            isRequired: EntityBaseMetadata.LastChangedExecutionOriginIsRequired,
            entityInfo.EntityChangeInfo.LastChangedExecutionOrigin);

        bool lastChangedExecutionOriginMinLengthValidation = true;
        bool lastChangedExecutionOriginMaxLengthValidation = true;

        if (entityInfo.EntityChangeInfo.LastChangedExecutionOrigin is not null)
        {
            lastChangedExecutionOriginMinLengthValidation = ValidationUtils.ValidateMinLength(
                executionContext,
                propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.LastChangedExecutionOriginPropertyName),
                minLength: EntityBaseMetadata.LastChangedExecutionOriginMinLength,
                entityInfo.EntityChangeInfo.LastChangedExecutionOrigin.Length);
            lastChangedExecutionOriginMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<EntityBase>(propertyName: EntityBaseMetadata.LastChangedExecutionOriginPropertyName),
                maxLength: EntityBaseMetadata.LastChangedExecutionOriginMaxLength,
                entityInfo.EntityChangeInfo.LastChangedExecutionOrigin.Length);
        }

        return
            idIsRequiredValidation &&
            tenantCodeIsRequiredValidation &&
            createdAtIsRequiredValidation &&
            createdByIsRequiredValidation &&
            createdByMinLengthValidation &&
            createdByMaxLengthValidation &&
            lastChangedAtIsRequiredValidation &&
            lastChangedByIsRequiredValidation &&
            lastChangedByMinLengthValidation &&
            lastChangedByMaxLengthValidation &&
            entityVersionIsRequiredValidation &&
            createdCorrelationIdIsRequiredValidation &&
            lastChangedCorrelationIdIsRequiredValidation &&
            createdExecutionOriginIsRequiredValidation &&
            createdExecutionOriginMinLengthValidation &&
            createdExecutionOriginMaxLengthValidation &&
            lastChangedExecutionOriginIsRequiredValidation &&
            lastChangedExecutionOriginMinLengthValidation &&
            lastChangedExecutionOriginMaxLengthValidation;
    }

    /// <summary>
    /// Validates that the entity's tenant matches the execution context tenant.
    /// </summary>
    protected static bool ValidateIfTenantCodeMatchesExecutionContext(
        ExecutionContext executionContext,
        TenantInfo tenantInfo)
    {
        if (tenantInfo.Code != executionContext.TenantInfo.Code)
        {
            executionContext.AddErrorMessage(
                code: CreateMessageCode<EntityBase>(propertyName: TenantMismatchMessageCode));

            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that all entities in a collection belong to the same tenant as the execution context.
    /// </summary>
    protected static bool ValidateTenantForCollection(
        ExecutionContext executionContext,
        IEnumerable<EntityBase> collection)
    {
        foreach (EntityBase item in collection)
        {
            if (item.EntityInfo.TenantInfo.Code != executionContext.TenantInfo.Code)
            {
                executionContext.AddErrorMessage(
                    code: CreateMessageCode<EntityBase>(propertyName: TenantMismatchMessageCode));

                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Sets the entity information with validation.
    /// </summary>
    protected internal bool SetEntityInfo(
        ExecutionContext executionContext,
        EntityInfo entityInfo)
    {
        bool isValid = ValidateEntityInfo(executionContext, entityInfo);

        if (!isValid)
            return false;

        EntityInfo = entityInfo;

        return true;
    }

    // Stryker disable once Block : Explicit interface implementation forwarding to protected internal method - already tested through SetEntityInfo
    bool IEntity.SetEntityInfo(ExecutionContext executionContext, EntityInfo entityInfo)
    {
        return SetEntityInfo(executionContext, entityInfo);
    }

    /// <summary>
    /// Creates a message code for validation errors specific to the derived entity type.
    /// </summary>
    protected abstract string CreateMessageCode(string messageSuffix);

    /// <summary>
    /// Creates a message code for validation errors for a specific entity type.
    /// </summary>
    protected static string CreateMessageCode<TEntityType>(string propertyName)
    {
        return $"{typeof(TEntityType)}.{propertyName}";
    }
}

/// <summary>
/// Generic abstract base class for domain entities with typed operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class EntityBase<TEntity> : EntityBase, IEntity<TEntity>
{
    private readonly Type _entityType = typeof(TEntity);

    /// <summary>
    /// Initializes a new instance of the EntityBase class.
    /// </summary>
    protected EntityBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the EntityBase class with entity info.
    /// </summary>
    protected EntityBase(EntityInfo entityInfo) : base(entityInfo)
    {
    }

    /// <summary>
    /// Creates a deep clone of this entity.
    /// </summary>
    public abstract IEntity<TEntity> Clone();

    /// <summary>
    /// Registers a new entity with the provided input and handler.
    /// </summary>
    /// <typeparam name="TEntityBase">The concrete entity type.</typeparam>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <param name="executionContext">The execution context.</param>
    /// <param name="input">The input data.</param>
    /// <param name="entityFactory">Factory function to create the entity.</param>
    /// <param name="handler">Handler to apply the input to the entity.</param>
    /// <returns>The created entity, or null if validation failed.</returns>
    protected static TEntityBase? RegisterNewInternal<TEntityBase, TInput>(
        ExecutionContext executionContext,
        TInput input,
        Func<ExecutionContext, TInput, TEntityBase> entityFactory,
        Func<ExecutionContext, TInput, TEntityBase, bool> handler)
        where TEntityBase : EntityBase<TEntity>
    {
        TEntityBase entity = entityFactory(executionContext, input);

        bool entityInfoResult = entity.SetEntityInfo(
            executionContext,
            entityInfo: EntityInfo.RegisterNew(
                executionContext,
                tenantInfo: executionContext.TenantInfo,
                createdBy: executionContext.ExecutionUser));

        if (!entityInfoResult)
            return null;

        bool isSuccess = handler(executionContext, input, entity);

        return isSuccess ? entity : null;
    }

    /// <summary>
    /// Registers a change to an existing entity.
    /// </summary>
    /// <typeparam name="TEntityBase">The concrete entity type.</typeparam>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <param name="executionContext">The execution context.</param>
    /// <param name="instance">The existing entity instance.</param>
    /// <param name="input">The input data.</param>
    /// <param name="handler">Handler to apply the changes to the entity.</param>
    /// <returns>The modified entity clone, or null if validation failed.</returns>
    /// <remarks>
    /// This method:
    /// 1. Validates the entity's tenant matches the execution context
    /// 2. Clones the entity to preserve the original
    /// 3. Updates the EntityInfo with change tracking
    /// 4. Applies the handler to the cloned entity
    /// </remarks>
    protected static TEntityBase? RegisterChangeInternal<TEntityBase, TInput>(
        ExecutionContext executionContext,
        EntityBase<TEntity> instance,
        TInput input,
        Func<ExecutionContext, TInput, TEntityBase, bool> handler)
        where TEntityBase : EntityBase<TEntity>
    {
        if (instance.EntityInfo.TenantInfo.Code != executionContext.TenantInfo.Code)
        {
            executionContext.AddErrorMessage(
                code: CreateMessageCode<TEntityBase>(propertyName: TenantMismatchMessageCode));

            return default;
        }

        var newInstance = (TEntityBase)instance.Clone();

        bool entityInfoResult = newInstance.SetEntityInfo(
            executionContext,
            entityInfo: newInstance.EntityInfo.RegisterChange(
                executionContext,
                changedBy: executionContext.ExecutionUser));

        if (!entityInfoResult)
            return default;

        bool isSuccess = handler(executionContext, input, newInstance);

        return isSuccess ? newInstance : default;
    }

    /// <inheritdoc/>
    protected override string CreateMessageCode(string messageSuffix)
    {
        return $"{_entityType}.{messageSuffix}";
    }
}
