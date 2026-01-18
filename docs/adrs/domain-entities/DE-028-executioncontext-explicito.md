# DE-028: ExecutionContext Explícito (não Implícito)

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **hospital** com dois modelos de prontuário:

**Modelo "mesa central" (implícito)**:
- Todos os médicos pegam e devolvem prontuários numa mesa central
- Enfermeira precisa "saber" qual prontuário o médico está usando
- Se dois médicos atendem simultaneamente, confusão sobre qual prontuário é de quem
- Auditoria difícil: "quem acessou este prontuário às 14h?"

**Modelo "prontuário em mãos" (explícito)**:
- Cada médico recebe o prontuário do paciente ao iniciar atendimento
- Passa o prontuário para enfermeira quando precisa de algo
- Qualquer pessoa sabe qual prontuário está sendo usado - está nas mãos
- Auditoria trivial: "Dr. Silva tinha o prontuário às 14h"

O `ExecutionContext` é como um prontuário de operação - contém quem está executando, quando, em qual tenant, e registra mensagens/eventos durante a operação. Passar explicitamente como parâmetro é como manter o prontuário em mãos.

---

### O Papel do ExecutionContext

O `ExecutionContext` é um **observador passivo** que:

```csharp
public class ExecutionContext
{
    // Metadados da operação
    public DateTimeOffset Timestamp { get; }       // Quando a operação iniciou
    public Guid CorrelationId { get; }             // ID para rastreamento
    public TenantInfo TenantInfo { get; }          // Multitenancy
    public string ExecutionUser { get; }           // Quem está executando
    public TimeProvider TimeProvider { get; }      // Provedor de tempo (testável)

    // Coleta de mensagens (observação)
    public void AddInformationMessage(string code, string? text = null);
    public void AddWarningMessage(string code, string? text = null);
    public void AddErrorMessage(string code, string? text = null);
    public void AddSuccessMessage(string code, string? text = null);
    public void AddException(Exception exception);

    // Status (consulta no final)
    public bool IsSuccessful { get; }              // Sem erros/exceções
    public bool HasErrorMessages { get; }          // Tem mensagens de erro
    public IEnumerable<Message> Messages { get; }  // Todas as mensagens
}
```

**IMPORTANTE**: O `ExecutionContext` **NÃO** controla fluxo. Métodos retornam seu próprio status (`bool`, `T?`, `Result<T>`). O contexto apenas **observa e registra**.

---

### O Problema Técnico

Existem várias formas de disponibilizar o `ExecutionContext` para as entidades:

**Opção 1: Injeção no construtor**
```csharp
// ❌ ExecutionContext injetado no construtor da entidade
public sealed class Order : EntityBase<Order>
{
    private readonly ExecutionContext _ctx;

    public Order(ExecutionContext ctx)  // Injetado
    {
        _ctx = ctx;
    }

    public Order? AddItem(AddItemInput input)
    {
        _ctx.AddInformationMessage("ITEM_ADDED", $"Item {input.ProductId}");
        // ...
    }
}
```

**Problemas**:
- Entidade fica acoplada a um contexto específico
- Reconstitution do banco precisa de contexto (mas não deveria usar)
- Contexto pode ficar "stale" se entidade viver mais que uma operação
- Clone precisa decidir: compartilha contexto ou cria novo?

**Opção 2: Propriedade estática / Service Locator**
```csharp
// ❌ ExecutionContext via propriedade estática
public static class Current
{
    public static ExecutionContext Context { get; set; }
}

public sealed class Order : EntityBase<Order>
{
    public Order? AddItem(AddItemInput input)
    {
        Current.Context.AddInformationMessage("ITEM_ADDED", ...);
        // ...
    }
}
```

**Problemas**:
- Estado global mutável
- Testes precisam configurar e limpar estado global
- Difícil rodar testes em paralelo
- Assinatura do método não revela dependência

**Opção 3: Resolver via IOC**
```csharp
// ❌ ExecutionContext via service locator
public sealed class Order : EntityBase<Order>
{
    public Order? AddItem(AddItemInput input)
    {
        var ctx = ServiceLocator.Get<ExecutionContext>();
        ctx.AddInformationMessage("ITEM_ADDED", ...);
        // ...
    }
}
```

**Problemas**:
- Dependência oculta na assinatura
- Acoplamento com container IOC
- Difícil testar sem o container configurado
- Entidade de domínio dependendo de infraestrutura (viola DE-027)

