using Bedrock.BuildingBlocks.Configuration.Handlers.Enums;

namespace Bedrock.BuildingBlocks.Configuration.Handlers;

/// <summary>
/// Classe base abstrata para handlers de configuracao.
/// Handlers estendem o comportamento do IConfiguration no pipeline.
/// </summary>
public abstract class ConfigurationHandlerBase
{
    /// <summary>
    /// Estrategia de carregamento deste handler.
    /// </summary>
    public LoadStrategy LoadStrategy { get; }

    /// <summary>
    /// Cria uma nova instancia do handler com a estrategia especificada.
    /// </summary>
    /// <param name="loadStrategy">Quando o handler carrega/atualiza dados.</param>
    protected ConfigurationHandlerBase(LoadStrategy loadStrategy)
    {
        LoadStrategy = loadStrategy;
    }

    /// <summary>
    /// Construtor padrao com AllTime como estrategia.
    /// </summary>
    protected ConfigurationHandlerBase()
        : this(LoadStrategy.AllTime)
    {
    }

    /// <summary>
    /// Processa um valor no pipeline de Get.
    /// </summary>
    /// <param name="key">Caminho completo da configuracao (ex: "Persistence:PostgreSql:ConnectionString").</param>
    /// <param name="currentValue">Valor atual (do IConfiguration ou handler anterior).</param>
    /// <returns>Valor transformado, substituido ou repassado.</returns>
    public abstract object? HandleGet(string key, object? currentValue);

    /// <summary>
    /// Processa um valor no pipeline de Set.
    /// </summary>
    /// <param name="key">Caminho completo da configuracao.</param>
    /// <param name="currentValue">Valor atual (do caller ou handler anterior).</param>
    /// <returns>Valor transformado.</returns>
    public abstract object? HandleSet(string key, object? currentValue);
}
