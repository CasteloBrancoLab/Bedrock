using ShopDemo.Auth.Domain.Entities.Fingerprints;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IFingerprintService
{
    (Fingerprint Fingerprint, FingerprintHash Hash) Generate();

    bool Validate(Fingerprint fingerprint, FingerprintHash expectedHash);
}