---

### Como Normalmente é Feito (e Por Que Não é Ideal)

```csharp
// ❌ COMUM: HttpContext.Current (ASP.NET antigo)
public class OrderService
{
    public void ProcessOrder(Order order)
    {
        var user = HttpContext.Current.User.Identity.Name;  // Implícito
        order.Process(user);
    }
}

// ❌ COMUM: Ambient context via ThreadLocal/AsyncLocal
public static class AmbientContext
{
    private static AsyncLocal<ExecutionContext> _current = new();
    public static ExecutionContext Current
    {
        get => _current.Value!;
        set => _current.Value = value;
    }
}

// ❌ COMUM: Propriedade em classe base
public abstract class EntityBase
{
    protected ExecutionContext Context { get; set; }  // Como é configurado?
}
```

**Problemas comuns**:
- Difícil testar sem ambiente real
- Assinatura não revela dependência
- Debug confuso: "de onde veio este contexto?"
- Acoplamento temporal: precisa configurar ANTES de chamar

## A Decisão

### Nossa Abordagem

`ExecutionContext` é **SEMPRE** passado como **parâmetro explícito**, sendo o **primeiro parâmetro** por convenção:

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // ✅ ExecutionContext como primeiro parâmetro - factory method
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,  // ✅ Explícito, primeiro
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (ctx, inp) => new SimpleAggregateRoot(),
            handler: (ctx, inp, instance) =>
            {
                // ctx disponível para validações que precisam registrar mensagens
                return
                    instance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
                    & instance.ChangeBirthDateInternal(ctx, inp.BirthDate);
            }
        );
    }

    // ✅ ExecutionContext como primeiro parâmetro - método de modificação
    public SimpleAggregateRoot? ChangeName(
        ExecutionContext executionContext,  // ✅ Explícito, primeiro
        ChangeNameInput input
    )
    {
        return RegisterChangeInternal(
            executionContext,
            instance: this,
            input,
            handler: (ctx, inp, newInstance) =>
                newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
        );
    }

    // ✅ ExecutionContext como primeiro parâmetro - validação estática
    public static bool ValidateFirstName(
        ExecutionContext executionContext,  // ✅ Explícito, primeiro
        string? firstName
    )
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            executionContext.AddErrorMessage(
                FirstNameRequiredMessageCode,
                $"First name is required"
            );
            return false;
        }
        return true;
    }
}
```

### Convenção de Posicionamento

`ExecutionContext` é **sempre o primeiro parâmetro**:

```csharp
// ✅ CORRETO - ExecutionContext primeiro
public static Entity? RegisterNew(
    ExecutionContext executionContext,
    RegisterNewInput input
)

public Entity? ChangeName(
    ExecutionContext executionContext,
    ChangeNameInput input
)

public static bool ValidateFirstName(
    ExecutionContext executionContext,
    string? firstName
)

// ❌ ERRADO - ExecutionContext no meio ou fim
public static Entity? RegisterNew(
    RegisterNewInput input,
    ExecutionContext executionContext  // Inconsistente
)
```

**Razões para ser primeiro**:
1. **Consistência**: Mesmo padrão em todos os métodos
2. **Visibilidade**: Impossível ignorar - está logo ali
3. **Previsibilidade**: Desenvolvedor sabe onde encontrar
4. **Auto-documentação**: Qualquer método com `ExecutionContext` primeiro é "context-aware"

### Fluxo de Propagação

```
+-------------------------------------------------------------------------+
│                         APPLICATION LAYER                               │
│                                                                         │
│  // Controller/Handler cria contexto no início da requisição            │
│  var ctx = ExecutionContext.Create(                                     │
│      correlationId: Guid.NewGuid(),                                     │
│      tenantInfo: GetTenantFromRequest(),                                │
│      executionUser: User.Identity.Name,                                 │
│      executionOrigin: "API",                                            │
│      minimumMessageType: MessageType.Information,                       │
│      timeProvider: TimeProvider.System                                  │
│  );                                                                     │
│                                                                         │
│  // Passa para Application Service                                      │
│  var result = await _orderService.CreateOrder(ctx, request);            │
│                                                                         │
│  // No final, consulta o contexto para logging/diagnóstico              │
│  if (!ctx.IsSuccessful)                                                 │
│      _logger.LogWarning("Issues: {Messages}", ctx.Messages);            │
+-------------------------------------------------------------------------+
                                    │
                                    ▼
