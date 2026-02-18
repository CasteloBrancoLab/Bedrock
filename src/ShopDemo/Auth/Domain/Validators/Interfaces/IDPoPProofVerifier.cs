using ShopDemo.Auth.Domain.Entities.DPoPKeys;

namespace ShopDemo.Auth.Domain.Validators.Interfaces;

public interface IDPoPProofVerifier
{
    DPoPProofInfo? ParseAndVerifyProof(string proofJwt);
}
