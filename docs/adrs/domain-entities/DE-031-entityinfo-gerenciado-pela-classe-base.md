# DE-031: EntityInfo Gerenciado pela Classe Base

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma **biblioteca** com dois sistemas de catalogação:

**Modelo "catalogação individual" (cada livro se cataloga)**:
- Cada livro decide seu próprio número de tombo
- Cada livro registra quando foi adquirido
- Cada livro mantém histórico de empréstimos
- Inconsistências inevitáveis: formatos diferentes, dados faltantes

**Modelo "catalogação central" (bibliotecário gerencia)**:
- Bibliotecário atribui número de tombo no registro
- Sistema central registra data de aquisição
- Histórico de empréstimos gerenciado pelo sistema
- Consistência garantida: mesmo formato, dados completos

O `EntityInfo` gerenciado pela classe base é como a catalogação central - metadados de infraestrutura (ID, versão, auditoria) são responsabilidade do sistema, não da entidade específica.

---

### O Problema Técnico

Se cada entidade gerenciar seus próprios metadados de infraestrutura:

```csharp
// ❌ ANTIPATTERN: Entidade gerencia próprios metadados
public sealed class Order : EntityBase<Order>
{
    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }
    public string? ModifiedBy { get; private set; }
    public int Version { get; private set; }

    public static Order? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        var order = new Order();

        // Cada entidade precisa lembrar de:
        order.Id = Guid.NewGuid();
        order.CreatedAt = ctx.TimeProvider.GetUtcNow();
        order.CreatedBy = ctx.ExecutionUser;
        order.Version = 1;

        // E ainda fazer a lógica de negócio...
        order.CustomerName = input.CustomerName;

        return order;
    }

    public Order? UpdateCustomer(ExecutionContext ctx, string newCustomerName)
    {
        var clone = this.Clone();

        // Lembrar de atualizar metadados
        clone.ModifiedAt = ctx.TimeProvider.GetUtcNow();
        clone.ModifiedBy = ctx.ExecutionUser;
        clone.Version++;  // Fácil esquecer!

        clone.CustomerName = newCustomerName;

        return clone;
    }
}
```

**Problemas graves**:

1. **Código repetitivo**: Toda entidade repete a mesma lógica de metadados
   ```csharp
   // Em Order
   order.CreatedAt = ctx.TimeProvider.GetUtcNow();
   order.CreatedBy = ctx.ExecutionUser;

   // Em Customer
   customer.CreatedAt = ctx.TimeProvider.GetUtcNow();
   customer.CreatedBy = ctx.ExecutionUser;

   // Em Product... mesma coisa
   ```

2. **Fácil esquecer**: Desenvolvedor pode esquecer de incrementar versão
   ```csharp
   public Order? UpdateShipping(ExecutionContext ctx, Address newAddress)
   {
       var clone = this.Clone();
       clone.ShippingAddress = newAddress;
       // Oops! Esqueceu de atualizar Version, ModifiedAt, ModifiedBy
       return clone;
   }
   ```

3. **Inconsistência**: Cada entidade pode usar formatos diferentes
   ```csharp
   // Order usa Guid
   public Guid Id { get; }

   // Customer usa string
   public string CustomerId { get; }

   // Product usa int
   public int ProductId { get; }
   ```

4. **Auditoria incompleta**: Fácil ter entidades sem dados de auditoria
   ```csharp
   // Desenvolvedor novo não sabe que precisa desses campos
   public sealed class NewEntity : EntityBase<NewEntity>
   {
       // Cadê CreatedAt, CreatedBy, Version?
   }
   ```

---

### Como Normalmente é Feito (e Por Que Não é Ideal)

**Opção 1: Propriedades abstratas na classe base**
```csharp
// ⚠️ Propriedades abstratas forçam implementação, mas não garantem uso correto
public abstract class EntityBase
{
    public abstract Guid Id { get; protected set; }
    public abstract DateTimeOffset CreatedAt { get; protected set; }
    public abstract string CreatedBy { get; protected set; }
}

public sealed class Order : EntityBase
{
    public override Guid Id { get; protected set; }
    public override DateTimeOffset CreatedAt { get; protected set; }
    public override string CreatedBy { get; protected set; }

    // Desenvolvedor ainda precisa lembrar de popular os valores
}
```

