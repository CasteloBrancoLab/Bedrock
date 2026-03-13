using ShopDemo.Auth.Domain.Services.Outputs;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IKeyAgreementService
{
    KeyAgreementOutput NegotiateKey(string clientPublicKeyBase64);
}
