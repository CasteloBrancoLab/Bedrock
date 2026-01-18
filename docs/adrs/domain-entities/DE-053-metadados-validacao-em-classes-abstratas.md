# DE-053: Metadados de Validação em Classes Abstratas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um fabricante de motores (classe abstrata) que produz motores para diferentes veículos (classes filhas: carros, motos, barcos). O motor tem suas próprias especificações de qualidade: cilindrada mínima, temperatura máxima de operação, pressão de óleo aceitável.

Essas especificações pertencem ao **motor**, não ao veículo. O fabricante de carros não define qual é a pressão de óleo aceitável do motor - isso é responsabilidade do fabricante de motores.

Se o motor não tivesse suas próprias especificações e delegasse tudo para o veículo, cada fabricante de veículo teria que "reinventar" as regras do motor, levando a inconsistências.

### O Problema Técnico

Em hierarquias de herança, surge a dúvida: onde definir os metadados de validação das propriedades da classe abstrata?

```csharp
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    // Onde ficam os metadados de FirstName e LastName?
    // Na classe abstrata Person ou nas classes filhas Employee/Customer?
}

public sealed class Employee : Person
{
    public string EmployeeNumber { get; private set; }
}

public sealed class Customer : Person
{
    public string CustomerId { get; private set; }
}
```

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos delegam toda a validação para as classes filhas:

**1. Sem Metadados na Classe Abstrata**

```csharp
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    // Nenhum metadado aqui
}

public sealed class Employee : Person
{
    // ❌ Employee define metadados de FirstName (propriedade da PAI)
    public static class EmployeeMetadata
    {
        public static int FirstNameMaxLength = 100;
        public static int LastNameMaxLength = 100;
        public static int EmployeeNumberMaxLength = 20;
    }
}

public sealed class Customer : Person
{
    // ❌ Customer também define metadados de FirstName (DUPLICAÇÃO)
    public static class CustomerMetadata
    {
        public static int FirstNameMaxLength = 150;  // Diferente de Employee!
        public static int LastNameMaxLength = 150;
        public static int CustomerIdMaxLength = 30;
    }
}
```

**2. Data Annotations na Classe Abstrata**

```csharp
public abstract class Person
{
    [MaxLength(100)]
    public string FirstName { get; private set; }

    [MaxLength(100)]
    public string LastName { get; private set; }
}
```

### Por Que Não Funciona Bem

**Problema 1: Duplicação e Inconsistência**

```csharp
// Employee diz que FirstName pode ter 100 caracteres
// Customer diz que FirstName pode ter 150 caracteres
// Qual é o correto? A regra deveria ser a mesma para todas as "Pessoas"
```

Se cada classe filha define os metadados da classe pai, regras divergentes surgem naturalmente.

**Problema 2: Data Annotations São Inflexíveis**

Data Annotations não permitem customização em runtime (ver ADR DE-012):

```csharp
// ❌ Não é possível mudar no startup para diferentes deployments
[MaxLength(100)]  // Hardcoded - imutável
public string FirstName { get; private set; }
```

**Problema 3: Violação do Princípio de Responsabilidade Única**

A classe filha não deveria ser responsável por definir as regras de validação das propriedades da classe pai. Isso quebra encapsulamento e viola SRP.

**Problema 4: Camadas Externas Não Sabem Onde Buscar**

```csharp
// Controller precisa validar FirstName
// Onde buscar o metadado?
var maxLength = ???  // Employee.EmployeeMetadata? Customer.CustomerMetadata? Person?
```

## A Decisão

### Nossa Abordagem

Classes abstratas que possuem propriedades com regras de validação **DEVEM** definir seus próprios metadados:

```csharp
public abstract class Person : EntityBase<Person>
{
    // ═══════════════════════════════════════════════════════════════════════════
    // METADADOS DA CLASSE ABSTRATA
    // ═══════════════════════════════════════════════════════════════════════════
    public static class PersonMetadata
    {
        private static readonly Lock _lockObject = new();

        // FirstName - propriedade da classe abstrata
        public static readonly string FirstNamePropertyName = nameof(FirstName);
        public static bool FirstNameIsRequired { get; private set; } = true;
        public static int FirstNameMinLength { get; private set; } = 1;
        public static int FirstNameMaxLength { get; private set; } = 100;

        // LastName - propriedade da classe abstrata
        public static readonly string LastNamePropertyName = nameof(LastName);
        public static bool LastNameIsRequired { get; private set; } = true;
        public static int LastNameMinLength { get; private set; } = 1;
        public static int LastNameMaxLength { get; private set; } = 100;

        public static void ChangeFirstNameMetadata(bool isRequired, int minLength, int maxLength)
        {
            lock (_lockObject)
            {
                FirstNameIsRequired = isRequired;
                FirstNameMinLength = minLength;
                FirstNameMaxLength = maxLength;
            }
        }
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // Validate* para propriedades da classe abstrata
    public static bool ValidateFirstName(ExecutionContext ctx, string? firstName) { ... }
    public static bool ValidateLastName(ExecutionContext ctx, string? lastName) { ... }
}

public sealed class Employee : Person
{
    // ═══════════════════════════════════════════════════════════════════════════
    // METADADOS DA CLASSE FILHA (apenas propriedades próprias)
    // ═══════════════════════════════════════════════════════════════════════════
    public static class EmployeeMetadata
    {
        // EmployeeNumber - propriedade da classe filha
        public static readonly string EmployeeNumberPropertyName = nameof(EmployeeNumber);
        public static bool EmployeeNumberIsRequired { get; private set; } = true;
        public static int EmployeeNumberMaxLength { get; private set; } = 20;
    }

    public string EmployeeNumber { get; private set; } = string.Empty;

    // Validate* apenas para propriedades próprias
    public static bool ValidateEmployeeNumber(ExecutionContext ctx, string? employeeNumber) { ... }
}
```

