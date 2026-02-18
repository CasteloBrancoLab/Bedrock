namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IPasswordBreachChecker
{
    Task<bool> IsBreachedAsync(
        string password,
        CancellationToken cancellationToken);
}
