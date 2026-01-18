# DE-032: Optimistic Locking com EntityVersion

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **editor de documentos colaborativo** com dois modelos de controle de concorrência:

**Modelo "quem salva por último ganha" (sem controle)**:
- Alice abre o documento (versão 1)
- Bob abre o documento (versão 1)
- Alice edita e salva → documento atualizado
- Bob edita e salva → sobrescreve alterações de Alice!
- Trabalho de Alice perdido silenciosamente

**Modelo "número de revisão" (optimistic locking)**:
- Alice abre o documento (versão 1)
- Bob abre o documento (versão 1)
- Alice edita e salva → documento versão 2
- Bob tenta salvar versão 1 → sistema rejeita: "documento foi modificado"
- Bob recarrega, vê alterações de Alice, decide como proceder

O `EntityVersion` é como o número de revisão - permite detectar conflitos de concorrência e evitar perda de dados.

---

### O Problema Técnico

Sem controle de concorrência, atualizações simultâneas causam "lost updates":

```csharp
// ❌ CENÁRIO: Lost Update (sem optimistic locking)

// Usuário A carrega pedido (Total: R$ 100, sem versão)
var orderA = repository.GetById(orderId);
// orderA.Total = 100

// Usuário B carrega o MESMO pedido (Total: R$ 100)
var orderB = repository.GetById(orderId);
// orderB.Total = 100

// Usuário A adiciona item de R$ 50
orderA = orderA.AddItem(ctx, itemA);  // Total agora R$ 150
repository.Save(orderA);              // Salva R$ 150

// Usuário B adiciona item de R$ 30 (ainda pensando que era R$ 100)
orderB = orderB.AddItem(ctx, itemB);  // Total = 100 + 30 = R$ 130
repository.Save(orderB);              // SOBRESCREVE com R$ 130!

// RESULTADO: Item de A (R$ 50) FOI PERDIDO!
// Pedido deveria ter R$ 180 (100 + 50 + 30), mas tem R$ 130
```

**Problemas graves**:

1. **Perda silenciosa de dados**: Alterações são sobrescritas sem aviso
2. **Inconsistência**: Estado final não reflete todas as operações
3. **Difícil reproduzir**: Bugs de concorrência são intermitentes
4. **Audit trail quebrado**: Histórico não mostra o que realmente aconteceu

---

### Estratégias de Controle de Concorrência

**Pessimistic Locking (bloqueio pessimista)**:
```sql
-- ⚠️ Bloqueia o registro durante toda a transação
SELECT * FROM Orders WHERE Id = @id FOR UPDATE;
-- Outros usuários ficam esperando...
```

**Problemas**:
- Bloqueia recursos por tempo indeterminado
- Deadlocks em operações complexas
- Não funciona bem em sistemas distribuídos
- Escalabilidade limitada

**Optimistic Locking (bloqueio otimista)** ✅:
```sql
-- ✅ Verifica versão no momento do UPDATE
UPDATE Orders
SET ..., Version = @newVersion
WHERE Id = @id AND Version = @expectedVersion;

-- Se RowsAffected = 0 → conflito detectado!
```

**Vantagens**:
- Sem bloqueios longos
- Funciona em sistemas distribuídos
- Alta escalabilidade
- Conflitos são raros (otimismo justificado)

## A Decisão

### Nossa Abordagem

O `EntityVersion` é um `RegistryVersion` (baseado em timestamp) que:
- É gerado automaticamente no `RegisterNew`
- É atualizado automaticamente no `RegisterChange`
- Permite detecção de conflitos no repository

```csharp
// RegistryVersion - versão baseada em timestamp (UTC Ticks)
public readonly struct RegistryVersion
    : IEquatable<RegistryVersion>, IComparable<RegistryVersion>
{
    public long Value { get; }  // UTC Ticks (8 bytes, ordenável)

    public static RegistryVersion GenerateNewVersion(TimeProvider timeProvider)
    {
        // Gera versão monotônica baseada em timestamp
        // Proteção contra clock drift incluída
    }
}
```

### Estrutura no EntityInfo

