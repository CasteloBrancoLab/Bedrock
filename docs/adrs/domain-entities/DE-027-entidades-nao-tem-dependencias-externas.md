# DE-027: Entidades Não Têm Dependências Externas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma **calculadora científica**:

**Calculadora autônoma**:
- Você digita `2 + 2` e ela retorna `4`
- Funciona em qualquer lugar: escritório, avião, praia
- Não precisa de internet, bateria extra, ou conexão com servidor
- Resultado é sempre previsível e instantâneo

**Calculadora "smart" com dependências**:
- Você digita `2 + 2` e ela... conecta na nuvem para buscar a operação de soma
- Sem internet? Não funciona
- Servidor lento? Calculadora lenta
- Servidor mudou a API? Calculadora quebra
- Para testar, precisa mockar o servidor

Em software, entidades de domínio devem ser como a calculadora autônoma: **autocontidas**, sem dependências externas que compliquem seu uso, teste e manutenção.

---

### O Problema Técnico

Quando entidades dependem de serviços externos, surgem problemas graves:

```csharp
// ❌ ANTIPATTERN: Entidade com dependências injetadas
public class Order : EntityBase<Order>
{
    private readonly IDiscountService _discountService;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly INotificationService _notificationService;

    // Construtor com injeção de dependência
    public Order(
        IDiscountService discountService,
        IInventoryRepository inventoryRepository,
        INotificationService notificationService
    )
    {
        _discountService = discountService;
        _inventoryRepository = inventoryRepository;
        _notificationService = notificationService;
    }

    public Order? AddItem(ExecutionContext ctx, Product product, int quantity)
    {
        // Precisa chamar serviço externo para validar estoque
        bool hasStock = _inventoryRepository.CheckStock(product.Id, quantity);
        if (!hasStock)
            return null;

        // Precisa chamar serviço externo para calcular desconto
        decimal discount = _discountService.Calculate(this, product);

        // Side-effect: notifica sistema externo
        _notificationService.NotifyItemAdded(this.Id, product.Id);

        // ... resto da lógica
    }
}
```

**Consequências graves**:

1. **Testabilidade comprometida**:
   ```csharp
   // Para testar Order, preciso mockar 3 serviços
   var mockDiscount = new Mock<IDiscountService>();
   var mockInventory = new Mock<IInventoryRepository>();
   var mockNotification = new Mock<INotificationService>();

   // Preciso configurar comportamento de cada mock
   mockInventory.Setup(x => x.CheckStock(It.IsAny<Guid>(), It.IsAny<int>()))
       .Returns(true);

   // Só então posso criar a entidade
   var order = new Order(mockDiscount.Object, mockInventory.Object, mockNotification.Object);

   // Teste é 80% setup, 20% teste real
   ```

2. **Reconstitution quebra**:
   ```csharp
   // Como reconstituir do banco se precisa de dependências?
   public static Order CreateFromExistingInfo(CreateFromExistingInfoInput input)
   {
       // De onde vêm os serviços?!
       return new Order(???, ???, ???);
   }
   ```

3. **Comportamento imprevisível**:
   ```csharp
   // Mesmo input pode dar resultados diferentes
   order1.AddItem(ctx, product, 10);  // Servidor retorna desconto 10%
   order2.AddItem(ctx, product, 10);  // Servidor retorna desconto 15% (promoção mudou)
   // Entidades idênticas com estados diferentes!
   ```

4. **Acoplamento temporal**:
   ```csharp
   // Ordem de chamadas importa
   order.AddItem(ctx, product, 10);  // OK, estoque existe
   // ... 5 minutos depois ...
   order.AddItem(ctx, product, 10);  // Falha! Estoque acabou
   // Entidade "quebrou" sem nenhuma modificação
   ```

### Como Normalmente é Feito (e Por Que Não é Ideal)

```csharp
// ❌ COMUM: Entidade "rica" com lógica que depende de serviços
public class Customer : EntityBase<Customer>
{
    private readonly ICreditService _creditService;
    private readonly IEmailService _emailService;

    public Customer(ICreditService creditService, IEmailService emailService)
    {
        _creditService = creditService;
        _emailService = emailService;
    }

    public bool CanPurchase(decimal amount)
    {
        // Chama serviço externo - entidade não é autocontida
        var creditLimit = _creditService.GetCreditLimit(this.Id);
        return creditLimit >= amount;
    }

    public void NotifyPurchase(Order order)
    {
        // Side-effect em entidade - viola imutabilidade conceitual
        _emailService.SendPurchaseConfirmation(this.Email, order);
    }
}
```

