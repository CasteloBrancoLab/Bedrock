namespace ShopDemo.Auth.Domain.Entities.Users;

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
