using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Inputs;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.ConsentTerms;

public sealed class ConsentTerm
    : EntityBase<ConsentTerm>,
    IConsentTerm
{
    // Properties
    public ConsentTermType Type { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string TermVersion { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset PublishedAt { get; private set; }

    // Constructors
    private ConsentTerm()
    {
    }

    private ConsentTerm(
        EntityInfo entityInfo,
        ConsentTermType type,
        string termVersion,
        string content,
        DateTimeOffset publishedAt
    ) : base(entityInfo)
    {
        Type = type;
        TermVersion = termVersion;
        Content = content;
        PublishedAt = publishedAt;
    }

    // Public Business Methods
    public static ConsentTerm? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewConsentTermInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new ConsentTerm(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetType(executionContext, input.Type)
                    & instance.SetTermVersion(executionContext, input.TermVersion)
                    & instance.SetContent(executionContext, input.Content)
                    & instance.SetPublishedAt(executionContext, input.PublishedAt);
            }
        );
    }

    public static ConsentTerm CreateFromExistingInfo(
        CreateFromExistingInfoConsentTermInput input
    )
    {
        return new ConsentTerm(
            input.EntityInfo,
            input.Type,
            input.TermVersion,
            input.Content,
            input.PublishedAt
        );
    }

    public override ConsentTerm Clone()
    {
        return new ConsentTerm(
            EntityInfo,
            Type,
            TermVersion,
            Content,
            PublishedAt
        );
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        ConsentTermType? type,
        string? termVersion,
        string? content,
        DateTimeOffset? publishedAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateType(executionContext, type)
            & ValidateTermVersion(executionContext, termVersion)
            & ValidateContent(executionContext, content)
            & ValidatePublishedAt(executionContext, publishedAt);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Type,
            TermVersion,
            Content,
            PublishedAt
        );
    }

    public static bool ValidateType(
        ExecutionContext executionContext,
        ConsentTermType? type
    )
    {
        bool typeIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ConsentTerm>(propertyName: ConsentTermMetadata.TypePropertyName),
            isRequired: ConsentTermMetadata.TypeIsRequired,
            value: type
        );

        return typeIsRequiredValidation;
    }

    public static bool ValidateTermVersion(
        ExecutionContext executionContext,
        string? termVersion
    )
    {
        bool termVersionIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ConsentTerm>(propertyName: ConsentTermMetadata.TermVersionPropertyName),
            isRequired: ConsentTermMetadata.TermVersionIsRequired,
            value: termVersion
        );

        if (!termVersionIsRequiredValidation)
            return false;

        bool termVersionMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ConsentTerm>(propertyName: ConsentTermMetadata.TermVersionPropertyName),
            maxLength: ConsentTermMetadata.TermVersionMaxLength,
            value: termVersion!.Length
        );

        return termVersionMaxLengthValidation;
    }

    public static bool ValidateContent(
        ExecutionContext executionContext,
        string? content
    )
    {
        bool contentIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ConsentTerm>(propertyName: ConsentTermMetadata.ContentPropertyName),
            isRequired: ConsentTermMetadata.ContentIsRequired,
            value: content
        );

        if (!contentIsRequiredValidation)
            return false;

        bool contentMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<ConsentTerm>(propertyName: ConsentTermMetadata.ContentPropertyName),
            maxLength: ConsentTermMetadata.ContentMaxLength,
            value: content!.Length
        );

        return contentMaxLengthValidation;
    }

    public static bool ValidatePublishedAt(
        ExecutionContext executionContext,
        DateTimeOffset? publishedAt
    )
    {
        bool publishedAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<ConsentTerm>(propertyName: ConsentTermMetadata.PublishedAtPropertyName),
            isRequired: ConsentTermMetadata.PublishedAtIsRequired,
            value: publishedAt
        );

        return publishedAtIsRequiredValidation;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetType(
        ExecutionContext executionContext,
        ConsentTermType type
    )
    {
        bool isValid = ValidateType(
            executionContext,
            type
        );

        if (!isValid)
            return false;

        Type = type;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetTermVersion(
        ExecutionContext executionContext,
        string termVersion
    )
    {
        bool isValid = ValidateTermVersion(
            executionContext,
            termVersion
        );

        if (!isValid)
            return false;

        TermVersion = termVersion;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetContent(
        ExecutionContext executionContext,
        string content
    )
    {
        bool isValid = ValidateContent(
            executionContext,
            content
        );

        if (!isValid)
            return false;

        Content = content;

        return true;
    }

    // Stryker disable once Block : SetPublishedAt recebe DateTimeOffset valido de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetPublishedAt recebe DateTimeOffset valido de RegisterNew - branch false inalcancavel")]
    private bool SetPublishedAt(
        ExecutionContext executionContext,
        DateTimeOffset publishedAt
    )
    {
        bool isValid = ValidatePublishedAt(
            executionContext,
            publishedAt
        );

        if (!isValid)
            return false;

        PublishedAt = publishedAt;

        return true;
    }
    // Stryker restore all

    // Metadata
    public static class ConsentTermMetadata
    {
        private static readonly Lock _lockObject = new();

        // Type
        public static readonly string TypePropertyName = "Type";
        public static bool TypeIsRequired { get; private set; } = true;

        // TermVersion
        public static readonly string TermVersionPropertyName = "TermVersion";
        public static bool TermVersionIsRequired { get; private set; } = true;
        public static int TermVersionMaxLength { get; private set; } = 50;

        // Content
        public static readonly string ContentPropertyName = "Content";
        public static bool ContentIsRequired { get; private set; } = true;
        public static int ContentMaxLength { get; private set; } = 100000;

        // PublishedAt
        public static readonly string PublishedAtPropertyName = "PublishedAt";
        public static bool PublishedAtIsRequired { get; private set; } = true;

        public static void ChangeTypeMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                TypeIsRequired = isRequired;
            }
        }

        public static void ChangeTermVersionMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                TermVersionIsRequired = isRequired;
                TermVersionMaxLength = maxLength;
            }
        }

        public static void ChangeContentMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                ContentIsRequired = isRequired;
                ContentMaxLength = maxLength;
            }
        }

        public static void ChangePublishedAtMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                PublishedAtIsRequired = isRequired;
            }
        }
    }
}
