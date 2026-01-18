# 🏢 TenantInfo - Identificação de Tenant em Sistemas Multi-Tenant

A estrutura `TenantInfo` encapsula as informações de identificação de um tenant (inquilino) em aplicações multi-tenant. Fornece uma abstração leve e imutável para representar o contexto de tenant em operações de domínio.

> 💡 **Visão Geral:** Estrutura imutável de **24 bytes** (Guid + referência) para identificação de tenant, com suporte a código único e nome opcional — perfeita para isolamento de dados em sistemas SaaS.

---

## 📋 Sumário

- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
  - [Acoplamento Direto ao Guid do Tenant](#1-️-acoplamento-direto-ao-guid-do-tenant)
  - [Inconsistência na Passagem de Informações de Tenant](#2--inconsistência-na-passagem-de-informações-de-tenant)
- [Funcionalidades](#-funcionalidades)
- [Como Usar](#-como-usar)
- [Integração com Outros Building Blocks](#-integração-com-outros-building-blocks)
- [Trade-offs](#️-tradeoffs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)

---

## 🎯 Contexto: Por Que Existe

### O Problema Real

Em aplicações multi-tenant (SaaS), cada operação precisa estar associada a um tenant específico para garantir isolamento de dados. As abordagens tradicionais apresentam problemas sérios:

**Exemplo de desafios comuns:**

```csharp
❌ Abordagem 1: Passar Guid do tenant diretamente
public class OrderService
{
    public Order CreateOrder(
        Guid tenantId,           // ⚠️ Apenas o código
        string tenantName,       // ⚠️ Passado separadamente
        CreateOrderRequest request
    )
    {
        // ⚠️ Precisa passar dois parâmetros sempre juntos
        // ⚠️ Fácil esquecer um ou passar inconsistente
        var order = new Order
        {
            TenantId = tenantId,
            TenantName = tenantName,  // ⚠️ Pode estar dessincronizado!
            // ...
        };

        return order;
    }
}

❌ Problemas:
- Parâmetros separados que deveriam estar juntos
- Fácil passar valores inconsistentes (código de um tenant, nome de outro)
- Métodos com muitos parâmetros (parameter bloat)
- Sem validação centralizada
- Difícil refatorar quando precisar adicionar mais informações
```

```csharp
❌ Abordagem 2: Criar classe mutável para tenant
public class TenantContext
{
    public Guid Id { get; set; }      // ⚠️ Mutável!
    public string Name { get; set; }  // ⚠️ Pode ser alterado a qualquer momento
}

public class OrderService
{
    public Order CreateOrder(TenantContext tenant, CreateOrderRequest request)
    {
        // ⚠️ Outro código pode alterar tenant.Id durante a execução!
        tenant.Id = Guid.NewGuid();  // BUG: Alterou o contexto global!

        var order = new Order { TenantId = tenant.Id };
        return order;
    }
}

❌ Problemas:
- Mutabilidade permite alterações acidentais
- Estado compartilhado pode causar race conditions
- Difícil rastrear quem alterou o tenant
- Não é thread-safe
- Referência pode ser null
```

### A Solução

O `TenantInfo` implementa uma estrutura **imutável** e **value type** para representar informações de tenant.

```csharp
✅ Abordagem com TenantInfo:
public class OrderService
{
    public Order CreateOrder(
        ExecutionContext executionContext,  // ✨ Contém TenantInfo
        CreateOrderRequest request
    )
    {
        // ✨ TenantInfo é imutável e sempre consistente
        var tenantInfo = executionContext.TenantInfo;

        var order = new Order
        {
            TenantCode = tenantInfo.Code,    // ✅ Sempre válido
            TenantName = tenantInfo.Name,    // ✅ Opcional, mas consistente
            // ...
        };

        return order;
    }
}

✅ Benefícios:
- Imutabilidade: Impossível alterar após criação
- Value type: Sem alocações extras, comparação por valor
- Encapsulamento: Código e nome sempre juntos
- Thread-safe: Pode ser compartilhado entre threads
- Integração: Funciona com ExecutionContext e EntityInfo
```

**Estrutura do TenantInfo:**

```
┌──────────────────────────────────────────────────────────────────────────┐
│                     ESTRUTURA DO TENANTINFO                              │
└──────────────────────────────────────────────────────────────────────────┘
│                                                                           │
│   readonly record struct TenantInfo                                      │
│   ├── Code: Guid (16 bytes)     → Identificador único do tenant          │
│   └── Name: string? (8 bytes)   → Nome legível (opcional, referência)    │
│                                                                           │
│   Características:                                                        │
│   ├── Imutável (readonly record struct)                                  │
│   ├── Value type (alocado na stack)                                      │
│   ├── Comparação por valor (record)                                      │
│   └── Factory method (Create) para criação controlada                    │
│                                                                           │
│   Tamanho em memória: ~24 bytes (16 Guid + 8 referência string)         │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Problemas Resolvidos

### 1. 🔗 Acoplamento Direto ao Guid do Tenant

**Problema:** Passar `Guid` diretamente acopla o código ao tipo primitivo e dificulta extensões futuras.

#### 📚 Analogia: O Crachá de Identificação

Imagine que você gerencia um prédio comercial com várias empresas (tenants):

**❌ Com Guid direto:**

```
Visitante chega na recepção:

Recepcionista: "Qual o código da empresa?"
Visitante: "12345678-1234-1234-1234-123456789012"

⚠️ PROBLEMAS:
1. Recepcionista não sabe QUAL empresa é só pelo código
2. Precisa consultar outro sistema para descobrir o nome
3. Se precisar do andar, precisa outra consulta
4. Informações espalhadas em vários lugares
```

**✅ Com TenantInfo (Crachá):**

```
Visitante chega na recepção:

Recepcionista entrega um CRACHÁ com:
┌─────────────────────────────┐
│ EMPRESA: Acme Corp          │  ← Name
│ CÓDIGO: 12345678-...        │  ← Code
└─────────────────────────────┘

✅ BENEFÍCIOS:
1. Todas as informações juntas
2. Fácil identificar visualmente
3. Pode adicionar mais campos no futuro (andar, setor)
4. Crachá é imutável (não pode ser alterado pelo visitante)
```

#### 💻 Impacto Real no Código

**❌ Código com Guid direto:**

```csharp
public class AuditService
{
    public void LogAction(
        Guid tenantId,      // ⚠️ Só o código
        string action,
        string userId
    )
    {
        // Precisa buscar o nome do tenant em outro lugar
        var tenantName = _tenantRepository.GetName(tenantId);  // ⚠️ Query extra!

        _logger.LogInformation(
            "Tenant {TenantName} ({TenantId}): {Action} by {User}",
            tenantName,
            tenantId,
            action,
            userId
        );
    }
}

// Chamada:
auditService.LogAction(
    tenantId: Guid.Parse("..."),
    action: "CreateOrder",
    userId: "user@email.com"
);

❌ Problemas:
- Query extra para buscar nome
- Parâmetros primitivos sem contexto
- Fácil passar parâmetros na ordem errada
```

**✅ Código com TenantInfo:**

```csharp
public class AuditService
{
    public void LogAction(
        TenantInfo tenantInfo,  // ✨ Código E nome juntos
        string action,
        string userId
    )
    {
        // Tudo disponível diretamente
        _logger.LogInformation(
            "Tenant {TenantName} ({TenantCode}): {Action} by {User}",
            tenantInfo.Name ?? "Unknown",
            tenantInfo.Code,
            action,
            userId
        );
    }
}

// Chamada:
auditService.LogAction(
    tenantInfo: executionContext.TenantInfo,  // ✨ Já vem pronto
    action: "CreateOrder",
    userId: executionContext.ExecutionUser
);

✅ Benefícios:
- Sem queries extras
- Informações encapsuladas
- Impossível passar na ordem errada
- Fácil estender no futuro
```

---

### 2. 📦 Inconsistência na Passagem de Informações de Tenant

**Problema:** Quando código e nome são passados separadamente, podem ficar dessincronizados.

#### 📚 Analogia: O Formulário Pré-Preenchido

Imagine preencher formulários em um cartório:

**❌ Sem TenantInfo:**

```
Formulário 1: Nome da Empresa: "Acme Corp"
Formulário 2: Código da Empresa: "12345678-..."
Formulário 3: Nome da Empresa: "ACME Corporation"  ← DIFERENTE!

⚠️ PROBLEMA: Cada formulário pode ter informações diferentes!
```

**✅ Com TenantInfo:**

```
Carimbo único aplicado em TODOS os formulários:
┌─────────────────────────────┐
│ Acme Corp | 12345678-...    │
└─────────────────────────────┘

✅ Todos os formulários têm a MESMA informação, garantida!
```

#### 💻 Impacto Real no Código

**❌ Código com valores separados:**

```csharp
public class ReportService
{
    public Report GenerateReport(
        Guid tenantCode,
        string tenantName,
        ReportRequest request
    )
    {
        var report = new Report
        {
            Header = new ReportHeader
            {
                TenantCode = tenantCode,
                TenantName = tenantName  // ⚠️ Pode não corresponder ao Code!
            }
        };

        // ... gera relatório

        return report;
    }
}

// Chamada ERRADA (compila, mas está errado!):
var report = reportService.GenerateReport(
    tenantCode: tenant1.Code,      // ⚠️ Código do tenant 1
    tenantName: tenant2.Name,      // ⚠️ Nome do tenant 2!
    request: request
);

❌ Problemas:
- Compilador não detecta inconsistência
- Bug difícil de encontrar
- Relatório com dados misturados
```

**✅ Código com TenantInfo:**

```csharp
public class ReportService
{
    public Report GenerateReport(
        TenantInfo tenantInfo,  // ✨ Código e nome SEMPRE consistentes
        ReportRequest request
    )
    {
        var report = new Report
        {
            Header = new ReportHeader
            {
                TenantCode = tenantInfo.Code,
                TenantName = tenantInfo.Name  // ✅ Garantido ser do mesmo tenant!
            }
        };

        // ... gera relatório

        return report;
    }
}

// Chamada SEGURA:
var report = reportService.GenerateReport(
    tenantInfo: executionContext.TenantInfo,  // ✅ Sempre consistente
    request: request
);

✅ Benefícios:
- Impossível passar dados inconsistentes
- Um único parâmetro = uma única fonte de verdade
- Código mais limpo e seguro
```

---

## ✨ Funcionalidades

### 🔒 Imutabilidade Garantida

Estrutura `readonly record struct` garante que valores não podem ser alterados após criação.

```csharp
var tenantInfo = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Acme Corp"
);

// ❌ Não compila - propriedades são readonly
// tenantInfo.Code = Guid.NewGuid();
// tenantInfo.Name = "Outro Nome";

// ✅ Para "alterar", crie uma nova instância
var updatedTenant = tenantInfo.WithName("Acme Corporation");
```

**Por quê é importante?**
- Thread-safe por design
- Sem efeitos colaterais inesperados
- Pode ser compartilhado livremente entre métodos

---

### 🏭 Factory Method Controlado

Criação via método `Create` permite validação e evolução futura.

```csharp
// ✅ Criação via factory method
var tenantInfo = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Acme Corp"
);

// ✅ Nome é opcional
var tenantSemNome = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: null
);
```

**Por quê usar factory method?**
- Permite adicionar validações no futuro
- Nome do método expressa intenção
- Pode retornar tipos diferentes (ex: Result<TenantInfo>) se necessário

---

### 🔄 Método WithName para Atualizações

Padrão funcional para criar cópia com nome alterado.

```csharp
var original = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Acme Corp"
);

// ✨ Cria nova instância com nome atualizado
var atualizado = original.WithName("Acme Corporation");

// Original permanece inalterado
Console.WriteLine(original.Name);    // "Acme Corp"
Console.WriteLine(atualizado.Name);  // "Acme Corporation"

// Código permanece o mesmo
Console.WriteLine(original.Code == atualizado.Code);  // True
```

**Por quê usar padrão With?**
- Mantém imutabilidade
- Expressa intenção claramente
- Facilita debugging (versão anterior preservada)

---

### ⚖️ Comparação por Valor

Como `record struct`, compara todos os campos automaticamente.

```csharp
var tenant1 = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Acme Corp"
);

var tenant2 = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Acme Corp"
);

