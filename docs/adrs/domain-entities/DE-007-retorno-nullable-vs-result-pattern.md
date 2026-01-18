# DE-007: Retorno Nullable vs Result Pattern

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine dois restaurantes que lidam com pedidos de formas diferentes:

**Restaurante A (Result Pattern)**: O garçom traz um envelope lacrado com cada pedido. Dentro do envelope pode haver o prato ou uma lista de problemas. Você precisa abrir o envelope, verificar o tipo de conteúdo, e então agir. Se quiser pedir vários pratos, cada um vem em seu próprio envelope.

**Restaurante B (Nullable + Context)**: O garçom traz o prato diretamente se estiver pronto, ou volta de mãos vazias. Se voltar sem o prato, você consulta o quadro de avisos da mesa (ExecutionContext) que lista todos os problemas de todos os pedidos de uma vez.

O Result Pattern adiciona uma camada de "envelope" que, em muitos casos, só adiciona cerimônia sem benefício real - especialmente quando já existe um local centralizado para mensagens.

### O Problema Técnico

O Result Pattern é popular em programação funcional e tem seus méritos. Porém, em C# com nosso design de ExecutionContext, ele introduz complexidades:

```csharp
// Com Result Pattern
public Result<SimpleAggregateRoot> RegisterNew(RegisterNewInput input)
{
    // ... validações ...
    if (!isValid)
        return Result<SimpleAggregateRoot>.Failure(errors);

    return Result<SimpleAggregateRoot>.Success(instance);
}

// Uso - requer unwrapping
var result = SimpleAggregateRoot.RegisterNew(input);

// Opção 1: Match (força callbacks)
return result.Match(
    onSuccess: entity => ProcessEntity(entity),
    onFailure: errors => HandleErrors(errors)
);

// Opção 2: Verificação explícita
if (result.IsSuccess)
{
    var entity = result.Value; // Pode lançar se IsSuccess for false
    // ...
}
else
{
    var errors = result.Errors;
    // ...
}
```

```csharp
// Com Nullable + ExecutionContext
public static SimpleAggregateRoot? RegisterNew(
    ExecutionContext executionContext,
    RegisterNewInput input
)
{
    // ... validações adicionam mensagens ao context ...
    if (!isValid)
        return null;

    return instance;
}

// Uso - direto e familiar
var entity = SimpleAggregateRoot.RegisterNew(context, input);

if (entity == null)
{
    return BadRequest(context.Messages);
}

// entity é garantidamente não-null aqui
await _repository.Save(entity);
```

## Como Normalmente é Feito

### Abordagem Tradicional

O Result Pattern é amplamente adotado, especialmente em projetos influenciados por programação funcional:

```csharp
// Result Pattern típico
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public IReadOnlyList<string> Errors { get; }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());
    public static Result<T> Failure(params string[] errors) => new(false, default!, errors);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<IReadOnlyList<string>, TResult> onFailure
    ) => IsSuccess ? onSuccess(Value) : onFailure(Errors);
}

// Uso
public Result<Order> CreateOrder(CreateOrderInput input)
{
    var customerResult = _customerService.GetCustomer(input.CustomerId);
    if (!customerResult.IsSuccess)
        return Result<Order>.Failure(customerResult.Errors.ToArray());

    var customer = customerResult.Value;

    var productResult = _productService.GetProduct(input.ProductId);
    if (!productResult.IsSuccess)
        return Result<Order>.Failure(productResult.Errors.ToArray());

    var product = productResult.Value;

    // Finalmente criar o pedido...
    var order = new Order(customer, product);
    return Result<Order>.Success(order);
}
```

### Por Que Não Funciona Bem Para Nosso Caso

1. **Incompatibilidade com `yield return` e `IAsyncEnumerable<T>`**:

