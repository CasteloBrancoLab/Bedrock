namespace Bedrock.BuildingBlocks.Security.Passwords;

public static class PasswordPolicyMetadata
{
    private static readonly Lock _lockObject = new();

    public static int MinLength { get; private set; } = 12;
    public static int MaxLength { get; private set; } = 128;
    public static bool AllowSpaces { get; private set; } = true;
    public static bool RequireUppercase { get; private set; } = true;
    public static bool RequireLowercase { get; private set; } = true;
    public static bool RequireDigit { get; private set; } = true;
    public static bool RequireSpecialCharacter { get; private set; } = true;
    public static int MinUniqueCharacters { get; private set; } = 4;

    public static void ChangeMetadata(
        int minLength,
        int maxLength,
        bool allowSpaces,
        bool requireUppercase,
        bool requireLowercase,
        bool requireDigit,
        bool requireSpecialCharacter,
        int minUniqueCharacters
    )
    {
        lock (_lockObject)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            AllowSpaces = allowSpaces;
            RequireUppercase = requireUppercase;
            RequireLowercase = requireLowercase;
            RequireDigit = requireDigit;
            RequireSpecialCharacter = requireSpecialCharacter;
            MinUniqueCharacters = minUniqueCharacters;
        }
    }
}
