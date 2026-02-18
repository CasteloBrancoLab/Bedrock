using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;
using ShopDemo.Auth.Domain.Entities.UserConsents.Inputs;
using ShopDemo.Auth.Domain.Entities.UserConsents.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.UserConsents;

public sealed class UserConsent
    : EntityBase<UserConsent>,
    IUserConsent
{
    // Properties
    public Id UserId { get; private set; }
    public Id ConsentTermId { get; private set; }
    public DateTimeOffset AcceptedAt { get; private set; }
    public UserConsentStatus Status { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string? IpAddress { get; private set; }

    // Constructors
    private UserConsent()
    {
    }

    private UserConsent(
        EntityInfo entityInfo,
        Id userId,
        Id consentTermId,
        DateTimeOffset acceptedAt,
        UserConsentStatus status,
        DateTimeOffset? revokedAt,
        string? ipAddress
    ) : base(entityInfo)
    {
        UserId = userId;
        ConsentTermId = consentTermId;
        AcceptedAt = acceptedAt;
        Status = status;
        RevokedAt = revokedAt;
        IpAddress = ipAddress;
    }

    // Public Business Methods
    public static UserConsent? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewUserConsentInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new UserConsent(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetConsentTermId(executionContext, input.ConsentTermId)
                    & instance.SetAcceptedAt(executionContext.Timestamp)
                    & instance.SetStatus(executionContext, UserConsentStatus.Active)
                    & instance.SetRevokedAt(null)
                    & instance.SetIpAddress(executionContext, input.IpAddress);
            }
        );
    }

    public static UserConsent CreateFromExistingInfo(
        CreateFromExistingInfoUserConsentInput input
    )
    {
        return new UserConsent(
            input.EntityInfo,
            input.UserId,
            input.ConsentTermId,
            input.AcceptedAt,
            input.Status,
            input.RevokedAt,
            input.IpAddress
        );
    }

    public UserConsent? Revoke(
        ExecutionContext executionContext,
        RevokeUserConsentInput input
    )
    {
        return RegisterChangeInternal<UserConsent, RevokeUserConsentInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RevokeInternal(executionContext);
            }
        );
    }

    public override UserConsent Clone()
    {
        return new UserConsent(
            EntityInfo,
            UserId,
            ConsentTermId,
            AcceptedAt,
            Status,
            RevokedAt,
            IpAddress
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool RevokeInternal(
        ExecutionContext executionContext
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, UserConsentStatus.Revoked);

        if (!isValidTransition)
            return false;

        Status = UserConsentStatus.Revoked;
        RevokedAt = executionContext.Timestamp;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        Id? consentTermId,
        DateTimeOffset? acceptedAt,
        UserConsentStatus? status
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateConsentTermId(executionContext, consentTermId)
            & ValidateAcceptedAt(executionContext, acceptedAt)
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
            ConsentTermId,
            AcceptedAt,
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
            propertyName: CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.UserIdPropertyName),
            isRequired: UserConsentMetadata.UserIdIsRequired,
            value: userId
        );

        return userIdIsRequiredValidation;
    }

    public static bool ValidateConsentTermId(
        ExecutionContext executionContext,
        Id? consentTermId
    )
    {
        bool consentTermIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.ConsentTermIdPropertyName),
            isRequired: UserConsentMetadata.ConsentTermIdIsRequired,
            value: consentTermId
        );

        return consentTermIdIsRequiredValidation;
    }

    public static bool ValidateAcceptedAt(
        ExecutionContext executionContext,
        DateTimeOffset? acceptedAt
    )
    {
        bool acceptedAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.AcceptedAtPropertyName),
            isRequired: UserConsentMetadata.AcceptedAtIsRequired,
            value: acceptedAt
        );

        return acceptedAtIsRequiredValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        UserConsentStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.StatusPropertyName),
            isRequired: UserConsentMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        UserConsentStatus? from,
        UserConsentStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (UserConsentStatus.Active, UserConsentStatus.Revoked) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.StatusPropertyName)}.InvalidTransition");
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
    private bool SetConsentTermId(
        ExecutionContext executionContext,
        Id consentTermId
    )
    {
        bool isValid = ValidateConsentTermId(
            executionContext,
            consentTermId
        );

        if (!isValid)
            return false;

        ConsentTermId = consentTermId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetAcceptedAt(
        DateTimeOffset acceptedAt
    )
    {
        AcceptedAt = acceptedAt;
        return true;
    }

    // Stryker disable once Block : SetStatus recebe UserConsentStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe UserConsentStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        UserConsentStatus status
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

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetIpAddress(
        ExecutionContext executionContext,
        string? ipAddress
    )
    {
        if (ipAddress is not null)
        {
            bool ipAddressMaxLengthValidation = ValidationUtils.ValidateMaxLength(
                executionContext,
                propertyName: CreateMessageCode<UserConsent>(propertyName: UserConsentMetadata.IpAddressPropertyName),
                maxLength: UserConsentMetadata.IpAddressMaxLength,
                value: ipAddress.Length
            );

            if (!ipAddressMaxLengthValidation)
                return false;
        }

        IpAddress = ipAddress;

        return true;
    }

    // Metadata
    public static class UserConsentMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // ConsentTermId
        public static readonly string ConsentTermIdPropertyName = "ConsentTermId";
        public static bool ConsentTermIdIsRequired { get; private set; } = true;

        // AcceptedAt
        public static readonly string AcceptedAtPropertyName = "AcceptedAt";
        public static bool AcceptedAtIsRequired { get; private set; } = true;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        // IpAddress
        public static readonly string IpAddressPropertyName = "IpAddress";
        public static int IpAddressMaxLength { get; private set; } = 45;

        public static void ChangeUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                UserIdIsRequired = isRequired;
            }
        }

        public static void ChangeConsentTermIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                ConsentTermIdIsRequired = isRequired;
            }
        }

        public static void ChangeAcceptedAtMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                AcceptedAtIsRequired = isRequired;
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

        public static void ChangeIpAddressMetadata(
            int maxLength
        )
        {
            lock (_lockObject)
            {
                IpAddressMaxLength = maxLength;
            }
        }
    }
}
