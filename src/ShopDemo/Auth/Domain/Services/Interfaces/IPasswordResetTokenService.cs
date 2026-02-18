namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IPasswordResetTokenService
{
    string GenerateToken();

    string HashToken(string token);

    bool ValidateToken(string token, string tokenHash);
}
