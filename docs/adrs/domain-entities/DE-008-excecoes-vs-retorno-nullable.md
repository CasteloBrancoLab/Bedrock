# DE-008: Exceções vs Retorno Nullable

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine dois tipos de problemas em um restaurante:

**Problema esperado**: Cliente pede um prato que está em falta. O garçom volta e diz: "Desculpe, não temos esse prato hoje. Aqui está o cardápio para você escolher outro." Isso é normal, acontece frequentemente, e há um processo definido para lidar com isso.

**Problema inesperado**: A cozinha pega fogo. O gerente grita "EVACUEM!", alarmes disparam, bombeiros são chamados. Isso é excepcional, raro, e interrompe completamente a operação normal.

Usar exceções para validação de negócio é como acionar os bombeiros toda vez que um prato está em falta. Funciona tecnicamente, mas é desproporcional, caro, e cria caos desnecessário.

### O Problema Técnico

Exceções em .NET têm custo significativo:

```csharp
// Quando uma exceção é lançada:
// 1. Stack trace é capturado (percorre toda a call stack)
// 2. Objetos de exceção são alocados no heap
// 3. Handlers são procurados (stack unwinding)
// 4. Finally blocks são executados
// 5. Filtros de exceção são avaliados

try
{
    var person = new Person(firstName); // Lança se inválido
}
catch (ValidationException ex)
{
    // Chegou aqui após todo o overhead acima
    return BadRequest(ex.Message);
}
```

Para validação de negócio - que acontece em **toda requisição** - esse custo se acumula rapidamente.

## Como Normalmente é Feito

### Abordagem Tradicional

Muitos projetos usam exceções para qualquer tipo de erro, incluindo validação:

```csharp
public class Person
{
    public Person(string firstName, string lastName, DateTime birthDate)
    {
        // Abordagem 1: Exceções genéricas
        if (string.IsNullOrEmpty(firstName))
            throw new ArgumentException("FirstName é obrigatório", nameof(firstName));

        if (firstName.Length > 100)
            throw new ArgumentException("FirstName muito longo", nameof(firstName));

        // Abordagem 2: Exceções de domínio customizadas
        if (birthDate > DateTime.Now)
            throw new DomainException("BirthDate não pode ser no futuro");

        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }
}

// No controller
[HttpPost]
public IActionResult Create(CreatePersonRequest request)
{
    try
    {
        var person = new Person(
            request.FirstName,
            request.LastName,
            request.BirthDate
        );
        _repository.Save(person);
        return Created(...);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ex.Message);
    }
    catch (DomainException ex)
    {
        return BadRequest(ex.Message);
    }
}
```

### Por Que Não Funciona Bem

1. **Performance degradada**: Exceções são 10-100x mais lentas que retorno de valor

```csharp
// Benchmark típico (operações/segundo)
// Retorno nullable: ~50.000.000 ops/s
// Exceção lançada:  ~500.000 ops/s (100x mais lento)

// Em API com 1000 req/s e 10% de validação falhando:
// - Com nullable: overhead imperceptível
// - Com exceções: 100 exceções/s = overhead mensurável
```

2. **Apenas uma mensagem por vez**: Exceção para na primeira falha

```csharp
public Person(string firstName, string lastName)
{
    if (string.IsNullOrEmpty(firstName))
        throw new ArgumentException("FirstName é obrigatório"); // Para aqui

    if (string.IsNullOrEmpty(lastName))
        throw new ArgumentException("LastName é obrigatório"); // Nunca executa

    // Usuário descobre um erro por vez
}
```

3. **Fluxo de controle confuso**: Exceções são "goto" disfarçado

```csharp
public async Task ProcessOrderAsync(OrderInput input)
{
    try
    {
        var customer = await GetCustomer(input.CustomerId);     // Pode lançar
        var product = await GetProduct(input.ProductId);        // Pode lançar
        var order = new Order(customer, product, input.Quantity); // Pode lançar
        await _repository.SaveAsync(order);                     // Pode lançar
    }
    catch (CustomerNotFoundException ex) { /* ... */ }
    catch (ProductNotFoundException ex) { /* ... */ }
    catch (ValidationException ex) { /* ... */ }
    catch (RepositoryException ex) { /* ... */ }
    // Qual linha falhou? Difícil saber sem stack trace
}
```

