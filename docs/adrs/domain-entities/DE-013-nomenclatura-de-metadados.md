# DE-013: Nomenclatura de Metadados (PropertyName + ConstraintType)

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma biblioteca com livros organizados de duas formas:

**Biblioteca A - Nomes aleatórios**:
Prateleiras com etiquetas como "Livros verdes", "Coisas do João", "Pilha 3". Para encontrar algo, você precisa conhecer o sistema pessoal de quem organizou.

**Biblioteca B - Sistema Dewey Decimal**:
Todo livro tem um código estruturado: 823.912 = Literatura (8) ? Inglesa (23) ? Ficção (912). Qualquer pessoa pode entender e usar o sistema sem treinamento especial.

Para metadados de validação, precisamos do "Sistema Dewey" - uma convenção previsível que qualquer desenvolvedor (ou LLM) possa entender e aplicar consistentemente.

### O Problema Técnico

Sem convenção de nomenclatura, cada desenvolvedor nomeia metadados à sua maneira:

```csharp
// Desenvolvedor A
public static int FirstName_MaxLen { get; } = 100;
public static bool FirstName_Req { get; } = true;

// Desenvolvedor B
public static int MaxLengthFirstName { get; } = 100;
public static bool RequiredFirstName { get; } = true;

// Desenvolvedor C
public static int FNameMax { get; } = 100;
public static bool FNameReq { get; } = true;

// Desenvolvedor D
public static int FIRST_NAME_MAX_LENGTH { get; } = 100;
public static bool IS_FIRST_NAME_REQUIRED { get; } = true;
```

Problemas:
1. **Inconsistência**: Cada classe tem estilo diferente
2. **Imprevisibilidade**: Qual é o nome do metadado de MaxLength para Email?
3. **Busca difícil**: Como encontrar todos os "IsRequired" se cada um nomeia diferente?
4. **LLMs falham**: Modelos de linguagem não conseguem inferir o padrão

## Como Normalmente é Feito

### Abordagem Tradicional

Projetos raramente definem convenções explícitas para metadados:

```csharp
// Cada classe tem seu próprio "estilo"
public static class PersonConstants
{
    public const int NameMaxLength = 100;
    public const int NameMinLength = 3;
    public const bool NameRequired = true;
}

public static class ProductSettings
{
    public const int MAX_TITLE_LEN = 200;
    public const int MIN_TITLE_LEN = 5;
    public const bool TITLE_IS_REQUIRED = true;
}

public static class OrderConfig
{
    public static int DescriptionMaximumLength { get; } = 500;
    public static int DescriptionMinimumLength { get; } = 10;
    public static bool DescriptionIsRequired { get; } = false;
}
```

### Por Que Não Funciona Bem

1. **Impossível automatizar**:

```csharp
// Como escrever código que encontra "MaxLength" de qualquer propriedade?
// Pode ser: MaxLength, MaxLen, MAX_LENGTH, MaximumLength, _MaxLen...

// Com convenção fixa:
var maxLengthProp = metadataType.GetProperty($"{propertyName}MaxLength");
// Sempre funciona!
```

2. **LLMs não conseguem inferir**:

```
Prompt: "Adicione metadados para a propriedade Email"

Sem convenção - LLM pode gerar qualquer variação:
- EmailMaxLen, Email_Max_Length, MAX_EMAIL_LENGTH, emailMaxLength...

Com convenção - LLM gera consistentemente:
- EmailIsRequired, EmailMinLength, EmailMaxLength, EmailPattern
```

3. **Code review subjetivo**:

```csharp
// Sem convenção, cada PR tem discussões de estilo:
// "Por que MaxLen e não MaxLength?"
// "Por que underscore aqui?"
// "Deveria ser UPPERCASE?"

// Com convenção, é objetivo:
// ? FirstNameMaxLength ? Segue o padrão
// ? FirstName_MaxLen ? Não segue o padrão
```

4. **Roslyn analyzers não funcionam**:

```csharp
// Analyzer quer validar: "MinLength <= MaxLength"
// Como encontrar os pares sem convenção?

// Com convenção:
// Se existe {Property}MinLength, deve existir {Property}MaxLength
// E MinLength <= MaxLength
```

5. **IntelliSense desorganizado**:

```csharp
// Sem convenção - metadados espalhados:
// DescriptionIsRequired
// MAX_TITLE_LEN
// NameMaxLength
// TITLE_IS_REQUIRED
// NameRequired

// Com convenção - agrupado por propriedade:
// FirstNameIsRequired
// FirstNameMaxLength
// FirstNameMinLength
// LastNameIsRequired
// LastNameMaxLength
// LastNameMinLength
```

## A Decisão

### Nossa Abordagem

**Formato obrigatório: `<PropertyName><ConstraintType>`**

```csharp
public static class SimpleAggregateRootMetadata
{
    // FirstName
    public static readonly string FirstNamePropertyName = nameof(FirstName);
    public static bool FirstNameIsRequired { get; private set; } = true;
    public static int FirstNameMinLength { get; private set; } = 1;
    public static int FirstNameMaxLength { get; private set; } = 255;

    // LastName
    public static readonly string LastNamePropertyName = nameof(LastName);
    public static bool LastNameIsRequired { get; private set; } = true;
    public static int LastNameMinLength { get; private set; } = 1;
    public static int LastNameMaxLength { get; private set; } = 255;

    // BirthDate
    public static readonly string BirthDatePropertyName = nameof(BirthDate);
    public static bool BirthDateIsRequired { get; private set; } = true;
    public static int BirthDateMinAgeInYears { get; private set; } = 0;
    public static int BirthDateMaxAgeInYears { get; private set; } = 150;
}
```

### Componentes do Nome

| Componente | Descrição | Exemplo |
|------------|-----------|---------|
| `PropertyName` | Nome exato da propriedade (PascalCase) | `FirstName`, `BirthDate`, `Email` |
| `ConstraintType` | Tipo de constraint (sufixo padronizado) | `IsRequired`, `MinLength`, `MaxLength` |

### Tipos de Constraints Suportados

**Booleanos**:
| Sufixo | Tipo | Uso |
|--------|------|-----|
| `IsRequired` | `bool` | Propriedade obrigatória |
| `IsUnique` | `bool` | Valor deve ser único |
| `IsReadOnly` | `bool` | Propriedade não pode ser alterada após criação |

**Numéricos - Comprimento (strings)**:
| Sufixo | Tipo | Uso |
|--------|------|-----|
| `MinLength` | `int` | Comprimento mínimo |
| `MaxLength` | `int` | Comprimento máximo |

**Numéricos - Idade/Tempo (datas)**:
| Sufixo | Tipo | Uso |
|--------|------|-----|
| `MinAgeInYears` | `int` | Idade mínima em anos |
| `MaxAgeInYears` | `int` | Idade máxima em anos |
| `MinAgeInDays` | `int` | Idade mínima em dias |
| `MaxAgeInDays` | `int` | Idade máxima em dias |

**Numéricos - Valores**:
| Sufixo | Tipo | Uso |
|--------|------|-----|
| `MinValue` | `decimal/int` | Valor mínimo |
| `MaxValue` | `decimal/int` | Valor máximo |

**Strings - Formato**:
| Sufixo | Tipo | Uso |
|--------|------|-----|
| `Pattern` | `string` | Regex de validação |
| `Format` | `string` | Formato esperado (ex: "yyyy-MM-dd") |

**Referência**:
| Sufixo | Tipo | Uso |
|--------|------|-----|
| `PropertyName` | `string` (readonly) | Nome da propriedade via `nameof()` |

### Exemplos por Tipo de Propriedade

**String simples (FirstName)**:
```csharp
public static readonly string FirstNamePropertyName = nameof(FirstName);
public static bool FirstNameIsRequired { get; private set; } = true;
public static int FirstNameMinLength { get; private set; } = 1;
public static int FirstNameMaxLength { get; private set; } = 255;
```

**String com formato (Email)**:
```csharp
public static readonly string EmailPropertyName = nameof(Email);
public static bool EmailIsRequired { get; private set; } = true;
public static int EmailMinLength { get; private set; } = 5;
public static int EmailMaxLength { get; private set; } = 255;
public static string EmailPattern { get; private set; } = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
```

**Data com idade (BirthDate)**:
```csharp
public static readonly string BirthDatePropertyName = nameof(BirthDate);
public static bool BirthDateIsRequired { get; private set; } = true;
public static int BirthDateMinAgeInYears { get; private set; } = 0;
public static int BirthDateMaxAgeInYears { get; private set; } = 150;
```

