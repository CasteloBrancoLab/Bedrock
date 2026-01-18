# DE-034: Antipadrão: Mutabilidade Direta

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma **cirurgia com dois protocolos diferentes**:

**Protocolo "modificação direta" (mutabilidade direta)**:
- Cirurgião começa a operar o paciente original
- Remove apêndice ✓
- Repara hérnia ✓
- Tenta remover cálculo renal... complicação!
- Paciente já está parcialmente operado - não dá para "desfazer"
- Estado inconsistente: algumas coisas feitas, outras não

**Protocolo "clone-modify-return" (nossa abordagem)**:
- Cirurgião planeja toda a cirurgia em simulador
- Simula remoção de apêndice ✓
- Simula reparo de hérnia ✓
- Simula remoção de cálculo... detecta complicação!
- Simulação descartada - paciente real intocado
- Nova estratégia planejada antes de tocar no paciente

O padrão Clone-Modify-Return funciona como o simulador cirúrgico - você testa todas as mudanças em uma cópia antes de "commit". Se algo falhar, o original permanece intacto.

---

### O Problema Técnico

Modificar `this` diretamente pode deixar a entidade em estado inconsistente se uma operação falhar no meio:

```csharp
// ❌ ANTIPADRÃO: Modificação direta de this
public sealed class Person : EntityBase<Person>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public string FullName => $"{FirstName} {LastName}";

    public void Anonymize(ExecutionContext ctx)
    {
        // Modifica this diretamente
        FirstName = "Anonymous";    // ✅ Modificado
        LastName = "User";          // ✅ Modificado

        // Agora uma validação falha...
        if (!SetBirthDate(ctx, DateOnly.FromDateTime(DateTime.MinValue)))
        {
            // ❌ PROBLEMA: FirstName e LastName JÁ foram alterados!
            // Entidade está em estado INCONSISTENTE:
            // - Nome: "Anonymous User" (novo)
            // - BirthDate: valor original (não alterado)
            throw new InvalidOperationException("Failed to anonymize");
        }
    }
}

// Uso:
var person = Person.RegisterNew(ctx, new RegisterNewInput("John", "Doe", birthDate));
// person.FullName = "John Doe"

try
{
    person.Anonymize(ctx);  // Falha no meio
}
catch
{
    // person.FullName = "Anonymous User" - MAS BirthDate é o original!
    // Estado INCONSISTENTE - parcialmente anonimizado
}
```

**Problemas graves**:

1. **Estado inconsistente**: Entidade fica "meio modificada"
2. **Difícil recuperar**: Não há como "desfazer" as mudanças parciais
3. **Bugs sutis**: Código que assume consistência vai quebrar
4. **Auditoria comprometida**: Qual era o estado real?

---

### Como Normalmente é Feito

```csharp
// ⚠️ COMUM: Métodos void que modificam this
public class Order
{
    public List<OrderItem> Items { get; } = new();
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }

    public void AddItem(OrderItem item)
    {
        Items.Add(item);           // Modificou
        Total += item.Price;       // Modificou
        // E se a próxima linha falhar?
        UpdateStatus();            // Pode falhar!
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;  // Modificou
        RefundPayment();                 // Pode falhar! Status já mudou.
    }
}
```

**Por que desenvolvedores fazem isso**:
- Parece mais simples (menos código)
- Padrão comum em ORMs (Entity Framework, etc.)
- Hábito de programação imperativa

## A Decisão

### Nossa Abordagem: Clone-Modify-Return

Todas as modificações seguem o padrão:

1. **Clone** a instância atual
2. **Modify** o clone (validando cada passo)
3. **Return** o clone se sucesso, ou null se falhou

```csharp
// ✅ CORRETO: Clone-Modify-Return via RegisterChangeInternal
public sealed class Person : EntityBase<Person>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }

    // Método público retorna NOVA instância (ou null)
    public Person? Anonymize(ExecutionContext executionContext)
    {
        return RegisterChangeInternal(
            executionContext,
            instance: this,           // Original (não será modificado)
            input: Unit.Value,        // Sem input adicional
            handler: (ctx, _, clone) =>
            {
                // Handler recebe CLONE, não this
                // Modifica o clone, não o original

                return
                    clone.SetFirstName(ctx, "Anonymous")
                    & clone.SetLastName(ctx, "User")
                    & clone.SetBirthDate(ctx, DateOnly.MinValue);

                // Se qualquer Set* falhar:
                // - clone é descartado
                // - this permanece INTACTO
                // - Retorna null
            }
        );
    }

    private bool SetFirstName(ExecutionContext ctx, string? firstName)
    {
        if (!ValidateFirstName(ctx, firstName))
            return false;

        FirstName = firstName!;
        return true;
    }

    // ... outros métodos Set*
}
```

### Fluxo do Clone-Modify-Return

