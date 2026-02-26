using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Validators.Interfaces;

public interface IDPoPProofValidator
{
    Task<bool> ValidateProofAsync(
        ExecutionContext executionContext,
        Id userId,
        string proofJwt,
        string expectedHttpMethod,
        string expectedHttpUri,
        CancellationToken cancellationToken);
}