```csharp
// ? Impossível com Result Pattern
public IAsyncEnumerable<Order> ProcessOrdersAsync(IEnumerable<OrderInput> inputs)
{
    foreach (var input in inputs)
    {
        var result = CreateOrder(input);
        if (result.IsSuccess)
            yield return result.Value; // ? Não compila: yield não funciona com callbacks
    }
}

// ? Funciona com nullable
public async IAsyncEnumerable<Order> ProcessOrdersAsync(
    ExecutionContext context,
    IEnumerable<OrderInput> inputs
)
{
    foreach (var input in inputs)
    {
        var order = CreateOrder(context, input);
        if (order != null)
            yield return order; // ? Compila normalmente
    }
}
```

2. **Closures implícitas em Match**:

```csharp
// Result Pattern força callbacks que capturam contexto
var result = CreateOrder(input);

return result.Match(
    onSuccess: order => {
        // Closure captura 'context', '_repository', '_logger'
        _logger.LogInformation("Order {Id} created", order.Id);
        return _repository.SaveAsync(order, context);
    },
    onFailure: errors => {
        // Outra closure captura 'context', '_logger'
        _logger.LogWarning("Order creation failed: {Errors}", errors);
        return Task.FromResult(BadRequest(errors));
    }
);
```

Muitos desenvolvedores, acostumados com LINQ, criam closures sem entender:
- Alocações de objetos para cada closure
- Captura de variáveis (podem mudar depois!)
- Lifetime estendido de objetos capturados
- Dificuldade de debug (stack traces confusos)

3. **Redundância com ExecutionContext**:

```csharp
// Com Result Pattern + ExecutionContext: redundância
public Result<Order> CreateOrder(ExecutionContext context, OrderInput input)
{
    if (!ValidateName(context, input.Name)) // Adiciona ao context
        return Result<Order>.Failure("Name invalid"); // Duplica no Result

    // Onde estão os erros? No Result? No context? Em ambos?
}
```

O ExecutionContext já existe para coletar mensagens. Result Pattern duplicaria essa responsabilidade.

4. **Combinação de múltiplos Results em Application Services**:

Em cenários reais, Application Services orquestram múltiplos Domain Services. Com Result Pattern, a combinação de resultados se torna verbosa e propensa a erros:

```csharp
// Com Result Pattern - combinação manual e verbosa
public async Task<Result<SaleConfirmation>> ProcessSaleAsync(SaleInput input)
{
    // Cada chamada retorna Result<T> - precisa combinar manualmente
    var customerResult = await _customerService.GetCustomerAsync(input.CustomerId);
    if (!customerResult.IsSuccess)
        return Result<SaleConfirmation>.Failure(customerResult.Errors);

    var inventoryResult = await _inventoryService.ReserveStockAsync(input.ProductId, input.Quantity);
    if (!inventoryResult.IsSuccess)
        return Result<SaleConfirmation>.Failure(inventoryResult.Errors);

    var pricingResult = await _pricingService.CalculatePriceAsync(input.ProductId, input.Quantity);
    if (!pricingResult.IsSuccess)
        return Result<SaleConfirmation>.Failure(pricingResult.Errors);

    var paymentResult = await _paymentService.ProcessPaymentAsync(
        customerResult.Value,
        pricingResult.Value.Total
    );
    if (!paymentResult.IsSuccess)
    {
        // Oops! Precisa reverter o estoque reservado
        await _inventoryService.ReleaseStockAsync(inventoryResult.Value.ReservationId);
        return Result<SaleConfirmation>.Failure(paymentResult.Errors);
    }

    // E se quiser combinar TODOS os erros de uma vez?
    // Precisa criar lógica adicional de agregação...
}
```

```csharp
// Com ExecutionContext - combinação natural
public async Task<SaleConfirmation?> ProcessSaleAsync(
    ExecutionContext context,
    SaleInput input
)
{
    // Todas as operações adicionam mensagens ao MESMO context
    var customer = await _customerService.GetCustomerAsync(context, input.CustomerId);
    var inventory = await _inventoryService.ReserveStockAsync(context, input.ProductId, input.Quantity);
    var pricing = await _pricingService.CalculatePriceAsync(context, input.ProductId, input.Quantity);

    // Pode verificar erros de TODAS as operações de uma vez
    if (context.HasErrors)
        return null; // Todas as mensagens já estão no context

    var payment = await _paymentService.ProcessPaymentAsync(context, customer, pricing.Total);

    if (payment == null)
    {
        await _inventoryService.ReleaseStockAsync(context, inventory.ReservationId);
        return null;
    }

    return new SaleConfirmation(customer, inventory, pricing, payment);
}
```

