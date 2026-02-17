# CF-002: ConfigureInternal Deve Registrar Secoes com MapSection

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine um cartorio que registra imoveis. Antes de qualquer escritura ser lavrada, o imovel deve existir no registro geral — com matricula, endereco e proprietario. Se alguem tentar lavrar uma escritura de compra e venda sem o imovel estar registrado, o cartorio recusa. Essa validacao previa garante que nenhuma operacao acontece sobre algo que o sistema nao conhece.

### O Problema Tecnico

O `ConfigurationManagerBase` precisa saber quais secoes do `IConfiguration` correspondem a quais classes tipadas. Sem esse mapeamento previo:

1. **Paths nao resolvidos**: `Get<DatabaseConfig>()` nao saberia qual secao do `IConfiguration` ler (e `"Persistence:PostgreSql"` ou `"Database"` ou `"ConnectionStrings"`?).
2. **Handlers orfaos**: Um handler registrado com `.ToClass<DatabaseConfig>()` precisa saber o path da secao para construir o escopo — sem o mapeamento, o escopo nao pode ser definido.
3. **Erros em runtime**: Sem validacao no startup, o sistema so descobriria o problema quando alguem chamasse `Get<T>()` — potencialmente em producao.

## Como Normalmente E Feito

### Abordagem Tradicional

Em `IOptions<T>`, o bind e feito no startup via `services.Configure<T>(config.GetSection("path"))`. O problema e que o path e uma string solta, sem relacao com o tipo:

```csharp
// Startup.cs — nenhuma validacao de que "Database" existe no appsettings
services.Configure<DatabaseConfig>(config.GetSection("Database"));
services.Configure<CacheConfig>(config.GetSection("Cache"));

// Meses depois, alguem renomeia "Database" para "Persistence:PostgreSql"
// no appsettings.json mas esquece de atualizar aqui.
// Resultado: DatabaseConfig com todos os valores default (null, 0, false).
```

### Por Que Nao Funciona Bem

- **Desconexao entre tipo e path**: O bind e uma string solta que nao tem relacao verificavel com o tipo.
- **Falha silenciosa**: Se a secao nao existir, `IOptions<T>` retorna uma instancia com valores default — sem erro, sem aviso.
- **Sem validacao cruzada**: Nao ha como garantir em startup que todos os tipos tem secoes validas.

## A Decisao

### Nossa Abordagem

O `ConfigureInternal` DEVE registrar todas as secoes via `MapSection<T>()` ANTES de registrar handlers:

```csharp
protected override void ConfigureInternal(ConfigurationOptions options)
{
    // PRIMEIRO: Mapear secoes para classes tipadas
    options.MapSection<DatabaseConfig>("Persistence:PostgreSql");
    options.MapSection<CacheConfig>("Persistence:Redis");
    options.MapSection<FeatureFlags>("FeatureManagement");

    // DEPOIS: Registrar handlers (que podem referenciar as secoes)
    options.AddHandler<DecryptionHandler>()
        .AtPosition(1)
        .ToClass<DatabaseConfig>()           // ← referencia a secao mapeada
        .ToProperty(x => x.ConnectionString);

    options.AddHandler<CacheHandler>()
        .AtPosition(2)
        .ToClass<CacheConfig>();             // ← referencia a secao mapeada
}
```

**Regras fundamentais:**

1. **`MapSection<T>(path)` obrigatorio**: Toda classe de configuracao usada em `Get<T>()` deve ter sido mapeada.
2. **Ordem**: `MapSection` antes de `AddHandler` — handlers que usam `.ToClass<T>()` dependem do mapeamento existir.
3. **Fail-fast**: `Get<T>()` lanca `InvalidOperationException` se `T` nao foi mapeado. `ToClass<T>()` lanca `InvalidOperationException` se `T` nao foi mapeado.
4. **Path unico por tipo**: Cada tipo so pode ser mapeado para uma secao. Mapeamentos duplicados sobrescrevem o anterior.

