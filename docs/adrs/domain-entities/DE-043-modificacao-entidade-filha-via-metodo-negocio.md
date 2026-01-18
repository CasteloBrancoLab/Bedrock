# DE-043: Modificação de Entidade Filha Via Método de Negócio Dela

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma empresa (Aggregate Root) com funcionários (entidades filhas). Quando um funcionário muda de cargo, a empresa não modifica diretamente o registro do funcionário. Ela solicita ao RH do funcionário (método de negócio da entidade filha) que faça a alteração, seguindo todos os procedimentos: verificar se o cargo existe, atualizar histórico, etc.

A empresa orquestra a mudança, mas **delega a execução** para quem sabe fazer: o próprio funcionário (via seus métodos de negócio).

### O Problema Técnico

Modificar entidades filhas diretamente quebra o encapsulamento e o padrão clone-modify-return:

```csharp
// ❌ PROBLEMA: Modificar diretamente
private bool ChangeChildTitle(Guid childId, string newTitle)
{
    var child = _children.First(c => c.Id == childId);
    child.Title = newTitle;  // ❌ Modificação direta!
    return true;
}

// Problemas:
// 1. Se Title tiver setter privado, não compila
// 2. Se tiver setter público, entidade filha perdeu encapsulamento
// 3. Validações da entidade filha são ignoradas
// 4. Clone-modify-return da filha não é usado
```

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos modificam propriedades diretamente ou usam setters:

```csharp
public class Order
{
    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        var item = _items.First(i => i.Id == itemId);
        item.Quantity = newQuantity;  // Setter público
    }
}

public class OrderItem
{
    public int Quantity { get; set; }  // ❌ Setter público
}
```

Ou criam métodos "set" na entidade filha que modificam diretamente:

```csharp
public class OrderItem
{
    public void SetQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException();
        Quantity = quantity;  // Modifica this
    }
}
```

### Por Que Não Funciona Bem

1. **Encapsulamento quebrado**: Propriedades com setter público
2. **Sem clone-modify-return**: Entidade filha é modificada in-place
3. **Inconsistência**: Aggregate Root usa clone-modify-return, mas filhas não
4. **Validação parcial**: Regras da filha podem ser ignoradas

## A Decisão

### Nossa Abordagem

A modificação da entidade filha DEVE ser feita chamando o **método de negócio da própria entidade filha**, que segue o padrão clone-modify-return:

```csharp
private bool ProcessCompositeChildEntityForChangeTitleInternal(
    ExecutionContext executionContext,
    Guid compositeChildEntityId,
    string title
)
{
    // 1. Localiza a entidade filha existente
    CompositeChildEntity? existingChild = null;
    int existingChildIndex = -1;
    // ... (localização)

    // 2. ✅ Chama método de negócio da própria entidade filha
    CompositeChildEntity? updatedChild = existingChild.ChangeTitle(
        executionContext,
        new ChildChangeTitleInput(title)
    );

    // 3. Se retornou null, validação da entidade filha falhou
    if (updatedChild is null)
        return false;

    // 4. Validação contextual (no contexto da Aggregate Root)
    bool isValid = ValidateCompositeChildEntityForChangeTitleInternal(
        executionContext,
        updatedChild,
        existingChildIndex
    );

    if (!isValid)
        return false;

    // 5. ✅ Substitui na coleção (nova instância, não modificação)
    _compositeChildEntityCollection[existingChildIndex] = updatedChild;

    return true;
}
```

### Por Que Funciona Melhor

1. **Clone-modify-return consistente**: Entidade filha também retorna nova instância

```csharp
// CompositeChildEntity.ChangeTitle segue o mesmo padrão:
public CompositeChildEntity? ChangeTitle(
    ExecutionContext executionContext,
    ChildChangeTitleInput input
)
{
    return RegisterChangeInternal<CompositeChildEntity, ChildChangeTitleInput>(
        executionContext,
        instance: this,  // Original não modificado
        input,
        handler: (ctx, inp, newInstance) =>
        {
            return newInstance.ChangeTitleInternal(ctx, inp.Title);
        }
    );
    // Retorna nova instância ou null
}
```

2. **Validações da entidade filha são executadas**

