using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Core.Entities.Users.Enums;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Domain.Entities.Users.Interfaces;

namespace ShopDemo.Auth.Domain.Entities.Users;

public sealed class User
    : EntityBase<User>,
    IAggregateRoot,
    IUser
{
    // Properties
    // Stryker disable once String : Default initializer is always overwritten by RegisterNew and CreateFromExistingInfo constructors
    public string Username { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public UserStatus Status { get; private set; }

    // Constructors
    private User()
    {
    }

    private User(
        EntityInfo entityInfo,
        string username,
        EmailAddress email,
        PasswordHash passwordHash,
        UserStatus status
    ) : base(entityInfo)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Status = status;
    }

    // Public Business Methods
    public static User? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: static (executionContext, input) => new User(),
            handler: static (executionContext, input, instance) =>
            {
                string username = input.Email.Value?.ToLowerInvariant() ?? string.Empty;

                return
                    instance.SetUsername(executionContext, username)
                    & instance.SetEmail(executionContext, input.Email)
                    & instance.SetPasswordHash(executionContext, input.PasswordHash)
                    & instance.SetStatus(executionContext, UserStatus.Active);
            }
        );
    }

    public static User CreateFromExistingInfo(
        CreateFromExistingInfoInput input
    )
    {
        return new User(
            input.EntityInfo,
            input.Username,
            input.Email,
            input.PasswordHash,
            input.Status
        );
    }

    public User? ChangeStatus(
        ExecutionContext executionContext,
        ChangeStatusInput input
    )
    {
        return RegisterChangeInternal<User, ChangeStatusInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeStatusInternal(executionContext, newInstance.Status, input.NewStatus);
            }
        );
    }

    public User? ChangeUsername(
        ExecutionContext executionContext,
        ChangeUsernameInput input
    )
    {
        return RegisterChangeInternal<User, ChangeUsernameInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeUsernameInternal(executionContext, input.NewUsername);
            }
        );
    }

    public User? ChangePasswordHash(
        ExecutionContext executionContext,
        ChangePasswordHashInput input
    )
    {
        return RegisterChangeInternal<User, ChangePasswordHashInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangePasswordHashInternal(executionContext, input.NewPasswordHash);
            }
        );
    }

    public override User Clone()
    {
        return new User(
            EntityInfo,
            Username,
            Email,
            PasswordHash,
            Status
        );
    }

    // Private Business Methods
    private bool ChangeStatusInternal(
        ExecutionContext executionContext,
        UserStatus currentStatus,
        UserStatus newStatus
    )
    {
        bool isValidTransition = ValidateStatusTransition(executionContext, currentStatus, newStatus);

        if (!isValidTransition)
            return false;

        Status = newStatus;

        return true;
    }

    private bool ChangeUsernameInternal(
        ExecutionContext executionContext,
        string newUsername
    )
    {
        return SetUsername(executionContext, newUsername);
    }

    private bool ChangePasswordHashInternal(
        ExecutionContext executionContext,
        PasswordHash newPasswordHash
    )
    {
        return SetPasswordHash(executionContext, newPasswordHash);
    }

    // Validation Methods
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? username,
        EmailAddress? email,
        PasswordHash? passwordHash,
        UserStatus? status
    )
    {
        return
            EntityBaseIsValid(executionContext, entityInfo)
            & ValidateUsername(executionContext, username)
            & ValidateEmail(executionContext, email)
            & ValidatePasswordHash(executionContext, passwordHash)
            & ValidateStatus(executionContext, status);
    }

    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Username,
            Email,
            PasswordHash,
            Status
        );
    }

    public static bool ValidateUsername(
        ExecutionContext executionContext,
        string? username
    )
    {
        bool usernameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.UsernamePropertyName),
            isRequired: UserMetadata.UsernameIsRequired,
            value: username
        );

        if (!usernameIsRequiredValidation)
            return false;

        bool usernameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.UsernamePropertyName),
            minLength: UserMetadata.UsernameMinLength,
            value: username!.Length
        );

        bool usernameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.UsernamePropertyName),
            maxLength: UserMetadata.UsernameMaxLength,
            value: username!.Length
        );

        return usernameIsRequiredValidation
            && usernameMinLengthValidation
            && usernameMaxLengthValidation;
    }

    public static bool ValidateEmail(
        ExecutionContext executionContext,
        EmailAddress? email
    )
    {
        bool emailIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.EmailPropertyName),
            isRequired: UserMetadata.EmailIsRequired,
            value: email
        );

        return emailIsRequiredValidation;
    }

    public static bool ValidatePasswordHash(
        ExecutionContext executionContext,
        PasswordHash? passwordHash
    )
    {
        bool passwordHashIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.PasswordHashPropertyName),
            isRequired: UserMetadata.PasswordHashIsRequired,
            value: passwordHash
        );

        if (!passwordHashIsRequiredValidation)
            return false;

        if (passwordHash!.Value.IsEmpty)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<User>(propertyName: UserMetadata.PasswordHashPropertyName)}.IsRequired");
            return false;
        }

        bool passwordHashMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.PasswordHashPropertyName),
            maxLength: UserMetadata.PasswordHashMaxLength,
            value: passwordHash.Value.Length
        );

        return passwordHashIsRequiredValidation
            && passwordHashMaxLengthValidation;
    }

    public static bool ValidateStatus(
        ExecutionContext executionContext,
        UserStatus? status
    )
    {
        bool statusIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.StatusPropertyName),
            isRequired: UserMetadata.StatusIsRequired,
            value: status
        );

        return statusIsRequiredValidation;
    }

    public static bool ValidateStatusTransition(
        ExecutionContext executionContext,
        UserStatus? from,
        UserStatus? to
    )
    {
        bool fromIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.StatusPropertyName),
            isRequired: true,
            value: from
        );

        bool toIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<User>(propertyName: UserMetadata.StatusPropertyName),
            isRequired: true,
            value: to
        );

        if (!fromIsRequiredValidation || !toIsRequiredValidation)
            return false;

        if (from == to)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<User>(propertyName: UserMetadata.StatusPropertyName)}.SameStatus");
            return false;
        }

        bool isValid = (from!.Value, to!.Value) switch
        {
            (UserStatus.Active, UserStatus.Suspended) => true,
            (UserStatus.Active, UserStatus.Blocked) => true,
            (UserStatus.Suspended, UserStatus.Active) => true,
            (UserStatus.Suspended, UserStatus.Blocked) => true,
            (UserStatus.Blocked, UserStatus.Active) => true,
            _ => false
        };

        if (!isValid)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<User>(propertyName: UserMetadata.StatusPropertyName)}.InvalidTransition");
        }

        return isValid;
    }

    // Set Methods
    private bool SetUsername(
        ExecutionContext executionContext,
        string username
    )
    {
        bool isValid = ValidateUsername(
            executionContext,
            username
        );

        if (!isValid)
            return false;

        Username = username;

        return true;
    }

    private bool SetEmail(
        ExecutionContext executionContext,
        EmailAddress email
    )
    {
        bool isValid = ValidateEmail(
            executionContext,
            email
        );

        if (!isValid)
            // Stryker disable once Boolean : Stryker cannot track coverage through static lambda delegates in RegisterNewInternal
            return false;

        Email = email;

        return true;
    }

    private bool SetPasswordHash(
        ExecutionContext executionContext,
        PasswordHash passwordHash
    )
    {
        bool isValid = ValidatePasswordHash(
            executionContext,
            passwordHash
        );

        if (!isValid)
            return false;

        PasswordHash = passwordHash;

        return true;
    }

    private bool SetStatus(
        ExecutionContext executionContext,
        UserStatus status
    )
    {
        bool isValid = ValidateStatus(
            executionContext,
            status
        );

        if (!isValid)
            // Stryker disable once Boolean : SetStatus always receives UserStatus.Active from RegisterNew - false path unreachable
            return false;

        Status = status;

        return true;
    }

    // Metadata
    public static class UserMetadata
    {
        private static readonly Lock _lockObject = new();

        // Username
        public static readonly string UsernamePropertyName = "Username";
        public static bool UsernameIsRequired { get; private set; } = true;
        public static int UsernameMinLength { get; private set; } = 1;
        public static int UsernameMaxLength { get; private set; } = 255;

        // Email
        public static readonly string EmailPropertyName = "Email";
        public static bool EmailIsRequired { get; private set; } = true;

        // PasswordHash
        public static readonly string PasswordHashPropertyName = "PasswordHash";
        public static bool PasswordHashIsRequired { get; private set; } = true;
        public static int PasswordHashMaxLength { get; private set; } = 128;

        // Status
        public static readonly string StatusPropertyName = "Status";
        public static bool StatusIsRequired { get; private set; } = true;

        public static void ChangeUsernameMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                UsernameIsRequired = isRequired;
                UsernameMinLength = minLength;
                UsernameMaxLength = maxLength;
            }
        }

        public static void ChangeEmailMetadata(
            bool isRequired
        )
        {
            lock (_lockObject)
            {
                EmailIsRequired = isRequired;
            }
        }

        public static void ChangePasswordHashMetadata(
            bool isRequired,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                PasswordHashIsRequired = isRequired;
                PasswordHashMaxLength = maxLength;
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
