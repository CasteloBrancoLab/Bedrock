# Research: Configuration BuildingBlock

**Phase 0 Output** | **Date**: 2026-02-15

## R1. Estrategia de Wrapping do IConfiguration

**Decisao**: ConfigurationManagerBase recebe `IConfiguration` via construtor (injecao de dependencia). NAO constroi o IConfiguration internamente.

**Racional**:
- Segue o principio de Inversao de Dependencia — o BuildingBlock depende da abstracao `IConfiguration`, nao de como ela e construida.
- Em aplicacoes .NET, o `IConfiguration` ja e construido pelo host builder (`Host.CreateDefaultBuilder()`) com todas as fontes padrao. Reconstruir seria duplicacao.
- Depender apenas de `Microsoft.Extensions.Configuration.Abstractions` (sem os pacotes concretos de fonte) mantém o BuildingBlock leve.
- Consumers configuram o IConfiguration como quiserem; o BuildingBlock apenas o estende com handlers.

**Alternativas consideradas**:
1. *ConfigurationManagerBase constroi IConfiguration via IConfigurationBuilder*: Rejeitado — exigiria dependencia de 4 pacotes NuGet extras (Configuration, Configuration.Json, Configuration.EnvironmentVariables, Configuration.UserSecrets) e duplicaria logica que o host builder ja faz.
2. *Aceitar IConfigurationBuilder e adicionar fontes*: Rejeitado — mistura responsabilidades. O BuildingBlock estende valores, nao gerencia fontes.

**Implicacoes para RF-004, RF-006, RF-007**:
- RF-004 (fontes padrao): O IConfiguration recebido ja contem as fontes padrao. A responsabilidade de configura-las e do consumer.
- RF-006 (separador `_` para env vars): O consumer configura a fonte de env vars com o separador desejado. O BuildingBlock documenta a convencao Bedrock mas nao a impoe — isso seria responsabilidade de um futuro BuildingBlock de bootstrapping.
- RF-007 (user secrets com array de assemblies): Mesma abordagem — convencao documentada, implementada pelo consumer.

**Impacto na spec**: RF-004, RF-006, RF-007 continuam validos como requisitos de *comportamento esperado* do IConfiguration que o ConfigurationManagerBase recebe. A responsabilidade de configurar essas fontes e do consumer (bootstrapper), nao deste BuildingBlock.

---

## R2. Compilacao e Cache de Expression Trees

**Decisao**: Usar `Expression<Func<TClass, TProperty>>` para derivacao de caminho, compilado uma unica vez e cacheado em `ConcurrentDictionary<(Type, string), string>`.

**Racional**:
- Expression trees permitem extrair o nome da propriedade em compile-time sem reflection em runtime.
- O cache evita recompilacao — a chave e `(typeof(TClass), propertyName)`, o valor e o path completo derivado.
- `ConcurrentDictionary` e thread-safe e adequado para cache read-heavy com writes raros (apenas no primeiro acesso de cada path).

**Alternativas consideradas**:
1. *`nameof()` com string concatenacao manual*: Rejeitado — requer que o developer passe o nome da classe manualmente, violando RF-015 (derivacao automatica).
2. *CallerMemberName + CallerFilePath*: Rejeitado — funciona apenas em metodos, nao em propriedades. Nao permite derivar a secao da classe.
3. *Source generators*: Considerado — geraria paths em compile-time (zero runtime cost). Rejeitado por agora por complexidade adicional (novo projeto de generator, debugging mais dificil). Pode ser adicionado como otimizacao futura.

**Implementacao**:
```
// Derivacao de path a partir de expressao
Expression<Func<PostgreSqlConfig, string>> expr = c => c.ConnectionString;
// Extrai: memberName = "ConnectionString", classType = typeof(PostgreSqlConfig)
// Busca secao registrada para PostgreSqlConfig = "Persistence:PostgreSql"
// Path final = "Persistence:PostgreSql:ConnectionString"
```

---

## R3. Padrao de Pipeline (Mediator)

