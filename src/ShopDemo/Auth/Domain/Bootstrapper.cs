using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain;

/// <summary>
/// Registra os servicos da camada Domain do Auth no IoC.
/// Mapeia interfaces de Domain Services para as implementacoes concretas.
/// </summary>
public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Domain Services (scoped — dependem de repositorios scoped)
        services.TryAddScoped<IAuthenticationService, AuthenticationService>();
        services.TryAddScoped<IApiTokenExpirationManager, ApiTokenExpirationManager>();
        services.TryAddScoped<IApiTokenPermissionValidator, ApiTokenPermissionValidator>();
        services.TryAddScoped<IBruteForceProtectionService, BruteForceProtectionService>();
        services.TryAddScoped<ICascadeRevocationService, CascadeRevocationService>();
        services.TryAddScoped<IClaimResolver, ClaimResolver>();
        services.TryAddScoped<IClientCredentialsService, ClientCredentialsService>();
        services.TryAddScoped<IConsentManager, ConsentManager>();
        services.TryAddScoped<IDenyListService, DenyListService>();
        services.TryAddScoped<IFingerprintService, FingerprintService>();
        services.TryAddScoped<IImpersonationService, ImpersonationService>();
        services.TryAddScoped<IKeyAgreementService, KeyAgreementService>();
        services.TryAddScoped<IKeyChainManager, KeyChainManager>();
        services.TryAddScoped<IPasswordPolicyService, PasswordPolicyService>();
        services.TryAddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
        services.TryAddScoped<IPayloadEncryptionService, PayloadEncryptionService>();
        services.TryAddScoped<IRecoveryCodeService, RecoveryCodeService>();
        services.TryAddScoped<IRequestSigningService, RequestSigningService>();
        services.TryAddScoped<ISessionManager, SessionManager>();
        services.TryAddScoped<ISigningKeyManager, SigningKeyManager>();
        services.TryAddScoped<ITenantResolver, TenantResolver>();
        services.TryAddScoped<ITokenExchangeService, TokenExchangeService>();
        services.TryAddScoped<ITotpService, TotpService>();

        return services;
    }
}
