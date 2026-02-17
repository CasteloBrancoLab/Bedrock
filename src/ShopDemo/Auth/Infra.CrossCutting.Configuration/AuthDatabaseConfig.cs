namespace ShopDemo.Auth.Infra.CrossCutting.Configuration;

/// <summary>
/// Configuracao de conexao com o banco de dados do bounded context Auth.
/// Mapeada para a secao "Persistence:PostgreSql" do IConfiguration.
/// </summary>
public sealed class AuthDatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;

    public int CommandTimeout { get; set; } = 30;
}
