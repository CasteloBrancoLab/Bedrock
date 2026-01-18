# 📋 ExecutionContext - Observador de Execução para Auditoria e Diagnóstico

A classe `ExecutionContext` atua como **observador passivo** durante a execução de operações, coletando mensagens de diferentes níveis de severidade e exceções para fins de auditoria, diagnóstico e observabilidade. Fornece contexto compartilhado (tenant, usuário, correlação, operação de negócio) e rastreamento thread-safe de resultados.

> 💡 **Visão Geral:** Centralize informações de execução (tenant, usuário, operação de negócio, mensagens, exceções) em um único objeto **thread-safe**, com suporte a níveis de severidade, filtragem por `MinimumMessageType` e diagnóstico no final da operação.

---

## 📋 Sumário

- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
  - [Informações de Contexto Espalhadas](#1--informações-de-contexto-espalhadas)
  - [Coleta Inconsistente de Erros e Mensagens](#2--coleta-inconsistente-de-erros-e-mensagens)
  - [Dificuldade de Diagnóstico Pós-Execução](#3--dificuldade-de-diagnóstico-pós-execução)
- [Funcionalidades](#-funcionalidades)
- [Como Usar](#-como-usar)
- [Decisões de Design](#-decisões-de-design)
- [Trade-offs](#️-tradeoffs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)

---

## 🎯 Contexto: Por Que Existe

### O Problema Real

Em aplicações empresariais, cada operação precisa de **contexto** (quem está executando, para qual tenant, quando) e **rastreabilidade** (o que aconteceu, quais erros, quais avisos). As abordagens tradicionais apresentam problemas sérios:

**Exemplo de desafios comuns:**

```csharp
❌ Abordagem 1: Parâmetros espalhados em cada método
public class OrderService
{
    public async Task<Result> ProcessOrder(
        Guid correlationId,       // ⚠️ Precisa passar em todo lugar
        Guid tenantId,            // ⚠️ Precisa passar em todo lugar
        string tenantName,        // ⚠️ Pode dessincronizar com tenantId
        string executionUser,     // ⚠️ Precisa passar em todo lugar
        TimeProvider timeProvider, // ⚠️ Precisa passar em todo lugar
        Order order
    )
    {
        // Validar pedido
        var validationResult = await _validator.Validate(
            correlationId, tenantId, tenantName, executionUser, timeProvider,  // ⚠️ Repetição!
            order
        );

        if (!validationResult.IsValid)
        {
            // Como coletar os erros de validação?
            // Como saber quais avisos ocorreram?
            return Result.Failure(validationResult.Errors);
        }

        // Processar pagamento
        var paymentResult = await _paymentService.Process(
            correlationId, tenantId, tenantName, executionUser, timeProvider,  // ⚠️ Repetição!
            order.Payment
        );

        // Como agregar mensagens de múltiplos serviços?
        return Result.Success();
    }
}

❌ Problemas:
- 5+ parâmetros repetidos em cada chamada de método
- Fácil esquecer um parâmetro ou passar na ordem errada
- Impossível coletar mensagens de múltiplos serviços
- Sem visão consolidada do que aconteceu
- Difícil fazer logging estruturado
```

```csharp
❌ Abordagem 2: Usar exceções para tudo
public class OrderService
{
    public async Task ProcessOrder(Order order)
    {
        try
        {
            await _validator.Validate(order);  // Throws ValidationException
            await _paymentService.Process(order);  // Throws PaymentException
            await _inventoryService.Reserve(order);  // Throws InventoryException
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            throw;  // ⚠️ Perde contexto, interrompe fluxo
        }
        catch (PaymentException ex)
        {
            _logger.LogError(ex, "Payment failed");
            throw;  // ⚠️ Não sabe se validação teve warnings
        }
        catch (InventoryException ex)
        {
            _logger.LogError(ex, "Inventory failed");
            throw;  // ⚠️ Não sabe se pagamento foi parcialmente processado
        }
    }
}

❌ Problemas:
- Exceções são caras (stack trace, allocation)
- Perde informações de etapas anteriores
- Não captura warnings ou informações
- Try-catch em cascata dificulta leitura
- Sem visão do que funcionou antes de falhar
```

```csharp
❌ Abordagem 3: Lista de erros manual
public class OrderService
{
    public async Task<(bool Success, List<string> Errors, List<string> Warnings)> ProcessOrder(Order order)
    {
        var errors = new List<string>();    // ⚠️ Não thread-safe
        var warnings = new List<string>();  // ⚠️ Não thread-safe

        var validationResult = await _validator.Validate(order);
        errors.AddRange(validationResult.Errors);
        warnings.AddRange(validationResult.Warnings);

        if (errors.Any())
            return (false, errors, warnings);

        var paymentResult = await _paymentService.Process(order);
        errors.AddRange(paymentResult.Errors);
        warnings.AddRange(paymentResult.Warnings);

        // ⚠️ Como diferenciar erro de warning?
        // ⚠️ Como saber qual serviço gerou cada mensagem?
        // ⚠️ Como ter timestamp de cada mensagem?
        // ⚠️ Como ter contexto (tenant, user, correlation)?

        return (!errors.Any(), errors, warnings);
    }
}

❌ Problemas:
- Não thread-safe (race conditions em paralelo)
- Sem metadados (timestamp, código, severidade)
- Sem contexto (tenant, user, correlation)
- Retorno complexo e inconsistente
- Cada serviço precisa retornar tuplas similares
```

### A Solução

O `ExecutionContext` implementa um **observador passivo** que centraliza contexto e coleta mensagens de forma thread-safe.

```csharp
✅ Abordagem com ExecutionContext:
public class OrderService
{
    public async Task<Result> ProcessOrder(ExecutionContext context, Order order)
    {
        // ✨ Contexto disponível: context.TenantInfo, context.ExecutionUser, context.ExecutionOrigin, context.BusinessOperationCode, context.CorrelationId

        // Validar pedido
        var validationResult = await _validator.Validate(context, order);

        if (!validationResult.IsValid)
        {
            context.AddErrorMessage("ORDER_VALIDATION_FAILED", "Pedido inválido");
            return Result.Failure("Validation failed");  // ✅ Método retorna falha
        }

        context.AddInformationMessage("ORDER_VALIDATED", $"Pedido {order.Id} validado");

        // Processar pagamento
        var paymentResult = await _paymentService.Process(context, order.Payment);

        if (!paymentResult.Success)
        {
            context.AddErrorMessage("PAYMENT_FAILED", paymentResult.Error);
            return Result.Failure("Payment failed");
        }

        context.AddSuccessMessage("ORDER_PROCESSED", $"Pedido {order.Id} processado");
        return Result.Success();
    }
}

// No final da operação (controller, handler, etc):
if (!context.IsSuccessful)
{
    _logger.LogWarning(
        "Operação falhou para tenant {Tenant}, user {User}, origin {Origin}, correlation {Correlation}. Mensagens: {@Messages}",
        context.TenantInfo.Name,
        context.ExecutionUser,
        context.ExecutionOrigin,
        context.CorrelationId,
        context.Messages
    );
}

✅ Benefícios:
- Contexto centralizado (tenant, user, origin, businessOperationCode, correlation, timeProvider)
- Coleta thread-safe de mensagens e exceções
- Níveis de severidade (Trace, Debug, Info, Warning, Error, Critical, Success)
- Filtragem por MinimumMessageType
- Diagnóstico consolidado no final (IsSuccessful, IsFaulted, IsPartiallySuccessful)
- Mensagens com metadados (Id, Timestamp, Code, Text)
- Clonável para operações paralelas
- Importável para agregar resultados
```

**Estrutura do ExecutionContext:**

```
┌──────────────────────────────────────────────────────────────────────────┐
│                     ESTRUTURA DO EXECUTIONCONTEXT                        │
└──────────────────────────────────────────────────────────────────────────┘
│                                                                           │
│   ExecutionContext (class - reference type)                              │
│   │                                                                       │
│   ├── Contexto Imutável:                                                 │
│   │   ├── Timestamp: DateTimeOffset      → Momento de criação            │
│   │   ├── CorrelationId: Guid            → Rastreamento distribuído      │
│   │   ├── TenantInfo: TenantInfo         → Identificação do tenant       │
│   │   ├── ExecutionUser: string          → Usuário executando            │
│   │   ├── ExecutionOrigin: string        → Origem da execução (API, etc) │
│   │   ├── MinimumMessageType: MessageType → Filtro de mensagens          │
│   │   └── TimeProvider: TimeProvider     → Fonte de tempo (testável)     │
│   │                                                                       │
│   ├── Contexto Mutável:                                                  │
│   │   └── BusinessOperationCode: string  → Operação de negócio atual     │
│   │                                                                       │
│   ├── Coleções Thread-Safe:                                              │
│   │   ├── _messageCollection: ConcurrentDictionary<Id, Message>          │
│   │   └── _exceptionCollection: ConcurrentBag<Exception>                 │
│   │                                                                       │
│   └── Propriedades de Diagnóstico:                                       │
│       ├── HasMessages: bool              → Tem alguma mensagem?          │
│       ├── HasErrorMessages: bool         → Tem Error ou Critical?        │
│       ├── HasExceptions: bool            → Tem exceções?                 │
│       ├── IsSuccessful: bool             → Sem erros e sem exceções      │
│       ├── IsFaulted: bool                → Tem erros ou exceções         │
│       ├── IsPartiallySuccessful: bool    → Tem Success + (Error/Exception)│
│       ├── Messages: IEnumerable<Message> → Todas as mensagens            │
│       └── Exceptions: IEnumerable<Exception> → Todas as exceções         │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Problemas Resolvidos

### 1. 🔗 Informações de Contexto Espalhadas

**Problema:** Parâmetros como tenant, usuário, correlationId precisam ser passados em cada método.

#### 📚 Analogia: O Crachá do Funcionário

Imagine um hospital onde cada procedimento precisa identificar o paciente:

**❌ Sem contexto centralizado:**

```
Enfermeiro: "Preciso do nome, CPF, convênio e médico responsável"
           → Anota em papel

Laboratório: "Preciso do nome, CPF, convênio e médico responsável"
            → Anota em outro papel (pode ter erro!)

Farmácia: "Preciso do nome, CPF, convênio e médico responsável"
         → Anota em outro papel (pode dessincronizar!)

⚠️ PROBLEMA: Cada setor anota separadamente, pode haver inconsistência
```

**✅ Com ExecutionContext (Pulseira do Paciente):**

```
Recepção: Cria pulseira com código de barras
         → Contém: Nome, CPF, Convênio, Médico, Data de Entrada

Enfermeiro: Escaneia pulseira → Todas as informações disponíveis
Laboratório: Escaneia pulseira → Mesmas informações, garantido!
Farmácia: Escaneia pulseira → Consistência total!

✅ CORRETO: Uma única fonte de verdade, passada entre setores
```

#### 💻 Impacto Real no Código

**❌ Código com parâmetros espalhados:**

```csharp
public class PaymentService
{
    public async Task<PaymentResult> ProcessPayment(
        Guid correlationId,
        Guid tenantId,
        string tenantName,
        string executionUser,
        TimeProvider timeProvider,
        Payment payment
    )
    {
        _logger.LogInformation(
            "Processing payment for tenant {TenantId} by user {User}",
            tenantId,
            executionUser
        );

        // Chamar gateway de pagamento
        var gatewayResult = await _gateway.Charge(
            correlationId,  // ⚠️ Passando tudo de novo
            tenantId,
            tenantName,
            executionUser,
            timeProvider,
            payment
        );

        // Registrar auditoria
        await _auditService.Log(
            correlationId,  // ⚠️ Passando tudo de novo
            tenantId,
            tenantName,
            executionUser,
            timeProvider,
            "PAYMENT_PROCESSED",
            payment.Amount
        );

        return gatewayResult;
    }
}

❌ Problemas:
- 6 parâmetros em cada método
- Fácil esquecer ou trocar ordem
- Repetição em cada chamada interna
- Assinaturas de método enormes
```

**✅ Código com ExecutionContext:**

```csharp
public class PaymentService
{
    public async Task<PaymentResult> ProcessPayment(
        ExecutionContext context,  // ✨ Um único objeto!
        Payment payment
    )
    {
        _logger.LogInformation(
            "Processing payment for tenant {TenantName} by user {User}, origin {Origin}, correlation {CorrelationId}",
            context.TenantInfo.Name,
            context.ExecutionUser,
            context.ExecutionOrigin,
            context.CorrelationId
        );

        context.AddInformationMessage("PAYMENT_STARTED", $"Iniciando pagamento de {payment.Amount}");

        // Chamar gateway de pagamento
        var gatewayResult = await _gateway.Charge(context, payment);  // ✨ Só context + dado

        if (!gatewayResult.Success)
        {
            context.AddErrorMessage("GATEWAY_ERROR", gatewayResult.Error);
            return PaymentResult.Failed(gatewayResult.Error);
        }

        // Registrar auditoria
        await _auditService.Log(context, "PAYMENT_PROCESSED", payment.Amount);  // ✨ Limpo!

        context.AddSuccessMessage("PAYMENT_COMPLETED", $"Pagamento de {payment.Amount} concluído");
        return PaymentResult.Success();
    }
}

✅ Benefícios:
- Apenas 2 parâmetros (context + dados do negócio)
- Impossível esquecer ou trocar ordem
- Contexto propagado automaticamente
- Assinaturas de método limpas
- Mensagens coletadas automaticamente
```

---

### 2. 📊 Coleta Inconsistente de Erros e Mensagens

**Problema:** Cada serviço coleta erros de forma diferente, sem padronização ou thread-safety.

#### 📚 Analogia: O Prontuário Eletrônico

Imagine um hospital com sistema de prontuário:

**❌ Sem padronização:**

```
Médico 1: Anota em papel "Paciente com febre"
Médico 2: Anota em planilha "T: 38.5°C"
Enfermeiro: Anota em app "Febre alta"

⚠️ PROBLEMA:
- Informações em formatos diferentes
- Sem timestamp preciso
- Sem identificação de quem registrou
- Difícil consolidar histórico
```

**✅ Com ExecutionContext (Prontuário Padronizado):**

```
Sistema único de registro:
┌────────────────────────────────────────────────────────────┐
│ ID: MSG-001                                                │
│ Timestamp: 2024-01-15 10:30:00 UTC                        │
│ Tipo: Warning                                              │
│ Código: VITAL_SIGNS_ABNORMAL                              │
│ Texto: Temperatura 38.5°C acima do normal                 │
│ Registrado por: Dr. Silva                                  │
└────────────────────────────────────────────────────────────┘

✅ CORRETO: Formato padronizado, com metadados, consolidado
```

#### 💻 Impacto Real no Código

**❌ Código com coleta inconsistente:**

```csharp
public class ImportService
{
    public async Task<ImportResult> ImportData(List<Record> records)
    {
        var errors = new List<string>();      // ⚠️ Não thread-safe
        var warnings = new List<string>();    // ⚠️ Sem timestamp
        var successCount = 0;

        // Processamento paralelo
        await Parallel.ForEachAsync(records, async (record, ct) =>
        {
            try
            {
                await ProcessRecord(record);
                successCount++;  // ⚠️ Race condition!
            }
            catch (ValidationException ex)
            {
                warnings.Add(ex.Message);  // ⚠️ Race condition!
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);  // ⚠️ Race condition!
            }
        });

        return new ImportResult
        {
            TotalProcessed = successCount,
            Errors = errors,
            Warnings = warnings
        };
    }
}

❌ Problemas:
- Race conditions em listas não thread-safe
- Sem timestamp nas mensagens
- Sem códigos padronizados
- Sem identificação da severidade real
```

**✅ Código com ExecutionContext:**

```csharp
public class ImportService
{
    public async Task<ImportResult> ImportData(ExecutionContext context, List<Record> records)
    {
        // Processamento paralelo - context é thread-safe!
        await Parallel.ForEachAsync(records, async (record, ct) =>
        {
            try
            {
                await ProcessRecord(context, record);
                context.AddSuccessMessage(
                    "RECORD_IMPORTED",
                    $"Registro {record.Id} importado"
                );
            }
            catch (ValidationException ex)
            {
                context.AddWarningMessage(
                    "RECORD_VALIDATION_WARNING",
                    $"Registro {record.Id}: {ex.Message}"
                );
            }
            catch (Exception ex)
            {
                context.AddErrorMessage(
                    "RECORD_IMPORT_ERROR",
                    $"Registro {record.Id}: {ex.Message}"
                );
                context.AddException(ex);  // ✨ Captura exceção também
            }
        });

        // Diagnóstico consolidado
        var result = new ImportResult
        {
            IsSuccessful = context.IsSuccessful,
            IsPartiallySuccessful = context.IsPartiallySuccessful,
            Messages = context.Messages.ToList(),
            Exceptions = context.Exceptions.ToList()
        };

        return result;
    }
}

✅ Benefícios:
- Thread-safe (ConcurrentDictionary + ConcurrentBag)
- Mensagens com timestamp automático
- Códigos padronizados
- Severidade explícita
- Exceções capturadas separadamente
- Diagnóstico consolidado (IsSuccessful, IsPartiallySuccessful)
```

---

### 3. 🔍 Dificuldade de Diagnóstico Pós-Execução

**Problema:** Após uma operação, é difícil saber o que aconteceu, quais avisos ocorreram, se houve sucesso parcial.

#### 📚 Analogia: O Relatório de Voo

Imagine pilotar um avião sem caixa-preta:

**❌ Sem registro consolidado:**

```
Após o voo:
- "Acho que houve turbulência"
- "Talvez tenha havido um alerta"
- "O motor fez um barulho estranho... ou não?"

⚠️ PROBLEMA: Sem registro, impossível diagnosticar
```

**✅ Com ExecutionContext (Caixa-Preta):**

```
Após o voo:
┌────────────────────────────────────────────────────────────┐
│ FLIGHT RECORDER - VOO 1234                                 │
├────────────────────────────────────────────────────────────┤
│ 10:00:00 [INFO] TAKEOFF_INITIATED - Decolagem iniciada    │
│ 10:05:23 [WARN] TURBULENCE_DETECTED - Turbulência leve    │
│ 10:15:45 [INFO] CRUISE_ALTITUDE - Altitude de cruzeiro    │
│ 10:30:00 [WARN] ENGINE_TEMP_HIGH - Motor 2: 95°C          │
│ 10:45:00 [SUCCESS] LANDING_COMPLETE - Pouso bem-sucedido  │
├────────────────────────────────────────────────────────────┤
│ STATUS: SUCCESSFUL (com warnings)                          │
│ EXCEPTIONS: 0                                              │
└────────────────────────────────────────────────────────────┘

✅ CORRETO: Histórico completo, diagnóstico preciso
```

#### 💻 Impacto Real no Código

**❌ Código sem diagnóstico consolidado:**

```csharp
public async Task<IActionResult> ProcessOrder([FromBody] OrderRequest request)
{
    try
    {
        var result = await _orderService.Process(request);

        if (result.HasErrors)
        {
            // ⚠️ Quais erros? Houve warnings também?
            return BadRequest(result.Errors);
        }

        return Ok(result);
    }
    catch (Exception ex)
    {
        // ⚠️ Perdeu todo o contexto de mensagens anteriores
        _logger.LogError(ex, "Error processing order");
        return StatusCode(500);
    }
}

❌ Problemas:
- Não sabe se houve warnings antes do erro
- Não sabe quais etapas funcionaram
- Perde contexto ao capturar exceção
- Log genérico sem detalhes
```

**✅ Código com ExecutionContext:**

```csharp
public async Task<IActionResult> ProcessOrder([FromBody] OrderRequest request)
{
    var context = ExecutionContext.Create(
        correlationId: HttpContext.TraceIdentifier.ToGuid(),
        tenantInfo: _tenantAccessor.TenantInfo,
        executionUser: User.Identity?.Name ?? "anonymous",
        executionOrigin: "API",
        businessOperationCode: "PROCESS_ORDER",
        minimumMessageType: MessageType.Information,
        timeProvider: TimeProvider.System
    );

    try
    {
        var result = await _orderService.Process(context, request);

        // Diagnóstico completo
        if (context.IsSuccessful)
        {
            _logger.LogInformation(
                "Order processed successfully. Messages: {@Messages}",
                context.Messages
            );
            return Ok(result);
        }

        if (context.IsPartiallySuccessful)
        {
            _logger.LogWarning(
                "Order partially processed. Successes: {Successes}, Errors: {Errors}",
                context.Messages.Count(m => m.MessageType == MessageType.Success),
                context.Messages.Count(m => m.MessageType == MessageType.Error)
            );
            return StatusCode(207, new { result, messages = context.Messages });
        }

        // IsFaulted
        _logger.LogError(
            "Order processing failed. Errors: {@Errors}, Exceptions: {@Exceptions}",
            context.Messages.Where(m => m.MessageType >= MessageType.Error),
            context.Exceptions
        );
        return BadRequest(new { errors = context.Messages });
    }
    catch (Exception ex)
    {
        context.AddException(ex);
        context.AddCriticalMessage("UNHANDLED_EXCEPTION", ex.Message);

        _logger.LogCritical(
            ex,
            "Unhandled exception. Context: {@Context}",
            new
            {
                context.CorrelationId,
                context.TenantInfo,
                context.ExecutionUser,
                context.Messages,
                context.Exceptions
            }
        );

        return StatusCode(500);
    }
}

✅ Benefícios:
- Diagnóstico completo (IsSuccessful, IsPartiallySuccessful, IsFaulted)
- Todas as mensagens preservadas
- Exceções capturadas com contexto
- Log estruturado com todos os detalhes
- Resposta apropriada para cada cenário
```

---

## ✨ Funcionalidades

### 📝 Adição de Mensagens por Severidade

Métodos específicos para cada nível de severidade, respeitando `MinimumMessageType`.

```csharp
var context = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),
    tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Acme Corp"),
    executionUser: "user@acme.com",
    executionOrigin: "API",
    businessOperationCode: "PROCESS_ORDER",
    minimumMessageType: MessageType.Warning,  // ✨ Filtra Trace, Debug e Information
    timeProvider: TimeProvider.System
);

// Mensagens filtradas (não são adicionadas)
context.AddTraceMessage("TRACE_MSG", "Não será adicionada");
context.AddDebugMessage("DEBUG_MSG", "Não será adicionada");
context.AddInformationMessage("INFO_MSG", "Não será adicionada");

// Mensagens adicionadas (respeitam MinimumMessageType)
context.AddWarningMessage("WARN_MSG", "Será adicionada");

// Mensagens SEMPRE adicionadas (ignoram MinimumMessageType)
context.AddErrorMessage("ERROR_MSG", "Sempre adicionada");     // ✨ Erros são críticos
context.AddCriticalMessage("CRITICAL_MSG", "Sempre adicionada"); // ✨ Críticos são críticos
context.AddSuccessMessage("SUCCESS_MSG", "Sempre adicionada");   // ✨ Sucesso é importante

Console.WriteLine(context.Messages.Count());  // 4 (Warning + Error + Critical + Success)
```

**Níveis de severidade (MessageType):**

| Nível | Valor | Descrição | Filtrado por MinimumMessageType? |
|-------|-------|-----------|----------------------------------|
| Trace | 0 | Diagnóstico detalhado | Sim |
| Debug | 1 | Informação de debug | Sim |
| Information | 2 | Informação geral | Sim |
| Warning | 3 | Aviso (não impede sucesso) | Sim |
| Error | 4 | Erro (impede sucesso) | **Não** (sempre adicionada) |
| Critical | 5 | Erro crítico | **Não** (sempre adicionada) |
| None | 6 | Sem categoria | Sim |
| Success | 7 | Operação bem-sucedida | **Não** (sempre adicionada) |

> 💡 **Importante:** Mensagens de **Error**, **Critical** e **Success** são **sempre adicionadas**, independente do `MinimumMessageType`. Isso porque essas mensagens são usadas para determinar o resultado da operação (`IsSuccessful`, `IsFaulted`, `HasErrorMessages`). Como controlam o fluxo de diagnóstico, não podem ser filtradas — caso contrário, uma operação com erro poderia ser considerada bem-sucedida apenas por configuração de log.

---

### 🔄 Clone e Import para Operações Paralelas

Suporte a clonagem e importação de contextos para processamento paralelo.

```csharp
var mainContext = ExecutionContext.Create(...);

// Processar items em paralelo, cada um com seu contexto
var tasks = items.Select(async item =>
{
    var itemContext = mainContext.Clone();  // ✨ Cópia independente

    await ProcessItem(itemContext, item);

    return itemContext;
});

var itemContexts = await Task.WhenAll(tasks);

// Agregar resultados no contexto principal
foreach (var itemContext in itemContexts)
{
    mainContext.Import(itemContext);  // ✨ Importa mensagens e exceções
}

// Diagnóstico consolidado
Console.WriteLine($"Successful: {mainContext.IsSuccessful}");
Console.WriteLine($"Total messages: {mainContext.Messages.Count()}");
```

---

### 🎯 Propriedades de Diagnóstico

Propriedades para avaliar resultado da execução.

```csharp
// Cenário 1: Tudo OK
context.AddSuccessMessage("OK", "Processado");
Console.WriteLine(context.IsSuccessful);           // True
Console.WriteLine(context.IsFaulted);              // False
Console.WriteLine(context.IsPartiallySuccessful);  // False

// Cenário 2: Erro total
context.AddErrorMessage("ERROR", "Falhou");
Console.WriteLine(context.IsSuccessful);           // False
Console.WriteLine(context.IsFaulted);              // True
Console.WriteLine(context.IsPartiallySuccessful);  // False (sem Success)

// Cenário 3: Sucesso parcial
context.AddSuccessMessage("OK", "Item 1 OK");
context.AddErrorMessage("ERROR", "Item 2 falhou");
Console.WriteLine(context.IsSuccessful);           // False
Console.WriteLine(context.IsFaulted);              // True
Console.WriteLine(context.IsPartiallySuccessful);  // True ✨ (tem Success + Error)
```

---

### ✏️ Modificação de Mensagens

Alterar texto ou tipo de mensagens existentes.

```csharp
// Adicionar mensagem
context.AddWarningMessage("IMPORT_ISSUE", "Importação com problemas");

// Obter ID da mensagem
var messageId = context.Messages.First().Id;

// Alterar texto
context.ChangeMessageText(messageId, "Importação concluída com 3 avisos");

// Alterar tipo por ID (útil para importações parciais)
context.ChangeMessageType(messageId, MessageType.Information);

// Alterar tipo por MessageType (altera todas as mensagens do tipo especificado)
context.ChangeMessageType(MessageType.Warning, MessageType.Information);  // ✨ Converte todos Warnings em Information

// Retorna false se ID não existe ou nenhuma mensagem foi alterada
var result = context.ChangeMessageText(Id.GenerateNewId(), "Não existe");
Console.WriteLine(result);  // False
```

---

### 🏷️ Alteração do Código de Operação de Negócio

O `BusinessOperationCode` identifica qual operação de negócio está sendo executada. Pode ser alterado após a criação do contexto para refletir mudanças no fluxo.

```csharp
// Criar contexto com operação inicial
var context = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),
    tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Acme Corp"),
    executionUser: "user@acme.com",
    executionOrigin: "API",
    businessOperationCode: "CREATE_ORDER",  // ✨ Operação inicial
    minimumMessageType: MessageType.Information,
    timeProvider: TimeProvider.System
);

Console.WriteLine(context.BusinessOperationCode);  // "CREATE_ORDER"

// Durante o fluxo, a operação pode mudar
context.ChangeBusinessOperationCode("PROCESS_PAYMENT");  // ✨ Atualiza operação

Console.WriteLine(context.BusinessOperationCode);  // "PROCESS_PAYMENT"

// Validação: rejeita null ou whitespace
context.ChangeBusinessOperationCode("");  // ❌ Throws ArgumentException
context.ChangeBusinessOperationCode(null!);  // ❌ Throws ArgumentException
```

**Casos de uso:**
- API recebe requisição genérica e determina operação específica após análise
- Fluxo de negócio muda de fase (ex: `CREATE_ORDER` → `PROCESS_PAYMENT` → `SHIP_ORDER`)
- Contexto é criado em middleware antes de saber a operação específica

---

### ⚠️ Captura de Exceções

Coletar exceções separadamente das mensagens.

```csharp
try
{
    await ProcessData(context, data);
}
catch (Exception ex)
{
    context.AddException(ex);  // ✨ Captura exceção
    context.AddCriticalMessage("EXCEPTION", ex.Message);
}

// Verificar exceções
Console.WriteLine(context.HasExceptions);  // True
Console.WriteLine(context.IsFaulted);      // True (exceção = falha)

foreach (var ex in context.Exceptions)
{
    Console.WriteLine($"Exception: {ex.GetType().Name} - {ex.Message}");
}
```

---

## 🚀 Como Usar

### 1️⃣ Uso Básico - Criação e Mensagens

```csharp
using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;

// Criar contexto
var context = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),
    tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Minha Empresa"),
    executionUser: "usuario@empresa.com",
    executionOrigin: "API",
    businessOperationCode: "CREATE_ORDER",
    minimumMessageType: MessageType.Information,
    timeProvider: TimeProvider.System
);

