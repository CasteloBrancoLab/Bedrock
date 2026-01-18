# DE-050: Classe Abstrata Não Expõe Métodos Públicos de Negócio

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fábrica de automóveis com uma plataforma base (classe abstrata) usada por vários modelos (classes derivadas): sedan, SUV, pickup. A plataforma define o chassi, motor e transmissão.

Se a plataforma base já viesse com o volante instalado em posição fixa, todos os carros teriam o volante no mesmo lugar. Mas e se o SUV precisar de um volante mais alto? E a pickup de um volante mais inclinado?

A solução é a plataforma fornecer o **mecanismo de direção** (infraestrutura), mas deixar cada modelo **instalar seu próprio volante** (API pública) na posição ideal para seu uso.

### O Problema Técnico

Em hierarquias de herança, é tentador definir métodos públicos de negócio na classe abstrata:

```csharp
// ❌ Abordagem comum - método público na classe abstrata
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    public Person? ChangeName(ExecutionContext ctx, string firstName, string lastName)
    {
        // Lógica definida na classe abstrata
        // ...
    }
}

public sealed class Employee : Person
{
    // Employee HERDA ChangeName, não pode customizar
    // E se Employee precisar de lógica adicional?
}
```

O raciocínio parece lógico: "todas as pessoas podem mudar de nome, então defino uma vez na classe base".

Mas isso ignora que classes filhas podem ter **necessidades diferentes** para a mesma operação.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos define métodos públicos na classe abstrata para "reutilização":

```csharp
// Abordagem comum - API pública definida na classe abstrata
public abstract class Person
{
    public Person? ChangeName(ExecutionContext ctx, string firstName, string lastName)
    {
        // Validação e lógica aqui
        var clone = this.Clone();
        clone.SetFirstName(firstName);
        clone.SetLastName(lastName);
        return clone;
    }
}

public sealed class Employee : Person
{
    // Herda ChangeName da classe pai
    // Não pode adicionar lógica específica de Employee
}

public sealed class Customer : Person
{
    // Também herda ChangeName
    // Também não pode customizar
}
```

### Por Que Não Funciona Bem

**1. Impossível Customizar na Classe Filha**

```csharp
public sealed class Employee : Person
{
    public Department Department { get; private set; }

    // ❌ Employee precisa notificar o departamento quando muda de nome
    // Mas ChangeName está definido na classe pai!

    // Não pode fazer isso:
    public override Person? ChangeName(ExecutionContext ctx, string firstName, string lastName)
    {
        var result = base.ChangeName(ctx, firstName, lastName);
        NotifyDepartment();  // Lógica específica de Employee
        return result;
    }
    // ChangeName não é virtual, e mesmo se fosse, retorna Person, não Employee
}
```

**2. Assinatura Inflexível**

```csharp
// Classe abstrata define a assinatura
public abstract class Person
{
    public Person? ChangeName(ExecutionContext ctx, string firstName, string lastName)
    {
        // ...
    }
}

// Employee quer usar Input Object, mas não pode
public sealed class Employee : Person
{
    // ❌ Não pode ter sua própria assinatura
    public Employee? ChangeName(ExecutionContext ctx, ChangeEmployeeNameInput input)
    {
        // Conflito com método herdado!
    }
}
```

**3. Retorno Genérico Demais**

```csharp
// Método retorna Person?, não o tipo concreto
Person? result = employee.ChangeName(ctx, "João", "Silva");

// Consumidor precisa fazer cast
Employee? updatedEmployee = (Employee?)result;  // 💥 Feio e propenso a erros
```

**4. Viola o Princípio Open/Closed**

A classe abstrata está "fechada" para extensão nessa operação. Classes filhas não podem estender o comportamento sem usar gambiarras (virtual + override, que trazem seus próprios problemas).

## A Decisão

### Nossa Abordagem

Classes abstratas **NÃO expõem métodos de negócio públicos**. Elas fornecem:

- **Validate*** → públicos estáticos (validação antecipada)
- ***Internal** → protegidos (operações completas sobre estado da pai)
- **Set*** → privados (atribuições individuais)

A classe filha **COMPÕE** essas peças para criar sua própria API pública:

