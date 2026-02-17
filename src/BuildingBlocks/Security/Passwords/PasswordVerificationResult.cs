using System.Diagnostics.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Security.Passwords;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct PasswordVerificationResult(
    bool IsValid,
    bool NeedsRehash);
