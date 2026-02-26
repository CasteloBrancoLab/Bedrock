using System.Security.Cryptography;
using System.Text;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class PayloadEncryptionServiceTests : TestBase
{
    private readonly PayloadEncryptionService _sut;

    public PayloadEncryptionServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _sut = new PayloadEncryptionService();
    }

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIPayloadEncryptionService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IPayloadEncryptionService>();
    }

    #endregion

    #region Encrypt Tests

    [Fact]
    public void Encrypt_ShouldReturnEncryptedPayload()
    {
        // Arrange
        LogArrange("Preparing 256-bit key and plaintext");
        byte[] sharedSecret = RandomNumberGenerator.GetBytes(32);
        byte[] plaintext = Encoding.UTF8.GetBytes("Hello, World!");

        // Act
        LogAct("Encrypting plaintext");
        var payload = _sut.Encrypt(sharedSecret, plaintext);

        // Assert
        LogAssert("Verifying encrypted payload has expected structure");
        payload.Ciphertext.Length.ShouldBe(plaintext.Length);
        payload.Nonce.Length.ShouldBe(12);
        payload.Tag.Length.ShouldBe(16);
    }

    [Fact]
    public void Encrypt_ShouldProduceDifferentCiphertextsForSamePlaintext()
    {
        // Arrange
        LogArrange("Preparing key and plaintext");
        byte[] sharedSecret = RandomNumberGenerator.GetBytes(32);
        byte[] plaintext = Encoding.UTF8.GetBytes("Same plaintext");

        // Act
        LogAct("Encrypting same plaintext twice");
        var payload1 = _sut.Encrypt(sharedSecret, plaintext);
        var payload2 = _sut.Encrypt(sharedSecret, plaintext);

        // Assert
        LogAssert("Verifying different nonces produce different ciphertexts");
        payload1.Nonce.ShouldNotBe(payload2.Nonce);
    }

    #endregion

    #region Decrypt Tests

    [Fact]
    public void Decrypt_WithCorrectKey_ShouldReturnOriginalPlaintext()
    {
        // Arrange
        LogArrange("Encrypting plaintext");
        byte[] sharedSecret = RandomNumberGenerator.GetBytes(32);
        byte[] plaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var payload = _sut.Encrypt(sharedSecret, plaintext);

        // Act
        LogAct("Decrypting with correct key");
        var decrypted = _sut.Decrypt(sharedSecret, payload);

        // Assert
        LogAssert("Verifying decrypted text matches original");
        decrypted.ShouldNotBeNull();
        decrypted.ShouldBe(plaintext);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Encrypting plaintext with one key");
        byte[] correctKey = RandomNumberGenerator.GetBytes(32);
        byte[] wrongKey = RandomNumberGenerator.GetBytes(32);
        byte[] plaintext = Encoding.UTF8.GetBytes("Secret data");
        var payload = _sut.Encrypt(correctKey, plaintext);

        // Act
        LogAct("Decrypting with wrong key");
        var result = _sut.Decrypt(wrongKey, payload);

        // Assert
        LogAssert("Verifying null returned for wrong key");
        result.ShouldBeNull();
    }

    [Fact]
    public void Decrypt_WithTamperedCiphertext_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Encrypting and tampering with ciphertext");
        byte[] sharedSecret = RandomNumberGenerator.GetBytes(32);
        byte[] plaintext = Encoding.UTF8.GetBytes("Original data");
        var payload = _sut.Encrypt(sharedSecret, plaintext);
        payload.Ciphertext[0] ^= 0xFF;

        // Act
        LogAct("Decrypting tampered ciphertext");
        var result = _sut.Decrypt(sharedSecret, payload);

        // Assert
        LogAssert("Verifying null returned for tampered data");
        result.ShouldBeNull();
    }

    #endregion
}