+-------------------------------------------------------------------------+
│                        APPLICATION SERVICE                              │
│                                                                         │
│  public async Task<Order?> CreateOrder(                                 │
│      ExecutionContext ctx,           // ✅ Recebido                      │
│      CreateOrderRequest request                                         │
│  )                                                                      │
│  {                                                                      │
│      // Passa para entidade                                             │
│      var order = Order.RegisterNew(ctx, new RegisterNewInput(...));     │
│                                                                         │
│      if (order is null)                                                 │
│          return null;  // Método retorna falha, ctx tem as mensagens    │
│                                                                         │
│      await _repository.Save(order);                                     │
│      return order;                                                      │
│  }                                                                      │
+-------------------------------------------------------------------------+
                                    │
                                    ▼
+-------------------------------------------------------------------------+
│                           DOMAIN ENTITY                                 │
│                                                                         │
│  public static Order? RegisterNew(                                      │
│      ExecutionContext ctx,           // ✅ Recebido                      │
│      RegisterNewInput input                                             │
│  )                                                                      │
│  {                                                                      │
│      // Usa TimeProvider do contexto para timestamps                    │
│      var now = ctx.TimeProvider.GetUtcNow();                            │
│                                                                         │
│      // Registra mensagens no contexto (observação)                     │
│      ctx.AddSuccessMessage("ORDER_CREATED", $"Order created at {now}"); │
│                                                                         │
│      // Retorna entidade ou null (método controla fluxo, não contexto)  │
│      return newOrder;                                                   │
│  }                                                                      │
+-------------------------------------------------------------------------+
```

### Testabilidade

```csharp
[Fact]
public void RegisterNew_WithValidInput_ShouldCreateEntity()
{
    // Arrange - contexto explícito, isolado para este teste
    var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 1, 15, 10, 30, 0, TimeSpan.Zero));
    var ctx = ExecutionContext.Create(
        correlationId: Guid.NewGuid(),
        tenantInfo: TenantInfo.Default,
        executionUser: "test-user",
        executionOrigin: "Test",
        minimumMessageType: MessageType.Trace,
        timeProvider: fakeTimeProvider
    );

    var input = new RegisterNewInput("John", "Doe", new DateOnly(1990, 5, 20));

    // Act - contexto passado explicitamente
    var result = SimpleAggregateRoot.RegisterNew(ctx, input);

    // Assert
    result.Should().NotBeNull();
    result!.FirstName.Should().Be("John");
    ctx.IsSuccessful.Should().BeTrue();
}

[Fact]
public void RegisterNew_WithInvalidInput_ShouldReturnNullAndAddMessages()
{
    // Arrange - contexto isolado deste teste
    var ctx = ExecutionContext.Create(
        correlationId: Guid.NewGuid(),
        tenantInfo: TenantInfo.Default,
        executionUser: "test-user",
        executionOrigin: "Test",
        minimumMessageType: MessageType.Trace,
        timeProvider: TimeProvider.System
    );

    var input = new RegisterNewInput("", "Doe", new DateOnly(1990, 5, 20));  // Nome vazio

    // Act
    var result = SimpleAggregateRoot.RegisterNew(ctx, input);

    // Assert
    result.Should().BeNull();
    ctx.HasErrorMessages.Should().BeTrue();
    ctx.Messages.Should().Contain(m => m.Code == "FIRST_NAME_REQUIRED");
}