**Problemas**:
- Implementação obrigatória, mas lógica de atribuição ainda é manual
- Cada entidade pode implementar de forma diferente

**Opção 2: Interceptors/AOP**
```csharp
// ⚠️ Metadados gerenciados por interceptor externo
[Auditable]
public sealed class Order : EntityBase
{
    // Interceptor magicamente popula CreatedAt, etc.
}
```

**Problemas**:
- Mágica - difícil entender o que acontece
- Dependência de framework de AOP
- Difícil testar

## A Decisão

### Nossa Abordagem

O `EntityInfo` é um **Value Object imutável** gerenciado pela **classe base** (`EntityBase`):

```csharp
// EntityInfo - Value Object com todos os metadados
public readonly record struct EntityInfo
{
    public Id Id { get; }                           // Identificador único
    public TenantInfo TenantInfo { get; }           // Multitenancy
    public EntityChangeInfo EntityChangeInfo { get; } // Auditoria (Created/Modified)
    public RegistryVersion EntityVersion { get; }   // Versão para optimistic locking
}

// EntityChangeInfo - Detalhes de auditoria
public readonly record struct EntityChangeInfo
{
    public DateTimeOffset CreatedAt { get; }
    public string CreatedBy { get; }
    public DateTimeOffset? LastChangedAt { get; }
    public string? LastChangedBy { get; }
}
```

### Classe Base Gerencia EntityInfo

```csharp
public abstract class EntityBase
{
    // EntityInfo gerenciado pela classe base
    public EntityInfo EntityInfo { get; private set; }

    protected EntityBase() { }

    protected EntityBase(EntityInfo entityInfo)
    {
        EntityInfo = entityInfo;
    }

    // Método protegido para atualizar EntityInfo (validado)
    protected internal bool SetEntityInfo(
        ExecutionContext executionContext,
        EntityInfo entityInfo
    )
    {
        bool isValid = ValidateEntityInfo(executionContext, entityInfo);

        if (!isValid)
            return false;

        EntityInfo = entityInfo;
        return true;
    }
}
```

### RegisterNewInternal e RegisterChangeInternal Gerenciam Automaticamente

```csharp
public abstract class EntityBase<TEntityBase> : EntityBase
{
    protected static TEntityBase? RegisterNewInternal<TInput>(
        ExecutionContext executionContext,
        TInput input,
        Func<ExecutionContext, TInput, TEntityBase> entityFactory,
        Func<ExecutionContext, TInput, TEntityBase, bool> handler
    )
    {
        var entity = entityFactory(executionContext, input);

        // ✅ EntityInfo criado automaticamente pela classe base
        bool entityInfoResult = entity.SetEntityInfo(
            executionContext,
            entityInfo: EntityInfo.RegisterNew(
                executionContext,
                tenantInfo: executionContext.TenantInfo,
                createdBy: executionContext.ExecutionUser
            )
        );

        if (!entityInfoResult)
            return default;

        // Handler foca APENAS na lógica de negócio
        bool isSuccess = handler(executionContext, input, entity);

        return isSuccess ? entity : default;
    }

    protected TEntityBase? RegisterChangeInternal<TInput>(
        ExecutionContext executionContext,
        TEntityBase instance,
        TInput input,
        Func<ExecutionContext, TInput, TEntityBase, bool> handler
    )
    {
        var newInstance = (TEntityBase)instance.Clone();

        // ✅ EntityInfo atualizado automaticamente pela classe base
        bool entityInfoResult = newInstance.SetEntityInfo(
            executionContext,
            entityInfo: newInstance.EntityInfo.RegisterChange(
                executionContext,
                changedBy: executionContext.ExecutionUser
            )
        );

        if (!entityInfoResult)
            return default;

        // Handler foca APENAS na lógica de negócio
        bool isSuccess = handler(executionContext, input, newInstance);

        return isSuccess ? newInstance : default;
    }
}
```