**Problemas**:
- Entidade não pode existir sem os serviços
- Testes unitários viram testes de integração
- Clone/reconstitution precisam resolver dependências
- Lógica de domínio misturada com infraestrutura

## A Decisão

### Nossa Abordagem

Entidades de domínio **NÃO dependem** de:
- ❌ Repositórios
- ❌ Serviços externos
- ❌ Factories injetadas
- ❌ Configurações dinâmicas
- ❌ Qualquer interface que precise de implementação externa

**ÚNICAS dependências permitidas**:
- ✅ `ExecutionContext` (contexto de execução passado como parâmetro)
- ✅ Outras entidades de domínio (associação/composição)
- ✅ Value Objects do próprio domínio

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // ✅ Propriedades - dados da entidade
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public BirthDate BirthDate { get; private set; }  // Value Object do domínio

    // ✅ Construtores privados - sem injeção de dependência
    private SimpleAggregateRoot() { }

    private SimpleAggregateRoot(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        string fullName,
        BirthDate birthDate
    ) : base(entityInfo)
    {
        // Apenas atribuição de valores
        // NENHUMA chamada a serviço externo
    }

    // ✅ Métodos recebem ExecutionContext como parâmetro
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,  // ✅ Contexto passado, não injetado
        RegisterNewInput input
    )
    {
        // Lógica usa apenas:
        // - executionContext (parâmetro)
        // - input (parâmetro)
        // - métodos da própria classe
        // - metadados estáticos
    }
}
```

### ExecutionContext: A Única "Dependência"

`ExecutionContext` não é uma dependência injetada - é um **parâmetro** que carrega contexto da operação:

```csharp
public class ExecutionContext
{
    // Identidade de quem está executando
    public string ExecutionUser { get; }

    // Tenant atual (multitenancy)
    public TenantInfo TenantInfo { get; }

    // Provedor de tempo (testável)
    public TimeProvider TimeProvider { get; }

    // Mensagens de validação
    public MessageCollection Messages { get; }
}
```

**Por que ExecutionContext não viola a regra**:

1. **É parâmetro, não dependência**: Passado em cada chamada, não injetado no construtor
2. **É dado, não comportamento**: Carrega informações, não executa lógica externa
3. **É determinístico**: Mesmo contexto = mesmo resultado
4. **É testável**: Fácil criar contexto de teste sem mocks

```csharp
// ✅ Teste simples - sem mocks de serviços
[Fact]
public void RegisterNew_WithValidInput_ShouldCreateEntity()
{
    // Arrange - contexto simples
    var ctx = new ExecutionContext(
        executionUser: "test-user",
        tenantInfo: TenantInfo.Default,
        timeProvider: new FakeTimeProvider(new DateTime(2025, 1, 15))
    );

    var input = new RegisterNewInput("John", "Doe", new BirthDate(1990, 5, 20));

    // Act - chamada direta, sem setup de mocks
    var result = SimpleAggregateRoot.RegisterNew(ctx, input);

    // Assert
    result.Should().NotBeNull();
    result!.FirstName.Should().Be("John");
}
```

### Onde Fica a Lógica que Precisa de Serviços?

A lógica que precisa de serviços externos fica em **Application Services** ou **Domain Services**:

```csharp
// ✅ CORRETO: Serviço de aplicação orquestra entidade + serviços externos
public class OrderApplicationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IDiscountService _discountService;

    public OrderApplicationService(
        IOrderRepository orderRepository,
        IInventoryService inventoryService,
        IDiscountService discountService
    )
    {
        _orderRepository = orderRepository;
        _inventoryService = inventoryService;
        _discountService = discountService;
    }

    public async Task<Order?> AddItemToOrder(
        ExecutionContext ctx,
        Guid orderId,
        Guid productId,
        int quantity
    )
    {
        // 1. Busca entidade (repositório)
        var order = await _orderRepository.GetById(orderId);
        if (order == null)
            return null;

        // 2. Consulta serviços externos ANTES de chamar entidade
        var hasStock = await _inventoryService.CheckStock(productId, quantity);
        if (!hasStock)
        {
            ctx.Messages.AddError("OUT_OF_STOCK", "Produto sem estoque");
            return null;
        }

        var discount = await _discountService.Calculate(order, productId);

        // 3. Chama entidade com dados já resolvidos
        var input = new AddItemInput(productId, quantity, discount);
        var updatedOrder = order.AddItem(ctx, input);

        // 4. Persiste resultado (repositório)
        if (updatedOrder != null)
            await _orderRepository.Save(updatedOrder);

        return updatedOrder;
    }
}

