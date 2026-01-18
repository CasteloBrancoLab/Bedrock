# DE-029: TimeProvider Encapsulado no ExecutionContext

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **cartório** com dois modelos de carimbo de data/hora:

**Modelo "relógio de parede" (dependência direta)**:
- Cada funcionário olha o relógio da parede quando precisa da hora
- Relógios diferentes em salas diferentes podem ter horas diferentes
- Impossível reproduzir um documento "como se fosse às 14h de ontem"
- Testes do sistema exigem manipular o relógio físico

**Modelo "carimbo oficial" (encapsulado)**:
- O carimbo de data/hora vem do protocolo de entrada
- Todos os documentos daquela operação usam o mesmo timestamp
- Fácil simular: "este protocolo foi aberto às 14h de ontem"
- Testes podem usar qualquer data/hora sem afetar o sistema real

O `TimeProvider` encapsulado no `ExecutionContext` é como o carimbo oficial - uma fonte centralizada e testável de tempo para toda a operação.

---

### O Problema Técnico

Usar `DateTime.Now` ou `DateTimeOffset.Now` diretamente no código cria diversos problemas:

```csharp
// ❌ ANTIPATTERN: DateTime.Now direto no código
public sealed class Order : EntityBase<Order>
{
    public DateTimeOffset CreatedAt { get; private set; }

    public static Order? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        // Pega hora do sistema - não testável
        var now = DateTimeOffset.Now;

        return new Order
        {
            CreatedAt = now,
            // ...
        };
    }

    public Order? Ship(ExecutionContext ctx)
    {
        // Outra chamada a DateTime.Now - pode ser diferente!
        var shippedAt = DateTimeOffset.Now;

        // Se RegisterNew e Ship executam na mesma operação,
        // os timestamps podem diferir por milissegundos
        // ...
    }
}
```

**Problemas graves**:

1. **Não testável**: Como testar lógica que depende de "agora"?
   ```csharp
   [Fact]
   public void Order_ShouldExpireAfter30Days()
   {
       var order = Order.RegisterNew(ctx, input);

       // Como simular que 30 dias se passaram?
       // Não posso mudar DateTime.Now!

       Assert.True(order.IsExpired);  // Sempre false
   }
   ```

2. **Timestamps inconsistentes**: Múltiplas chamadas retornam valores diferentes
   ```csharp
   var order = Order.RegisterNew(ctx, input);     // 10:30:00.123
   order = order.AddItem(ctx, item);              // 10:30:00.456
   order = order.Ship(ctx);                       // 10:30:00.789

   // Qual é o "momento" real da operação?
   // Cada passo tem timestamp diferente
   ```

3. **Bugs dependentes de timezone/horário**:
   ```csharp
   // Código funciona em dev, falha em produção (timezone diferente)
   var today = DateTime.Now.Date;
   var isBusinessDay = today.DayOfWeek != DayOfWeek.Sunday;

   // Servidor em UTC, usuário em GMT-3
   // "Hoje" é diferente dependendo de onde olha
   ```

4. **Impossível reproduzir cenários**:
   ```csharp
   // Bug reportado: "pedido criado às 23:59 não aparece no relatório do dia"
   // Como reproduzir? Esperar até 23:59?
   ```

---

### Como Normalmente é Feito (e Por Que Não é Ideal)

**Opção 1: Interface IClock injetada**
```csharp
// ❌ Clock injetado em cada classe
public sealed class Order : EntityBase<Order>
{
    private readonly IClock _clock;

    public Order(IClock clock)
    {
        _clock = clock;
    }

    public static Order? RegisterNew(IClock clock, RegisterNewInput input)
    {
        var now = clock.GetCurrentTime();
        // ...
    }
}
```

**Problemas**:
- Mais um parâmetro em cada método/construtor
- Entidade depende de serviço externo (viola DE-027)
- Reconstitution precisa de clock que não será usado

**Opção 2: Propriedade estática substituível**
```csharp
// ❌ Clock via propriedade estática
public static class SystemClock
{
    public static Func<DateTimeOffset> Now { get; set; } = () => DateTimeOffset.Now;
}

public sealed class Order : EntityBase<Order>
{
    public static Order? RegisterNew(RegisterNewInput input)
    {
        var now = SystemClock.Now();  // Estado global
        // ...
    }
}
```