### Entidades Não Tocam em EntityInfo

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // Propriedades de negócio
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }

    // ✅ RegisterNew NÃO mexe em EntityInfo - classe base faz isso
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (ctx, inp) => new SimpleAggregateRoot(),
            handler: (ctx, inp, instance) =>
            {
                // Apenas lógica de negócio - EntityInfo já foi configurado
                return
                    instance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
                    & instance.ChangeBirthDateInternal(ctx, inp.BirthDate);
            }
        );
    }

    // ✅ ChangeName NÃO mexe em EntityInfo - classe base faz isso
    public SimpleAggregateRoot? ChangeName(
        ExecutionContext executionContext,
        ChangeNameInput input
    )
    {
        return RegisterChangeInternal(
            executionContext,
            instance: this,
            input,
            handler: (ctx, inp, newInstance) =>
            {
                // Apenas lógica de negócio
                // newInstance.EntityInfo já tem versão incrementada!
                return newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName);
            }
        );
    }
}
```

### O Que EntityInfo Contém

```csharp
public readonly record struct EntityInfo
{
    // Identificador único (gerado no RegisterNew)
    public Id Id { get; }

    // Informações de tenant (multitenancy)
    public TenantInfo TenantInfo { get; }

    // Auditoria de criação e modificação
    public EntityChangeInfo EntityChangeInfo { get; }
    // ├── CreatedAt      - Quando foi criado
    // ├── CreatedBy      - Quem criou
    // ├── LastChangedAt  - Quando foi modificado (null se nunca)
    // └── LastChangedBy  - Quem modificou (null se nunca)

    // Versão para optimistic locking
    public RegistryVersion EntityVersion { get; }
}
```

### Fluxo de Gerenciamento

```
+-------------------------------------------------------------------------+
│                        RegisterNewInternal                              │
│                                                                         │
│  1. Cria instância vazia (entityFactory)                                │
│  2. ✅ Cria EntityInfo com:                                             │
│     - Id gerado (ULID baseado em TimeProvider)                          │
│     - TenantInfo do ExecutionContext                                    │
│     - CreatedAt/CreatedBy do ExecutionContext                           │
│     - EntityVersion = 1                                                 │
│  3. Chama handler (lógica de negócio)                                   │
│  4. Retorna entidade ou null                                            │
+-------------------------------------------------------------------------+