// Adicionar mensagens durante a operação
context.AddInformationMessage("OPERATION_STARTED", "Iniciando operação");

// ... lógica de negócio ...

if (someCondition)
{
    context.AddWarningMessage("DATA_INCOMPLETE", "Dados incompletos, usando padrões");
}

context.AddSuccessMessage("OPERATION_COMPLETED", "Operação concluída");

// Verificar resultado
Console.WriteLine($"Sucesso: {context.IsSuccessful}");
Console.WriteLine($"Mensagens: {context.Messages.Count()}");
```

**Quando usar:** Qualquer operação que precise de contexto e rastreamento de mensagens.

---

### 2️⃣ Uso em Serviços de Domínio

```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IPaymentGateway _paymentGateway;

    public async Task<Order?> CreateOrder(ExecutionContext context, CreateOrderRequest request)
    {
        // Validação
        if (request.Items.Count == 0)
        {
            context.AddErrorMessage("ORDER_EMPTY", "Pedido não pode estar vazio");
            return null;  // ✅ Método retorna falha
        }

        context.AddInformationMessage("ORDER_VALIDATED", "Pedido validado");

        // Criar pedido
        var order = Order.Create(context, request);

        if (order == null)
        {
            context.AddErrorMessage("ORDER_CREATION_FAILED", "Falha ao criar pedido");
            return null;
        }

        // Processar pagamento
        var paymentResult = await _paymentGateway.Process(context, order.Payment);

        if (!paymentResult.Success)
        {
            context.AddErrorMessage("PAYMENT_FAILED", paymentResult.ErrorMessage);
            return null;
        }

        context.AddInformationMessage("PAYMENT_PROCESSED", "Pagamento processado");

        // Persistir
        await _repository.SaveAsync(order);

        context.AddSuccessMessage("ORDER_CREATED", $"Pedido {order.Id} criado com sucesso");

        return order;
    }
}
```

**Quando usar:** Serviços de domínio que precisam rastrear múltiplas etapas.

---

### 3️⃣ Uso em Controllers/Handlers

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ITenantAccessor _tenantAccessor;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var context = ExecutionContext.Create(
            correlationId: Guid.Parse(HttpContext.TraceIdentifier),
            tenantInfo: _tenantAccessor.CurrentTenant,
            executionUser: User.Identity?.Name ?? "anonymous",
            executionOrigin: "API",
            businessOperationCode: "CREATE_ORDER",
            minimumMessageType: MessageType.Information,
            timeProvider: TimeProvider.System
        );

        var order = await _orderService.CreateOrder(context, request);

        if (context.IsSuccessful)
        {
            return CreatedAtAction(nameof(GetById), new { id = order!.Id }, order);
        }

        if (context.IsPartiallySuccessful)
        {
            return StatusCode(207, new
            {
                order,
                warnings = context.Messages.Where(m => m.MessageType == MessageType.Warning)
            });
        }

        // IsFaulted
        return BadRequest(new
        {
            errors = context.Messages
                .Where(m => m.MessageType >= MessageType.Error)
                .Select(m => new { m.Code, m.Text })
        });
    }
}
```

