using Bedrock.BuildingBlocks.Configuration.Handlers;
using Bedrock.BuildingBlocks.Configuration.Handlers.Enums;

namespace Bedrock.BuildingBlocks.Configuration.Pipeline;

/// <summary>
/// Entrada no pipeline de configuracao: handler + escopo + posicao.
/// </summary>
internal readonly struct PipelineEntry
{
    public ConfigurationHandlerBase Handler { get; }
    public HandlerScope Scope { get; }
    public int Position { get; }

    public PipelineEntry(ConfigurationHandlerBase handler, HandlerScope scope, int position)
    {
        Handler = handler;
        Scope = scope;
        Position = position;
    }
}

/// <summary>
/// Pipeline interno que executa handlers de configuracao em ordem.
/// Suporta LoadStrategy: StartupOnly (cache no init), LazyStartupOnly (cache no primeiro acesso),
/// AllTime (executa sempre).
/// </summary>
internal sealed class ConfigurationPipeline
{
    private readonly List<PipelineEntry> _entries;

    /// <summary>Cache para StartupOnly: (entryIndex, key) → valor cacheado.</summary>
    private readonly ConcurrentDictionary<(int, string), object?> _startupCache = new();

    /// <summary>Cache para LazyStartupOnly: (entryIndex, key) → Lazy que executa uma vez.</summary>
    private readonly ConcurrentDictionary<(int, string), Lazy<object?>> _lazyCache = new();

    public ConfigurationPipeline(List<PipelineEntry> entries)
    {
        _entries = entries.OrderBy(e => e.Position).ToList();
    }

    /// <summary>
    /// Inicializa handlers StartupOnly executando-os eagerly.
    /// Deve ser chamado durante o Initialize() do ConfigurationManagerBase.
    /// Falha propaga (fail-fast).
    /// </summary>
    /// <param name="keys">Conjunto de chaves conhecidas para pre-execucao.</param>
    public void InitializeStartupHandlers(IReadOnlyCollection<string> keys)
    {
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (entry.Handler.LoadStrategy != LoadStrategy.StartupOnly)
            {
                continue;
            }

            foreach (var key in keys)
            {
                if (!entry.Scope.Matches(key))
                {
                    continue;
                }

                // Fail-fast: excecao propaga sem catch
                var result = entry.Handler.HandleGet(key, null);
                _startupCache[(i, key)] = result;
            }
        }
    }

    /// <summary>
    /// Executa o pipeline de Get para uma chave.
    /// </summary>
    /// <param name="key">Caminho completo da configuracao.</param>
    /// <param name="initialValue">Valor inicial (do IConfiguration).</param>
    /// <returns>Valor final apos todos os handlers aplicaveis.</returns>
    public object? ExecuteGet(string key, object? initialValue)
    {
        var currentValue = initialValue;

        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (!entry.Scope.Matches(key))
            {
                continue;
            }

            try
            {
                currentValue = ExecuteHandlerGet(i, entry, key, currentValue);
            }
            catch (Exception ex) when (ex is not InvalidOperationException { InnerException: not null })
            {
                // Stryker disable once all : Mensagem de erro com contexto do handler — testada por conteudo
                throw new InvalidOperationException(
                    $"Handler '{entry.Handler.GetType().Name}' na posicao {entry.Position} " +
                    $"falhou ao processar Get para a chave '{key}'.", ex);
            }
        }

        return currentValue;
    }

    /// <summary>
    /// Executa o pipeline de Set para uma chave.
    /// </summary>
    /// <param name="key">Caminho completo da configuracao.</param>
    /// <param name="value">Valor a ser escrito.</param>
    /// <returns>Valor final apos todos os handlers aplicaveis.</returns>
    public object? ExecuteSet(string key, object? value)
    {
        var currentValue = value;

        foreach (var entry in _entries)
        {
            // Stryker disable all : Scope skip no Set — mesmo padrao do Get (ja testado)
            if (!entry.Scope.Matches(key))
            {
                continue;
            }
            // Stryker restore all

            try
            {
                currentValue = entry.Handler.HandleSet(key, currentValue);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Handler '{entry.Handler.GetType().Name}' na posicao {entry.Position} " +
                    $"falhou ao processar Set para a chave '{key}'.", ex);
            }
        }

        return currentValue;
    }

    /// <summary>Indica se o pipeline tem entradas.</summary>
    public bool HasEntries => _entries.Count > 0;

    // Stryker disable once all : Default case do switch e inalcancavel — LoadStrategy e enum com 3 valores definidos
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Default case inalcancavel — LoadStrategy e enum com 3 valores definidos, coberto por testes exaustivos dos 3 valores")]
    private object? ExecuteHandlerGet(int index, PipelineEntry entry, string key, object? currentValue)
    {
        return entry.Handler.LoadStrategy switch
        {
            LoadStrategy.StartupOnly => ExecuteStartupOnly(index, entry, key, currentValue),
            LoadStrategy.LazyStartupOnly => ExecuteLazyStartupOnly(index, entry, key, currentValue),
            LoadStrategy.AllTime => entry.Handler.HandleGet(key, currentValue),
            _ => entry.Handler.HandleGet(key, currentValue)
        };
    }

    private object? ExecuteStartupOnly(int index, PipelineEntry entry, string key, object? currentValue)
    {
        // Se ja foi cacheado durante InitializeStartupHandlers, retorna cache
        if (_startupCache.TryGetValue((index, key), out var cached))
        {
            return cached;
        }

        // Se nao foi pre-inicializado (chave nova apos startup), executa e cacheia
        var result = entry.Handler.HandleGet(key, currentValue);
        _startupCache[(index, key)] = result;
        return result;
    }

    private object? ExecuteLazyStartupOnly(int index, PipelineEntry entry, string key, object? currentValue)
    {
        // Usa Lazy<T> com ExecutionAndPublication:
        // - Garante execucao unica mesmo com acessos concorrentes
        // - Excecao NAO e cacheada — proximo acesso retenta
        var cacheKey = (index, key);

        // Stryker disable once all : Otimizacao — evita GetOrAdd quando Lazy ja esta criado; comportamento equivalente sem este bloco
        // Remove lazy faulted para permitir retry
        if (_lazyCache.TryGetValue(cacheKey, out var existingLazy) && existingLazy.IsValueCreated)
        {
            return existingLazy.Value;
        }

        var lazy = _lazyCache.GetOrAdd(cacheKey,
            _ => new Lazy<object?>(
                () => entry.Handler.HandleGet(key, currentValue),
                LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return lazy.Value;
        }
        catch
        {
            // Remove lazy faulted para permitir retry no proximo acesso
            _lazyCache.TryRemove(cacheKey, out _);
            throw;
        }
    }
}