4. **Stack trace poluído**: Logs cheios de stack traces para erros esperados

```
System.ArgumentException: FirstName é obrigatório
   at Domain.Entities.Person..ctor(String firstName, String lastName)
   at Application.Services.PersonService.CreatePerson(CreatePersonInput input)
   at Api.Controllers.PersonController.Create(CreatePersonRequest request)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor...
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker...
   [mais 20 linhas de framework]
```

Todo esse stack trace para dizer "FirstName está vazio"? Desperdício de logs e dificulta encontrar erros reais.

5. **Semântica incorreta**: Validação não é "excepcional"

```csharp
// O que é EXCEPCIONAL (inesperado, raro, indica bug):
// - Conexão com banco caiu
// - Arquivo de configuração corrompido
// - Null reference onde não deveria ser possível
// - Divisão por zero em cálculo que deveria ter sido validado

// O que NÃO é excepcional (esperado, frequente, input do usuário):
// - FirstName vazio
// - Email em formato inválido
// - Data de nascimento no futuro
// - CPF com dígito verificador errado
```

## A Decisão

### Nossa Abordagem

**Retorno nullable** para validação de negócio, **exceções** apenas para falhas inesperadas:

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    // VALIDAÇÃO DE NEGÓCIO ? Retorno nullable + mensagens no context
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        // Validação: retorna null se falhar, mensagens no context
        var instance = new SimpleAggregateRoot();

        bool isSuccess =
            instance.ChangeNameInternal(executionContext, input.FirstName, input.LastName)
            & instance.ChangeBirthDateInternal(executionContext, input.BirthDate);

        return isSuccess ? instance : null;
    }

    // FALHA INESPERADA ? Exceção
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,  // Se for null, é bug de configuração
        RegisterNewInput input
    )
    {
        // Dependência obrigatória null = bug, não input inválido
        ArgumentNullException.ThrowIfNull(executionContext);

        // ... resto do método
    }
}
```

### Critérios de Decisão

| Situação | Mecanismo | Razão |
|----------|-----------|-------|
| Input de usuário inválido | Nullable + Context | Esperado, frequente, usuário pode corrigir |
| Regra de negócio violada | Nullable + Context | Esperado, parte do domínio |
| Dependência obrigatória null | `ArgumentNullException` | Bug de configuração/DI |
| Configuração inválida do sistema | `InvalidOperationException` | Bug de deploy/setup |
| Violação de invariante interna | `InvalidOperationException` | Bug no código |
| Recurso externo indisponível | Exceção específica | Falha de infraestrutura |

### Exemplos Práticos

**Input de usuário - use nullable**:

```csharp
// ? CORRETO: Validação de negócio retorna null
public static Person? RegisterNew(ExecutionContext context, RegisterNewInput input)
{
    if (!ValidateFirstName(context, input.FirstName))
        return null; // Mensagem já está no context

    if (!ValidateBirthDate(context, input.BirthDate))
        return null; // Mensagem já está no context

    return new Person(input.FirstName, input.BirthDate);
}

// ? ERRADO: Lançar exceção para input inválido
public static Person RegisterNew(RegisterNewInput input)
{
    if (string.IsNullOrEmpty(input.FirstName))
        throw new ValidationException("FirstName é obrigatório"); // NÃO!

    return new Person(input.FirstName, input.BirthDate);
}
```

**Dependência obrigatória - use exceção**:

```csharp
// ? CORRETO: Exceção para dependência null (bug de configuração)
public PersonService(IRepository repository, ITimeProvider timeProvider)
{
    ArgumentNullException.ThrowIfNull(repository);
    ArgumentNullException.ThrowIfNull(timeProvider);

    _repository = repository;
    _timeProvider = timeProvider;
}

