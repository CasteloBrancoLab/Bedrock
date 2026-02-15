# Public API Contract: Configuration BuildingBlock

**Phase 1 Output** | **Date**: 2026-02-15
**Namespace**: `Bedrock.BuildingBlocks.Configuration`

## API Surface

### ConfigurationManagerBase

```csharp
namespace Bedrock.BuildingBlocks.Configuration;

/// <summary>
/// Classe base abstrata para gerenciamento de configuracao com pipeline de handlers.
/// Encapsula IConfiguration e estende seu comportamento com handlers customizados.
/// </summary>
public abstract class ConfigurationManagerBase
{
    /// <summary>
    /// Cria uma nova instancia do configuration manager.
    /// Chama Initialize() que invoca ConfigureInternal().
    /// </summary>
    /// <param name="configuration">IConfiguration com fontes padrao ja configuradas.</param>
    /// <param name="logger">Logger para observabilidade do pipeline.</param>
    protected ConfigurationManagerBase(IConfiguration configuration, ILogger logger);

    /// <summary>
    /// Le uma secao inteira de configuracao como objeto tipado.
    /// Cada propriedade passa pelo pipeline de handlers.
    /// </summary>
    /// <typeparam name="TSection">Tipo do objeto de configuracao mapeado para a secao.</typeparam>
    /// <returns>Objeto populado com valores resolvidos pelo pipeline.</returns>
    /// <exception cref="InvalidOperationException">Se TSection nao foi mapeada via MapSection.</exception>
    public TSection Get<TSection>() where TSection : class, new();

    /// <summary>
    /// Le uma propriedade especifica de uma secao de configuracao.
    /// O caminho e derivado automaticamente da classe e propriedade.
    /// </summary>
    /// <typeparam name="TSection">Tipo da secao de configuracao.</typeparam>
    /// <typeparam name="TProperty">Tipo da propriedade.</typeparam>
    /// <param name="propertyExpression">Expressao type-safe para a propriedade.</param>
    /// <returns>Valor resolvido pelo pipeline.</returns>
    public TProperty Get<TSection, TProperty>(
        Expression<Func<TSection, TProperty>> propertyExpression)
        where TSection : class;

    /// <summary>
    /// Escreve um valor de configuracao atraves do pipeline de Set.
    /// </summary>
    /// <typeparam name="TSection">Tipo da secao de configuracao.</typeparam>
    /// <typeparam name="TProperty">Tipo da propriedade.</typeparam>
    /// <param name="propertyExpression">Expressao type-safe para a propriedade.</param>
    /// <param name="value">Valor a ser escrito.</param>
    public void Set<TSection, TProperty>(
        Expression<Func<TSection, TProperty>> propertyExpression,
        TProperty value)
        where TSection : class;

    /// <summary>
    /// Ponto de extensao para subclasses configurarem secoes e handlers.
    /// Chamado uma vez durante Initialize().
    /// </summary>
    /// <param name="options">Opcoes de configuracao (fluent API).</param>
    protected abstract void ConfigureInternal(ConfigurationOptions options);
}
```

### ConfigurationHandlerBase

```csharp
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
    protected ConfigurationHandlerBase(LoadStrategy loadStrategy);

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
```

### LoadStrategy

```csharp
namespace Bedrock.BuildingBlocks.Configuration.Handlers.Enums;

/// <summary>
/// Define quando um handler carrega ou atualiza seus dados.
/// </summary>
public enum LoadStrategy
{
    /// <summary>Executa uma vez durante Initialize(). Cache permanente. Falha = fail-fast.</summary>
    StartupOnly = 0,

    /// <summary>Executa no primeiro acesso. Cache permanente. Falha NAO e cacheada (retry).</summary>
    LazyStartupOnly = 1,

    /// <summary>Executa a cada acesso. Sem cache.</summary>
    AllTime = 2
}
```

### HandlerScope

```csharp
namespace Bedrock.BuildingBlocks.Configuration.Handlers;

/// <summary>
/// Define o escopo de aplicacao de um handler (quais chaves ele processa).
/// </summary>
public readonly struct HandlerScope : IEquatable<HandlerScope>
{
    /// <summary>Tipo de escopo: Global, Class, ou Property.</summary>
    public ScopeType ScopeType { get; }

    /// <summary>Padrao de matching (vazio para global, secao para class, path completo para property).</summary>
    public string PathPattern { get; }

    /// <summary>Verifica se uma chave corresponde a este escopo.</summary>
    /// <param name="key">Chave completa de configuracao.</param>
    /// <returns>true se o handler deve processar esta chave.</returns>
    public bool Matches(string key);

    /// <summary>Cria escopo global (todas as chaves).</summary>
    public static HandlerScope Global();

    /// <summary>Cria escopo por classe (todas as propriedades de uma secao).</summary>
    /// <param name="sectionPath">Caminho da secao (ex: "Persistence:PostgreSql").</param>
    public static HandlerScope ForClass(string sectionPath);

    /// <summary>Cria escopo por propriedade (chave exata).</summary>
    /// <param name="fullPath">Caminho completo (ex: "Persistence:PostgreSql:ConnectionString").</param>
    public static HandlerScope ForProperty(string fullPath);
}

/// <summary>Tipo de escopo de um handler.</summary>
public enum ScopeType
{
    Global = 0,
    Class = 1,
    Property = 2
}
```

### ConfigurationPath

