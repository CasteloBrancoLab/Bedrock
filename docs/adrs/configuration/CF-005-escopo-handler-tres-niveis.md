# CF-005: Escopo de Handler Define Quais Chaves Sao Processadas

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine um sistema de seguranca de um edificio. Existem tres niveis de acesso: (1) o seguranca da portaria confere todas as pessoas que entram — escopo global; (2) o seguranca do andar confere apenas quem vai para aquele andar — escopo por secao; (3) a fechadura biometrica de uma sala confere apenas quem quer entrar naquela sala especifica — escopo por propriedade. Cada nivel filtra de forma diferente, e a combinacao dos tres garante seguranca granular sem redundancia.

### O Problema Tecnico

Handlers de configuracao frequentemente precisam atuar apenas sobre um subconjunto das chaves:

1. **Handler de descriptografia**: Deve atuar apenas na `ConnectionString`, nao em todas as propriedades.
2. **Handler de validacao**: Deve validar todas as propriedades de uma secao especifica.
3. **Handler de logging**: Deve registrar acesso a qualquer configuracao — escopo global.

Sem escopo, todos os handlers processam todas as chaves — desperdicio de processamento e risco de transformacoes indesejadas.

## Como Normalmente E Feito

### Abordagem Tradicional

Filtragem manual dentro de cada handler:

```csharp
public class DecryptHandler : IConfigHandler
{
    public object? Process(string key, object? value)
    {
        // Filtragem manual — cada handler repete isso
        if (key != "Persistence:PostgreSql:ConnectionString")
            return value;

        return Decrypt(value);
    }
}
```

### Por Que Nao Funciona Bem

- **Filtragem duplicada**: Cada handler reimplementa a logica de "devo processar esta chave?".
- **Magic strings no handler**: O handler conhece paths de configuracao — acoplamento entre handler e estrutura do config.
- **Erro silencioso**: Se o path mudar, o handler para de atuar sem erro.
- **Sem granularidade padrao**: Nao ha vocabulario comum para expressar "todas as chaves", "chaves de uma secao" ou "uma chave especifica".

## A Decisao

### Nossa Abordagem

O escopo e definido na fluent API de registro, nao no handler. Tres niveis hierarquicos:

```csharp
protected override void ConfigureInternal(ConfigurationOptions options)
{
    options.MapSection<DatabaseConfig>("Persistence:PostgreSql");

    // GLOBAL — processa todas as chaves de todas as secoes
    options.AddHandler<LoggingHandler>()
        .AtPosition(1);
        // Sem .ToClass() → escopo Global (default)

    // CLASS — processa todas as propriedades de DatabaseConfig
    options.AddHandler<ValidationHandler>()
        .AtPosition(2)
        .ToClass<DatabaseConfig>();
        // Escopo: "Persistence:PostgreSql:*"

    // PROPERTY — processa apenas ConnectionString
    options.AddHandler<DecryptionHandler>()
        .AtPosition(3)
        .ToClass<DatabaseConfig>()
        .ToProperty(x => x.ConnectionString);
        // Escopo: "Persistence:PostgreSql:ConnectionString" (exato)
}
```

**Modelo de escopo (`HandlerScope`):**

| Nivel | ScopeType | Matching | Exemplo |
|-------|-----------|----------|---------|
| Global | `ScopeType.Global` | Todas as chaves | `*` |
| Class | `ScopeType.Class` | Chaves que iniciam com `sectionPath:` | `Persistence:PostgreSql:*` |
| Property | `ScopeType.Property` | Chave exata | `Persistence:PostgreSql:ConnectionString` |

**Algoritmo de matching:**

```csharp
public bool Matches(string key) => ScopeType switch
{
    ScopeType.Global   => true,
    ScopeType.Class    => key.StartsWith(PathPattern, Ordinal)
                          && key.Length > PathPattern.Length
                          && key[PathPattern.Length] == ':',
    ScopeType.Property => string.Equals(key, PathPattern, Ordinal),
    _                  => false
};
```

**Detalhe importante do matching de Class**: Nao basta `StartsWith` — a chave deve ter um `:` apos o path da secao. Isso evita falso positivo: `"Persistence:PostgreSqlExtra:Foo"` nao casa com o escopo de `"Persistence:PostgreSql"`.