**Problemas**:
- Estado global mutável
- Testes paralelos interferem entre si
- Não está claro qual clock está sendo usado

## A Decisão

### Nossa Abordagem

O `TimeProvider` é uma propriedade do `ExecutionContext`, disponível para toda a operação:

```csharp
public class ExecutionContext
{
    // TimeProvider encapsulado - uma fonte de tempo para toda a operação
    public TimeProvider TimeProvider { get; }

    // Timestamp da criação do contexto - momento em que a operação iniciou
    public DateTimeOffset Timestamp { get; }

    public static ExecutionContext Create(
        Guid correlationId,
        TenantInfo tenantInfo,
        string executionUser,
        MessageType minimumMessageType,
        TimeProvider timeProvider  // ✅ TimeProvider passado na criação
    )
    {
        return new ExecutionContext(
            timestamp: timeProvider.GetUtcNow(),  // Captura o momento da criação
            // ...
            timeProvider: timeProvider
        );
    }
}
```

### Como Usar

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
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
                // ✅ TimeProvider vem do contexto
                var now = ctx.TimeProvider.GetUtcNow();

                // Ou usar o Timestamp do contexto (momento da criação)
                var operationStart = ctx.Timestamp;

                return
                    instance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
                    & instance.ChangeBirthDateInternal(ctx, inp.BirthDate);
            }
        );
    }
}
```

### Timestamp vs TimeProvider.GetUtcNow()

O `ExecutionContext` oferece duas formas de obter tempo:

| Propriedade | Descrição | Quando Usar |
|-------------|-----------|-------------|
| `Timestamp` | Momento em que o contexto foi criado | Auditoria, "quando a operação começou" |
| `TimeProvider.GetUtcNow()` | Momento atual (pode mudar) | Cálculos que precisam do "agora real" |

```csharp
// Timestamp - fixo para toda a operação
var ctx = ExecutionContext.Create(..., timeProvider);
// ctx.Timestamp capturado no momento da criação

await Task.Delay(5000);  // 5 segundos depois

// ctx.Timestamp ainda é o mesmo (momento da criação)
// ctx.TimeProvider.GetUtcNow() avançou 5 segundos
```

**Cenários de uso**:

```csharp
// ✅ Auditoria - usar Timestamp (consistente para toda operação)
entity.CreatedAt = ctx.Timestamp;
entity.ModifiedAt = ctx.Timestamp;

// ✅ Expiração relativa ao início - usar Timestamp
var expiresAt = ctx.Timestamp.AddHours(24);

// ✅ Tempo real decorrido - usar TimeProvider
var elapsed = ctx.TimeProvider.GetUtcNow() - ctx.Timestamp;
if (elapsed > TimeSpan.FromMinutes(5))
    ctx.AddWarningMessage("SLOW_OPERATION", "Operation taking too long");
```

### Testabilidade

```csharp
[Fact]
public void RegisterNew_ShouldSetCreatedAtFromContext()
{
    // Arrange - TimeProvider fake com data específica
    var fakeTime = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero);
    var fakeTimeProvider = new FakeTimeProvider(fakeTime);

    var ctx = ExecutionContext.Create(
        correlationId: Guid.NewGuid(),
        tenantInfo: TenantInfo.Default,
        executionUser: "test-user",
        executionOrigin: "Test",
        minimumMessageType: MessageType.Trace,
        timeProvider: fakeTimeProvider
    );

    // Act
    var entity = SimpleAggregateRoot.RegisterNew(ctx, validInput);

    // Assert - timestamps são previsíveis
    entity.Should().NotBeNull();
    entity!.EntityInfo.CreatedAt.Should().Be(fakeTime);
    ctx.Timestamp.Should().Be(fakeTime);
}

[Fact]
public void Order_ShouldExpireAfter30Days()
{
    // Arrange - data específica
    var orderDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    var fakeTimeProvider = new FakeTimeProvider(orderDate);

    var ctx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);
    var order = Order.RegisterNew(ctx, input);

    // Act - avançar 31 dias
    fakeTimeProvider.Advance(TimeSpan.FromDays(31));

    // Assert - agora está expirado
    var checkCtx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);
    order!.IsExpired(checkCtx).Should().BeTrue();
}

