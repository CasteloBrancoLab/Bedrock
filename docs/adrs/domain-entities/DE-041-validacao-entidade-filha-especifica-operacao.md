# DE-041: Validação de Entidade Filha Específica por Operação

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um clube com regras para admissão de novos sócios e regras diferentes para transferência de sócios existentes. Um novo sócio precisa de indicação de dois membros. Um sócio sendo transferido de outra filial não precisa de indicação (já foi validado antes).

Se aplicarmos as mesmas regras para ambos os casos, ou exigimos indicação desnecessária para transferências, ou permitimos novos sócios sem indicação. Cada **operação** tem suas próprias regras.

### O Problema Técnico

Regras de validação de entidades filhas podem variar conforme a operação:

```csharp
// ❌ PROBLEMA: Validação única para todas as operações
public bool ValidateChild(Child child)
{
    // Verifica duplicidade de título...
    // Mas em "Update", preciso ignorar o próprio item!
    // Em "RegisterNew", verifico contra lista vazia (clone novo)
    // Em "Import", talvez permita duplicatas temporariamente
}
```

Uma validação genérica não atende todos os contextos.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos usam um método `IsValid()` genérico:

```csharp
public class Order
{
    public bool AddItem(OrderItem item)
    {
        if (!item.IsValid())
            return false;

        if (HasDuplicateItem(item))
            return false;

        _items.Add(item);
        return true;
    }

    public bool UpdateItem(OrderItem updatedItem)
    {
        // Problema: HasDuplicateItem vai encontrar o próprio item!
        if (!updatedItem.IsValid())
            return false;

        if (HasDuplicateItem(updatedItem))
            return false; // ❌ Sempre retorna true se título não mudou!

        // ...
    }
}
```

### Por Que Não Funciona Bem

1. **Regras conflitantes**: Mesma validação não serve para operações diferentes
2. **Lógica espalhada**: Condicionais `if (isUpdate)` dentro da validação
3. **Difícil de manter**: Adicionar nova operação requer revisar todas as validações

## A Decisão

### Nossa Abordagem

Toda entidade filha DEVE ter método de validação **específico por operação**:

```csharp
// ✅ Validação específica para RegisterNew
private bool ValidateCompositeChildEntityForRegisterNewInternal(
    ExecutionContext executionContext,
    CompositeChildEntity compositeChildEntity
)
{
    // Verifica duplicidade contra coleção existente
    // (em RegisterNew, o clone começa vazio, então verifica só os já adicionados)
    bool hasDuplicatedTitle = false;
    foreach (var existing in _compositeChildEntityCollection)
    {
        if (existing.Title == compositeChildEntity.Title)
        {
            hasDuplicatedTitle = true;
            break;
        }
    }

    if (hasDuplicatedTitle)
    {
        executionContext.AddErrorMessage(
            code: $"{CreateMessageCode<CompositeAggregateRoot>(...))}.DuplicateTitle"
        );
        return false;
    }

    return compositeChildEntity.IsValid(executionContext);
}

// ✅ Validação específica para ChangeTitle
private bool ValidateCompositeChildEntityForChangeTitleInternal(
    ExecutionContext executionContext,
    CompositeChildEntity compositeChildEntity,
    int currentIndex  // ← Parâmetro extra: índice do item sendo alterado
)
{
    // Verifica duplicidade IGNORANDO o próprio item
    for (int i = 0; i < _compositeChildEntityCollection.Count; i++)
    {
        if (i == currentIndex)
            continue;  // ✅ Ignora o próprio item

        if (_compositeChildEntityCollection[i].Title == compositeChildEntity.Title)
        {
            executionContext.AddErrorMessage(...);
            return false;
        }
    }

    return compositeChildEntity.IsValid(executionContext);
}
```

**Padrão de nomenclatura:**
```
Validate[NomeDaEntidadeFilha]For[NomeDaOperação]Internal
```

### Por Que Funciona Melhor

1. **Regras claras por operação**: Cada método sabe exatamente o que validar

