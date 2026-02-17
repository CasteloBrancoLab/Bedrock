using Bedrock.BuildingBlocks.Configuration.Handlers;
using Bedrock.BuildingBlocks.Configuration.Pipeline;

namespace Bedrock.BuildingBlocks.Configuration;

/// <summary>
/// Opcoes de configuracao para o pipeline de handlers.
/// Usado no ConfigureInternal() e no registro DI.
/// </summary>
public sealed class ConfigurationOptions
{
    private readonly Dictionary<Type, string> _sectionMappings = new();
    private readonly List<HandlerRegistration> _handlerRegistrations = [];

    /// <summary>
    /// Mapeia uma classe de configuracao para uma secao do IConfiguration.
    /// </summary>
    /// <typeparam name="TSection">Tipo da classe de configuracao (POCO).</typeparam>
    /// <param name="sectionPath">Caminho da secao (ex: "Persistence:PostgreSql").</param>
    /// <returns>Esta instancia para encadeamento.</returns>
    /// <exception cref="ArgumentException">Se sectionPath for nulo ou vazio.</exception>
    public ConfigurationOptions MapSection<TSection>(string sectionPath)
        where TSection : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionPath);
        _sectionMappings[typeof(TSection)] = sectionPath;
        return this;
    }

    /// <summary>
    /// Adiciona um handler ao pipeline via fluent API.
    /// </summary>
    /// <typeparam name="THandler">Tipo do handler (deve estender ConfigurationHandlerBase).</typeparam>
    /// <returns>Builder para configurar posicao, escopo e estrategia.</returns>
    public ConfigurationHandlerBuilder<THandler> AddHandler<THandler>()
        where THandler : ConfigurationHandlerBase
    {
        var registration = new HandlerRegistration(typeof(THandler));
        _handlerRegistrations.Add(registration);
        return new ConfigurationHandlerBuilder<THandler>(registration, _sectionMappings);
    }

    /// <summary>
    /// Retorna os mapeamentos de secao registrados (uso interno).
    /// </summary>
    internal IReadOnlyDictionary<Type, string> GetSectionMappings() => _sectionMappings;

    /// <summary>
    /// Constroi os pipelines de Get e Set a partir dos registros de handlers.
    /// Valida que nao ha posicoes duplicadas por pipeline (RF-014).
    /// </summary>
    /// <returns>Tupla com pipeline de Get e pipeline de Set.</returns>
    /// <exception cref="InvalidOperationException">Se houver posicoes duplicadas em um mesmo pipeline.</exception>
    internal (ConfigurationPipeline getPipeline, ConfigurationPipeline setPipeline) BuildPipelines()
    {
        var getEntries = new List<PipelineEntry>();
        var setEntries = new List<PipelineEntry>();

        foreach (var reg in _handlerRegistrations)
        {
            var handler = reg.CreateHandler();
            var entry = new PipelineEntry(handler, reg.Scope, reg.Position);

            if (reg.IncludeInGet)
            {
                getEntries.Add(entry);
            }

            if (reg.IncludeInSet)
            {
                setEntries.Add(entry);
            }
        }

        // Stryker disable once all : Validacao de posicoes duplicadas no Get — testada por excecao
        ValidateNoDuplicatePositions(getEntries, "Get");
        // Stryker disable once all : Validacao de posicoes duplicadas no Set — testada por excecao
        ValidateNoDuplicatePositions(setEntries, "Set");

        return (new ConfigurationPipeline(getEntries), new ConfigurationPipeline(setEntries));
    }

    private static void ValidateNoDuplicatePositions(List<PipelineEntry> entries, string pipelineName)
    {
        var positions = new HashSet<int>();
        foreach (var entry in entries)
        {
            if (!positions.Add(entry.Position))
            {
                // Stryker disable all : Mensagem de erro com contexto — testada por conteudo
                throw new InvalidOperationException(
                    $"Posicao duplicada {entry.Position} no pipeline de {pipelineName}. " +
                    $"Cada handler deve ter uma posicao unica dentro do mesmo pipeline.");
                // Stryker restore all
            }
        }
    }
}

/// <summary>
/// Registro interno de um handler pendente de construcao.
/// </summary>
internal sealed class HandlerRegistration
{
    public Type HandlerType { get; }
    public int Position { get; set; }
    public HandlerScope Scope { get; set; } = HandlerScope.Global();
    public bool IncludeInGet { get; set; } = true;
    public bool IncludeInSet { get; set; } = true;

    public HandlerRegistration(Type handlerType)
    {
        HandlerType = handlerType;
    }

    public ConfigurationHandlerBase CreateHandler()
    {
        return (ConfigurationHandlerBase)Activator.CreateInstance(HandlerType)!;
    }
}