**Quando usar:** Camada de apresentação (controllers, handlers) para diagnóstico de resposta.

---

### 4️⃣ Uso em Processamento Paralelo

```csharp
public class BatchProcessor
{
    public async Task<BatchResult> ProcessBatch(ExecutionContext context, List<Item> items)
    {
        var itemContexts = new ConcurrentBag<ExecutionContext>();

        await Parallel.ForEachAsync(items, async (item, ct) =>
        {
            // Clone para cada item
            var itemContext = context.Clone();

            try
            {
                await ProcessItem(itemContext, item);
                itemContext.AddSuccessMessage("ITEM_PROCESSED", $"Item {item.Id} processado");
            }
            catch (Exception ex)
            {
                itemContext.AddException(ex);
                itemContext.AddErrorMessage("ITEM_FAILED", $"Item {item.Id}: {ex.Message}");
            }

            itemContexts.Add(itemContext);
        });

        // Agregar todos os contextos
        foreach (var itemContext in itemContexts)
        {
            context.Import(itemContext);
        }

        return new BatchResult
        {
            TotalItems = items.Count,
            SuccessCount = context.Messages.Count(m => m.MessageType == MessageType.Success),
            ErrorCount = context.Messages.Count(m => m.MessageType == MessageType.Error),
            IsPartialSuccess = context.IsPartiallySuccessful
        };
    }
}
```

