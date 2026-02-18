using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.SigningKeys;

public sealed class SigningKey
    : EntityBase<SigningKey>,
    ISigningKey
{
    // Properties
    public Kid Kid { get; private set; }
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Algorithm { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string PublicKey { get; private set; } = string.Empty;
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string EncryptedPrivateKey { get; private set; } = string.Empty;
    public SigningKeyStatus Status { get; private set; }
    public DateTimeOffset? RotatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    // Constructors
    private SigningKey()
    {
    }

    private SigningKey(
        EntityInfo entityInfo,
        Kid kid,
        string algorithm,
        string publicKey,
        string encryptedPrivateKey,
        SigningKeyStatus status,
        DateTimeOffset? rotatedAt,
        DateTimeOffset expiresAt
    ) : base(entityInfo)
    {
        Kid = kid;
        Algorithm = algorithm;
        PublicKey = publicKey;
        EncryptedPrivateKey = encryptedPrivateKey;
        Status = status;
        RotatedAt = rotatedAt;
        ExpiresAt = expiresAt;
    }

    // Public Business Methods
    public static SigningKey? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewSigningKeyInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new SigningKey(),
            handler: static (executionContext, input, instance) =>
            {
                return instance.SetKid(executionContext, input.Kid)
                    & instance.SetAlgorithm(executionContext, input.Algorithm)
                    & instance.SetPublicKey(executionContext, input.PublicKey)
                    & instance.SetEncryptedPrivateKey(executionContext, input.EncryptedPrivateKey)
                    & instance.SetStatus(executionContext, SigningKeyStatus.Active)
                    & instance.SetRotatedAt(null)
                    & instance.SetExpiresAt(executionContext, input.ExpiresAt);
            }
        );
    }

    public static SigningKey CreateFromExistingInfo(
        CreateFromExistingInfoSigningKeyInput input
    )
    {
        return new SigningKey(
            input.EntityInfo,
            input.Kid,
            input.Algorithm,
            input.PublicKey,
            input.EncryptedPrivateKey,
            input.Status,
            input.RotatedAt,
            input.ExpiresAt
        );
    }

    public SigningKey? Rotate(
        ExecutionContext executionContext,
        RotateSigningKeyInput input
    )
    {
        return RegisterChangeInternal<SigningKey, RotateSigningKeyInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RotateInternal(executionContext);
            }
        );
    }

    public SigningKey? Revoke(
        ExecutionContext executionContext,
        RevokeSigningKeyInput input
    )
    {
        return RegisterChangeInternal<SigningKey, RevokeSigningKeyInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.RevokeInternal(executionContext);
            }
        );
    }

    public override SigningKey Clone()
    {
        return new SigningKey(
            EntityInfo,
            Kid,
            Algorithm,
            PublicKey,
            EncryptedPrivateKey,
            Status,
            RotatedAt,
            ExpiresAt
        );
    }

    // Private Business Methods
    // Stryker disable all : Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool RotateInternal(
        ExecutionContext executionContext
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, SigningKeyStatus.Rotated);

        if (!isValidTransition)
            return false;

        Status = SigningKeyStatus.Rotated;
        RotatedAt = executionContext.Timestamp;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterChangeInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool RevokeInternal(
        ExecutionContext executionContext
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, Status, SigningKeyStatus.Revoked);

        if (!isValidTransition)
            return false;

        Status = SigningKeyStatus.Revoked;

        return true;
    }
    // Stryker restore all

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        Kid? kid,
        string? algorithm,
        string? publicKey,
        string? encryptedPrivateKey,
        SigningKeyStatus? status,
        DateTimeOffset? expiresAt
    )
    {
        return EntityBaseIsValid(executionContext, entityInfo)
            & ValidateKid(executionContext, kid)
            & ValidateAlgorithm(executionContext, algorithm)
            & ValidatePublicKey(executionContext, publicKey)
            & ValidateEncryptedPrivateKey(executionContext, encryptedPrivateKey)
            & ValidateStatus(executionContext, status)
            & ValidateExpiresAt(executionContext, expiresAt);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Kid,
            Algorithm,
            PublicKey,
            EncryptedPrivateKey,
            Status,
            ExpiresAt
        );
    }

    public static bool ValidateKid(
        ExecutionContext executionContext,
        Kid? kid
    )
    {
        bool kidIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.KidPropertyName),
            isRequired: SigningKeyMetadata.KidIsRequired,
            value: kid
        );

        return kidIsRequiredValidation;
    }

    public static bool ValidateAlgorithm(
        ExecutionContext executionContext,
        string? algorithm
    )
    {
        bool algorithmIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.AlgorithmPropertyName),
            isRequired: SigningKeyMetadata.AlgorithmIsRequired,
            value: algorithm
        );

        if (!algorithmIsRequiredValidation)
            return false;

        bool algorithmMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.AlgorithmPropertyName),
            maxLength: SigningKeyMetadata.AlgorithmMaxLength,
            value: algorithm!.Length
        );

        return algorithmMaxLengthValidation;
    }

    public static bool ValidatePublicKey(
        ExecutionContext executionContext,
        string? publicKey
    )
    {
        bool publicKeyIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.PublicKeyPropertyName),
            isRequired: SigningKeyMetadata.PublicKeyIsRequired,
            value: publicKey
        );

        if (!publicKeyIsRequiredValidation)
            return false;

        bool publicKeyMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.PublicKeyPropertyName),
            maxLength: SigningKeyMetadata.PublicKeyMaxLength,
            value: publicKey!.Length
        );

        return publicKeyMaxLengthValidation;
    }

    public static bool ValidateEncryptedPrivateKey(
        ExecutionContext executionContext,
        string? encryptedPrivateKey
    )
    {
        bool encryptedPrivateKeyIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.EncryptedPrivateKeyPropertyName),
            isRequired: SigningKeyMetadata.EncryptedPrivateKeyIsRequired,
            value: encryptedPrivateKey
        );

        if (!encryptedPrivateKeyIsRequiredValidation)
            return false;

        bool encryptedPrivateKeyMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.EncryptedPrivateKeyPropertyName),
            maxLength: SigningKeyMetadata.EncryptedPrivateKeyMaxLength,
            value: encryptedPrivateKey!.Length
        );

        return encryptedPrivateKeyMaxLengthValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        SigningKeyStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.StatusPropertyName),
            isRequired: SigningKeyMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset? expiresAt
    )
    {
        bool expiresAtIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.ExpiresAtPropertyName),
            isRequired: SigningKeyMetadata.ExpiresAtIsRequired,
            value: expiresAt
        );

        return expiresAtIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        SigningKeyStatus? from,
        SigningKeyStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (SigningKeyStatus.Active, SigningKeyStatus.Rotated) => true,
            (SigningKeyStatus.Active, SigningKeyStatus.Revoked) => true,
            (SigningKeyStatus.Rotated, SigningKeyStatus.Revoked) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<SigningKey>(propertyName: SigningKeyMetadata.StatusPropertyName)}.InvalidTransition");
        }

        return isValid;
    }

    // Set Methods
    // Stryker disable all : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetKid(
        ExecutionContext executionContext,
        Kid kid
    )
    {
        bool isValid = ValidateKid(executionContext, kid);

        if (!isValid)
            return false;

        Kid = kid;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetAlgorithm(
        ExecutionContext executionContext,
        string algorithm
    )
    {
        bool isValid = ValidateAlgorithm(executionContext, algorithm);

        if (!isValid)
            return false;

        Algorithm = algorithm;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetPublicKey(
        ExecutionContext executionContext,
        string publicKey
    )
    {
        bool isValid = ValidatePublicKey(executionContext, publicKey);

        if (!isValid)
            return false;

        PublicKey = publicKey;

        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetEncryptedPrivateKey(
        ExecutionContext executionContext,
        string encryptedPrivateKey
    )
    {
        bool isValid = ValidateEncryptedPrivateKey(executionContext, encryptedPrivateKey);

        if (!isValid)
            return false;

        EncryptedPrivateKey = encryptedPrivateKey;

        return true;
    }

    // Stryker disable once Block : SetStatus recebe SigningKeyStatus.Active de RegisterNew - branch false inalcancavel
    [ExcludeFromCodeCoverage(Justification = "SetStatus recebe SigningKeyStatus.Active de RegisterNew - branch false inalcancavel")]
    private bool SetStatus(
        ExecutionContext executionContext,
        SigningKeyStatus status
    )
    {
        bool isValid = ValidateStatus(executionContext, status);

        if (!isValid)
            return false;

        Status = status;

        return true;
    }

    private bool SetRotatedAt(DateTimeOffset? rotatedAt)
    {
        RotatedAt = rotatedAt;
        return true;
    }

    [ExcludeFromCodeCoverage(Justification = "Chamado via static lambda em RegisterNewInternal - Coverlet nao rastreia cobertura atraves de delegates estaticos")]
    private bool SetExpiresAt(
        ExecutionContext executionContext,
        DateTimeOffset expiresAt
    )
    {
        bool isValid = ValidateExpiresAt(executionContext, expiresAt);

        if (!isValid)
            return false;

        ExpiresAt = expiresAt;

        return true;
    }
    // Stryker restore all

    // Metadata
    public static class SigningKeyMetadata
    {
        private static readonly Lock _lockObject = new();

        // Kid
        public static readonly string KidPropertyName = "Kid";
        public static bool KidIsRequired { get; private set; } = true;

        // Algorithm
        public static readonly string AlgorithmPropertyName = "Algorithm";
        public static bool AlgorithmIsRequired { get; private set; } = true;
        public static int AlgorithmMaxLength { get; private set; } = 20;

        // PublicKey
        public static readonly string PublicKeyPropertyName = "PublicKey";
        public static bool PublicKeyIsRequired { get; private set; } = true;
        public static int PublicKeyMaxLength { get; private set; } = 512;

        // EncryptedPrivateKey
        public static readonly string EncryptedPrivateKeyPropertyName = "EncryptedPrivateKey";
        public static bool EncryptedPrivateKeyIsRequired { get; private set; } = true;
        public static int EncryptedPrivateKeyMaxLength { get; private set; } = 2048;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        // ExpiresAt
        public static readonly string ExpiresAtPropertyName = "ExpiresAt";
        public static bool ExpiresAtIsRequired { get; private set; } = true;

        public static void ChangeKidMetadata(bool isRequired)
        {
            lock (_lockObject) { KidIsRequired = isRequired; }
        }

        public static void ChangeAlgorithmMetadata(bool isRequired, int maxLength)
        {
            lock (_lockObject) { AlgorithmIsRequired = isRequired; AlgorithmMaxLength = maxLength; }
        }

        public static void ChangePublicKeyMetadata(bool isRequired, int maxLength)
        {
            lock (_lockObject) { PublicKeyIsRequired = isRequired; PublicKeyMaxLength = maxLength; }
        }

        public static void ChangeEncryptedPrivateKeyMetadata(bool isRequired, int maxLength)
        {
            lock (_lockObject) { EncryptedPrivateKeyIsRequired = isRequired; EncryptedPrivateKeyMaxLength = maxLength; }
        }

        public static void ChangeStatusMetadata(bool isRequired)
        {
            lock (_lockObject) { StatusIsRequired = isRequired; }
        }

        public static void ChangeExpiresAtMetadata(bool isRequired)
        {
            lock (_lockObject) { ExpiresAtIsRequired = isRequired; }
        }
    }
}
