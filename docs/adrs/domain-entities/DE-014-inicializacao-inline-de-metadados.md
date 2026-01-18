# DE-014: Inicialização Inline de Metadados

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um formulário de cadastro com um **manual de instruções separado**:

**Opção A - Valores em lugar separado**:
O formulário lista os campos, mas para saber os limites você precisa abrir outro documento: "Consulte a página 47 do manual para saber o tamanho máximo do nome".

**Opção B - Valores no próprio campo**:
Cada campo do formulário já mostra "Máx: 100 caracteres" diretamente ao lado. Tudo visível de uma olhada.

Em código, construtores estáticos são o "manual separado" - você vê a declaração em um lugar, mas precisa procurar outro lugar para saber o valor. Inicialização inline é o "valor ao lado" - declaração e valor juntos.

---

### O Problema Técnico

C# oferece duas formas de inicializar membros estáticos:

```csharp
// Opção A: Construtor estático (cctor)
public static class PersonMetadata
{
    public static int FirstNameMaxLength { get; private set; }
    public static int LastNameMaxLength { get; private set; }
    public static bool FirstNameIsRequired { get; private set; }

    // Valores definidos em outro lugar - precisa scroll/busca
    static PersonMetadata()
    {
        FirstNameMaxLength = 100;
        LastNameMaxLength = 50;
        FirstNameIsRequired = true;
    }
}

// Opção B: Inicialização inline
public static class PersonMetadata
{
    // Valor visível imediatamente - zero scroll
    public static int FirstNameMaxLength { get; private set; } = 100;
    public static int LastNameMaxLength { get; private set; } = 50;
    public static bool FirstNameIsRequired { get; private set; } = true;
}
```

O construtor estático cria **fragmentação visual**: declaração em um lugar, valor em outro.

## A Decisão

### Nossa Abordagem

**SEMPRE** inicialize metadados inline (não em construtores estáticos):

```csharp
public static class SimpleAggregateRootMetadata
{
    // ? Inline: valor visível ao lado da declaração
    public static readonly string FirstNamePropertyName = nameof(FirstName);
    public static bool FirstNameIsRequired { get; private set; } = true;
    public static int FirstNameMinLength { get; private set; } = 1;
    public static int FirstNameMaxLength { get; private set; } = 255;

    public static readonly string LastNamePropertyName = nameof(LastName);
    public static bool LastNameIsRequired { get; private set; } = true;
    public static int LastNameMinLength { get; private set; } = 1;
    public static int LastNameMaxLength { get; private set; } = 255;

    public static readonly string BirthDatePropertyName = nameof(BirthDate);
    public static bool BirthDateIsRequired { get; private set; } = true;
    public static int BirthDateMinAgeInYears { get; private set; } = 0;
    public static int BirthDateMaxAgeInYears { get; private set; } = 150;
}
```

### Antipadrão: Construtor Estático

```csharp
// ? NÃO FAÇA: Construtor estático
public static class SimpleAggregateRootMetadata
{
    // Declarações sem valores - precisa procurar o cctor
    public static readonly string FirstNamePropertyName;
    public static bool FirstNameIsRequired { get; private set; }
    public static int FirstNameMinLength { get; private set; }
    public static int FirstNameMaxLength { get; private set; }

    public static readonly string LastNamePropertyName;
    public static bool LastNameIsRequired { get; private set; }
    public static int LastNameMinLength { get; private set; }
    public static int LastNameMaxLength { get; private set; }

    // Valores em lugar separado - ruim para code review
    static SimpleAggregateRootMetadata()
    {
        FirstNamePropertyName = nameof(FirstName);
        FirstNameIsRequired = true;
        FirstNameMinLength = 1;
        FirstNameMaxLength = 255;

        LastNamePropertyName = nameof(LastName);
        LastNameIsRequired = true;
        LastNameMinLength = 1;
        LastNameMaxLength = 255;
    }
}
```

### Valores Derivados Também São Inline

Mesmo valores que dependem de outros metadados devem ser inline:

```csharp
public static class SimpleAggregateRootMetadata
{
    // FirstName
    public static int FirstNameMinLength { get; private set; } = 1;
    public static int FirstNameMaxLength { get; private set; } = 255;

    // LastName
    public static int LastNameMinLength { get; private set; } = 1;
    public static int LastNameMaxLength { get; private set; } = 255;

    // FullName - valores derivados, mas ainda inline
    public static int FullNameMinLength { get; private set; } = FirstNameMinLength + LastNameMinLength + 1; // +1 para espaço
    public static int FullNameMaxLength { get; private set; } = FirstNameMaxLength + LastNameMaxLength + 1; // +1 para espaço
}
```

### Benefícios

1. **Valor visível ao lado da declaração**:
   - Sem scroll para encontrar inicialização
   - Code review mais rápido
   - Menos carga cognitiva

2. **Agrupamento natural**:
   - Propriedade e seus constraints ficam juntos
   - Facilita manutenção

3. **Refactoring mais seguro**:
   - Renomear propriedade atualiza tudo no mesmo lugar
   - Menos chance de esquecer atualizar o cctor

4. **Performance equivalente**:
   - CLR inicializa inline antes do cctor de qualquer forma
   - Não há diferença de performance

### Trade-offs (Com Perspectiva)

- **Valores muito longos**: Expressões complexas podem ficar longas em uma linha

Para valores derivados complexos, extraia para métodos privados:

