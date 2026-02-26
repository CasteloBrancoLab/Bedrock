namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface ITotpService
{
    byte[] GenerateSecret();

    string GenerateQrCodeUri(
        byte[] secret,
        string issuer,
        string accountName);

    bool ValidateCode(
        byte[] secret,
        string code,
        DateTimeOffset timestamp);
}