```csharp
namespace Bedrock.BuildingBlocks.Configuration;

/// <summary>
/// Encapsula um caminho de configuracao derivado de classe + propriedade.
/// </summary>
public readonly struct ConfigurationPath : IEquatable<ConfigurationPath>
{
    /// <summary>Secao de configuracao (ex: "Persistence:PostgreSql").</summary>
    public string Section { get; }

    /// <summary>Nome da propriedade (ex: "ConnectionString").</summary>
    public string Property { get; }

    /// <summary>Caminho completo (ex: "Persistence:PostgreSql:ConnectionString").</summary>
    public string FullPath { get; }

    /// <summary>Cria um ConfigurationPath a partir de secao e propriedade.</summary>
    public static ConfigurationPath Create(string section, string property);
}
```

### ConfigurationOptions (Registration)

```csharp
namespace Bedrock.BuildingBlocks.Configuration.Registration;

/// <summary>
/// Opcoes de configuracao para o pipeline de handlers.
/// Usado no ConfigureInternal() e no registro DI.
/// </summary>
public sealed class ConfigurationOptions
{
    /// <summary>
    /// Mapeia uma classe de configuracao para uma secao do IConfiguration.
    /// </summary>
    /// <typeparam name="TSection">Tipo da classe de configuracao (POCO).</typeparam>
    /// <param name="sectionPath">Caminho da secao (ex: "Persistence:PostgreSql").</param>
    /// <returns>Esta instancia para encadeamento.</returns>
    public ConfigurationOptions MapSection<TSection>(string sectionPath)
        where TSection : class, new();

    /// <summary>
    /// Adiciona um handler ao pipeline via fluent API.
    /// </summary>
    /// <typeparam name="THandler">Tipo do handler (deve estender ConfigurationHandlerBase).</typeparam>
    /// <returns>Builder para configurar posicao, escopo e estrategia.</returns>
    public ConfigurationHandlerBuilder<THandler> AddHandler<THandler>()
        where THandler : ConfigurationHandlerBase;
}
```

### ConfigurationHandlerBuilder\<T\> (Registration)

```csharp
namespace Bedrock.BuildingBlocks.Configuration.Registration;

/// <summary>
/// Fluent builder para configuracao de um handler no pipeline.
/// </summary>
public sealed class ConfigurationHandlerBuilder<THandler>
    where THandler : ConfigurationHandlerBase
{
    /// <summary>Define a posicao do handler no pipeline (ordem de execucao).</summary>
    public ConfigurationHandlerBuilder<THandler> AtPosition(int position);

    /// <summary>Define a estrategia de carregamento.</summary>
    public ConfigurationHandlerBuilder<THandler> WithLoadStrategy(LoadStrategy loadStrategy);

    /// <summary>Registra o handler apenas no pipeline de Get.</summary>
    public ConfigurationHandlerBuilder<THandler> ForGet();

    /// <summary>Registra o handler apenas no pipeline de Set.</summary>
    public ConfigurationHandlerBuilder<THandler> ForSet();

    /// <summary>Registra o handler em ambos os pipelines (default).</summary>
    public ConfigurationHandlerBuilder<THandler> ForBoth();

    /// <summary>Define escopo por classe (todas as propriedades da secao).</summary>
    /// <typeparam name="TClass">Classe de configuracao alvo.</typeparam>
    public ClassScopeBuilder<TClass> ToClass<TClass>() where TClass : class;
}

/// <summary>
/// Builder aninhado para definir escopo por propriedade especifica.
/// </summary>
public sealed class ClassScopeBuilder<TClass> where TClass : class
{
    /// <summary>Refina o escopo para uma propriedade especifica da classe.</summary>
    /// <typeparam name="TProperty">Tipo da propriedade.</typeparam>
    /// <param name="propertyExpression">Expressao type-safe para a propriedade.</param>
    public ClassScopeBuilder<TClass> ToProperty<TProperty>(
        Expression<Func<TClass, TProperty>> propertyExpression);
}
```

### ServiceCollectionExtensions

```csharp
namespace Bedrock.BuildingBlocks.Configuration.Registration;

/// <summary>
/// Extension methods para registro do Configuration BuildingBlock no IoC.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra um ConfigurationManager concreto e seus handlers no container de DI.
    /// </summary>
    /// <typeparam name="TManager">Tipo concreto do ConfigurationManager.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Action para configurar opcoes (secoes, handlers, pipeline).</param>
    /// <returns>Service collection para encadeamento.</returns>
    public static IServiceCollection AddBedrockConfiguration<TManager>(
        this IServiceCollection services,
        Action<ConfigurationOptions>? configure = null)
        where TManager : ConfigurationManagerBase;
}
```

## Namespace Map

```
Bedrock.BuildingBlocks.Configuration
├── ConfigurationManagerBase
├── ConfigurationPath
│
├── Handlers/
│   ├── ConfigurationHandlerBase
│   ├── HandlerScope
│   ├── ScopeType (enum dentro de HandlerScope ou separado)
│   └── Enums/
│       └── LoadStrategy
│
├── Pipeline/
│   └── ConfigurationPipeline (internal)
│
└── Registration/
    ├── ConfigurationOptions
    ├── ConfigurationHandlerBuilder<T>
    ├── ClassScopeBuilder<T>
    └── ServiceCollectionExtensions
```

## Dependencias Externas

| Pacote | Versao | Uso |
|--------|--------|-----|
| Microsoft.Extensions.Configuration.Abstractions | 10.0.1 | IConfiguration |
| Microsoft.Extensions.DependencyInjection.Abstractions | (transitivo) | IServiceCollection |
| Microsoft.Extensions.Logging.Abstractions | 10.0.1 | ILogger |

## Projeto Interno (Core)

| Projeto | Uso |
|---------|-----|
| Bedrock.BuildingBlocks.Core | Utilitarios compartilhados (se necessario) |
