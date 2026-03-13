using Bedrock.BuildingBlocks.Serialization.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;
using ShopDemo.Auth.Infra.Data.Outbox;
using ShopDemo.Auth.Infra.Data.Repositories;

namespace ShopDemo.Auth.Infra.Data;

/// <summary>
/// Registra os servicos da camada Infra.Data do Auth no IoC.
/// Mapeia as interfaces de repositorio do Domain para as implementacoes facade.
/// </summary>
public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Repositories (scoped — facade que delega para PostgreSql)
        services.TryAddScoped<IUserRepository, UserRepository>();
        services.TryAddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.TryAddScoped<IRoleRepository, RoleRepository>();
        services.TryAddScoped<IClaimRepository, ClaimRepository>();
        services.TryAddScoped<IRoleClaimRepository, RoleClaimRepository>();
        services.TryAddScoped<IUserRoleRepository, UserRoleRepository>();
        services.TryAddScoped<IClaimDependencyRepository, ClaimDependencyRepository>();
        services.TryAddScoped<IRoleHierarchyRepository, RoleHierarchyRepository>();
        services.TryAddScoped<IDPoPKeyRepository, DPoPKeyRepository>();
        services.TryAddScoped<IDenyListRepository, DenyListEntryRepository>();
        services.TryAddScoped<IKeyChainRepository, KeyChainRepository>();
        services.TryAddScoped<ISigningKeyRepository, SigningKeyRepository>();
        services.TryAddScoped<IMfaSetupRepository, MfaSetupRepository>();
        services.TryAddScoped<IRecoveryCodeRepository, RecoveryCodeRepository>();
        services.TryAddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.TryAddScoped<IExternalLoginRepository, ExternalLoginRepository>();
        services.TryAddScoped<ITenantRepository, TenantRepository>();
        services.TryAddScoped<IServiceClientRepository, ServiceClientRepository>();
        services.TryAddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.TryAddScoped<IServiceClientScopeRepository, ServiceClientScopeRepository>();
        services.TryAddScoped<IServiceClientClaimRepository, ServiceClientClaimRepository>();
        services.TryAddScoped<IImpersonationSessionRepository, ImpersonationSessionRepository>();
        services.TryAddScoped<ISessionRepository, SessionRepository>();
        services.TryAddScoped<IConsentTermRepository, ConsentTermRepository>();
        services.TryAddScoped<IUserConsentRepository, UserConsentRepository>();
        services.TryAddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        services.TryAddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();
        services.TryAddScoped<IIdempotencyRecordRepository, IdempotencyRecordRepository>();
        services.TryAddScoped<ITokenExchangeRepository, TokenExchangeRepository>();

        // Outbox — facades (scoped — delegam para PostgreSql)
        services.TryAddScoped<IAuthOutboxRepository, AuthOutboxRepository>();
        services.TryAddScoped<IAuthOutboxWriter, AuthOutboxWriter>();

        // Outbox — serializacao (singleton — stateless apos inicializacao)
        services.TryAddSingleton<IStringSerializer, AuthOutboxJsonSerializer>();

        return services;
    }
}
