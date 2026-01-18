# DE-019: Input Objects Pattern (readonly record struct)

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **formulário de pedido em um restaurante**:

**Cenário problemático** (parâmetros individuais):
O garçom precisa memorizar: "Mesa 5, hambúrguer, sem cebola, batata média, refrigerante grande, sem gelo, pra viagem". Se o restaurante adicionar "molho extra?" ao cardápio, todos os garçons precisam reaprender o script.

**Cenário correto** (objeto estruturado):
O garçom usa uma **comanda estruturada** com campos para cada opção. Adicionar "molho extra" é só mais um campo na comanda - nenhum garçom precisa mudar seu comportamento.

Em código, passar muitos parâmetros individuais é como o garçom memorizando - frágil e difícil de evoluir. Input Objects são como a comanda estruturada.

---

### O Problema Técnico

Métodos com muitos parâmetros são difíceis de manter:

```csharp
// ❌ Parâmetros individuais - problemas múltiplos
public static Person? RegisterNew(
    ExecutionContext context,
    string firstName,
    string lastName,
    DateTime birthDate,
    string? email,
    string? phone
)
{
    // ...
}

// Problemas:
// 1. ORDEM: fácil trocar firstName com lastName
var person = Person.RegisterNew(ctx, lastName, firstName, ...); // Bug silencioso!

// 2. EVOLUÇÃO: adicionar "middleName" quebra TODOS os call sites
public static Person? RegisterNew(
    ExecutionContext context,
    string firstName,
    string middleName,  // NOVO - quebra tudo!
    string lastName,
    // ...
)

// 3. LEGIBILIDADE: o que é cada string?
var person = Person.RegisterNew(ctx, "John", "Doe", birthDate, null, null);
// Qual null é email? Qual é phone?
```

## A Decisão

### Nossa Abordagem

Métodos públicos recebem **Input Objects** (`readonly record struct`) ao invés de parâmetros individuais:

```csharp
// ✅ Input Object - SEMPRE readonly record struct
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate
);

// Método recebe o Input Object
public static SimpleAggregateRoot? RegisterNew(
    ExecutionContext executionContext,
    RegisterNewInput input
)
{
    // Acesso via input.FirstName, input.LastName, etc.
}

// Uso - claro e auto-documentado
var person = SimpleAggregateRoot.RegisterNew(context, new RegisterNewInput(
    FirstName: "John",
    LastName: "Doe",
    BirthDate: birthDate
));
```

### Por Que `readonly record struct` (Não `class` ou `record class`)

```csharp
// ✅ readonly record struct - nossa escolha (OBRIGATÓRIO)
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate
);
```

| Característica | `readonly record struct` | `record class` | `class` |
|----------------|--------------------------|----------------|---------|
| **Alocação** | Stack (zero GC) | Heap (GC pressure) | Heap (GC pressure) |
| **Equality** | Por valor (automático) | Por valor (automático) | Por referência |
| **ToString()** | Automático | Automático | Manual |
| **Imutabilidade** | Garantida (`readonly`) | Imutável por padrão | Manual |
| **Null** | Não-nullable | Nullable | Nullable |
| **Performance** | Excelente | Boa | Boa |

### Por Que `readonly` é Obrigatório

```csharp
// ✅ readonly record struct - previne modificações
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate
);

// Sem readonly, alguém poderia fazer:
public void ProcessInput(RegisterNewInput input)
{
    input.FirstName = "Hacked"; // ❌ Modificação oculta!
    _service.Process(input);    // Service recebe valor modificado
}

// Com readonly:
public void ProcessInput(RegisterNewInput input)
{
    input.FirstName = "Hacked"; // ❌ NÃO COMPILA!
}
```

### Estrutura dos Input Objects

**Um Input Object por operação**:

```csharp
// RegisterNew - criação de nova entidade
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate
);

// ChangeName - alteração de nome
public readonly record struct ChangeNameInput(
    string FirstName,
    string LastName
);

// ChangeBirthDate - alteração de data de nascimento
public readonly record struct ChangeBirthDateInput(
    BirthDate BirthDate
);

// CreateFromExistingInfo - reconstitution
public readonly record struct CreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string FirstName,
    string LastName,
    string FullName,
    BirthDate BirthDate
);
```

### Organização de Arquivos

```
SimpleAggregateRoots/
├── SimpleAggregateRoot.cs          # Entidade + Metadata (classe aninhada)
├── Inputs/                         # Pasta para Input Objects
    ├── RegisterNewInput.cs
    ├── ChangeNameInput.cs
    ├── ChangeBirthDateInput.cs
    └── CreateFromExistingInfoInput.cs
```

