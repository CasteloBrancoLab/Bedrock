# DE-051: Hierarquia IsValid em Classes Abstratas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um sistema de inspeção veicular com três níveis:

1. **Inspeção Básica** (pública): qualquer pessoa pode verificar se um veículo tem os documentos básicos - placa, CRLV, IPVA. Funciona para qualquer veículo.

2. **Inspeção de Plataforma** (interna): a montadora verifica se o chassi, motor e transmissão da plataforma base estão em conformidade. Só quem conhece a plataforma sabe fazer isso.

3. **Inspeção do Modelo** (específica): cada modelo (sedan, SUV, pickup) tem inspeções específicas - o SUV verifica tração 4x4, a pickup verifica capacidade de carga. Só o fabricante do modelo sabe fazer isso.

O sistema de inspeção completa COMPÕE os três níveis: primeiro a básica, depois a de plataforma, depois a específica do modelo.

### O Problema Técnico

Em classes abstratas de entidades, precisamos de um sistema de validação que:

1. Permita validação antecipada por camadas externas (controllers, serviços)
2. Valide propriedades da classe abstrata (pai)
3. Valide propriedades da classe concreta (filha)
4. Componha tudo de forma coesa

```csharp
// Como validar uma entidade que herda de classe abstrata?
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
}

public sealed class Employee : Person
{
    public string EmployeeNumber { get; private set; }
    public Department Department { get; private set; }
}

// Precisamos validar:
// 1. FirstName e LastName (propriedades da pai)
// 2. EmployeeNumber e Department (propriedades da filha)
// 3. E expor isso para camadas externas
```

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos usa uma das seguintes abordagens:

**1. Método Virtual com Override**

```csharp
public abstract class Person
{
    public virtual bool IsValid(ExecutionContext ctx)
    {
        return ValidateFirstName(ctx, FirstName)
            && ValidateLastName(ctx, LastName);
    }
}

public sealed class Employee : Person
{
    public override bool IsValid(ExecutionContext ctx)
    {
        return base.IsValid(ctx)
            && ValidateEmployeeNumber(ctx, EmployeeNumber);
    }
}
```

**2. Método Abstrato**

```csharp
public abstract class Person
{
    public abstract bool IsValid(ExecutionContext ctx);
}

public sealed class Employee : Person
{
    public override bool IsValid(ExecutionContext ctx)
    {
        // Filha precisa saber validar as propriedades da pai também
        return ValidateFirstName(ctx, FirstName)
            && ValidateLastName(ctx, LastName)
            && ValidateEmployeeNumber(ctx, EmployeeNumber);
    }
}
```

### Por Que Não Funciona Bem

**Problema 1: Métodos de Instância Não Servem Para Validação Antecipada**

```csharp
// Controller quer validar ANTES de criar a entidade
public IActionResult CreateEmployee(CreateEmployeeRequest request)
{
    // ❌ Não posso chamar employee.IsValid() porque não tenho employee ainda!

    // Preciso validar os dados ANTES de tentar criar
    if (!ValidarDadosDeAlgumaForma(request))
        return BadRequest();

    var employee = Employee.RegisterNew(ctx, request);
}
```

**Problema 2: Método Abstrato Duplica Conhecimento**

Se `IsValid` é abstrato, a classe filha precisa conhecer as propriedades da classe pai para validá-las. Isso quebra encapsulamento e duplica código.

**Problema 3: Método Virtual Permite Esquecer `base.IsValid()`**

```csharp
public sealed class Employee : Person
{
    public override bool IsValid(ExecutionContext ctx)
    {
        // ❌ Desenvolvedor esqueceu de chamar base.IsValid()
        return ValidateEmployeeNumber(ctx, EmployeeNumber);
        // FirstName e LastName não são validados!
    }
}
```

## A Decisão

### Nossa Abordagem: Três Métodos com Responsabilidades Distintas

Classes abstratas implementam uma **hierarquia de três métodos** para validação:

```csharp
public abstract class Person : EntityBase<Person>
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // 1. IsValid ESTÁTICO PÚBLICO - Validação Antecipada
    // ═══════════════════════════════════════════════════════════════════════════
    // Permite que camadas externas validem dados ANTES de criar/modificar entidade
    public static bool IsValid(
        ExecutionContext ctx,
        EntityInfo entityInfo,
        string? firstName,
        string? lastName
    )
    {
        return EntityBaseIsValid(ctx, entityInfo)
            & ValidateFirstName(ctx, firstName)
            & ValidateLastName(ctx, lastName);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 2. IsValidInternal PROTEGIDO - Orquestra Validação Completa
    // ═══════════════════════════════════════════════════════════════════════════
    // Valida propriedades da pai + chama IsValidConcreteInternal da filha
    protected override bool IsValidInternal(ExecutionContext ctx)
    {
        return IsValid(ctx, EntityInfo, FirstName, LastName)
            && IsValidConcreteInternal(ctx);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 3. IsValidConcreteInternal ABSTRATO - Validação da Classe Filha
    // ═══════════════════════════════════════════════════════════════════════════
    // Classe filha DEVE implementar para validar suas propriedades específicas
    protected abstract bool IsValidConcreteInternal(ExecutionContext ctx);
}

public sealed class Employee : Person
{
    public string EmployeeNumber { get; private set; }

    // Classe filha expõe seu próprio IsValid estático público
    public static bool IsValid(
        ExecutionContext ctx,
        EntityInfo entityInfo,
        string? firstName,
        string? lastName,
        string? employeeNumber
    )
    {
        // Compõe validação da pai + validação própria
        return Person.IsValid(ctx, entityInfo, firstName, lastName)
            & ValidateEmployeeNumber(ctx, employeeNumber);
    }

    // Implementa o método abstrato da pai
    protected override bool IsValidConcreteInternal(ExecutionContext ctx)
    {
        return ValidateEmployeeNumber(ctx, EmployeeNumber);
    }
}
```

### Os Três Métodos Explicados

| Método | Visibilidade | Tipo | Responsabilidade |
|--------|-------------|------|------------------|
| `IsValid` (estático) | **public static** | Concreto | Validação antecipada - camadas externas validam dados antes de criar entidade |
| `IsValidInternal` | **protected override** | Concreto | Orquestra validação completa - chama IsValid da pai + IsValidConcreteInternal |
| `IsValidConcreteInternal` | **protected abstract** | Abstrato | Ponto de extensão - filha implementa para validar suas propriedades |

### Fluxo de Validação

```
┌─────────────────────────────────────────────────────────────────┐
│                    VALIDAÇÃO ANTECIPADA                         │
│                    (Camadas Externas)                           │
├─────────────────────────────────────────────────────────────────┤
│  Controller/Service chama:                                      │
│  Employee.IsValid(ctx, entityInfo, firstName, lastName, empNum) │
│      │                                                          │
│      ├── Person.IsValid(ctx, entityInfo, firstName, lastName)   │
│      │       │                                                  │
│      │       ├── EntityBaseIsValid(ctx, entityInfo)             │
│      │       ├── ValidateFirstName(ctx, firstName)              │
│      │       └── ValidateLastName(ctx, lastName)                │
│      │                                                          │
│      └── ValidateEmployeeNumber(ctx, employeeNumber)            │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    VALIDAÇÃO DE INSTÂNCIA                       │
│                    (EntityBase.IsValid)                         │
├─────────────────────────────────────────────────────────────────┤
│  entity.IsValid(ctx) chama IsValidInternal:                     │
│                                                                 │
│  Person.IsValidInternal(ctx)                                    │
│      │                                                          │
│      ├── Person.IsValid(ctx, EntityInfo, FirstName, LastName)   │
│      │       │                                                  │
│      │       ├── EntityBaseIsValid(ctx, EntityInfo)             │
│      │       ├── ValidateFirstName(ctx, FirstName)              │
│      │       └── ValidateLastName(ctx, LastName)                │
│      │                                                          │
│      └── Employee.IsValidConcreteInternal(ctx)                  │
│              │                                                  │
│              └── ValidateEmployeeNumber(ctx, EmployeeNumber)    │
└─────────────────────────────────────────────────────────────────┘
```

