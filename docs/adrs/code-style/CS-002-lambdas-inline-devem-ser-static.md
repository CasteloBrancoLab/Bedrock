# CS-002: Lambdas Inline Devem Ser Static em Metodos do Projeto

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fabrica onde cada operario pode guardar ferramentas no
bolso enquanto trabalha na linha de montagem. Parece pratico, mas
cria dois problemas: (1) ferramentas no bolso ocupam espaco e pesam
— o operario fica mais lento; (2) se o operario trocar de estacao,
as ferramentas vao junto mesmo sem necessidade — acoplamento
invisivel. Agora imagine que a fabrica proibe bolsos: todas as
ferramentas ficam em bancadas nomeadas ao lado de cada estacao. O
operario pega o que precisa, usa e devolve. Zero peso extra, zero
surpresas ao trocar de estacao.

### O Problema Tecnico

Em C#, uma lambda que captura variaveis do escopo externo (closure)
faz o compilador gerar uma classe anonima no heap para armazenar
essas variaveis. Isso causa:

1. **Alocacao no heap**: Cada invocacao cria uma instancia da classe
   de closure — pressao no GC, especialmente em hot paths.
2. **Acoplamento invisivel**: A lambda carrega estado do escopo
   externo que nao e visivel na assinatura. Bugs sutis surgem quando
   o estado capturado muda entre a criacao e a execucao da lambda.
3. **Dificuldade de rastreamento**: Lambdas anonimas nao aparecem
   com nomes significativos em stack traces, profilers ou
   "Find All References".