**Nota**: A classe de metadados é **aninhada** dentro da entidade (não em arquivo separado), pois metadados são intrínsecos à entidade. O nome segue o padrão `<EntityName>Metadata`:

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // Metadata como classe aninhada - nome: EntityName + "Metadata"
    public static class SimpleAggregateRootMetadata
    {
        public static readonly string FirstNamePropertyName = nameof(FirstName);
        public static bool FirstNameIsRequired { get; private set; } = true;
        public static int FirstNameMinLength { get; private set; } = 1;
        public static int FirstNameMaxLength { get; private set; } = 255;
        // ...
    }

    // Propriedades, métodos, etc.
    public string FirstName { get; private set; }
    // ...
}

// Acesso externo:
var maxLength = SimpleAggregateRoot.SimpleAggregateRootMetadata.FirstNameMaxLength;
```

Input Objects, por outro lado, ficam em arquivos separados porque:
- São usados por **outras camadas** (Application, API)
- Podem ter **muitos** para uma única entidade
- Facilita **descoberta** e navegação

### Benefícios

#### 1. Evoluibilidade - Adicionar Parâmetros Sem Quebrar

```csharp
// Versão 1.0
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate
);

// Versão 2.0 - adiciona MiddleName com default
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate,
    string? MiddleName = null  // NOVO - não quebra call sites existentes!
);

// Call sites antigos continuam funcionando:
new RegisterNewInput("John", "Doe", birthDate) // ✅ Compila
// MiddleName = null (default)

// Novos call sites podem usar:
new RegisterNewInput("John", "Doe", birthDate, "William") // ✅
```

#### 2. Legibilidade - Named Arguments Implícitos

```csharp
// ❌ Parâmetros individuais - o que é cada string?
Person.RegisterNew(ctx, "John", "Doe", birthDate, null, "555-1234");

// ✅ Input Object - auto-documentado
Person.RegisterNew(ctx, new RegisterNewInput(
    FirstName: "John",
    LastName: "Doe",
    BirthDate: birthDate
));
```

#### 3. Customização - Factories por Tenant via IOC

```csharp
// Interface de factory
public interface IChangeNameInputFactory
{
    ChangeNameInput Create(string firstName, string lastName);
}

// Tenant Brasil: nome e sobrenome separados
public class BrazilChangeNameInputFactory : IChangeNameInputFactory
{
    public ChangeNameInput Create(string firstName, string lastName)
        => new(firstName, lastName);
}

// Tenant Espanha: nome completo em um campo
public class SpainChangeNameInputFactory : IChangeNameInputFactory
{
    public ChangeNameInput Create(string firstName, string lastName)
        => new($"{firstName} {lastName}", string.Empty);
}

// Controller resolve factory via DI
public class PersonController
{
    private readonly IChangeNameInputFactory _inputFactory;

    public PersonController(IChangeNameInputFactory inputFactory)
    {
        _inputFactory = inputFactory;
    }

    [HttpPut("{id}/name")]
    public IActionResult ChangeName(Guid id, ChangeNameRequest request)
    {
        // Factory cria Input conforme regras do tenant
        var input = _inputFactory.Create(request.FirstName, request.LastName);

        var result = _person.ChangeName(_context, input);
        // ...
    }
}
```

#### 4. Performance - Stack Allocation

```csharp
// record struct = stack allocation
// Não cria pressão no Garbage Collector

public void ProcessMany(IEnumerable<PersonData> data)
{
    foreach (var item in data)
    {
        // Input é alocado na stack, não no heap
        // Zero GC pressure mesmo com milhões de iterações
        var input = new RegisterNewInput(
            item.FirstName,
            item.LastName,
            item.BirthDate
        );

        Person.RegisterNew(_context, input);
    }
}
```

#### 5. Imutabilidade - Previne Modificações Ocultas

```csharp
// readonly previne modificações entre camadas
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate
);

// Middleware não pode modificar o input
public class ValidationMiddleware
{
    public void Process(RegisterNewInput input)
    {
        // ❌ NÃO COMPILA - readonly!
        input.FirstName = input.FirstName.Trim();

        // ✅ Crie novo input se precisar transformar
        var trimmedInput = input with { FirstName = input.FirstName.Trim() };
    }
}
```

### Input Objects vs DTOs

| Aspecto | Input Object | DTO |
|---------|--------------|-----|
| **Propósito** | Parâmetros de operação de domínio | Transferência entre camadas |
| **Localização** | Domain layer | Application/API layer |
| **Validação** | Nenhuma (domínio valida) | Pode ter Data Annotations |
| **Tipo** | `record struct` | `class` ou `record` |
| **Escopo** | Uma operação específica | Pode representar entidade inteira |

```csharp
// DTO - API layer
public class CreatePersonRequest
{
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(255)]
    public string LastName { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }
}

// Input Object - Domain layer
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate  // Value Object, não DateTime
);

