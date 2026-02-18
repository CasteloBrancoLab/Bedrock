using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.ServiceClients;

public sealed class ServiceClient
    : EntityBase<ServiceClient>,
    IServiceClient
{
    // Properties
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string ClientId { get; private set; } = string.Empty;
    // Stryker disable once all : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public byte[] ClientSecretHash { get; private set; } = [];
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Name { get; private set; } = string.Empty;
    public ServiceClientStatus Status { get; private set; }
    public Id CreatedByUserId { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    // Constructors
    private ServiceClient()
    {
    }

    private ServiceClient(
        EntityInfo entityInfo,
        string clientId,
        byte[] clientSecretHash,
        string name,
        ServiceClientStatus status,
        Id createdByUserId,
        DateTimeOffset? expiresAt,
        DateTimeOffset? revokedAt
    ) : base(entityInfo)
    {
        ClientId = clientId;
        ClientSecretHash = clientSecretHash;
        Name = name;
        Status = status;
        CreatedByUserId = createdByUserId;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
    }

    // Public Business Methods
    public static ServiceClient? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewServiceClientInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new ServiceClient(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetClientId(executionContext, input.ClientId)
                    & instance.SetClientSecretHash(executionContext, input.ClientSecretHash)
                    & instance.SetName(executionContext, input.Name)
                    & instance.SetStatus(executionContext, ServiceClientStatus.Active)
                    & instance.SetCreatedByUserId(executionContext, input.CreatedByUserId)
                    & instance.SetExpiresAt(input.ExpiresAt)
                    & instance.SetRevokedAt(null);
            }
        );
    }

    public static ServiceClient CreateFromExistingInfo(
        CreateFromExistingInfoServiceClientInput input
    )
    {
        return new ServiceClient(
            input.EntityInfo,
            input.ClientId,
            input.ClientSecretHash,
            input.Name,
            input.Status,
            input.CreatedByUserId,
            input.ExpiresAt,
            input.RevokedAt
        );
    }

    public ServiceClient? Revoke(
        ExecutionContext executionContext,
        RevokeServiceClientInput input
    )
    {
        return RegisterChangeInternal<ServiceClient, RevokeServiceClientInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RevokeInternal(executionContext);
            }
        );
    }

    public override ServiceClient Clone()
    {
        return new ServiceClient(
            EntityInfo,
            ClientId,
            ClientSecretHash,
            Name,
            Status,
            CreatedByUserId,
            ExpiresAt,
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
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, ServiceClientStatus.Revoked);

        if (!isValidTransition)
            return false;

        Status = ServiceClientStatus.Revoked;
        RevokedAt = executionContext.Timestamp;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? clientId,
        byte[]? clientSecretHash,
        string? name,
        ServiceClientStatus? status,
        Id? createdByUserId
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateClientId(executionContext, clientId)
            & ValidateClientSecretHash(executionContext, clientSecretHash)
            & ValidateName(executionContext, name)
            & ValidateStatus(executionContext, status)
            & ValidateCreatedByUserId(executionContext, createdByUserId);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            ClientId,
            ClientSecretHash,
            Name,
            Status,
            CreatedByUserId
        );
    }

    public static bool ValidateClientId(
        ExecutionContext executionContext,
        string? clientId
    )
    {
        bool clientIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.ClientIdPropertyName),
            isRequired: ServiceClientMetadata.ClientIdIsRequired,
            value: clientId
        );

        if (!clientIdIsRequiredValidation)
            return false;

        bool clientIdMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.ClientIdPropertyName),
            minLength: 1,
            value: clientId!.Length
        );

        if (!clientIdMinLengthValidation)
            return false;

        bool clientIdMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.ClientIdPropertyName),
            maxLength: ServiceClientMetadata.ClientIdMaxLength,
            value: clientId!.Length
        );

        return clientIdMaxLengthValidation;
    }

    public static bool ValidateClientSecretHash(
        ExecutionContext executionContext,
        byte[]? clientSecretHash
    )
    {
        bool clientSecretHashIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.ClientSecretHashPropertyName),
            isRequired: ServiceClientMetadata.ClientSecretHashIsRequired,
            value: clientSecretHash
        );

        if (!clientSecretHashIsRequiredValidation)
            return false;

        bool clientSecretHashMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.ClientSecretHashPropertyName),
            minLength: 1,
            value: clientSecretHash!.Length
        );

        if (!clientSecretHashMinLengthValidation)
            return false;

        bool clientSecretHashMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.ClientSecretHashPropertyName),
            maxLength: ServiceClientMetadata.ClientSecretHashMaxLength,
            value: clientSecretHash!.Length
        );

        return clientSecretHashMaxLengthValidation;
    }

    public static bool ValidateName(
        ExecutionContext executionContext,
        string? name
    )
    {
        bool nameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.NamePropertyName),
            isRequired: ServiceClientMetadata.NameIsRequired,
            value: name
        );

        if (!nameIsRequiredValidation)
            return false;

        bool nameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.NamePropertyName),
            minLength: 1,
            value: name!.Length
        );

        if (!nameMinLengthValidation)
            return false;

        bool nameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.NamePropertyName),
            maxLength: ServiceClientMetadata.NameMaxLength,
            value: name!.Length
        );

        return nameMaxLengthValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        ServiceClientStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.StatusPropertyName),
            isRequired: ServiceClientMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateCreatedByUserId(
        ExecutionContext executionContext,
        Id? createdByUserId
    )
    {
        bool createdByUserIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.CreatedByUserIdPropertyName),
            isRequired: ServiceClientMetadata.CreatedByUserIdIsRequired,
            value: createdByUserId
        );

        if (!createdByUserIdIsRequiredValidation)
            return false;

        if (createdByUserId!.Value.Value == Guid.Empty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.CreatedByUserIdPropertyName)}.IsRequired");
            return false;
        }

        return true;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        ServiceClientStatus? from,
        ServiceClientStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (ServiceClientStatus.Active, ServiceClientStatus.Revoked) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<ServiceClient>(propertyName: ServiceClientMetadata.StatusPropertyName)}.InvalidTransition");
        }

        return isValid;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetClientId(
        ExecutionContext executionContext,
        string clientId
    )
    {
        bool isValid = ValidateClientId(
            executionContext,
            clientId
        );

        if (!isValid)
            return false;

        ClientId = clientId;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetClientSecretHash(
        ExecutionContext executionContext,
        byte[] clientSecretHash
    )
    {
        bool isValid = ValidateClientSecretHash(
            executionContext,
            clientSecretHash
        );

        if (!isValid)
            return false;

        ClientSecretHash = clientSecretHash;

        return true;
    }

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

    // Stryker disable once Block : SetStatus recebe ServiceClientStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe ServiceClientStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        ServiceClientStatus status
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
    private bool SetCreatedByUserId(
        ExecutionContext executionContext,
        Id createdByUserId
    )
    {
        bool isValid = ValidateCreatedByUserId(
            executionContext,
            createdByUserId
        );

        if (!isValid)
            return false;

        CreatedByUserId = createdByUserId;

        return true;
    }
    // Stryker restore all

    private bool SetExpiresAt(DateTimeOffset? expiresAt)
    {
        ExpiresAt = expiresAt;
        return true;
    }

    private bool SetRevokedAt(DateTimeOffset? revokedAt)
    {
        RevokedAt = revokedAt;
        return true;
    }

    // Metadata
    public static class ServiceClientMetadata
    {
        private static readonly Lock _lockObject = new();

        // ClientId
        public static readonly string ClientIdPropertyName = "ClientId";
        public static bool ClientIdIsRequired { get; private set; } = true;
        public static int ClientIdMaxLength { get; private set; } = 255;

        // ClientSecretHash
        public static readonly string ClientSecretHashPropertyName = "ClientSecretHash";
        public static bool ClientSecretHashIsRequired { get; private set; } = true;
        public static int ClientSecretHashMaxLength { get; private set; } = 1024;

        // Name
        public static readonly string NamePropertyName = "Name";
        public static bool NameIsRequired { get; private set; } = true;
        public static int NameMaxLength { get; private set; } = 255;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        // CreatedByUserId
        public static readonly string CreatedByUserIdPropertyName = "CreatedByUserId";
        public static bool CreatedByUserIdIsRequired { get; private set; } = true;

        public static void ChangeClientIdMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ClientIdIsRequired = isRequired;
                ClientIdMaxLength = maxLength;
            }
        }

        public static void ChangeClientSecretHashMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ClientSecretHashIsRequired = isRequired;
                ClientSecretHashMaxLength = maxLength;
            }
        }

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

        public static void ChangeStatusMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                StatusIsRequired = isRequired;
            }
        }

        public static void ChangeCreatedByUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                CreatedByUserIdIsRequired = isRequired;
            }
        }
    }
}