// ? CORRETO: Exceção para ExecutionContext null
public static Person? RegisterNew(ExecutionContext executionContext, RegisterNewInput input)
{
    ArgumentNullException.ThrowIfNull(executionContext); // Bug se null

    // ... validação retorna null se falhar
}
```

**Violação de invariante - use exceção**:

```csharp
// ? CORRETO: Exceção para estado impossível (indica bug)
public void ApplyDiscount(decimal percentage)
{
    if (Status != OrderStatus.Draft)
        throw new InvalidOperationException(
            $"Cannot apply discount to order in status {Status}. " +
            "This indicates a bug in the calling code."
        );

    // Se chegou aqui com status errado, é bug no código chamador
}
```

## Consequências

### Benefícios

- **Performance previsível**: Validação não tem overhead de exceções
- **Feedback completo**: Todas as mensagens de validação de uma vez
- **Logs limpos**: Stack traces apenas para erros reais
- **Semântica correta**: Exceções para excepcional, retorno para esperado
- **Debug simplificado**: Erros de validação não "explodem" o debugger

### Trade-offs (Com Perspectiva)

- **Disciplina necessária**: Desenvolvedor deve verificar null e consultar context
- **Dois caminhos de erro**: Nullable para validação, exceção para bugs

### Trade-offs Frequentemente Superestimados

**"Exceções são mais seguras - forçam tratamento"**

Na prática, exceções não forçam nada - só mudam onde o erro aparece:

```csharp
// Com exceção: erro aparece no handler global (ou crasha a aplicação)
try
{
    var person = new Person(input.FirstName); // Lança
}
catch (Exception ex)
{
    // Desenvolvedor esqueceu de tratar especificamente
    _logger.LogError(ex, "Erro inesperado"); // Trata como erro genérico
}

// Com nullable: compilador avisa se não tratar
Person? person = Person.RegisterNew(context, input);
person.FirstName; // ?? Warning: possible null reference
// Desenvolvedor é FORÇADO a lidar com null
```

Com nullable reference types, o compilador **realmente força** o tratamento.

**"Exceções são mais expressivas"**

Expressividade vem do design, não do mecanismo:

```csharp
// Exceção "expressiva"
throw new FirstNameTooLongException(firstName, maxLength: 100);