**Quando usar:** Processamento em lote ou paralelo que precisa agregar resultados.

---

## 🎯 Decisões de Design

O `ExecutionContext` foi projetado com decisões intencionais que **não devem ser alteradas** sem justificativa forte:

### 1️⃣ Validações Limitadas ao Essencial

```csharp
// ✅ Valida apenas tipos referência não-nulos
ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
ArgumentException.ThrowIfNullOrWhiteSpace(executionUser, nameof(executionUser));
ArgumentException.ThrowIfNullOrWhiteSpace(executionOrigin, nameof(executionOrigin));
ArgumentException.ThrowIfNullOrWhiteSpace(businessOperationCode, nameof(businessOperationCode));

// ❌ NÃO valida regras de negócio
// Não valida Guid.Empty para correlationId
// Não valida TenantInfo.Code != Guid.Empty
```

**Por quê?** A camada de negócio garante valores válidos antes de criar o contexto. Adicionar validações de negócio violaria Single Responsibility Principle.

---

### 2️⃣ Propriedades Calculadas (Sem Cache)

```csharp
public bool HasErrorMessages
{
    get
    {
        // Itera a coleção a cada acesso
        foreach (Message message in _messageCollection.Values)
        {
            if (message.MessageType is MessageType.Error or MessageType.Critical)
                return true;
        }
        return false;
    }
}
```

