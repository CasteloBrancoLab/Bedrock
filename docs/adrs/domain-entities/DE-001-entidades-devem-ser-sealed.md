# DE-001: Entidades Devem Ser Sealed

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você é dono de um cofre ultra-seguro em um banco. O cofre tem uma combinação secreta que só você conhece, e existe um protocolo rigoroso para abri-lo: verificar identidade, inserir código, aguardar timer, etc.

Agora imagine que alguém herda seu cofre e decide "melhorar" o processo: remove a verificação de identidade porque "demora muito", ou troca o timer por um mais curto. Tecnicamente o cofre ainda funciona, mas a segurança foi comprometida sem você saber.

Na programação orientada a objetos, herança permite exatamente isso: uma classe filha pode sobrescrever comportamentos da classe pai, potencialmente quebrando garantias que a classe pai assumia como verdadeiras.

### O Problema Técnico

Entidades de domínio têm **invariantes** - regras que DEVEM ser sempre verdadeiras:
- Estado inválido nunca existe na memória
- Toda modificação passa por validação
- Construtores são privados, forçando uso de factory methods

Se a classe for aberta para herança, subclasses podem:
- Adicionar construtores públicos (bypass de validação)
- Sobrescrever métodos públicos com lógica diferente
- Quebrar a garantia de "estado sempre válido"

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos deixa entidades abertas para herança, seja por esquecimento ou pela crença de que "pode ser útil no futuro":

```csharp
// Abordagem comum - classe aberta
public class Person : EntityBase<Person>
{
    public string FirstName { get; private set; }

    private Person() { }

    public static Person? Create(string firstName)
    {
        if (string.IsNullOrEmpty(firstName))
            return null;
        return new Person { FirstName = firstName };
    }

    public virtual Person? ChangeName(string newName)
    {
        if (string.IsNullOrEmpty(newName))
            return null;
        var clone = this.Clone();
        clone.FirstName = newName;
        return clone;
    }
}
```

### Por Que Não Funciona Bem

Uma subclasse pode quebrar todas as garantias:

```csharp
// Subclasse maliciosa ou descuidada
public class UnsafePerson : Person
{
    // Bypass do factory method - cria estado inválido
    public UnsafePerson()
    {
        // FirstName permanece null/empty!
    }

    // Sobrescreve validação
    public override Person? ChangeName(string newName)
    {
        // Ignora validação completamente
        var clone = this.Clone();
        // Acessa via reflection ou cast para modificar
        return clone;
    }
}

// Agora o sistema tem Person com estado inválido
Person person = new UnsafePerson(); // FirstName é null!
```

Problemas resultantes:
- **NullReferenceException** em código que assume `FirstName` sempre preenchido
- **Dados corrompidos** persistidos no banco
- **Bugs difíceis de rastrear** - o problema está em outra classe
- **Violação de contratos** - APIs que garantem dados válidos retornam dados inválidos

## A Decisão

### Nossa Abordagem

Entidades de domínio DEVEM ser `sealed`:

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    // Propriedades com setters privados
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // Construtores privados - sem bypass possível
    private SimpleAggregateRoot() { }

    private SimpleAggregateRoot(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        // ...
    ) : base(entityInfo)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    // Factory methods são a ÚNICA forma de criar instâncias
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        // Validação obrigatória - não há como contornar
    }
}
```

### Por Que Funciona Melhor

1. **Garantia de compilação**: Ninguém pode herdar, logo ninguém pode sobrescrever
2. **Comportamento previsível**: O código faz exatamente o que está escrito
3. **Invariantes protegidas**: Métodos privados de validação não podem ser contornados
4. **Otimização do compilador**: `sealed` permite inlining e devirtualização

## Consequências

### Benefícios

- **Segurança**: Impossível criar subclasses que quebrem validações
- **Performance**: Compilador otimiza chamadas de métodos (devirtualização)
- **Simplicidade**: Menos caminhos de código para analisar e testar
- **Confiança**: Consumidores da entidade podem confiar nas garantias

### Trade-offs (Com Perspectiva)

- **Sem herança para variações**: Não é possível criar `SpecialPerson extends Person`
- **Mais código em alguns casos**: Variações exigem composição ou interfaces

### Trade-offs Frequentemente Superestimados

**"Herança é necessária para reutilização de código"**

Na prática, herança em entidades de domínio é raramente a melhor escolha:

```csharp
// ❌ O que parece natural...
public class PremiumCustomer : Customer
{
    public decimal DiscountPercentage { get; }
}

// ...mas na prática causa problemas:
// - Customer do banco pode virar Premium? Precisa recriar objeto
// - E se Premium puder virar Regular? Mais recriação
// - Queries no banco ficam complexas (TPH, TPT, TPC)
// - Validações da base podem não fazer sentido para derivada
```

A realidade é que variações de comportamento em domínio geralmente são:
- **Temporais**: Cliente é premium ESTE mês (composição com período)
- **Contextuais**: Mesmo cliente tem desconto diferente por produto (strategy)
- **Configuráveis**: Regras mudam por tenant/região (strategy via IOC)

Herança resolve bem problemas de **taxonomia fixa** (Animal → Mamífero → Cachorro), mas entidades de domínio raramente são taxonomias fixas.

**"Preciso compartilhar código entre entidades similares"**

Composição e métodos de extensão resolvem isso sem os problemas da herança:

```csharp
// Composição - comportamento encapsulado em Value Object
public sealed class Customer
{
    public CustomerTier Tier { get; private set; }  // Value Object com lógica
    public decimal GetDiscount() => Tier.CalculateDiscount();
}

