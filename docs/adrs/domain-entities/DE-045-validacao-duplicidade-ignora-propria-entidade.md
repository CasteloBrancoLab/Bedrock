# DE-045: Validação de Duplicidade Ignora a Própria Entidade

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você está editando seu perfil em uma rede social. Você quer alterar seu username de "joao123" para "joao_silva". O sistema verifica se "joao_silva" já existe. Mas e se você decidir manter "joao123"? O sistema não deve dizer "username já existe" - afinal, **é o seu próprio username**!

A verificação de duplicidade durante **alteração** deve ignorar o registro que está sendo alterado.

### O Problema Técnico

Ao alterar uma entidade filha, a verificação de duplicidade pode encontrar a própria entidade:

```csharp
// ❌ PROBLEMA: Verificação ingênua
private bool ValidateChildForChangeTitle(Child updatedChild)
{
    foreach (var existing in _children)
    {
        if (existing.Title == updatedChild.Title)
            return false;  // ❌ Falso positivo se título não mudou!
    }
    return true;
}

// Cenário:
// - child1.Title = "A", child2.Title = "B"
// - Usuário altera child1: Title de "A" para "A" (não mudou)
// - Validação encontra child1 na coleção
// - Retorna false incorretamente!
```

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos verificam se o valor mudou antes de validar:

```csharp
public bool UpdateTitle(Guid childId, string newTitle)
{
    var child = _children.First(c => c.Id == childId);

    // Só valida se mudou
    if (child.Title != newTitle)
    {
        if (_children.Any(c => c.Title == newTitle))
            return false;
    }

    child.Title = newTitle;
    return true;
}
```

Ou usam Id para excluir:

```csharp
if (_children.Any(c => c.Id != childId && c.Title == newTitle))
    return false;
```

### Por Que Não Funciona Bem

1. **Lógica espalhada**: Verificação de "mudou?" misturada com validação
2. **Dois critérios diferentes**: Às vezes usa índice, às vezes Id
3. **Inconsistente com RegisterNew**: Lá não precisa excluir ninguém

## A Decisão

### Nossa Abordagem

Durante operações de **alteração**, a validação de duplicidade DEVE ignorar a própria entidade sendo alterada, **usando o índice** para identificá-la:

```csharp
private bool ValidateCompositeChildEntityForChangeTitleInternal(
    ExecutionContext executionContext,
    CompositeChildEntity compositeChildEntity,
    int currentIndex  // ✅ Índice do item sendo alterado
)
{
    bool hasDuplicatedTitle = false;

    for (int i = 0; i < _compositeChildEntityCollection.Count; i++)
    {
        // ✅ Ignora a própria entidade pelo índice
        if (i == currentIndex)
            continue;

        if (_compositeChildEntityCollection[i].Title == compositeChildEntity.Title)
        {
            hasDuplicatedTitle = true;
            break;
        }
    }

    if (hasDuplicatedTitle)
    {
        executionContext.AddErrorMessage(
            code: $"{CreateMessageCode<CompositeAggregateRoot>(...)}.DuplicateTitle"
        );
        return false;
    }

    return compositeChildEntity.IsValid(executionContext);
}
```

**Contraste com RegisterNew** (que não precisa excluir ninguém):

```csharp
private bool ValidateCompositeChildEntityForRegisterNewInternal(
    ExecutionContext executionContext,
    CompositeChildEntity compositeChildEntity
    // ❌ Sem parâmetro de índice - é item novo!
)
{
    // Verifica contra TODOS os existentes
    foreach (var existing in _compositeChildEntityCollection)
    {
        if (existing.Title == compositeChildEntity.Title)
        {
            // ... retorna erro
        }
    }
    // ...
}
```

### Por Que Funciona Melhor

1. **Semântica correta**: Editar para o mesmo valor não é duplicidade