**Por quê?** O padrão de uso é write-heavy (muitas mensagens) e read-light (poucas consultas no final). Cache adicionaria complexidade desnecessária e complicaria thread-safety.

---

### 3️⃣ ChangeMessageType Não Valida MinimumMessageType

```csharp
// Por ID - Permite "downgrade" de Error → Warning
context.ChangeMessageType(messageId, MessageType.Warning);

// Por ID - Permite "upgrade" de Warning → Error
context.ChangeMessageType(messageId, MessageType.Error);

// Por MessageType - Altera todas as mensagens de um tipo para outro
context.ChangeMessageType(MessageType.Warning, MessageType.Information);  // ✨ Todas as Warnings viram Information
```

**Por quê?** `MinimumMessageType` filtra na **entrada**; `ChangeMessageType` ajusta **semântica de negócio**. Exemplo: importação parcial bem-sucedida pode rebaixar erros para warnings.

---

### 4️⃣ ExecutionContext Observa, Não Controla

```csharp
// ✅ USO CORRETO: Método retorna seu próprio status
public Result ProcessOrder(Order order, ExecutionContext context)
{
    if (!ValidateOrder(order))
    {
        context.AddErrorMessage("INVALID_ORDER", "Validation failed");
        return Result.Failure("Validation failed");  // ✅ Retorna falha
    }

    context.AddSuccessMessage("ORDER_PROCESSED", $"Order {order.Id}");
    return Result.Success();  // ✅ Retorna sucesso
}

// ❌ USO INCORRETO: Usar context para controlar fluxo
public void ProcessOrder(Order order, ExecutionContext context)
{
    ValidateOrder(order, context);

    // ❌ NÃO FAÇA ISSO
    if (context.HasErrorMessages)
        return;

    Process(order, context);
}
```

