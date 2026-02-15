# Data Model: Configuration BuildingBlock

**Phase 1 Output** | **Date**: 2026-02-15

## Tipos e Relacionamentos

```
ConfigurationManagerBase (abstract class)
├── _configuration: IConfiguration (interno, injetado)
├── _logger: ILogger (interno, injetado)
├── _sectionMappings: Dictionary<Type, string> (tipo → secao)
├── _pathCache: ConcurrentDictionary<(Type,string), string> (cache de paths)
├── _getPipeline: ConfigurationPipeline (pipeline de Get)
├── _setPipeline: ConfigurationPipeline (pipeline de Set)
│
├── Initialize() → ConfigureInternal(ConfigurationOptions)  [Template Method]
├── Get<TSection>() → TSection                              [le secao inteira]
├── Get<TSection,TProp>(Expression) → TProp                  [le propriedade]
├── Set<TSection,TProp>(Expression, value) → void            [escreve propriedade]
│
└── depende de:
    ├── ConfigurationPipeline
    ├── ConfigurationOptions (durante Initialize)
    └── IConfiguration (runtime)

ConfigurationHandlerBase (abstract class)
├── LoadStrategy: LoadStrategy (enum)
├── HandleGet(string key, object? value) → object?
├── HandleSet(string key, object? value) → object?
│
└── estendido por: handlers concretos do consumer

ConfigurationPipeline (internal sealed class)
├── _entries: List<PipelineEntry> (ordenada por posicao)
│   └── PipelineEntry: (handler, scope, position)
├── ExecuteGet(string key, object? initialValue) → object?
├── ExecuteSet(string key, object? value) → object?
│
└── filtra handlers por HandlerScope.Matches(key)

HandlerScope (readonly struct)
├── ScopeType: ScopeType (enum: Global, Class, Property)
├── PathPattern: string (path ou prefixo para matching)
├── Matches(string key) → bool
│
└── criado por: ConfigurationHandlerBuilder (fluent API)

ConfigurationPath (readonly struct)
├── Section: string (ex: "Persistence:PostgreSql")
├── Property: string (ex: "ConnectionString")
├── FullPath: string (ex: "Persistence:PostgreSql:ConnectionString")
│
└── criado por: ConfigurationManagerBase (derivacao automatica)

LoadStrategy (enum)
├── StartupOnly = 0    (executa uma vez no Initialize, cache permanente)
├── LazyStartupOnly = 1 (executa no primeiro acesso, cache permanente)
├── AllTime = 2         (executa a cada acesso, sem cache)

ConfigurationOptions (class, usada durante registro)
├── MapSection<T>(string sectionPath)
├── AddHandler<T>() → ConfigurationHandlerBuilder<T>
│
└── consumida por: ConfigurationManagerBase.ConfigureInternal()

ConfigurationHandlerBuilder<T> (fluent builder)
├── AtPosition(int) → self
├── WithLoadStrategy(LoadStrategy) → self
├── ForGet() → self
├── ForSet() → self
├── ForBoth() → self (default)
├── ToClass<TClass>() → ClassScopeBuilder<TClass>
│
└── ClassScopeBuilder<TClass>
    └── ToProperty<TProp>(Expression<Func<TClass,TProp>>) → self

ServiceCollectionExtensions (static class)
└── AddBedrockConfiguration<TManager>(IServiceCollection, Action<ConfigurationOptions>)
```

## Entidades Detalhadas

### 1. ConfigurationManagerBase

| Campo | Tipo | Visibilidade | Descricao |
|-------|------|-------------|-----------|
| `_configuration` | `IConfiguration` | `private readonly` | IConfiguration interno, injetado via construtor |
| `_logger` | `ILogger` | `private readonly` | Logger para observabilidade do pipeline |
| `_sectionMappings` | `Dictionary<Type, string>` | `private readonly` | Mapeamento tipo → secao de configuracao |
| `_pathCache` | `ConcurrentDictionary<(Type, string), string>` | `private static readonly` | Cache global de paths derivados |
| `_getPipeline` | `ConfigurationPipeline` | `private readonly` | Pipeline de handlers para operacoes Get |
| `_setPipeline` | `ConfigurationPipeline` | `private readonly` | Pipeline de handlers para operacoes Set |