**Validacao em cascata:**

```
MapSection<DatabaseConfig>("Persistence:PostgreSql")
  └→ Registra: typeof(DatabaseConfig) → "Persistence:PostgreSql"

AddHandler<X>().ToClass<DatabaseConfig>()
  └→ Lookup: typeof(DatabaseConfig) → "Persistence:PostgreSql" ✓
  └→ Escopo: HandlerScope.ForClass("Persistence:PostgreSql")

Get<DatabaseConfig>()
  └→ Lookup: typeof(DatabaseConfig) → "Persistence:PostgreSql" ✓
  └→ Para cada propriedade: resolve "Persistence:PostgreSql:{PropertyName}"
```

### Por Que Funciona Melhor

- **Fail-fast**: Erros de configuracao sao detectados no startup, nao em runtime.
- **Ponto unico de verdade**: O mapeamento tipo→secao existe em um unico lugar (`ConfigureInternal`).
- **Handlers dependentes**: `.ToClass<T>()` valida que `T` foi mapeado — impossivel registrar um handler para uma secao inexistente.
- **Resolucao automatica**: `Get<T>()` resolve paths completos (`Secao:Propriedade`) automaticamente a partir do mapeamento.

## Consequencias

### Beneficios

- Configuracao centralizada e previsivel em `ConfigureInternal`.
- Erros de mapeamento detectados em startup.
- Eliminacao de magic strings fora do `ConfigureInternal` — o path e registrado uma vez e usado automaticamente.
- Code agents sabem que devem comecar por `MapSection` antes de qualquer `AddHandler`.

### Trade-offs (Com Perspectiva)

- **Registro manual**: Cada secao deve ser registrada explicitamente. Na pratica, um BC tipico tem 3-5 secoes de configuracao — o esforco e minimo e a clareza compensa.
- **Nao ha auto-discovery**: Nao ha scanning automatico de classes de configuracao. Isso e intencional — mapeamentos explicitos sao mais faceis de rastrear e debugar.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Registry Pattern**: O `_sectionMappings` atua como registry central de mapeamentos tipo→secao.
- **Fail-Fast Principle**: Erros sao detectados o mais cedo possivel (startup), nao o mais tarde (primeiro uso).

### O Que o DDD Diz

> "Make the implicit explicit."
>
> *Torne o implicito explicito.*

Evans (2003). O mapeamento entre tipo e secao do `IConfiguration` e tornado explicito via `MapSection` — nao depende de convencoes de nomes implicitas.

### O Que o Clean Code Diz

> "If you must do something dangerous, at least do it in a controlled way."
>
> *Se voce deve fazer algo perigoso, pelo menos faca de forma controlada.*

Robert C. Martin (2008). Strings de configuracao sao perigosas (rename silencioso, typos). Concentrar todas em `ConfigureInternal` minimiza a superficie de erro.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Por que o mapeamento tipo→secao deve ser explicito e nao por convencao de nomes?"
2. "Como o fail-fast do MapSection evita erros silenciosos em producao?"
3. "Qual a diferenca entre MapSection e services.Configure<T>() do ASP.NET?"

### Leitura Recomendada

- Martin Fowler, *Patterns of Enterprise Application Architecture* (2002) — Registry
- Jim Shore, [Fail Fast](https://www.martinfowler.com/ieeeSoftware/failFast.pdf) (2004)
- Microsoft, [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Configuration | Define `ConfigurationOptions.MapSection<T>()` e `ConfigurationManagerBase.GetSectionPath()` |

## Referencias no Codigo

- Fluent API: `src/BuildingBlocks/Configuration/Registration/ConfigurationOptions.cs`
- Resolucao de path: `src/BuildingBlocks/Configuration/ConfigurationManagerBase.cs` (metodo `GetSectionPath`)
- ADR relacionada: [CF-001 — ConfigurationManager Herda ConfigurationManagerBase](./CF-001-manager-herda-configurationmanagerbase.md)
