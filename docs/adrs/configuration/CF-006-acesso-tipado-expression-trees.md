# CF-006: Acesso a Configuracao Deve Ser Tipado via Expression Trees

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine um almoxarifado com milhares de gavetas. No sistema antigo, voce pede o item pelo codigo escrito a mao em um papel: "A-23-B" — se errar uma letra, recebe a gaveta errada (ou nenhuma). No sistema novo, voce aponta para o item no catalogo digital e o sistema busca automaticamente. Nao ha como errar o codigo porque voce nunca digita — o sistema resolve.

### O Problema Tecnico

O padrao mais comum para acessar configuracoes em .NET usa magic strings:

1. **Typos silenciosos**: `config["Persistance:PostgreSql:ConectionString"]` retorna `null` sem erro (note os typos em "Persistence" e "Connection").
2. **Refactoring quebrado**: Renomear uma propriedade ou secao nao atualiza as strings de acesso.
3. **Sem IntelliSense**: Strings nao tem autocomplete — o desenvolvedor precisa consultar o `appsettings.json` para lembrar os nomes corretos.
4. **Paths manuais**: O consumidor precisa montar `"Secao:Subsecao:Propriedade"` manualmente — verbose e error-prone.

## Como Normalmente E Feito

### Abordagem Tradicional

Acesso via indexador de string ou `GetValue<T>()`:

```csharp
// Abordagem 1: Indexador — magic string completa
var connStr = configuration["Persistence:PostgreSql:ConnectionString"];

// Abordagem 2: GetSection + GetValue — ainda magic strings
var section = configuration.GetSection("Persistence:PostgreSql");
var connStr = section.GetValue<string>("ConnectionString");

// Abordagem 3: IOptions<T> — bind automatico, mas acesso ao valor e via propriedade
var connStr = options.Value.ConnectionString; // ok, mas sem pipeline de handlers
```

### Por Que Nao Funciona Bem

- **Magic strings em 1 e 2**: Qualquer typo retorna `null`/default sem erro.
- **Sem transformacao em 3**: `IOptions<T>` faz bind direto — nao passa por pipeline de handlers (descriptografia, validacao, etc.).
- **Paths duplicados**: Varios pontos do codigo referenciam o mesmo path com strings diferentes — um muda, outro nao.

## A Decisao

### Nossa Abordagem

O acesso a configuracao DEVE usar expression trees para resolucao type-safe de paths:

```csharp
// LEITURA — secao inteira (todas as propriedades passam pelo pipeline)
DatabaseConfig dbConfig = configManager.Get<DatabaseConfig>();

// LEITURA — propriedade especifica (type-safe, sem magic strings)
string connStr = configManager.Get<DatabaseConfig, string>(
    x => x.ConnectionString);

// ESCRITA — propriedade especifica (passa pelo pipeline de Set)
configManager.Set<DatabaseConfig, string>(
    x => x.ConnectionString, "nova-connection-string");
```

**Resolucao automatica de paths:**

```
Get<DatabaseConfig, string>(x => x.ConnectionString)
  │
  ├→ typeof(DatabaseConfig) → lookup em _sectionMappings
  │   └→ "Persistence:PostgreSql"
  │
  ├→ Expression tree → extrai "ConnectionString"
  │
  ├→ Monta path completo: "Persistence:PostgreSql:ConnectionString"
  │   └→ Cache estatico: PathCache[(typeof(DatabaseConfig), "ConnectionString")]
  │
  ├→ Le valor bruto do IConfiguration
  │
  ├→ Executa pipeline de Get (handlers aplicaveis ao path)
  │
  └→ Verifica in-memory overrides (Set previo)
```

**Conversao de tipos:**

```csharp
// Tipos suportados nativamente
int port = configManager.Get<DatabaseConfig, int>(x => x.Port);
bool ssl = configManager.Get<DatabaseConfig, bool>(x => x.UseSsl);
string[] hosts = configManager.Get<DatabaseConfig, string[]>(x => x.Hosts);

// Tipos nullable
int? timeout = configManager.Get<DatabaseConfig, int?>(x => x.Timeout);
```

O `ConfigurationManagerBase` converte valores automaticamente:
- `string` → retorno direto
- `int`, `long`, `double`, `decimal`, `bool` → parsing com `CultureInfo.InvariantCulture`
- `T?` (nullable) → `null` se secao nao existir
- `T[]` (arrays) → leitura de children indexados do `IConfiguration`
- Outros tipos → `Convert.ChangeType` como fallback