**Por quê?** O `ExecutionContext` é um **observador passivo**. Métodos devem retornar seu próprio status (bool, Result<T>, exceções). As propriedades de diagnóstico (`IsSuccessful`, `IsFaulted`) são consultadas no **final** para auditoria e logging.

---

## ⚖️ Trade-offs

### Benefícios

| Benefício | Impacto | Análise |
|-----------|---------|---------|
| **Contexto centralizado** | ✅ Alto | Tenant, user, origin, businessOperationCode, correlation em um único objeto |
| **Thread-safe** | ✅ Alto | ConcurrentDictionary + ConcurrentBag |
| **Diagnóstico consolidado** | ✅ Alto | IsSuccessful, IsFaulted, IsPartiallySuccessful |
| **Níveis de severidade** | ✅ Médio | Trace, Debug, Info, Warning, Error, Critical, Success |
| **Filtragem por MinimumMessageType** | ✅ Médio | Reduz ruído em produção |
| **Clone/Import** | ✅ Médio | Suporte a operações paralelas |
| **Testabilidade** | ✅ Médio | TimeProvider injetado |

### Custos

| Custo | Impacto | Mitigação |
|-------|---------|-----------|
| **Reference type** | ⚠️ Baixo | Alocação única, reusado durante operação |
| **Iteração em propriedades** | ⚠️ Baixo | Padrão write-heavy/read-light |
| **Aprendizado inicial** | ⚠️ Baixo | Documentação e exemplos |

