using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Validators.Interfaces;

namespace ShopDemo.Auth.Domain.Validators;

public sealed class DPoPProofValidator : IDPoPProofValidator
{
    private const string InvalidProofMessageCode = "DPoPProofValidator.InvalidProof";
    private const string UnregisteredKeyMessageCode = "DPoPProofValidator.UnregisteredKey";
    private const string InvalidHttpMethodMessageCode = "DPoPProofValidator.InvalidHttpMethod";
    private const string InvalidHttpUriMessageCode = "DPoPProofValidator.InvalidHttpUri";

    private readonly IDPoPKeyRepository _dPoPKeyRepository;
    private readonly IDPoPProofVerifier _proofVerifier;

    public DPoPProofValidator(
        IDPoPKeyRepository dPoPKeyRepository,
        IDPoPProofVerifier proofVerifier
    )
    {
        _dPoPKeyRepository = dPoPKeyRepository ?? throw new ArgumentNullException(nameof(dPoPKeyRepository));
        _proofVerifier = proofVerifier ?? throw new ArgumentNullException(nameof(proofVerifier));
    }

    public async Task<bool> ValidateProofAsync(
        ExecutionContext executionContext,
        Id userId,
        string proofJwt,
        string expectedHttpMethod,
        string expectedHttpUri,
        CancellationToken cancellationToken)
    {
        DPoPProofInfo? proofInfo = _proofVerifier.ParseAndVerifyProof(proofJwt);

        if (proofInfo is null)
        {
            executionContext.AddErrorMessage(code: InvalidProofMessageCode);
            return false;
        }

        DPoPKey? key = await _dPoPKeyRepository.GetActiveByUserIdAndThumbprintAsync(
            executionContext,
            userId,
            proofInfo.Value.JwkThumbprint,
            cancellationToken);

        if (key is null)
        {
            executionContext.AddErrorMessage(code: UnregisteredKeyMessageCode);
            return false;
        }

        if (!string.Equals(proofInfo.Value.HttpMethod, expectedHttpMethod, StringComparison.OrdinalIgnoreCase))
        {
            executionContext.AddErrorMessage(code: InvalidHttpMethodMessageCode);
            return false;
        }

        if (!string.Equals(proofInfo.Value.HttpUri, expectedHttpUri, StringComparison.Ordinal))
        {
            executionContext.AddErrorMessage(code: InvalidHttpUriMessageCode);
            return false;
        }

        return true;
    }
}
