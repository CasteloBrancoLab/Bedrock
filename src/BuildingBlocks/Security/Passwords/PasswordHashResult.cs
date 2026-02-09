namespace Bedrock.BuildingBlocks.Security.Passwords;

public readonly record struct PasswordHashResult(
    byte[] Hash,
    int PepperVersion);