var tenant3 = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Outro Nome"
);

Console.WriteLine(tenant1 == tenant2);  // True (mesmo código E nome)
Console.WriteLine(tenant1 == tenant3);  // False (nome diferente)

// Funciona em HashSet e Dictionary
var tenants = new HashSet<TenantInfo> { tenant1 };
Console.WriteLine(tenants.Contains(tenant2));  // True
```

---

## 🚀 Como Usar

### 1️⃣ Uso Básico - Criação Simples

```csharp
using Bedrock.BuildingBlocks.Core.TenantInfos;

// Criar TenantInfo com código e nome
var tenantInfo = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Acme Corporation"
);

Console.WriteLine($"Tenant: {tenantInfo.Name} ({tenantInfo.Code})");
// Saída: Tenant: Acme Corporation (12345678-1234-1234-1234-123456789012)
```

**Quando usar:** Criação manual de TenantInfo para testes ou configuração inicial.

---

### 2️⃣ Uso com ExecutionContext

```csharp
using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.TenantInfos;

// Criar TenantInfo
var tenantInfo = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Acme Corp"
);

// Criar ExecutionContext com TenantInfo
var executionContext = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),
    tenantInfo: tenantInfo,                    // ✨ TenantInfo encapsulado
    executionUser: "user@acme.com",
    minimumMessageType: MessageType.Information,
    timeProvider: TimeProvider.System
);

