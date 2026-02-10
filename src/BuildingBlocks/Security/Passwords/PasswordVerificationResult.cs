namespace Bedrock.BuildingBlocks.Security.Passwords;

public readonly record struct PasswordVerificationResult(
    bool IsValid,
    bool NeedsRehash);