// Controller faz a conversão
[HttpPost]
public IActionResult CreatePerson(CreatePersonRequest request)
{
    // DTO → Input Object
    var input = new RegisterNewInput(
        request.FirstName,
        request.LastName,
        BirthDate.Create(request.BirthDate) // Conversão para Value Object
    );

    var person = SimpleAggregateRoot.RegisterNew(_context, input);
    // ...
}
```

### Trade-offs (Com Perspectiva)

- **Mais tipos para criar**: Cada operação tem seu Input Object
  - **Mitigação**: São tipos simples (1-2 linhas de código cada)

- **Conversão DTO → Input**: Precisa mapear na camada de aplicação
  - **Mitigação**: Separação de responsabilidades clara e testável

### Trade-offs Frequentemente Superestimados

**"Muitos arquivos pequenos"**

Na verdade, arquivos pequenos e focados são mais fáceis de:
- Navegar (IDE mostra estrutura clara)
- Testar (escopo limitado)
- Revisar (mudanças isoladas)

**"Overhead de criar structs"**

`record struct` tem overhead **zero** em runtime:
- Alocação na stack (não no heap)
- Sem boxing para tipos primitivos
- Compilador pode inline completamente

**"Duplicação entre DTO e Input Object"**

Esta é uma reclamação comum, especialmente de desenvolvedores que criam projetos mas saem antes da fase de manutenção (turnover de 1-2 anos). Na **criação**, os objetos podem ter o mesmo estado, mas são **responsabilidades diferentes** que **divergem com o tempo**.

A "duplicação" é **intencional** - são responsabilidades diferentes:
- **DTO (API)**: validação de API, serialização, campos específicos de infraestrutura
- **Input (Domain)**: parâmetros de operação de domínio, tipos ricos (Value Objects)

**Exemplo 1: API Payload vs Use Case Input**

```csharp
// API Payload - pode ter campos de infraestrutura
public class CreatePersonRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }

    // Campos de INFRAESTRUTURA - nada a ver com o domínio!
    public int? CacheDurationSeconds { get; set; }  // Cliente controla cache
    public string? CorrelationId { get; set; }       // Rastreamento distribuído
    public string? IdempotencyKey { get; set; }      // Retry seguro
}

// Input de Domínio - apenas dados de negócio
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate  // Value Object, não DateTime!
);
```

**Exemplo 2: Domain Model vs Data Model**

O mesmo princípio se aplica a outras camadas:

```csharp
// Domain Entity - comportamento e regras de negócio
public sealed class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName => $"{FirstName} {LastName}";
    public BirthDate BirthDate { get; private set; }
    public int Age => BirthDate.CalculateAge();
}

// PostgreSQL - várias colunas normalizadas
CREATE TABLE persons (
    id UUID PRIMARY KEY,
    first_name VARCHAR(255),
    last_name VARCHAR(255),
    birth_date DATE,
    created_at TIMESTAMP,
    -- ...
);

// Redis - key/value com estado serializado
// Key: "person:550e8400-e29b-41d4-a716-446655440000"
// Value: "{\"firstName\":\"John\",\"lastName\":\"Doe\",...}"