[Fact]
public void Report_ShouldIncludeOrdersFromSpecificDate()
{
    // Arrange - simular "ontem às 23:59"
    var yesterday2359 = new DateTimeOffset(2025, 6, 14, 23, 59, 0, TimeSpan.Zero);
    var fakeTimeProvider = new FakeTimeProvider(yesterday2359);

    var ctx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);
    var order = Order.RegisterNew(ctx, input);

    // Assert - ordem criada "ontem" às 23:59
    order!.CreatedAt.Date.Should().Be(new DateTime(2025, 6, 14));
}
```

### Cobertura de Condições de Borda (100% Coverage)

Com `TimeProvider` controlável, é possível testar **todos os ramos** de condições temporais - algo impossível com `DateTime.Now`:

```csharp
// Código de produção
public bool IsExpired(ExecutionContext ctx)
{
    return ctx.TimeProvider.GetUtcNow() >= ExpiresAt;  // >= tem dois casos: > e ==
}

// ❌ Com DateTime.Now - impossível testar o caso "exatamente igual"
// Como garantir que DateTime.Now seja EXATAMENTE igual a ExpiresAt?

// ✅ Com TimeProvider - 100% coverage possível
[Fact]
public void IsExpired_WhenBeforeExpiration_ShouldReturnFalse()
{
    var expiresAt = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
    var order = CreateOrderWithExpiration(expiresAt);

    // 1 segundo ANTES da expiração
    var beforeExpiration = expiresAt.AddSeconds(-1);
    var fakeTimeProvider = new FakeTimeProvider(beforeExpiration);
    var ctx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);

    order.IsExpired(ctx).Should().BeFalse();  // < caso
}

[Fact]
public void IsExpired_WhenExactlyAtExpiration_ShouldReturnTrue()
{
    var expiresAt = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
    var order = CreateOrderWithExpiration(expiresAt);

    // EXATAMENTE no momento da expiração
    var fakeTimeProvider = new FakeTimeProvider(expiresAt);
    var ctx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);

    order.IsExpired(ctx).Should().BeTrue();  // == caso (impossível sem fake!)
}

[Fact]
public void IsExpired_WhenAfterExpiration_ShouldReturnTrue()
{
    var expiresAt = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
    var order = CreateOrderWithExpiration(expiresAt);

    // 1 segundo APÓS expiração
    var afterExpiration = expiresAt.AddSeconds(1);
    var fakeTimeProvider = new FakeTimeProvider(afterExpiration);
    var ctx = ExecutionContext.Create(..., timeProvider: fakeTimeProvider);

    order.IsExpired(ctx).Should().BeTrue();  // > caso
}
```

**Por que isso importa**:
- Condições `<=`, `>=`, `<`, `>` têm comportamentos diferentes no limite
- Bugs de "off-by-one" em tempo são comuns e difíceis de reproduzir
- Com `FakeTimeProvider`, você testa EXATAMENTE o momento crítico
- 100% de cobertura em lógica temporal é alcançável

### Consistência em Operações Longas

```csharp
public async Task<ImportResult> ImportLargeFile(
    ExecutionContext ctx,
    Stream fileStream
)
{
    // Timestamp do contexto é consistente para toda a importação
    var importStartedAt = ctx.Timestamp;

    var results = new List<ImportedRecord>();

    await foreach (var record in ParseRecords(fileStream))
    {
        // Todos os registros marcados com o mesmo timestamp de importação
        var entity = Entity.RegisterNew(ctx, new RegisterNewInput(record));

        if (entity is not null)
        {
            // entity.CreatedAt será ctx.Timestamp (não o momento da iteração)
            results.Add(new ImportedRecord(entity, importStartedAt));
        }
    }

    // Tempo real decorrido (para métricas)
    var elapsed = ctx.TimeProvider.GetUtcNow() - ctx.Timestamp;
    ctx.AddInformationMessage("IMPORT_COMPLETE", $"Imported {results.Count} in {elapsed}");

    return new ImportResult(results, importStartedAt, elapsed);
}
```

### Fluxo de Propagação

```
+-------------------------------------------------------------------------+
│                         APPLICATION LAYER                               │
│                                                                         │
│  // Em produção: TimeProvider.System                                    │
│  var ctx = ExecutionContext.Create(                                     │
│      ...,                                                               │
│      timeProvider: TimeProvider.System                                  │
│  );                                                                     │
│                                                                         │
│  // Em testes: FakeTimeProvider                                         │
│  var ctx = ExecutionContext.Create(                                     │
│      ...,                                                               │
│      timeProvider: new FakeTimeProvider(specificDate)                   │
│  );                                                                     │
+-------------------------------------------------------------------------+
                                    │
                                    ▼