O modificador `static` em lambdas (C# 9+) proibe a captura de
variaveis, forcando que todo contexto seja passado explicitamente
via parametros do delegate — exatamente como o padrao
Clone-Modify-Return das entidades de dominio.

## Como Normalmente E Feito

### Abordagem Tradicional

A maioria dos projetos C# usa lambdas livremente, sem restricao
sobre captura de variaveis:

```csharp
// Lambda com closure — captura 'input' e 'logger' do escopo
entity.RegisterChangeInternal(ctx, input, (clone, c, i) =>
{
    logger.Log("Changing..."); // captura 'logger'
    return clone.ChangeStatusInternal(c, i);
});
```

### Por Que Nao Funciona Bem

- A cada chamada, o compilador aloca uma instancia da classe de
  closure para armazenar `logger`.
- O leitor do codigo nao sabe quais variaveis estao sendo capturadas
  sem inspecionar o corpo da lambda.
- Em frameworks como o Bedrock, onde metodos de entidade sao
  chamados milhares de vezes, closures se acumulam e degradam
  performance.
- Code agents (LLMs) geram closures por padrao — sem regra
  explicita, cada lambda gerada potencialmente captura estado.

## A Decisao

### Nossa Abordagem

Toda lambda ou anonymous delegate inline passada como argumento a
um metodo cujo tipo pertence ao namespace raiz do projeto DEVE usar
o modificador `static`:

```csharp
// Correto — static lambda, contexto via parametros
entity.RegisterChangeInternal(ctx, input,
    static (clone, c, i) => clone.ChangeStatusInternal(c, i));

// Incorreto — lambda sem static (possivel closure)
entity.RegisterChangeInternal(ctx, input,
    (clone, c, i) => clone.ChangeStatusInternal(c, i));
```

### Escopo da Regra

| Aspecto | Definicao |
|---------|-----------|
| Alvo | Lambdas e anonymous delegates inline como argumento |
| Condicao | Tipo dono do metodo pertence ao namespace raiz do projeto |
| Violacao | Lambda sem modificador `static` |
| Method groups | Ignorados (seguros por natureza — nao capturam locals) |
| Lambdas em variaveis | Fora do escopo (data flow impraticavel) |
| Metodos externos | Ignorados (LINQ, Moq, Shouldly, xUnit, etc.) |

### Deteccao do Namespace Raiz

O namespace raiz NAO e hardcoded. A regra descobre o namespace raiz
dinamicamente a partir dos projetos configurados na fixture do teste
de arquitetura (`GetProjectPaths()`). Isso permite que qualquer
projeto que use o BuildingBlock `Testing.Architecture` herde a regra
automaticamente, independente do namespace base (Bedrock, ShopDemo,
MeuProjeto, etc.).

### Supressao via Comentario

Para casos legitimos onde uma closure e necessaria, a regra pode
ser suprimida com comentarios no mesmo padrao do Stryker:

```csharp
// CS002 disable once : delegate de logging externo requer closure
obj.Method((x, y) => logger.Log(x));

// Ou para bloco:
// CS002 disable : integracao com API legada que exige closure
obj.Method1((x) => legacyState.Process(x));
obj.Method2((x) => legacyState.Finalize(x));
// CS002 restore
```

Requisitos para supressao:
- Justificativa DEVE ser em pt-BR.
- DEVE explicar *por que* a closure e necessaria.
- Preferir `disable once` sobre `disable/restore` quando possivel.

### Por Que Funciona Melhor

1. **Zero alocacoes**: `static` lambdas nao geram classes de
   closure — o compilador garante em tempo de compilacao.
2. **Contexto explicito**: Todo dado necessario DEVE ser passado
   via parametros do delegate, tornando dependencias visiveis.
3. **Consistencia com Domain Entities**: O padrao
   `RegisterChangeInternal` e `RegisterNewInternal` ja usa static
   lambdas. A regra generaliza para todo o projeto.
4. **Code agents seguros**: Com a regra Roslyn, qualquer lambda
   gerada por LLM que capture estado e detectada automaticamente.

## Consequencias

### Beneficios

- Zero closures em hot paths — alinhado com BB-I (Performance).
- Dependencias de cada lambda sao explicitas na assinatura.
- Regra Roslyn (CS002) verifica compliance automaticamente.
- Padrao ja estabelecido nas entidades de dominio e generalizado
  para todo o projeto.

### Trade-offs (Com Perspectiva)

- **Verbosidade**: Lambdas `static` exigem que todo contexto seja
  passado via parametros. Para lambdas simples isso e minimo; para
  lambdas complexas, e exatamente o que queremos (contexto
  explicito).
- **Aprendizado**: Desenvolvedores acostumados com closures
  precisam adaptar-se. Na pratica, o compilador informa
  imediatamente quando uma `static` lambda tenta capturar.
- **Casos de supressao**: Integracoes com APIs externas ou cenarios
  excepcionais podem exigir closures. O mecanismo de supressao
  com justificativa cobre esses casos.

## Fundamentacao Teorica

### Padroes de Design Relacionados

**Explicit Dependencies Principle**: Toda dependencia de um
componente deve ser visivel na sua interface. A `static` lambda
materializa esse principio: se precisa de um dado, recebe como
parametro.

**Zero-Allocation Pattern**: Em frameworks de alta performance
(.NET, game engines), closures sao um dos principais ofensores de
alocacoes involuntarias. A proibicao de closures elimina essa
categoria inteira de alocacao.

### O Que o .NET Performance Guidelines Diz

> "Avoid closures over instance members in hot paths. Use static
> lambdas or local functions to prevent unintended allocations."
>
> *Evite closures sobre membros de instancia em hot paths. Use
> lambdas static ou funcoes locais para prevenir alocacoes
> involuntarias.*

### Outros Fundamentos

**Principle of Least Surprise**: Uma `static` lambda nunca captura
estado externo — o leitor sabe exatamente com o que a lambda
trabalha apenas lendo seus parametros.

## Aprenda Mais

### Perguntas Para Fazer a LLM

- "Como o compilador C# implementa closures e qual o impacto em
  alocacoes de heap?"
- "Qual a diferenca entre uma static lambda e uma lambda normal
  em termos de IL gerado?"
- "Quais frameworks .NET de alta performance proibem closures e
  como fazem isso?"

### Leitura Recomendada

- [Static anonymous functions — C# 9](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/static-anonymous-functions)
- [Performance: avoid closures — .NET docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1859)
- [Dissecting the pattern — closures in C#](https://devblogs.microsoft.com/premier-developer/dissecting-the-local-functions-in-c-7/)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Testing.Architecture | Implementa regra Roslyn CS002 que verifica esta convencao |
| Domain.Entities | Padrao original: `RegisterChangeInternal` e `RegisterNewInternal` usam static lambdas |
| Core | Candidato a revisao: verificar se APIs existentes usam closures desnecessarias |
| Security | Candidato a revisao: PasswordPolicy e outros componentes |

## Referencias no Codigo

- `src/BuildingBlocks/Testing/Architecture/Rules/CodeStyleRules/CS002_StaticLambdasInProjectMethodsRule.cs` — Regra Roslyn (a implementar)
- `src/Templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs` — Exemplo do padrao static lambda em RegisterChangeInternal
- `src/Templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs` — Exemplo do padrao static lambda em RegisterNewInternal
