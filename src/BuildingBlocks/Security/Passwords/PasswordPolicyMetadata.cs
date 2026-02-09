namespace Bedrock.BuildingBlocks.Security.Passwords;

public static class PasswordPolicyMetadata
{
    private static readonly Lock _lockObject = new();

    public static int MinLength { get; private set; } = 12;
    public static int MaxLength { get; private set; } = 128;
    public static bool AllowSpaces { get; private set; } = true;

    public static void ChangeMetadata(
        int minLength,
        int maxLength,
        bool allowSpaces
    )
    {
        lock (_lockObject)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            AllowSpaces = allowSpaces;
        }
    }
}