```
+-------------------------------------------------------------------------+
│                         ESTADO INICIAL                                   │
│                                                                          │
│  person (original)                                                       │
│  ├── FirstName: "John"                                                   │
│  ├── LastName: "Doe"                                                     │
│  └── BirthDate: 1990-05-20                                               │
+-------------------------------------------------------------------------+
                                    │
                                    ▼
+-------------------------------------------------------------------------+
│                     1. CLONE (RegisterChangeInternal)                    │
│                                                                          │
│  clone = person.Clone()                                                  │
│  ├── FirstName: "John"       (cópia)                                     │
│  ├── LastName: "Doe"         (cópia)                                     │
│  └── BirthDate: 1990-05-20   (cópia)                                     │
│                                                                          │
│  person (original) permanece INTOCADO                                    │
+-------------------------------------------------------------------------+
                                    │
                                    ▼
+-------------------------------------------------------------------------+
│                     2. MODIFY (handler executa)                          │
│                                                                          │
│  clone.SetFirstName(ctx, "Anonymous")  → true ✓                          │
│  clone.SetLastName(ctx, "User")        → true ✓                          │
│  clone.SetBirthDate(ctx, MinValue)     → false ✗ (validação falhou)      │
│                                                                          │
│  clone (modificado parcialmente - será descartado)                       │
│  ├── FirstName: "Anonymous"                                              │
│  ├── LastName: "User"                                                    │
│  └── BirthDate: 1990-05-20   (não alterado)                              │
+-------------------------------------------------------------------------+
                                    │
                                    ▼
+-------------------------------------------------------------------------+
│                     3. RETURN (resultado)                                │
│                                                                          │
│  handler retornou false → clone DESCARTADO                               │
│  RegisterChangeInternal retorna NULL                                     │
│                                                                          │
│  person (original) permanece INTACTO:                                    │
│  ├── FirstName: "John"                                                   │
│  ├── LastName: "Doe"                                                     │
│  └── BirthDate: 1990-05-20                                               │
│                                                                          │
│  ✅ Estado CONSISTENTE preservado!                                       │
+-------------------------------------------------------------------------+
```

### Caso de Sucesso

```csharp
var ctx = ExecutionContext.Create(...);

var person = Person.RegisterNew(ctx, new RegisterNewInput("John", "Doe", birthDate));
// person.FullName = "John Doe"

// Chama método que retorna NOVA instância
var anonymized = person.Anonymize(ctx);

if (anonymized is not null)
{
    // Sucesso!
    // person ainda é "John Doe" (original intacto)
    // anonymized é "Anonymous User" (nova instância)

    Console.WriteLine(person.FullName);      // "John Doe"
    Console.WriteLine(anonymized.FullName);  // "Anonymous User"
}
```

### Caso de Falha

```csharp
var ctx = ExecutionContext.Create(...);

var person = Person.RegisterNew(ctx, new RegisterNewInput("John", "Doe", birthDate));

// Tenta modificar com dados inválidos
var result = person.ChangeName(ctx, new ChangeNameInput("", ""));  // Nomes vazios

if (result is null)
{
    // Falhou - MAS person está INTACTO!
    Console.WriteLine(person.FullName);  // Ainda "John Doe"

    // Erros estão no contexto
    foreach (var error in ctx.Messages.Where(m => m.Type == MessageType.Error))
    {
        Console.WriteLine(error.Text);
    }
    // "First name is required"
    // "Last name is required"
}
```

### Por Que RegisterChangeInternal?

O método `RegisterChangeInternal` na classe base faz mais do que clone:

```csharp
protected TEntityBase? RegisterChangeInternal<TInput>(
    ExecutionContext executionContext,
    TEntityBase instance,
    TInput input,
    Func<ExecutionContext, TInput, TEntityBase, bool> handler
)
{
    // 1. Clona a instância
    var clone = (TEntityBase)instance.Clone();

    // 2. Atualiza EntityInfo (auditoria automática)
    clone.SetEntityInfo(
        executionContext,
        clone.EntityInfo.RegisterChange(
            executionContext,
            changedBy: executionContext.ExecutionUser
        )
    );
    // EntityInfo agora tem:
    // - LastChangedAt = now
    // - LastChangedBy = current user
    // - EntityVersion = nova versão (optimistic locking)

    // 3. Executa handler (modificações de negócio)
    var handlerResult = handler(executionContext, input, clone);

    // 4. Retorna clone se sucesso, null se falhou
    if (!handlerResult)
        return null;

    return clone;
}
```

**Benefícios automáticos**:
- Auditoria (LastChangedAt/By) sempre atualizada
- Versão sempre incrementada (optimistic locking)
- Clone garantido (impossível esquecer)
- Pattern consistente em toda a aplicação

### Comparação

| Aspecto | Mutabilidade Direta | Clone-Modify-Return |
|---------|---------------------|---------------------|
| **Estado após falha** | Inconsistente | Original intacto |
| **Auditoria** | Manual (fácil esquecer) | Automática |
| **Versioning** | Manual | Automático |
| **Debugging** | Difícil (estado mutou) | Fácil (comparar antes/depois) |
| **Testes** | Complexos (side effects) | Simples (entrada → saída) |
| **Thread-safety** | Problemático | Seguro (imutável externamente) |

### Trade-offs (Com Perspectiva)

