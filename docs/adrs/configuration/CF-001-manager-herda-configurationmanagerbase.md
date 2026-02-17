# CF-001: ConfigurationManager Deve Herdar ConfigurationManagerBase

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine um painel de controle de uma usina eletrica. Cada setor da usina (turbinas, refrigeracao, distribuicao) tem seus proprios medidores e alarmes. Se cada setor construir seu painel do zero, nao ha padrao de leitura, de posicionamento de indicadores nem de tratamento de alarmes. Um operador que conhece o painel das turbinas nao consegue operar o da refrigeracao. Agora imagine que todos os paineis seguem um chassi padrao: mesma posicao para indicadores criticos, mesmo protocolo de alarme, mesma forma de leitura. Cada setor so configura quais medidores exibir — o comportamento do painel e garantido pelo chassi.

### O Problema Tecnico

Aplicacoes .NET usam `IConfiguration` para acessar configuracoes de diversas fontes (appsettings.json, variaveis de ambiente, Azure Key Vault). O problema surge quando se precisa de comportamento adicional sobre essas configuracoes:

1. **Logica espalhada**: Descriptografia de connection strings, validacao de valores, transformacao de formatos — cada desenvolvedor implementa no ponto de uso, sem padrao.
2. **Sem extensibilidade**: `IConfiguration` e read-only por design. Adicionar caching, fallback ou transformacao exige workarounds ad-hoc.
3. **Acoplamento com fontes**: Codigo de negocio referencia diretamente `IConfiguration["caminho:manual"]` com magic strings, quebrando ao renomear secoes.
4. **Sem pipeline**: Nao ha como encadear comportamentos (descriptografar → cachear → validar) de forma composivel e ordenada.

## Como Normalmente E Feito

### Abordagem Tradicional

A maioria dos projetos .NET usa `IOptions<T>` ou acesso direto ao `IConfiguration`:

```csharp
// Abordagem 1: IOptions — bind automatico, mas sem transformacao
public class UserService
{
    private readonly DatabaseOptions _options;

    public UserService(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
        // Precisa descriptografar? Faz aqui.
        // Precisa validar? Faz aqui tambem.
        // Precisa cachear? Implementa por conta propria.
    }
}

// Abordagem 2: IConfiguration direta — magic strings
public class OrderService
{
    public OrderService(IConfiguration config)
    {
        var connStr = config["Persistence:PostgreSql:ConnectionString"];
        // E se renomear a secao? Quebra silenciosamente.
    }
}
```

### Por Que Nao Funciona Bem

- **Logica de transformacao duplicada**: Cada service que precisa descriptografar uma config reimplementa a logica.
- **Sem composicao**: Nao ha como dizer "primeiro descriptografe, depois valide, depois cacheie" de forma declarativa.
- **Magic strings**: `config["caminho:manual"]` quebra silenciosamente se o caminho mudar.
- **Acoplamento**: Services conhecem detalhes da estrutura do `IConfiguration` (nomes de secoes, hierarquia de chaves).

## A Decisao

### Nossa Abordagem

Todo gerenciador de configuracao no Bedrock DEVE herdar de `ConfigurationManagerBase`:

```csharp
public sealed class AppConfigurationManager : ConfigurationManagerBase
{
    public AppConfigurationManager(IConfiguration configuration, ILogger logger)
        : base(configuration, logger) { }

    protected override void ConfigureInternal(ConfigurationOptions options)
    {
        // 1. Mapear secoes para classes tipadas
        options.MapSection<DatabaseConfig>("Persistence:PostgreSql");
        options.MapSection<CacheConfig>("Persistence:Redis");

        // 2. Registrar handlers no pipeline
        options.AddHandler<DecryptionHandler>()
            .AtPosition(1)
            .ToClass<DatabaseConfig>()
            .ToProperty(x => x.ConnectionString);

        options.AddHandler<ValidationHandler>()
            .AtPosition(2)
            .ForGet();
    }
}
```

**Regras fundamentais:**

1. **Herda `ConfigurationManagerBase`**: Herda pipeline de Get/Set, caching, resolucao de paths e inicializacao.
2. **Implementa `ConfigureInternal`**: Ponto unico de configuracao — secoes e handlers.
3. **Construtor delega para base**: Passa `IConfiguration` e `ILogger` para o chassi.
4. **`sealed`**: O manager concreto nao deve ser estendido — extensibilidade via handlers, nao heranca.
5. **Vive em `Infra.CrossCutting.Configuration`**: Conforme [IN-001](../infrastructure/IN-001-camadas-canonicas-bounded-context.md), todo bounded context tem um projeto `{BC}.Infra.CrossCutting.Configuration` que centraliza modelos de configuracao e o ConfigurationManager concreto. Exemplo: `ShopDemo.Auth.Infra.CrossCutting.Configuration`.