**Decisao**: Pipeline implementado como lista ordenada de `(handler, scope, position)` tuples. Execucao sequencial com short-circuit por scope matching.

**Racional**:
- Lista ordenada por posicao e a implementacao mais simples que atende ao requisito de ordenacao explicita (RF-008, RF-014).
- Scope matching e feito antes de invocar o handler — handlers fora do escopo sao pulados sem custo.
- O padrao mediator e familiar e testavel: cada handler recebe input e produz output.

**Alternativas consideradas**:
1. *Chain of Responsibility com linked list*: Rejeitado — mais complexo sem beneficio. A lista ordenada ja e uma chain.
2. *Middleware pipeline (ASP.NET style com next delegate)*: Considerado — elegante mas adiciona complexidade de delegates aninhados. O padrao sequencial e mais simples e suficiente.
3. *MediatR library*: Rejeitado — dependencia externa desnecessaria para um pipeline simples e controlado.

**Separacao Get/Set**: Dois pipelines independentes (conforme RF-002, RF-003). Um handler pode ser registrado em um ou ambos. A posicao e independente por pipeline.

---

## R4. Thread-Safety para LazyStartupOnly

**Decisao**: Usar `Lazy<T>` com `LazyThreadSafetyMode.ExecutionAndPublication` para cache de handlers LazyStartupOnly.

**Racional**:
- `Lazy<T>` garante que o factory execute apenas uma vez, mesmo com acessos concorrentes.
- `ExecutionAndPublication` e o modo padrao — se o factory lanca excecao, ela NAO e cacheada (permite retry conforme edge case da spec: "falhas nao sao cacheadas").
- Para StartupOnly, o valor e resolvido durante Initialize() — sem concorrencia, pois acontece antes de qualquer acesso.
- Para AllTime, sem cache — executa a cada acesso.

**Alternativas consideradas**:
1. *Double-check locking manual*: Rejeitado — `Lazy<T>` faz o mesmo sem boilerplate.
2. *`ConcurrentDictionary.GetOrAdd`*: Considerado — funcional mas semanticamente menos claro que `Lazy<T>` para "execute once".
3. *`SemaphoreSlim` com async*: Rejeitado — handlers sao sincronos (Get/Set retornam valor, nao Task). Se handlers async forem necessarios no futuro, sera uma extensao.

**Nota sobre retry**: `Lazy<T>` com `ExecutionAndPublication` NAO cacheia excecoes. Se o factory falhar, o proximo acesso retentara. Isso atende ao requisito da spec para LazyStartupOnly (edge case 4).

---

## R5. Zero-Allocation na Derivacao de Path

**Decisao**: Paths derivados sao strings (imutaveis, GC-friendly). O cache em `ConcurrentDictionary` evita recomputacao. `ConfigurationPath` e um `readonly struct` que encapsula a string.

**Racional**:
- Configuracao e acessada com frequencia moderada (nao e hot path de alta frequencia como serialization).
- O custo real esta na primeira derivacao (expression tree → string). Apos cache, e apenas um dictionary lookup.
- `readonly struct` para ConfigurationPath evita boxing e alocacao no stack.

**Alternativas consideradas**:
1. *`Span<char>` para paths*: Rejeitado — paths precisam ser armazenados em cache (dictionary key), o que exige string. Span nao pode ser armazenado em heap.
2. *`string.Create` com SpanAction*: Pode ser usado na construcao inicial do path (concatenacao section + ":" + property) para evitar alocacao intermediaria.
3. *Interning de strings*: Considerado — paths sao finitos e reutilizados. `string.Intern()` reduziria memoria mas adiciona complexidade. Nao necessario dado o volume baixo de paths unicos.

---

## R6. Padrao de Registro DI

**Decisao**: Extension method `AddBedrockConfiguration<TManager>()` em `IServiceCollection` com `Action<ConfigurationOptions>` para configuracao fluente.

