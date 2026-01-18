# DE-038: Field de Coleção Sempre Inicializado (Não Nullable)

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma pasta de documentos. Quando você cria uma nova pasta, ela começa **vazia** - não "inexistente". Você não precisa verificar se a pasta existe antes de adicionar documentos. A pasta está sempre lá, pronta para receber arquivos.

Se a pasta pudesse ser "nula" (inexistente), você precisaria verificar sua existência antes de cada operação. Isso adiciona complexidade desnecessária e risco de erros.

### O Problema Técnico

Fields de coleção nullable exigem null checks em todos os métodos que os utilizam:

```csharp
// ❌ PROBLEMA: Field nullable
private List<OrderItem>? _items;

public void AddItem(OrderItem item)
{
    if (_items == null)
        _items = [];

    _items.Add(item);
}

public int GetItemCount()
{
    return _items?.Count ?? 0; // Null check obrigatório
}

public decimal GetTotal()
{
    if (_items == null)
        return 0;

    return _items.Sum(i => i.Price); // Outro null check
}
```

Cada método precisa lidar com a possibilidade de null.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos usam inicialização lazy (preguiçosa):

```csharp
public class Order
{
    private List<OrderItem>? _items;

    public IReadOnlyList<OrderItem> Items
    {
        get
        {
            if (_items == null)
                _items = [];
            return _items.AsReadOnly();
        }
    }

    public void AddItem(OrderItem item)
    {
        if (_items == null)
            _items = [];
        _items.Add(item);
    }
}
```

Ou usam `Lazy<T>`:

```csharp
private readonly Lazy<List<OrderItem>> _items = new(() => []);
```

### Por Que Não Funciona Bem

1. **Código repetitivo**: Null checks espalhados por múltiplos métodos
2. **Fácil de esquecer**: Um método sem null check causa `NullReferenceException`
3. **Complexidade desnecessária**: Lazy loading para algo que quase sempre será usado
4. **Thread-safety**: Lazy manual pode ter race conditions

## A Decisão

### Nossa Abordagem

O field de coleção DEVE ser inicializado como lista vazia `= []`, nunca nullable:

```csharp
public sealed class CompositeAggregateRoot
    : EntityBase<CompositeAggregateRoot>,
    IAggregateRoot
{
    // ✅ Sempre inicializado, nunca null
    private readonly List<CompositeChildEntity> _compositeChildEntityCollection = [];

    // ✅ Métodos não precisam de null checks
    public IReadOnlyList<CompositeChildEntity> CompositeChildEntities
    {
        get { return _compositeChildEntityCollection.AsReadOnly(); }
    }

    // ✅ Pode usar diretamente
    private bool ProcessChild(...)
    {
        _compositeChildEntityCollection.Add(child); // Seguro, nunca null
        return true;
    }
}
```

### Por Que Funciona Melhor

1. **Elimina null checks**: Todo método pode usar o field diretamente

```csharp
// Com field sempre inicializado:
foreach (var child in _compositeChildEntityCollection) // ✅ Seguro
{
    // ...
}

// vs. field nullable:
foreach (var child in _compositeChildEntityCollection ?? []) // ❌ Defensivo
{
    // ...
}
```

2. **Propriedade pública nunca retorna null**: Melhor UX para consumidores

```csharp
// Consumidor não precisa verificar null:
var count = aggregateRoot.CompositeChildEntities.Count; // ✅ Sempre funciona

// vs.
var count = aggregateRoot.CompositeChildEntities?.Count ?? 0; // ❌ Defensivo
```

3. **Código mais limpo e menos propenso a erros**

## Consequências

### Benefícios

- **Simplicidade**: Sem null checks, código mais direto
- **Segurança**: Impossível `NullReferenceException` no field
- **Consistência**: Propriedade pública sempre retorna lista (vazia ou não)
- **Legibilidade**: Menos código defensivo

### Trade-offs (Com Perspectiva)

O único "custo" é a alocação de uma lista vazia. Vamos contextualizar:

**Custo de lista vazia:**
```csharp
private readonly List<T> _items = []; // ~24-40 bytes
```
- Header do objeto: ~16-24 bytes (dependendo de x86/x64)
- Ponteiro interno para array: 8 bytes
- Campos de tamanho: ~8 bytes

**Comparação com operações do dia-a-dia:**

| Operação | Alocação aproximada |
|----------|---------------------|
| Lista vazia `= []` | ~24-40 bytes |
| `.ToList()` em LINQ | Nova lista + cópia de elementos |
| `.Where().Select()` | Iteradores + closures (~100+ bytes) |
| String interpolation | Nova string no heap |
| Boxing de value type | ~16-24 bytes por box |
| Entity Framework query | Centenas de alocações |
| Uma chamada HTTP | Buffers de KB |

**Quando seria relevante:**
- Criar milhões de entidades vazias em loop tight (cenário artificial)
- Entidades que quase nunca têm filhos E são criadas em massa (raro)

**Na prática:**
A micro-otimização de usar nullable não compensa a complexidade adicionada. Se a entidade existe, é razoável que sua coleção de filhos também exista (mesmo que vazia).

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans não prescreve inicialização específica, mas o princípio de **Always Valid Domain Model** se aplica:

> "A MODEL should be valid at all times."
>
> *Um MODELO deve ser válido o tempo todo.*

Uma entidade com coleção null é um estado ambíguo - significa "sem filhos" ou "não carregado"? Inicializar sempre remove essa ambiguidade.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008):

> "Don't return null. [...] If you are tempted to return null from a method, consider throwing an exception or returning a special case object instead."
>
> *Não retorne null. [...] Se você estiver tentado a retornar null de um método, considere lançar uma exceção ou retornar um objeto de caso especial.*

Lista vazia `[]` é o "caso especial" para coleções - representa ausência de elementos, não ausência de coleção.

### Outros Fundamentos

**Null Object Pattern** (GoF):

A lista vazia é uma forma do Null Object Pattern - um objeto que representa "nada" de forma segura, sem precisar de null checks.

**Effective Java - Item 54** (Joshua Bloch):

> "Return empty collections or arrays, not nulls."
>
> *Retorne coleções ou arrays vazios, não nulos.*

Bloch argumenta que null para representar "vazio" é uma fonte comum de bugs e código defensivo desnecessário.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre lista vazia e null semanticamente?"
- "Quando lazy initialization faz sentido para coleções?"
- "Como o Null Object Pattern se aplica a coleções?"
- "Quais são os custos reais de alocação de List<T> em .NET?"

### Leitura Recomendada

- [Effective Java - Item 54](https://www.oreilly.com/library/view/effective-java/9780134686097/)
- [Null Object Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/specialCase.html)
- [Framework Design Guidelines - Null Collections](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Entidades com coleções filhas seguem este padrão de inicialização |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Field Sempre Inicializado (Não Nullable)
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Field `_compositeChildEntityCollection = []`
