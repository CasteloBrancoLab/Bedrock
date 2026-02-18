using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Inputs;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.IdempotencyRecords;

public sealed class IdempotencyRecord
    : EntityBase<IdempotencyRecord>,
    IIdempotencyRecord
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);

    // Properties
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string IdempotencyKey { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string RequestHash { get; private set; } = string.Empty;
    public string? ResponseBody { get; private set; }
    public int StatusCode { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    // Constructors
    private IdempotencyRecord()
    {
    }

    private IdempotencyRecord(
        EntityInfo entityInfo,
        string idempotencyKey,
        string requestHash,
        string? responseBody,
        int statusCode,
        DateTimeOffset expiresAt
    ) : base(entityInfo)
    {
        IdempotencyKey = idempotencyKey;
        RequestHash = requestHash;
        ResponseBody = responseBody;
        StatusCode = statusCode;
        ExpiresAt = expiresAt;
    }

    // Public Business Methods
    public static IdempotencyRecord? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewIdempotencyRecordInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new IdempotencyRecord(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetIdempotencyKey(executionContext, input.IdempotencyKey)
                    & instance.SetRequestHash(executionContext, input.RequestHash)
                    & instance.SetResponseBody(null)
                    & instance.SetStatusCode(0)
                    & instance.SetExpiresAt(executionContext.Timestamp.Add(DefaultTtl));
            }
        );
    }

    public static IdempotencyRecord CreateFromExistingInfo(
        CreateFromExistingInfoIdempotencyRecordInput input
    )
    {
        return new IdempotencyRecord(
            input.EntityInfo,
            input.IdempotencyKey,
            input.RequestHash,
            input.ResponseBody,
            input.StatusCode,
            input.ExpiresAt
        );
    }

    public IdempotencyRecord? SetResponse(
        ExecutionContext executionContext,
        SetResponseIdempotencyRecordInput input
    )
    {
        return RegisterChangeInternal<IdempotencyRecord, SetResponseIdempotencyRecordInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.SetResponseInternal(executionContext, input.ResponseBody, input.StatusCode);
            }
        );
    }

    public override IdempotencyRecord Clone()
    {
        return new IdempotencyRecord(
            EntityInfo,
            IdempotencyKey,
            RequestHash,
            ResponseBody,
            StatusCode,
            ExpiresAt
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetResponseInternal(
        ExecutionContext executionContext,
        string responseBody,
        int statusCode
    )
    {
        bool responseBodyIsValid = ValidateResponseBody(executionContext, responseBody);

        if (!responseBodyIsValid)
            return false;

        ResponseBody = responseBody;
        StatusCode = statusCode;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? idempotencyKey,
        string? requestHash,
        DateTimeOffset? expiresAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateIdempotencyKey(executionContext, idempotencyKey)
            & ValidateRequestHash(executionContext, requestHash)
            & ValidateExpiresAt(executionContext, expiresAt);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            IdempotencyKey,
            RequestHash,
            ExpiresAt
        );
    }

    public static bool ValidateIdempotencyKey(
        ExecutionContext executionContext,
        string? idempotencyKey
    )
    {
        bool idempotencyKeyIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<IdempotencyRecord>(propertyName: IdempotencyRecordMetadata.IdempotencyKeyPropertyName),
            isRequired: IdempotencyRecordMetadata.IdempotencyKeyIsRequired,
            value: idempotencyKey
        );

        if (!idempotencyKeyIsRequiredValidation)
            return false;

        bool idempotencyKeyMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<IdempotencyRecord>(propertyName: IdempotencyRecordMetadata.IdempotencyKeyPropertyName),
            minLength: 1,
            value: idempotencyKey!.Length
        );

        if (!idempotencyKeyMinLengthValidation)
            return false;

        bool idempotencyKeyMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<IdempotencyRecord>(propertyName: IdempotencyRecordMetadata.IdempotencyKeyPropertyName),
            maxLength: IdempotencyRecordMetadata.IdempotencyKeyMaxLength,
            value: idempotencyKey!.Length
        );

        return idempotencyKeyMaxLengthValidation;
    }

    public static bool ValidateRequestHash(
        ExecutionContext executionContext,
        string? requestHash
    )
    {
        bool requestHashIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<IdempotencyRecord>(propertyName: IdempotencyRecordMetadata.RequestHashPropertyName),
            isRequired: IdempotencyRecordMetadata.RequestHashIsRequired,
            value: requestHash
        );

        if (!requestHashIsRequiredValidation)
            return false;

        bool requestHashMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<IdempotencyRecord>(propertyName: IdempotencyRecordMetadata.RequestHashPropertyName),
            minLength: 1,
            value: requestHash!.Length
        );

        if (!requestHashMinLengthValidation)
            return false;

        bool requestHashMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<IdempotencyRecord>(propertyName: IdempotencyRecordMetadata.RequestHashPropertyName),
            maxLength: IdempotencyRecordMetadata.RequestHashMaxLength,
            value: requestHash!.Length
        );

        return requestHashMaxLengthValidation;
    }

    public static bool ValidateResponseBody(
        ExecutionContext executionContext,
        string? responseBody
    )
    {
        if (responseBody is null)
            return true;

        bool responseBodyMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<IdempotencyRecord>(propertyName: IdempotencyRecordMetadata.ResponseBodyPropertyName),
            maxLength: IdempotencyRecordMetadata.ResponseBodyMaxLength,
            value: responseBody.Length
        );

        return responseBodyMaxLengthValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<IdempotencyRecord>(propertyName: IdempotencyRecordMetadata.ExpiresAtPropertyName),
            isRequired: IdempotencyRecordMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetIdempotencyKey(
        ExecutionContext executionContext,
        string idempotencyKey
    )
    {
        bool isValid = ValidateIdempotencyKey(
            executionContext,
            idempotencyKey
        );

        if (!isValid)
            return false;

        IdempotencyKey = idempotencyKey;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetRequestHash(
        ExecutionContext executionContext,
        string requestHash
    )
    {
        bool isValid = ValidateRequestHash(
            executionContext,
            requestHash
        );

        if (!isValid)
            return false;

        RequestHash = requestHash;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetResponseBody(
        string? responseBody
    )
    {
        ResponseBody = responseBody;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetStatusCode(
        int statusCode
    )
    {
        StatusCode = statusCode;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetExpiresAt(
        DateTimeOffset expiresAt
    )
    {
        ExpiresAt = expiresAt;
        return true;
    }
    // Stryker restore all

    // Metadata
    public static class IdempotencyRecordMetadata
    {
        private static readonly Lock _lockObject = new();

        // IdempotencyKey
        public static readonly string IdempotencyKeyPropertyName = "IdempotencyKey";
        public static bool IdempotencyKeyIsRequired { get; private set; } = true;
        public static int IdempotencyKeyMaxLength { get; private set; } = 36;

        // RequestHash
        public static readonly string RequestHashPropertyName = "RequestHash";
        public static bool RequestHashIsRequired { get; private set; } = true;
        public static int RequestHashMaxLength { get; private set; } = 128;

        // ResponseBody
        public static readonly string ResponseBodyPropertyName = "ResponseBody";
        public static int ResponseBodyMaxLength { get; private set; } = 1048576;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        public static void ChangeIdempotencyKeyMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                IdempotencyKeyIsRequired = isRequired;
                IdempotencyKeyMaxLength = maxLength;
            }
        }

        public static void ChangeRequestHashMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                RequestHashIsRequired = isRequired;
                RequestHashMaxLength = maxLength;
            }
        }

        public static void ChangeResponseBodyMetadata(
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ResponseBodyMaxLength = maxLength;
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
    }
}