// ✅ Entidade é simples e autocontida
public sealed class Order : EntityBase<Order>
{
    // SEM dependências injetadas

    public Order? AddItem(ExecutionContext ctx, AddItemInput input)
    {
        // Recebe desconto já calculado - não chama serviço
        // Recebe quantidade já validada contra estoque - não chama repositório
        // Apenas aplica regras de negócio da entidade
    }
}
```

### Comparação Visual

```
+-------------------------------------------------------------------------+
│                    ❌ ENTIDADE COM DEPENDÊNCIAS                         │
│                                                                         │
│  +---------------------+                                               │
│  │      Order          │                                               │
│  │  +---------------+  │      +-----------------+                     │
│  │  │ _discountSvc  │--+-----→│ IDiscountService│ (externa)           │
│  │  +---------------│  │      +-----------------+                     │
│  │  │ _inventorySvc │--+-----→│ IInventoryService│ (externa)          │
│  │  +---------------│  │      +-----------------+                     │
│  │  │ _notifySvc    │--+-----→│ INotificationSvc│ (externa)           │
│  │  +---------------+  │      +-----------------+                     │
│  +---------------------+                                               │
│                                                                         │
│  Problemas: Testabilidade, reconstitution, acoplamento                 │
+-------------------------------------------------------------------------+