// Acessar TenantInfo do contexto
Console.WriteLine($"Executando para: {executionContext.TenantInfo.Name}");
```

**Quando usar:** Aplicações que usam ExecutionContext para rastrear operações.

---

### 3️⃣ Uso com EntityInfo (Entidades de Domínio)

```csharp
using Bedrock.BuildingBlocks.Domain.Entities.Models;

public class Order : EntityBase<Order>
{
    public string Description { get; private set; }

    private Order() { }

    public static Order? RegisterNew(
        ExecutionContext executionContext,
        string description
    )
    {
        return RegisterNewInternal<Order, string>(
            executionContext,
            input: description,
            entityFactory: (ctx, desc) => new Order(),
            handler: (ctx, desc, entity) =>
            {
                entity.Description = desc;
                return true;
            }
        );
    }

    public override IEntity<Order> Clone() => /* ... */;
}

// Uso:
var order = Order.RegisterNew(executionContext, "Pedido #001");

// ✨ TenantInfo é propagado automaticamente do ExecutionContext para EntityInfo
Console.WriteLine($"Order tenant: {order.EntityInfo.TenantInfo.Code}");
```

**Quando usar:** Entidades de domínio em sistemas multi-tenant.

---

### 4️⃣ Uso para Atualização de Nome

```csharp
// Cenário: Tenant mudou de nome (rebranding)
var original = TenantInfo.Create(
    code: Guid.Parse("12345678-1234-1234-1234-123456789012"),
    name: "Acme Corp"
);