**Numérico (Price)**:
```csharp
public static readonly string PricePropertyName = nameof(Price);
public static bool PriceIsRequired { get; private set; } = true;
public static decimal PriceMinValue { get; private set; } = 0.01m;
public static decimal PriceMaxValue { get; private set; } = 999999.99m;
```

**Propriedade derivada (FullName)**:
```csharp
public static readonly string FullNamePropertyName = nameof(FullName);
public static bool FullNameIsRequired { get; private set; } = true;
// Derivado de FirstName + LastName
public static int FullNameMinLength { get; private set; } = FirstNameMinLength + LastNameMinLength + 1;
public static int FullNameMaxLength { get; private set; } = FirstNameMaxLength + LastNameMaxLength + 1;
```

### Antipadrões

```csharp
// ? Underscore
public static int FirstName_MaxLength { get; private set; } = 100;

// ? Omitir "Is" em booleanos
public static bool FirstNameRequired { get; private set; } = true;

// ? Abreviações
public static int FNameMaxLen { get; private set; } = 100;

// ? SCREAMING_CASE
public static int FIRST_NAME_MAX_LENGTH { get; private set; } = 100;

// ? camelCase
public static int firstNameMaxLength { get; private set; } = 100;

// ? Ordem invertida (ConstraintType antes de PropertyName)
public static int MaxLengthFirstName { get; private set; } = 100;

// ? Sufixo não padronizado
public static int FirstNameMaximumLength { get; private set; } = 100;  // Use MaxLength
public static int FirstNameMaxChars { get; private set; } = 100;       // Use MaxLength
```

### Organização no Código

```csharp
public static class SimpleAggregateRootMetadata
{
    // Lock para alterações thread-safe
    private static readonly Lock _lockObject = new();

    // -------------------------------------------------------------------
    // Agrupe metadados por propriedade, em ordem alfabética
    // Dentro de cada grupo, ordem: PropertyName, IsRequired, Min*, Max*, Pattern
    // -------------------------------------------------------------------

    // BirthDate
    public static readonly string BirthDatePropertyName = nameof(BirthDate);
    public static bool BirthDateIsRequired { get; private set; } = true;
    public static int BirthDateMinAgeInYears { get; private set; } = 0;
    public static int BirthDateMaxAgeInYears { get; private set; } = 150;

    // Email
    public static readonly string EmailPropertyName = nameof(Email);
    public static bool EmailIsRequired { get; private set; } = true;
    public static int EmailMinLength { get; private set; } = 5;
    public static int EmailMaxLength { get; private set; } = 255;
    public static string EmailPattern { get; private set; } = @"...";

    // FirstName
    public static readonly string FirstNamePropertyName = nameof(FirstName);
    public static bool FirstNameIsRequired { get; private set; } = true;
    public static int FirstNameMinLength { get; private set; } = 1;
    public static int FirstNameMaxLength { get; private set; } = 255;
}
```

## Consequências

### Benefícios

- **Previsibilidade**: Qualquer desenvolvedor sabe o nome do metadado sem consultar documentação
- **LLM-friendly**: Modelos de linguagem geram metadados consistentes
- **Automação**: Roslyn analyzers, geradores de código, e ferramentas funcionam
- **IntelliSense**: Metadados agrupados por propriedade no autocomplete
- **Code review objetivo**: "Segue ou não segue a convenção" - sem discussões subjetivas
- **Busca fácil**: `Ctrl+F` por `IsRequired` encontra todos os campos obrigatórios

### Trade-offs (Com Perspectiva)

- **Nomes longos**: `BirthDateMinAgeInYears` tem 20 caracteres
- **Rigidez**: Não permite variações "criativas"

### Trade-offs Frequentemente Superestimados

**"Nomes muito longos"**

Na prática, nomes longos são MELHORES para legibilidade:

```csharp
// O que é mais claro?
if (age < BDMinAge) { ... }           // Abreviado - o que é BD?
if (age < BirthDateMinAgeInYears) { ... }  // Explícito - auto-documentado
```

O IntelliSense autocompleta - você não digita o nome inteiro. E o código é lido muito mais vezes do que escrito.

**"Não permite flexibilidade"**