### Quando Usar vs Quando Evitar

#### ✅ Use quando:
1. Operação envolve múltiplos serviços/camadas
2. Precisa de contexto compartilhado (tenant, user, correlation)
3. Precisa coletar mensagens de múltiplas fontes
4. Precisa de diagnóstico pós-execução
5. Tem processamento paralelo que precisa agregar resultados
6. Precisa de auditoria/logging estruturado

#### ❌ Evite quando:
1. Operação é trivial (uma única chamada simples)
2. Não precisa de contexto compartilhado
3. Não precisa de rastreamento de mensagens
4. Performance é crítica e overhead mínimo importa

---

## 🔬 Exemplos Avançados

### 🏭 Pipeline de Processamento com Múltiplas Etapas

```csharp
public class OrderPipeline
{
    private readonly IValidator _validator;
    private readonly IInventoryService _inventory;
    private readonly IPaymentService _payment;
    private readonly IShippingService _shipping;
    private readonly INotificationService _notification;

    public async Task<PipelineResult> Execute(ExecutionContext context, Order order)
    {
        // Etapa 1: Validação
        context.AddInformationMessage("PIPELINE_STAGE", "Iniciando validação");

        var validationResult = await _validator.Validate(context, order);
        if (!validationResult.IsValid)
        {
            context.AddErrorMessage("VALIDATION_FAILED", "Pedido inválido");
            return PipelineResult.Failed(PipelineStage.Validation);
        }

        // Etapa 2: Reserva de Estoque
        context.AddInformationMessage("PIPELINE_STAGE", "Reservando estoque");

        var inventoryResult = await _inventory.Reserve(context, order);
        if (!inventoryResult.Success)
        {
            context.AddErrorMessage("INVENTORY_FAILED", "Estoque insuficiente");
            return PipelineResult.Failed(PipelineStage.Inventory);
        }

        // Etapa 3: Pagamento
        context.AddInformationMessage("PIPELINE_STAGE", "Processando pagamento");

        var paymentResult = await _payment.Process(context, order);
        if (!paymentResult.Success)
        {
            // Rollback do estoque
            await _inventory.Release(context, order);
            context.AddWarningMessage("INVENTORY_RELEASED", "Estoque liberado após falha de pagamento");
            context.AddErrorMessage("PAYMENT_FAILED", paymentResult.Error);
            return PipelineResult.Failed(PipelineStage.Payment);
        }

        // Etapa 4: Envio (não bloqueia sucesso)
        context.AddInformationMessage("PIPELINE_STAGE", "Agendando envio");

        var shippingResult = await _shipping.Schedule(context, order);
        if (!shippingResult.Success)
        {
            context.AddWarningMessage("SHIPPING_DELAYED", "Envio será agendado posteriormente");
            // Não retorna falha - pedido foi processado
        }

        // Etapa 5: Notificação (não bloqueia sucesso)
        context.AddInformationMessage("PIPELINE_STAGE", "Enviando notificações");

        try
        {
            await _notification.Send(context, order);
        }
        catch (Exception ex)
        {
            context.AddWarningMessage("NOTIFICATION_FAILED", "Notificação falhou, será reenviada");
            context.AddException(ex);
            // Não retorna falha - pedido foi processado
        }

        context.AddSuccessMessage("ORDER_COMPLETED", $"Pedido {order.Id} processado com sucesso");

        return PipelineResult.Success(order);
    }
}

// Uso:
var context = ExecutionContext.Create(...);
var result = await pipeline.Execute(context, order);

// Log estruturado com todo o histórico
_logger.LogInformation(
    "Pipeline completed. Result: {Result}, Stages: {@Stages}, Warnings: {WarningCount}",
    result.Success ? "Success" : $"Failed at {result.FailedStage}",
    context.Messages.Where(m => m.Code == "PIPELINE_STAGE"),
    context.Messages.Count(m => m.MessageType == MessageType.Warning)
);
```