// Criar nova instância com nome atualizado
var atualizado = original.WithName("Acme International");

// Usar em novo contexto
var novoContexto = ExecutionContext.Create(
    correlationId: Guid.NewGuid(),
    tenantInfo: atualizado,  // ✨ Nome atualizado, código mantido
    executionUser: "admin@acme.com",
    minimumMessageType: MessageType.Information,
    timeProvider: TimeProvider.System
);
```

**Quando usar:** Atualização de informações do tenant mantendo o código.

---

## 🔗 Integração com Outros Building Blocks

O `TenantInfo` integra-se com outros building blocks do framework:

```
┌──────────────────────────────────────────────────────────────────────────┐
│                     INTEGRAÇÃO DO TENANTINFO                             │
└──────────────────────────────────────────────────────────────────────────┘
│                                                                           │
│   ExecutionContext                                                        │
│   ├── TenantInfo ─────────────────┐                                      │
│   ├── CorrelationId               │                                      │
│   ├── ExecutionUser               │                                      │
│   └── TimeProvider                │                                      │
│                                   │                                      │
│   EntityInfo ◄────────────────────┤ (propagado automaticamente)          │
│   ├── Id                          │                                      │
│   ├── TenantInfo ◄────────────────┘                                      │
│   ├── EntityChangeInfo                                                   │
│   └── EntityVersion                                                      │
│                                                                           │
│   EntityBase                                                              │
│   └── EntityInfo                                                         │
│       └── TenantInfo ← Usado para isolamento de dados por tenant         │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘
```

**Fluxo de propagação:**

```csharp
// 1. TenantInfo criado na entrada da aplicação (ex: middleware)
var tenantInfo = TenantInfo.Create(tenantCode, tenantName);

// 2. Encapsulado no ExecutionContext
var context = ExecutionContext.Create(..., tenantInfo, ...);

// 3. Propagado para EntityInfo ao criar entidades
var entity = MyEntity.RegisterNew(context, ...);
// entity.EntityInfo.TenantInfo == context.TenantInfo ✅

