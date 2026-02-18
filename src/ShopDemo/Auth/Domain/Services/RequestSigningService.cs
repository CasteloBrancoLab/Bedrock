using System.Security.Cryptography;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class RequestSigningService : IRequestSigningService
{
    public string ComputeSignature(
        byte[] requestBody,
        byte[] sharedSecret)
    {
        byte[] hash = HMACSHA256.HashData(sharedSecret, requestBody);
        return Convert.ToHexStringLower(hash);
    }

    public bool ValidateSignature(
        byte[] requestBody,
        byte[] sharedSecret,
        string providedSignature)
    {
        string computedSignature = ComputeSignature(requestBody, sharedSecret);
        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(computedSignature),
            System.Text.Encoding.UTF8.GetBytes(providedSignature));
    }
}
