# DE-040: Processamento de Entidades Filhas Uma a Uma

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um professor corrigindo provas. Ele pode corrigir todas as provas de uma vez (em lote), parando na primeira nota zero e ignorando as demais. Ou pode corrigir **uma por uma**, anotando todos os erros de cada prova para que os alunos saibam exatamente onde erraram.

Se o professor parar na primeira prova com erro, os outros alunos não recebem feedback. Corrigindo uma por uma, todos recebem feedback completo sobre seus erros.

### O Problema Técnico

Ao processar múltiplas entidades filhas, há duas abordagens:

```csharp
// ❌ PROBLEMA: Processar em lote, parar no primeiro erro
public bool AddItems(List<ItemInput> inputs)
{
    foreach (var input in inputs)
    {
        var item = Item.Create(input);
        if (item == null)
            return false;  // Para no primeiro erro!

        _items.Add(item);
    }
    return true;
}
// Resultado: Se o item 3 de 10 falhar, só há mensagem sobre o item 3
// Os itens 4-10 nunca foram validados
```

O usuário não sabe se os itens 4-10 são válidos ou não.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos usam validação com curto-circuito:

```csharp
public bool ProcessChildren(List<ChildInput> inputs)
{
    foreach (var input in inputs)
    {
        if (!ValidateChild(input))
            return false; // Para imediatamente

        CreateAndAddChild(input);
    }
    return true;
}
```

Ou lançam exceção no primeiro erro:

```csharp
public void ProcessChildren(List<ChildInput> inputs)
{
    foreach (var input in inputs)
    {
        var child = Child.Create(input)
            ?? throw new ValidationException($"Invalid child: {input}");

        _children.Add(child);
    }
}
```

### Por Que Não Funciona Bem

1. **Feedback incompleto**: Usuário corrige um erro, submete, encontra outro erro, corrige, submete...
2. **UX ruim**: Múltiplas viagens para corrigir múltiplos problemas
3. **Trabalho perdido**: Em operações clone-modify-return, o clone é descartado de qualquer forma - por que não coletar todos os erros?

## A Decisão

### Nossa Abordagem

Entidades filhas DEVEM ser processadas **individualmente** através de método específico, usando `&=` para continuar mesmo após falhas:

```csharp
public static CompositeAggregateRoot? RegisterNew(
    ExecutionContext executionContext,
    RegisterNewInput input
)
{
    return RegisterNewInternal(
        executionContext,
        input,
        entityFactory: (executionContext, input) => new CompositeAggregateRoot(),
        handler: static (executionContext, input, instance) =>
        {
            // ✅ Primeiro, processa campos da Aggregate Root
            bool isValid =
                instance.ChangeNameInternal(executionContext, input.Name)
                & instance.ChangeCodeInternal(executionContext, input.Code);

            // ✅ Depois, processa cada entidade filha uma a uma
            if (input.ChildRegisterNewInputCollection != null)
            {
                foreach (ChildRegisterNewInput childInput in input.ChildRegisterNewInputCollection)
                {
                    // ✅ &= continua iterando mesmo se falhar
                    isValid &= instance.ProcessCompositeChildEntityForRegisterNewInternal(
                        executionContext,
                        childInput
                    );
                }
            }

            return isValid;
        }
    );
}
```

**Padrão de nomenclatura do método:**
```
Process[NomeDaEntidadeFilha]For[NomeDaOperação]Internal
```

Exemplo: `ProcessCompositeChildEntityForRegisterNewInternal`

### Por Que Funciona Melhor

1. **Feedback completo**: Todas as mensagens de validação são coletadas

```csharp
// Input com 3 filhos, 2 inválidos
var result = aggregate.RegisterNew(context, input);

// context.Messages contém:
// - "ChildEntities[0].Title is required"
// - "ChildEntities[2].Title exceeds max length"
// Usuário vê TODOS os problemas de uma vez
```

2. **Estado da Aggregate Root disponível**: Validação pode depender do contexto

