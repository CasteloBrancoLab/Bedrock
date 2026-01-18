# 📝 EntityChangeInfo - Rastreamento de Auditoria de Entidades

O `EntityChangeInfo` é um `readonly record struct` que encapsula informações de auditoria sobre criação e modificação de entidades, garantindo rastreabilidade completa do ciclo de vida.

> 💡 **Visão Geral:** Rastreie **quem** criou e **quem** modificou uma entidade, com timestamps precisos via `TimeProvider`, **CorrelationIds** e **ExecutionOrigins** para rastreabilidade completa de operações, mantendo imutabilidade e integração nativa com `ExecutionContext`.

---

## 📋 Sumário

- [Por Que Usar EntityChangeInfo](#-por-que-usar-entitychangeinfo)
- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
- [Funcionalidades](#-funcionalidades)
- [Como Usar](#-como-usar)
- [Integração com ExecutionContext](#-integração-com-executioncontext)
- [Trade-offs](#️-trade-offs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)

---

## 🎯 Por Que Usar EntityChangeInfo?

| Característica | Campos Soltos | **EntityChangeInfo** | Dictionary |
|----------------|---------------|---------------------|------------|
| **Tipagem forte** | ⚠️ Inconsistente | ✅ **Garantida** | ❌ Object boxing |
| **Imutabilidade** | ❌ Mutável | ✅ **readonly record struct** | ❌ Mutável |
| **Auditoria completa** | ⚠️ Manual | ✅ **Automática** | ⚠️ Manual |
| **Integração TimeProvider** | ❌ DateTime.Now | ✅ **Testável** | ❌ DateTime.Now |
| **Semântica clara** | ❌ Ambígua | ✅ **Explícita** | ❌ Strings mágicas |

---

## 🎯 Contexto: Por Que Existe

### O Problema Real

Em sistemas empresariais, auditoria não é opcional — é requisito legal (LGPD, SOX, HIPAA). Saber **quem** criou e **quem** modificou um registro é fundamental para compliance e investigação de incidentes.

**Exemplo de abordagens problemáticas:**

```csharp
❌ Campos soltos na entidade:
public class Order
{
    public DateTime CreatedAt { get; set; }      // ⚠️ Mutável!
    public string CreatedBy { get; set; }        // ⚠️ Pode ser alterado
    public DateTime? ModifiedAt { get; set; }    // ⚠️ Nomenclatura inconsistente
    public string ModifiedBy { get; set; }       // ⚠️ "Modified" vs "Changed"?
}

❌ Problemas:
- Campos mutáveis permitem adulteração de auditoria
- Nomenclatura inconsistente entre projetos
- DateTime.Now não é testável
- Sem garantia de preenchimento correto
```

### A Solução

```csharp
✅ Abordagem com EntityChangeInfo:
public sealed class Order : EntityBase<Order>
{
    // EntityInfo já contém EntityChangeInfo ✨
    // Auditoria garantida, imutável e testável
}

// Criação automática via ExecutionContext
var order = Order.RegisterNew(executionContext, input);
// order.EntityInfo.EntityChangeInfo.CreatedAt → Preenchido automaticamente
// order.EntityInfo.EntityChangeInfo.CreatedBy → executionContext.ExecutionUser

✅ Benefícios:
- Imutável por design (readonly record struct)
- Timestamps via TimeProvider (testável)
- Nomenclatura padronizada em todo o sistema
- Integração automática com ExecutionContext
```

---

## 🔧 Problemas Resolvidos

### 1. 📅 Timestamps Consistentes e Testáveis

**Problema:** `DateTime.Now` não é testável e varia entre servidores.

#### 📚 Analogia: Carimbador Oficial

Imagine um cartório onde cada documento precisa de um carimbo de data. Se cada funcionário usasse seu próprio relógio, teríamos inconsistências. O `EntityChangeInfo` é como ter um **carimbador oficial central** — todos os timestamps vêm da mesma fonte confiável (`TimeProvider`).

#### 💻 Impacto Real no Código

```csharp
❌ Antes - DateTime.Now direto:
public class Order
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;  // ⚠️ Não testável
}

// Em testes:
var order = new Order();
Thread.Sleep(100);
Assert.Equal(expectedTime, order.CreatedAt);  // ❌ Falha! Tempo já passou

✅ Depois - Via TimeProvider:
var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero));
var context = ExecutionContext.Create(..., timeProvider: fakeTime);

var changeInfo = EntityChangeInfo.RegisterNew(context, "user@test.com");

Assert.Equal(new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero), changeInfo.CreatedAt);  // ✅ Determinístico!
```

---

### 2. 🔒 Imutabilidade para Integridade de Auditoria

**Problema:** Campos mutáveis permitem adulteração de registros de auditoria.

#### 📚 Analogia: Livro-Razão Contábil

Em contabilidade, o livro-razão é escrito à caneta — não se apaga, apenas se faz lançamentos corretivos. O `EntityChangeInfo` funciona igual: uma vez criado, os dados de criação são **imutáveis**. Modificações geram um **novo registro** com os dados de alteração preenchidos.

#### 💻 Impacto Real no Código

```csharp
❌ Antes - Campos mutáveis:
order.CreatedBy = "hacker@evil.com";  // ⚠️ Auditoria comprometida!
order.CreatedAt = DateTime.MinValue;   // ⚠️ Evidência destruída

✅ Depois - Imutável:
var changeInfo = EntityChangeInfo.RegisterNew(context, "admin@company.com");
// changeInfo.CreatedBy = "hacker";  // ❌ Erro de compilação! Propriedade readonly

// Para registrar alteração, cria-se novo registro:
var updatedInfo = changeInfo.RegisterChange(context, "editor@company.com");
// updatedInfo.CreatedBy → "admin@company.com" (preservado!)
// updatedInfo.LastChangedBy → "editor@company.com" (novo dado)
```

---

### 3. 📊 Separação Clara: Criação vs Modificação

**Problema:** Confusão entre dados de criação e última modificação.

#### 📚 Analogia: Certidão de Nascimento vs Histórico Médico

Uma pessoa tem uma **certidão de nascimento** (imutável: data, local, pais) e um **histórico médico** (atualizado ao longo da vida). O `EntityChangeInfo` separa claramente:
- `CreatedAt`/`CreatedBy`/`CreatedCorrelationId`/`CreatedExecutionOrigin` → Certidão (nunca muda)
- `LastChangedAt`/`LastChangedBy`/`LastChangedCorrelationId`/`LastChangedExecutionOrigin` → Histórico (atualiza a cada modificação)

#### 💻 Impacto Real no Código

```csharp
// Criação inicial
var info = EntityChangeInfo.RegisterNew(context, "creator@company.com");
// info.CreatedAt                → 2024-01-15 10:00:00
// info.CreatedBy                → "creator@company.com"
// info.CreatedCorrelationId     → abc-123 (do ExecutionContext)
// info.CreatedExecutionOrigin   → "API" (do ExecutionContext)
// info.LastChangedAt            → null  ✨ Nunca foi alterado
// info.LastChangedBy            → null  ✨ Nunca foi alterado
// info.LastChangedCorrelationId → null  ✨ Nunca foi alterado
// info.LastChangedExecutionOrigin → null  ✨ Nunca foi alterado

// Após primeira modificação
var modified = info.RegisterChange(context, "editor@company.com");
// modified.CreatedAt                → 2024-01-15 10:00:00  ✨ Preservado!
// modified.CreatedBy                → "creator@company.com" ✨ Preservado!
// modified.CreatedCorrelationId     → abc-123              ✨ Preservado!
// modified.CreatedExecutionOrigin   → "API"                ✨ Preservado!
// modified.LastChangedAt            → 2024-01-15 14:30:00  ✨ Atualizado
// modified.LastChangedBy            → "editor@company.com" ✨ Atualizado
// modified.LastChangedCorrelationId → def-456 (nova operação) ✨ Atualizado
// modified.LastChangedExecutionOrigin → "Batch" (nova operação) ✨ Atualizado
```

---

### 4. 🔗 Rastreabilidade de Operações via CorrelationId

**Problema:** Dificuldade em rastrear qual requisição/operação causou uma alteração.

#### 📚 Analogia: Número de Protocolo

Quando você dá entrada em um processo em um órgão público, recebe um **número de protocolo**. Com esse número, você consegue rastrear todo o andamento e saber exatamente qual atendimento gerou cada movimentação. O `CorrelationId` funciona igual — cada operação tem um identificador único que permite rastrear todas as alterações feitas naquela requisição.

#### 💻 Impacto Real no Código

```csharp
// Em uma API, todas as entidades criadas/modificadas na mesma requisição
// terão o mesmo CorrelationId
var context = ExecutionContext.Create(
    correlationId: Guid.Parse("abc-123-def-456"),  // ID da requisição HTTP
    // ...
);

var order = Order.RegisterNew(context, input);
var customer = Customer.RegisterNew(context, customerInput);
var invoice = Invoice.RegisterNew(context, invoiceInput);

// Todas têm o mesmo CreatedCorrelationId!
// order.EntityInfo.EntityChangeInfo.CreatedCorrelationId    → abc-123-def-456
// customer.EntityInfo.EntityChangeInfo.CreatedCorrelationId → abc-123-def-456
// invoice.EntityInfo.EntityChangeInfo.CreatedCorrelationId  → abc-123-def-456

// Facilita debugging: "quais entidades foram criadas na requisição abc-123?"
```

---

## ✨ Funcionalidades

### 📝 Propriedades de Auditoria

```csharp
public readonly record struct EntityChangeInfo
{
    public DateTimeOffset CreatedAt { get; }        // ✨ Quando foi criado
    public string CreatedBy { get; }                // ✨ Quem criou
    public Guid CreatedCorrelationId { get; }       // ✨ CorrelationId da operação de criação
    public string CreatedExecutionOrigin { get; }   // ✨ Origem da operação de criação (API, Batch, etc.)
    public DateTimeOffset? LastChangedAt { get; }   // ✨ Última modificação (null se nunca)
    public string? LastChangedBy { get; }           // ✨ Quem modificou por último
    public Guid? LastChangedCorrelationId { get; }  // ✨ CorrelationId da última modificação
    public string? LastChangedExecutionOrigin { get; } // ✨ Origem da última modificação
}
```

**Por que `DateTimeOffset` e não `DateTime`?**
- Armazena informação de fuso horário
- Evita ambiguidades em sistemas distribuídos
- Padrão recomendado para timestamps de auditoria

**Por que `CorrelationId`?**
- Vincula a entidade à operação/requisição que a criou ou modificou
- Permite rastrear todas as alterações feitas em uma única transação
- Fundamental para debugging e auditoria em sistemas distribuídos

**Por que `ExecutionOrigin`?**
- Identifica a origem da operação (API, Batch, Worker, CLI, etc.)
- Permite analisar de onde vieram as alterações
- Útil para troubleshooting e auditorias de segurança

---

### 🆕 RegisterNew - Criação de Nova Entidade

Cria um novo registro de auditoria para entidades recém-criadas.

```csharp
var context = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),
    tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Acme Corp"),
    executionUser: "admin@acme.com",
    executionOrigin: "API",
    minimumMessageType: MessageType.Information,
    timeProvider: TimeProvider.System
);

var changeInfo = EntityChangeInfo.RegisterNew(
    executionContext: context,
    createdBy: "admin@acme.com"
);

// changeInfo.CreatedAt              → Timestamp atual via TimeProvider
// changeInfo.CreatedBy              → "admin@acme.com"
// changeInfo.CreatedCorrelationId   → CorrelationId do ExecutionContext
// changeInfo.CreatedExecutionOrigin → "API" (do ExecutionContext)
// changeInfo.LastChangedAt          → null
// changeInfo.LastChangedBy          → null
// changeInfo.LastChangedCorrelationId   → null
// changeInfo.LastChangedExecutionOrigin → null
```

---

### 🔄 RegisterChange - Registro de Modificação

Cria um novo `EntityChangeInfo` preservando dados de criação e atualizando dados de modificação.

```csharp
var originalInfo = EntityChangeInfo.RegisterNew(context, "creator@acme.com");

// Simula passagem de tempo em testes
fakeTimeProvider.Advance(TimeSpan.FromHours(4));

var modifiedInfo = originalInfo.RegisterChange(
    executionContext: context,
    changedBy: "editor@acme.com"
);

// modifiedInfo.CreatedAt                  → Mesmo do original ✨
// modifiedInfo.CreatedBy                  → "creator@acme.com" ✨
// modifiedInfo.CreatedCorrelationId       → Mesmo do original ✨
// modifiedInfo.CreatedExecutionOrigin     → Mesmo do original ✨
// modifiedInfo.LastChangedAt              → 4 horas depois
// modifiedInfo.LastChangedBy              → "editor@acme.com"
// modifiedInfo.LastChangedCorrelationId   → CorrelationId da nova operação
// modifiedInfo.LastChangedExecutionOrigin → ExecutionOrigin da nova operação
```

---

### 📦 CreateFromExistingInfo - Reconstrução de Dados Existentes

Reconstrói um `EntityChangeInfo` a partir de dados já existentes (ex: banco de dados).

```csharp
// Dados vindos do banco de dados
var changeInfo = EntityChangeInfo.CreateFromExistingInfo(
    createdAt: dbRecord.CreatedAt,
    createdBy: dbRecord.CreatedBy,
    createdCorrelationId: dbRecord.CreatedCorrelationId,
    createdExecutionOrigin: dbRecord.CreatedExecutionOrigin,
    lastChangedAt: dbRecord.LastChangedAt,
    lastChangedBy: dbRecord.LastChangedBy,
    lastChangedCorrelationId: dbRecord.LastChangedCorrelationId,
    lastChangedExecutionOrigin: dbRecord.LastChangedExecutionOrigin
);
```

**Quando usar:**
- Mapeamento de DTOs para entidades
- Reconstrução de entidades do banco de dados

---

## 📖 Como Usar

### 1️⃣ Uso Básico - Criação de Entidade

```csharp
// No factory method da entidade
public static Order? RegisterNew(ExecutionContext context, OrderInput input)
{
    var entityInfo = EntityInfo.RegisterNew(
        executionContext: context,
        tenantInfo: context.TenantInfo,
        createdBy: context.ExecutionUser  // ✨ EntityChangeInfo criado internamente
    );

    // entityInfo.EntityChangeInfo contém os dados de auditoria
    return new Order(entityInfo, input);
}
```

**Quando usar:** Sempre que criar uma nova entidade no sistema.

---

### 2️⃣ Uso Intermediário - Modificação de Entidade

```csharp
// No método de alteração da entidade
public Order? UpdateStatus(ExecutionContext context, OrderStatus newStatus)
{
    var clone = this.Clone();

    // Atualiza EntityInfo (que internamente atualiza EntityChangeInfo)
    clone.SetEntityInfo(
        context,
        entityInfo: this.EntityInfo.RegisterChange(
            executionContext: context,
            changedBy: context.ExecutionUser  // ✨ LastChangedBy atualizado
        )
    );

    clone.Status = newStatus;
    return clone;
}
```

**Quando usar:** Qualquer operação que modifique uma entidade existente.

---

### 3️⃣ Uso Avançado - Reconstrução do Banco de Dados

```csharp
// No repositório, ao carregar do banco
public Order? GetById(Id orderId)
{
    var dbRecord = _dbContext.Orders.Find(orderId);
    if (dbRecord == null) return null;

    var changeInfo = EntityChangeInfo.CreateFromExistingInfo(
        createdAt: dbRecord.CreatedAt,
        createdBy: dbRecord.CreatedBy,
        createdCorrelationId: dbRecord.CreatedCorrelationId,
        createdExecutionOrigin: dbRecord.CreatedExecutionOrigin,
        lastChangedAt: dbRecord.LastChangedAt,
        lastChangedBy: dbRecord.LastChangedBy,
        lastChangedCorrelationId: dbRecord.LastChangedCorrelationId,
        lastChangedExecutionOrigin: dbRecord.LastChangedExecutionOrigin
    );

    var entityInfo = EntityInfo.CreateFromExistingInfo(
        id: dbRecord.Id,
        tenantInfo: TenantInfo.Create(dbRecord.TenantId, dbRecord.TenantCode),
        entityChangeInfo: changeInfo,
        entityVersion: dbRecord.Version
    );

    return Order.CreateFromExistingInfo(entityInfo, dbRecord);
}

// Ou usando o overload simplificado do EntityInfo
public Order? GetByIdSimplificado(Id orderId)
{
    var dbRecord = _dbContext.Orders.Find(orderId);
    if (dbRecord == null) return null;

    var entityInfo = EntityInfo.CreateFromExistingInfo(
        id: dbRecord.Id,
        tenantInfo: TenantInfo.Create(dbRecord.TenantId, dbRecord.TenantCode),
        createdAt: dbRecord.CreatedAt,
        createdBy: dbRecord.CreatedBy,
        createdCorrelationId: dbRecord.CreatedCorrelationId,
        createdExecutionOrigin: dbRecord.CreatedExecutionOrigin,
        lastChangedAt: dbRecord.LastChangedAt,
        lastChangedBy: dbRecord.LastChangedBy,
        lastChangedCorrelationId: dbRecord.LastChangedCorrelationId,
        lastChangedExecutionOrigin: dbRecord.LastChangedExecutionOrigin,
        entityVersion: dbRecord.Version
    );

    return Order.CreateFromExistingInfo(entityInfo, dbRecord);
}
```

**Quando usar:** Ao reconstruir entidades de fontes externas (banco, APIs, mensageria).

---

## 🔗 Integração com ExecutionContext

O `EntityChangeInfo` foi projetado para trabalhar nativamente com `ExecutionContext`:

```csharp
// ExecutionContext fornece:
// - TimeProvider para timestamps consistentes
// - ExecutionUser como padrão para CreatedBy/LastChangedBy
// - CorrelationId para rastreabilidade de operações
// - ExecutionOrigin para identificar a origem da operação (API, Batch, Worker, etc.)

var context = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),        // ✨ Usado para rastreabilidade
    tenantInfo: tenant,
    executionUser: "system@batch.com",    // ✨ Usado automaticamente
    executionOrigin: "Batch",             // ✨ Origem da operação
    minimumMessageType: MessageType.Warning,
    timeProvider: TimeProvider.System     // ✨ Fonte de tempo
);

// RegisterNew usa:
// - context.TimeProvider para CreatedAt
// - context.CorrelationId para CreatedCorrelationId
// - context.ExecutionOrigin para CreatedExecutionOrigin
var info = EntityChangeInfo.RegisterNew(context, context.ExecutionUser);

// RegisterChange usa:
// - context.TimeProvider para LastChangedAt
// - context.CorrelationId para LastChangedCorrelationId
// - context.ExecutionOrigin para LastChangedExecutionOrigin
// - Preserva CreatedAt, CreatedBy, CreatedCorrelationId e CreatedExecutionOrigin
var modified = info.RegisterChange(context, context.ExecutionUser);
```

---

## ⚖️ Trade-offs

### Benefícios

| Benefício | Impacto | Análise |
|-----------|---------|---------|
| **Imutabilidade** | ✅ Alto | Impossível adulterar registros de auditoria |
| **Testabilidade** | ✅ Alto | TimeProvider permite testes determinísticos |
| **Consistência** | ✅ Alto | Nomenclatura padronizada em todo o sistema |
| **Semântica clara** | ✅ Médio | Separação explícita criação vs modificação |
| **Rastreabilidade** | ✅ Alto | CorrelationId permite vincular entidades a operações específicas |

### Custos

| Custo | Impacto | Mitigação |
|-------|---------|-----------|
| **Alocação em modificações** | ⚠️ Baixo | `readonly record struct` é stack-allocated na maioria dos casos |
| **Verbosidade** | ⚠️ Baixo | `EntityInfo` encapsula, uso direto é raro |

### Quando Usar vs Quando Evitar

#### ✅ Use quando:
1. Entidades de domínio precisam de auditoria
2. Compliance requer rastreabilidade (LGPD, SOX)
3. Sistema precisa de testes com timestamps controlados
4. Múltiplos usuários modificam os mesmos registros

#### ❌ Evite quando:
1. Objetos de transferência simples (DTOs) sem necessidade de auditoria
2. Dados efêmeros (cache, sessão)
3. Value objects que não precisam de rastreamento

---

## 🔬 Exemplos Avançados

### 🔍 Auditoria Completa em Relatórios

```csharp
public class AuditReportService
{
    public AuditReport GenerateReport(IEnumerable<EntityBase> entities)
    {
        var entries = entities.Select(e => new AuditEntry
        {
            EntityId = e.EntityInfo.Id,
            EntityType = e.GetType().Name,
            CreatedAt = e.EntityInfo.EntityChangeInfo.CreatedAt,
            CreatedBy = e.EntityInfo.EntityChangeInfo.CreatedBy,
            CreatedCorrelationId = e.EntityInfo.EntityChangeInfo.CreatedCorrelationId,
            CreatedExecutionOrigin = e.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin,
            LastChangedAt = e.EntityInfo.EntityChangeInfo.LastChangedAt,
            LastChangedBy = e.EntityInfo.EntityChangeInfo.LastChangedBy,
            LastChangedCorrelationId = e.EntityInfo.EntityChangeInfo.LastChangedCorrelationId,
            LastChangedExecutionOrigin = e.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin,
            WasEverModified = e.EntityInfo.EntityChangeInfo.LastChangedAt.HasValue,
            DaysSinceCreation = (DateTimeOffset.UtcNow - e.EntityInfo.EntityChangeInfo.CreatedAt).Days
        });

        return new AuditReport
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            TotalEntities = entries.Count(),
            NeverModified = entries.Count(e => !e.WasEverModified),
            Entries = entries.ToList()
        };
    }

    // Busca todas as entidades criadas em uma operação específica
    public IEnumerable<EntityBase> GetEntitiesByCorrelationId(
        IEnumerable<EntityBase> entities,
        Guid correlationId
    )
    {
        return entities.Where(e =>
            e.EntityInfo.EntityChangeInfo.CreatedCorrelationId == correlationId ||
            e.EntityInfo.EntityChangeInfo.LastChangedCorrelationId == correlationId
        );
    }

    // Busca todas as entidades criadas por uma origem específica
    public IEnumerable<EntityBase> GetEntitiesByExecutionOrigin(
        IEnumerable<EntityBase> entities,
        string executionOrigin
    )
    {
        return entities.Where(e =>
            e.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin == executionOrigin ||
            e.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin == executionOrigin
        );
    }
}
```

---

### 🧪 Testes com Tempo Controlado

```csharp
[Fact]
public void RegisterChange_ShouldPreserveCreationData_AndUpdateChangeData()
{
    // Arrange
    var creationTime = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
    var modificationTime = new DateTimeOffset(2024, 1, 15, 14, 30, 0, TimeSpan.Zero);
    var creationCorrelationId = Guid.NewGuid();
    var modificationCorrelationId = Guid.NewGuid();

    var fakeTime = new FakeTimeProvider(creationTime);
    var creationContext = ExecutionContext.Create(
        correlationId: creationCorrelationId,
        tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test"),
        executionUser: "creator@test.com",
        executionOrigin: "API",
        minimumMessageType: MessageType.Information,
        timeProvider: fakeTime
    );

    var original = EntityChangeInfo.RegisterNew(creationContext, "creator@test.com");

    // Act
    fakeTime.SetUtcNow(modificationTime);
    var modificationContext = ExecutionContext.Create(
        correlationId: modificationCorrelationId,
        tenantInfo: creationContext.TenantInfo,
        executionUser: "editor@test.com",
        executionOrigin: "Batch",
        minimumMessageType: MessageType.Information,
        timeProvider: fakeTime
    );
    var modified = original.RegisterChange(modificationContext, "editor@test.com");

    // Assert - Dados de criação preservados
    Assert.Equal(creationTime, modified.CreatedAt);
    Assert.Equal("creator@test.com", modified.CreatedBy);
    Assert.Equal(creationCorrelationId, modified.CreatedCorrelationId);
    Assert.Equal("API", modified.CreatedExecutionOrigin);

    // Assert - Dados de modificação atualizados
    Assert.Equal(modificationTime, modified.LastChangedAt);
    Assert.Equal("editor@test.com", modified.LastChangedBy);
    Assert.Equal(modificationCorrelationId, modified.LastChangedCorrelationId);
    Assert.Equal("Batch", modified.LastChangedExecutionOrigin);
}
```

---

## 📚 Referências

- [EntityInfo](entity-info.md) - Estrutura que contém EntityChangeInfo
- [EntityBase](../entity-base.md) - Classe base que usa EntityInfo
- [ExecutionContext](../../core/execution-contexts/execution-context.md) - Contexto de execução com TimeProvider
- [TimeProvider (Microsoft Docs)](https://learn.microsoft.com/en-us/dotnet/api/system.timeprovider) - Abstração de tempo do .NET 8+