**Ciclo de vida:**

```
Construtor → base(config, logger)
  └→ Initialize()
      ├→ ConfigureInternal(options)  ← subclasse configura
      ├→ BuildPipelines()            ← monta Get e Set pipelines
      └→ InitializeStartupHandlers() ← pre-executa handlers StartupOnly
```

### Por Que Funciona Melhor

- **Template Method**: O ciclo de vida e garantido pela base class. A subclasse so configura *o que* — nao *como*.
- **Pipeline composivel**: Handlers sao encadeados por posicao, cada um com responsabilidade unica.
- **Type safety**: `Get<TSection>()` e `Get<TSection, TProperty>(x => x.Prop)` eliminam magic strings.
- **Extensibilidade sem heranca**: Novos comportamentos sao adicionados como handlers, nao como subclasses.

## Consequencias

### Beneficios

- Ponto unico de configuracao por bounded context.
- Ciclo de vida previsivel e testavel.
- Pipeline de handlers composivel e ordenado.
- Code agents sabem exatamente o que implementar: herdar base, implementar `ConfigureInternal`, mapear secoes e registrar handlers.

### Trade-offs (Com Perspectiva)

- **Uma classe a mais por BC**: Na pratica, o ConfigurationManager e uma classe de configuracao com 10-30 linhas em `ConfigureInternal`. O ganho em organizacao e extensibilidade compensa amplamente.
- **Indireção via pipeline**: `Get<T>()` passa por handlers ao inves de ir direto ao `IConfiguration`. Para a vasta maioria dos cenarios, o overhead e negligenciavel (microsegundos vs. milissegundos de I/O para ler de fontes externas).

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Template Method** (GoF): `ConfigurationManagerBase.Initialize()` define o algoritmo; `ConfigureInternal()` e o metodo que a subclasse implementa para configurar detalhes especificos.
- **Mediator** (GoF): O pipeline de handlers atua como mediator entre o consumidor e as diversas fontes/transformacoes de configuracao.
- **Builder** (GoF): `ConfigurationOptions` usa fluent builder para registrar secoes e handlers de forma declarativa.

### O Que o Clean Architecture Diz

> "Source code dependencies must point only inward, toward higher-level policies."
>
> *Dependencias de codigo-fonte devem apontar apenas para dentro, em direcao a politicas de nivel mais alto.*

Robert C. Martin (2017). O `ConfigurationManagerBase` vive no BuildingBlock (politica de alto nivel). A subclasse concreta vive no bounded context (detalhe) e depende da base — nunca o contrario.

### O Que o Clean Code Diz

> "Don't Repeat Yourself."
>
> *Nao se repita.*

Hunt & Thomas (1999). Sem a base class, cada BC reimplementaria ciclo de vida, pipeline e resolucao de paths. Com a base, tudo isso e herdado.

### Outros Fundamentos

- **Open/Closed Principle** (SOLID): Fechado para modificacao (base class), aberto para extensao (handlers).
- **Hollywood Principle**: "Don't call us, we'll call you" — a base class chama `ConfigureInternal`, nao o contrario.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Como o Template Method Pattern se aplica ao ConfigurationManagerBase?"
2. "Qual a diferenca entre extensibilidade via heranca e extensibilidade via composicao de handlers?"
3. "Por que o ConfigurationManager concreto deve ser sealed?"

### Leitura Recomendada

- GoF, *Design Patterns* (1994) — Template Method
- Robert C. Martin, *Clean Architecture* (2017), Cap. 8 — OCP
- Microsoft, [Options pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Configuration | Define `ConfigurationManagerBase`, `ConfigurationOptions`, `ConfigurationPipeline` |
| Bedrock.BuildingBlocks.Core | Dependencia base do Configuration BuildingBlock |

## Referencias no Codigo

- Base class: `src/BuildingBlocks/Configuration/ConfigurationManagerBase.cs`
- Registration: `src/BuildingBlocks/Configuration/Registration/ConfigurationOptions.cs`
- Pipeline: `src/BuildingBlocks/Configuration/Pipeline/ConfigurationPipeline.cs`
