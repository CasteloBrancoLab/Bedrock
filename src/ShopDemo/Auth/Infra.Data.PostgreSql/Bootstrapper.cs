using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql;

/// <summary>
/// Registra os servicos da camada Infra.Data.PostgreSql do Auth no IoC.
/// </summary>
public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Mappers (singleton — stateless, caches internamente)
        services.TryAddSingleton<IDataModelMapper<UserDataModel>, UserDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<RefreshTokenDataModel>, RefreshTokenDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<RoleDataModel>, RoleDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ClaimDataModel>, ClaimDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<RoleClaimDataModel>, RoleClaimDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<UserRoleDataModel>, UserRoleDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ClaimDependencyDataModel>, ClaimDependencyDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<RoleHierarchyDataModel>, RoleHierarchyDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<DPoPKeyDataModel>, DPoPKeyDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<DenyListEntryDataModel>, DenyListEntryDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<KeyChainDataModel>, KeyChainDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<SigningKeyDataModel>, SigningKeyDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<MfaSetupDataModel>, MfaSetupDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<RecoveryCodeDataModel>, RecoveryCodeDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<PasswordResetTokenDataModel>, PasswordResetTokenDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ExternalLoginDataModel>, ExternalLoginDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<TenantDataModel>, TenantDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ServiceClientDataModel>, ServiceClientDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ApiKeyDataModel>, ApiKeyDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ServiceClientScopeDataModel>, ServiceClientScopeDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ServiceClientClaimDataModel>, ServiceClientClaimDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ImpersonationSessionDataModel>, ImpersonationSessionDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<SessionDataModel>, SessionDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<ConsentTermDataModel>, ConsentTermDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<UserConsentDataModel>, UserConsentDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<LoginAttemptDataModel>, LoginAttemptDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<PasswordHistoryDataModel>, PasswordHistoryDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<IdempotencyRecordDataModel>, IdempotencyRecordDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<TokenExchangeDataModel>, TokenExchangeDataModelMapper>();

        // Connection (scoped — mantém NpgsqlConnection por request)
        services.TryAddScoped<IAuthPostgreSqlConnection, AuthPostgreSqlConnection>();

        // Unit of Work (scoped — mantém transacao por request)
        services.TryAddScoped<IAuthPostgreSqlUnitOfWork, AuthPostgreSqlUnitOfWork>();

        // DataModel Repositories (scoped — dependem do UoW)
        services.TryAddScoped<IUserDataModelRepository, UserDataModelRepository>();
        services.TryAddScoped<IRefreshTokenDataModelRepository, RefreshTokenDataModelRepository>();
        services.TryAddScoped<IRoleDataModelRepository, RoleDataModelRepository>();
        services.TryAddScoped<IClaimDataModelRepository, ClaimDataModelRepository>();
        services.TryAddScoped<IRoleClaimDataModelRepository, RoleClaimDataModelRepository>();
        services.TryAddScoped<IUserRoleDataModelRepository, UserRoleDataModelRepository>();
        services.TryAddScoped<IClaimDependencyDataModelRepository, ClaimDependencyDataModelRepository>();
        services.TryAddScoped<IRoleHierarchyDataModelRepository, RoleHierarchyDataModelRepository>();
        services.TryAddScoped<IDPoPKeyDataModelRepository, DPoPKeyDataModelRepository>();
        services.TryAddScoped<IDenyListEntryDataModelRepository, DenyListEntryDataModelRepository>();
        services.TryAddScoped<IKeyChainDataModelRepository, KeyChainDataModelRepository>();
        services.TryAddScoped<ISigningKeyDataModelRepository, SigningKeyDataModelRepository>();
        services.TryAddScoped<IMfaSetupDataModelRepository, MfaSetupDataModelRepository>();
        services.TryAddScoped<IRecoveryCodeDataModelRepository, RecoveryCodeDataModelRepository>();
        services.TryAddScoped<IPasswordResetTokenDataModelRepository, PasswordResetTokenDataModelRepository>();
        services.TryAddScoped<IExternalLoginDataModelRepository, ExternalLoginDataModelRepository>();
        services.TryAddScoped<ITenantDataModelRepository, TenantDataModelRepository>();
        services.TryAddScoped<IServiceClientDataModelRepository, ServiceClientDataModelRepository>();
        services.TryAddScoped<IApiKeyDataModelRepository, ApiKeyDataModelRepository>();
        services.TryAddScoped<IServiceClientScopeDataModelRepository, ServiceClientScopeDataModelRepository>();
        services.TryAddScoped<IServiceClientClaimDataModelRepository, ServiceClientClaimDataModelRepository>();
        services.TryAddScoped<IImpersonationSessionDataModelRepository, ImpersonationSessionDataModelRepository>();
        services.TryAddScoped<ISessionDataModelRepository, SessionDataModelRepository>();
        services.TryAddScoped<IConsentTermDataModelRepository, ConsentTermDataModelRepository>();
        services.TryAddScoped<IUserConsentDataModelRepository, UserConsentDataModelRepository>();
        services.TryAddScoped<ILoginAttemptDataModelRepository, LoginAttemptDataModelRepository>();
        services.TryAddScoped<IPasswordHistoryDataModelRepository, PasswordHistoryDataModelRepository>();
        services.TryAddScoped<IIdempotencyRecordDataModelRepository, IdempotencyRecordDataModelRepository>();
        services.TryAddScoped<ITokenExchangeDataModelRepository, TokenExchangeDataModelRepository>();

        // PostgreSql Repositories (scoped — dependem do DataModel Repository)
        services.TryAddScoped<IUserPostgreSqlRepository, UserPostgreSqlRepository>();
        services.TryAddScoped<IRefreshTokenPostgreSqlRepository, RefreshTokenPostgreSqlRepository>();
        services.TryAddScoped<IRolePostgreSqlRepository, RolePostgreSqlRepository>();
        services.TryAddScoped<IClaimPostgreSqlRepository, ClaimPostgreSqlRepository>();
        services.TryAddScoped<IRoleClaimPostgreSqlRepository, RoleClaimPostgreSqlRepository>();
        services.TryAddScoped<IUserRolePostgreSqlRepository, UserRolePostgreSqlRepository>();
        services.TryAddScoped<IClaimDependencyPostgreSqlRepository, ClaimDependencyPostgreSqlRepository>();
        services.TryAddScoped<IRoleHierarchyPostgreSqlRepository, RoleHierarchyPostgreSqlRepository>();
        services.TryAddScoped<IDPoPKeyPostgreSqlRepository, DPoPKeyPostgreSqlRepository>();
        services.TryAddScoped<IDenyListEntryPostgreSqlRepository, DenyListEntryPostgreSqlRepository>();
        services.TryAddScoped<IKeyChainPostgreSqlRepository, KeyChainPostgreSqlRepository>();
        services.TryAddScoped<ISigningKeyPostgreSqlRepository, SigningKeyPostgreSqlRepository>();
        services.TryAddScoped<IMfaSetupPostgreSqlRepository, MfaSetupPostgreSqlRepository>();
        services.TryAddScoped<IRecoveryCodePostgreSqlRepository, RecoveryCodePostgreSqlRepository>();
        services.TryAddScoped<IPasswordResetTokenPostgreSqlRepository, PasswordResetTokenPostgreSqlRepository>();
        services.TryAddScoped<IExternalLoginPostgreSqlRepository, ExternalLoginPostgreSqlRepository>();
        services.TryAddScoped<ITenantPostgreSqlRepository, TenantPostgreSqlRepository>();
        services.TryAddScoped<IServiceClientPostgreSqlRepository, ServiceClientPostgreSqlRepository>();
        services.TryAddScoped<IApiKeyPostgreSqlRepository, ApiKeyPostgreSqlRepository>();
        services.TryAddScoped<IServiceClientScopePostgreSqlRepository, ServiceClientScopePostgreSqlRepository>();
        services.TryAddScoped<IServiceClientClaimPostgreSqlRepository, ServiceClientClaimPostgreSqlRepository>();
        services.TryAddScoped<IImpersonationSessionPostgreSqlRepository, ImpersonationSessionPostgreSqlRepository>();
        services.TryAddScoped<ISessionPostgreSqlRepository, SessionPostgreSqlRepository>();
        services.TryAddScoped<IConsentTermPostgreSqlRepository, ConsentTermPostgreSqlRepository>();
        services.TryAddScoped<IUserConsentPostgreSqlRepository, UserConsentPostgreSqlRepository>();
        services.TryAddScoped<ILoginAttemptPostgreSqlRepository, LoginAttemptPostgreSqlRepository>();
        services.TryAddScoped<IPasswordHistoryPostgreSqlRepository, PasswordHistoryPostgreSqlRepository>();
        services.TryAddScoped<IIdempotencyRecordPostgreSqlRepository, IdempotencyRecordPostgreSqlRepository>();
        services.TryAddScoped<ITokenExchangePostgreSqlRepository, TokenExchangePostgreSqlRepository>();

        return services;
    }
}