Com ExecutionContext, não há necessidade de "combinar" Results - todas as mensagens fluem naturalmente para o mesmo local.

5. **Result Pattern assume apenas erros - domínio precisa de múltiplos tipos de mensagem**:

O Result Pattern típico modela apenas dois estados: sucesso ou falha (erros). Na prática, o domínio precisa comunicar **múltiplos tipos de mensagem**:

```csharp
// Result Pattern: binário (sucesso/erro)
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public IReadOnlyList<string> Errors { get; } // Apenas erros!
}
```

Mas o domínio conhece regras de negócio que geram **warnings**, **informações** e **sugestões** - não apenas erros:

```csharp
// Cenário real: venda aprovada COM warnings
public Sale? RegisterSale(ExecutionContext context, RegisterSaleInput input)
{
    var customer = GetCustomer(context, input.CustomerId);
    if (customer == null)
        return null; // Erro: cliente não encontrado

    // Regra de negócio: cliente com cadastro desatualizado
    // NÃO impede a venda, mas gera WARNING
    if (customer.LastUpdateAt < context.TimeProvider.Now.AddDays(-180))
    {
        context.AddMessage(new Message(
            MessageType.Warning,
            "CUSTOMER_OUTDATED",
            $"Cadastro do cliente {customer.Name} está desatualizado há mais de 180 dias. " +
            "Considere solicitar atualização dos dados."
        ));
    }

    // Regra de negócio: primeira compra do cliente
    // Gera INFO para o vendedor
    if (customer.TotalPurchases == 0)
    {
        context.AddMessage(new Message(
            MessageType.Info,
            "FIRST_PURCHASE",
            $"Esta é a primeira compra do cliente {customer.Name}. " +
            "Considere oferecer desconto de boas-vindas."
        ));
    }

    // Regra de negócio: produto próximo do vencimento
    // Gera WARNING mas não impede venda
    var product = GetProduct(context, input.ProductId);
    if (product?.ExpirationDate < context.TimeProvider.Now.AddDays(30))
    {
        context.AddMessage(new Message(
            MessageType.Warning,
            "PRODUCT_NEAR_EXPIRATION",
            $"Produto {product.Name} vence em menos de 30 dias. " +
            "Verifique se o cliente está ciente."
        ));
    }

    // Venda é VÁLIDA (retorna não-null), mas com warnings
    var sale = CreateSaleInternal(context, customer, product, input);
    return sale;
}
```

**No controller, o tratamento é natural**:

```csharp
[HttpPost]
public async Task<IActionResult> CreateSale(CreateSaleRequest request)
{
    var context = new ExecutionContext(_timeProvider);

    var sale = await _saleService.RegisterSale(context, request.ToInput());

    if (sale == null)
    {
        // Erros impedem a operação
        return BadRequest(context.GetMessages(MessageType.Error));
    }

    // Sucesso! Mas pode ter warnings/infos para o frontend
    return Ok(new CreateSaleResponse
    {
        SaleId = sale.Id,
        Warnings = context.GetMessages(MessageType.Warning),
        Infos = context.GetMessages(MessageType.Info)
    });
}
```

**Com Result Pattern, isso seria extremamente complicado**:

```csharp
// Result Pattern: como modelar sucesso COM warnings?
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public IReadOnlyList<string> Errors { get; }
    public IReadOnlyList<string> Warnings { get; }  // Adicionar?
    public IReadOnlyList<string> Infos { get; }     // Adicionar?

    // E se precisar de novos tipos no futuro?
    // Suggestions? Deprecations? Audit?
}

// Ou criar Result separado para warnings?
public class ResultWithWarnings<T> { ... }

// Ou usar tupla?
public (Result<Sale> Result, List<Warning> Warnings) RegisterSale(...) { ... }
```