```csharp
// ChangeTitle internamente valida:
// - Title não pode ser vazio
// - Title não pode exceder MaxLength
// - etc.
// Se qualquer validação falhar, retorna null
```

3. **Substituição, não mutação**

```csharp
// Antes: coleção contém [child1, child2, child3]
// child2.ChangeTitle retorna updatedChild2 (nova instância)
_compositeChildEntityCollection[1] = updatedChild2;
// Depois: coleção contém [child1, updatedChild2, child3]
// child2 original continua existindo (não foi modificado)
```

4. **Duas camadas de validação**

```csharp
// Camada 1: Validação da entidade filha (via ChangeTitle)
CompositeChildEntity? updatedChild = existingChild.ChangeTitle(...);
if (updatedChild is null) return false;  // Validação da filha falhou

// Camada 2: Validação contextual (no contexto da Aggregate Root)
bool isValid = ValidateCompositeChildEntityForChangeTitleInternal(...);
if (!isValid) return false;  // Validação de duplicidade, etc.
```

## Consequências

### Benefícios

- **Consistência arquitetural**: Mesmo padrão em todos os níveis
- **Validação em camadas**: Filha valida seus campos, Root valida contexto
- **Encapsulamento preservado**: Filha controla seu próprio estado
- **Atomicidade**: Tudo ou nada, em ambos os níveis

### Trade-offs (Com Perspectiva)

- **Mais objetos criados**: Clone da Aggregate Root + clone da Child
  - *Perspectiva*: São objetos leves, overhead negligenciável
  - *Comparação*: Uma query EF Core cria muito mais objetos

- **Fluxo mais longo**: Root delega para Child
  - *Perspectiva*: É separação de responsabilidades, não complexidade
  - *Benefício*: Cada entidade é responsável por suas próprias regras

## Fundamentação Teórica

### O Que o DDD Diz

O conceito de **Entities with Identity** implica que entidades têm comportamento próprio:

> "Entities [...] have behavior. [...] The behavior of an Entity is defined by its methods."
>
> *Entidades [...] têm comportamento. [...] O comportamento de uma Entidade é definido por seus métodos.*

Modificar uma entidade deve ser através de seus próprios métodos, não manipulação externa.

### O Que o Clean Code Diz

**Tell, Don't Ask** (Martin Fowler):

> "Rather than asking an object for data and acting on that data, tell the object what to do."
>
> *Ao invés de pedir dados a um objeto e agir sobre esses dados, diga ao objeto o que fazer.*

A Aggregate Root "diz" para a entidade filha mudar seu título, não "pega" o título e muda.

**Law of Demeter**:

> "Only talk to your immediate friends."
>
> *Só fale com seus amigos imediatos.*

A Aggregate Root interage com a entidade filha através da interface pública dela (métodos de negócio), não acessando seus internals.

### Outros Fundamentos

**Composition over Inheritance**:

A relação entre Aggregate Root e entidades filhas é composição. Cada parte mantém sua própria responsabilidade e encapsulamento.

**Command Pattern** (implícito):

O input (`ChildChangeTitleInput`) funciona como um comando enviado à entidade filha, que decide como executá-lo.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como o princípio Tell, Don't Ask se aplica a agregados DDD?"
- "Qual a diferença entre composição e agregação em OOP?"
- "Como manter consistência transacional com clone-modify-return aninhado?"
- "Quando uma entidade filha deve ter seus próprios métodos de negócio?"

### Leitura Recomendada

- [Tell, Don't Ask - Martin Fowler](https://martinfowler.com/bliki/TellDontAsk.html)
- [Law of Demeter - Wikipedia](https://en.wikipedia.org/wiki/Law_of_Demeter)
- [DDD Entities - Eric Evans](https://www.domainlanguage.com/ddd/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece RegisterChangeInternal para entidades filhas |
| [ExecutionContext](../../building-blocks/core/execution-context.md) | Compartilhado entre Aggregate Root e entidades filhas para coletar todas as mensagens |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Modificação de Entidade Filha Via Método de Negócio Dela
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Chamada `existingChild.ChangeTitle(...)` em ProcessCompositeChildEntityForChangeTitleInternal
- [CompositeChildEntity.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeChildEntity.cs) - Método `ChangeTitle` que segue clone-modify-return
