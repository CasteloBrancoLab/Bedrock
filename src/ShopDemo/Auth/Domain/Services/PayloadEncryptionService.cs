using System.Security.Cryptography;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class PayloadEncryptionService : IPayloadEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public EncryptedPayload Encrypt(ReadOnlyMemory<byte> sharedSecret, ReadOnlyMemory<byte> plaintext)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[TagSize];

        using AesGcm aes = new(sharedSecret.Span, TagSize);
        aes.Encrypt(nonce, plaintext.Span, ciphertext, tag);

        return new EncryptedPayload(ciphertext, nonce, tag);
    }

    public byte[]? Decrypt(ReadOnlyMemory<byte> sharedSecret, EncryptedPayload payload)
    {
        try
        {
            byte[] plaintext = new byte[payload.Ciphertext.Length];

            using AesGcm aes = new(sharedSecret.Span, TagSize);
            aes.Decrypt(payload.Nonce, payload.Ciphertext, payload.Tag, plaintext);

            return plaintext;
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
