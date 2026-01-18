# DE-052: Construtores Protegidos em Classes Abstratas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fábrica de veículos com uma linha de montagem base (classe abstrata) que produz o chassi com motor e transmissão. Cada modelo específico (sedan, SUV, pickup) usa essa linha base e adiciona seus próprios componentes.

A linha base precisa estar **acessível** aos modelos específicos para que eles possam "herdar" o chassi montado. Se a linha base fosse completamente fechada (privada), nenhum modelo conseguiria usar o chassi pré-montado - cada um teria que montar do zero.

Além disso, a linha base precisa de **duas saídas**: uma "saída vazia" (construtor sem parâmetros) para quando o modelo vai adicionar componentes um a um durante a montagem, e uma "saída completa" (construtor com parâmetros) para quando o chassi já vem pronto de outro lugar (reconstitution).

### O Problema Técnico

Em classes abstratas de entidades, precisamos definir construtores que:

1. Permitam que classes filhas inicializem o estado herdado
2. Não permitam instanciação direta da classe abstrata
3. Suportem o padrão de validação incremental em RegisterNew
4. Suportem o padrão de reconstitution (re-hydration) de dados persistidos

```csharp
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    // Qual visibilidade usar para os construtores?
    // A classe filha precisa de construtor vazio para RegisterNew
    // E construtor completo para CreateFromExistingInfo
}

public sealed class Employee : Person
{
    public string EmployeeNumber { get; private set; }

    // Employee precisa chamar base() em ambos os construtores
    private Employee() : base() { }  // Para RegisterNew
    private Employee(...) : base(...) { }  // Para reconstitution
}
```

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos usa uma das seguintes abordagens:

**1. Apenas Construtor Completo na Classe Abstrata**

```csharp
public abstract class Person
{
    public string FirstName { get; private set; }

    protected Person(string firstName)
    {
        FirstName = firstName;
    }
}

public sealed class Employee : Person
{
    // ❌ Forçado a passar valor placeholder inválido
    private Employee() : base(null!) { }  // Quebra "estado inválido nunca existe"
}
```

**2. Setters Protegidos**

```csharp
public abstract class Person
{
    // ❌ Setter protegido - filha pode alterar a qualquer momento
    public string FirstName { get; protected set; }
}

public sealed class Employee : Person
{
    public Employee(string firstName, string empNumber)
    {
        FirstName = firstName;  // Funciona, mas quebra encapsulamento
        EmployeeNumber = empNumber;
    }
}
```

### Por Que Não Funciona Bem

**Problema 1: Apenas Construtor Completo Força Valores Placeholder**

```csharp
public sealed class Employee : Person
{
    // ❌ Precisa passar null! ou string.Empty para base()
    private Employee() : base(null!, null!) { }

    public static Employee? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        var instance = new Employee();  // Estado inicial com null!
        // Valida e atribui propriedade por propriedade...
    }
}
```

Isso viola o princípio de "estado inválido nunca existe na memória" (ADR DE-004).

**Problema 2: Setters Protegidos Quebram Encapsulamento**

```csharp
public sealed class Employee : Person
{
    public void AlgumMetodo()
    {
        // ❌ Pode alterar FirstName a qualquer momento, sem validação
        FirstName = "Qualquer coisa";
    }
}
```

A classe filha pode modificar o estado da pai fora dos métodos de negócio apropriados.

## A Decisão

### Nossa Abordagem

Classes abstratas têm **DOIS construtores protegidos**:

```csharp
public abstract class Person : EntityBase<Person>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUTOR 1: Vazio e Protegido - Para RegisterNew
    // ═══════════════════════════════════════════════════════════════════════════
    protected Person()
    {
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUTOR 2: Completo e Protegido - Para Reconstitution e Clone
    // ═══════════════════════════════════════════════════════════════════════════
    protected Person(
        EntityInfo entityInfo,
        string firstName,
        string lastName
    ) : base(entityInfo)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}

public sealed class Employee : Person
{
    public string EmployeeNumber { get; private set; } = string.Empty;

    // Construtor vazio privado chama base() vazio
    private Employee() : base()  // ✅ Sem valores placeholder
    {
    }

    // Construtor completo privado chama base(...) completo
    private Employee(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        string employeeNumber
    ) : base(entityInfo, firstName, lastName)  // ✅ Inicializa estado herdado
    {
        EmployeeNumber = employeeNumber;
    }

    // Factory method para criação com validação
    public static Employee? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        var instance = new Employee();  // ✅ Usa construtor vazio limpo
        // Valida e atribui propriedade por propriedade via *Internal...
        return instance;
    }

    // Factory method para reconstitution
    public static Employee CreateFromExistingInfo(CreateFromExistingInfoInput input)
    {
        return new Employee(
            input.EntityInfo,
            input.FirstName,
            input.LastName,
            input.EmployeeNumber
        );
    }
}
```

### Diferença de Visibilidade por Tipo de Classe

| Tipo de Classe | Construtor Vazio | Construtor Completo |
|----------------|------------------|---------------------|
| **Concreta (sealed)** | `private` - para validação incremental em RegisterNew | `private` - usado em Clone e CreateFromExistingInfo |
| **Abstrata** | `protected` - filhas precisam chamar base() | `protected` - filhas precisam chamar base(...) |

### Por Que Construtor Vazio É Necessário em Classes Abstratas