```csharp
public readonly record struct EntityInfo
{
    public Id Id { get; }
    public TenantInfo TenantInfo { get; }
    public EntityChangeInfo EntityChangeInfo { get; }
    public RegistryVersion EntityVersion { get; }  // ✅ Versão para optimistic locking
}
```

### Geração Automática pela Classe Base

```csharp
// RegisterNewInternal - gera primeira versão
protected static TEntityBase? RegisterNewInternal<TInput>(...)
{
    bool entityInfoResult = entity.SetEntityInfo(
        executionContext,
        entityInfo: EntityInfo.RegisterNew(
            executionContext,
            tenantInfo: executionContext.TenantInfo,
            createdBy: executionContext.ExecutionUser
        )
    );
    // EntityInfo.RegisterNew gera:
    // EntityVersion = RegistryVersion.GenerateNewVersion(timeProvider)
}

// RegisterChangeInternal - gera nova versão
protected TEntityBase? RegisterChangeInternal<TInput>(...)
{
    bool entityInfoResult = newInstance.SetEntityInfo(
        executionContext,
        entityInfo: newInstance.EntityInfo.RegisterChange(
            executionContext,
            changedBy: executionContext.ExecutionUser
        )
    );
    // EntityInfo.RegisterChange gera:
    // EntityVersion = RegistryVersion.GenerateNewVersion(timeProvider)
}
```

### Fluxo de Optimistic Locking

```
+-------------------------------------------------------------------------+
│                           LOAD (Leitura)                                │
│                                                                         │
│  var order = repository.GetById(orderId);                               │
│  // order.EntityInfo.EntityVersion = 638750000000000000 (v1)            │
+-------------------------------------------------------------------------+
                                    │
                                    ▼
+-------------------------------------------------------------------------+
│                          MODIFY (Modificação)                           │
│                                                                         │
│  var updatedOrder = order.AddItem(ctx, item);                           │
│  // RegisterChangeInternal gera nova versão automaticamente             │
│  // updatedOrder.EntityInfo.EntityVersion = 638750000000000001 (v2)     │
│                                                                         │
│  // Entidade SABE qual era a versão original (v1)                       │
│  // e qual é a nova versão (v2)                                         │
+-------------------------------------------------------------------------+
                                    │
                                    ▼
+-------------------------------------------------------------------------+
│                           SAVE (Persistência)                           │
│                                                                         │
│  // Repository usa a versão ORIGINAL para verificar conflito            │
│  UPDATE Orders                                                          │
│  SET ..., EntityVersion = @newVersion                                   │
│  WHERE Id = @id AND EntityVersion = @originalVersion;                   │
│                                                                         │
│  // Se RowsAffected = 0 → ConcurrencyException!                        │
│  // Se RowsAffected = 1 → Sucesso                                       │
+-------------------------------------------------------------------------+
```

### Implementação no Repository

```csharp
public class OrderRepository : IOrderRepository
{
    public async Task<bool> SaveAsync(Order order, RegistryVersion originalVersion)
    {
        var rowsAffected = await _db.ExecuteAsync(@"
            UPDATE Orders
            SET
                CustomerName = @CustomerName,
                Total = @Total,
                EntityVersion = @NewVersion,
                LastChangedAt = @LastChangedAt,
                LastChangedBy = @LastChangedBy
            WHERE
                Id = @Id
                AND EntityVersion = @OriginalVersion",  -- Verifica versão!
            new
            {
                order.CustomerName,
                order.Total,
                NewVersion = order.EntityInfo.EntityVersion.Value,
                order.EntityInfo.EntityChangeInfo.LastChangedAt,
                order.EntityInfo.EntityChangeInfo.LastChangedBy,
                Id = order.EntityInfo.Id.Value,
                OriginalVersion = originalVersion.Value
            });

        if (rowsAffected == 0)
        {
            throw new ConcurrencyException(
                $"Order {order.EntityInfo.Id} was modified by another user. " +
                $"Expected version {originalVersion}, but it was changed."
            );
        }

        return true;
    }
}
```

### Uso no Application Service