**Regras fundamentais:**

1. **Expression trees obrigatorias**: `Get<TSection, TProperty>(x => x.Prop)` — sem overloads que aceitem strings.
2. **Path resolvido automaticamente**: O consumidor nunca monta paths manuais — o sistema resolve `Secao:Propriedade`.
3. **Cache de paths**: `PathCache` (static `ConcurrentDictionary`) cacheia paths resolvidos — zero alocacao apos primeiro acesso.
4. **InvariantCulture**: Parsing numerico sempre usa `CultureInfo.InvariantCulture` — independente do locale do servidor.
5. **Get por secao**: `Get<TSection>()` retorna o objeto completo, com todas as propriedades resolvidas pelo pipeline.

### Por Que Funciona Melhor

- **Zero magic strings**: O compilador valida nomes de propriedades — typos sao erros de compilacao.
- **Refactoring seguro**: Renomear `ConnectionString` para `ConnString` atualiza automaticamente todos os usos via expression tree.
- **IntelliSense completo**: `x => x.` mostra todas as propriedades da classe de configuracao.
- **Path automatico**: O consumidor nao precisa saber que `DatabaseConfig` mapeia para `"Persistence:PostgreSql"`.
- **Pipeline integrado**: Diferente de `IOptions<T>`, o valor passa pelo pipeline de handlers antes de ser retornado.

## Consequencias

### Beneficios

- Eliminacao total de magic strings para acesso a configuracao.
- Erros de nome de propriedade detectados em compile-time.
- Cache de paths elimina overhead de resolucao repetida.
- Conversao de tipos automatica com `InvariantCulture`.
- Code agents geram acessos corretos sem consultar `appsettings.json`.

### Trade-offs (Com Perspectiva)

- **Expression trees vs. strings**: Expression trees tem overhead de construcao (~1μs). Na pratica, o `PathCache` garante que a resolucao acontece uma unica vez por (tipo, propriedade) — acessos subsequentes sao lookup em `ConcurrentDictionary` (nanosegundos).
- **Classe de configuracao obrigatoria**: Nao ha como acessar configuracoes "soltas" sem uma classe tipada. Isso e intencional — toda configuracao deve ter um tipo associado para type safety.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Expression Builder** (Fowler, DSL): Expression trees constroem uma representacao da query de acesso, que e compilada em runtime.
- **Identity Map** (Fowler, POEAA): `PathCache` atua como identity map para paths resolvidos — garante uma unica resolucao por chave.

### O Que o Clean Code Diz

> "Use Explanatory Variables."
>
> *Use variaveis explanatorias.*

Robert C. Martin (2008). `x => x.ConnectionString` e auto-documentado — o leitor sabe exatamente qual propriedade esta sendo lida sem consultar documentacao externa.

### Outros Fundamentos

- **Principle of Least Surprise**: `Get<DatabaseConfig, string>(x => x.ConnectionString)` faz exatamente o que parece — le a ConnectionString da secao mapeada para DatabaseConfig.
- **DRY Principle**: O path `"Persistence:PostgreSql"` e declarado uma vez em `MapSection` e nunca repetido nos pontos de consumo.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Como expression trees em C# permitem extrair nomes de propriedades em compile-time?"
2. "Qual o overhead de expression trees vs. string-based access e quando importa?"
3. "Como o ConfigurationManagerBase trata conversao de tipos nullable e arrays?"

### Leitura Recomendada

- Microsoft, [Expression Trees (C#)](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/)
- Martin Fowler, *Domain-Specific Languages* (2010) — Expression Builder
- Microsoft, [CultureInfo.InvariantCulture](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.invariantculture)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Configuration | Define `ConfigurationManagerBase.Get<T>()`, `Get<T,P>()`, `Set<T,P>()`, `ConfigurationPath`, `PathCache` |

## Referencias no Codigo

- Get/Set tipados: `src/BuildingBlocks/Configuration/ConfigurationManagerBase.cs`
- Path resolution: `src/BuildingBlocks/Configuration/ConfigurationPath.cs`
- Expression extraction: `src/BuildingBlocks/Configuration/ConfigurationManagerBase.cs` (metodo `ExtractPropertyName`)
- ADR relacionada: [CF-002 — MapSection](./CF-002-configurar-secoes-com-mapsection.md) (mapeamento tipo→secao)
