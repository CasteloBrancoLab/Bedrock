using Bedrock.BuildingBlocks.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ShopDemo.Auth.Infra.CrossCutting.Configuration;

/// <summary>
/// Gerenciador de configuracao do bounded context Auth.
/// Centraliza mapeamentos de secoes e handlers especificos do contexto.
/// </summary>
public sealed class AuthConfigurationManager : ConfigurationManagerBase
{
    public AuthConfigurationManager(IConfiguration configuration, ILogger<AuthConfigurationManager> logger)
        : base(configuration, logger)
    {
    }

    protected override void ConfigureInternal(ConfigurationOptions options)
    {
        options.MapSection<AuthDatabaseConfig>("Persistence:PostgreSql");
    }
}