+-------------------------------------------------------------------------+
│                           DOMAIN ENTITY                                 │
│                                                                         │
│  // Entidade não sabe se é produção ou teste                            │
│  // Apenas usa ctx.TimeProvider ou ctx.Timestamp                        │
│  var now = ctx.TimeProvider.GetUtcNow();                                │
│  var operationStart = ctx.Timestamp;                                    │
+-------------------------------------------------------------------------+
```

### Benefícios

1. **Testabilidade total**: Qualquer cenário de tempo pode ser simulado
   ```csharp
   var fakeTime = new FakeTimeProvider(new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero));
   // Testar virada de ano, fuso horário, etc.
   ```

2. **Consistência**: Toda a operação usa a mesma referência de tempo
   ```csharp
   // Todos os registros da importação têm o mesmo CreatedAt
   // Não há "drift" de milissegundos entre operações
   ```

3. **Centralização**: Uma única fonte de verdade para tempo
   ```csharp
   // Não há dúvida sobre qual "agora" usar
   // Sempre ctx.TimeProvider ou ctx.Timestamp
   ```

4. **Sem dependência direta**: Entidades não dependem de DateTime.Now
   ```csharp
   // Entidade é pura - recebe tempo via contexto
   // Facilita testes e raciocínio sobre o código
   ```

5. **Compatibilidade com .NET 8+**: Usa a abstração `TimeProvider` do .NET
   ```csharp
   // TimeProvider.System em produção
   // FakeTimeProvider em testes (Microsoft.Extensions.TimeProvider.Testing)
   ```

### Trade-offs (Com Perspectiva)

- **Indireção**: Precisa acessar via `ctx.TimeProvider` em vez de `DateTime.Now`
  - **Mitigação**: A indireção é mínima e o benefício de testabilidade é enorme.

- **Dois conceitos**: `Timestamp` vs `TimeProvider.GetUtcNow()` pode confundir
  - **Mitigação**: Documentação clara. Regra simples: `Timestamp` para auditoria, `GetUtcNow()` para cálculos de "agora real".

### Trade-offs Frequentemente Superestimados

**"DateTime.Now é mais simples"**

Mais simples de escrever, mais difícil de testar:

```csharp
// "Simples" - mas como testar expiração de 30 dias?
if (DateTime.Now > order.CreatedAt.AddDays(30))
    return OrderStatus.Expired;

// Testável - posso simular qualquer data
if (ctx.TimeProvider.GetUtcNow() > order.CreatedAt.AddDays(30))
    return OrderStatus.Expired;