- **Uma alocação extra**: Clone cria nova instância
  - **Mitigação**: Uma entidade tem ~100-500 bytes. Uma query HTTP transfere megabytes. A alocação é negligenciável.

- **Mais código**: Precisa usar RegisterChangeInternal
  - **Mitigação**: É exatamente a mesma quantidade de código - só muda onde fica. E você ganha auditoria automática.

### Trade-offs Frequentemente Superestimados

**"Criar clone é lento"**

```csharp
// Clone de objeto com 10 propriedades: ~50 nanosegundos
// Query ao banco de dados: ~1.000.000 nanosegundos

// Clone é 20.000x mais rápido que uma query
// Você não vai perceber a diferença
```

**"É mais código"**

Compare:

```csharp
// Mutabilidade direta
public void ChangeName(string firstName, string lastName)
{
    FirstName = firstName;
    LastName = lastName;
    LastChangedAt = DateTime.UtcNow;     // Fácil esquecer!
    LastChangedBy = GetCurrentUser();    // De onde vem?
    Version++;                           // Fácil esquecer!
}

// Clone-Modify-Return
public Person? ChangeName(ExecutionContext ctx, ChangeNameInput input)
{
    return RegisterChangeInternal(ctx, this, input, (c, i, clone) =>
        clone.SetFirstName(c, i.FirstName) & clone.SetLastName(c, i.LastName)
    );
    // Auditoria e versão são AUTOMÁTICAS!
}
```

O código é similar em tamanho, mas Clone-Modify-Return não esquece nada.

## Fundamentação Teórica

### O Que o Functional Programming Diz

Linguagens funcionais (Haskell, F#, Clojure) tratam imutabilidade como padrão:

> "Immutable data structures make it easier to reason about program behavior and eliminate entire classes of bugs."
>
> *Estruturas de dados imutáveis facilitam raciocinar sobre o comportamento do programa e eliminam classes inteiras de bugs.*

Clone-Modify-Return traz benefícios da programação funcional para OOP.

### O Que o DDD Diz

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre modificação de agregados:

> "The modification of an Aggregate should be an all-or-nothing operation. If validation fails, the Aggregate should remain in its previous consistent state."
>
> *A modificação de um Agregado deve ser uma operação tudo-ou-nada. Se a validação falhar, o Agregado deve permanecer em seu estado consistente anterior.*

Clone-Modify-Return implementa exatamente isso - tudo ou nada.

### O Que o Event Sourcing Diz

Greg Young sobre consistência:

> "Never put an Aggregate in an invalid state. If you cannot complete an operation, the Aggregate should remain unchanged."
>
> *Nunca coloque um Agregado em estado inválido. Se você não pode completar uma operação, o Agregado deve permanecer inalterado.*

Com Clone-Modify-Return, se qualquer parte falha, nenhuma mudança é aplicada.

## Antipadrões Relacionados

### Antipadrão: Métodos void

```csharp
// ❌ void não comunica falha
public void UpdatePrice(decimal newPrice)
{
    if (newPrice < 0)
        throw new ArgumentException("Price cannot be negative");

    Price = newPrice;  // Modificou this
}

// Chamador não sabe se modificou ou lançou exceção
product.UpdatePrice(-10);  // Exceção ou modificação?
```

### Antipadrão: Try/Catch para Controle de Fluxo

```csharp
// ❌ Exceção para validação esperada
try
{
    person.ChangeName("", "");  // Lança exceção
}
catch (ValidationException ex)
{
    // Estado de person é desconhecido!
    // Modificou parcialmente antes de lançar?
}
```

### Antipadrão: Flags de Sucesso

```csharp
// ❌ Flag separada do resultado
public bool ChangeName(string firstName, string lastName)
{
    if (string.IsNullOrWhiteSpace(firstName))
        return false;  // Não modificou... ou modificou?

    FirstName = firstName;

    if (string.IsNullOrWhiteSpace(lastName))
        return false;  // Já modificou FirstName!

    LastName = lastName;
    return true;
}
```

## Decisões Relacionadas

- [DE-003](./DE-003-imutabilidade-controlada-clone-modify-return.md) - Imutabilidade Controlada (Clone-Modify-Return)
- [DE-004](./DE-004-estado-invalido-nunca-existe-na-memoria.md) - Estado Inválido Nunca Existe na Memória
- [DE-023](./DE-023-register-internal-chamado-uma-unica-vez.md) - Register*Internal Chamado Uma Única Vez
- [DE-031](./DE-031-entityinfo-gerenciado-pela-classe-base.md) - EntityInfo Gerenciado pela Classe Base

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Implementa RegisterChangeInternal que garante Clone-Modify-Return |
| [EntityInfo](../../building-blocks/domain-entities/models/entity-info.md) | Atualizado automaticamente durante RegisterChangeInternal |
| [EntityChangeInfo](../../building-blocks/domain-entities/models/entity-change-info.md) | Rastreia LastChangedAt/By automaticamente |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - comentário LLM_ANTIPATTERN sobre mutabilidade direta
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - Implementação de RegisterChangeInternal