```csharp
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    // Validação pública - reutilizável
    public static bool ValidateFirstName(ExecutionContext ctx, string? firstName) { ... }
    public static bool ValidateLastName(ExecutionContext ctx, string? lastName) { ... }

    // *Internal protegido - operação completa sobre estado da pai
    protected bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName)
    {
        return SetFirstName(ctx, firstName)
            & SetLastName(ctx, lastName);
    }

    // Set* privados - encapsulados
    private bool SetFirstName(ExecutionContext ctx, string firstName) { ... }
    private bool SetLastName(ExecutionContext ctx, string lastName) { ... }
}

public sealed class Employee : Person
{
    public Department Department { get; private set; }

    // ✅ Employee define SUA própria API pública
    public Employee? ChangeName(ExecutionContext ctx, ChangeEmployeeNameInput input)
    {
        return RegisterChangeInternal<Employee, ChangeEmployeeNameInput>(
            ctx,
            instance: this,
            input,
            handler: static (ctx, input, newInstance) =>
            {
                // Usa infraestrutura da classe pai
                bool nameChanged = newInstance.ChangeNameInternal(ctx, input.FirstName, input.LastName);

                // Adiciona lógica específica de Employee
                if (nameChanged)
                    newInstance.NotifyDepartmentInternal(ctx);

                return nameChanged;
            }
        );
    }
}

public sealed class Customer : Person
{
    // ✅ Customer define SUA própria API pública (pode ser diferente)
    public Customer? UpdatePersonalInfo(ExecutionContext ctx, UpdatePersonalInfoInput input)
    {
        return RegisterChangeInternal<Customer, UpdatePersonalInfoInput>(
            ctx,
            instance: this,
            input,
            handler: static (ctx, input, newInstance) =>
            {
                // Usa infraestrutura da classe pai
                return newInstance.ChangeNameInternal(ctx, input.FirstName, input.LastName);
                // Customer não precisa notificar ninguém
            }
        );
    }
}
```

### Por Que Funciona Melhor

1. **Flexibilidade Total**: Cada classe filha define sua própria assinatura, Input Object e lógica adicional
2. **Retorno Tipado**: `Employee.ChangeName()` retorna `Employee?`, não `Person?`
3. **Composição**: Classes filhas compõem a infraestrutura da pai, não herdam comportamento rígido
4. **Open/Closed**: Classe abstrata é "fechada" para modificação, mas "aberta" para extensão via composição

## Consequências

### Benefícios

- **Autonomia da Classe Filha**: Define sua própria API sem restrições da pai
- **Type Safety**: Retornos são do tipo concreto, não do tipo abstrato
- **Flexibilidade**: Cada filha pode ter assinaturas diferentes para operações similares
- **Testabilidade**: APIs públicas são testadas na classe concreta, não na abstrata

### Trade-offs

- **Mais Código**: Cada classe filha define seus próprios métodos públicos
- **Possível Duplicação**: Assinaturas similares podem aparecer em várias filhas

### Trade-offs Frequentemente Superestimados

**"Preciso garantir que todas as filhas tenham o mesmo método"**

Se você precisa de uma interface comum, use uma **interface**:

```csharp
public interface INameChangeable
{
    bool ChangeName(ExecutionContext ctx, string firstName, string lastName);
}

public sealed class Employee : Person, INameChangeable
{
    public bool ChangeName(ExecutionContext ctx, string firstName, string lastName)
    {
        // Implementação específica de Employee
    }
}
```

A interface garante a existência do método. A classe abstrata não precisa defini-lo.

**"Vou ter que repetir a mesma lógica em cada filha"**

A lógica **compartilhada** vai no `*Internal` protegido. A lógica **específica** vai no método público da filha. Não há repetição real - cada filha adiciona apenas o que é específico dela.

## Fundamentação Teórica

### Composição Sobre Herança

O princípio "Favor Composition Over Inheritance" (GoF, 1994) se aplica aqui. A classe filha não **herda** comportamento público da pai - ela **compõe** usando os métodos protegidos.

### Interface Segregation Principle (ISP)

Classes filhas não são forçadas a expor métodos que não fazem sentido para elas. Se `Customer` não precisa de `ChangeName` (talvez use `UpdatePersonalInfo`), não é obrigada a ter.

### Template Method Pattern (Invertido)

O Template Method clássico define o "esqueleto" na classe abstrata e deixa detalhes para as filhas. Aqui invertemos: a classe filha define o "esqueleto" (método público) e usa peças da pai (*Internal) para compor.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre herdar comportamento e compor usando infraestrutura?"
- "Como o princípio de composição sobre herança se aplica a Domain Entities?"
- "Por que interfaces são melhores que classes abstratas para definir contratos públicos?"

### Leitura Recomendada

- [Effective Java - Item 18: Favor composition over inheritance](https://www.oreilly.com/library/view/effective-java/9780134686097/)
- [Design Patterns - GoF: Composite Pattern](https://en.wikipedia.org/wiki/Composite_pattern)
- [Interface Segregation Principle](https://en.wikipedia.org/wiki/Interface_segregation_principle)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece RegisterChangeInternal que as filhas usam para compor seus métodos públicos |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_GUIDANCE sobre métodos públicos de negócio
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_RULE sobre classe filha definir sua API
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_RULE sobre classe abstrata como infraestrutura