+-------------------------------------------------------------------------+
│                    ✅ ENTIDADE AUTOCONTIDA                              │
│                                                                         │
│  +---------------------+                                               │
│  │      Order          │                                               │
│  │  +---------------+  │                                               │
│  │  │ Items         │  │  ✓ Apenas dados                               │
│  │  │ TotalAmount   │  │  ✓ Apenas dados                               │
│  │  │ Status        │  │  ✓ Apenas dados                               │
│  │  +---------------+  │                                               │
│  │                     │                                               │
│  │  AddItem(ctx, input)│  ✓ Recebe tudo que precisa como parâmetro    │
│  +---------------------+                                               │
│                                                                         │
│  +-----------------------------------------------------------------+   │
│  │                  Application Service                             │   │
│  │  +---------------+  +---------------+  +---------------+        │   │
│  │  │ _discountSvc  │  │ _inventorySvc │  │ _repository   │        │   │
│  │  +---------------+  +---------------+  +---------------+        │   │
│  │          │                  │                  │                 │   │
│  │          +------------------+------------------+                 │   │
│  │                             │                                    │   │
│  │                             ▼                                    │   │
│  │                    Order.AddItem(ctx, input)                     │   │
│  │                    (dados já resolvidos)                         │   │
│  +-----------------------------------------------------------------+   │
+-------------------------------------------------------------------------+
```

### Benefícios

1. **Testabilidade trivial**: Sem mocks, sem setup complexo
   ```csharp
   // Teste de entidade = teste unitário REAL
   var result = Entity.RegisterNew(ctx, input);
   result.Should().NotBeNull();
   ```

2. **Reconstitution simples**: Não precisa resolver dependências
   ```csharp
   // Construtor privado não tem parâmetros de serviço
   public static Order CreateFromExistingInfo(CreateFromExistingInfoInput input)
   {
       return new Order(input.EntityInfo, input.Items, input.TotalAmount);
       // Funciona sempre, sem container de DI
   }
   ```

3. **Comportamento determinístico**: Mesmo input = mesmo output
   ```csharp
   var result1 = order.AddItem(ctx, input);
   var result2 = order.AddItem(ctx, input);
   // result1 e result2 são idênticos (assumindo mesmo estado inicial)
   ```

4. **Clone funciona**: Não precisa clonar referências de serviços
   ```csharp
   public Order Clone()
   {
       return new Order(EntityInfo, Items, TotalAmount);
       // Cópia perfeita, sem preocupação com dependências
   }
   ```

5. **Serialização funciona**: Entidade pode ser serializada/deserializada
   ```csharp
   var json = JsonSerializer.Serialize(order);
   var restored = JsonSerializer.Deserialize<Order>(json);
   // Funciona porque não há referências a serviços
   ```

### Trade-offs (Com Perspectiva)

- **Mais código no Application Service**: Lógica de orquestração fica fora da entidade
  - **Mitigação**: Separação clara de responsabilidades. Entidade = regras de negócio. Service = orquestração.

- **Dados precisam ser passados como parâmetros**: Não pode "buscar" dentro da entidade
  - **Mitigação**: Input Objects encapsulam todos os dados necessários.

### Trade-offs Frequentemente Superestimados

**"Entidade fica 'anêmica'"**

Entidade sem dependências NÃO é anêmica. Ela ainda tem:
- Validações de negócio
- Cálculos internos
- Regras de transição de estado
- Invariantes protegidas

O que ela NÃO tem é acoplamento com infraestrutura.

```csharp
// ✅ Entidade RICA sem dependências externas
public sealed class Order : EntityBase<Order>
{
    public Order? AddItem(ExecutionContext ctx, AddItemInput input)
    {
        // Validação de negócio (na entidade)
        if (Status == OrderStatus.Closed)
        {
            ctx.Messages.AddError("ORDER_CLOSED", "Pedido fechado");
            return null;
        }

        // Cálculo interno (na entidade)
        var newTotal = TotalAmount + (input.UnitPrice * input.Quantity) - input.Discount;

        // Regra de negócio (na entidade)
        if (newTotal > MaxOrderAmount)
        {
            ctx.Messages.AddError("MAX_EXCEEDED", "Valor máximo excedido");
            return null;
        }

        // Modificação de estado (na entidade)
        return RegisterChangeInternal(ctx, this, input, handler: (c, i, clone) =>
            clone.AddItemInternal(c, i.ProductId, i.Quantity, i.UnitPrice, i.Discount)
        );
    }
}
```

**"Preciso validar contra dados externos"**

Validação contra dados externos acontece no Application Service, ANTES de chamar a entidade:

```csharp
// Application Service
public async Task<Order?> AddItem(ExecutionContext ctx, AddItemRequest request)
{
    // Validação externa (no service)
    var product = await _productRepository.GetById(request.ProductId);
    if (product == null)
    {
        ctx.Messages.AddError("PRODUCT_NOT_FOUND", "Produto não encontrado");
        return null;
    }

    // Preparar input com dados já validados
    var input = new AddItemInput(
        ProductId: product.Id,
        Quantity: request.Quantity,
        UnitPrice: product.Price,
        Discount: await _discountService.Calculate(order, product)
    );

    // Entidade recebe dados já validados
    return order.AddItem(ctx, input);
}
```

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre a pureza do modelo:

> "The domain layer is the heart of business software. [...] It should be isolated from other concerns of the software."
>
> *A camada de domínio é o coração do software de negócio. [...] Ela deve ser isolada de outras preocupações do software.*

Dependências de repositórios e serviços externos são "outras preocupações" que poluem o domínio.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre entidades:

> "Entities should not have any dependencies on infrastructure or application services. They should be plain objects with behavior."
>
> *Entidades não devem ter dependências de infraestrutura ou serviços de aplicação. Devem ser objetos simples com comportamento.*

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre dependências:

> "Dependencies should point inward. Nothing in an inner circle can know anything at all about something in an outer circle."
>
> *Dependências devem apontar para dentro. Nada em um círculo interno pode saber qualquer coisa sobre algo em um círculo externo.*

Entidades (círculo interno) não devem conhecer serviços e repositórios (círculos externos).

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre entidades:

> "Entities encapsulate Enterprise-wide business rules. [...] They are the least likely to change when something external changes."
>
> *Entidades encapsulam regras de negócio de toda a empresa. [...] São as menos prováveis de mudar quando algo externo muda.*

Se entidade depende de IDiscountService, ela muda quando IDiscountService muda - violando este princípio.

Robert C. Martin sobre a Dependency Rule:

> "Source code dependencies can only point inwards. Nothing in an inner circle can know anything at all about something in an outer circle."
>
> *Dependências de código fonte só podem apontar para dentro. Nada em um círculo interno pode saber qualquer coisa sobre algo em um círculo externo.*

### Princípio de Inversão de Dependência (DIP)

SOLID - Dependency Inversion Principle:

> "High-level modules should not depend on low-level modules. Both should depend on abstractions."
>
> *Módulos de alto nível não devem depender de módulos de baixo nível. Ambos devem depender de abstrações.*

Mas DIP não significa "injete tudo na entidade". Significa que a CAMADA de domínio não depende da CAMADA de infraestrutura. Entidades são autocontidas; Application Services é que orquestram as dependências.

### Hexagonal Architecture

Alistair Cockburn sobre Ports and Adapters:

> "The application is blissfully unaware of the nature of the input/output devices."
>
> *A aplicação desconhece completamente a natureza dos dispositivos de entrada/saída.*

Entidades são parte do "hexágono interno" - não conhecem adapters (repositórios, serviços HTTP, etc.).

## Antipadrões Documentados

### Antipadrão 1: Injeção de Repositório na Entidade

```csharp
// ❌ Entidade depende de repositório
public class Order : EntityBase<Order>
{
    private readonly IProductRepository _productRepository;