// 4. Persistido no banco de dados para isolamento
// WHERE TenantCode = @tenantCode
```

---

## ⚖️ Trade-offs

### Benefícios

| Benefício | Impacto | Análise |
|-----------|---------|---------|
| **Imutabilidade** | ✅ Alto | Thread-safe, sem efeitos colaterais, debugging facilitado |
| **Encapsulamento** | ✅ Alto | Código e nome sempre juntos, impossível dessincronizar |
| **Value Type** | ✅ Médio | Sem alocação no heap, comparação por valor eficiente |
| **Integração** | ✅ Alto | Funciona nativamente com ExecutionContext e EntityInfo |
| **Extensibilidade** | ✅ Médio | Factory method permite adicionar campos/validações no futuro |

### Custos

| Custo | Impacto | Mitigação |
|-------|---------|-----------|
| **Cópia em cada alteração** | ⚠️ Baixo | `WithName` cria cópia, mas structs são leves (~24 bytes) |
| **Null check para Name** | ⚠️ Baixo | Usar `?.` ou `?? "Default"` quando acessar Name |

### Quando Usar vs Quando Evitar

#### ✅ Use quando:
1. Precisa identificar tenant em operações de domínio
2. Usa ExecutionContext para rastrear execuções
3. Tem entidades que precisam de isolamento por tenant
4. Quer garantir consistência entre código e nome do tenant
5. Precisa passar informações de tenant entre camadas

#### ❌ Evite quando:
1. Aplicação não é multi-tenant (single-tenant)
2. Não precisa do nome do tenant (use `Guid` diretamente)
3. Precisa de informações muito extensas de tenant (crie um modelo específico)

---

## 🔬 Exemplos Avançados

### 🏭 Middleware de Extração de Tenant

```csharp
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantResolver _tenantResolver;

    public TenantMiddleware(RequestDelegate next, ITenantResolver tenantResolver)
    {
        _next = next;
        _tenantResolver = tenantResolver;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        // Extrair código do tenant do header ou rota
        var tenantCode = ExtractTenantCode(httpContext);

        // Resolver nome do tenant (pode vir de cache ou banco)
        var tenantName = await _tenantResolver.GetTenantNameAsync(tenantCode);

        // Criar TenantInfo
        var tenantInfo = TenantInfo.Create(
            code: tenantCode,
            name: tenantName
        );

        // Criar ExecutionContext e disponibilizar via DI
        var executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: httpContext.User.Identity?.Name ?? "anonymous",
            minimumMessageType: MessageType.Information,
            timeProvider: TimeProvider.System
        );

        // Disponibilizar para a requisição
        httpContext.Items["ExecutionContext"] = executionContext;

        await _next(httpContext);
    }

    private Guid ExtractTenantCode(HttpContext context)
    {
        // Exemplo: extrair de header X-Tenant-Id
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader))
        {
            if (Guid.TryParse(tenantHeader, out var tenantCode))
                return tenantCode;
        }

        throw new UnauthorizedAccessException("Tenant não identificado");
    }
}
```

**Pontos importantes:**
- TenantInfo criado uma vez por requisição
- Propagado via ExecutionContext para toda a aplicação
- Imutabilidade garante consistência durante toda a requisição

---

### 🧪 Testes com TenantInfo Fixo

```csharp
public class OrderServiceTests
{
    private readonly TenantInfo _testTenant = TenantInfo.Create(
        code: Guid.Parse("00000000-0000-0000-0000-000000000001"),
        name: "Test Tenant"
    );

    [Fact]
    public void CreateOrder_ShouldAssociateTenant()
    {
        // Arrange
        var context = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: _testTenant,
            executionUser: "test@test.com",
            minimumMessageType: MessageType.Information,
            timeProvider: TimeProvider.System
        );

        // Act
        var order = Order.RegisterNew(context, "Test Order");

        // Assert
        Assert.NotNull(order);
        Assert.Equal(_testTenant.Code, order.EntityInfo.TenantInfo.Code);
        Assert.Equal(_testTenant.Name, order.EntityInfo.TenantInfo.Name);
    }

    [Fact]
    public void TenantInfo_ShouldCompareByValue()
    {
        // Arrange
        var tenant1 = TenantInfo.Create(
            code: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            name: "Test"
        );

        var tenant2 = TenantInfo.Create(
            code: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            name: "Test"
        );

        // Act & Assert
        Assert.Equal(tenant1, tenant2);
        Assert.True(tenant1 == tenant2);
    }
}
```

**Pontos importantes:**
- TenantInfo fixo para testes determinísticos
- Comparação por valor facilita assertions
- Mesmo padrão usado em produção

---

## 📚 Referências

- [Multi-tenancy Patterns](https://docs.microsoft.com/en-us/azure/architecture/guide/multitenant/overview) - Microsoft Azure Architecture Guide
- [Record Structs in C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) - Documentação oficial do C#
- [Immutability in C#](https://docs.microsoft.com/en-us/dotnet/csharp/write-safe-efficient-code) - Padrões de código seguro e eficiente
