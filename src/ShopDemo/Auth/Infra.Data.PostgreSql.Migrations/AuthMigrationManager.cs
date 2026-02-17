using System.Reflection;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Infra.CrossCutting.Configuration;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Migrations;

/// <summary>
/// Gerenciador de migrations do bounded context Auth.
/// Le a connection string via AuthConfigurationManager.
/// </summary>
public sealed class AuthMigrationManager : MigrationManagerBase
{
    private readonly AuthConfigurationManager _configurationManager;

    protected override string ConnectionString =>
        _configurationManager.Get<AuthDatabaseConfig>().ConnectionString;

    protected override string TargetSchema => "public";

    protected override Assembly MigrationAssembly => typeof(AuthMigrationManager).Assembly;

    public AuthMigrationManager(
        ILogger<AuthMigrationManager> logger,
        AuthConfigurationManager configurationManager)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);
        _configurationManager = configurationManager;
    }
}