    public Order(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Order? AddItem(Guid productId, int quantity)
    {
        // Busca produto dentro da entidade - ERRADO
        var product = _productRepository.GetById(productId);
        // ...
    }
}
```

### Antipadrão 2: Injeção de Serviço de Domínio na Entidade

```csharp
// ❌ Entidade depende de serviço de domínio
public class Customer : EntityBase<Customer>
{
    private readonly ICreditScoreService _creditService;

    public Customer(ICreditScoreService creditService)
    {
        _creditService = creditService;
    }

    public bool CanApproveCredit(decimal amount)
    {
        // Chama serviço dentro da entidade - ERRADO
        var score = _creditService.CalculateScore(this);
        return score > 700 && amount < 10000;
    }
}
```

### Antipadrão 3: Service Locator na Entidade

```csharp
// ❌ Entidade usa Service Locator
public class Invoice : EntityBase<Invoice>
{
    public Invoice? ApplyTax()
    {
        // Resolve serviço via locator - ERRADO
        var taxService = ServiceLocator.Get<ITaxService>();
        var taxRate = taxService.GetRate(this.Country);
        // ...
    }
}
```

### Antipadrão 4: Dependência de Configuração

```csharp
// ❌ Entidade depende de configuração externa
public class Product : EntityBase<Product>
{
    private readonly IConfiguration _config;

    public Product(IConfiguration config)
    {
        _config = config;
    }

    public decimal GetPrice()
    {
        var markup = _config.GetValue<decimal>("PriceMarkup");
        return BasePrice * (1 + markup);
    }
}
```

### Antipadrão 5: Entidade que Envia Notificações

```csharp
// ❌ Entidade com side-effects de infraestrutura
public class Order : EntityBase<Order>
{
    private readonly IEmailService _emailService;
    private readonly IEventBus _eventBus;

    public Order? Complete()
    {
        Status = OrderStatus.Completed;

        // Side-effects de infraestrutura na entidade - ERRADO
        _emailService.SendOrderConfirmation(CustomerEmail);
        _eventBus.Publish(new OrderCompletedEvent(Id));

        return this;
    }
}
```

## Decisões Relacionadas

- [DE-002](./DE-002-construtores-privados-com-factory-methods.md) - Construtores privados (sem injeção)
- [DE-018](./DE-018-reconstitution-nao-valida-dados.md) - Reconstitution funciona sem dependências
- [DE-019](./DE-019-input-objects-pattern.md) - Input Objects carregam dados necessários
- [DE-020](./DE-020-dois-construtores-privados.md) - Construtores simples, sem serviços

## Building Blocks Relacionados

- **[CustomTimeProvider](../../building-blocks/core/time-providers/custom-time-provider.md)** - O TimeProvider é a única dependência permitida via ExecutionContext, permitindo testes determinísticos sem acoplar a entidade à infraestrutura.

## Leitura Recomendada

- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Implementing Domain-Driven Design - Vaughn Vernon](https://vaughnvernon.com/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Hexagonal Architecture - Alistair Cockburn](https://alistair.cockburn.us/hexagonal-architecture/)
- [Anemic Domain Model - Martin Fowler](https://martinfowler.com/bliki/AnemicDomainModel.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Não tem dependências externas, apenas ExecutionContext como parâmetro explícito em métodos |
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Passado como parâmetro explícito aos métodos, não como dependência injetada |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Entidades Não Têm Dependências Externas
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Construtores privados sem parâmetros de serviço
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Métodos públicos recebem ExecutionContext como parâmetro