**Metodos publicos**:

| Metodo | Retorno | Descricao |
|--------|---------|-----------|
| `Get<TSection>()` | `TSection` | Le secao inteira do IConfiguration, passa cada propriedade pelo pipeline |
| `Get<TSection, TProp>(Expression<Func<TSection, TProp>>)` | `TProp` | Le propriedade especifica, passa pelo pipeline |
| `Set<TSection, TProp>(Expression<Func<TSection, TProp>>, TProp)` | `void` | Escreve propriedade, passa pelo pipeline de Set |

**Metodos protegidos**:

| Metodo | Retorno | Descricao |
|--------|---------|-----------|
| `Initialize()` | `void` | Chamado no construtor, invoca ConfigureInternal |
| `ConfigureInternal(ConfigurationOptions)` | `void` | **Abstract** — subclasse configura secoes e handlers |

**Construtor**: `protected ConfigurationManagerBase(IConfiguration configuration, ILogger logger)`

### 2. ConfigurationHandlerBase

| Campo | Tipo | Visibilidade | Descricao |
|-------|------|-------------|-----------|
| `LoadStrategy` | `LoadStrategy` | `public` (get) | Estrategia de carregamento deste handler |

**Metodos abstratos**:

| Metodo | Retorno | Descricao |
|--------|---------|-----------|
| `HandleGet(string key, object? currentValue)` | `object?` | Processa valor no pipeline de Get. Recebe chave completa + valor atual. |
| `HandleSet(string key, object? currentValue)` | `object?` | Processa valor no pipeline de Set. Recebe chave completa + valor atual. |

**Construtor**: `protected ConfigurationHandlerBase(LoadStrategy loadStrategy)`

### 3. ConfigurationPipeline

| Campo | Tipo | Visibilidade | Descricao |
|-------|------|-------------|-----------|
| `_entries` | `List<PipelineEntry>` | `private readonly` | Lista ordenada de (handler, scope, position) |
| `_cachedValues` | `ConcurrentDictionary<(int, string), Lazy<object?>>` | `private readonly` | Cache para StartupOnly e LazyStartupOnly |

**PipelineEntry** (internal readonly struct):

| Campo | Tipo | Descricao |
|-------|------|-----------|
| `Handler` | `ConfigurationHandlerBase` | Instancia do handler |
| `Scope` | `HandlerScope` | Escopo de aplicacao |
| `Position` | `int` | Posicao no pipeline |

### 4. HandlerScope

| Campo | Tipo | Descricao |
|-------|------|-----------|
| `ScopeType` | `ScopeType` | Global, Class, ou Property |
| `PathPattern` | `string` | Padrao de matching (vazio para global, secao para class, path completo para property) |

**Metodos**:

| Metodo | Retorno | Descricao |
|--------|---------|-----------|
| `Matches(string key)` | `bool` | Global: sempre true. Class: key.StartsWith(pattern + ":"). Property: key == pattern. |
| `static Global()` | `HandlerScope` | Factory para escopo global |
| `static ForClass(string sectionPath)` | `HandlerScope` | Factory para escopo de classe |
| `static ForProperty(string fullPath)` | `HandlerScope` | Factory para escopo de propriedade |

### 5. ConfigurationPath

| Campo | Tipo | Descricao |
|-------|------|-----------|
| `Section` | `string` | Secao de configuracao (ex: "Persistence:PostgreSql") |
| `Property` | `string` | Nome da propriedade (ex: "ConnectionString") |
| `FullPath` | `string` | Caminho completo (ex: "Persistence:PostgreSql:ConnectionString") |

**Factory**: `static ConfigurationPath Create(string section, string property)` — constroi com `string.Create` para zero-allocation intermediaria.