**Pontos importantes:**
- Cada etapa adiciona mensagens informativas
- Falhas críticas (validação, estoque, pagamento) retornam imediatamente
- Falhas não-críticas (envio, notificação) geram warnings mas não bloqueiam
- Histórico completo disponível para logging e auditoria

---

### 🔄 Saga Pattern com Compensação

```csharp
public class OrderSaga
{
    public async Task<SagaResult> Execute(ExecutionContext context, Order order)
    {
        var completedSteps = new Stack<Func<Task>>();

        try
        {
            // Step 1: Reserve Inventory
            await _inventory.Reserve(context, order);
            completedSteps.Push(() => _inventory.Release(context, order));
            context.AddSuccessMessage("SAGA_STEP", "Inventory reserved");

            // Step 2: Charge Payment
            await _payment.Charge(context, order);
            completedSteps.Push(() => _payment.Refund(context, order));
            context.AddSuccessMessage("SAGA_STEP", "Payment charged");

            // Step 3: Create Shipment
            await _shipping.Create(context, order);
            completedSteps.Push(() => _shipping.Cancel(context, order));
            context.AddSuccessMessage("SAGA_STEP", "Shipment created");

            context.AddSuccessMessage("SAGA_COMPLETED", "Order saga completed successfully");
            return SagaResult.Success();
        }
        catch (Exception ex)
        {
            context.AddException(ex);
            context.AddErrorMessage("SAGA_FAILED", $"Saga failed: {ex.Message}");

            // Compensate in reverse order
            context.AddWarningMessage("SAGA_COMPENSATING", "Initiating compensation...");

            while (completedSteps.Count > 0)
            {
                var compensate = completedSteps.Pop();
                try
                {
                    await compensate();
                    context.AddInformationMessage("SAGA_COMPENSATED", "Step compensated");
                }
                catch (Exception compensateEx)
                {
                    context.AddException(compensateEx);
                    context.AddCriticalMessage(
                        "SAGA_COMPENSATION_FAILED",
                        $"Compensation failed: {compensateEx.Message}"
                    );
                }
            }

            return SagaResult.Failed(ex);
        }
    }
}
```

**Pontos importantes:**
- Cada step bem-sucedido adiciona função de compensação
- Falha dispara compensação em ordem reversa
- Falhas de compensação são capturadas mas não interrompem outras compensações
- Histórico completo para análise posterior

---

## 📚 Referências

- [Correlation ID Pattern](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CorrelationIdentifier.html) - Enterprise Integration Patterns
- [Structured Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/) - Microsoft Docs
- [Saga Pattern](https://microservices.io/patterns/data/saga.html) - Microservices.io
- [ConcurrentDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2) - Thread-safe dictionary
- [ConcurrentBag](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentbag-1) - Thread-safe unordered collection