```

**"Não preciso testar lógica de tempo"**

Bugs de tempo são comuns e difíceis de reproduzir:
- Pedido "desaparece" na virada do dia
- Relatório mostra dados errados em timezones diferentes
- Promoção expira "antes da hora" em alguns servidores

Com `TimeProvider` testável, esses cenários são triviais de verificar.

## Fundamentação Teórica

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre dependências:

> "Depend on abstractions, not on concretions."
>
> *Dependa de abstrações, não de implementações concretas.*

`DateTime.Now` é uma implementação concreta. `TimeProvider` é uma abstração que pode ser substituída.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre testabilidade:

> "The architecture should make it easy to test the business rules without the UI, database, web server, or any other external element."
>
> *A arquitetura deve facilitar testar as regras de negócio sem a UI, banco de dados, servidor web, ou qualquer outro elemento externo.*

O relógio do sistema é um "elemento externo". Encapsular em `TimeProvider` permite testar sem depender do relógio real.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre serviços:

> "When a significant process or transformation in the domain is not a natural responsibility of an ENTITY or VALUE OBJECT, add an operation to the model as a standalone interface declared as a SERVICE."
>
> *Quando um processo ou transformação significativo no domínio não é uma responsabilidade natural de uma ENTITY ou VALUE OBJECT, adicione uma operação ao modelo como uma interface standalone declarada como SERVICE.*

Obter o tempo atual não é responsabilidade da entidade. É um serviço que deve ser fornecido externamente (via contexto).

### Dependency Injection Principle

Mark Seemann em "Dependency Injection in .NET" (2011):

> "Any volatile dependency should be injected. Time is volatile - it changes every millisecond."
>
> *Qualquer dependência volátil deve ser injetada. Tempo é volátil - muda a cada milissegundo.*

`TimeProvider` no `ExecutionContext` é a injeção da dependência de tempo.

## Antipadrões Documentados

### Antipadrão 1: DateTime.Now Direto

```csharp
// ❌ Dependência direta do relógio do sistema
public sealed class Order : EntityBase<Order>
{
    public static Order? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        var now = DateTime.Now;  // Não testável!
        // ...
    }
}
```

### Antipadrão 2: DateTimeOffset.UtcNow Direto

```csharp
// ❌ Mesmo problema, só muda o tipo
public sealed class Order : EntityBase<Order>
{
    public bool IsExpired()
    {
        return DateTimeOffset.UtcNow > ExpiresAt;  // Não testável!
    }
}
```

### Antipadrão 3: Clock Injetado na Entidade

```csharp
// ❌ Entidade depende de serviço (viola DE-027)
public sealed class Order : EntityBase<Order>
{
    private readonly IClock _clock;

    public Order(IClock clock)
    {
        _clock = clock;  // Dependência externa em entidade!
    }
}
```

### Antipadrão 4: Clock Estático Global

```csharp
// ❌ Estado global mutável
public static class SystemClock
{
    public static Func<DateTimeOffset> Now { get; set; } = () => DateTimeOffset.UtcNow;
}

// Testes paralelos podem interferir
SystemClock.Now = () => fakeTime;  // Afeta todos os testes rodando!
```

### Antipadrão 5: Ignorar TimeProvider do Contexto

```csharp
// ❌ Ignora o TimeProvider disponível no contexto
public static Order? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
{
    // ctx.TimeProvider está disponível, mas código ignora
    var now = DateTimeOffset.UtcNow;  // Por que não usar ctx.TimeProvider?
    // ...
}
```

## Decisões Relacionadas

- [DE-027](./DE-027-entidades-nao-tem-dependencias-externas.md) - Entidades não têm dependências externas (TimeProvider vem via contexto, não injetado)
- [DE-028](./DE-028-executioncontext-explicito.md) - ExecutionContext explícito (TimeProvider é parte do contexto)

## Building Blocks Relacionados

- **[CustomTimeProvider](../../building-blocks/core/time-providers/custom-time-provider.md)** - Documentação completa sobre o TimeProvider customizável do Bedrock, incluindo uso em testes, benchmarks de performance e exemplos práticos.

## Leitura Recomendada

- [TimeProvider - .NET 8 Abstraction](https://learn.microsoft.com/en-us/dotnet/api/system.timeprovider)
- [FakeTimeProvider for Testing](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.time.testing.faketimeprovider)
- [Clean Code - Robert C. Martin](https://blog.cleancoder.com/)
- [Dependency Injection in .NET - Mark Seemann](https://www.manning.com/books/dependency-injection-in-dot-net)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Encapsula o TimeProvider, fornecendo acesso centralizado e testável ao tempo |
| [CustomTimeProvider](../../building-blocks/core/time-providers/custom-time-provider.md) | Implementação customizável de TimeProvider que é encapsulada no ExecutionContext |

## Referências no Código

- [ExecutionContext.cs](../../../src/BuildingBlocks/Core/ExecutionContexts/ExecutionContext.cs) - propriedade TimeProvider
- [ExecutionContext.cs](../../../src/BuildingBlocks/Core/ExecutionContexts/ExecutionContext.cs) - propriedade Timestamp
- [ExecutionContext.cs](../../../src/BuildingBlocks/Core/ExecutionContexts/ExecutionContext.cs) - método Create usando TimeProvider