Em classes concretas sealed que herdam de abstratas, o construtor vazio existe para permitir **validação incremental** no `RegisterNew`:

```csharp
public sealed class Employee : Person
{
    private Employee() : base() { }  // ✅ Chama construtor vazio protegido da pai

    public static Employee? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        var instance = new Employee();  // Construtor vazio

        // Valida e atribui via métodos *Internal
        instance.ChangeNameInternal(ctx, input.FirstName, input.LastName);
        instance.ChangeEmployeeNumberInternal(ctx, input.EmployeeNumber);

        return instance;
    }
}
```

Se a classe abstrata não tiver construtor vazio, a filha seria forçada a passar valores placeholder:

```csharp
// ❌ SEM construtor vazio na pai
private Employee() : base(null!, null!) { }  // Viola DE-004

// ✅ COM construtor vazio na pai
private Employee() : base() { }  // Estado inicial limpo
```

### Fluxo de RegisterNew com Hierarquia

```
┌─────────────────────────────────────────────────────────────────┐
│                    REGISTER NEW                                  │
│                    (Validação Incremental)                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Employee.RegisterNew(ctx, input)                               │
│      │                                                          │
│      ▼                                                          │
│  var instance = new Employee();                                 │
│      │                                                          │
│      ▼                                                          │
│  Employee() : base()  ◄── Construtor vazio privado              │
│      │                                                          │
│      ▼                                                          │
│  Person()  ◄── Construtor vazio PROTEGIDO da pai                │
│      │                                                          │
│      ▼                                                          │
│  instance.ChangeNameInternal(ctx, firstName, lastName)          │
│  instance.ChangeEmployeeNumberInternal(ctx, empNumber)          │
│      │                                                          │
│      ▼                                                          │
│  return instance;  // Estado válido, validado incrementalmente  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Fluxo de Reconstitution (Re-hydration)

```
┌─────────────────────────────────────────────────────────────────┐
│                    RECONSTITUTION                               │
│                    (CreateFromExistingInfo)                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Repository carrega dados do banco:                             │
│                                                                 │
│  var dto = _db.Query("SELECT * FROM Employees WHERE Id = @id"); │
│                                                                 │
│  Employee.CreateFromExistingInfo(                               │
│      new CreateFromExistingInfoInput(                           │
│          dto.EntityInfo,                                        │
│          dto.FirstName,     ─┐                                  │
│          dto.LastName,       ├── Propriedades da classe pai     │
│          dto.EmployeeNumber ─┘── Propriedade da classe filha    │
│      )                                                          │
│  );                                                             │
│      │                                                          │
│      ▼                                                          │
│  new Employee(entityInfo, firstName, lastName, employeeNumber)  │
│      │                                                          │
│      ▼                                                          │
│  : base(entityInfo, firstName, lastName)  ◄── Construtor        │
│      │                                        completo da pai   │
│      ▼                                                          │
│  Person.FirstName = firstName                                   │
│  Person.LastName = lastName                                     │
│  Employee.EmployeeNumber = employeeNumber                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Consequências

### Benefícios

- **Estado Sempre Válido**: Sem valores placeholder (null!) em construtores vazios
- **Encapsulamento Preservado**: Setters permanecem privados, estado só muda via construtores ou *Internal
- **Validação Incremental**: Classe filha pode usar RegisterNew com validação propriedade por propriedade
- **Reconstitution Funcional**: Classes filhas podem reconstruir entidades completas
- **Inicialização Garantida**: Impossível criar instância com estado da pai não inicializado

### Trade-offs

- **Dois Construtores na Classe Abstrata**: Mais código na classe pai
- **Acoplamento de Assinatura**: Mudança no construtor completo da pai afeta todas as filhas

### Relação com Outras ADRs

| ADR | Relação |
|-----|---------|
| DE-004 | Construtor vazio evita passar null! que violaria "estado inválido nunca existe" |
| DE-020 | Classes concretas têm dois construtores privados; abstratas têm dois protegidos |
| DE-047 | Set* permanece privado - construtores protegidos são a forma de inicializar estado da pai |
| DE-018 | Reconstitution não valida - construtor completo apenas atribui valores |

## Fundamentação Teórica

### Princípio de Substituição de Liskov (LSP)

Os construtores protegidos garantem que toda subclasse inicialize corretamente o estado da classe base. Isso é essencial para que instâncias da filha possam ser usadas onde a pai é esperada.

### Princípio da Responsabilidade Única

A classe abstrata é responsável por definir **como** seu estado é inicializado. A classe filha é responsável por **quando** chamar essa inicialização (em seu próprio construtor ou factory method).

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que classes abstratas precisam de construtor vazio protegido?"
- "Qual a diferença entre protected e internal para construtores em hierarquias?"
- "Como evitar passar null! em chamadas base() de construtores?"

### Leitura Recomendada

- ADR DE-004: Estado Inválido Nunca Existe na Memória
- ADR DE-020: Dois Construtores Privados (Vazio e Completo)
- ADR DE-018: Reconstitution Não Valida Dados
- ADR DE-047: Métodos Set* Privados em Classes Abstratas

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que recebe EntityInfo no construtor |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_GUIDANCE sobre construtores em classes abstratas
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - declaração `protected AbstractAggregateRoot()` e `protected AbstractAggregateRoot(EntityInfo, string)`
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - comparação com construtores privados em classe concreta
