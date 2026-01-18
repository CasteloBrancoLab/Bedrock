# DE-033: Antipadrão: Readonly Struct para Entidades

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **formulário de cadastro online** com dois modelos de feedback:

**Modelo "tudo ou nada" (readonly struct com validação no construtor)**:
- Usuário preenche 10 campos
- Clica em "Enviar"
- Sistema valida TUDO de uma vez
- Erro: "Nome inválido" - mas qual dos 10 campos está errado?
- Usuário corrige, envia de novo
- Erro: "CPF inválido" - ah, tinha outro erro também!
- Processo frustrante: descobrir erros um por um

**Modelo "feedback incremental" (classe com setters private)**:
- Usuário preenche cada campo
- Sistema valida campo por campo
- Ao final: "Nome muito curto (mín 3), CPF inválido (formato), Email já existe"
- Usuário vê TODOS os erros de uma vez
- Corrige tudo e envia com sucesso

O `readonly struct` força validação "tudo ou nada" no construtor - você só pode criar se TUDO estiver válido. Isso impede feedback completo ao usuário.

---

### O Problema Técnico

`readonly struct` parece ideal para entidades imutáveis, mas tem limitações fundamentais:

```csharp
// ❌ ANTIPADRÃO: Entidade como readonly record struct
public readonly record struct Person
{
    public string FirstName { get; }
    public string LastName { get; }
    public DateOnly BirthDate { get; }

    public Person(string firstName, string lastName, DateOnly birthDate)
    {
        // Validação no construtor - TUDO ou NADA
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required");

        if (firstName.Length < 3)
            throw new ArgumentException("First name too short");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required");

        if (birthDate > DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentException("Birth date cannot be in future");

        // Só chega aqui se TUDO estiver válido
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }
}

// Problema: usuário vê UM erro por vez
try
{
    var person = new Person("", "", DateOnly.FromDateTime(DateTime.Now.AddYears(1)));
}
catch (ArgumentException ex)
{
    // ex.Message = "First name is required"
    // Usuário NÃO sabe que lastName e birthDate também estão errados!
}
```

**Problemas graves**:

1. **Feedback incompleto**: Usuário vê apenas o primeiro erro
2. **UX degradada**: Múltiplas tentativas para descobrir todos os erros
3. **Exceções para controle de fluxo**: Validação esperada não deveria usar exceções
4. **Reconstitution impossível**: Dados históricos podem violar regras atuais

---

### Por Que Parece uma Boa Ideia

```csharp
// readonly struct tem vantagens REAIS para VALUE OBJECTS:
public readonly record struct Money(decimal Amount, string Currency);
public readonly record struct EmailAddress(string Value);
public readonly record struct Coordinate(double Latitude, double Longitude);

// Vantagens:
// ✅ Imutabilidade garantida pelo compilador
// ✅ Stack allocation (zero GC pressure)
// ✅ Semântica de valor (comparação por conteúdo)
// ✅ Simples e conciso
```

O problema é usar para **ENTIDADES** que precisam de:
- Validação incremental com feedback completo
- Reconstitution de dados históricos
- Padrão Clone-Modify-Return

## A Decisão

### Nossa Abordagem

Use **classe sealed** com **setters private** para entidades:

```csharp
// ✅ CORRETO: Classe sealed com setters private
public sealed class Person : EntityBase<Person>
{
    // Setters private - imutabilidade EXTERNA, mutabilidade INTERNA controlada
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }

    // Construtor private - ninguém cria diretamente
    private Person() { }

    // Factory method com validação INCREMENTAL
    public static Person? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (ctx, inp) => new Person(),
            handler: (ctx, inp, instance) =>
            {
                // Validação incremental - coleta TODOS os erros
                return
                    instance.SetFirstName(ctx, inp.FirstName)
                    & instance.SetLastName(ctx, inp.LastName)      // & bitwise, não &&
                    & instance.SetBirthDate(ctx, inp.BirthDate);   // Continua mesmo se anterior falhou
            }
        );
    }

    // Métodos Set* validam e atribuem
    private bool SetFirstName(ExecutionContext ctx, string? firstName)
    {
        if (!ValidateFirstName(ctx, firstName))
            return false;

        FirstName = firstName!;
        return true;
    }

    // Validação estática reutilizável
    public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            ctx.AddErrorMessage("FIRST_NAME_REQUIRED", "First name is required");
            return false;
        }

        if (firstName.Length < PersonMetadata.FirstNameMinLength)
        {
            ctx.AddErrorMessage(
                "FIRST_NAME_TOO_SHORT",
                $"First name must be at least {PersonMetadata.FirstNameMinLength} characters"
            );
            return false;
        }

        return true;
    }

    // Similar para LastName, BirthDate...
}
```

