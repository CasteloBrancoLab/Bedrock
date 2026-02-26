using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ExternalLogins.Inputs;
using ShopDemo.Auth.Domain.Entities.ExternalLogins.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.ExternalLogins;

public sealed class ExternalLogin
    : EntityBase<ExternalLogin>,
    IExternalLogin
{
    // Properties
    public Id UserId { get; private set; }
    public LoginProvider Provider { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string ProviderUserId { get; private set; } = string.Empty;
    public string? Email { get; private set; }

    // Constructors
    private ExternalLogin()
    {
    }

    private ExternalLogin(
        EntityInfo entityInfo,
        Id userId,
        LoginProvider provider,
        string providerUserId,
        string? email
    ) : base(entityInfo)
    {
        UserId = userId;
        Provider = provider;
        ProviderUserId = providerUserId;
        Email = email;
    }

    // Public Business Methods
    public static ExternalLogin? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewExternalLoginInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new ExternalLogin(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetUserId(executionContext, input.UserId)
                    & instance.SetProvider(executionContext, input.Provider)
                    & instance.SetProviderUserId(executionContext, input.ProviderUserId)
                    & instance.SetEmail(input.Email);
            }
        );
    }

    public static ExternalLogin CreateFromExistingInfo(
        CreateFromExistingInfoExternalLoginInput input
    )
    {
        return new ExternalLogin(
            input.EntityInfo,
            input.UserId,
            input.Provider,
            input.ProviderUserId,
            input.Email
        );
    }

    public override ExternalLogin Clone()
    {
        return new ExternalLogin(
            EntityInfo,
            UserId,
            Provider,
            ProviderUserId,
            Email
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Id? userId,
        LoginProvider? provider,
        string? providerUserId
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUserId(executionContext, userId)
            & ValidateProvider(executionContext, provider)
            & ValidateProviderUserId(executionContext, providerUserId);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            UserId,
            Provider,
            ProviderUserId
        );
    }

    public static bool ValidateUserId(
        ExecutionContext executionContext,
        Id? userId
    )
    {
        bool userIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ExternalLogin>(propertyName: ExternalLoginMetadata.UserIdPropertyName),
            isRequired: ExternalLoginMetadata.UserIdIsRequired,
            value: userId
        );

        return userIdIsRequiredValidation;
    }

    public static bool ValidateProvider(
        ExecutionContext executionContext,
        LoginProvider? provider
    )
    {
        bool providerIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ExternalLogin>(propertyName: ExternalLoginMetadata.ProviderPropertyName),
            isRequired: ExternalLoginMetadata.ProviderIsRequired,
            value: provider
        );

        if (!providerIsRequiredValidation)
            return false;

        bool providerValueIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ExternalLogin>(propertyName: ExternalLoginMetadata.ProviderPropertyName),
            isRequired: true,
            value: provider!.Value.Value
        );

        if (!providerValueIsRequiredValidation)
            return false;

        bool providerMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ExternalLogin>(propertyName: ExternalLoginMetadata.ProviderPropertyName),
            maxLength: ExternalLoginMetadata.ProviderMaxLength,
            value: provider!.Value.Value.Length
        );

        return providerMaxLengthValidation;
    }

    public static bool ValidateProviderUserId(
        ExecutionContext executionContext,
        string? providerUserId
    )
    {
        bool providerUserIdIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ExternalLogin>(propertyName: ExternalLoginMetadata.ProviderUserIdPropertyName),
            isRequired: ExternalLoginMetadata.ProviderUserIdIsRequired,
            value: providerUserId
        );

        if (!providerUserIdIsRequiredValidation)
            return false;

        bool providerUserIdMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ExternalLogin>(propertyName: ExternalLoginMetadata.ProviderUserIdPropertyName),
            maxLength: ExternalLoginMetadata.ProviderUserIdMaxLength,
            value: providerUserId!.Length
        );

        return providerUserIdMaxLengthValidation;
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
    private bool SetProvider(
        ExecutionContext executionContext,
        LoginProvider provider
    )
    {
        bool isValid = ValidateProvider(
            executionContext,
            provider
        );

        if (!isValid)
            return false;

        Provider = provider;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetProviderUserId(
        ExecutionContext executionContext,
        string providerUserId
    )
    {
        bool isValid = ValidateProviderUserId(
            executionContext,
            providerUserId
        );

        if (!isValid)
            return false;

        ProviderUserId = providerUserId;

        return true;
    }
    // Stryker restore all

    private bool SetEmail(string? email)
    {
        Email = email;
        return true;
    }

    // Metadata
    public static class ExternalLoginMetadata
    {
        private static readonly Lock _lockObject = new();

        // UserId
        public static readonly string UserIdPropertyName = "UserId";
        public static bool UserIdIsRequired { get; private set; } = true;

        // Provider
        public static readonly string ProviderPropertyName = "Provider";
        public static bool ProviderIsRequired { get; private set; } = true;
        public static int ProviderMaxLength { get; private set; } = 50;

        // ProviderUserId
        public static readonly string ProviderUserIdPropertyName = "ProviderUserId";
        public static bool ProviderUserIdIsRequired { get; private set; } = true;
        public static int ProviderUserIdMaxLength { get; private set; } = 255;

        // Email
        public static readonly string EmailPropertyName = "Email";
        public static int EmailMaxLength { get; private set; } = 320;

        public static void ChangeUserIdMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                UserIdIsRequired = isRequired;
            }
        }

        public static void ChangeProviderMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ProviderIsRequired = isRequired;
                ProviderMaxLength = maxLength;
            }
        }

        public static void ChangeProviderUserIdMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ProviderUserIdIsRequired = isRequired;
                ProviderUserIdMaxLength = maxLength;
            }
        }

        public static void ChangeEmailMetadata(
            int maxLength
        )
        {
            lock (_lockObject)
            {
                EmailMaxLength = maxLength;
            }
        }
    }
}
