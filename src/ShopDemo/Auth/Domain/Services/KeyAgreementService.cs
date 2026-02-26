using System.Security.Cryptography;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class KeyAgreementService : IKeyAgreementService
{
    public KeyAgreementResult NegotiateKey(string clientPublicKeyBase64)
    {
        byte[] clientPublicKeyBytes = Convert.FromBase64String(clientPublicKeyBase64);

        using ECDiffieHellman serverEcdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

        using ECDiffieHellman clientEcdh = ECDiffieHellman.Create();
        clientEcdh.ImportSubjectPublicKeyInfo(clientPublicKeyBytes, out _);

        byte[] sharedSecret = serverEcdh.DeriveKeyFromHash(
            clientEcdh.PublicKey,
            HashAlgorithmName.SHA256);

        byte[] serverPublicKeyBytes = serverEcdh.ExportSubjectPublicKeyInfo();
        string serverPublicKeyBase64 = Convert.ToBase64String(serverPublicKeyBytes);

        return new KeyAgreementResult(serverPublicKeyBase64, sharedSecret);
    }
}