O Result Pattern foi projetado para **sucesso/falha binário**. O ExecutionContext foi projetado para **coletar mensagens de qualquer tipo** - muito mais flexível para regras de negócio reais.

## A Decisão

### Nossa Abordagem

Métodos de factory e modificação retornam `T?` (nullable), com mensagens coletadas no `ExecutionContext`:

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        var instance = new SimpleAggregateRoot();

        bool isSuccess =
            instance.ChangeNameInternal(executionContext, input.FirstName, input.LastName)
            & instance.ChangeBirthDateInternal(executionContext, input.BirthDate);

        // Null = falhou, mensagens no context
        // Não-null = sucesso, instância válida
        return isSuccess ? instance : null;
    }

    public SimpleAggregateRoot? ChangeName(
        ExecutionContext executionContext,
        ChangeNameInput input
    )
    {
        // Clone-Modify-Return também usa nullable
        return RegisterChangeInternal(
            executionContext,
            instance: this,
            input,
            handler: (ctx, inp, clone) =>
                clone.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
        );
    }
}
```

**Uso em Controller**:

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreatePersonRequest request)
{
    var context = new ExecutionContext(_timeProvider);

    var person = SimpleAggregateRoot.RegisterNew(
        context,
        new RegisterNewInput(request.FirstName, request.LastName, request.BirthDate)
    );

    if (person == null)
    {
        // Todas as mensagens de erro estão no context
        return BadRequest(context.Messages);
    }

    // person é garantidamente válido (não-null)
    await _repository.SaveAsync(person);
    return Created($"/persons/{person.Id}", person);
}
```

### Por Que Funciona Melhor

1. **Compatível com generators e async streams**:

```csharp
public async IAsyncEnumerable<SimpleAggregateRoot> ImportPersonsAsync(
    ExecutionContext context,
    IAsyncEnumerable<PersonDto> dtos
)
{
    await foreach (var dto in dtos)
    {
        var person = SimpleAggregateRoot.RegisterNew(context, dto.ToInput());
        if (person != null)
            yield return person;
        // Erros já estão no context para processamento posterior
    }
}
```

2. **Sem closures desnecessárias**:

```csharp
// Fluxo linear, sem callbacks
var person = SimpleAggregateRoot.RegisterNew(context, input);

if (person == null)
    return BadRequest(context.Messages);

_logger.LogInformation("Person {Id} created", person.Id);
await _repository.SaveAsync(person);
return Created(...);
```

3. **Single source of truth para mensagens**:

```csharp
// Todas as mensagens em um só lugar
var context = new ExecutionContext();

var person = SimpleAggregateRoot.RegisterNew(context, personInput);
var address = Address.Create(context, addressInput);
var phone = Phone.Create(context, phoneInput);

if (context.HasErrors)
{
    // TODAS as mensagens de TODAS as operações
    return BadRequest(context.Messages);
}
```

4. **Null-safety nativo do C#**:

```csharp
var person = SimpleAggregateRoot.RegisterNew(context, input);

// Compilador avisa se tentar usar sem null-check
person.FirstName; // ?? Warning: possible null reference

if (person != null)
{
    person.FirstName; // ? OK - compilador sabe que não é null
}

// Ou com pattern matching
if (person is { } validPerson)
{
    validPerson.FirstName; // ? OK
}
```

## Consequências

### Benefícios

- **Compatibilidade com yield/IAsyncEnumerable**: Sem callbacks, funciona com generators
- **Menos alocações**: Sem objetos Result para cada operação
- **Código linear**: Fluxo de leitura top-to-bottom, sem callbacks
- **Single source of truth**: ExecutionContext centraliza todas as mensagens
- **Familiar**: Desenvolvedores C# já conhecem nullable reference types
- **Combinação natural em Application Services**: Múltiplos serviços contribuem para o mesmo context
- **Múltiplos tipos de mensagem**: Suporta Error, Warning, Info, etc. - não apenas erros

### Trade-offs (Com Perspectiva)

