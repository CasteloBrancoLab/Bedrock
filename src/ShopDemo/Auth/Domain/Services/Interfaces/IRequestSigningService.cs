namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IRequestSigningService
{
    string ComputeSignature(
        byte[] requestBody,
        byte[] sharedSecret);

    bool ValidateSignature(
        byte[] requestBody,
        byte[] sharedSecret,
        string providedSignature);
}