```csharp
public class OrderService
{
    public async Task<Order?> AddItemAsync(
        ExecutionContext ctx,
        Guid orderId,
        AddItemInput itemInput
    )
    {
        // 1. Carrega entidade (captura versão original)
        var order = await _repository.GetByIdAsync(orderId);
        if (order is null)
        {
            ctx.AddErrorMessage("ORDER_NOT_FOUND", $"Order {orderId} not found");
            return null;
        }

        // Guarda versão original ANTES da modificação
        var originalVersion = order.EntityInfo.EntityVersion;

        // 2. Modifica entidade (gera nova versão automaticamente)
        var updatedOrder = order.AddItem(ctx, itemInput);
        if (updatedOrder is null)
            return null;

        // 3. Tenta salvar com verificação de versão
        try
        {
            await _repository.SaveAsync(updatedOrder, originalVersion);
            return updatedOrder;
        }
        catch (ConcurrencyException ex)
        {
            ctx.AddErrorMessage("CONCURRENCY_CONFLICT", ex.Message);
            return null;
        }
    }
}
```

### Por Que Timestamp em Vez de Inteiro Incrementado?

| Aspecto | Inteiro Incrementado | Timestamp (RegistryVersion) |
|---------|---------------------|----------------------------|
| **Tamanho** | 4 bytes (int) | 8 bytes (long) |
| **Ordenação temporal** | Não | Sim - sabe QUANDO foi modificado |
| **Distribuído** | Conflitos se múltiplos nós | Naturalmente único (resolução 100ns) |
| **Debug** | "Versão 42" - quando foi? | Converte para DateTimeOffset |
| **Clock drift** | N/A | Proteção contra retrocesso |

```csharp
// Inteiro: sabe que é versão 42, mas QUANDO foi isso?
entity.Version = 42;

// RegistryVersion: sabe exatamente quando
entity.EntityInfo.EntityVersion.AsDateTimeOffset
// → 2025-06-15T10:30:00.0000000+00:00
```

### Características do RegistryVersion

```csharp
/// ESTRUTURA DO LONG (64 bits / 8 bytes):
/// +------------------------------------------------------------+
/// │           UTC Ticks (64 bits)                              │
/// │           ~29.000 anos desde 01/01/0001 00:00:00 UTC       │
/// │           Resolução: 100 nanosegundos                      │
/// +------------------------------------------------------------+

// Características:
// - Performance: ~40 nanosegundos por geração
// - Tamanho: 8 bytes (menor que Guid)
// - Ordenável: v1 < v2 significa v1 foi criada antes
// - Monotônico: sempre crescente, mesmo com clock drift
// - Thread-safe: usa ThreadStatic, sem locks
```

### Proteção Contra Clock Drift

```csharp
public static RegistryVersion GenerateNewVersion(DateTimeOffset dateTimeOffset)
{
    long ticks = dateTimeOffset.UtcTicks;

    // Se relógio retrocedeu ou tempo igual ao último:
    // incrementa 1 tick (100ns) para garantir monotonicidade
    if (ticks <= _lastTicks)
        ticks = _lastTicks + 1;

    _lastTicks = ticks;
    return new RegistryVersion(ticks);
}
```

Isso garante que versões sejam SEMPRE crescentes, mesmo se:
- Relógio do sistema retroceder (ajuste NTP, virtualização)
- Múltiplas versões geradas no mesmo instante
- Clock drift de hardware

### Testabilidade

