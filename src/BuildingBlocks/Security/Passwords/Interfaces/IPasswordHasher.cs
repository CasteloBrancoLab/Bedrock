namespace Bedrock.BuildingBlocks.Security.Passwords.Interfaces;

public interface IPasswordHasher
{
    PasswordHashResult HashPassword(
        ExecutionContext executionContext,
        string password);

    PasswordVerificationResult VerifyPassword(
        ExecutionContext executionContext,
        string password,
        byte[] storedHash);

    bool NeedsRehash(byte[] storedHash);
}
