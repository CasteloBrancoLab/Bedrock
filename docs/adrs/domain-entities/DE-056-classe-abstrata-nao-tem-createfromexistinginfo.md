# DE-056: Classe Abstrata Não Tem CreateFromExistingInfo

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma **fábrica de veículos**:

**Cenário problemático**:
A fábrica tem uma linha de montagem abstrata para "Veículo" que tenta montar um veículo genérico. Mas um veículo genérico não existe - você só pode montar um Carro, uma Moto ou um Caminhão.

**Cenário correto**:
A linha de montagem de "Veículo" define as peças comuns (motor, rodas, chassi), mas a montagem final acontece nas linhas específicas de Carro, Moto ou Caminhão.

Em software, classes abstratas definem estrutura comum, mas a reconstituição só faz sentido nas classes concretas.

---

### O Problema Técnico

Classes abstratas não podem ser instanciadas diretamente. Portanto, um método `CreateFromExistingInfo` na classe abstrata não teria como retornar uma instância:

```csharp
// ❌ IMPOSSÍVEL: Classe abstrata não pode ser instanciada
public abstract class Customer
{
    public static Customer CreateFromExistingInfo(
        CreateFromExistingInfoCustomerInput input
    )
    {
        // O que retornar aqui?
        // return new Customer(...); // ❌ Não compila - Customer é abstract
    }
}
```

## A Decisão

### Nossa Abordagem

**Classes abstratas NÃO expõem método CreateFromExistingInfo**:

```csharp
public abstract class Customer : EntityBase<Customer>, IAggregateRoot
{
    // ❌ NÃO TEM CreateFromExistingInfo

    // ✅ Construtor protegido para filhas usarem em reconstitution
    protected Customer(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        string fullName,
        EmailAddress emailAddress,
        PersonType personType,
        CustomerStatus status,
        string documentNumber
    ) : base(entityInfo)
    {
        FirstName = firstName;
        LastName = lastName;
        FullName = fullName;
        EmailAddress = emailAddress;
        PersonType = personType;
        Status = status;
        DocumentNumber = documentNumber;
    }
}
```

**Classes filhas (concretas) implementam seu próprio CreateFromExistingInfo**:

```csharp
public sealed class Individual : Customer
{
    // ✅ Classe concreta tem CreateFromExistingInfo
    public static Individual CreateFromExistingInfo(
        CreateFromExistingInfoIndividualInput input
    )
    {
        return new Individual(
            input.EntityInfo,
            input.FirstName,
            input.LastName,
            input.FullName,
            input.EmailAddress,
            input.PersonType,
            input.Status,
            input.DocumentNumber,
            input.BirthDate  // Propriedade específica de Individual
        );
    }

    private Individual(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        string fullName,
        EmailAddress emailAddress,
        PersonType personType,
        CustomerStatus status,
        string documentNumber,
        BirthDate? birthDate
    ) : base(entityInfo, firstName, lastName, fullName, emailAddress, personType, status, documentNumber)
    {
        BirthDate = birthDate;
    }
}
```

### Razões Técnicas

#### 1. Classes Abstratas Não São Instanciáveis

O princípio fundamental de OOP: classes abstratas existem para serem herdadas, não instanciadas.

```csharp
// Compile-time error
var customer = new Customer(...); // ❌ Cannot create instance of abstract type

// Só podemos criar instâncias de tipos concretos
var individual = new Individual(...); // ✅
var legalEntity = new LegalEntity(...); // ✅
```

#### 2. Cada Classe Concreta Tem Propriedades Específicas

Reconstitution precisa de TODAS as propriedades, incluindo as específicas:

```csharp
// Individual tem BirthDate
public readonly record struct CreateFromExistingInfoIndividualInput(
    EntityInfo EntityInfo,
    string FirstName,
    string LastName,
    string FullName,
    EmailAddress EmailAddress,
    PersonType PersonType,
    CustomerStatus Status,
    string DocumentNumber,
    BirthDate? BirthDate  // ✅ Propriedade específica
);

// LegalEntity tem PhoneNumber
public readonly record struct CreateFromExistingInfoLegalEntityInput(
    EntityInfo EntityInfo,
    string FirstName,
    string LastName,
    string FullName,
    EmailAddress EmailAddress,
    PersonType PersonType,
    CustomerStatus Status,
    string DocumentNumber,
    PhoneNumber PhoneNumber  // ✅ Propriedade específica
);
```

#### 3. Repository Conhece o Tipo Concreto

O repository sabe qual tipo concreto está persistido e usa o factory method apropriado:

```csharp
public class CustomerRepository : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        var dto = await _db.QueryAsync(id);

        // Repository conhece o tipo pelo PersonType ou discriminator
        return dto.PersonType switch
        {
            PersonType.Individual => Individual.CreateFromExistingInfo(
                new CreateFromExistingInfoIndividualInput(
                    dto.EntityInfo,
                    dto.FirstName,
                    dto.LastName,
                    dto.FullName,
                    dto.EmailAddress,
                    dto.PersonType,
                    dto.Status,
                    dto.DocumentNumber,
                    dto.BirthDate
                )
            ),
            PersonType.LegalEntity => LegalEntity.CreateFromExistingInfo(
                new CreateFromExistingInfoLegalEntityInput(
                    dto.EntityInfo,
                    dto.FirstName,
                    dto.LastName,
                    dto.FullName,
                    dto.EmailAddress,
                    dto.PersonType,
                    dto.Status,
                    dto.DocumentNumber,
                    dto.PhoneNumber
                )
            ),
            _ => throw new InvalidOperationException($"Unknown PersonType: {dto.PersonType}")
        };
    }
}
```

