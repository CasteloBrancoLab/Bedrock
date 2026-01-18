# DE-039: Defensive Copy de Coleções no Construtor

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você contrata uma empresa de mudança. Você entrega uma lista com seus pertences para eles fazerem o inventário. Se eles simplesmente guardarem **sua lista original**, e você depois adicionar ou remover itens dessa lista, o inventário deles ficará diferente do que foi acordado.

A solução é a empresa fazer uma **cópia da lista** no momento da contratação. Assim, modificações posteriores na sua lista não afetam o inventário oficial deles.

### O Problema Técnico

Quando uma entidade recebe uma coleção via parâmetro, ela não pode simplesmente guardar a referência:

```csharp
// ❌ PROBLEMA: Guardar referência direta
public Order(List<OrderItem> items)
{
    _items = items; // Compartilha referência!
}

// Código externo pode modificar depois:
var items = new List<OrderItem> { item1 };
var order = new Order(items);

items.Add(item2);   // Modifica _items indiretamente!
items.Clear();      // Limpa _items indiretamente!

// Order não tem mais controle sobre seu estado interno
```

A entidade perdeu controle sobre sua coleção porque compartilha referência com código externo.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos simplesmente atribuem o parâmetro ao field:

```csharp
public class Order
{
    private readonly List<OrderItem> _items;

    public Order(List<OrderItem> items)
    {
        _items = items; // ❌ Referência compartilhada
    }
}
```

Ou usam cast forçado:

```csharp
public Order(IEnumerable<OrderItem> items)
{
    _items = (List<OrderItem>)items; // ❌ Cast perigoso + referência compartilhada
}
```

### Por Que Não Funciona Bem

1. **Aliasing**: Código externo mantém referência e pode modificar
2. **Encapsulamento violado**: Entidade não é dona exclusiva de seu estado
3. **Bugs sutis**: Modificações externas causam comportamento inesperado
4. **Cast perigoso**: `InvalidCastException` em runtime se o tipo real for diferente

## A Decisão

### Nossa Abordagem

O construtor DEVE criar uma **cópia defensiva** da coleção recebida:

```csharp
public sealed class CompositeAggregateRoot
    : EntityBase<CompositeAggregateRoot>,
    IAggregateRoot
{
    private readonly List<CompositeChildEntity> _compositeChildEntityCollection = [];

    private CompositeAggregateRoot(
        EntityInfo entityInfo,
        string name,
        string code,
        IEnumerable<CompositeChildEntity> compositeChildEntities  // ✅ IEnumerable para flexibilidade
    ) : base(entityInfo)
    {
        Name = name;
        Code = code;
        // ✅ Cópia defensiva via spread operator
        _compositeChildEntityCollection = [.. compositeChildEntities];
    }
}
```

**Dois aspectos importantes:**

1. **Parâmetro como `IEnumerable<T>`**: Aceita qualquer coleção (List, Array, IReadOnlyList, etc.)
2. **Cópia via `[.. collection]` ou `.ToList()`**: Cria nova instância

### Por Que Funciona Melhor

1. **Entidade é dona exclusiva**: Nenhuma referência externa aponta para `_compositeChildEntityCollection`

```csharp
var items = new List<CompositeChildEntity> { child1 };
var aggregate = new CompositeAggregateRoot(..., items);

items.Add(child2);  // ✅ Não afeta o aggregate!
items.Clear();      // ✅ Não afeta o aggregate!

// aggregate._compositeChildEntityCollection tem apenas child1
```

2. **Flexibilidade no parâmetro**: `IEnumerable<T>` aceita qualquer fonte

```csharp
// Todas essas chamadas funcionam:
new CompositeAggregateRoot(..., new List<Child> { ... });
new CompositeAggregateRoot(..., new Child[] { ... });
new CompositeAggregateRoot(..., existingCollection.AsReadOnly());
new CompositeAggregateRoot(..., query.Where(x => x.IsActive));
```

3. **Sem cast perigoso**: Não depende do tipo real do parâmetro

## Consequências

### Benefícios

- **Isolamento total**: Entidade é dona exclusiva da coleção interna
- **Segurança**: Modificações externas não afetam o estado interno
- **Flexibilidade**: Aceita qualquer `IEnumerable<T>` como fonte
- **Previsibilidade**: Estado da entidade depende apenas de suas próprias operações

### Trade-offs (Com Perspectiva)

**Custo de cópia**: `[.. collection]` ou `.ToList()` cria nova lista e copia referências.

Para contextualizar:
```csharp
// Custo: N referências copiadas + alocação de array interno
_compositeChildEntityCollection = [.. compositeChildEntities];
```

| Tamanho da coleção | Custo aproximado |
|--------------------|------------------|
| 0 elementos | ~24 bytes (lista vazia) |
| 10 elementos | ~24 bytes + ~80 bytes (array) |
| 100 elementos | ~24 bytes + ~800 bytes (array) |

**Comparação com operações típicas:**
- Uma query EF Core simples: milhares de alocações
- Serialização JSON: buffers de KB
- Chamada HTTP: buffers de KB ou MB

**Quando seria relevante:**
- Coleções com milhares de elementos copiadas frequentemente
- Criação em massa de agregados com coleções grandes

**Na prática:**
A cópia defensiva é uma operação O(n) leve (apenas referências são copiadas, não os objetos). O custo é negligenciável comparado ao benefício de isolamento.

## Fundamentação Teórica

### O Que o DDD Diz

O princípio de **Aggregate como unidade de consistência** implica que a Aggregate Root deve ter controle total sobre seu estado interno. Compartilhar referências viola esse princípio.

Vaughn Vernon em "Implementing Domain-Driven Design":

> "An Aggregate should be completely self-contained and self-consistent."
>
> *Um Agregado deve ser completamente autocontido e autoconsistente.*

### O Que o Clean Code Diz

Robert C. Martin enfatiza **minimizar acoplamento**. Referências compartilhadas criam acoplamento implícito entre a entidade e código externo.

### Outros Fundamentos

**Effective Java - Item 50** (Joshua Bloch):

> "Make defensive copies when needed. [...] If a class has mutable components that it receives from or returns to its clients, the class must defensively copy these components."
>
> *Faça cópias defensivas quando necessário. [...] Se uma classe tem componentes mutáveis que recebe ou retorna para seus clientes, a classe deve copiar defensivamente esses componentes.*

Este é exatamente o caso: recebemos uma coleção mutável e devemos copiá-la.

**Aliasing Bug**:

Compartilhar referências de coleções é uma fonte comum de bugs chamados "aliasing bugs" - onde modificações em um lugar afetam outro lugar inesperadamente.

**Immutability Principle**:

Embora as entidades não sejam completamente imutáveis (usamos clone-modify-return), o estado interno deve ser protegido contra modificações externas não controladas.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "O que é aliasing bug e como evitá-lo?"
- "Qual a diferença entre cópia rasa (shallow) e profunda (deep)?"
- "Quando usar IEnumerable vs ICollection vs List como parâmetro?"
- "Como implementar defensive copy para coleções aninhadas?"

### Leitura Recomendada

- [Effective Java - Item 50: Make defensive copies](https://www.oreilly.com/library/view/effective-java/9780134686097/)
- [Aliasing in Programming](https://en.wikipedia.org/wiki/Aliasing_(computing))
- [Defensive Programming](https://en.wikipedia.org/wiki/Defensive_programming)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Construtores de entidades compostas seguem este padrão |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Defensive Copy de Coleções no Construtor
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Construtor com `[.. compositeChildEntities]`