+-------------------------------------------------------------------------+
│                       RegisterChangeInternal                            │
│                                                                         │
│  1. Clona instância existente                                           │
│  2. ✅ Atualiza EntityInfo com:                                         │
│     - Mantém Id, TenantInfo, CreatedAt, CreatedBy                       │
│     - Atualiza LastChangedAt/LastChangedBy do ExecutionContext          │
│     - Incrementa EntityVersion                                          │
│  3. Chama handler (lógica de negócio)                                   │
│  4. Retorna nova instância ou null                                      │
+-------------------------------------------------------------------------+
```

### Benefícios

1. **Zero código repetitivo**: Entidades não precisam gerenciar metadados
   ```csharp
   // Antes: cada entidade repetia
   order.Id = Guid.NewGuid();
   order.CreatedAt = DateTime.UtcNow;

   // Depois: classe base faz automaticamente
   // Entidade só implementa lógica de negócio
   ```

2. **Impossível esquecer**: Se usar RegisterNewInternal/RegisterChangeInternal, EntityInfo é gerenciado
   ```csharp
   // Não há como criar entidade sem EntityInfo válido
   // Não há como modificar sem incrementar versão
   ```

3. **Consistência garantida**: Todas as entidades têm mesmo formato
   ```csharp
   // Order.EntityInfo.Id é do tipo Id
   // Customer.EntityInfo.Id é do tipo Id
   // Product.EntityInfo.Id é do tipo Id
   // Sempre o mesmo tipo!
   ```

4. **Auditoria completa**: Sempre há CreatedAt, CreatedBy, etc.
   ```csharp
   // Toda entidade tem auditoria porque EntityBase fornece
   entity.EntityInfo.EntityChangeInfo.CreatedAt
   entity.EntityInfo.EntityChangeInfo.CreatedBy
   ```

5. **Separação de responsabilidades**:
   ```csharp
   // Entidade: lógica de negócio
   // EntityBase: metadados de infraestrutura
   ```

6. **Testabilidade**: EntityInfo usa TimeProvider do contexto
   ```csharp
   var fakeTimeProvider = new FakeTimeProvider(specificDate);
   var ctx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);
   var entity = Entity.RegisterNew(ctx, input);

   // EntityInfo.EntityChangeInfo.CreatedAt é previsível!
   entity.EntityInfo.EntityChangeInfo.CreatedAt.Should().Be(specificDate);
   ```

### Trade-offs (Com Perspectiva)

- **Acoplamento com EntityBase**: Entidades devem herdar de EntityBase
  - **Mitigação**: EntityBase fornece infraestrutura comum. É um acoplamento útil, não prejudicial.

- **EntityInfo como "caixa preta"**: Desenvolvedor pode não saber que existe
  - **Mitigação**: Documentação e convenção. EntityInfo está em `EntityBase`, visível para todos.

### CreateFromExistingInfo Não Usa Contexto

Para reconstitution (carregar do banco), `EntityInfo` é criado diretamente:

```csharp
public static SimpleAggregateRoot CreateFromExistingInfo(
    CreateFromExistingInfoInput input
)
{
    // ✅ Sem ExecutionContext - apenas reconstitui dados existentes
    return new SimpleAggregateRoot(
        entityInfo: EntityInfo.CreateFromExistingInfo(
            id: input.Id,
            tenantInfo: input.TenantInfo,
            entityChangeInfo: input.EntityChangeInfo,
            entityVersion: input.EntityVersion
        )
    )
    {
        FirstName = input.FirstName,
        LastName = input.LastName,
        BirthDate = input.BirthDate
    };
}
```

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre identidade:

> "An object defined primarily by its identity is called an ENTITY. [...] When an object is distinguished by its identity, rather than its attributes, make this primary to its definition in the model."
>
> *Um objeto definido primariamente por sua identidade é chamado de ENTITY. [...] Quando um objeto é distinguido por sua identidade, ao invés de seus atributos, faça isso primário em sua definição no modelo.*

O ID gerenciado pela classe base garante que toda entidade tenha identidade consistente.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre agregados:

> "The Aggregate Root is responsible for ensuring that all invariants are satisfied before and after any operation."
>
> *O Aggregate Root é responsável por garantir que todas as invariantes sejam satisfeitas antes e depois de qualquer operação.*

EntityInfo gerenciado pela classe base é uma invariante garantida para todas as entidades.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre DRY:

> "Every piece of knowledge must have a single, unambiguous, authoritative representation within a system."
>
> *Toda peça de conhecimento deve ter uma única, não ambígua, representação autoritativa dentro de um sistema.*

Lógica de EntityInfo em um só lugar (EntityBase), não duplicada em cada entidade.

### Template Method Pattern

O `RegisterNewInternal` e `RegisterChangeInternal` são variações do Template Method Pattern (GoF):

> "Define the skeleton of an algorithm in an operation, deferring some steps to subclasses."
>
> *Defina o esqueleto de um algoritmo em uma operação, adiando alguns passos para subclasses.*

- **Esqueleto**: Criar/clonar, atualizar EntityInfo, chamar handler, retornar
- **Passos adiados**: Handler (lógica de negócio específica)

## Antipadrões Documentados

### Antipadrão 1: Entidade Gerencia Próprio ID

```csharp
// ❌ Entidade cria próprio ID
public sealed class Order : EntityBase<Order>
{
    public Guid OrderId { get; private set; }  // ID separado do EntityInfo

