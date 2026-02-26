using System.Security.Cryptography;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class KeyAgreementServiceTests : TestBase
{
    private readonly KeyAgreementService _sut;

    public KeyAgreementServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _sut = new KeyAgreementService();
    }

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIKeyAgreementService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IKeyAgreementService>();
    }

    #endregion

    #region NegotiateKey Tests

    [Fact]
    public void NegotiateKey_WithValidPublicKey_ShouldReturnResult()
    {
        // Arrange
        LogArrange("Generating client ECDH P-256 key pair");
        using var clientEcdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var clientPublicKeyBase64 = Convert.ToBase64String(clientEcdh.ExportSubjectPublicKeyInfo());

        // Act
        LogAct("Negotiating key");
        var result = _sut.NegotiateKey(clientPublicKeyBase64);

        // Assert
        LogAssert("Verifying result contains server public key and shared secret");
        result.ServerPublicKeyBase64.ShouldNotBeNullOrWhiteSpace();
        result.SharedSecret.ShouldNotBeEmpty();
        result.SharedSecret.Length.ShouldBe(32); // SHA256 derived key
    }

    [Fact]
    public void NegotiateKey_ShouldProduceMatchingSharedSecret()
    {
        // Arrange
        LogArrange("Generating client ECDH P-256 key pair");
        using var clientEcdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var clientPublicKeyBase64 = Convert.ToBase64String(clientEcdh.ExportSubjectPublicKeyInfo());

        // Act
        LogAct("Negotiating key and computing client-side shared secret");
        var result = _sut.NegotiateKey(clientPublicKeyBase64);

        byte[] serverPublicKeyBytes = Convert.FromBase64String(result.ServerPublicKeyBase64);
        using var serverEcdhForClient = ECDiffieHellman.Create();
        serverEcdhForClient.ImportSubjectPublicKeyInfo(serverPublicKeyBytes, out _);
        byte[] clientSharedSecret = clientEcdh.DeriveKeyFromHash(serverEcdhForClient.PublicKey, HashAlgorithmName.SHA256);

        // Assert
        LogAssert("Verifying both sides derive the same shared secret");
        result.SharedSecret.ShouldBe(clientSharedSecret);
    }

    [Fact]
    public void NegotiateKey_WithInvalidBase64_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing invalid base64 key");

        // Act & Assert
        LogAct("Negotiating with invalid key");
        LogAssert("Verifying FormatException is thrown");
        Should.Throw<FormatException>(() => _sut.NegotiateKey("not-valid-base64!!!"));
    }

    #endregion
}
