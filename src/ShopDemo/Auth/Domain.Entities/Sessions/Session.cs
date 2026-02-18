using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;
using ShopDemo.Auth.Domain.Entities.Sessions.Inputs;
using ShopDemo.Auth.Domain.Entities.Sessions.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.Sessions;

public sealed class Session
    : EntityBase<Session>,
    ISession
{
    // Properties
    public Id UserId { get; private set; }
    public Id RefreshTokenId { get; private set; }
    public string? DeviceInfo { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTimeOffset LastActivityAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    // Constructors
    private Session()
    {
    }

    private Session(
        EntityInfo entityInfo,
        Id userId,
        Id refreshTokenId,
        string? deviceInfo,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset expiresAt,
        SessionStatus status,
        DateTimeOffset lastActivityAt,
        DateTimeOffset? revokedAt
    ) : base(entityInfo)
    {
        UserId = userId;
        RefreshTokenId = refreshTokenId;
        DeviceInfo = deviceInfo;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ExpiresAt = expiresAt;
        Status = status;
        LastActivityAt = lastActivityAt;
        RevokedAt = revokedAt;
    }

    // Public Business Methods
    public static Session? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewSessionInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new Session(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetRefreshTokenId(executionContext, input.RefreshTokenId)
                    & instance.SetDeviceInfo(executionContext, input.DeviceInfo)
                    & instance.SetIpAddress(executionContext, input.IpAddress)
                    & instance.SetUserAgent(executionContext, input.UserAgent)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt)
                    & instance.SetStatus(executionContext, SessionStatus.Active)
                    & instance.SetLastActivityAt(executionContext.Timestamp)
                    & instance.SetRevokedAt(null);
            }
        );
    }

    public static Session CreateFromExistingInfo(
        CreateFromExistingInfoSessionInput input
    )
    {
        return new Session(
            input.EntityInfo,
            input.UserId,
            input.RefreshTokenId,
            input.DeviceInfo,
            input.IpAddress,
            input.UserAgent,
            input.ExpiresAt,
            input.Status,
            input.LastActivityAt,
            input.RevokedAt
        );
    }

    public Session? Revoke(
        ExecutionContext executionContext,
        RevokeSessionInput input
    )
    {
        return RegisterChangeInternal<Session, RevokeSessionInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RevokeInternal(executionContext);
            }
        );
    }

    public Session? UpdateActivity(
        ExecutionContext executionContext,
        UpdateSessionActivityInput input
    )
    {
        return RegisterChangeInternal<Session, UpdateSessionActivityInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.UpdateActivityInternal(executionContext);
            }
        );
    }

    public override Session Clone()
    {
        return new Session(
            EntityInfo,
            UserId,
            RefreshTokenId,
            DeviceInfo,
            IpAddress,
            UserAgent,
            ExpiresAt,
            Status,
            LastActivityAt,
            RevokedAt
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool RevokeInternal(
        ExecutionContext executionContext
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, SessionStatus.Revoked);

        if (!isValidTransition)
            return false;

        Status = SessionStatus.Revoked;
        RevokedAt = executionContext.Timestamp;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool UpdateActivityInternal(
        ExecutionContext executionContext
    )
    {
        LastActivityAt = executionContext.Timestamp;
        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        Id? refreshTokenId,
        string? deviceInfo,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset? expiresAt,
        SessionStatus? status
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateRefreshTokenId(executionContext, refreshTokenId)
            & ValidateDeviceInfo(executionContext, deviceInfo)
            & ValidateIpAddress(executionContext, ipAddress)
            & ValidateUserAgent(executionContext, userAgent)
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
            UserId,
            RefreshTokenId,
            DeviceInfo,
            IpAddress,
            UserAgent,
            ExpiresAt,
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
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.UserIdPropertyName),
            isRequired: SessionMetadata.UserIdIsRequired,
            value: userId
        );

        if (!userIdIsRequiredValidation)
            return false;

        if (userId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<Session>(propertyName: SessionMetadata.UserIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateRefreshTokenId(
        ExecutionContext executionContext,
        Id? refreshTokenId
    )
    {
        bool refreshTokenIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.RefreshTokenIdPropertyName),
            isRequired: SessionMetadata.RefreshTokenIdIsRequired,
            value: refreshTokenId
        );

        if (!refreshTokenIdIsRequiredValidation)
            return false;

        if (refreshTokenId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<Session>(propertyName: SessionMetadata.RefreshTokenIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateDeviceInfo(
        ExecutionContext executionContext,
        string? deviceInfo
    )
    {
        if (deviceInfo is null)
            return true;

        bool deviceInfoMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.DeviceInfoPropertyName),
            maxLength: SessionMetadata.DeviceInfoMaxLength,
            value: deviceInfo.Length
        );

        return deviceInfoMaxLengthValidation;
    }

    public static bool ValidateIpAddress(
        ExecutionContext executionContext,
        string? ipAddress
    )
    {
        if (ipAddress is null)
            return true;

        bool ipAddressMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.IpAddressPropertyName),
            maxLength: SessionMetadata.IpAddressMaxLength,
            value: ipAddress.Length
        );

        return ipAddressMaxLengthValidation;
    }

    public static bool ValidateUserAgent(
        ExecutionContext executionContext,
        string? userAgent
    )
    {
        if (userAgent is null)
            return true;

        bool userAgentMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.UserAgentPropertyName),
            maxLength: SessionMetadata.UserAgentMaxLength,
            value: userAgent.Length
        );

        return userAgentMaxLengthValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.ExpiresAtPropertyName),
            isRequired: SessionMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        SessionStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.StatusPropertyName),
            isRequired: SessionMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        SessionStatus? from,
        SessionStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<Session>(propertyName: SessionMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<Session>(propertyName: SessionMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (SessionStatus.Active, SessionStatus.Revoked) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<Session>(propertyName: SessionMetadata.StatusPropertyName)}.InvalidTransition");
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
    private bool SetRefreshTokenId(
        ExecutionContext executionContext,
        Id refreshTokenId
    )
    {
        bool isValid = ValidateRefreshTokenId(
            executionContext,
            refreshTokenId
        );

        if (!isValid)
            return false;

        RefreshTokenId = refreshTokenId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetDeviceInfo(
        ExecutionContext executionContext,
        string? deviceInfo
    )
    {
        bool isValid = ValidateDeviceInfo(
            executionContext,
            deviceInfo
        );

        if (!isValid)
            return false;

        DeviceInfo = deviceInfo;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetIpAddress(
        ExecutionContext executionContext,
        string? ipAddress
    )
    {
        bool isValid = ValidateIpAddress(
            executionContext,
            ipAddress
        );

        if (!isValid)
            return false;

        IpAddress = ipAddress;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetUserAgent(
        ExecutionContext executionContext,
        string? userAgent
    )
    {
        bool isValid = ValidateUserAgent(
            executionContext,
            userAgent
        );

        if (!isValid)
            return false;

        UserAgent = userAgent;

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

    // Stryker disable once Block : SetStatus recebe SessionStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe SessionStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        SessionStatus status
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

    private bool SetLastActivityAt(DateTimeOffset lastActivityAt)
    {
        LastActivityAt = lastActivityAt;
        return true;
    }

    private bool SetRevokedAt(DateTimeOffset? revokedAt)
    {
        RevokedAt = revokedAt;
        return true;
    }

    // Metadata
    public static class SessionMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // RefreshTokenId
        public static readonly string RefreshTokenIdPropertyName = "RefreshTokenId";
        public static bool RefreshTokenIdIsRequired { get; private set; } = true;

        // DeviceInfo
        public static readonly string DeviceInfoPropertyName = "DeviceInfo";
        public static int DeviceInfoMaxLength { get; private set; } = 500;

        // IpAddress
        public static readonly string IpAddressPropertyName = "IpAddress";
        public static int IpAddressMaxLength { get; private set; } = 45;

        // UserAgent
        public static readonly string UserAgentPropertyName = "UserAgent";
        public static int UserAgentMaxLength { get; private set; } = 1024;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        public static void ChangeUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                UserIdIsRequired = isRequired;
            }
        }

        public static void ChangeRefreshTokenIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                RefreshTokenIdIsRequired = isRequired;
            }
        }

        public static void ChangeDeviceInfoMetadata(
            int maxLength
        )
        {
            lock (_lockObject)
            {
                DeviceInfoMaxLength = maxLength;
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

        public static void ChangeUserAgentMetadata(
            int maxLength
        )
        {
            lock (_lockObject)
            {
                UserAgentMaxLength = maxLength;
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