### Feedback Completo ao Usuário

```csharp
// Cenário: múltiplos erros de validação
var ctx = ExecutionContext.Create(...);

var input = new RegisterNewInput(
    FirstName: "",           // Erro 1: vazio
    LastName: "A",           // Erro 2: muito curto
    BirthDate: DateOnly.FromDateTime(DateTime.Now.AddYears(1))  // Erro 3: futuro
);

var person = Person.RegisterNew(ctx, input);

// person é null (falhou)
// MAS ctx.Messages contém TODOS os erros:

foreach (var msg in ctx.Messages.Where(m => m.Type == MessageType.Error))
{
    Console.WriteLine($"{msg.Code}: {msg.Text}");
}

// Output:
// FIRST_NAME_REQUIRED: First name is required
// LAST_NAME_TOO_SHORT: Last name must be at least 2 characters
// BIRTH_DATE_IN_FUTURE: Birth date cannot be in the future

// Usuário vê TUDO de uma vez e corrige em uma única tentativa!
```

### Operador Bitwise AND (&) vs Short-Circuit (&&)

```csharp
// ❌ Short-circuit (&&) - para no primeiro false
return
    instance.SetFirstName(ctx, inp.FirstName)
    && instance.SetLastName(ctx, inp.LastName)    // Não executa se anterior falhou
    && instance.SetBirthDate(ctx, inp.BirthDate); // Não executa se anterior falhou

// ✅ Bitwise AND (&) - executa TODOS
return
    instance.SetFirstName(ctx, inp.FirstName)
    & instance.SetLastName(ctx, inp.LastName)     // Executa sempre
    & instance.SetBirthDate(ctx, inp.BirthDate);  // Executa sempre
```

O operador `&` garante que TODAS as validações executam, coletando todos os erros.

### Comparação: readonly struct vs sealed class

| Aspecto | readonly struct | sealed class (nossa escolha) |
|---------|-----------------|------------------------------|
| **Feedback de erros** | Um por vez (exceções) | Todos de uma vez (mensagens) |
| **Reconstitution** | Impossível se regras mudaram | Sempre funciona |
| **Validação** | Tudo ou nada no construtor | Incremental, propriedade por propriedade |
| **Clone-Modify-Return** | Complexo (struct é copiada) | Simples (Clone + Set*) |
| **Imutabilidade externa** | Garantida pelo compilador | Garantida por design (setters private) |
| **Alocação** | Stack | Heap |
| **GC pressure** | Zero | Mínimo (pooling possível) |

### Quando USAR readonly struct

`readonly struct` é **CORRETO** para:

```csharp
// ✅ Value Objects simples (primitivos de domínio)
public readonly record struct Money(decimal Amount, string Currency);
public readonly record struct EmailAddress(string Value);
public readonly record struct TenantInfo(Guid Code, string? Name);

// ✅ Input Objects (DTOs imutáveis)
public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    DateOnly BirthDate
);

// ✅ Dados compostos sem identidade
public readonly record struct Coordinate(double Latitude, double Longitude);
public readonly record struct DateRange(DateOnly Start, DateOnly End);
```

### Quando NÃO USAR readonly struct

`readonly struct` é **INCORRETO** para:

```csharp
// ❌ Entidades de domínio (precisam de validação incremental)
public readonly record struct Person(...);

// ❌ Aggregate Roots (precisam de Clone-Modify-Return)
public readonly record struct Order(...);

// ❌ Qualquer coisa que precise de reconstitution
public readonly record struct Customer(...);
```

### Trade-offs (Com Perspectiva)

- **Alocação no heap**: Classe aloca no heap, struct na stack
  - **Mitigação**: Uma alocação de ~100 bytes é negligenciável. Uma query HTTP é 1000x mais custosa. Priorize UX sobre micro-otimização.

- **Mais código**: Classe requer mais boilerplate que struct
  - **Mitigação**: O código adicional é a validação incremental - exatamente o que queremos. Não é overhead, é funcionalidade.

### Trade-offs Frequentemente Superestimados

**"Struct é mais performático"**

Na prática, a diferença é irrelevante para entidades:

