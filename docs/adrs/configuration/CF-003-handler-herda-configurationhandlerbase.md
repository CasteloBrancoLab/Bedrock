# CF-003: Handler Deve Herdar ConfigurationHandlerBase com LoadStrategy

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine uma linha de montagem de automoveis. Cada estacao (pintura, soldagem, eletrica) tem um operador especializado. Todos os operadores seguem o mesmo protocolo: recebem a peca da estacao anterior, aplicam sua transformacao e passam para a proxima. Alem disso, cada estacao tem uma estrategia de quando preparar seus materiais: (1) preparar tudo antes da linha abrir, (2) preparar no primeiro carro que passar, ou (3) preparar a cada carro novo.

### O Problema Tecnico

Handlers de configuracao precisam de um contrato claro:

1. **Interface inconsistente**: Sem base class, cada handler define seus proprios metodos com nomes diferentes (`Decrypt()`, `Transform()`, `Process()`).
2. **Sem estrategia de caching**: Alguns handlers sao caros (chamada a Key Vault, descriptografia). Sem mecanismo padrao, cada handler implementa seu proprio cache — ou nao implementa.
3. **Responsabilidade indefinida**: Sem separacao Get/Set, handlers que so devem atuar na leitura acabam interferindo na escrita.

## Como Normalmente E Feito

### Abordagem Tradicional

Interfaces genericas sem estrategia de execucao:

```csharp
public interface IConfigHandler
{
    object? Process(string key, object? value);
}

public class DecryptHandler : IConfigHandler
{
    private readonly Dictionary<string, object?> _cache = new(); // cache manual

    public object? Process(string key, object? value)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var result = Decrypt(value);
        _cache[key] = result; // cada handler reimplementa caching
        return result;
    }
}
```

### Por Que Nao Funciona Bem

- **Cache duplicado**: Cada handler reimplementa caching de forma diferente (ou esquece).
- **Sem distincao Get/Set**: O mesmo metodo e chamado para leitura e escrita.
- **Sem estrategia declarativa**: Nao ha como dizer "este handler so executa uma vez no startup" de forma padronizada.

## A Decisao

### Nossa Abordagem

Todo handler de configuracao DEVE herdar `ConfigurationHandlerBase` e declarar sua `LoadStrategy` no construtor:

```csharp
public sealed class DecryptionHandler : ConfigurationHandlerBase
{
    private readonly IKeyVaultClient _client;

    // LoadStrategy declarada no construtor
    public DecryptionHandler()
        : base(LoadStrategy.StartupOnly) { }

    public override object? HandleGet(string key, object? currentValue)
    {
        if (currentValue is not string encrypted)
            return currentValue;

        return _client.Decrypt(encrypted);
    }

    public override object? HandleSet(string key, object? currentValue)
    {
        // Set nao descriptografa — passa adiante
        return currentValue;
    }
}
```

**Contrato da base class:**

```csharp
public abstract class ConfigurationHandlerBase
{
    public LoadStrategy LoadStrategy { get; }

    protected ConfigurationHandlerBase(LoadStrategy loadStrategy);

    // Transformacao na leitura — recebe valor atual, retorna valor transformado
    public abstract object? HandleGet(string key, object? currentValue);

    // Transformacao na escrita — recebe valor atual, retorna valor transformado
    public abstract object? HandleSet(string key, object? currentValue);
}
```

**LoadStrategy — 3 estrategias:**

| Strategy | Quando Executa | Cache | Uso Tipico |
|----------|---------------|-------|------------|
| `StartupOnly` | Uma vez no `Initialize()`, pré-executa eagerly | `ConcurrentDictionary` | Descriptografia, secrets do Key Vault |
| `LazyStartupOnly` | Uma vez no primeiro `Get()`, via `Lazy<T>` | `Lazy<T>` com retry em falha | Configuracoes caras que nem sempre sao lidas |
| `AllTime` | Toda chamada `Get()` | Nenhum | Validacao, transformacao leve |

**Comportamento de cache gerenciado pelo pipeline:**

```
StartupOnly:
  Initialize() → handler.HandleGet(key, raw) → cache[(index,key)] = result
  Get()        → return cache[(index,key)]  (sem re-executar handler)

LazyStartupOnly:
  Get() (1a vez) → Lazy<T> executa handler.HandleGet() → cache
  Get() (2a vez) → return Lazy.Value (sem re-executar)
  Get() (falha)  → remove Lazy faulted, proximo Get() retenta

AllTime:
  Get()          → handler.HandleGet(key, currentValue) (sempre)
```