// MongoDB - documento desnormalizado
{
    "_id": "550e8400-e29b-41d4-a716-446655440000",
    "name": { "first": "John", "last": "Doe", "full": "John Doe" },
    "birthDate": ISODate("1990-01-15"),
    "metadata": { "createdAt": ISODate("2024-01-01"), ... }
}
```

A mesma **Domain Entity** tem representações de dados **completamente diferentes** dependendo do storage. Unificar Domain Model e Data Model é um erro de design que só aparece quando você precisa:
- Migrar de banco de dados
- Adicionar cache (Redis)
- Criar projeções para leitura (CQRS)
- Suportar múltiplos storages simultaneamente

**O problema do turnover**

Desenvolvedores que criam projetos e saem em 1-2 anos frequentemente:
1. Veem apenas a **fase de criação** (onde tudo parece "igual")
2. Não vivenciam a **fase de manutenção** (onde as responsabilidades divergem)
3. Concluem incorretamente que é "duplicação desnecessária"

A separação de responsabilidades é um investimento que paga dividendos na **manutenção**, não na **criação**.

Unificá-los cria acoplamento indesejado entre camadas que só se manifesta quando:
- Requisitos mudam (e sempre mudam)
- Novas integrações são adicionadas
- Performance precisa ser otimizada
- Equipe precisa trabalhar em paralelo

## Fundamentação Teórica

### Parameter Object Pattern

Martin Fowler em "Refactoring" (1999) documenta o Parameter Object como refactoring para métodos com muitos parâmetros:

> "Replace Parameter with Object: Group parameters that naturally go together into an object."
>
> *Substitua Parâmetro por Objeto: Agrupe parâmetros que naturalmente andam juntos em um objeto.*

Este é um dos refactorings mais recomendados para melhorar legibilidade e evoluibilidade.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre Value Objects:

> "An object that represents a descriptive aspect of the domain with no conceptual identity is called a VALUE OBJECT. VALUE OBJECTS are instantiated to represent elements of the design that we care about only for what they are, not who or which they are."
>
> *Um objeto que representa um aspecto descritivo do domínio sem identidade conceitual é chamado VALUE OBJECT. VALUE OBJECTS são instanciados para representar elementos do design que nos importamos apenas pelo que eles são, não quem ou qual eles são.*

Input Objects são **Value Objects** - definidos por seus atributos, não por identidade. Dois `RegisterNewInput` com os mesmos valores são semanticamente idênticos.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre imutabilidade:

> "A Value Object should be immutable. Once created, a Value Object can never be altered."
>
> *Um Value Object deve ser imutável. Uma vez criado, um Value Object nunca pode ser alterado.*

Por isso usamos `readonly record struct` - garante imutabilidade em tempo de compilação.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre número de parâmetros:

> "The ideal number of arguments for a function is zero (niladic). Next comes one (monadic), followed closely by two (dyadic). Three arguments (triadic) should be avoided where possible. More than three (polyadic) requires very special justification—and then shouldn't be used anyway."
>
> *O número ideal de argumentos para uma função é zero (niládica). Depois vem um (monádica), seguida de perto por dois (diádica). Três argumentos (triádica) devem ser evitados quando possível. Mais de três (poliádica) requer justificativa muito especial—e mesmo assim não deveria ser usado.*

Input Objects reduzem qualquer número de parâmetros para **dois**: `ExecutionContext` + `Input`.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre DTOs e camadas:

> "The structures that cross the boundaries are simple data structures. [...] We don't want to cheat and pass Entity objects or database rows."
>
> *As estruturas que cruzam fronteiras são estruturas de dados simples. [...] Não queremos trapacear e passar objetos Entity ou linhas de banco de dados.*

Input Objects são essas "estruturas de dados simples" - transportam dados entre camadas sem vazamento de detalhes de implementação.

### Command Pattern

Input Objects são similares a Commands em CQRS - encapsulam a intenção de uma operação com todos os dados necessários.

## Antipadrões Documentados

### Antipadrão 1: Muitos Parâmetros Individuais

```csharp
// ❌ Difícil de evoluir e fácil de errar
public static Person? RegisterNew(
    ExecutionContext ctx,
    string firstName,
    string lastName,
    DateTime birthDate,
    string? email,
    string? phone,
    string? address,
    string? city,
    string? state,
    string? zipCode
)
```

### Antipadrão 2: Input Object como Class

```csharp
// ❌ Alocação no heap, GC pressure
public class RegisterNewInput
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

### Antipadrão 3: Input Object Mutável

```csharp
// ❌ Permite modificações ocultas
public record struct RegisterNewInput
{
    public string FirstName { get; set; }  // Mutável!
    public string LastName { get; set; }   // Mutável!
}
```

### Antipadrão 4: Input Object Genérico/Reutilizado

```csharp
// ❌ Um Input para múltiplas operações
public record struct PersonInput(
    string? FirstName,    // Nullable para permitir updates parciais
    string? LastName,     // Confuso - quando é obrigatório?
    DateTime? BirthDate,  // Complexidade desnecessária
    bool IsCreating       // Flag para diferenciar operações 🤮
);
```

### Antipadrão 5: Validação no Input Object

```csharp
// ❌ Validação no Input (responsabilidade do domínio)
public record struct RegisterNewInput
{
    private string _firstName;

    public string FirstName
    {
        get => _firstName;
        init
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Required"); // ❌ Domínio deveria validar
            _firstName = value;
        }
    }
}
```

## Decisões Relacionadas

- [DE-002](./DE-002-construtores-privados-com-factory-methods.md) - Factory methods que recebem Input Objects
- [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) - RegisterNew vs CreateFromExistingInfo (ambos usam Input Objects)
- [DE-034](./DE-034-factories-customizadas-via-ioc-para-multitenancy.md) - Factories de Input Objects para multitenancy

## Leitura Recomendada

- [Refactoring - Martin Fowler](https://refactoring.com/) - Parameter Object pattern
- [C# Records](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [Struct Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/struct)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Define o padrão de Input objects para factory methods, promovendo API estável e evoluível |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Input Objects Pattern
- [RegisterNewInput.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/Inputs/RegisterNewInput.cs) - Definição do Input para RegisterNew
- [ChangeNameInput.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/Inputs/ChangeNameInput.cs) - Definição do Input para ChangeName
- [CreateFromExistingInfoInput.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/Inputs/CreateFromExistingInfoInput.cs) - Definição do Input para reconstitution
