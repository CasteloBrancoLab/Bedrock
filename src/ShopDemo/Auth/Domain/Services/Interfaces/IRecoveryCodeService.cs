namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IRecoveryCodeService
{
    IReadOnlyList<string> GenerateCodes(int count = 10);

    string HashCode(string code);

    bool ValidateCode(string code, string codeHash);
}