[Fact]
public void RegisterNew_WithDifferentTenants_ShouldBeIsolated()
{
    // Dois contextos com tenants diferentes - isolamento total
    var ctxTenantA = ExecutionContext.Create(..., tenantInfo: new TenantInfo("TenantA"), ...);
    var ctxTenantB = ExecutionContext.Create(..., tenantInfo: new TenantInfo("TenantB"), ...);

    // Cada operação usa seu próprio contexto
    var entityA = Entity.RegisterNew(ctxTenantA, inputA);
    var entityB = Entity.RegisterNew(ctxTenantB, inputB);

    // Mensagens ficam no contexto correto
    ctxTenantA.Messages.Should().NotIntersectWith(ctxTenantB.Messages);
}
```

**Benefícios para testes**:
- Cada teste tem seu próprio contexto - isolamento total
- Sem cleanup de estado global entre testes
- Testes podem rodar em paralelo sem interferência
- Fácil criar cenários específicos (tenant diferente, user diferente, tempo específico)
- TimeProvider fake permite testar lógica dependente de tempo

### Comparação

| Aspecto | Implícito (Service Locator, ThreadLocal) | Parâmetro Explícito |
|---------|------------------------------------------|---------------------|
| **Visibilidade** | Oculto | Visível na assinatura |
| **Testabilidade** | Precisa setup global | Cada teste cria seu contexto |
| **Debug** | "De onde veio isso?" | Call stack mostra origem |
| **Paralelismo** | Pode ter interferência | Sempre isolado |
| **Aprendizado** | Precisa conhecer convenção | Auto-documentado |
| **Refactoring** | Fácil esquecer dependência | Compilador avisa |
| **Reconstitution** | Confuso - precisa de contexto? | Claro - sem contexto |

### Benefícios

1. **Auto-documentação**: Assinatura revela dependências
   ```csharp
   // Qualquer desenvolvedor sabe: "preciso de um ExecutionContext"
   public static Entity? RegisterNew(ExecutionContext ctx, ...)
   ```

2. **Testabilidade trivial**: Sem mocks de infraestrutura
   ```csharp
   var ctx = ExecutionContext.Create(...);  // Crio o que preciso
   var result = Entity.RegisterNew(ctx, input);  // Passo explicitamente
   ```

3. **Isolamento garantido**: Não há estado compartilhado
   ```csharp
   // 100 requests simultâneas, cada uma com seu contexto
   // Zero interferência
   ```

4. **Rastreabilidade**: Fácil debugar e auditar
   ```csharp
   // Call stack mostra exatamente de onde o contexto veio
   // Todas as mensagens ficam no contexto que foi passado
   ```

5. **Clareza para iniciantes**: Explícito > implícito
   ```csharp
   // Novo desenvolvedor entende imediatamente:
   // "Ah, preciso passar o contexto para este método"
   ```

6. **Reconstitution limpo**: Sem contexto para dados do banco
   ```csharp
   // CreateFromExistingInfo NÃO recebe ExecutionContext
   // porque não está executando operação, apenas reconstituindo
   public static Entity CreateFromExistingInfo(CreateFromExistingInfoInput input)
   ```

7. **Operações especulativas com Clone**: Isolar tentativas e mesclar se sucesso
   ```csharp
   // Contexto clonado para operação opcional/especulativa
   var clonedCtx = mainCtx.Clone();
   var result = TryOptionalOperation(clonedCtx, input);

   if (result is not null)
   {
       // Sucesso - importar mensagens e exceções do clone para o principal
       mainCtx.Import(clonedCtx);
   }
   // Se falhou, mensagens do clone são simplesmente descartadas
   ```

### Clone para Operações Especulativas

O método `Clone()` do `ExecutionContext` permite **operações especulativas** - tentar algo sem "sujar" o contexto principal:

```csharp
public async Task<Order?> ProcessOrderWithOptionalDiscount(
    ExecutionContext ctx,
    Order order,
    DiscountRequest? discountRequest
)
{
    // Operação principal
    var updatedOrder = order.AddItems(ctx, items);
    if (updatedOrder is null)
        return null;

    // Operação opcional - tenta aplicar desconto
    if (discountRequest is not null)
    {
        // Clone isola mensagens da tentativa
        var discountCtx = ctx.Clone();

        var discountedOrder = updatedOrder.ApplyDiscount(discountCtx, discountRequest);

        if (discountedOrder is not null)
        {
            // Desconto aplicado com sucesso - importar mensagens e exceções
            ctx.Import(discountCtx);
            updatedOrder = discountedOrder;
        }
        // Se falhou, mensagens de erro do desconto são descartadas
        // O pedido continua sem desconto, sem poluir o contexto principal
    }

    return updatedOrder;
}
```

**Casos de uso comuns**:

1. **Operações opcionais**: Tentar aplicar algo que pode falhar sem afetar a operação principal
   ```csharp
   // Tenta enriquecer dados, mas se falhar, continua sem
   var enrichedCtx = ctx.Clone();
   var enrichedData = TryEnrichWithExternalData(enrichedCtx, data);
   if (enrichedData is not null)
       ctx.Import(enrichedCtx);
   ```

2. **Validação prévia (dry-run)**: Verificar se operação seria válida antes de executar
   ```csharp
   // Simula operação para ver se daria certo
   var simulationCtx = ctx.Clone();
   var wouldSucceed = SimulateOperation(simulationCtx, input);
   // Mensagens da simulação não vão para o contexto real
   ```

3. **Fallback com isolamento**: Tentar abordagem A, se falhar tenta B
   ```csharp
   var attemptACtx = ctx.Clone();
   var resultA = TryApproachA(attemptACtx, input);

   if (resultA is not null)
   {
       ctx.Import(attemptACtx);
       return resultA;
   }

   // Abordagem A falhou - tentar B (mensagens de A descartadas)
   var attemptBCtx = ctx.Clone();
   var resultB = TryApproachB(attemptBCtx, input);

   if (resultB is not null)
   {
       ctx.Import(attemptBCtx);
       return resultB;
   }

   // Ambas falharam - adicionar mensagem de erro consolidada
   ctx.AddErrorMessage("OPERATION_FAILED", "Both approaches failed");
   return null;
   ```

**Implementação do Import**: O método `Import(ExecutionContext other)` copia todas as mensagens e exceções do contexto de origem para o contexto de destino. As mensagens mantêm seus IDs originais, garantindo idempotência (importar o mesmo contexto duas vezes não duplica mensagens).

### Trade-offs (Com Perspectiva)

- **Mais parâmetros**: Todo método que precisa observar/registrar recebe ExecutionContext
  - **Mitigação**: É apenas um parâmetro. O benefício de clareza e testabilidade supera o "custo".

- **Propagação manual**: Precisa passar contexto de método em método
  - **Mitigação**: A propagação é explícita e rastreável. Com abordagens implícitas, a propagação também acontece, mas é invisível.

### Trade-offs Frequentemente Superestimados

**"É muito código passar contexto"**

Na prática, é uma linha extra por método:

```csharp
// Com abordagem implícita (parece menos código)
public Order? AddItem(AddItemInput input)