**Racional**:
- Segue o padrao .NET de `Add*` extension methods para registro de servicos.
- O tipo generico `TManager` permite que cada aplicacao tenha seu ConfigurationManager concreto.
- `ConfigurationOptions` encapsula toda a configuracao de handlers, secoes e pipeline.
- Handlers sao registrados no container como Singleton (stateless) ou Scoped (stateful).

**Alternativas consideradas**:
1. *Self-contained ServiceProvider (como MigrationManagerBase)*: Rejeitado — configuracao precisa participar do container principal da aplicacao para resolver handlers que dependem de outros servicos.
2. *Registro manual sem extension method*: Possivel mas nao ergonomico. O extension method encapsula a complexidade de registrar manager + handlers + pipeline.
3. *Auto-discovery de handlers via assembly scanning*: Rejeitado — implicito demais (viola BB-IV). Registro explicito via fluent API e preferivel.

**Pattern**:
```
services.AddBedrockConfiguration<AppConfigurationManager>(options =>
{
    options.MapSection<PostgreSqlConfig>("Persistence:PostgreSql");

    options.AddHandler<KeyVaultHandler>()
        .AtPosition(1)
        .WithLoadStrategy(LoadStrategy.StartupOnly)
        .ToClass<PostgreSqlConfig>()
        .ToProperty(c => c.ConnectionString);
});
```

---

## R7. Mapeamento Secao-Classe

**Decisao**: O mapeamento entre classe de configuracao e secao do IConfiguration e feito explicitamente via `options.MapSection<TClass>(string sectionPath)` no ConfigureInternal.

**Racional**:
- Explicito e sem ambiguidade — o developer define qual classe mapeia para qual secao.
- O path completo de uma propriedade e: `sectionPath + ":" + propertyName`.
- O mapeamento e armazenado em um dicionario interno `Dictionary<Type, string>` (tipo → secao).

**Alternativas consideradas**:
1. *Convencao por nome da classe* (ex: `PostgreSqlConfig` → `PostgreSql`): Rejeitado — ambiguo (como derivar `Persistence:PostgreSql` de `PostgreSqlConfig`?). Requer convencoes frageis.
2. *Atributo `[ConfigurationSection("Persistence:PostgreSql")]`*: Considerado — declarativo e proximo da classe. Rejeitado porque mistura responsabilidade de mapeamento com a classe de configuracao (que deveria ser um POCO simples).
3. *Derivacao automatica do namespace*: Rejeitado — namespaces nao correspondem necessariamente a hierarquia de configuracao.

---

## R8. Suporte a Arrays e Nullable

**Decisao**: Delegado ao `IConfiguration.GetSection().Get<T>()` do Microsoft.Extensions.Configuration para binding de arrays e nullable.

**Racional**:
- O IConfiguration ja suporta binding de arrays (secoes indexadas: `Key:0`, `Key:1`, etc.) e tipos nullable nativamente.
- Reimplementar seria duplicacao. O ConfigurationManagerBase le o valor tipado do IConfiguration e entao passa pelo pipeline de handlers.
- Para arrays, o handler recebe o array ja resolvido como `object` e pode transforma-lo.

**Nota**: O tipo da propriedade e conhecido em compile-time via expression tree. O ConfigurationManagerBase usa `IConfiguration.GetSection(sectionPath).Get<T>()` para o binding inicial, onde `T` e o tipo da propriedade.

---

## R9. Grafo de Dependencias Atualizado

**Decisao**: Configuration depende apenas de Core. Posicao no grafo:

```
Core (zero dependencias)
  ↑
Configuration (← Core)
  ↑
Domain.Entities (← Core)
  ↑
...
```

**Pacotes NuGet**:
- `Microsoft.Extensions.Configuration.Abstractions` (ja em Directory.Packages.props)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (transitivo de DI, pode precisar adicao explicita)
- `Microsoft.Extensions.Logging.Abstractions` (ja em Directory.Packages.props)

**Nao depende de**: Observability (sem ExecutionContext), Domain, Data, Persistence.