    public static Order? RegisterNew(...)
    {
        var order = new Order();
        order.OrderId = Guid.NewGuid();  // Gerenciamento manual
        // ...
    }
}
```

### Antipadrão 2: Entidade Gerencia Auditoria

```csharp
// ❌ Entidade gerencia própria auditoria
public sealed class Order : EntityBase<Order>
{
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }

    public static Order? RegisterNew(ExecutionContext ctx, ...)
    {
        var order = new Order();
        order.CreatedAt = ctx.TimeProvider.GetUtcNow();  // Manual
        order.CreatedBy = ctx.ExecutionUser;             // Repetitivo
        // ...
    }
}
```

### Antipadrão 3: Esquecer de Atualizar Versão

```csharp
// ❌ Modificação sem incrementar versão
public Order? UpdateShipping(ExecutionContext ctx, Address newAddress)
{
    var clone = this.Clone();
    clone.ShippingAddress = newAddress;
    // Oops! EntityVersion não foi incrementada
    // Optimistic locking quebrado
    return clone;
}
```

### Antipadrão 4: Modificar EntityInfo Diretamente

```csharp
// ❌ Tentar modificar EntityInfo de fora
public void ForceVersion(int newVersion)
{
    // EntityInfo é readonly record struct
    // E EntityBase.EntityInfo tem setter private
    // Não é possível - e isso é intencional!
}
```

### Antipadrão 5: Bypass de RegisterChangeInternal

```csharp
// ❌ Modificar sem passar por RegisterChangeInternal
public Order? UnsafeUpdate(string newCustomer)
{
    var clone = (Order)this.Clone();
    clone.CustomerName = newCustomer;
    // EntityInfo não atualizado!
    // Versão não incrementada!
    // Auditoria não atualizada!
    return clone;
}
```

## Decisões Relacionadas

- [DE-003](./DE-003-imutabilidade-controlada-clone-modify-return.md) - Imutabilidade Controlada (Clone é parte do fluxo)
- [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) - Separação RegisterNew vs CreateFromExistingInfo
- [DE-020](./DE-020-dois-construtores-privados.md) - Dois Construtores Privados (um recebe EntityInfo)
- [DE-023](./DE-023-register-internal-chamado-uma-unica-vez.md) - Register*Internal chamado uma única vez
- [DE-032](./DE-032-optimistic-locking-com-entityversion.md) - Optimistic Locking com EntityVersion

## Building Blocks Relacionados

- **[Id](../../building-blocks/core/ids/id.md)** - Documentação completa sobre identificadores únicos baseados em UUIDv7, incluindo ordenação temporal, performance e casos de uso.
- **[RegistryVersion](../../building-blocks/core/registry-versions/registry-version.md)** - Documentação completa sobre versões monotônicas para optimistic locking, incluindo proteção contra clock drift.
- **[CustomTimeProvider](../../building-blocks/core/time-providers/custom-time-provider.md)** - Documentação sobre o TimeProvider usado para gerar timestamps de auditoria testáveis.

## Leitura Recomendada

- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Implementing Domain-Driven Design - Vaughn Vernon](https://vaughnvernon.com/)
- [Template Method Pattern - GoF](https://refactoring.guru/design-patterns/template-method)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Gerencia EntityInfo na classe base, expondo propriedades convenientes e protegendo invariantes |
| [EntityInfo](../../building-blocks/domain-entities/models/entity-info.md) | Modelo que encapsula metadados da entidade (Id, TenantInfo, versão, auditoria) |
| [EntityChangeInfo](../../building-blocks/domain-entities/models/entity-change-info.md) | Modelo que encapsula informações de auditoria (CreatedAt/By, LastChangedAt/By) gerenciado pela EntityBase |

## Referências no Código

- [EntityInfo.cs](../../../src/BuildingBlocks/Domain.Entities/Models/EntityInfo.cs) - Value Object com metadados
- [EntityChangeInfo.cs](../../../src/BuildingBlocks/Domain.Entities/Models/EntityChangeInfo.cs) - Informações de auditoria
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - propriedade EntityInfo e construtores
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - SetEntityInfo
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - RegisterNewInternal criando EntityInfo
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - RegisterChangeInternal atualizando EntityInfo