- **Null não carrega informação**: O null não diz "por quê" falhou
- **Requer disciplina**: Precisa sempre consultar ExecutionContext após null

### Trade-offs Frequentemente Superestimados

**"Null é ambíguo - pode ser 'não encontrado' ou 'inválido'"**

Na verdade, o significado do null é naturalmente determinado pelo **contexto semântico da operação**:

```csharp
// OPERAÇÕES DE MUDANÇA DE ESTADO (Create, Update, Delete)
// Null = operação não foi realizada (validação falhou)
var person = SimpleAggregateRoot.RegisterNew(context, input);
if (person == null) // Operação não realizada ? consultar context.Messages

var updated = person.ChangeName(context, newNameInput);
if (updated == null) // Mudança não aplicada ? consultar context.Messages

// OPERAÇÕES DE LEITURA/RECONSTITUIÇÃO (Get, Find, Load)
// Null = não encontrado (não é erro, é ausência de dado)
var existingPerson = await _repository.FindByIdAsync(id);
if (existingPerson == null) // Não encontrado (não há mensagens de erro)
```

Isso pode parecer confuso à primeira vista, mas é um pensamento **natural**:
- **Criar/Modificar** ? null significa "não consegui fazer"
- **Buscar/Reconstituir** ? null significa "não existe"

O Result Pattern não resolve essa suposta ambiguidade - apenas a desloca:

```csharp
// Com Result Pattern, a "ambiguidade" continua
var result = await _repository.FindByIdAsync(id);

// Se IsSuccess = false, é "não encontrado" ou "erro de conexão"?
// Se IsSuccess = true mas Value = null, é válido ou bug?

// E com callbacks:
result.Match(
    onSuccess: person => { /* chamado */ },
    onFailure: errors => { /* não chamado */ }
);
// Se NENHUM callback foi chamado, o que aconteceu?
// Se onSuccess foi chamado mas person é null, é válido?
```

A semântica da operação é quem define o significado - não o tipo de retorno.

**"Result Pattern é mais type-safe"**

Com nullable reference types do C# 8+, o compilador já garante null-safety:

```csharp
// Compilador força tratamento de null
SimpleAggregateRoot? person = RegisterNew(context, input);

person.FirstName; // ?? CS8602: Dereference of possibly null reference

if (person != null)
    person.FirstName; // ? OK
```

A "type safety" do Result Pattern é redundante com nullable reference types habilitado.

**"Result Pattern compõe melhor com LINQ"**

Na teoria sim, com `SelectMany`/`Bind`. Na prática, poucos projetos C# usam essa composição monádica:

```csharp
// Composição monádica (rara em C# real)
var result =
    from customer in GetCustomer(id)
    from order in CreateOrder(customer)
    from payment in ProcessPayment(order)
    select payment;

// O que projetos realmente fazem (imperativo)
var customer = await GetCustomerAsync(id);
if (customer == null) return NotFound();

var order = CreateOrder(context, customer);
if (order == null) return BadRequest(context.Messages);

var payment = await ProcessPaymentAsync(order);
if (payment == null) return BadRequest(context.Messages);

return Ok(payment);
```

O estilo imperativo é mais legível para a maioria dos desenvolvedores C#.

### Quando Result Pattern SERIA Apropriado

Result Pattern faz sentido quando:

1. **Não existe ExecutionContext**: Se não houver local centralizado para mensagens
2. **Composição monádica real**: Projetos que usam estilo funcional consistentemente
3. **Erros são dados**: Quando erros precisam ser serializados e transportados

**Sobre bibliotecas públicas**: Alguém poderia argumentar que bibliotecas deveriam usar Result Pattern por não poderem assumir como o consumidor trata erros.

Nós **somos** uma biblioteca, mas decidimos manter o ExecutionContext porque:

- **Carga leve para o chamador**: Passar um `ExecutionContext` como parâmetro é trivial comparado aos problemas que resolve
- **Evita combinação manual de Results**: Em Application Services que orquestram múltiplos serviços, a alternativa seria combinar Results manualmente - verboso e propenso a erros
- **Menos alocações**: Cada `Result<T>` é uma alocação; com nullable + context compartilhado, as alocações são mínimas
- **Mensagens além de erros**: O domínio precisa comunicar warnings, infos, sugestões - Result Pattern é binário (sucesso/erro)

O custo de "exigir um ExecutionContext" é pequeno comparado aos benefícios. É uma decisão consciente de design, não uma limitação.

## Fundamentação Teórica

### Padrões de Design Relacionados

**Null Object Pattern (variação)** - Retornamos `null` literal ao invés de um Null Object porque o contexto (método de criação/modificação) deixa claro o significado. Um Null Object seria over-engineering para nosso caso.

**Collect Parameter Pattern** - O `ExecutionContext` é um collect parameter que acumula mensagens de múltiplas operações. Este padrão é mais antigo que Result Pattern e igualmente válido.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) não prescreve Result Pattern ou nullable. O foco é em **Ubiquitous Language** e **clareza de intenção**.

Nossos factory methods expressam claramente a intenção:
- `RegisterNew` ? registra nova entidade (ou falha)
- `ChangeName` ? altera nome (ou falha)

O retorno nullable comunica "pode falhar" de forma idiomática em C#.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) discute **Application Services** coletando erros:

> "The Application Service catches any exceptions thrown by the domain and translates them into a result that the client can understand."
>
> *O Application Service captura quaisquer exceções lançadas pelo domínio e traduz em um resultado que o cliente pode entender.*

Nosso ExecutionContext faz exatamente isso - coleta mensagens para tradução posterior.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) prefere **exceções sobre códigos de erro**, mas reconhece que há contextos onde códigos de retorno são apropriados.

Mais importante, ele enfatiza **simplicidade**:

> "The first rule of functions is that they should be small. The second rule of functions is that they should be smaller than that."
>
> *A primeira regra de funções é que devem ser pequenas. A segunda regra de funções é que devem ser menores que isso.*

Match callbacks aumentam a complexidade cognitiva. Null-check é mais simples:

```csharp
// Simples (2 caminhos óbvios)
if (person == null) return BadRequest(...);
// continua...

// Mais complexo (callback com closure)
return result.Match(
    onSuccess: p => /* path 1 */,
    onFailure: e => /* path 2 */
);
```

### O Que o Clean Architecture Diz

Clean Architecture não prescreve Result Pattern. O importante é que **Entities não dependam de frameworks**.

Tanto `T?` quanto `Result<T>` satisfazem essa regra. Escolhemos `T?` por ser nativo da linguagem.

### Outros Fundamentos

**C# Language Design** - A equipe do C# investiu pesadamente em nullable reference types (C# 8+). Isso indica que nullable é a abordagem idiomática para representar "pode não haver valor".

**Performance** - Result Pattern aloca um objeto para cada operação. Com nullable, não há alocação extra:

```csharp
// Result Pattern: 1 alocação por operação
return Result<Person>.Success(person); // new Result<Person>()

// Nullable: zero alocações extras
return person; // retorna a referência diretamente
```

Em hot paths com milhares de operações, essa diferença é mensurável.

**Interoperability** - APIs REST, gRPC, e GraphQL não têm conceito de "Result monad". Eles trabalham com dados + erros separados. Nosso modelo (objeto ou null + mensagens no context) mapeia naturalmente para essas APIs.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre Result Pattern e Either monad?"
- "Como nullable reference types funcionam em C# 8+?"
- "Por que closures em C# causam alocações?"
- "Como IAsyncEnumerable difere de Task<IEnumerable>?"

### Leitura Recomendada

- [Nullable Reference Types - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Result Pattern in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) - Para entender o contexto funcional
- [Closure Allocations in C#](https://devblogs.microsoft.com/premier-developer/dissecting-the-local-functions-in-c-7/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Single source of truth para mensagens, elimina a necessidade de Result Pattern ao centralizar feedback de operações |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Retorno Nullable não Result Pattern
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - RegisterNew retornando nullable
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: ExecutionContext Explícito