```csharp
[Fact]
public async Task Save_WhenVersionMismatch_ShouldThrowConcurrencyException()
{
    // Arrange
    var fakeTimeProvider = new FakeTimeProvider(
        new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero)
    );
    var ctx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);

    var order = Order.RegisterNew(ctx, input);
    await _repository.SaveAsync(order, order.EntityInfo.EntityVersion);

    // Simula outro usuário modificando
    fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
    var otherCtx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);
    var otherOrder = await _repository.GetByIdAsync(order.EntityInfo.Id);
    var otherUpdated = otherOrder!.AddItem(otherCtx, otherItem);
    await _repository.SaveAsync(otherUpdated, otherOrder.EntityInfo.EntityVersion);

    // Act - tenta salvar com versão antiga
    var originalVersion = order.EntityInfo.EntityVersion;  // Versão antes da modificação do outro
    var myUpdated = order.AddItem(ctx, myItem);

    // Assert
    var act = () => _repository.SaveAsync(myUpdated, originalVersion);
    await act.Should().ThrowAsync<ConcurrencyException>();
}

[Fact]
public void RegisterChange_ShouldGenerateNewVersion()
{
    // Arrange
    var time1 = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
    var fakeTimeProvider = new FakeTimeProvider(time1);
    var ctx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);

    var entity = Entity.RegisterNew(ctx, input);
    var originalVersion = entity.EntityInfo.EntityVersion;

    // Avança tempo
    fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

    // Act
    var updated = entity.ChangeName(ctx, newNameInput);

    // Assert
    updated!.EntityInfo.EntityVersion.Should().BeGreaterThan(originalVersion);
    updated.EntityInfo.EntityVersion.AsDateTimeOffset.Should().Be(time1.AddMinutes(5));
}
```

### Benefícios

1. **Detecção de conflitos**: Sabe quando dados foram modificados por outro
   ```csharp
   // WHERE EntityVersion = @originalVersion
   // Se não encontrar, conflito detectado
   ```

2. **Sem bloqueios**: Não trava recursos durante edição
   ```csharp
   // Usuário pode demorar horas editando
   // Só verifica conflito no momento do save
   ```

3. **Escalabilidade**: Funciona em sistemas distribuídos
   ```csharp
   // Múltiplos servidores, cada um gera versão única
   // Timestamp com resolução de 100ns evita colisões
   ```

4. **Auditoria temporal**: Sabe quando cada versão foi criada
   ```csharp
   entity.EntityInfo.EntityVersion.AsDateTimeOffset
   // → Exatamente quando esta versão foi gerada
   ```

5. **Gerenciamento automático**: Desenvolvedor não precisa lembrar
   ```csharp
   // RegisterChangeInternal atualiza versão automaticamente
   // Impossível esquecer de incrementar
   ```

### Trade-offs (Com Perspectiva)

- **Conflito requer retry/merge**: Usuário precisa resolver conflito
  - **Mitigação**: Conflitos são raros em uso normal. Quando ocorrem, é melhor saber do que perder dados.

- **8 bytes vs 4 bytes**: RegistryVersion usa mais espaço que int
  - **Mitigação**: 4 bytes extras por registro é negligenciável. O benefício de ordenação temporal vale.

### Trade-offs Frequentemente Superestimados

**"Pessimistic locking é mais seguro"**

Pessimistic locking evita conflitos, mas cria outros problemas:
- Deadlocks em operações complexas
- Recursos bloqueados se usuário abandonar sessão
- Não funciona bem em sistemas distribuídos
- Escalabilidade limitada

Optimistic locking é mais adequado para a maioria dos cenários web/API.

**"Conflitos vão acontecer o tempo todo"**

Na prática, conflitos são raros:
- Usuários geralmente trabalham em dados diferentes
- Mesmo trabalhando no mesmo dado, timing de conflito é raro
- Sistemas são "otimistas" justamente porque conflitos são exceção

## Fundamentação Teórica

### O Que o DDD Diz

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre concorrência:

> "Optimistic concurrency is a simple and effective way to handle concurrent modifications of Aggregates. [...] Use a version number or timestamp as the concurrency control mechanism."
>
> *Concorrência otimista é uma forma simples e efetiva de lidar com modificações concorrentes de Aggregates. [...] Use um número de versão ou timestamp como mecanismo de controle de concorrência.*

### O Que o Patterns of Enterprise Application Architecture Diz

Martin Fowler em "Patterns of Enterprise Application Architecture" (2002) sobre Optimistic Offline Lock:

> "Optimistic Offline Lock solves the problem of concurrent business transactions by detecting a conflict when it occurs rather than preventing it from happening."
>
> *Optimistic Offline Lock resolve o problema de transações de negócio concorrentes detectando um conflito quando ele ocorre ao invés de preveni-lo de acontecer.*

### Princípio da Imutabilidade

