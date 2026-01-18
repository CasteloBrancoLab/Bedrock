# DE-036: Coleções de Entidades Filhas com Field Privado List<T>

## Status
Aceita


## Contexto

### O Problema (Analogia)

Imagine uma empresa com departamentos e funcionários. O departamento é responsável por gerenciar seus funcionários - contratar, demitir, transferir. Se qualquer pessoa pudesse adicionar ou remover funcionários diretamente da lista do departamento, sem passar pelo RH, haveria caos: funcionários fantasmas, demissões não autorizadas, inconsistências nos registros.

O departamento precisa **controlar o acesso** à sua lista de funcionários, expondo apenas consultas públicas enquanto mantém as operações de modificação sob seu controle.

### O Problema Técnico

Quando uma Aggregate Root possui entidades filhas (composição), a coleção interna deve ser protegida contra modificações externas:

```csharp
// ❌ PROBLEMA: List<T> pública permite modificação direta
public class Order
{
    public List<OrderItem> Items { get; set; } = [];
}

// Código externo pode:
order.Items.Add(item);           // Bypass de regras de negócio
order.Items.Clear();             // Limpar sem validação
order.Items[0] = hackedItem;     // Substituir diretamente
order.Items = new List<...>();   // Substituir a coleção inteira
```

Isso viola o princípio de que a Aggregate Root é a guardiã da consistência do agregado.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos expõe `List<T>` ou `ICollection<T>` diretamente:

```csharp
public class Department
{
    public List<Employee> Employees { get; set; } = [];

    public void AddEmployee(Employee employee)
    {
        // Validação aqui...
        Employees.Add(employee);
    }
}

// Problema: o método AddEmployee pode ser ignorado
department.Employees.Add(employee); // Bypass!
```

### Por Que Não Funciona Bem

1. **Bypass de regras**: Chamadores podem manipular a coleção diretamente
2. **Invariantes violadas**: Regras como "máximo 10 itens" ou "sem duplicatas" podem ser ignoradas
3. **Encapsulamento quebrado**: A Aggregate Root perde controle sobre seu estado interno
4. **Auditoria impossível**: Modificações diretas não passam por métodos de negócio

## A Decisão

### Nossa Abordagem

Coleções de entidades filhas DEVEM ser encapsuladas como **field privado `List<T>`**:

```csharp
public sealed class CompositeAggregateRoot
    : EntityBase<CompositeAggregateRoot>,
    IAggregateRoot
{
    // ✅ Field privado - inacessível externamente
    private readonly List<CompositeChildEntity> _compositeChildEntityCollection = [];

    // ✅ Propriedade pública retorna versão readonly (ver DE-037)
    public IReadOnlyList<CompositeChildEntity> CompositeChildEntities
    {
        get { return _compositeChildEntityCollection.AsReadOnly(); }
    }

    // ✅ Modificações apenas via métodos de negócio
    public CompositeAggregateRoot? AddCompositeChildEntity(
        ExecutionContext executionContext,
        AddChildInput input
    )
    {
        // Validação, regras de negócio, etc.
        // Só então modifica _compositeChildEntityCollection
    }
}
```

### Por Que Funciona Melhor

1. **Encapsulamento real**: Código externo não tem acesso ao field
2. **Modificações controladas**: Toda alteração passa por métodos de negócio
3. **Invariantes garantidas**: Regras de negócio são sempre aplicadas
4. **Auditoria natural**: Cada modificação é rastreável via métodos

## Consequências

### Benefícios

- **Aggregate Root como guardiã**: Mantém controle total sobre entidades filhas
- **Consistência garantida**: Invariantes do agregado são respeitadas
- **API clara**: Consumidores sabem que devem usar métodos de negócio
- **Testabilidade**: Comportamento previsível e verificável

### Trade-offs (Com Perspectiva)

- **Mais código**: Cada operação precisa de método dedicado
  - *Perspectiva*: Esse "overhead" é exatamente a documentação viva das regras de negócio
- **Não é "property-based"**: Não funciona com binding direto de UI
  - *Perspectiva*: Entidades de domínio não devem ser usadas diretamente em UI de qualquer forma (use DTOs/ViewModels)

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) define Aggregate como:

> "A cluster of associated objects that we treat as a unit for the purpose of data changes. External references are restricted to one member of the AGGREGATE, designated as the root."
>
> *Um cluster de objetos associados que tratamos como uma unidade para propósitos de mudanças de dados. Referências externas são restritas a um membro do AGREGADO, designado como a raiz.*

A raiz (Aggregate Root) é responsável por **todas as modificações** no agregado. Expor a coleção diretamente viola esse princípio fundamental.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) reforça:

> "The Root Entity controls access to the internal parts of the Aggregate."
>
> *A Entidade Raiz controla o acesso às partes internas do Agregado.*

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende o **Princípio de Menor Exposição**:

> "A class should expose as little of its internals as possible."
>
> *Uma classe deve expor o mínimo possível de seus internals.*

Expor `List<T>` diretamente é expor detalhes de implementação. O field privado + métodos de negócio é a forma correta de encapsulamento.

### Outros Fundamentos

**Effective Java - Item 50** (Joshua Bloch):

> "Make defensive copies when needed. [...] Do not incorporate mutable objects passed as parameters into internal state."
>
> *Faça cópias defensivas quando necessário. [...] Não incorpore objetos mutáveis passados como parâmetros no estado interno.*

O field privado é a primeira linha de defesa contra modificações externas.

**Law of Demeter (Princípio do Menor Conhecimento)**:

O código externo não deve conhecer a estrutura interna da coleção. Deve interagir apenas através de métodos da Aggregate Root.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como implementar encapsulamento de coleções em Entity Framework Core?"
- "Qual a diferença entre composição e agregação em DDD?"
- "Por que backing fields são importantes para encapsulamento?"
- "Como proteger coleções em um modelo anêmico vs modelo rico?"

### Leitura Recomendada

- [DDD Aggregates - Martin Fowler](https://martinfowler.com/bliki/DDD_Aggregate.html)
- [Encapsulating Collections - Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/encapsulating-collections/)
- [Implementing DDD - Vaughn Vernon](https://www.amazon.com/Implementing-Domain-Driven-Design-Vaughn-Vernon/dp/0321834577)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece a infraestrutura base para entidades que contêm coleções filhas |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_GUIDANCE: Coleções de Entidades Filhas em Aggregate Roots
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Field `_compositeChildEntityCollection`
