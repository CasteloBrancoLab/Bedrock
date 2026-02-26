namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IPayloadEncryptionService
{
    EncryptedPayload Encrypt(ReadOnlyMemory<byte> sharedSecret, ReadOnlyMemory<byte> plaintext);

    byte[]? Decrypt(ReadOnlyMemory<byte> sharedSecret, EncryptedPayload payload);
}