```csharp
private bool ProcessChildForRegisterNewInternal(...)
{
    // Pode acessar estado do aggregate para validar
    if (_compositeChildEntityCollection.Count >= MaxChildren)
    {
        executionContext.AddError("Max children reached");
        return false;
    }
    // ...
}
```

3. **Operação em clone**: Se qualquer validação falhar, o clone é descartado

```csharp
// Dentro de RegisterNewInternal:
var clone = entityFactory();
bool isValid = handler(context, input, clone);

if (!isValid)
    return null;  // Clone descartado, original intacto

return clone;
```

4. **Método específico por operação**: Regras podem variar

```csharp
// Para RegisterNew: verifica duplicidade contra lista vazia (clone novo)
private bool ProcessChildForRegisterNewInternal(...) { ... }

// Para Update: verifica duplicidade ignorando o item sendo atualizado
private bool ProcessChildForUpdateInternal(...) { ... }
```

## Consequências

### Benefícios

- **UX melhorada**: Usuário vê todos os erros de uma vez
- **Menos round-trips**: Corrige tudo em uma submissão
- **Validação contextual**: Pode usar estado da Aggregate Root
- **Separação clara**: Cada operação tem seu método de processamento

### Trade-offs (Com Perspectiva)

- **Mais iterações**: Processa todos mesmo se primeiro falhar
  - *Perspectiva*: Em clone-modify-return, o clone é descartado de qualquer forma
  - *Benefício*: Feedback completo vale a iteração extra

- **Mais mensagens acumuladas**: `ExecutionContext` pode ter muitas mensagens
  - *Perspectiva*: É exatamente o que queremos - feedback completo
  - *Nota*: Se houver limite, pode-se adicionar early-exit após N erros

## Fundamentação Teórica

### O Que o DDD Diz

O conceito de **Aggregate Consistency Boundary** implica que toda a operação deve ser validada antes de persistir. Isso naturalmente leva a coletar todas as validações.

Vaughn Vernon em "Implementing Domain-Driven Design":

> "An Aggregate should be modified atomically. If any part of the modification fails, the entire modification should fail."
>
> *Um Agregado deve ser modificado atomicamente. Se qualquer parte da modificação falhar, toda a modificação deve falhar.*

Nossa abordagem vai além: não só falha atomicamente, mas **reporta todos os problemas**.

### O Que o Clean Code Diz

Robert C. Martin enfatiza **funções que fazem uma coisa**. O método `Process*For*Internal` tem uma responsabilidade clara:
1. Criar/validar a entidade filha
2. Validar no contexto da operação
3. Adicionar à coleção se válido

### Outros Fundamentos

**Fail-Fast vs Fail-Complete**:

Há dois paradigmas de validação:
- **Fail-Fast**: Para no primeiro erro (bom para performance, ruim para UX)
- **Fail-Complete**: Coleta todos os erros (melhor UX, custo aceitável)

Para operações de usuário, Fail-Complete é quase sempre preferível.

**User Experience (UX) Research**:

Estudos de UX mostram que formulários que reportam todos os erros de uma vez têm maior taxa de conclusão do que aqueles que reportam um erro por vez.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre fail-fast e fail-complete em validação?"
- "Como implementar validação que coleta todos os erros em diferentes linguagens?"
- "Por que usar &= ao invés de && para validação completa?"
- "Como balancear performance e feedback em validação de listas grandes?"

### Leitura Recomendada

- [Notification Pattern - Martin Fowler](https://martinfowler.com/eaaDev/Notification.html)
- [Validation in Domain-Driven Design](https://enterprisecraftsmanship.com/posts/validation-and-ddd/)
- [UX Guidelines for Error Messages](https://www.nngroup.com/articles/error-message-guidelines/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece RegisterNewInternal que trabalha com handler de processamento |
| [ExecutionContext](../../building-blocks/core/execution-context.md) | Acumula mensagens de todas as validações |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Processamento de Entidades Filhas Uma a Uma com Método Específico
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Método `ProcessCompositeChildEntityForRegisterNewInternal`
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Loop com `&=` no handler de RegisterNew