```csharp
// Struct: ~0 nanosegundos de alocação
// Classe: ~10 nanosegundos de alocação

// Query ao banco: ~1.000.000 nanosegundos (1ms)
// Chamada HTTP: ~100.000.000 nanosegundos (100ms)

// A "economia" de struct é 0.00001% do tempo total
// Em troca, você perde validação incremental e reconstitution
```

**"readonly garante imutabilidade"**

Setters private também garantem imutabilidade externa:

```csharp
// readonly struct - imutabilidade pelo compilador
public readonly record struct Person { ... }

// sealed class - imutabilidade por design
public sealed class Person
{
    public string Name { get; private set; }  // Externo: imutável. Interno: controlado.
}

// Para o código EXTERNO, ambos são igualmente imutáveis
// A diferença é que classe permite validação incremental INTERNA
```

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) distingue Entities de Value Objects:

> "An object defined primarily by its identity is called an Entity. [...] An object that represents a descriptive aspect of the domain with no conceptual identity is called a Value Object."
>
> *Um objeto definido primariamente por sua identidade é chamado de Entidade. [...] Um objeto que representa um aspecto descritivo do domínio sem identidade conceitual é chamado de Value Object.*

- **Entidades** (Person, Order, Customer): Precisam de identidade, lifecycle, validação rica → **classe**
- **Value Objects** (Money, Email, Coordinate): Definidos por atributos, imutáveis por natureza → **struct**

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre feedback de erros:

> "Use exceptions for exceptional conditions, not for control flow."
>
> *Use exceções para condições excepcionais, não para controle de fluxo.*

Validação de input de usuário é **esperada**, não excepcional. Usar exceções para validação viola este princípio.

### Princípio da Menor Surpresa

Bertrand Meyer em "Object-Oriented Software Construction" (1997):

> "A component should behave in a way that most users will expect it to behave."
>
> *Um componente deveria se comportar da forma que a maioria dos usuários espera que ele se comporte.*

Usuários esperam ver **todos** os erros de validação, não descobri-los um por um. Nossa abordagem atende essa expectativa.

## Antipadrões Relacionados

### Antipadrão: Validação no Construtor com Exceções

```csharp
// ❌ Exceção para cada erro - usuário vê um por vez
public Person(string firstName, string lastName)
{
    if (string.IsNullOrWhiteSpace(firstName))
        throw new ArgumentException("First name required");  // Para aqui

    if (string.IsNullOrWhiteSpace(lastName))
        throw new ArgumentException("Last name required");   // Nunca chega

    FirstName = firstName;
    LastName = lastName;
}
```

### Antipadrão: Lista de Erros no Construtor

```csharp
// ❌ Tentar coletar erros no construtor - complexidade desnecessária
public Person(string firstName, string lastName)
{
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(firstName))
        errors.Add("First name required");

    if (string.IsNullOrWhiteSpace(lastName))
        errors.Add("Last name required");

    if (errors.Any())
        throw new ValidationException(errors);  // Ainda usa exceção!

    FirstName = firstName;
    LastName = lastName;
}
```

### Antipadrão: Result Type com readonly struct

```csharp
// ❌ Complexidade acidental para contornar limitação do struct
public readonly record struct PersonResult(Person? Person, IReadOnlyList<string> Errors);

public static PersonResult Create(string firstName, string lastName)
{
    var errors = new List<string>();
    // ... validações ...

    if (errors.Any())
        return new PersonResult(null, errors);

    return new PersonResult(new Person(firstName, lastName), Array.Empty<string>());
}

// Problema: struct não pode ser null, então precisa de wrapper
// Adiciona complexidade sem benefício real
```

## Decisões Relacionadas

- [DE-001](./DE-001-entidades-devem-ser-sealed.md) - Entidades Devem Ser Sealed (classe, não struct)
- [DE-002](./DE-002-construtores-privados-com-factory-methods.md) - Construtores Privados com Factory Methods
- [DE-003](./DE-003-imutabilidade-controlada-clone-modify-return.md) - Imutabilidade Controlada
- [DE-006](./DE-006-operador-bitwise-and-para-validacao-completa.md) - Operador Bitwise AND para Validação Completa
- [DE-019](./DE-019-input-objects-pattern.md) - Input Objects Pattern (readonly struct é OK para inputs)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que implementa o padrão correto (sealed class com setters private) |
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Coleta mensagens de erro para feedback completo ao usuário |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - comentário LLM_ANTIPATTERN sobre readonly struct
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - Implementação como sealed class
