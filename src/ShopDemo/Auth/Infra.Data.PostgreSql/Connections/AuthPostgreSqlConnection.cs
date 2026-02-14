using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;
using Microsoft.Extensions.Configuration;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Connections;

public sealed class AuthPostgreSqlConnection
    : PostgreSqlConnectionBase,
    IAuthPostgreSqlConnection
{
    // Constants
    private const string ConnectionStringConfigKey = "ConnectionStrings:AuthPostgreSql";

    // Fields
    private readonly IConfiguration _configuration;

    // Constructors
    // Stryker disable all : Construtor armazena dependencia usada em ConfigureInternal (excluido de cobertura)
    public AuthPostgreSqlConnection(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    // Stryker restore all

    // Protected Methods
    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    protected override void ConfigureInternal(PostgreSqlConnectionOptions options)
    {
        string? connectionString = _configuration[ConnectionStringConfigKey];

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        options.WithConnectionString(connectionString);
    }
    // Stryker restore all
}