### Responsabilidades Claras

| Classe | Define Metadados De | Define Validate* De |
|--------|---------------------|---------------------|
| **Person (abstrata)** | FirstName, LastName | ValidateFirstName, ValidateLastName |
| **Employee (filha)** | EmployeeNumber | ValidateEmployeeNumber |
| **Customer (filha)** | CustomerId | ValidateCustomerId |

### Camadas Externas Sabem Onde Buscar

```csharp
// Controller validando dados de Employee
public IActionResult CreateEmployee(CreateEmployeeRequest request)
{
    bool isValid =
        // Metadados da classe pai
        Person.ValidateFirstName(ctx, request.FirstName)
        & Person.ValidateLastName(ctx, request.LastName)
        // Metadados da classe filha
        & Employee.ValidateEmployeeNumber(ctx, request.EmployeeNumber);

    if (!isValid)
        return BadRequest(ctx.Messages);

    // ...
}

// UI usando metadados para MaxLength de input
<input maxlength="@Person.PersonMetadata.FirstNameMaxLength" />
<input maxlength="@Employee.EmployeeMetadata.EmployeeNumberMaxLength" />
```

### Quando Metadados São Desnecessários

Metadados na classe abstrata seriam irrelevantes **APENAS** se ela fosse um simples agregador de propriedades sem lógica de validação própria:

```csharp
// Caso RARO - classe abstrata sem validação própria
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }  // Sem validação de negócio
    public DateTime CreatedAt { get; protected set; }  // Sem validação de negócio
}
```

Neste caso, a classe filha seria responsável por toda a validação. Mas este cenário é raro em entidades de domínio reais.

## Consequências

### Benefícios

- **Single Source of Truth**: Metadados de FirstName estão em Person, não duplicados em cada filha
- **Consistência**: Todas as filhas usam as mesmas regras para propriedades herdadas
- **Encapsulamento**: Classe abstrata é responsável por suas próprias propriedades
- **Descobribilidade**: Camadas externas sabem exatamente onde buscar cada metadado

### Trade-offs

- **Mais Classes Metadata**: Cada classe (abstrata e filhas) tem sua própria classe Metadata
- **Composição na Validação**: Filhas precisam chamar validações da pai explicitamente

### Relação com Outras ADRs

| ADR | Relação |
|-----|---------|
| DE-012 | Metadados estáticos vs Data Annotations - por que não usar annotations |
| DE-048 | Validate* públicos em classes abstratas - mesma lógica |
| DE-051 | Hierarquia IsValid - composição de validações pai + filha |

## Fundamentação Teórica

### Princípio de Responsabilidade Única (SRP)

Cada classe é responsável por definir as regras de suas próprias propriedades. A classe abstrata não delega essa responsabilidade para as filhas.

### Princípio de Substituição de Liskov (LSP)

Se todas as filhas usam os mesmos metadados da classe pai, qualquer instância de filha pode ser usada onde a pai é esperada, com as mesmas regras de validação.

### Don't Repeat Yourself (DRY)

Metadados centralizados na classe abstrata evitam duplicação em cada classe filha.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que metadados de validação pertencem à classe que define a propriedade?"
- "Como Single Source of Truth se aplica a hierarquias de herança?"
- "Qual a diferença entre delegar validação e compor validação?"

### Leitura Recomendada

- ADR DE-012: Metadados Estáticos vs Data Annotations
- ADR DE-016: Single Source of Truth para Regras de Validação
- ADR DE-048: Métodos Validate* Públicos em Classes Abstratas

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que demonstra o padrão de metadados |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_GUIDANCE sobre metadados em classes abstratas
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - declaração `public static class AbstractAggregateRootMetadata`
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - comparação com metadados em classe concreta