```csharp
// Cenário: child1.Title = "Original"
// Usuário "altera" para "Original" (sem mudança real)

// Com nossa abordagem:
// - currentIndex = 0 (posição do child1)
// - Loop pula índice 0
// - Não encontra duplicata
// - Operação sucede ✅

// Sem ignorar própria entidade:
// - Loop encontra child1 com Title = "Original"
// - Retorna erro de duplicidade
// - Operação falha ❌ (falso positivo)
```

2. **Por que usar índice ao invés de Id?**

```csharp
// Opção A: Usar índice (nossa escolha)
if (i == currentIndex) continue;

// Opção B: Usar Id
if (existing.Id == updatedChild.Id) continue;

// Por que índice é melhor neste contexto:
// 1. Já temos o índice do loop de localização (DE-042)
// 2. Não precisa comparar Guids em cada iteração
// 3. Mais eficiente: comparação de int vs Guid

// Quando Id seria necessário:
// Se a coleção pudesse ser reordenada entre localização e validação
// (não acontece no nosso padrão clone-modify-return)
```

3. **Método de validação específico por operação** (ver DE-041)

```csharp
// RegisterNew: sem parâmetro de índice
ValidateCompositeChildEntityForRegisterNewInternal(context, child);

// ChangeTitle: com parâmetro de índice
ValidateCompositeChildEntityForChangeTitleInternal(context, child, index);

// Cada operação tem sua assinatura apropriada
```

## Consequências

### Benefícios

- **Sem falsos positivos**: Alterar para mesmo valor funciona corretamente
- **Semântica clara**: Duplicidade é em relação aos **outros**, não a si mesmo
- **Performance**: Comparação de int, não de Guid
- **Consistência**: Padrão claro para operações de alteração

### Trade-offs (Com Perspectiva)

- **Parâmetro extra**: Métodos de validação para alteração precisam do índice
  - *Perspectiva*: É informação necessária, não overhead
  - *Benefício*: Torna explícito que é operação de alteração

- **Dois métodos de validação**: Um para RegisterNew, outro para Change*
  - *Perspectiva*: É exatamente a separação que DE-041 prescreve
  - *Benefício*: Cada operação tem regras apropriadas

## Fundamentação Teórica

### O Que o DDD Diz

O conceito de **Invariants** deve considerar o contexto:

> "Invariants must hold true for the lifetime of the Aggregate."
>
> *Invariantes devem ser verdadeiras durante a vida do Agregado.*

A invariante "sem títulos duplicados" deve ser interpretada corretamente: não pode haver dois **diferentes** itens com mesmo título. Um item consigo mesmo não é duplicata.

### O Que o Clean Code Diz

**Principle of Least Surprise**:

Usuários esperam poder "salvar" sem alterar nada. Receber erro de "título duplicado" quando não mudou o título é surpreendente.

**Correct by Construction**:

O código deve ser correto por design, não precisar de workarounds. Ignorar o próprio item é a solução correta.

### Outros Fundamentos

**Identity vs Equality**:

Em DDD, entidades têm identidade. Comparar uma entidade consigo mesma pela identidade (posição/Id) revela que não é duplicata.

**Set Theory**:

Um conjunto não pode conter o mesmo elemento duas vezes. Um elemento não é duplicata de si mesmo.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como bancos de dados tratam constraints unique em updates?"
- "Qual a diferença entre identity e equality em OOP?"
- "Como implementar validação de unicidade que ignora o registro atual?"
- "Por que comparação por índice é mais eficiente que por Id?"

### Leitura Recomendada

- [DDD Identity - Martin Fowler](https://martinfowler.com/bliki/EvansClassification.html)
- [Unique Constraints in Updates - SQL Best Practices](https://www.sqlshack.com/)
- [Value Object vs Entity - Eric Evans](https://www.domainlanguage.com/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Entidades usam EntityInfo.Id para identidade, mas índice para exclusão em validação |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Validação de Duplicidade Deve Ignorar a Própria Entidade
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Método `ValidateCompositeChildEntityForChangeTitleInternal` com parâmetro `currentIndex`
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Comparação `if (i == currentIndex) continue;`
