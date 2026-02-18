using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes.Inputs;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.ServiceClientScopes;

public sealed class ServiceClientScope
    : EntityBase<ServiceClientScope>,
    IServiceClientScope
{
    // Properties
    public Id ServiceClientId { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Scope { get; private set; } = string.Empty;

    // Constructors
    private ServiceClientScope()
    {
    }

    private ServiceClientScope(
        EntityInfo entityInfo,
        Id serviceClientId,
        string scope
    ) : base(entityInfo)
    {
        ServiceClientId = serviceClientId;
        Scope = scope;
    }

    // Public Business Methods
    public static ServiceClientScope? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewServiceClientScopeInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new ServiceClientScope(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetServiceClientId(executionContext, input.ServiceClientId)
                    & instance.SetScope(executionContext, input.Scope);
            }
        );
    }

    public static ServiceClientScope CreateFromExistingInfo(
        CreateFromExistingInfoServiceClientScopeInput input
    )
    {
        return new ServiceClientScope(
            input.EntityInfo,
            input.ServiceClientId,
            input.Scope
        );
    }

    public override ServiceClientScope Clone()
    {
        return new ServiceClientScope(
            EntityInfo,
            ServiceClientId,
            Scope
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? serviceClientId,
        string? scope
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateServiceClientId(executionContext, serviceClientId)
            & ValidateScope(executionContext, scope);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            ServiceClientId,
            Scope
        );
    }

    public static bool ValidateServiceClientId(
        ExecutionContext executionContext,
        Id? serviceClientId
    )
    {
        bool serviceClientIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClientScope>(propertyName: ServiceClientScopeMetadata.ServiceClientIdPropertyName),
            isRequired: ServiceClientScopeMetadata.ServiceClientIdIsRequired,
            value: serviceClientId
        );

        return serviceClientIdIsRequiredValidation;
    }

    public static bool ValidateScope(
        ExecutionContext executionContext,
        string? scope
    )
    {
        bool scopeIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClientScope>(propertyName: ServiceClientScopeMetadata.ScopePropertyName),
            isRequired: ServiceClientScopeMetadata.ScopeIsRequired,
            value: scope
        );

        if (!scopeIsRequiredValidation)
            return false;

        bool scopeMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ServiceClientScope>(propertyName: ServiceClientScopeMetadata.ScopePropertyName),
            maxLength: ServiceClientScopeMetadata.ScopeMaxLength,
            value: scope!.Length
        );

        return scopeMaxLengthValidation;
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
    private bool SetScope(
        ExecutionContext executionContext,
        string scope
    )
    {
        bool isValid = ValidateScope(
            executionContext,
            scope
        );

        if (!isValid)
            return false;

        Scope = scope;

        return true;
    }
    // Stryker restore all

    // Metadata
    public static class ServiceClientScopeMetadata
    {
        private static readonly Lock _lockObject = new();

        // ServiceClientId
        public static readonly string ServiceClientIdPropertyName = "ServiceClientId";
        public static bool ServiceClientIdIsRequired { get; private set; } = true;

        // Scope
        public static readonly string ScopePropertyName = "Scope";
        public static bool ScopeIsRequired { get; private set; } = true;
        public static int ScopeMaxLength { get; private set; } = 255;

        public static void ChangeServiceClientIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ServiceClientIdIsRequired = isRequired;
            }
        }

        public static void ChangeScopeMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ScopeIsRequired = isRequired;
                ScopeMaxLength = maxLength;
            }
        }
    }
}