// Extension methods - para operações cross-cutting
public static class AuditableExtensions
{
    public static bool WasModifiedToday<T>(this T entity) where T : IAuditable
        => entity.LastModifiedAt.Date == DateTime.Today;
}
```

### Alternativas Para Variações de Comportamento

Se você precisa de comportamentos diferentes para diferentes contextos:

```csharp
// ✅ Composição - entidades relacionadas dentro do agregado
public sealed class Order
{
    public Customer Customer { get; private set; }  // Composição
    public Address ShippingAddress { get; private set; }
}

// ✅ Strategy Pattern - comportamentos injetados via IOC
public interface IDiscountStrategy
{
    decimal Calculate(Order order);
}

// ✅ Interfaces - polimorfismo controlado
public interface IAuditable
{
    DateTime CreatedAt { get; }
    string CreatedBy { get; }
}

// ❌ Herança - quebra encapsulamento
public class PremiumCustomer : Customer { } // NÃO FAÇA ISSO
```

## Fundamentação Teórica

### Padrões de Design Relacionados

**Composite Pattern (GoF)** - Preferimos composição sobre herança. Variações de comportamento são modeladas como objetos compostos (Value Objects, entidades relacionadas) ao invés de hierarquias de classes.

**Strategy Pattern (GoF)** - Comportamentos que variam (ex: cálculo de desconto por tipo de cliente) são extraídos para interfaces injetadas, não para subclasses.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) enfatiza que **Aggregates devem proteger suas invariantes**:

> "An AGGREGATE is a cluster of associated objects that we treat as a unit for the purpose of data changes. Each AGGREGATE has a root and a boundary. The root is a single, specific ENTITY [...] The root is the only member of the AGGREGATE that outside objects are allowed to hold references to."
>
> *Um AGGREGATE é um grupo de objetos associados que tratamos como uma unidade para fins de alteração de dados. Cada AGGREGATE tem uma raiz e uma fronteira. A raiz é uma única ENTITY específica [...] A raiz é o único membro do AGGREGATE ao qual objetos externos podem manter referências.*

Classes `sealed` são a implementação mais direta dessa proteção: se ninguém pode herdar, ninguém pode contornar as invariantes do Aggregate Root.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) reforça:

> "The Aggregate Root is responsible for ensuring that all invariants are satisfied before and after any operation."
>
> *O Aggregate Root é responsável por garantir que todas as invariantes sejam satisfeitas antes e depois de qualquer operação.*

Herança aberta permite que subclasses quebrem essa responsabilidade.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) não aborda `sealed` diretamente, mas o princípio **"Prefer Composition Over Inheritance"** (Prefira Composição sobre Herança) é central:

> "Inheritance is a very powerful mechanism, but it is also very easy to misuse. [...] The problem with inheritance is that it breaks encapsulation."
>
> *Herança é um mecanismo muito poderoso, mas também é muito fácil de usar incorretamente. [...] O problema com herança é que ela quebra o encapsulamento.*

`sealed` é a forma de dizer: "esta classe não foi projetada para herança, use composição".

### O Que o Clean Architecture Diz

Clean Architecture foca em **dependency rules** e **boundaries**. Entidades estão no centro (Enterprise Business Rules) e não devem depender de nada externo.

`sealed` não é mencionado explicitamente, mas proteger entidades de modificações externas (via herança) está alinhado com o princípio de que o núcleo deve ser estável e protegido.

### Outros Fundamentos

**Effective Java - Item 19** (Joshua Bloch):
> "Design and document for inheritance or else prohibit it."
>
> *Projete e documente para herança ou então proíba-a.*

Bloch argumenta que classes não projetadas explicitamente para herança devem ser `final` (equivalente a `sealed` em C#). O custo de documentar e manter um contrato de herança é alto; proibir herança é a escolha mais segura.

**SOLID - Open/Closed Principle (OCP)**:

Alguém poderia argumentar que `sealed` viola OCP ("aberto para extensão"). Mas OCP se refere a extensão via **abstração** (interfaces, composição), não via herança concreta. A entidade é "fechada" para modificação direta, mas "aberta" para extensão via:
- Interfaces que implementa
- Value Objects que compõe
- Strategies que recebe

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que sealed melhora a performance em .NET?"
- "Como aplicar o princípio de composição sobre herança em entidades de domínio?"
- "Quais são os riscos de permitir herança em classes com invariantes?"
- "Como o compilador otimiza chamadas de métodos em classes sealed?"

### Leitura Recomendada

- [Effective Java - Item 19: Design and document for inheritance or else prohibit it](https://www.oreilly.com/library/view/effective-java/9780134686097/)
- [.NET Performance Tips - Sealed Classes](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6/#sealed)
- [Domain-Driven Design - Aggregates](https://martinfowler.com/bliki/DDD_Aggregate.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que implementa o padrão sealed para entidades de domínio, fornecendo a infraestrutura para factory methods e proteção de invariantes |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - comentário LLM_RULE sobre sealed
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - declaração `public sealed class`
