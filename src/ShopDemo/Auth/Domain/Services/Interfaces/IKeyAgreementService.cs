namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IKeyAgreementService
{
    KeyAgreementResult NegotiateKey(string clientPublicKeyBase64);
}
