using ShopDemo.Auth.Domain.Entities.ExternalLogins;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IExternalAuthService
{
    LoginProvider Provider { get; }

    Task<ExternalUserInfo?> ExchangeCodeForUserInfoAsync(
        string authorizationCode,
        CancellationToken cancellationToken);
}