// Com parâmetro explícito (uma linha a mais)
public Order? AddItem(ExecutionContext ctx, AddItemInput input)

// A diferença é 1 parâmetro. O ganho é:
// - Testabilidade
// - Debugabilidade
// - Isolamento
// - Auto-documentação
```

**"Posso usar injeção no construtor"**

Injeção no construtor funciona para serviços, não para entidades:

```csharp
// Entidades são criadas de duas formas:
// 1. RegisterNew - precisa de contexto para a operação
// 2. CreateFromExistingInfo - NÃO deveria precisar de contexto

// Se contexto está no construtor, CreateFromExistingInfo precisa passar um
// contexto "fake" que não será usado - confuso e propenso a bugs
```

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre clareza:

> "The code should express the model clearly. [...] If the code is hard to understand, the model is probably confused."
>
> *O código deve expressar o modelo claramente. [...] Se o código é difícil de entender, o modelo provavelmente está confuso.*

Dependências implícitas obscurecem o modelo. Parâmetros explícitos expressam claramente o que o método precisa.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre dependências ocultas:

> "Hidden temporal couplings are bad. [...] Each function should make clear what it needs in its signature."
>
> *Acoplamentos temporais ocultos são ruins. [...] Cada função deve deixar claro o que precisa em sua assinatura.*

Service Locator e propriedades estáticas são acoplamentos ocultos - o método precisa que algo tenha sido configurado antes, mas isso não está na assinatura.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre testabilidade:

> "The architecture should make it easy to test the business rules without the UI, database, web server, or any other external element."
>
> *A arquitetura deve facilitar testar as regras de negócio sem a UI, banco de dados, servidor web, ou qualquer outro elemento externo.*

Abordagens implícitas acoplam o código à infraestrutura. Parâmetros explícitos permitem testes isolados.

### Princípio da Menor Surpresa

Bertrand Meyer em "Object-Oriented Software Construction" (1997):

> "A component should behave in a way that most users will expect it to behave."
>
> *Um componente deveria se comportar da forma que a maioria dos usuários espera que ele se comporte.*

Desenvolvedores esperam que os parâmetros de um método sejam suficientes para executá-lo. Dependências implícitas violam essa expectativa.

### Explicit Dependencies Principle

Mark Seemann em "Dependency Injection in .NET" (2011):

> "Classes should explicitly require any dependencies through their constructor or method parameters."
>
> *Classes devem explicitamente requisitar quaisquer dependências através de seus construtores ou parâmetros de método.*

`ExecutionContext` como parâmetro de método segue este princípio para entidades (que não podem usar injeção de construtor por causa de reconstitution).

## Antipadrões Documentados

### Antipadrão 1: Service Locator para Contexto

```csharp
// ❌ Resolve contexto via service locator
public Order? AddItem(AddItemInput input)
{
    var ctx = ServiceLocator.Get<ExecutionContext>();  // Dependência oculta
    ctx.AddInformationMessage("ITEM_ADDED", ...);
    // ...
}

