# 🏛️ EntityBase - Classe Base para Entidades de Domínio

O `EntityBase` é a classe abstrata fundamental que todas as entidades de domínio devem herdar, fornecendo infraestrutura padronizada para identificação, auditoria, multi-tenancy e validação.

> 💡 **Visão Geral:** Herde de `EntityBase<T>` para obter automaticamente **EntityInfo** (Id, Tenant, Auditoria, Versão), **validação de metadados**, **padrão Clone-Modify-Return** e **factory methods** seguros.

---

## 📋 Sumário

- [Por Que Usar EntityBase](#-por-que-usar-entitybase)
- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
- [Arquitetura](#-arquitetura)
- [Funcionalidades](#-funcionalidades)
- [Como Usar](#-como-usar)
- [Metadata e Validação](#-metadata-e-validação)
- [Trade-offs](#️-trade-offs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)

---

## 🎯 Por Que Usar EntityBase?

| Característica | Entidade Manual | **EntityBase** | Active Record |
|----------------|-----------------|----------------|---------------|
| **Metadados padronizados** | ❌ Copy-paste | ✅ **Herdados** | ⚠️ Acoplados a ORM |
| **Imutabilidade** | ❌ Difícil | ✅ **Clone-Modify-Return** | ❌ Mutável |
| **Validação integrada** | ❌ Ad-hoc | ✅ **ValidationUtils** | ⚠️ Annotations |
| **Factory methods** | ❌ Manual | ✅ **RegisterNew/Change** | ❌ Construtor público |
| **Multi-tenancy** | ❌ Manual | ✅ **TenantInfo automático** | ❌ Não suportado |
| **Testabilidade** | ❌ Dependências ocultas | ✅ **ExecutionContext** | ❌ Static dependencies |

---

## 🎯 Contexto: Por Que Existe

### O Problema Real

Ao desenvolver sistemas com DDD, cada entidade precisa de infraestrutura comum: ID, auditoria, validação. Sem uma classe base bem projetada, desenvolvedores repetem código e introduzem inconsistências.

**Exemplo de abordagens problemáticas:**

```csharp
❌ Entidade sem padrão definido:
public class Order
{
    public Guid Id { get; set; }           // ⚠️ Público e mutável
    public Guid TenantId { get; set; }     // ⚠️ Quem valida?

    public Order() { }                      // ⚠️ Construtor público
    public Order(Guid id) { Id = id; }     // ⚠️ Múltiplos construtores

    public void UpdateStatus(Status s)      // ⚠️ Mutação direta
    {
        Status = s;
        ModifiedAt = DateTime.Now;          // ⚠️ Esqueceu ModifiedBy!
    }
}

❌ Problemas:
- Construtor público permite estado inválido
- Mutação direta sem auditoria consistente
- Sem validação de metadados
- Código de infraestrutura repetido em cada entidade
```

### A Solução

```csharp
✅ Abordagem com EntityBase:
public sealed class Order : EntityBase<Order>
{
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }

    private Order(EntityInfo entityInfo, OrderStatus status, Money total)
        : base(entityInfo)
    {
        Status = status;
        Total = total;
    }

    public static Order? RegisterNew(ExecutionContext context, OrderInput input)
    {
        return RegisterNewInternal<Order, OrderInput>(
            context,
            input,
            entityFactory: (ctx, inp) => new Order(
                entityInfo: default,
                status: OrderStatus.Pending,
                total: inp.Total
            ),
            handler: (ctx, inp, entity) => entity.IsValid(ctx)
        );
    }

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
    }

    public override IEntity<Order> Clone() => new Order(EntityInfo, Status, Total);
}

✅ Benefícios:
- Construtor privado: criação apenas via factory methods
- EntityInfo gerenciado automaticamente
- Clone-Modify-Return garante imutabilidade
- Auditoria atualizada em cada modificação
- Validação integrada
```

---

## 🔧 Problemas Resolvidos

### 1. 🔐 Criação Controlada de Entidades

**Problema:** Construtores públicos permitem criar entidades em estado inválido.

#### 📚 Analogia: Certidão de Nascimento

Você não pode simplesmente escrever uma certidão de nascimento em casa — ela precisa ser emitida por um cartório oficial que valida os dados e aplica o carimbo. O `EntityBase` funciona como esse cartório: entidades só são criadas via `RegisterNew`, que valida dados e aplica metadados oficiais.

#### 💻 Impacto Real no Código

```csharp
❌ Antes - Construtor público:
var order = new Order();           // ⚠️ Sem Id!
order.Id = Guid.Empty;             // ⚠️ Id inválido
order.TenantId = Guid.Empty;       // ⚠️ Sem tenant
// Entidade em estado inconsistente salva no banco...

✅ Depois - Factory method:
// new Order() não é acessível (construtor privado)
var order = Order.RegisterNew(context, input);

if (order == null)
{
    // Validação falhou, erros no context
    foreach (var msg in context.Messages)
        Console.WriteLine(msg.Text);
}

// Entidade válida ou null com erros explicados
```

---

### 2. 🔄 Imutabilidade via Clone-Modify-Return

**Problema:** Mutação direta dificulta rastreamento de mudanças e causa bugs sutis.

#### 📚 Analogia: Contrato com Aditivo

Quando você precisa alterar um contrato, não rabisca o original — você cria um **aditivo** (novo documento) referenciando o original. O padrão Clone-Modify-Return faz o mesmo: cria uma **cópia** da entidade, modifica a cópia, e retorna a nova versão. O original permanece intacto.

#### 💻 Impacto Real no Código

```csharp
❌ Antes - Mutação direta:
public void UpdateStatus(OrderStatus newStatus)
{
    this.Status = newStatus;  // ⚠️ Objeto original modificado
    this.ModifiedAt = DateTime.Now;
}

// Problemas:
var order = GetOrder();
order.UpdateStatus(OrderStatus.Shipped);
// Se algo falhar depois, order já está modificado!
// Difícil reverter, difícil rastrear

✅ Depois - Clone-Modify-Return:
public Order? UpdateStatus(ExecutionContext context, OrderStatus newStatus)
{
    return RegisterChangeInternal<Order, OrderStatus>(
        context,
        instance: this,            // ✨ Original
        input: newStatus,
        handler: (ctx, status, entity) =>
        {
            entity.Status = status;  // ✨ Modifica o clone
            return true;
        }
    );
}

// Uso:
var order = GetOrder();
var updatedOrder = order.UpdateStatus(context, OrderStatus.Shipped);

// order → Original intacto
// updatedOrder → Nova versão (ou null se falhou)
// Fácil comparar, reverter, auditar
```

---

### 3. 📋 Validação Padronizada de Metadados

**Problema:** Cada entidade valida seus metadados de forma diferente (ou não valida).

#### 📚 Analogia: Checklist de Voo

Antes de decolar, pilotos seguem um checklist padronizado — não inventam itens na hora. O `EntityBase.ValidateEntityInfo()` é esse checklist: valida Id, TenantInfo, CreatedAt, CreatedBy, etc. de forma consistente em **todas** as entidades.

#### 💻 Impacto Real no Código

```csharp
// EntityBase já valida automaticamente:
public static bool ValidateEntityInfo(ExecutionContext context, EntityInfo entityInfo)
{
    // ✅ Id é obrigatório?
    bool idIsValid = ValidationUtils.ValidateIsRequired(
        context,
        propertyName: "EntityBase.Id",
        isRequired: EntityBaseMetadata.IdIsRequired,
        entityInfo.Id
    );

    // ✅ TenantCode é obrigatório?
    bool tenantIsValid = ValidationUtils.ValidateIsRequired(
        context,
        propertyName: "EntityBase.EntityInfo.TenantInfo.Code",
        isRequired: EntityBaseMetadata.TenantCodeIsRequired,
        entityInfo.TenantInfo.Code
    );

    // ✅ CreatedBy tem tamanho válido?
    bool createdByLengthIsValid = ValidationUtils.ValidateMaxLength(
        context,
        propertyName: "EntityBase.EntityInfo.EntityChangeInfo.CreatedBy",
        maxLength: EntityBaseMetadata.CreatedByMaxLength,
        entityInfo.EntityChangeInfo.CreatedBy?.Length ?? 0
    );

    // ... todas as validações de metadados
    return idIsValid && tenantIsValid && createdByLengthIsValid /* && ... */;
}
```

---

## 🏗️ Arquitetura

### Hierarquia de Classes

```
EntityBase (abstract)
    │
    ├── EntityInfo EntityInfo { get; }     ← Metadados consolidados
    │
    ├── ValidateEntityInfo()               ← Validação de metadados
    ├── SetEntityInfo()                    ← Atualização controlada
    ├── IsValidInternal()                  ← Gancho para subclasses
    └── CreateMessageCode<T>()             ← Códigos de erro padronizados
         │
         └── EntityBase<TEntity> : EntityBase (abstract generic)
                  │
                  ├── Clone()                      ← Clonagem (abstract)
                  ├── RegisterNewInternal<>()      ← Factory para criação
                  └── RegisterChangeInternal<>()   ← Factory para modificação
```

### Fluxo de Criação (RegisterNew)

```
┌─────────────────────────────────────────────────────────────────┐
│                    RegisterNewInternal                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. entityFactory(context, input)                               │
│     └── Cria instância com dados de negócio                     │
│                                                                 │
│  2. EntityInfo.RegisterNew(context, tenant, user)               │
│     └── Gera Id, cria EntityChangeInfo, EntityVersion           │
│                                                                 │
│  3. entity.SetEntityInfo(context, entityInfo)                   │
│     └── Valida e atribui EntityInfo                             │
│                                                                 │
│  4. handler(context, input, entity)                             │
│     └── Validações adicionais de negócio                        │
│                                                                 │
│  5. return isSuccess ? entity : null                            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Fluxo de Modificação (RegisterChange)

```
┌─────────────────────────────────────────────────────────────────┐
│                    RegisterChangeInternal                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. instance.Clone()                                            │
│     └── Cria cópia da entidade original                         │
│                                                                 │
│  2. EntityInfo.RegisterChange(context, user)                    │
│     └── Preserva criação, atualiza LastChanged e Version        │
│                                                                 │
│  3. newInstance.SetEntityInfo(context, newEntityInfo)           │
│     └── Valida e atribui novo EntityInfo                        │
│                                                                 │
│  4. handler(context, input, newInstance)                        │
│     └── Aplica modificações de negócio no clone                 │
│                                                                 │
│  5. return isSuccess ? newInstance : null                       │
│     └── Original intacto, retorna clone modificado              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## ✨ Funcionalidades

### 🔧 EntityBaseMetadata - Configuração de Validação

Classe interna que permite configurar regras de validação globalmente.

```csharp
public static class EntityBaseMetadata
{
    // Configurações de Id
    public static string IdPropertyName { get; } = "Id";
    public static bool IdIsRequired { get; private set; } = true;

    // Configurações de TenantCode
    public static string TenantCodePropertyName { get; }
    public static bool TenantCodeIsRequired { get; private set; } = true;

    // Configurações de CreatedBy
    public static bool CreatedByIsRequired { get; private set; } = true;
    public static int CreatedByMinLength { get; private set; } = 1;
    public static int CreatedByMaxLength { get; private set; } = 255;

    // ... outras configurações

    // Métodos para customização
    public static void ChangeIdMetadata(bool isRequired);
    public static void ChangeTenantCodeMetadata(bool isRequired);
    public static void ChangeCreationInfoMetadata(...);
    public static void ChangeUpdateInfoMetadata(...);
    public static void ChangeEntityVersionMetadata(bool isRequired);
}
```

**Exemplo de customização:**

```csharp
// Em startup ou configuração do módulo
EntityBase.EntityBaseMetadata.ChangeCreationInfoMetadata(
    createdAtIsRequired: true,
    createdByIsRequired: true,
    createdByMinLength: 3,      // Mínimo 3 caracteres
    createdByMaxLength: 100     // Máximo 100 caracteres
);
```

---

### 🆕 RegisterNewInternal - Factory para Criação

Template method que padroniza a criação de entidades.

```csharp
protected static TEntityBase? RegisterNewInternal<TEntityBase, TInput>(
    ExecutionContext executionContext,
    TInput input,
    Func<ExecutionContext, TInput, TEntityBase> entityFactory,
    Func<ExecutionContext, TInput, TEntityBase, bool> handler
) where TEntityBase : EntityBase<TEntity>
```

**Parâmetros:**
- `executionContext` → Contexto com TimeProvider, TenantInfo, ExecutionUser
- `input` → Dados de entrada para criação
- `entityFactory` → Função que cria a instância com dados de negócio
- `handler` → Função de validação/processamento adicional

**Retorno:** Entidade válida ou `null` (erros no ExecutionContext)

---

### 🔄 RegisterChangeInternal - Factory para Modificação

Template method que padroniza modificações com Clone-Modify-Return.

```csharp
protected static TEntityBase? RegisterChangeInternal<TEntityBase, TInput>(
    ExecutionContext executionContext,
    EntityBase<TEntity> instance,
    TInput input,
    Func<ExecutionContext, TInput, TEntityBase, bool> handler
) where TEntityBase : EntityBase<TEntity>
```

**Parâmetros:**
- `executionContext` → Contexto de execução
- `instance` → Entidade original a ser modificada
- `input` → Dados da modificação
- `handler` → Função que aplica a modificação no clone

**Retorno:** Clone modificado ou `null` (erros no ExecutionContext)

---

### 🧬 Clone - Clonagem Obrigatória

Método abstrato que cada entidade deve implementar.

```csharp
public abstract IEntity<TEntity> Clone();

// Implementação típica:
public override IEntity<Order> Clone()
{
    return new Order(
        entityInfo: EntityInfo,
        status: Status,
        total: Total,
        items: Items.ToList()  // Cópia profunda de coleções!
    );
}
```

**Importante:** Para coleções, sempre faça cópia profunda!

---

## 📖 Como Usar

### 1️⃣ Uso Básico - Entidade Simples

```csharp
public sealed class Customer : EntityBase<Customer>
{
    // Propriedades de negócio
    public string Name { get; private set; }
    public Email Email { get; private set; }

    // Construtor privado
    private Customer(EntityInfo entityInfo, string name, Email email)
        : base(entityInfo)
    {
        Name = name;
        Email = email;
    }

    // Factory method para criação
    public static Customer? RegisterNew(ExecutionContext context, CustomerInput input)
    {
        return RegisterNewInternal<Customer, CustomerInput>(
            context,
            input,
            entityFactory: (ctx, inp) => new Customer(
                entityInfo: default,
                name: inp.Name,
                email: inp.Email
            ),
            handler: (ctx, inp, entity) =>
            {
                // Validações de negócio
                if (string.IsNullOrWhiteSpace(entity.Name))
                {
                    ctx.AddErrorMessage("CUSTOMER_NAME_REQUIRED", "Nome é obrigatório");
                    return false;
                }
                return true;
            }
        );
    }

    // Clonagem
    public override IEntity<Customer> Clone()
    {
        return new Customer(EntityInfo, Name, Email);
    }
}
```

---

### 2️⃣ Uso Intermediário - Com Modificações

```csharp
public sealed class Order : EntityBase<Order>
{
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderItem> Items { get; private set; }
    public Money Total { get; private set; }

    private Order(EntityInfo entityInfo, OrderStatus status, List<OrderItem> items, Money total)
        : base(entityInfo)
    {
        Status = status;
        Items = items.AsReadOnly();
        Total = total;
    }

    public static Order? RegisterNew(ExecutionContext context, OrderInput input)
    {
        return RegisterNewInternal<Order, OrderInput>(
            context,
            input,
            entityFactory: (ctx, inp) => new Order(
                entityInfo: default,
                status: OrderStatus.Pending,
                items: inp.Items.ToList(),
                total: CalculateTotal(inp.Items)
            ),
            handler: (ctx, inp, entity) =>
            {
                if (!entity.Items.Any())
                {
                    ctx.AddErrorMessage("ORDER_EMPTY", "Pedido deve ter pelo menos um item");
                    return false;
                }
                return true;
            }
        );
    }

    // Método de modificação
    public Order? Approve(ExecutionContext context)
    {
        if (Status != OrderStatus.Pending)
        {
            context.AddErrorMessage("ORDER_INVALID_STATUS", "Apenas pedidos pendentes podem ser aprovados");
            return null;
        }

        return RegisterChangeInternal<Order, OrderStatus>(
            context,
            instance: this,
            input: OrderStatus.Approved,
            handler: (ctx, newStatus, entity) =>
            {
                entity.Status = newStatus;
                return true;
            }
        );
    }

    public override IEntity<Order> Clone()
    {
        return new Order(EntityInfo, Status, Items.ToList(), Total);
    }
}
```

---

### 3️⃣ Uso Avançado - Com Agregados

```csharp
public sealed class Invoice : EntityBase<Invoice>
{
    public Customer Customer { get; private set; }
    public IReadOnlyList<InvoiceItem> Items { get; private set; }
    public InvoiceStatus Status { get; private set; }

    private Invoice(
        EntityInfo entityInfo,
        Customer customer,
        List<InvoiceItem> items,
        InvoiceStatus status
    ) : base(entityInfo)
    {
        Customer = customer;
        Items = items.AsReadOnly();
        Status = status;
    }

    public static Invoice? RegisterNew(
        ExecutionContext context,
        Customer customer,
        List<InvoiceItem> items
    )
    {
        return RegisterNewInternal<Invoice, (Customer, List<InvoiceItem>)>(
            context,
            input: (customer, items),
            entityFactory: (ctx, inp) => new Invoice(
                entityInfo: default,
                customer: inp.Item1,
                items: inp.Item2,
                status: InvoiceStatus.Draft
            ),
            handler: (ctx, inp, entity) =>
            {
                // Validação do agregado
                if (inp.Item1.EntityInfo.TenantInfo.TenantId != ctx.TenantInfo.TenantId)
                {
                    ctx.AddErrorMessage("INVOICE_TENANT_MISMATCH", "Cliente pertence a outro tenant");
                    return false;
                }
                return true;
            }
        );
    }

    // Adicionar item (retorna nova Invoice)
    public Invoice? AddItem(ExecutionContext context, InvoiceItem newItem)
    {
        return RegisterChangeInternal<Invoice, InvoiceItem>(
            context,
            instance: this,
            input: newItem,
            handler: (ctx, item, entity) =>
            {
                if (entity.Status != InvoiceStatus.Draft)
                {
                    ctx.AddErrorMessage("INVOICE_NOT_DRAFT", "Só pode adicionar itens em rascunho");
                    return false;
                }

                var updatedItems = entity.Items.ToList();
                updatedItems.Add(item);
                entity.Items = updatedItems.AsReadOnly();
                return true;
            }
        );
    }

    public override IEntity<Invoice> Clone()
    {
        return new Invoice(EntityInfo, Customer, Items.ToList(), Status);
    }
}
```

---

## 📊 Metadata e Validação

### Códigos de Erro Padronizados

O `EntityBase` gera códigos de erro consistentes:

```csharp
// Formato: {TipoEntidade}.{Propriedade}.{TipoValidacao}

// Exemplos de códigos gerados:
"EntityBase.Id.IsRequired"
"EntityBase.EntityInfo.TenantInfo.Code.IsRequired"
"EntityBase.EntityInfo.EntityChangeInfo.CreatedBy.MaxLength"
"Order.Status.IsRequired"
"Order.Items.MinLength"
```

### Customização de Validações

```csharp
// Para sistemas single-tenant
EntityBase.EntityBaseMetadata.ChangeTenantCodeMetadata(isRequired: false);

// Para sistemas sem auditoria de modificação
EntityBase.EntityBaseMetadata.ChangeUpdateInfoMetadata(
    lastChangedAtIsRequired: false,
    lastChangedByIsRequired: false,
    lastChangedByMinLength: 0,
    lastChangedByMaxLength: 255
);
```

---

## ⚖️ Trade-offs

### Benefícios

| Benefício | Impacto | Análise |
|-----------|---------|---------|
| **Padronização** | ✅ Alto | Todas as entidades seguem o mesmo padrão |
| **Imutabilidade** | ✅ Alto | Clone-Modify-Return previne bugs |
| **Validação integrada** | ✅ Alto | Erros consistentes via ExecutionContext |
| **Testabilidade** | ✅ Alto | Sem dependências ocultas |
| **Auditoria automática** | ✅ Alto | EntityInfo gerenciado automaticamente |

### Custos

| Custo | Impacto | Mitigação |
|-------|---------|-----------|
| **Verbosidade** | ⚠️ Médio | Templates de código, snippets de IDE |
| **Curva de aprendizado** | ⚠️ Médio | Padrão consistente, aprende uma vez |
| **Clone manual** | ⚠️ Baixo | Implementação simples e mecânica |

### Quando Usar vs Quando Evitar

#### ✅ Use quando:
1. Entidades de domínio com comportamento
2. Sistemas que precisam de auditoria
3. Arquitetura DDD/Clean Architecture
4. Controle de concorrência otimista necessário

#### ❌ Evite quando:
1. DTOs e modelos de transferência simples
2. Value objects (use `readonly record struct`)
3. Entidades anêmicas sem comportamento
4. Protótipos rápidos sem requisitos de auditoria

---

## 🔬 Exemplos Avançados

### 🏭 Aggregate Root com Invariantes

```csharp
public sealed class ShoppingCart : EntityBase<ShoppingCart>
{
    private const int MaxItems = 50;
    private const decimal MaxTotal = 10_000m;

    public IReadOnlyList<CartItem> Items { get; private set; }
    public Money Total => Items.Sum(i => i.Subtotal);

    private ShoppingCart(EntityInfo entityInfo, List<CartItem> items)
        : base(entityInfo)
    {
        Items = items.AsReadOnly();
    }

    public ShoppingCart? AddItem(ExecutionContext context, Product product, int quantity)
    {
        // Invariante: máximo de itens
        if (Items.Count >= MaxItems)
        {
            context.AddErrorMessage("CART_MAX_ITEMS", $"Carrinho não pode ter mais de {MaxItems} itens");
            return null;
        }

        return RegisterChangeInternal<ShoppingCart, (Product, int)>(
            context,
            instance: this,
            input: (product, quantity),
            handler: (ctx, inp, entity) =>
            {
                var newItems = entity.Items.ToList();
                var existingItem = newItems.FirstOrDefault(i => i.ProductId == inp.Item1.EntityInfo.Id);

                if (existingItem != null)
                {
                    var index = newItems.IndexOf(existingItem);
                    newItems[index] = existingItem.WithQuantity(existingItem.Quantity + inp.Item2);
                }
                else
                {
                    newItems.Add(CartItem.Create(inp.Item1, inp.Item2));
                }

                entity.Items = newItems.AsReadOnly();

                // Invariante: valor máximo
                if (entity.Total.Amount > MaxTotal)
                {
                    ctx.AddErrorMessage("CART_MAX_TOTAL", $"Valor máximo do carrinho é {MaxTotal:C}");
                    return false;
                }

                return true;
            }
        );
    }

    public override IEntity<ShoppingCart> Clone()
    {
        return new ShoppingCart(EntityInfo, Items.ToList());
    }
}
```

---

### 🔄 State Machine com Entidade

```csharp
public sealed class OrderStateMachine : EntityBase<OrderStateMachine>
{
    public OrderState State { get; private set; }
    public IReadOnlyList<StateTransition> History { get; private set; }

    private OrderStateMachine(
        EntityInfo entityInfo,
        OrderState state,
        List<StateTransition> history
    ) : base(entityInfo)
    {
        State = state;
        History = history.AsReadOnly();
    }

    public OrderStateMachine? TransitionTo(ExecutionContext context, OrderState newState)
    {
        if (!IsValidTransition(State, newState))
        {
            context.AddErrorMessage(
                "ORDER_INVALID_TRANSITION",
                $"Transição de {State} para {newState} não permitida"
            );
            return null;
        }

        return RegisterChangeInternal<OrderStateMachine, OrderState>(
            context,
            instance: this,
            input: newState,
            handler: (ctx, state, entity) =>
            {
                var transition = new StateTransition(
                    from: entity.State,
                    to: state,
                    at: ctx.TimeProvider.GetUtcNow(),
                    by: ctx.ExecutionUser
                );

                var updatedHistory = entity.History.ToList();
                updatedHistory.Add(transition);

                entity.State = state;
                entity.History = updatedHistory.AsReadOnly();

                ctx.AddSuccessMessage(
                    "ORDER_STATE_CHANGED",
                    $"Pedido transicionou de {transition.From} para {transition.To}"
                );

                return true;
            }
        );
    }

    private static bool IsValidTransition(OrderState from, OrderState to)
    {
        return (from, to) switch
        {
            (OrderState.Pending, OrderState.Confirmed) => true,
            (OrderState.Confirmed, OrderState.Shipped) => true,
            (OrderState.Shipped, OrderState.Delivered) => true,
            (OrderState.Pending, OrderState.Cancelled) => true,
            (OrderState.Confirmed, OrderState.Cancelled) => true,
            _ => false
        };
    }

    public override IEntity<OrderStateMachine> Clone()
    {
        return new OrderStateMachine(EntityInfo, State, History.ToList());
    }
}
```

---

## 📚 Referências

- [EntityInfo](models/entity-info.md) - Metadados consolidados de entidade
- [EntityChangeInfo](models/entity-change-info.md) - Dados de auditoria
- [ValidationUtils](../core/validations/validation-utils.md) - Utilitários de validação
- [ExecutionContext](../core/execution-contexts/execution-context.md) - Contexto de execução
- [Id](../core/ids/id.md) - Gerador de IDs UUIDv7
- [TenantInfo](../core/tenant-infos/tenant-info.md) - Identificador de tenant
- [RegistryVersion](../core/registry-versions/registry-version.md) - Versionamento monotônico

### Leitura Recomendada

- [Domain-Driven Design (Eric Evans)](https://www.domainlanguage.com/ddd/) - Conceitos de Entities e Aggregates
- [Implementing Domain-Driven Design (Vaughn Vernon)](https://vaughnvernon.com/) - Implementação prática
- [Clean Architecture (Robert C. Martin)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Separação de concerns