### Por Que IsValid Estático é Público (Não Protegido)

Embora a ADR DE-050 diga que classes abstratas não expõem métodos de negócio públicos, **validação não é operação de negócio** - é infraestrutura de suporte.

```csharp
// Controller precisa validar ANTES de criar
public IActionResult CreateEmployee(CreateEmployeeRequest request)
{
    // ✅ IsValid estático público permite validação antecipada
    if (!Employee.IsValid(ctx, entityInfo, request.FirstName, request.LastName, request.EmployeeNumber))
        return BadRequest(ctx.Messages);

    var employee = Employee.RegisterNew(ctx, new RegisterEmployeeInput(...));
    return Ok(employee);
}
```

Se `IsValid` fosse protegido, seria impossível validar dados antes de ter uma instância.

### Por Que IsValidConcreteInternal é Abstrato (Não Virtual)

```csharp
// ❌ VIRTUAL - Filha pode "esquecer" de implementar
protected virtual bool IsValidConcreteInternal(ExecutionContext ctx)
{
    return true;  // Default "vazio" - perigoso!
}

// ✅ ABSTRACT - Compilador OBRIGA a filha a implementar
protected abstract bool IsValidConcreteInternal(ExecutionContext ctx);
```

O método abstrato garante que **toda classe concreta** valide suas próprias propriedades.

## Consequências

### Benefícios

- **Validação Antecipada**: Camadas externas validam dados antes de criar entidades
- **Composição Clara**: Cada nível valida apenas suas próprias propriedades
- **Garantia de Completude**: Método abstrato força a filha a validar suas propriedades
- **Sem Duplicação**: `IsValidInternal` reutiliza `IsValid` estático

### Trade-offs

- **Três Métodos**: Mais métodos para entender e manter
- **Convenção Por Código**: Filha deve chamar `Person.IsValid` em seu próprio `IsValid` estático (verificável via Roslyn ou code review)

### Diferença de Visibilidade por Tipo de Classe

| Tipo de Classe | `IsValidConcreteInternal` |
|----------------|---------------------------|
| **Concreta (sealed, não herda de abstrata)** | `private` - não há extensão |
| **Abstrata** | `protected abstract` - filha deve implementar |

## Fundamentação Teórica

### Template Method Pattern

O `IsValidInternal` é um Template Method: define o "esqueleto" da validação (validar pai + chamar método abstrato da filha) enquanto delega a validação específica para a filha via `IsValidConcreteInternal`.

### Strategy Pattern (Implícito)

Cada classe concreta é uma "estratégia" de validação específica que se encaixa no template definido pela classe abstrata.

### Liskov Substitution Principle (LSP)

Qualquer subclasse de `Person` pode ser usada onde `Person` é esperada, e a validação funcionará corretamente porque o `IsValidInternal` garante que tanto as propriedades da pai quanto da filha são validadas.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre método virtual e abstrato para pontos de extensão?"
- "Por que validação antecipada precisa de métodos estáticos?"
- "Como o Template Method Pattern se aplica a hierarquias de validação?"

### Leitura Recomendada

- [Template Method Pattern - GoF](https://refactoring.guru/design-patterns/template-method)
- ADR DE-009: Métodos Validate* Públicos e Estáticos
- ADR DE-050: Classe Abstrata Não Expõe Métodos Públicos de Negócio

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Define `IsValidInternal` que classes abstratas sobrescrevem |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_GUIDANCE sobre IsValid estático público
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - declaração `public static bool IsValid`
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - declaração `protected override bool IsValidInternal`
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - declaração `protected abstract bool IsValidConcreteInternal`