// Problemas:
// - Assinatura não revela dependência
// - Entidade depende de infraestrutura (viola DE-027)
// - Difícil testar sem container configurado
```

### Antipadrão 2: Propriedade Estática Global

```csharp
// ❌ Contexto via propriedade estática
public static class Current
{
    public static ExecutionContext Context { get; set; }  // Estado global mutável
}

public Order? AddItem(AddItemInput input)
{
    Current.Context.AddInformationMessage(...);  // De onde veio?
}

// Problemas:
// - Estado global mutável
// - Testes precisam configurar/limpar
// - Testes paralelos podem interferir
```

### Antipadrão 3: Contexto Injetado no Construtor

```csharp
// ❌ ExecutionContext no construtor da entidade
public sealed class Order : EntityBase<Order>
{
    private readonly ExecutionContext _ctx;

    private Order(ExecutionContext ctx)
    {
        _ctx = ctx;
    }

    // CreateFromExistingInfo precisa passar contexto que não será usado
    public static Order CreateFromExistingInfo(
        ExecutionContext ctx,  // ❌ Por que reconstitution precisa disso?
        CreateFromExistingInfoInput input
    )
    {
        return new Order(ctx) { ... };  // ctx nunca será usado
    }
}
```

### Antipadrão 4: Contexto em Propriedade de Classe Base

```csharp
// ❌ Contexto como propriedade herdada
public abstract class EntityBase
{
    protected ExecutionContext? Context { get; set; }  // Quem configura?
}

public sealed class Order : EntityBase
{
    public Order? AddItem(AddItemInput input)
    {
        Context!.AddInformationMessage(...);  // Pode ser null!
    }
}

// Problemas:
// - Propriedade pode estar null
// - Não está claro quem/quando configura
// - Reconstitution não deveria ter contexto
```

### Antipadrão 5: Contexto via ThreadLocal/AsyncLocal

```csharp
// ❌ Ambient context via threading
public static class AmbientContext
{
    private static readonly AsyncLocal<ExecutionContext> _current = new();
    public static ExecutionContext Current
    {
        get => _current.Value!;
        set => _current.Value = value;
    }
}

public Order? AddItem(AddItemInput input)
{
    AmbientContext.Current.AddInformationMessage(...);
}

// Problemas:
// - Dependência oculta na assinatura
// - Bugs sutis em cenários async complexos
// - Testes precisam configurar ThreadLocal
```

## Decisões Relacionadas

- [DE-027](./DE-027-entidades-nao-tem-dependencias-externas.md) - Entidades não têm dependências externas (nem service locator)
- [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) - Separação RegisterNew vs CreateFromExistingInfo (contexto só no RegisterNew)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Implementa o padrão de passagem explícita como primeiro parâmetro, centralizando mensagens e metadados de execução |
| [CustomTimeProvider](../../building-blocks/core/time-providers/custom-time-provider.md) | TimeProvider customizável encapsulado no ExecutionContext, permitindo testes determinísticos |

## Leitura Recomendada

- [Clean Code - Robert C. Martin](https://blog.cleancoder.com/)
- [Dependency Injection in .NET - Mark Seemann](https://www.manning.com/books/dependency-injection-in-dot-net)
- [Explicit Dependencies Principle](https://deviq.com/principles/explicit-dependencies-principle)
- [Service Locator is an Anti-Pattern - Mark Seemann](https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/)

## Referências no Código

- [ExecutionContext.cs](../../../src/BuildingBlocks/Core/ExecutionContexts/ExecutionContext.cs) - Implementação do ExecutionContext
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - RegisterNew com ExecutionContext como primeiro parâmetro
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Todos os métodos públicos seguem o padrão de ExecutionContext como primeiro parâmetro