```csharp
// RegisterNew: verifica duplicidade contra todos os itens já adicionados
ValidateChildForRegisterNewInternal(child);

// Update: verifica duplicidade ignorando o item sendo atualizado
ValidateChildForUpdateInternal(child, currentIndex);

// Import: pode ter regras mais relaxadas ou diferentes
ValidateChildForImportInternal(child);
```

2. **Parâmetros específicos**: Cada operação pode receber contexto necessário

```csharp
// ChangeTitle precisa saber qual item está sendo alterado
ValidateCompositeChildEntityForChangeTitleInternal(
    executionContext,
    updatedChild,
    existingChildIndex  // ✅ Contexto específico da operação
);
```

3. **Encapsulamento**: Método privado, regras não vazam para fora

```csharp
// Apenas a Aggregate Root conhece essas regras
private bool ValidateCompositeChildEntityForRegisterNewInternal(...) { ... }
private bool ValidateCompositeChildEntityForChangeTitleInternal(...) { ... }
```

4. **Fácil de estender**: Nova operação = novo método de validação

```csharp
// Adicionar operação "Duplicate" é criar novo método
private bool ValidateCompositeChildEntityForDuplicateInternal(
    ExecutionContext executionContext,
    CompositeChildEntity original,
    CompositeChildEntity duplicate
)
{
    // Regras específicas para duplicação
    // Ex: título deve ser diferente do original
}
```

## Consequências

### Benefícios

- **Separação de responsabilidades**: Cada operação tem suas regras isoladas
- **Flexibilidade**: Parâmetros e lógica específicos por operação
- **Manutenibilidade**: Mudança em uma operação não afeta outras
- **Clareza**: Nome do método documenta o propósito

### Trade-offs (Com Perspectiva)

- **Mais métodos**: Um método de validação por operação
  - *Perspectiva*: É exatamente a separação que queremos - Single Responsibility
  - *Benefício*: Cada método é simples e focado

- **Possível duplicação de código**: Regras comuns podem se repetir
  - *Perspectiva*: Se houver duplicação significativa, extraia para método auxiliar privado
  - *Exemplo*: `HasDuplicateTitle(child, excludeIndex: null)` como helper

## Fundamentação Teórica

### O Que o DDD Diz

O conceito de **Invariants** em DDD implica que cada operação pode ter invariantes diferentes:

> "Invariants are business rules that must always be consistent."
>
> *Invariantes são regras de negócio que devem sempre ser consistentes.*

Diferentes operações podem ter diferentes invariantes a verificar.

### O Que o Clean Code Diz

**Single Responsibility Principle (SRP)**:

> "A class should have only one reason to change."
>
> *Uma classe deve ter apenas uma razão para mudar.*

Aplicado a métodos: cada método de validação tem uma razão para existir (validar uma operação específica).

**Open/Closed Principle (OCP)**:

> "Software entities should be open for extension, but closed for modification."
>
> *Entidades de software devem estar abertas para extensão, mas fechadas para modificação.*

Adicionar nova operação é criar novo método, não modificar existentes.

### Outros Fundamentos

**Strategy Pattern** (implícito):

Cada método de validação é como uma "estratégia" diferente para validar, escolhida conforme a operação.

**Tell, Don't Ask** (Martin Fowler):

Ao invés de perguntar ao objeto seu estado e decidir externamente, a Aggregate Root encapsula a decisão de validação internamente.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como o Strategy Pattern se aplica a validações contextuais?"
- "Qual a diferença entre validação de estado e validação de operação?"
- "Como organizar validações quando há muitas operações diferentes?"
- "Quando extrair lógica comum de validação para métodos auxiliares?"

### Leitura Recomendada

- [Validation in Domain-Driven Design](https://enterprisecraftsmanship.com/posts/validation-and-ddd/)
- [Single Responsibility Principle - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2014/05/08/SingleReponsibilityPrinciple.html)
- [Strategy Pattern - Refactoring Guru](https://refactoring.guru/design-patterns/strategy)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Entidades compostas implementam validações específicas por operação |
| [ExecutionContext](../../building-blocks/core/execution-context.md) | Recebe mensagens de erro das validações |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Validação de Entidade Filha Específica por Operação
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Método `ValidateCompositeChildEntityForRegisterNewInternal`
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Método `ValidateCompositeChildEntityForChangeTitleInternal`