**Regras fundamentais:**

1. **Herda `ConfigurationHandlerBase`**: Nao usar interfaces — a base class define o contrato e carrega a `LoadStrategy`.
2. **`LoadStrategy` no construtor**: Declarativa — o pipeline sabe como cachear sem o handler precisar implementar cache.
3. **`HandleGet` e `HandleSet` abstratos**: Ambos devem ser implementados. Se o handler nao atua em Get ou Set, retornar `currentValue` (passthrough).
4. **Recebe `key`**: O handler recebe a chave completa (ex: `"Persistence:PostgreSql:ConnectionString"`) para tomar decisoes baseadas no contexto.
5. **Stateless por padrao**: O caching e gerenciado pelo pipeline, nao pelo handler. O handler deve ser stateless em relacao a cache.

### Por Que Funciona Melhor

- **Cache centralizado**: O pipeline gerencia cache para `StartupOnly` e `LazyStartupOnly` — o handler so implementa a logica de transformacao.
- **Retry automatico**: `LazyStartupOnly` remove o `Lazy<T>` faulted e retenta no proximo acesso — sem codigo adicional no handler.
- **Fail-fast**: `StartupOnly` executa durante `Initialize()` — se o Key Vault estiver indisponivel, a aplicacao falha no startup, nao em runtime.
- **Distincao clara Get/Set**: Handlers que so atuam na leitura implementam `HandleSet` como passthrough.

## Consequencias

### Beneficios

- Contrato unico e claro para todos os handlers.
- Tres estrategias de caching cobrindo cenarios comuns (eager, lazy, sem cache).
- Cache gerenciado pelo pipeline — handlers sao simples e stateless.
- Code agents geram handlers corretos com 3 decisoes: LoadStrategy, logica de Get, logica de Set.

### Trade-offs (Com Perspectiva)

- **Dois metodos abstratos**: Handlers que so atuam em Get ainda devem implementar `HandleSet` (como passthrough). Na pratica, e uma linha (`return currentValue;`).
- **LoadStrategy fixa**: A estrategia e definida no construtor e nao muda. Se um handler precisar mudar de estrategia, cria-se um novo handler. Isso e intencional — configuracao e deterministica.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Strategy Pattern** (GoF): `LoadStrategy` define a estrategia de execucao/caching sem alterar o handler.
- **Chain of Responsibility** (GoF): Handlers sao encadeados no pipeline, cada um transformando o valor e passando adiante.
- **Template Method** (GoF): A base class define o contrato; a subclasse implementa a logica especifica.

### O Que o Clean Code Diz

> "Do One Thing."
>
> *Faca uma unica coisa.*

Robert C. Martin (2008). Cada handler faz uma unica transformacao. O pipeline compoe multiplos handlers. O cache e responsabilidade do pipeline, nao do handler.

### Outros Fundamentos

- **Single Responsibility Principle** (SOLID): Handler = transformacao. Pipeline = orquestracao e cache. Separacao clara de responsabilidades.
- **Interface Segregation Principle** (SOLID): `HandleGet` e `HandleSet` sao metodos separados — consumidores do pipeline chamam apenas o que precisam.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre StartupOnly e LazyStartupOnly em termos de quando e como o cache e populado?"
2. "Por que o LazyStartupOnly remove o Lazy<T> faulted ao inves de cachear a excecao?"
3. "Como implementar um handler de descriptografia com LoadStrategy.StartupOnly?"

### Leitura Recomendada

- GoF, *Design Patterns* (1994) — Strategy, Chain of Responsibility
- Microsoft, [Lazy<T> Class](https://learn.microsoft.com/en-us/dotnet/api/system.lazy-1)
- Microsoft, [ConcurrentDictionary](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Configuration | Define `ConfigurationHandlerBase`, `LoadStrategy`, `ConfigurationPipeline` (cache management) |

## Referencias no Codigo

- Base class: `src/BuildingBlocks/Configuration/Handlers/ConfigurationHandlerBase.cs`
- LoadStrategy enum: `src/BuildingBlocks/Configuration/Handlers/Enums/LoadStrategy.cs`
- Cache no pipeline: `src/BuildingBlocks/Configuration/Pipeline/ConfigurationPipeline.cs`
- ADR relacionada: [CF-004 — Posicao Explicita no Pipeline](./CF-004-posicao-explicita-unica-no-pipeline.md)
