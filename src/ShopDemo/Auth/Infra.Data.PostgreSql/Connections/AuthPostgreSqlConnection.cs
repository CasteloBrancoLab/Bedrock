using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;
using ShopDemo.Auth.Infra.CrossCutting.Configuration;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections;

public sealed class AuthPostgreSqlConnection
    : PostgreSqlConnectionBase,
    IAuthPostgreSqlConnection
{
    // Fields
    private readonly AuthConfigurationManager _configurationManager;

    // Constructors
    // Stryker disable all : Construtor armazena dependencia usada em ConfigureInternal (excluido de cobertura)
    public AuthPostgreSqlConnection(AuthConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }
    // Stryker restore all

    // Protected Methods
    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    protected override void ConfigureInternal(PostgreSqlConnectionOptions options)
    {
        AuthDatabaseConfig config = _configurationManager.Get<AuthDatabaseConfig>();

        ArgumentException.ThrowIfNullOrWhiteSpace(config.ConnectionString);

        options.WithConnectionString(config.ConnectionString);
    }
    // Stryker restore all
}