O `RegistryVersion` é imutável (readonly struct). Cada modificação gera uma NOVA versão, preservando a anterior:

```csharp
var v1 = entity.EntityInfo.EntityVersion;
var updated = entity.ChangeName(ctx, input);
var v2 = updated.EntityInfo.EntityVersion;

// v1 ainda existe, inalterada
// v2 é uma nova versão
// v1 < v2 (ordenação temporal garantida)
```

## Antipadrões Documentados

### Antipadrão 1: Sem Controle de Versão

```csharp
// ❌ UPDATE sem verificar versão
UPDATE Orders SET CustomerName = @name WHERE Id = @id;

// Qualquer modificação sobrescreve silenciosamente
// Lost updates garantidos
```

### Antipadrão 2: Versão Gerenciada Manualmente

```csharp
// ❌ Desenvolvedor precisa lembrar de incrementar
public Order? UpdateCustomer(ExecutionContext ctx, string newName)
{
    var clone = this.Clone();
    clone.CustomerName = newName;
    clone.Version++;  // Fácil esquecer!
    return clone;
}
```

### Antipadrão 3: Versão como Inteiro Simples

```csharp
// ❌ Inteiro não tem informação temporal
public int Version { get; set; }

// Versão 42 - quando foi isso? Não sei.
// Em sistemas distribuídos, pode colidir.
```

### Antipadrão 4: Ignorar Conflitos

```csharp
// ❌ Captura exceção e ignora
try
{
    await repository.SaveAsync(entity, originalVersion);
}
catch (ConcurrencyException)
{
    // Ignora conflito - dados perdidos silenciosamente
}
```

### Antipadrão 5: Pessimistic Lock para Tudo

```csharp
// ❌ Bloqueia registro para qualquer operação
using var transaction = await _db.BeginTransactionAsync(IsolationLevel.Serializable);
var order = await _db.QueryAsync("SELECT * FROM Orders WITH (UPDLOCK) WHERE Id = @id");

// Outros usuários bloqueados esperando
// Deadlocks em operações complexas
// Escalabilidade limitada
```

## Decisões Relacionadas

- [DE-003](./DE-003-imutabilidade-controlada-clone-modify-return.md) - Imutabilidade Controlada (cada modificação gera nova versão)
- [DE-023](./DE-023-register-internal-chamado-uma-unica-vez.md) - Register*Internal chamado uma única vez (versão atualizada automaticamente)
- [DE-029](./DE-029-timeprovider-encapsulado-no-executioncontext.md) - TimeProvider encapsulado (versão usa TimeProvider)
- [DE-031](./DE-031-entityinfo-gerenciado-pela-classe-base.md) - EntityInfo gerenciado pela classe base (EntityVersion faz parte)

## Building Blocks Relacionados

- **[RegistryVersion](../../building-blocks/core/registry-versions/registry-version.md)** - Documentação completa sobre versões monotônicas baseadas em timestamp, incluindo proteção contra clock drift, performance e casos de uso.

## Leitura Recomendada

- [Implementing Domain-Driven Design - Vaughn Vernon](https://vaughnvernon.com/)
- [Patterns of Enterprise Application Architecture - Martin Fowler](https://martinfowler.com/eaaCatalog/optimisticOfflineLock.html)
- [Optimistic Concurrency - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/optimistic-concurrency)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityInfo](../../building-blocks/domain-entities/models/entity-info.md) | Contém a propriedade EntityVersion usada para optimistic locking |
| [RegistryVersion](../../building-blocks/core/registry-versions/registry-version.md) | Value Object que encapsula a versão da entidade, incrementando automaticamente em cada modificação |

## Referências no Código

- [RegistryVersion.cs](../../../src/BuildingBlocks/Core/RegistryVersions/RegistryVersion.cs) - Implementação completa
- [EntityInfo.cs](../../../src/BuildingBlocks/Domain.Entities/Models/EntityInfo.cs) - propriedade EntityVersion
- [EntityInfo.cs](../../../src/BuildingBlocks/Domain.Entities/Models/EntityInfo.cs) - geração de nova versão
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - validação de EntityVersion