A "flexibilidade" de nomenclatura livre é justamente o problema. Quando cada um nomeia como quer, ninguém consegue automatizar.

```csharp
// "Flexível" - cada projeto diferente
Project A: FirstNameMaxLength
Project B: FirstName_MaxLen
Project C: MAX_LEN_FIRST_NAME

// Com convenção - mesmo padrão em todos os projetos
// Ferramentas, LLMs, e desenvolvedores sabem o que esperar
```

**"É mais trabalho seguir a convenção"**

Na verdade, é MENOS trabalho total:

```csharp
// Sem convenção - precisa pensar e decidir cada vez
"Hmm, devo usar MaxLength ou MaxLen? Com underscore ou sem?"

// Com convenção - não precisa pensar
"PropertyName + ConstraintType = FirstNameMaxLength. Próximo."
```

## Fundamentação Teórica

### Padrões de Design Relacionados

**Naming Conventions** - Toda linguagem e framework bem-sucedido tem convenções de nomenclatura (.NET: PascalCase para públicos, camelCase para privados). Estendemos isso para metadados.

**Domain-Specific Language (DSL)** - A convenção cria uma "mini-linguagem" para metadados: `{Property}{Constraint}` é uma gramática simples que qualquer um pode aprender.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) enfatiza **Ubiquitous Language**:

> "Use the model as the backbone of a language. Commit the team to exercising that language relentlessly in all communication within the team and in the code."
>
> *Use o modelo como a espinha dorsal de uma linguagem. Comprometa a equipe a exercitar essa linguagem implacavelmente em toda comunicação dentro da equipe e no código.*

A convenção `{Property}{Constraint}` é parte da nossa Ubiquitous Language para metadados.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre nomenclatura:

> "The name of a variable, function, or class should answer all the big questions. It should tell you why it exists, what it does, and how it is used."
>
> *O nome de uma variável, função ou classe deve responder todas as grandes questões. Deve dizer por que existe, o que faz, e como é usado.*

`FirstNameMaxLength` responde: "É o comprimento máximo (MaxLength) da propriedade FirstName". Auto-explicativo.

O princípio **"Use Intention-Revealing Names"** (Use Nomes que Revelam Intenção):

> "The name should reveal intent. [...] Choosing good names takes time but saves more than it takes."
>
> *O nome deve revelar intenção. [...] Escolher bons nomes toma tempo mas economiza mais do que toma.*

### O Que o Clean Architecture Diz

Clean Architecture não aborda nomenclatura diretamente, mas o princípio de **boundaries claros** se aplica: a convenção de nomenclatura cria um "contrato" claro entre quem define e quem consome metadados.

### Outros Fundamentos

**Principle of Least Surprise**:

> "A component should behave in a way that most users will expect it to behave."
>
> *Um componente deve se comportar da forma que a maioria dos usuários esperaria.*

Se `FirstNameMaxLength` existe, o desenvolvedor espera encontrar `LastNameMaxLength` para outra string. A convenção atende essa expectativa.

**Convention over Configuration** (Ruby on Rails):

> "By following default conventions, developers can avoid explicit configuration."
>
> *Seguindo convenções padrão, desenvolvedores podem evitar configuração explícita.*

A convenção elimina decisões: não há configuração de "como nomear metadados" - há apenas uma forma correta.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que convenções de nomenclatura melhoram a manutenibilidade do código?"
- "Como criar Roslyn analyzers para validar convenções de nomenclatura?"
- "Qual a relação entre Ubiquitous Language do DDD e convenções de código?"
- "Como convenções de nomenclatura ajudam LLMs a gerar código consistente?"

### Leitura Recomendada

- [.NET Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines)
- [Clean Code - Chapter 2: Meaningful Names](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)
- [Domain-Driven Design - Ubiquitous Language](https://martinfowler.com/bliki/UbiquitousLanguage.html)
- [Convention over Configuration](https://en.wikipedia.org/wiki/Convention_over_configuration)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Define a convenção de nomenclatura para metadados que todas as entidades devem seguir |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Convenção de Nomenclatura
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Por Que Este Padrão é Crítico
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_TEMPLATE: Tipos de Constraints Suportados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_TEMPLATE: Adicionando Novos Metadados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Metadados implementados seguindo a convenção