// Nullable + mensagem igualmente expressiva
context.AddMessage(new Message(
    MessageType.Error,
    "FIRST_NAME_TOO_LONG",
    $"FirstName '{firstName}' excede o limite de 100 caracteres."
));
return null;
```

A diferença é que a segunda opção:
- Não interrompe o fluxo
- Permite coletar mais erros
- Não gera stack trace

**"Performance de exceção é irrelevante em I/O-bound"**

Verdade parcial. Mas:

```csharp
// Validação acontece ANTES do I/O
public async Task<Person?> CreatePersonAsync(ExecutionContext context, CreatePersonInput input)
{
    // 1. Validação (CPU-bound, frequente, deveria ser rápido)
    var person = Person.RegisterNew(context, input);
    if (person == null)
        return null; // Nem chegou no I/O

    // 2. I/O (lento, mas só executa se validação passou)
    await _repository.SaveAsync(person);

    return person;
}
```

Se a validação falha em 30% das requisições (input de usuário), você está pagando o custo de exceção em 30% das requisições - ANTES do I/O.

### Quando Exceção é Apropriada

| Cenário | Tipo de Exceção | Razão |
|---------|-----------------|-------|
| Parâmetro null em construtor | `ArgumentNullException` | Bug de DI/configuração |
| Estado impossível alcançado | `InvalidOperationException` | Bug no código |
| Arquivo de config ausente | `FileNotFoundException` | Falha de deploy |
| Conexão com banco falhou | `DbException` (ou wrapper) | Falha de infra |
| Timeout em chamada externa | `TimeoutException` | Falha de rede |

## Fundamentação Teórica

### Padrões de Design Relacionados

**Null Object Pattern** - Consideramos usar Null Object ao invés de `null` literal, mas decidimos que `null` é mais claro no contexto de "operação não realizada". Um Null Object sugere "objeto válido com comportamento neutro", que não é o caso.

**Special Case Pattern** - Similar ao Null Object, mas para casos específicos. Nosso `null` é o "special case" para "validação falhou".

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) não prescreve exceções vs retorno, mas enfatiza que o **domínio deve expressar conceitos de negócio claramente**.

Validação de negócio é um **conceito de negócio** - "este input não atende às regras". Modelar isso como exceção (erro técnico) confunde o modelo.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) discute validação:

> "Validation is about checking that data conforms to business rules. [...] Invalid data should be reported to the client in a meaningful way."
>
> *Validação é sobre verificar se dados atendem às regras de negócio. [...] Dados inválidos devem ser reportados ao cliente de forma significativa.*

"Forma significativa" inclui mostrar **todos** os erros - o que exceções não permitem naturalmente.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) dedica um capítulo a tratamento de erros:

> "Use Exceptions Rather Than Return Codes"
>
> *Use Exceções ao Invés de Códigos de Retorno*

Mas o contexto é importante: Martin está comparando **exceções vs códigos numéricos de erro** (estilo C), não vs nullable reference types com context. Ele também diz:

> "Don't Use Exceptions for Flow Control"
>
> *Não Use Exceções para Controle de Fluxo*

Validação de input é **controle de fluxo**: "se válido, continue; se não, retorne erro". Usar exceção para isso viola esse princípio.

### O Que o Clean Architecture Diz

Clean Architecture separa **Enterprise Business Rules** (Entities) de **Application Business Rules** (Use Cases). Ambas as camadas podem ter validação.

Exceções que "atravessam" camadas criam acoplamento indesejado. Retorno nullable mantém cada camada responsável por seu próprio tratamento de erro.

### Outros Fundamentos

**Effective Java - Item 77** (Joshua Bloch):
> "Use exceptions only for exceptional conditions. [...] Exceptions are, as their name implies, to be used only for exceptional conditions; they should never be used for ordinary control flow."
>
> *Use exceções apenas para condições excepcionais. [...] Exceções são, como o nome indica, para serem usadas apenas para condições excepcionais; nunca devem ser usadas para controle de fluxo ordinário.*

Bloch explica que exceções são otimizadas pelo JVM/CLR para o caso **não lançado**. Quando lançadas, são intencionalmente caras para desencorajar uso em fluxo normal.

**Microsoft .NET Guidelines**:
> "Do not use exceptions for normal flow of control, if possible. [...] Framework designers should design APIs so that users can write code that does not throw exceptions."
>
> *Não use exceções para fluxo normal de controle, se possível. [...] Designers de framework devem projetar APIs para que usuários possam escrever código que não lance exceções.*

Nossa API com nullable + context permite exatamente isso: código que não lança exceções para validação.

**Performance Studies**:

Benchmarks consistentemente mostram que exceções são ordens de magnitude mais lentas que retorno de valor:

- Exceção lançada: ~2-5µs (microsegundos)
- Retorno nullable: ~2-5ns (nanosegundos)
- Diferença: ~1000x

Em APIs de alta performance, essa diferença é significativa.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que exceções são lentas em .NET?"
- "Qual a diferença entre exceções checked e unchecked?"
- "Como o stack unwinding funciona em C#?"
- "Por que Effective Java recomenda evitar exceções para controle de fluxo?"

### Leitura Recomendada

- [Effective Java - Item 77: Use exceptions only for exceptional conditions](https://www.oreilly.com/library/view/effective-java/9780134686097/)
- [Microsoft .NET Exception Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Exception Performance in .NET](https://mattwarren.org/2016/12/20/Why-Exceptions-should-be-Exceptional/)
- [Clean Code - Chapter 7: Error Handling](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Coleta mensagens de validação sem usar exceções, permitindo validação de negócio com retorno nullable |
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Fornece validações que retornam bool e adicionam mensagens ao context, evitando exceções para validação de negócio |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Exceções vs Retorno Nullable
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - RegisterNew usando nullable
