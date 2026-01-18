# DE-037: Propriedade Pública Retorna IReadOnlyList via AsReadOnly

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um museu com sua coleção de obras de arte. O museu permite que visitantes **vejam** as obras, mas não podem tocá-las, movê-las ou levá-las embora. O museu expõe a coleção através de uma "janela de visualização" - você pode olhar, mas não manipular.

Se o museu simplesmente entregasse a lista de obras para os visitantes, eles poderiam adicionar obras falsas, remover originais ou reorganizar tudo. A "janela de visualização" (readonly) protege a coleção enquanto permite consultas.

### O Problema Técnico

Mesmo com field privado (DE-036), a forma de exposição pública importa:

```csharp
// ❌ PROBLEMA 1: Retornar o field diretamente
public List<OrderItem> Items => _items;
// Código externo pode fazer cast e modificar:
((List<OrderItem>)order.Items).Add(item);

// ❌ PROBLEMA 2: Retornar IEnumerable<T>
public IEnumerable<OrderItem> Items => _items;
// Não garante readonly, permite múltiplas enumerações com resultados diferentes
// Não expõe .Count, indexador, etc.

// ❌ PROBLEMA 3: Retornar IList<T> ou ICollection<T>
public IList<OrderItem> Items => _items;
// Interface permite Add, Remove, Clear!
```

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos retornam `IEnumerable<T>` pensando que é "readonly":

```csharp
public class Order
{
    private readonly List<OrderItem> _items = [];

    public IEnumerable<OrderItem> Items => _items;
}

// Problema: IEnumerable não garante muito
var items = order.Items;
var count1 = items.Count(); // Enumera
var count2 = items.Count(); // Enumera de novo

// Se alguém modificar _items entre as duas chamadas, resultados diferentes!
```

Ou usam `.ToList()`:

```csharp
public IReadOnlyList<OrderItem> Items => _items.ToList();
// Funciona, mas cria nova lista a cada acesso!
```

### Por Que Não Funciona Bem

1. **IEnumerable não é readonly**: É apenas uma abstração de iteração
2. **ToList() é ineficiente**: Cria nova lista a cada acesso à propriedade
3. **Cast é possível**: `(List<T>)` em runtime pode funcionar dependendo do tipo real
4. **API pobre**: IEnumerable não tem .Count, indexador, etc.

## A Decisão

### Nossa Abordagem

A propriedade pública DEVE retornar `IReadOnlyList<T>` usando `.AsReadOnly()`:

```csharp
public sealed class CompositeAggregateRoot
    : EntityBase<CompositeAggregateRoot>,
    IAggregateRoot
{
    private readonly List<CompositeChildEntity> _compositeChildEntityCollection = [];

    // ✅ IReadOnlyList via AsReadOnly()
    public IReadOnlyList<CompositeChildEntity> CompositeChildEntities
    {
        get
        {
            return _compositeChildEntityCollection.AsReadOnly();
        }
    }
}
```

### Por Que Funciona Melhor

1. **Verdadeiramente readonly**: `AsReadOnly()` retorna `ReadOnlyCollection<T>` que não permite modificações:

```csharp
var items = aggregateRoot.CompositeChildEntities;
items.Add(...);    // ❌ Erro de compilação - método não existe
items[0] = ...;    // ❌ Erro de compilação - indexador readonly
items.Clear();     // ❌ Erro de compilação - método não existe
```

2. **Sem alocação extra**: `AsReadOnly()` retorna um wrapper leve sobre a lista original:

```csharp
// AsReadOnly() internamente:
public ReadOnlyCollection<T> AsReadOnly()
{
    return new ReadOnlyCollection<T>(this); // Wrapper, não cópia
}
```

3. **API rica**: `IReadOnlyList<T>` expõe:
   - `.Count` (sem enumerar)
   - Indexador `[i]` (acesso direto)
   - `IEnumerable<T>` (iteração)

4. **Cast-safe**: Tentar `(List<T>)` causa `InvalidCastException`:

```csharp
var items = aggregateRoot.CompositeChildEntities;
var list = (List<CompositeChildEntity>)items; // ❌ InvalidCastException!
```

## Consequências

### Benefícios

- **Proteção real**: Impossível modificar via propriedade pública
- **Performance**: Wrapper leve, sem cópia de elementos
- **API completa**: Count, indexador, LINQ, tudo disponível
- **Type-safe**: Compilador impede operações de modificação

### Trade-offs (Com Perspectiva)

- **Wrapper criado a cada acesso**: `AsReadOnly()` cria novo `ReadOnlyCollection<T>`
  - *Perspectiva*: É apenas um objeto wrapper (~24 bytes), não copia os elementos
  - *Comparação*: Um único `.ToList()` aloca muito mais (nova lista + cópia de todos os elementos)
  - *Nota*: Se performance for crítica, pode cachear o wrapper em um field

```csharp
// Otimização opcional (raramente necessária):
private ReadOnlyCollection<CompositeChildEntity>? _compositeChildEntitiesReadOnly;

public IReadOnlyList<CompositeChildEntity> CompositeChildEntities
{
    get
    {
        return _compositeChildEntitiesReadOnly ??= _compositeChildEntityCollection.AsReadOnly();
    }
}
// Cuidado: wrapper cacheado não reflete modificações subsequentes à lista
```

## Fundamentação Teórica

### O Que o DDD Diz

Vaughn Vernon em "Implementing Domain-Driven Design" (2013):

> "Exposing the internal collection directly allows clients to modify the Aggregate's state, which can lead to invariant violations."
>
> *Expor a coleção interna diretamente permite que clientes modifiquem o estado do Agregado, o que pode levar a violações de invariantes.*

`IReadOnlyList` + `AsReadOnly()` é a implementação idiomática em C# dessa proteção.

### O Que o Clean Code Diz

O princípio de **Information Hiding** (David Parnas, 1972, citado em Clean Code):

> "Hide implementation details. [...] Expose behavior, not data."
>
> *Esconda detalhes de implementação. [...] Exponha comportamento, não dados.*

A propriedade expõe uma **visão readonly** dos dados, não os dados em si.

### Outros Fundamentos

**Effective C# - Item 26** (Bill Wagner):

> "Use IReadOnlyList<T> to expose collections that should not be modified by callers."
>
> *Use IReadOnlyList<T> para expor coleções que não devem ser modificadas por chamadores.*

**Framework Design Guidelines** (Microsoft):

> "DO use ReadOnlyCollection<T> [...] when you want to protect a collection from modifications."
>
> *USE ReadOnlyCollection<T> [...] quando quiser proteger uma coleção de modificações.*

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre IEnumerable, ICollection, IList e IReadOnlyList em C#?"
- "Como AsReadOnly() funciona internamente?"
- "Quando usar ImmutableList<T> vs IReadOnlyList<T>?"
- "Por que não usar .ToList() para retornar cópia readonly?"

### Leitura Recomendada

- [ReadOnlyCollection<T> - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.readonlycollection-1)
- [Encapsulating Collections - Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/encapsulating-collections/)
- [Framework Design Guidelines - Collection Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/guidelines-for-collections)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Entidades que contêm coleções filhas seguem este padrão de exposição |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Propriedade Pública Retorna IReadOnlyList<T> via AsReadOnly()
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Propriedade `CompositeChildEntities`
