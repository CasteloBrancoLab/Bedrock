using ShopDemo.Auth.Domain.Entities.Fingerprints;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class FingerprintService : IFingerprintService
{
    public (Fingerprint Fingerprint, FingerprintHash Hash) Generate()
    {
        var fingerprint = Fingerprint.CreateNew();
        var hash = fingerprint.ComputeHash();

        return (fingerprint, hash);
    }

    public bool Validate(Fingerprint fingerprint, FingerprintHash expectedHash)
    {
        var computedHash = fingerprint.ComputeHash();
        return computedHash == expectedHash;
    }
}
