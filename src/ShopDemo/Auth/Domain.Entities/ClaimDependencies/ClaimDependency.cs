using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies.Inputs;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.ClaimDependencies;

public sealed class ClaimDependency
    : EntityBase<ClaimDependency>,
    IClaimDependency
{
    // Properties
    public Id ClaimId { get; private set; }
    public Id DependsOnClaimId { get; private set; }

    // Constructors
    private ClaimDependency()
    {
    }

    private ClaimDependency(
        EntityInfo entityInfo,
        Id claimId,
        Id dependsOnClaimId
    ) : base(entityInfo)
    {
        ClaimId = claimId;
        DependsOnClaimId = dependsOnClaimId;
    }

    // Public Business Methods
    public static ClaimDependency? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewClaimDependencyInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new ClaimDependency(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetClaimId(executionContext, input.ClaimId)
                    & instance.SetDependsOnClaimId(executionContext, input.DependsOnClaimId);
            }
        );
    }

    public static ClaimDependency CreateFromExistingInfo(
        CreateFromExistingInfoClaimDependencyInput input
    )
    {
        return new ClaimDependency(
            input.EntityInfo,
            input.ClaimId,
            input.DependsOnClaimId
        );
    }

    public override ClaimDependency Clone()
    {
        return new ClaimDependency(
            EntityInfo,
            ClaimId,
            DependsOnClaimId
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? claimId,
        Id? dependsOnClaimId
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateClaimId(executionContext, claimId)
            & ValidateDependsOnClaimId(executionContext, dependsOnClaimId);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            ClaimId,
            DependsOnClaimId
        );
    }

    public static bool ValidateClaimId(
        ExecutionContext executionContext,
        Id? claimId
    )
    {
        bool claimIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ClaimDependency>(propertyName: ClaimDependencyMetadata.ClaimIdPropertyName),
            isRequired: ClaimDependencyMetadata.ClaimIdIsRequired,
            value: claimId
        );

        if (!claimIdIsRequiredValidation)
            return false;

        if (claimId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ClaimDependency>(propertyName: ClaimDependencyMetadata.ClaimIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateDependsOnClaimId(
        ExecutionContext executionContext,
        Id? dependsOnClaimId
    )
    {
        bool dependsOnClaimIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ClaimDependency>(propertyName: ClaimDependencyMetadata.DependsOnClaimIdPropertyName),
            isRequired: ClaimDependencyMetadata.DependsOnClaimIdIsRequired,
            value: dependsOnClaimId
        );

        if (!dependsOnClaimIdIsRequiredValidation)
            return false;

        if (dependsOnClaimId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ClaimDependency>(propertyName: ClaimDependencyMetadata.DependsOnClaimIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetClaimId(
        ExecutionContext executionContext,
        Id claimId
    )
    {
        bool isValid = ValidateClaimId(
            executionContext,
            claimId
        );

        if (!isValid)
            return false;

        ClaimId = claimId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetDependsOnClaimId(
        ExecutionContext executionContext,
        Id dependsOnClaimId
    )
    {
        bool isValid = ValidateDependsOnClaimId(
            executionContext,
            dependsOnClaimId
        );

        if (!isValid)
            return false;

        DependsOnClaimId = dependsOnClaimId;

        return true;
    }
    // Stryker restore all

    // Metadata
    public static class ClaimDependencyMetadata
    {
        private static readonly Lock _lockObject = new();

        // ClaimId
        public static readonly string ClaimIdPropertyName = "ClaimId";
        public static bool ClaimIdIsRequired { get; private set; } = true;

        // DependsOnClaimId
        public static readonly string DependsOnClaimIdPropertyName = "DependsOnClaimId";
        public static bool DependsOnClaimIdIsRequired { get; private set; } = true;

        public static void ChangeClaimIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ClaimIdIsRequired = isRequired;
            }
        }

        public static void ChangeDependsOnClaimIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                DependsOnClaimIdIsRequired = isRequired;
            }
        }
    }
}
