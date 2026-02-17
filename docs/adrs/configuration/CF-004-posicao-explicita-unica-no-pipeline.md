# CF-004: Handlers Devem Ter Posicao Explicita e Unica no Pipeline

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine uma receita de bolo onde as instrucoes nao tem ordem. "Adicione farinha", "bata os ovos", "unte a forma" — cada cozinheiro interpreta a ordem de forma diferente e o resultado varia. Agora imagine que cada passo tem um numero: (1) unte a forma, (2) bata os ovos, (3) adicione farinha. A receita e deterministica — sempre o mesmo resultado.

### O Problema Tecnico

Um pipeline de handlers de configuracao executa transformacoes em sequencia. A ordem importa:

1. **Descriptografar antes de validar**: Se a validacao rodar antes da descriptografia, ela validara o texto cifrado, nao o valor real.
2. **Cachear depois de transformar**: Se o cache rodar antes da transformacao, o valor cacheado sera o valor bruto.
3. **Nondeterminismo**: Sem ordem explicita, a execucao depende da ordem de registro — que pode mudar ao refatorar, adicionando fragilidade.

## Como Normalmente E Feito

### Abordagem Tradicional

Ordem implicita baseada em registro ou prioridade numerica sem validacao:

```csharp
// Middleware pattern — ordem depende da sequencia de .Use()
pipeline.Use<DecryptHandler>();
pipeline.Use<CacheHandler>();
pipeline.Use<ValidationHandler>();
// Se alguem inverter a ordem de duas linhas, comportamento muda silenciosamente
```

### Por Que Nao Funciona Bem

- **Ordem implicita**: Depende da posicao no codigo — refatoracao quebra o pipeline silenciosamente.
- **Sem validacao**: Dois handlers podem ter a mesma prioridade sem erro.
- **Sem visibilidade**: Nao ha como saber a ordem de execucao sem ler todo o codigo de registro.

## A Decisao

### Nossa Abordagem

Cada handler DEVE declarar uma posicao explicita e unica no pipeline via `.AtPosition(n)`:

```csharp
protected override void ConfigureInternal(ConfigurationOptions options)
{
    options.MapSection<DatabaseConfig>("Persistence:PostgreSql");

    // Posicoes explicitas — ordem de execucao visivel
    options.AddHandler<DecryptionHandler>()
        .AtPosition(1);                    // executa primeiro

    options.AddHandler<ValidationHandler>()
        .AtPosition(2)                     // executa segundo
        .ForGet();                         // so no pipeline de Get

    options.AddHandler<AuditHandler>()
        .AtPosition(3)                     // executa terceiro
        .ForBoth();                        // Get e Set
}
```

**Pipelines separados para Get e Set:**

```csharp
// Registrar handler so no Get
options.AddHandler<ReadOnlyHandler>().AtPosition(1).ForGet();

// Registrar handler so no Set
options.AddHandler<WriteOnlyHandler>().AtPosition(1).ForSet();

// Registrar em ambos (default)
options.AddHandler<BothHandler>().AtPosition(2).ForBoth();
```

**Validacao de duplicatas:**

```
BuildPipelines()
  ├→ Separa handlers por Get e Set
  ├→ ValidateNoDuplicatePositions(getEntries, "Get")
  │   └→ Se posicao duplicada → InvalidOperationException
  └→ ValidateNoDuplicatePositions(setEntries, "Set")
      └→ Se posicao duplicada → InvalidOperationException
```

**Regras fundamentais:**

1. **`AtPosition(n)` obrigatorio**: Toda posicao deve ser definida explicitamente.
2. **Posicao unica por pipeline**: Dentro do pipeline de Get, nao pode haver dois handlers com a mesma posicao. Idem para Set. Posicoes podem ser repetidas entre Get e Set (sao pipelines independentes).
3. **Execucao ordenada**: Handlers executam em ordem crescente de posicao (1, 2, 3...).
4. **Fail-fast**: `BuildPipelines()` valida duplicatas durante `Initialize()` — nao em runtime.
5. **Direcionalidade**: `.ForGet()`, `.ForSet()`, `.ForBoth()` controlam em quais pipelines o handler participa.

### Por Que Funciona Melhor

- **Determinismo**: A ordem de execucao e explicita e verificavel — nao depende da ordem de registro.
- **Seguranca contra duplicatas**: Posicoes duplicadas sao detectadas no startup.
- **Pipelines independentes**: Get e Set podem ter handlers diferentes e em ordens diferentes.
- **Legibilidade**: Olhando `ConfigureInternal`, a ordem de execucao e imediatamente visivel.

## Consequencias

### Beneficios

- Ordem de execucao deterministica e verificavel.
- Duplicatas detectadas em startup (fail-fast).
- Pipelines de Get e Set independentes — flexibilidade para handlers unidirecionais.
- Code agents geram handlers com posicoes claras e nao ambiguas.

### Trade-offs (Com Perspectiva)

- **Posicoes manuais**: O desenvolvedor escolhe os numeros. Na pratica, a convencao e usar incrementos de 1 ou 10 (para permitir insercoes futuras). Com 3-5 handlers por BC, gerenciar posicoes e trivial.
- **Sem reordenacao automatica**: Se remover o handler na posicao 2, as posicoes 1 e 3 permanecem — nao ha compactacao. Isso e intencional — posicoes sao contratos, nao indices.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Pipeline Pattern**: Handlers executam em sequencia, cada um transformando o valor para o proximo.
- **Chain of Responsibility** (GoF): Cada handler decide se e como transformar o valor — o pipeline garante a ordem.

### O Que o Clean Code Diz

> "Make your code read like well-written prose."
>
> *Faca seu codigo ler como prosa bem escrita.*

Robert C. Martin (2008). `AtPosition(1)`, `AtPosition(2)`, `AtPosition(3)` — a ordem e legivel como uma lista numerada.

### Outros Fundamentos

- **Principle of Least Surprise**: A ordem de execucao corresponde exatamente a ordem numerica declarada — sem surpresas.
- **Fail-Fast Principle**: Posicoes duplicadas sao erro de configuracao, detectado antes de qualquer request.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Por que posicoes explicitas sao melhores que ordem de registro para pipelines de configuracao?"
2. "Como pipelines separados de Get e Set permitem handlers unidirecionais?"
3. "Qual a convencao para numerar posicoes quando se espera adicionar handlers no futuro?"

### Leitura Recomendada

- GoF, *Design Patterns* (1994) — Chain of Responsibility
- Microsoft, [ASP.NET Core Middleware ordering](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Configuration | Define `ConfigurationHandlerBuilder.AtPosition()`, `ConfigurationOptions.BuildPipelines()`, `ConfigurationPipeline` |

## Referencias no Codigo

- Builder fluent: `src/BuildingBlocks/Configuration/Registration/ConfigurationHandlerBuilder.cs`
- Validacao de duplicatas: `src/BuildingBlocks/Configuration/Registration/ConfigurationOptions.cs` (metodo `ValidateNoDuplicatePositions`)
- Pipeline ordenado: `src/BuildingBlocks/Configuration/Pipeline/ConfigurationPipeline.cs`
- ADR relacionada: [CF-003 — Handler Herda ConfigurationHandlerBase](./CF-003-handler-herda-configurationhandlerbase.md)
