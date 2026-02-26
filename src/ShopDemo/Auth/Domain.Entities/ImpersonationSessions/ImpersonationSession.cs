using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Enums;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Inputs;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.ImpersonationSessions;

public sealed class ImpersonationSession
    : EntityBase<ImpersonationSession>,
    IImpersonationSession
{
    // Properties
    public Id OperatorUserId { get; private set; }
    public Id TargetUserId { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public ImpersonationSessionStatus Status { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }

    // Constructors
    private ImpersonationSession()
    {
    }

    private ImpersonationSession(
        EntityInfo entityInfo,
        Id operatorUserId,
        Id targetUserId,
        DateTimeOffset expiresAt,
        ImpersonationSessionStatus status,
        DateTimeOffset? endedAt
    ) : base(entityInfo)
    {
        OperatorUserId = operatorUserId;
        TargetUserId = targetUserId;
        ExpiresAt = expiresAt;
        Status = status;
        EndedAt = endedAt;
    }

    // Public Business Methods
    public static ImpersonationSession? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewImpersonationSessionInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new ImpersonationSession(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetOperatorUserId(executionContext, input.OperatorUserId)
                    & instance.SetTargetUserId(executionContext, input.TargetUserId)
                    & ValidateOperatorNotTarget(executionContext, input.OperatorUserId, input.TargetUserId)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt)
                    & instance.SetStatus(executionContext, ImpersonationSessionStatus.Active)
                    & instance.SetEndedAt(null);
            }
        );
    }

    public static ImpersonationSession CreateFromExistingInfo(
        CreateFromExistingInfoImpersonationSessionInput input
    )
    {
        return new ImpersonationSession(
            input.EntityInfo,
            input.OperatorUserId,
            input.TargetUserId,
            input.ExpiresAt,
            input.Status,
            input.EndedAt
        );
    }

    public ImpersonationSession? End(
        ExecutionContext executionContext,
        EndImpersonationSessionInput input
    )
    {
        return RegisterChangeInternal<ImpersonationSession, EndImpersonationSessionInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.EndInternal(executionContext);
            }
        );
    }

    public override ImpersonationSession Clone()
    {
        return new ImpersonationSession(
            EntityInfo,
            OperatorUserId,
            TargetUserId,
            ExpiresAt,
            Status,
            EndedAt
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool EndInternal(
        ExecutionContext executionContext
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, ImpersonationSessionStatus.Ended);

        if (!isValidTransition)
            return false;

        Status = ImpersonationSessionStatus.Ended;
        EndedAt = executionContext.Timestamp;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? operatorUserId,
        Id? targetUserId,
        DateTimeOffset? expiresAt,
        ImpersonationSessionStatus? status
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateOperatorUserId(executionContext, operatorUserId)
            & ValidateTargetUserId(executionContext, targetUserId)
            & ValidateOperatorNotTarget(executionContext, operatorUserId, targetUserId)
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
            OperatorUserId,
            TargetUserId,
            ExpiresAt,
            Status
        );
    }

    public static bool ValidateOperatorUserId(
        ExecutionContext executionContext,
        Id? operatorUserId
    )
    {
        bool operatorUserIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.OperatorUserIdPropertyName),
            isRequired: ImpersonationSessionMetadata.OperatorUserIdIsRequired,
            value: operatorUserId
        );

        if (!operatorUserIdIsRequiredValidation)
            return false;

        if (operatorUserId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.OperatorUserIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateTargetUserId(
        ExecutionContext executionContext,
        Id? targetUserId
    )
    {
        bool targetUserIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.TargetUserIdPropertyName),
            isRequired: ImpersonationSessionMetadata.TargetUserIdIsRequired,
            value: targetUserId
        );

        if (!targetUserIdIsRequiredValidation)
            return false;

        if (targetUserId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.TargetUserIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.ExpiresAtPropertyName),
            isRequired: ImpersonationSessionMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        ImpersonationSessionStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.StatusPropertyName),
            isRequired: ImpersonationSessionMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateOperatorNotTarget(
        ExecutionContext executionContext,
        Id? operatorUserId,
        Id? targetUserId
    )
    {
        bool operatorIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.OperatorUserIdPropertyName),
            isRequired: true,
            value: operatorUserId
        );

        bool targetIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.TargetUserIdPropertyName),
            isRequired: true,
            value: targetUserId
        );

        if (!operatorIsRequiredValidation || !targetIsRequiredValidation)
            return false;

        if (operatorUserId == targetUserId)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.TargetUserIdPropertyName)}.SelfImpersonation");
            return false;
        }

        return true;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        ImpersonationSessionStatus? from,
        ImpersonationSessionStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (ImpersonationSessionStatus.Active, ImpersonationSessionStatus.Ended) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ImpersonationSession>(propertyName: ImpersonationSessionMetadata.StatusPropertyName)}.InvalidTransition");
        }

        return isValid;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetOperatorUserId(
        ExecutionContext executionContext,
        Id operatorUserId
    )
    {
        bool isValid = ValidateOperatorUserId(
            executionContext,
            operatorUserId
        );

        if (!isValid)
            return false;

        OperatorUserId = operatorUserId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetTargetUserId(
        ExecutionContext executionContext,
        Id targetUserId
    )
    {
        bool isValid = ValidateTargetUserId(
            executionContext,
            targetUserId
        );

        if (!isValid)
            return false;

        TargetUserId = targetUserId;

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

    // Stryker disable once Block : SetStatus recebe ImpersonationSessionStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe ImpersonationSessionStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        ImpersonationSessionStatus status
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

    private bool SetEndedAt(DateTimeOffset? endedAt)
    {
        EndedAt = endedAt;
        return true;
    }

    // Metadata
    public static class ImpersonationSessionMetadata
    {
        private static readonly Lock _lockObject = new();

        // OperatorUserId
        public static readonly string OperatorUserIdPropertyName = "OperatorUserId";
        public static bool OperatorUserIdIsRequired { get; private set; } = true;

        // TargetUserId
        public static readonly string TargetUserIdPropertyName = "TargetUserId";
        public static bool TargetUserIdIsRequired { get; private set; } = true;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        public static void ChangeOperatorUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                OperatorUserIdIsRequired = isRequired;
            }
        }

        public static void ChangeTargetUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                TargetUserIdIsRequired = isRequired;
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
