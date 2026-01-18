# DE-042: Localização de Entidade Filha por Id

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma biblioteca onde você quer alterar a data de devolução de um livro específico. Você não pode simplesmente dizer "altere o terceiro livro emprestado" - precisa informar **qual livro** (pelo código de registro). O bibliotecário primeiro **localiza** o livro pelo código, verifica que existe, e só então faz a alteração.

Se o livro não for encontrado, o bibliotecário informa que não existe empréstimo com aquele código.

### O Problema Técnico

Operações de modificação em entidades filhas precisam primeiro localizar o item na coleção:

```csharp
// ❌ PROBLEMA: Assumir que o item existe
public bool UpdateChildTitle(Guid childId, string newTitle)
{
    var child = _children.First(c => c.Id == childId); // Pode lançar exceção!
    child.Title = newTitle;
    return true;
}

// ❌ PROBLEMA: Silenciosamente ignorar se não encontrar
public bool UpdateChildTitle(Guid childId, string newTitle)
{
    var child = _children.FirstOrDefault(c => c.Id == childId);
    if (child == null)
        return true; // ❌ Retorna sucesso sem fazer nada!

    // ...
}
```

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos lançam exceção ou retornam booleano sem mensagem:

```csharp
public void UpdateItem(Guid itemId, string newValue)
{
    var item = _items.FirstOrDefault(i => i.Id == itemId)
        ?? throw new NotFoundException($"Item {itemId} not found");

    item.Value = newValue;
}
```

Ou usam LINQ sem feedback:

```csharp
public bool UpdateItem(Guid itemId, string newValue)
{
    var item = _items.FirstOrDefault(i => i.Id == itemId);
    if (item == null)
        return false; // ❌ Chamador não sabe o motivo

    item.Value = newValue;
    return true;
}
```

### Por Que Não Funciona Bem

1. **Exceções para fluxo de controle**: `NotFoundException` é controle de fluxo, não exceção
2. **Sem feedback**: Retornar `false` não explica o problema
3. **Inconsistente com padrão**: Outras validações adicionam mensagens ao `ExecutionContext`

## A Decisão

### Nossa Abordagem

Antes de modificar uma entidade filha, DEVE-SE localizá-la na coleção. Se não encontrar, adicionar mensagem de erro e retornar `false`:

```csharp
private bool ProcessCompositeChildEntityForChangeTitleInternal(
    ExecutionContext executionContext,
    Guid compositeChildEntityId,
    string title
)
{
    // ✅ Localização explícita com índice (para substituição posterior)
    CompositeChildEntity? existingChild = null;
    int existingChildIndex = -1;

    for (int i = 0; i < _compositeChildEntityCollection.Count; i++)
    {
        if (_compositeChildEntityCollection[i].EntityInfo.Id.Value == compositeChildEntityId)
        {
            existingChild = _compositeChildEntityCollection[i];
            existingChildIndex = i;
            break;
        }
    }

    // ✅ Feedback claro se não encontrar
    if (existingChild is null)
    {
        executionContext.AddErrorMessage(
            code: $"{CreateMessageCode<CompositeAggregateRoot>(
                propertyName: CompositeAggregateRootMetadata.CompositeChildEntitiesPropertyName
            )}.NotFound"
        );

        return false;
    }

    // Continua com a modificação...
}
```

### Por Que Funciona Melhor

1. **Feedback consistente**: Usa o mesmo padrão de `ExecutionContext.AddErrorMessage`

```csharp
var result = aggregate.ChangeCompositeChildEntityTitle(context, input);

if (result == null)
{
    // context.Messages contém:
    // "CompositeAggregateRoot.CompositeChildEntities.NotFound"
    // Chamador sabe exatamente o problema
}
```

2. **Índice preservado**: Guarda a posição para substituição posterior

```csharp
// Depois da modificação:
_compositeChildEntityCollection[existingChildIndex] = updatedChild;
// Substitui no mesmo índice, mantendo ordem
```

3. **Sem exceções para fluxo normal**: Item não encontrado é situação esperada, não excepcional

```csharp
// Não lançamos exceção:
if (existingChild is null)
{
    // Adiciona mensagem, retorna false
    // Fluxo normal, não excepcional
}
```

4. **Loop explícito vs LINQ**: Mais verboso, mas captura índice diretamente

```csharp
// Com LINQ precisaria de duas operações:
var existingChild = _collection.FirstOrDefault(c => c.Id == id);
var index = _collection.IndexOf(existingChild); // Segunda iteração!

// Com loop: uma iteração só
for (int i = 0; i < _collection.Count; i++)
{
    if (_collection[i].Id == id)
    {
        existingChild = _collection[i];
        existingChildIndex = i;
        break;
    }
}
```

## Consequências

### Benefícios

- **Feedback claro**: Mensagem específica no `ExecutionContext`
- **Consistência**: Mesmo padrão de tratamento de erros das validações
- **Performance**: Uma única iteração para encontrar item e índice
- **Sem exceções**: Fluxo de controle via retorno, não exceções

### Trade-offs (Com Perspectiva)

- **Código mais verboso**: Loop manual vs LINQ
  - *Perspectiva*: Clareza e performance compensam verbosidade
  - *Nota*: Se coleção for grande e operação frequente, considere Dictionary interno

- **Message code precisa existir**: Sistema de i18n deve ter tradução para ".NotFound"
  - *Perspectiva*: É uma mensagem padrão, reutilizável em todas as entidades

## Fundamentação Teórica

### O Que o DDD Diz

O conceito de **Aggregate Boundary** implica que operações devem ser validadas dentro do agregado:

> "The root Entity [...] is responsible for checking invariants."
>
> *A Entidade raiz [...] é responsável por verificar invariantes.*

Verificar se o item existe é uma invariante: não podemos modificar algo que não existe.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008):

> "Don't return null. [...] If you are tempted to return null from a method, consider [...] returning a special case object instead."
>
> *Não retorne null. [...] Se você estiver tentado a retornar null de um método, considere [...] retornar um objeto de caso especial.*

Nosso "caso especial" é adicionar mensagem de erro e retornar `false` - comunicação clara do problema.

Sobre exceções:

> "Use Exceptions for Exceptional Conditions"
>
> *Use Exceções para Condições Excepcionais*

Item não encontrado em uma operação de edição é **esperado** (usuário pode ter passado Id errado), não excepcional.

### Outros Fundamentos

**Fail-Fast Principle**:

Detectamos o problema (item não existe) imediatamente, antes de tentar qualquer modificação.

**Defensive Programming**:

Não assumimos que o item existe - verificamos explicitamente.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Quando usar exceções vs retorno de erro em C#?"
- "Como otimizar busca em coleções para operações frequentes?"
- "Qual a diferença entre FirstOrDefault e Find em List<T>?"
- "Como implementar lookup por Id eficientemente?"

### Leitura Recomendada

- [Clean Code - Error Handling](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)
- [Exceptions vs Return Codes](https://enterprisecraftsmanship.com/posts/exceptions-for-flow-control/)
- [Defensive Programming](https://en.wikipedia.org/wiki/Defensive_programming)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Entidades filhas são localizadas pelo Id gerenciado em EntityInfo |
| [ExecutionContext](../../building-blocks/core/execution-context.md) | Recebe mensagem de erro quando item não é encontrado |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Localização de Entidade Filha por Id
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Método `ProcessCompositeChildEntityForChangeTitleInternal` (bloco de localização)