**Regras fundamentais:**

1. **Escopo no registro, nao no handler**: O handler nao filtra chaves — o pipeline filtra baseado no `HandlerScope`.
2. **Default e Global**: Se `.ToClass<T>()` nao for chamado, o handler atua em todas as chaves.
3. **`.ToProperty()` refina `.ToClass()`**: Nao e possivel definir escopo de propriedade sem definir a classe primeiro.
4. **Type-safe**: `.ToProperty(x => x.ConnectionString)` usa expression tree — sem magic strings.
5. **`HandlerScope` e value object**: Imutavel, `IEquatable<HandlerScope>`, comparacao por valor.

### Por Que Funciona Melhor

- **Separacao de responsabilidades**: O handler faz transformacao. O escopo faz filtragem. Nao se misturam.
- **Type safety**: `.ToClass<T>()` valida que `T` foi mapeado. `.ToProperty(x => x.Prop)` usa expression tree.
- **Eficiencia**: Handlers com escopo Property ou Class sao ignorados para chaves fora do escopo — zero overhead.
- **Granularidade padronizada**: Tres niveis cobrem todos os cenarios comuns sem complexidade excessiva.

## Consequencias

### Beneficios

- Handlers simples e focados — so implementam transformacao, sem filtragem.
- Escopo declarativo e type-safe na fluent API.
- Tres niveis cobrem desde "todas as configs" ate "uma propriedade especifica".
- Matching eficiente via `HandlerScope.Matches()` — sem regex, sem alocacoes.

### Trade-offs (Com Perspectiva)

- **Tres niveis apenas**: Nao ha escopo intermediario (ex: "duas propriedades de uma secao"). Na pratica, registrar dois handlers com escopo Property e mais claro e explicito do que inventar escopos customizados.
- **Escopo imutavel**: Uma vez registrado, o escopo nao muda. Isso e intencional — o escopo e configuracao estatica definida no startup.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Strategy Pattern** (GoF): `HandlerScope` e a estrategia de matching — o pipeline delega a decisao de "devo processar?" para o scope.
- **Value Object** (DDD): `HandlerScope` e imutavel, comparavel por valor, sem identidade propria — um value object classico.
- **Specification Pattern** (Fowler): `Matches(key)` e uma especificacao que filtra chaves.

### O Que o DDD Diz

> "Value Objects are things that matter only by the combination of their attributes."
>
> *Value Objects sao coisas que importam apenas pela combinacao de seus atributos.*

Evans (2003). `HandlerScope` e definido por `ScopeType` + `PathPattern` — dois scopes com os mesmos atributos sao identicos.

### Outros Fundamentos

- **Single Responsibility Principle** (SOLID): Handler = transformacao. Scope = filtragem. Duas responsabilidades, duas classes.
- **Tell, Don't Ask**: O pipeline nao pergunta ao handler "voce quer processar esta chave?" — ele verifica o scope e decide.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Por que o escopo de Class usa StartsWith + verificacao de ':' ao inves de apenas StartsWith?"
2. "Como o HandlerScope implementa o padrao Value Object com IEquatable?"
3. "Quando usar escopo Global vs. Class vs. Property?"

### Leitura Recomendada

- Eric Evans, *Domain-Driven Design* (2003), Cap. 5 — Value Objects
- Martin Fowler, [Specification Pattern](https://www.martinfowler.com/apsupp/spec.pdf)
- GoF, *Design Patterns* (1994) — Strategy

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Configuration | Define `HandlerScope`, `ScopeType`, `ClassScopeBuilder<T>`, `ConfigurationHandlerBuilder<T>.ToClass()` |

## Referencias no Codigo

- Value object: `src/BuildingBlocks/Configuration/Handlers/HandlerScope.cs`
- Fluent API de escopo: `src/BuildingBlocks/Configuration/Registration/ConfigurationHandlerBuilder.cs`
- Matching no pipeline: `src/BuildingBlocks/Configuration/Pipeline/ConfigurationPipeline.cs` (metodo `ExecuteGet`)
- ADR relacionada: [CF-002 — MapSection](./CF-002-configurar-secoes-com-mapsection.md) (`.ToClass<T>()` depende de `MapSection`)
