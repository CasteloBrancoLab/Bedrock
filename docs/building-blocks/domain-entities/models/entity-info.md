# 🏷️ EntityInfo - Identidade Completa de Entidades

O `EntityInfo` é um `readonly record struct` que encapsula todos os metadados fundamentais de uma entidade de domínio: identificação, multi-tenancy, auditoria e versionamento.

> 💡 **Visão Geral:** Consolide **Id**, **TenantInfo**, **EntityChangeInfo** e **RegistryVersion** em uma única estrutura imutável, garantindo consistência e integridade dos metadados de entidades.

---

## 📋 Sumário

- [Por Que Usar EntityInfo](#-por-que-usar-entityinfo)
- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
- [Funcionalidades](#-funcionalidades)
- [Como Usar](#-como-usar)
- [Integração com EntityBase](#-integração-com-entitybase)
- [Trade-offs](#️-trade-offs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)

---

## 🎯 Por Que Usar EntityInfo?

| Característica | Propriedades Soltas | **EntityInfo** | Base Class Herdada |
|----------------|---------------------|----------------|-------------------|
| **Coesão** | ❌ Espalhadas | ✅ **Agrupadas** | ⚠️ Acopladas |
| **Imutabilidade** | ❌ Mutáveis | ✅ **Garantida** | ⚠️ Difícil garantir |
| **Testabilidade** | ❌ Dependências ocultas | ✅ **Injeção via ExecutionContext** | ⚠️ Static dependencies |
| **Reutilização** | ❌ Copy-paste | ✅ **Composição** | ⚠️ Herança rígida |
| **Consistência** | ❌ Manual | ✅ **Automática** | ⚠️ Parcial |

---

## 🎯 Contexto: Por Que Existe

### O Problema Real

Toda entidade de domínio precisa de metadados fundamentais: ID, tenant, auditoria e versão. Quando esses dados ficam espalhados ou são gerenciados manualmente, surgem inconsistências e código duplicado.

**Exemplo de abordagens problemáticas:**

```csharp
❌ Propriedades soltas na entidade:
public class Order
{
    public Guid Id { get; set; }                    // ⚠️ Guid vs Id custom?
    public Guid TenantId { get; set; }              // ⚠️ Só o ID, sem código?
    public string TenantCode { get; set; }          // ⚠️ Duplicado em cada entidade
    public DateTime CreatedAt { get; set; }         // ⚠️ DateTime vs DateTimeOffset?
    public string CreatedBy { get; set; }           // ⚠️ Validação?
    public DateTime? ModifiedAt { get; set; }       // ⚠️ Nomenclatura diferente
    public string ModifiedBy { get; set; }          // ⚠️ Quem preenche?
    public int Version { get; set; }                // ⚠️ int vs long vs custom?
}

❌ Problemas:
- Cada entidade repete 8+ propriedades de metadados
- Tipos inconsistentes entre entidades
- Preenchimento manual e propenso a erros
- Difícil garantir que todos os campos sejam preenchidos
```

### A Solução

```csharp
✅ Abordagem com EntityInfo:
public sealed class Order : EntityBase<Order>
{
    // EntityInfo encapsula TUDO: Id, TenantInfo, EntityChangeInfo, EntityVersion
    // Herdado de EntityBase - zero duplicação!

    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }
    // ... apenas propriedades de NEGÓCIO
}

// Criação automática com todos os metadados
var order = Order.RegisterNew(executionContext, input);
// order.EntityInfo.Id                → UUIDv7 gerado automaticamente
// order.EntityInfo.TenantInfo        → Do ExecutionContext
// order.EntityInfo.EntityChangeInfo  → Auditoria automática
// order.EntityInfo.EntityVersion     → Versão monotônica

✅ Benefícios:
- Uma única estrutura para todos os metadados
- Tipos consistentes em todo o sistema
- Preenchimento automático via ExecutionContext
- Imutabilidade garantida
```

---

## 🔧 Problemas Resolvidos

### 1. 📦 Fragmentação de Metadados

**Problema:** Metadados de entidade espalhados e inconsistentes.

#### 📚 Analogia: Documento de Identidade

Imagine se cada pessoa tivesse que carregar documentos separados: um papel com nome, outro com CPF, outro com foto, outro com endereço. Seria caótico! O RG/CNH consolida tudo em um documento. O `EntityInfo` faz o mesmo para entidades — consolida **Id**, **Tenant**, **Auditoria** e **Versão** em uma estrutura única.

#### 💻 Impacto Real no Código

```csharp
❌ Antes - Metadados fragmentados:
public class Customer
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string LastModifiedBy { get; set; }
    public int Version { get; set; }

    // Propriedades de negócio
    public string Name { get; set; }
}

// Cada entidade repete 8 propriedades de infraestrutura!

✅ Depois - EntityInfo consolidado:
public sealed class Customer : EntityBase<Customer>
{
    // EntityInfo vem de EntityBase ✨
    // Contém: Id, TenantInfo, EntityChangeInfo, EntityVersion

    // Apenas propriedades de NEGÓCIO
    public string Name { get; private set; }
}
```

---

### 2. 🔄 Consistência em Operações

**Problema:** Ao criar ou modificar entidades, fácil esquecer de atualizar algum metadado.

#### 📚 Analogia: Carimbo de Protocolo

Quando você protocola um documento em um órgão público, o funcionário carimba **data**, **hora**, **número de protocolo** e **assinatura** de uma vez só — não há como esquecer um item. O `EntityInfo.RegisterNew()` e `RegisterChange()` funcionam igual: atualizam **todos** os metadados necessários atomicamente.

#### 💻 Impacto Real no Código

```csharp
❌ Antes - Atualização manual e propensa a erros:
public Order UpdateStatus(OrderStatus newStatus)
{
    var order = this.Clone();
    order.Status = newStatus;
    order.ModifiedAt = DateTime.Now;     // ⚠️ E se esquecer?
    order.ModifiedBy = currentUser;      // ⚠️ Quem é currentUser?
    order.Version++;                     // ⚠️ E se esquecer?
    return order;
}

✅ Depois - Atualização atômica:
public Order? UpdateStatus(ExecutionContext context, OrderStatus newStatus)
{
    return RegisterChangeInternal<Order, OrderStatus>(
        context,
        instance: this,
        input: newStatus,
        handler: (ctx, status, entity) =>
        {
            entity.Status = status;
            return true;
        }
    );
    // EntityInfo.RegisterChange() chamado automaticamente ✨
    // LastChangedAt, LastChangedBy e EntityVersion atualizados!
}
```

---

### 3. 🧪 Testabilidade de Metadados

**Problema:** Metadados que dependem de `DateTime.Now` ou `Guid.NewGuid()` são imprevisíveis em testes.

#### 📚 Analogia: Laboratório de Análises

Um laboratório de análises clínicas precisa de condições controladas — temperatura, iluminação, equipamentos calibrados. Se cada analista usasse seu próprio termômetro caseiro, os resultados seriam inconsistentes. O `EntityInfo` garante que todos os metadados venham de fontes controladas (`TimeProvider`, `ExecutionContext`), permitindo **testes determinísticos**.

#### 💻 Impacto Real no Código

```csharp
❌ Antes - Testes flaky:
[Fact]
public void CreateOrder_ShouldSetCreatedAt()
{
    var before = DateTime.Now;
    var order = new Order();
    var after = DateTime.Now;

    Assert.InRange(order.CreatedAt, before, after);  // ⚠️ Flaky!
}

✅ Depois - Testes determinísticos:
[Fact]
public void CreateOrder_ShouldSetCreatedAt()
{
    var fixedTime = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero);
    var fakeTimeProvider = new FakeTimeProvider(fixedTime);

    var context = ExecutionContext.Create(
        correlationId: Guid.NewGuid(),
        tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test"),
        executionUser: "test@test.com",
        executionOrigin: "Test",
        minimumMessageType: MessageType.Information,
        timeProvider: fakeTimeProvider
    );

    var order = Order.RegisterNew(context, new OrderInput());

    Assert.Equal(fixedTime, order.EntityInfo.EntityChangeInfo.CreatedAt);  // ✅ Determinístico!
}
```

---

## ✨ Funcionalidades

### 📝 Propriedades Consolidadas

```csharp
public readonly record struct EntityInfo
{
    public Id Id { get; }                         // ✨ UUIDv7 monotônico
    public TenantInfo TenantInfo { get; }         // ✨ Multi-tenancy
    public EntityChangeInfo EntityChangeInfo { get; }  // ✨ Auditoria
    public RegistryVersion EntityVersion { get; }      // ✨ Versionamento
}
```

**Composição sobre herança:**
- `Id` → [Gerador UUIDv7 monotônico](../../core/ids/id.md)
- `TenantInfo` → [Identificador de tenant](../../core/tenant-infos/tenant-info.md)
- `EntityChangeInfo` → [Auditoria de criação/modificação](entity-change-info.md)
- `RegistryVersion` → [Versão monotônica para concorrência](../../core/registry-versions/registry-version.md)

---

### 🆕 RegisterNew - Criação de Entidade

Cria um `EntityInfo` completo para uma nova entidade.

```csharp
var context = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),
    tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Acme Corp"),
    executionUser: "admin@acme.com",
    executionOrigin: "API",
    minimumMessageType: MessageType.Information,
    timeProvider: TimeProvider.System
);

var entityInfo = EntityInfo.RegisterNew(
    executionContext: context,
    tenantInfo: context.TenantInfo,
    createdBy: context.ExecutionUser
);

// entityInfo.Id            → UUIDv7 gerado via TimeProvider
// entityInfo.TenantInfo    → Acme Corp
// entityInfo.EntityChangeInfo.CreatedAt → Timestamp atual
// entityInfo.EntityChangeInfo.CreatedBy → "admin@acme.com"
// entityInfo.EntityVersion → RegistryVersion monotônico
```

**O que acontece internamente:**
1. `Id.GenerateNewId(timeProvider)` → Gera UUIDv7
2. `EntityChangeInfo.RegisterNew()` → Cria auditoria
3. `RegistryVersion.GenerateNewVersion()` → Gera versão

---

### 🔄 RegisterChange - Modificação de Entidade

Cria um novo `EntityInfo` preservando Id e TenantInfo, atualizando auditoria e versão.

```csharp
var originalEntity = Order.RegisterNew(context, input);

// Após modificação...
var newEntityInfo = originalEntity.EntityInfo.RegisterChange(
    executionContext: context,
    changedBy: context.ExecutionUser
);

// newEntityInfo.Id                         → Mesmo ✨
// newEntityInfo.TenantInfo                 → Mesmo ✨
// newEntityInfo.EntityChangeInfo.CreatedAt → Mesmo ✨
// newEntityInfo.EntityChangeInfo.CreatedBy → Mesmo ✨
// newEntityInfo.EntityChangeInfo.LastChangedAt → ATUALIZADO
// newEntityInfo.EntityChangeInfo.LastChangedBy → ATUALIZADO
// newEntityInfo.EntityVersion              → NOVA VERSÃO
```

---

### 📦 CreateFromExistingInfo - Reconstrução

Reconstrói um `EntityInfo` a partir de dados existentes (banco de dados, APIs).

```csharp
// Opção 1: Passando EntityChangeInfo já construído
var entityInfo = EntityInfo.CreateFromExistingInfo(
    id: Id.CreateFromGuid(dbRecord.Id),
    tenantInfo: TenantInfo.Create(dbRecord.TenantId, dbRecord.TenantCode),
    entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
        createdAt: dbRecord.CreatedAt,
        createdBy: dbRecord.CreatedBy,
        createdCorrelationId: dbRecord.CreatedCorrelationId,
        createdExecutionOrigin: dbRecord.CreatedExecutionOrigin,
        lastChangedAt: dbRecord.LastChangedAt,
        lastChangedBy: dbRecord.LastChangedBy,
        lastChangedCorrelationId: dbRecord.LastChangedCorrelationId,
        lastChangedExecutionOrigin: dbRecord.LastChangedExecutionOrigin
    ),
    entityVersion: RegistryVersion.CreateFromExistingVersion(dbRecord.Version)
);

// Opção 2: Overload simplificado com parâmetros separados
var entityInfoSimplificado = EntityInfo.CreateFromExistingInfo(
    id: Id.CreateFromGuid(dbRecord.Id),
    tenantInfo: TenantInfo.Create(dbRecord.TenantId, dbRecord.TenantCode),
    createdAt: dbRecord.CreatedAt,
    createdBy: dbRecord.CreatedBy,
    createdCorrelationId: dbRecord.CreatedCorrelationId,
    createdExecutionOrigin: dbRecord.CreatedExecutionOrigin,
    lastChangedAt: dbRecord.LastChangedAt,
    lastChangedBy: dbRecord.LastChangedBy,
    lastChangedCorrelationId: dbRecord.LastChangedCorrelationId,
    lastChangedExecutionOrigin: dbRecord.LastChangedExecutionOrigin,
    entityVersion: RegistryVersion.CreateFromExistingVersion(dbRecord.Version)
);
```

---

## 📖 Como Usar

### 1️⃣ Uso Básico - Via EntityBase

O uso mais comum é indireto, via `EntityBase<T>`:

```csharp
public sealed class Product : EntityBase<Product>
{
    public string Name { get; private set; }
    public Money Price { get; private set; }

    private Product(EntityInfo entityInfo, string name, Money price)
        : base(entityInfo)
    {
        Name = name;
        Price = price;
    }

    public static Product? RegisterNew(ExecutionContext context, ProductInput input)
    {
        return RegisterNewInternal<Product, ProductInput>(
            context,
            input,
            entityFactory: (ctx, inp) => new Product(
                entityInfo: default,  // Será preenchido por RegisterNewInternal
                name: inp.Name,
                price: inp.Price
            ),
            handler: (ctx, inp, entity) =>
            {
                // Validações adicionais se necessário
                return entity.IsValid(ctx);
            }
        );
    }
}

// Uso
var product = Product.RegisterNew(context, new ProductInput("Widget", Money.FromDecimal(99.90m)));
// product.EntityInfo está completamente preenchido!
```

**Quando usar:** Na grande maioria dos casos — deixe `EntityBase` gerenciar.

---

### 2️⃣ Uso Intermediário - Acesso aos Metadados

```csharp
public class OrderService
{
    public OrderDto ToDto(Order order)
    {
        return new OrderDto
        {
            // Acesso via EntityInfo
            Id = order.EntityInfo.Id.ToGuid(),
            TenantCode = order.EntityInfo.TenantInfo.Code,
            CreatedAt = order.EntityInfo.EntityChangeInfo.CreatedAt,
            CreatedBy = order.EntityInfo.EntityChangeInfo.CreatedBy,
            LastModifiedAt = order.EntityInfo.EntityChangeInfo.LastChangedAt,
            LastModifiedBy = order.EntityInfo.EntityChangeInfo.LastChangedBy,
            Version = order.EntityInfo.EntityVersion.Value,

            // Dados de negócio
            Status = order.Status,
            Total = order.Total
        };
    }
}
```

**Quando usar:** Ao mapear entidades para DTOs ou exibição.

---

### 3️⃣ Uso Avançado - Reconstrução do Banco

```csharp
public sealed class OrderRepository : IOrderRepository
{
    public Order? GetById(ExecutionContext context, Id orderId)
    {
        var dbRecord = _dbContext.Orders
            .Where(o => o.Id == orderId.ToGuid())
            .Where(o => o.TenantId == context.TenantInfo.TenantId)
            .FirstOrDefault();

        if (dbRecord == null)
            return null;

        // Usando o overload simplificado com suporte a CorrelationId e ExecutionOrigin
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromGuid(dbRecord.Id),
            tenantInfo: TenantInfo.Create(dbRecord.TenantId, dbRecord.TenantCode),
            createdAt: dbRecord.CreatedAt,
            createdBy: dbRecord.CreatedBy,
            createdCorrelationId: dbRecord.CreatedCorrelationId,
            createdExecutionOrigin: dbRecord.CreatedExecutionOrigin,
            lastChangedAt: dbRecord.LastChangedAt,
            lastChangedBy: dbRecord.LastChangedBy,
            lastChangedCorrelationId: dbRecord.LastChangedCorrelationId,
            lastChangedExecutionOrigin: dbRecord.LastChangedExecutionOrigin,
            entityVersion: RegistryVersion.CreateFromExistingVersion(dbRecord.Version)
        );

        return Order.CreateFromExistingInfo(entityInfo, dbRecord);
    }
}
```

**Quando usar:** Implementação de repositórios.

---

## 🔗 Integração com EntityBase

`EntityInfo` é o coração de `EntityBase`:

```csharp
public abstract class EntityBase : IEntity
{
    public EntityInfo EntityInfo { get; private set; }

    protected EntityBase(EntityInfo entityInfo)
    {
        EntityInfo = entityInfo;
    }

    // Métodos protegidos gerenciam EntityInfo automaticamente
    protected static TEntityBase? RegisterNewInternal<TEntityBase, TInput>(...)
    {
        // Cria EntityInfo via RegisterNew
        bool entityInfoResult = entity.SetEntityInfo(
            executionContext,
            entityInfo: EntityInfo.RegisterNew(
                executionContext,
                tenantInfo: executionContext.TenantInfo,
                createdBy: executionContext.ExecutionUser
            )
        );
        // ...
    }

    protected static TEntityBase? RegisterChangeInternal<TEntityBase, TInput>(...)
    {
        // Atualiza EntityInfo via RegisterChange
        bool entityInfoResult = newInstance.SetEntityInfo(
            executionContext,
            entityInfo: newInstance.EntityInfo.RegisterChange(
                executionContext,
                changedBy: executionContext.ExecutionUser
            )
        );
        // ...
    }
}
```

---

## ⚖️ Trade-offs

### Benefícios

| Benefício | Impacto | Análise |
|-----------|---------|---------|
| **Coesão** | ✅ Alto | Todos os metadados em um lugar |
| **Consistência** | ✅ Alto | Tipos e nomenclatura padronizados |
| **Imutabilidade** | ✅ Alto | `readonly record struct` |
| **Testabilidade** | ✅ Alto | Dependências injetadas via ExecutionContext |
| **DRY** | ✅ Alto | Zero duplicação de código de metadados |

### Custos

| Custo | Impacto | Mitigação |
|-------|---------|-----------|
| **Indireção** | ⚠️ Baixo | `entity.EntityInfo.Id` vs `entity.Id` — IDE autocomplete ajuda |
| **Curva de aprendizado** | ⚠️ Baixo | Padrão consistente, aprende uma vez |

### Quando Usar vs Quando Evitar

#### ✅ Use quando:
1. Entidades de domínio que precisam de persistência
2. Sistemas multi-tenant
3. Auditoria é requisito
4. Controle de concorrência otimista é necessário

#### ❌ Evite quando:
1. Value objects simples
2. DTOs e modelos de transferência
3. Entidades anêmicas sem comportamento

---

## 🔬 Exemplos Avançados

### 🔍 Verificação de Concorrência Otimista

```csharp
public sealed class OrderRepository
{
    public bool Update(ExecutionContext context, Order order)
    {
        var currentVersion = _dbContext.Orders
            .Where(o => o.Id == order.EntityInfo.Id.ToGuid())
            .Select(o => o.Version)
            .FirstOrDefault();

        // Verifica se a versão mudou desde a leitura
        if (currentVersion != order.EntityInfo.EntityVersion.Value)
        {
            context.AddErrorMessage(
                "ORDER_CONCURRENCY_CONFLICT",
                $"Order was modified by another user. Current version: {currentVersion}"
            );
            return false;
        }

        // Persiste com nova versão
        _dbContext.Orders.Update(MapToDbRecord(order));
        return _dbContext.SaveChanges() > 0;
    }
}
```

---

### 🏢 Filtro Multi-Tenant Automático

```csharp
public abstract class TenantAwareRepository<T> where T : EntityBase
{
    protected IQueryable<TDbRecord> ApplyTenantFilter<TDbRecord>(
        ExecutionContext context,
        IQueryable<TDbRecord> query
    ) where TDbRecord : ITenantAware
    {
        // Filtra automaticamente pelo tenant do contexto
        return query.Where(r => r.TenantId == context.TenantInfo.TenantId);
    }
}

// Uso
public sealed class OrderRepository : TenantAwareRepository<Order>
{
    public IEnumerable<Order> GetAll(ExecutionContext context)
    {
        return ApplyTenantFilter(context, _dbContext.Orders)
            .Select(MapToEntity)
            .ToList();
    }
}
```

---

### 📊 Relatório de Atividade por Usuário

```csharp
public class ActivityReportService
{
    public UserActivityReport GetUserActivity(
        ExecutionContext context,
        string userEmail,
        DateTimeOffset startDate,
        DateTimeOffset endDate
    )
    {
        var entities = _repository.GetEntitiesModifiedByUser(userEmail, startDate, endDate);

        return new UserActivityReport
        {
            UserEmail = userEmail,
            Period = new DateRange(startDate, endDate),

            // Entidades criadas pelo usuário
            Created = entities
                .Where(e => e.EntityInfo.EntityChangeInfo.CreatedBy == userEmail)
                .Where(e => e.EntityInfo.EntityChangeInfo.CreatedAt >= startDate)
                .Where(e => e.EntityInfo.EntityChangeInfo.CreatedAt <= endDate)
                .Count(),

            // Entidades modificadas pelo usuário
            Modified = entities
                .Where(e => e.EntityInfo.EntityChangeInfo.LastChangedBy == userEmail)
                .Where(e => e.EntityInfo.EntityChangeInfo.LastChangedAt >= startDate)
                .Where(e => e.EntityInfo.EntityChangeInfo.LastChangedAt <= endDate)
                .Count(),

            // Detalhamento por tipo de entidade
            ByEntityType = entities
                .GroupBy(e => e.GetType().Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                )
        };
    }
}
```

---

## 📚 Referências

- [EntityChangeInfo](entity-change-info.md) - Dados de auditoria
- [EntityBase](../entity-base.md) - Classe base que usa EntityInfo
- [Id](../../core/ids/id.md) - Gerador de IDs UUIDv7
- [TenantInfo](../../core/tenant-infos/tenant-info.md) - Identificador de tenant
- [RegistryVersion](../../core/registry-versions/registry-version.md) - Versionamento monotônico
- [ExecutionContext](../../core/execution-contexts/execution-context.md) - Contexto de execução