#### 4. Construtor Protegido É Suficiente

A classe abstrata fornece um construtor protegido que as filhas usam:

```csharp
public abstract class Customer
{
    // Construtor protegido - filhas usam para inicializar propriedades da base
    protected Customer(
        EntityInfo entityInfo,
        string firstName,
        ...
    ) : base(entityInfo)
    {
        FirstName = firstName;
        ...
    }
}

public sealed class Individual : Customer
{
    private Individual(..., BirthDate? birthDate)
        : base(entityInfo, firstName, ...)  // ✅ Usa construtor protegido da pai
    {
        BirthDate = birthDate;  // Inicializa propriedade própria
    }
}
```

### Benefícios

1. **Type Safety**: Não há tentativa de instanciar classe abstrata
2. **Clareza**: Cada classe concreta define claramente seu reconstitution
3. **Completude**: Input inclui todas as propriedades (base + específicas)
4. **Flexibilidade**: Cada classe pode ter lógica de reconstitution específica se necessário

### Trade-offs

- **Duplicação de Propriedades nos Inputs**: Inputs das classes concretas repetem as propriedades da base
  - **Mitigação**: Isso é intencional - cada input é auto-contido e documenta completamente o que é necessário

## Fundamentação Teórica

### Princípio de Classes Abstratas

Classes abstratas representam conceitos incompletos que precisam ser especializados:

> "An abstract class is a class that is declared abstract—it may or may not include abstract methods. Abstract classes cannot be instantiated, but they can be subclassed."
>
> *Uma classe abstrata é uma classe declarada como abstract—pode ou não incluir métodos abstratos. Classes abstratas não podem ser instanciadas, mas podem ser herdadas.*
>
> — Java Documentation (conceito aplicável a C#)

### Factory Method Pattern

Gang of Four em "Design Patterns" (1994):

> "Define an interface for creating an object, but let subclasses decide which class to instantiate."
>
> *Defina uma interface para criar um objeto, mas deixe as subclasses decidirem qual classe instanciar.*

`CreateFromExistingInfo` é um factory method - e por definição, a decisão de qual classe instanciar pertence às classes concretas.

### Liskov Substitution Principle

Se a classe abstrata tivesse `CreateFromExistingInfo` retornando `Customer`, quebraria o princípio:

```csharp
// ❌ VIOLAÇÃO: Retorno genérico perde informação de tipo
public abstract class Customer
{
    public static Customer CreateFromExistingInfo(CustomerDto dto)
    {
        // Teria que usar switch/if para decidir o tipo
        // Isso acopla a classe abstrata aos tipos concretos
    }
}

// ✅ CORRETO: Cada tipo concreto retorna seu próprio tipo
public sealed class Individual : Customer
{
    public static Individual CreateFromExistingInfo(...) // Retorna Individual, não Customer
}
```

## Antipadrões Documentados

### Antipadrão 1: CreateFromExistingInfo na Classe Abstrata

```csharp
// ❌ Tenta ter CreateFromExistingInfo na classe abstrata
public abstract class Customer
{
    public static Customer CreateFromExistingInfo(object dto)
    {
        // Precisa de switch para saber qual tipo criar
        if (dto is IndividualDto individual)
            return Individual.CreateFromExistingInfo(individual);
        if (dto is LegalEntityDto legalEntity)
            return LegalEntity.CreateFromExistingInfo(legalEntity);

        throw new ArgumentException("Unknown type");
        // ❌ Classe abstrata conhece todos os tipos concretos - alto acoplamento
    }
}
```

### Antipadrão 2: Input Genérico Com Properties Opcionais

```csharp
// ❌ Um input para todos os tipos com propriedades nullable
public readonly record struct CreateFromExistingInfoCustomerInput(
    EntityInfo EntityInfo,
    string FirstName,
    // ... propriedades comuns ...
    BirthDate? BirthDate,     // null se for LegalEntity
    PhoneNumber? PhoneNumber  // null se for Individual
);
// ❌ Confuso - quais propriedades são realmente necessárias?
```

### Antipadrão 3: Input de Reconstitution Para Classe Abstrata

```csharp
// ❌ NÃO crie input de reconstitution para classe abstrata
public readonly record struct CreateFromExistingInfoCustomerInput(
    EntityInfo EntityInfo,
    string FirstName,
    string LastName,
    // ... propriedades da classe base
);
// ❌ Este input não tem uso - classe abstrata não tem CreateFromExistingInfo

// ✅ Apenas classes concretas têm inputs de reconstitution
public readonly record struct CreateFromExistingInfoIndividualInput(...);
public readonly record struct CreateFromExistingInfoLegalEntityInput(...);
```

## Decisões Relacionadas

- [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) - Separação RegisterNew vs CreateFromExistingInfo
- [DE-018](./DE-018-reconstitution-nao-valida-dados.md) - Reconstitution não valida dados
- [DE-019](./DE-019-input-objects-pattern.md) - Input Objects Pattern
- [DE-052](./DE-052-construtores-protegidos-em-classes-abstratas.md) - Construtores protegidos em classes abstratas

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - LLM_RULE: Reconstitution É Responsabilidade da Classe Concreta
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - LLM_RULE: Classe Abstrata Não Tem Input de Reconstitution
- [LeafAggregateRootTypeA.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/LeafAggregateRootTypeA.cs) - Implementação de CreateFromExistingInfo em classe concreta
- [LeafAggregateRootTypeB.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/LeafAggregateRootTypeB.cs) - Implementação de CreateFromExistingInfo em classe concreta