```csharp
// Se a expressão for muito complexa
public static int FullNameMaxLength { get; private set; } = CalculateFullNameMaxLength();

private static int CalculateFullNameMaxLength()
    => FirstNameMaxLength + LastNameMaxLength + 1;
```

Mas prefira inline simples sempre que possível.

### Trade-offs Frequentemente Superestimados

**"Construtor estático é mais organizado"**

Na verdade, é o oposto:

```csharp
// Com cctor - precisa de dois lugares para entender
// Lugar 1: declarações
public static int FirstNameMaxLength { get; private set; }
public static int LastNameMaxLength { get; private set; }
// ... mais 20 propriedades ...

// Lugar 2: valores (50 linhas abaixo)
static PersonMetadata()
{
    FirstNameMaxLength = 100;
    LastNameMaxLength = 50;
    // ... mais 20 atribuições ...
}

// Com inline - tudo junto
public static int FirstNameMaxLength { get; private set; } = 100;
public static int LastNameMaxLength { get; private set; } = 50;
```

**"Construtor estático permite lógica condicional"**

Se você precisa de lógica condicional na inicialização, provavelmente está fazendo algo errado. Metadados devem ter valores padrão simples, alteráveis via `Change*Metadata()` no startup (ver [DE-015](./DE-015-customizacao-de-metadados-apenas-no-startup.md)).

**"Construtor estático garante thread-safety"**

Inicialização inline também é thread-safe - o CLR garante que campos estáticos são inicializados antes de qualquer acesso.

## Fundamentação Teórica

### C# Language Specification

A especificação do C# define que inicializadores de campo são executados na ordem textual, antes do construtor estático:

> "Static field initializers are executed in the textual order in which they appear in the class declaration."
>
> "Inicializadores de campos estáticos são executados na ordem textual em que aparecem na declaração da classe."
> — C# Language Specification

Isso significa que:

```csharp
public static int A { get; private set; } = 10;
public static int B { get; private set; } = A + 5; // B = 15 (funciona!)
```

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre organização de código:

> "Variables should be declared as close to their usage as possible."
>
> *Variáveis devem ser declaradas o mais próximo possível de seu uso.*

Inicialização inline segue este princípio - o valor inicial está junto à declaração, não em outro lugar do arquivo.

O princípio de **minimizar distância vertical**:

> "Concepts that are closely related should be kept vertically close to each other."
>
> *Conceitos intimamente relacionados devem ser mantidos verticalmente próximos uns dos outros.*

Declaração e inicialização são conceitos intimamente relacionados - separá-los viola este princípio.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre expressividade do modelo:

> "If the design of the model is sloppy, the code will be sloppy and hard to understand."
>
> *Se o design do modelo for desleixado, o código será desleixado e difícil de entender.*

Inicialização espalhada em construtores estáticos é "design desleixado" - dificulta entendimento do modelo.

### Performance: Inline vs Construtor Estático

O CLR trata ambos de forma equivalente em termos de performance. A diferença está apenas na **legibilidade e manutenibilidade** do código.

### Princípio da Localidade (Locality of Reference)

> "Code is easier to understand when related information is kept together."
>
> *Código é mais fácil de entender quando informações relacionadas são mantidas juntas.*
> — Code Complete, Steve McConnell

Separar declaração de inicialização viola o princípio da localidade e força o leitor a "caçar" informações pelo arquivo.

## Antipadrões Documentados

### Antipadrão 1: Misturar Inline e Construtor Estático

```csharp
// ? Inconsistente - alguns inline, alguns no cctor
public static class PersonMetadata
{
    public static int FirstNameMaxLength { get; private set; } = 100; // Inline
    public static int LastNameMaxLength { get; private set; }         // Cctor

    static PersonMetadata()
    {
        LastNameMaxLength = 50; // Confuso - por que este é diferente?
    }
}
```

### Antipadrão 2: Lógica Complexa no Construtor Estático

```csharp
// ? Lógica demais no cctor
static PersonMetadata()
{
    var config = LoadConfigFromFile(); // IO no cctor!
    FirstNameMaxLength = config.GetValue("FirstNameMaxLength");

    if (Environment.GetEnvironmentVariable("TENANT") == "enterprise")
    {
        FirstNameMaxLength = 500; // Lógica condicional no cctor
    }
}
```

Use `Change*Metadata()` no startup ao invés de lógica no cctor.

### Antipadrão 3: Valores Padrão Não-Óbvios

```csharp
// ? Valor padrão escondido ou não-óbvio
public static int FirstNameMaxLength { get; private set; } // Qual é o default? 0?

// ? Valor padrão explícito
public static int FirstNameMaxLength { get; private set; } = 255;
```

## Decisões Relacionadas

- [DE-012](./DE-012-metadados-estaticos-vs-data-annotations.md) - Por que usar propriedades estáticas
- [DE-013](./DE-013-nomenclatura-de-metadados.md) - Convenção de nomenclatura
- [DE-015](./DE-015-customizacao-de-metadados-apenas-no-startup.md) - Customização apenas no startup

## Leitura Recomendada

- [C# Language Specification - Static Field Initialization](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#static-field-initialization)
- [Clean Code - Chapter 2: Meaningful Names](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)
- [Principle of Locality in Programming](https://en.wikipedia.org/wiki/Locality_of_reference)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Estabelece o padrão de inicialização inline de metadados para garantir valores default consistentes |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Inline Initialization de Metadados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Metadados com inicialização inline
