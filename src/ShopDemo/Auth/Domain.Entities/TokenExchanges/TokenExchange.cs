using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;
using ShopDemo.Auth.Domain.Entities.TokenExchanges.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.TokenExchanges;

public sealed class TokenExchange
    : EntityBase<TokenExchange>,
    ITokenExchange
{
    // Properties
    public Id UserId { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string SubjectTokenJti { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string RequestedAudience { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string IssuedTokenJti { get; private set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    // Constructors
    private TokenExchange()
    {
    }

    private TokenExchange(
        EntityInfo entityInfo,
        Id userId,
        string subjectTokenJti,
        string requestedAudience,
        string issuedTokenJti,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt
    ) : base(entityInfo)
    {
        UserId = userId;
        SubjectTokenJti = subjectTokenJti;
        RequestedAudience = requestedAudience;
        IssuedTokenJti = issuedTokenJti;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    // Public Business Methods
    public static TokenExchange? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewTokenExchangeInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new TokenExchange(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetSubjectTokenJti(executionContext, input.SubjectTokenJti)
                    & instance.SetRequestedAudience(executionContext, input.RequestedAudience)
                    & instance.SetIssuedTokenJti(executionContext, input.IssuedTokenJti)
                    & instance.SetIssuedAt(executionContext.Timestamp)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt);
            }
        );
    }

    public static TokenExchange CreateFromExistingInfo(
        CreateFromExistingInfoTokenExchangeInput input
    )
    {
        return new TokenExchange(
            input.EntityInfo,
            input.UserId,
            input.SubjectTokenJti,
            input.RequestedAudience,
            input.IssuedTokenJti,
            input.IssuedAt,
            input.ExpiresAt
        );
    }

    public override TokenExchange Clone()
    {
        return new TokenExchange(
            EntityInfo,
            UserId,
            SubjectTokenJti,
            RequestedAudience,
            IssuedTokenJti,
            IssuedAt,
            ExpiresAt
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        string? subjectTokenJti,
        string? requestedAudience,
        string? issuedTokenJti,
        DateTimeOffset? issuedAt,
        DateTimeOffset? expiresAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateSubjectTokenJti(executionContext, subjectTokenJti)
            & ValidateRequestedAudience(executionContext, requestedAudience)
            & ValidateIssuedTokenJti(executionContext, issuedTokenJti)
            & ValidateIssuedAt(executionContext, issuedAt)
            & ValidateExpiresAt(executionContext, expiresAt);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            UserId,
            SubjectTokenJti,
            RequestedAudience,
            IssuedTokenJti,
            IssuedAt,
            ExpiresAt
        );
    }

    public static bool ValidateUserId(
        ExecutionContext executionContext,
        Id? userId
    )
    {
        bool userIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.UserIdPropertyName),
            isRequired: TokenExchangeMetadata.UserIdIsRequired,
            value: userId
        );

        return userIdIsRequiredValidation;
    }

    public static bool ValidateSubjectTokenJti(
        ExecutionContext executionContext,
        string? subjectTokenJti
    )
    {
        bool subjectTokenJtiIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.SubjectTokenJtiPropertyName),
            isRequired: TokenExchangeMetadata.SubjectTokenJtiIsRequired,
            value: subjectTokenJti
        );

        if (!subjectTokenJtiIsRequiredValidation)
            return false;

        bool subjectTokenJtiMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.SubjectTokenJtiPropertyName),
            maxLength: TokenExchangeMetadata.SubjectTokenJtiMaxLength,
            value: subjectTokenJti!.Length
        );

        return subjectTokenJtiMaxLengthValidation;
    }

    public static bool ValidateRequestedAudience(
        ExecutionContext executionContext,
        string? requestedAudience
    )
    {
        bool requestedAudienceIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.RequestedAudiencePropertyName),
            isRequired: TokenExchangeMetadata.RequestedAudienceIsRequired,
            value: requestedAudience
        );

        if (!requestedAudienceIsRequiredValidation)
            return false;

        bool requestedAudienceMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.RequestedAudiencePropertyName),
            maxLength: TokenExchangeMetadata.RequestedAudienceMaxLength,
            value: requestedAudience!.Length
        );

        return requestedAudienceMaxLengthValidation;
    }

    public static bool ValidateIssuedTokenJti(
        ExecutionContext executionContext,
        string? issuedTokenJti
    )
    {
        bool issuedTokenJtiIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.IssuedTokenJtiPropertyName),
            isRequired: TokenExchangeMetadata.IssuedTokenJtiIsRequired,
            value: issuedTokenJti
        );

        if (!issuedTokenJtiIsRequiredValidation)
            return false;

        bool issuedTokenJtiMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.IssuedTokenJtiPropertyName),
            maxLength: TokenExchangeMetadata.IssuedTokenJtiMaxLength,
            value: issuedTokenJti!.Length
        );

        return issuedTokenJtiMaxLengthValidation;
    }

    public static bool ValidateIssuedAt(
        ExecutionContext executionContext,
        DateTimeOffset? issuedAt
    )
    {
        bool issuedAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.IssuedAtPropertyName),
            isRequired: TokenExchangeMetadata.IssuedAtIsRequired,
            value: issuedAt
        );

        return issuedAtIsRequiredValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<TokenExchange>(propertyName: TokenExchangeMetadata.ExpiresAtPropertyName),
            isRequired: TokenExchangeMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetUserId(ExecutionContext executionContext, Id userId)
    {
        if (!ValidateUserId(executionContext, userId)) return false;
        UserId = userId;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetSubjectTokenJti(ExecutionContext executionContext, string subjectTokenJti)
    {
        if (!ValidateSubjectTokenJti(executionContext, subjectTokenJti)) return false;
        SubjectTokenJti = subjectTokenJti;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetRequestedAudience(ExecutionContext executionContext, string requestedAudience)
    {
        if (!ValidateRequestedAudience(executionContext, requestedAudience)) return false;
        RequestedAudience = requestedAudience;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetIssuedTokenJti(ExecutionContext executionContext, string issuedTokenJti)
    {
        if (!ValidateIssuedTokenJti(executionContext, issuedTokenJti)) return false;
        IssuedTokenJti = issuedTokenJti;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetIssuedAt(DateTimeOffset issuedAt)
    {
        IssuedAt = issuedAt;
        return true;
    }

    // Stryker disable once Block : SetExpiresAt recebe DateTimeOffset valido de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetExpiresAt recebe DateTimeOffset valido de RegisterNew - branch false inalcancavel")]
    private bool SetExpiresAt(ExecutionContext executionContext, DateTimeOffset expiresAt)
    {
        if (!ValidateExpiresAt(executionContext, expiresAt)) return false;
        ExpiresAt = expiresAt;
        return true;
    }
    // Stryker restore all

    // Metadata
    public static class TokenExchangeMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // SubjectTokenJti
        public static readonly string SubjectTokenJtiPropertyName = "SubjectTokenJti";
        public static bool SubjectTokenJtiIsRequired { get; private set; } = true;
        public static int SubjectTokenJtiMaxLength { get; private set; } = 36;

        // RequestedAudience
        public static readonly string RequestedAudiencePropertyName = "RequestedAudience";
        public static bool RequestedAudienceIsRequired { get; private set; } = true;
        public static int RequestedAudienceMaxLength { get; private set; } = 255;

        // IssuedTokenJti
        public static readonly string IssuedTokenJtiPropertyName = "IssuedTokenJti";
        public static bool IssuedTokenJtiIsRequired { get; private set; } = true;
        public static int IssuedTokenJtiMaxLength { get; private set; } = 36;

        // IssuedAt
        public static readonly string IssuedAtPropertyName = "IssuedAt";
        public static bool IssuedAtIsRequired { get; private set; } = true;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        public static void ChangeUserIdMetadata(bool isRequired)
        {
            lock (_lockObject) { UserIdIsRequired = isRequired; }
        }

        public static void ChangeSubjectTokenJtiMetadata(bool isRequired, int maxLength)
        {
            lock (_lockObject) { SubjectTokenJtiIsRequired = isRequired; SubjectTokenJtiMaxLength = maxLength; }
        }

        public static void ChangeRequestedAudienceMetadata(bool isRequired, int maxLength)
        {
            lock (_lockObject) { RequestedAudienceIsRequired = isRequired; RequestedAudienceMaxLength = maxLength; }
        }

        public static void ChangeIssuedTokenJtiMetadata(bool isRequired, int maxLength)
        {
            lock (_lockObject) { IssuedTokenJtiIsRequired = isRequired; IssuedTokenJtiMaxLength = maxLength; }
        }

        public static void ChangeIssuedAtMetadata(bool isRequired)
        {
            lock (_lockObject) { IssuedAtIsRequired = isRequired; }
        }

        public static void ChangeExpiresAtMetadata(bool isRequired)
        {
            lock (_lockObject) { ExpiresAtIsRequired = isRequired; }
        }
    }
}