### 6. LoadStrategy

```
enum LoadStrategy
{
    StartupOnly = 0,      // Executa no Initialize(), cache permanente
    LazyStartupOnly = 1,  // Executa no primeiro acesso, cache permanente (Lazy<T>)
    AllTime = 2           // Executa a cada acesso, sem cache
}
```

### 7. ConfigurationOptions

| Campo | Tipo | Descricao |
|-------|------|-----------|
| `_sectionMappings` | `Dictionary<Type, string>` | Tipo → secao |
| `_handlerRegistrations` | `List<HandlerRegistration>` | Registros de handlers pendentes |

**Metodos**:

| Metodo | Retorno | Descricao |
|--------|---------|-----------|
| `MapSection<T>(string sectionPath)` | `ConfigurationOptions` | Registra mapeamento tipo → secao |
| `AddHandler<T>()` | `ConfigurationHandlerBuilder<T>` | Inicia fluent builder para handler |

### 8. ConfigurationHandlerBuilder\<T\>

Fluent builder que acumula configuracao de um handler.

| Metodo | Retorno | Descricao |
|--------|---------|-----------|
| `AtPosition(int position)` | `self` | Define posicao no pipeline |
| `WithLoadStrategy(LoadStrategy)` | `self` | Define estrategia de carregamento |
| `ForGet()` | `self` | Registra apenas no pipeline de Get |
| `ForSet()` | `self` | Registra apenas no pipeline de Set |
| `ForBoth()` | `self` | Registra em ambos (default) |
| `ToClass<TClass>()` | `ClassScopeBuilder<TClass>` | Define escopo por classe |

**ClassScopeBuilder\<TClass\>**: Builder aninhado com metodo `ToProperty<TProp>(Expression<Func<TClass, TProp>>)` para escopo por propriedade.

## Diagrama de Fluxo (Get)

```
Developer chama: manager.Get<PostgreSqlConfig>()
    │
    ▼
ConfigurationManagerBase.Get<TSection>()
    │
    ├── Busca secao: _sectionMappings[typeof(PostgreSqlConfig)] = "Persistence:PostgreSql"
    │
    ├── Para cada propriedade de PostgreSqlConfig:
    │   │
    │   ├── Deriva path: "Persistence:PostgreSql:ConnectionString" (cache)
    │   │
    │   ├── Le valor inicial: _configuration.GetSection(path).Get<TProp>()
    │   │
    │   ├── Executa pipeline: _getPipeline.ExecuteGet(path, initialValue)
    │   │   │
    │   │   ├── Handler 1 (scope: global) → Matches? Sim → HandleGet(key, value)
    │   │   ├── Handler 2 (scope: class "Persistence:PostgreSql") → Matches? Sim
    │   │   ├── Handler 3 (scope: property "Persistence:PostgreSql:ConnectionString") → Sim
    │   │   └── Handler 4 (scope: property "Persistence:PostgreSql:Schema") → Nao → Skip
    │   │
    │   └── Atribui valor final na propriedade do objeto resultado
    │
    └── Retorna objeto PostgreSqlConfig populado
```

## Validacoes e Invariantes

1. **Posicoes duplicadas**: `ConfigurationOptions.Build()` DEVE rejeitar dois handlers na mesma posicao no mesmo pipeline (RF-014).
2. **Secao nao mapeada**: `Get<TSection>()` DEVE lancar `InvalidOperationException` se TSection nao foi mapeada via `MapSection`.
3. **StartupOnly failure**: Se handler StartupOnly lanca excecao no Initialize(), a excecao propaga (fail-fast).
4. **LazyStartupOnly failure**: Excecao NAO e cacheada. Proximo acesso retenta.
5. **Null em nullable**: Propriedade nullable (`int?`) retorna null se chave inexistente. Propriedade non-nullable retorna default.
6. **Array vazio**: Propriedade `string[]` retorna `[]` (array vazio) se secao inexistente, nao null.
