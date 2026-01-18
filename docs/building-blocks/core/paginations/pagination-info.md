# 📄 PaginationInfo - Paginação Type-Safe com Ordenação e Filtros

A classe `PaginationInfo` fornece uma estrutura imutável e type-safe para representar informações de paginação, ordenação e filtros em consultas. Ideal para APIs REST, queries de banco de dados e integração segura entre front-end e back-end.

> 💡 **Visão Geral:** Estruture suas consultas paginadas de forma **segura** e **validada**, com suporte a ordenação múltipla e filtros dinâmicos, tudo em um único objeto imutável.

---

## 📋 Sumário

- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
  - [Strings Arbitrárias de Ordenação/Filtro](#1-️-strings-arbitrárias-de-ordenaçãofiltro-sql-injection)
  - [Parâmetros de Paginação Espalhados](#2--parâmetros-de-paginação-espalhados)
  - [Falta de Validação Consistente](#3--falta-de-validação-consistente)
- [Funcionalidades](#-funcionalidades)
- [Arquitetura: Fluxo Seguro Front-to-Back](#-arquitetura-fluxo-seguro-front-to-back)
- [Como Usar](#-como-usar)
  - [Todos os Registros (Unbounded)](#6️⃣-todos-os-registros-unbounded)
- [Estruturas Relacionadas](#-estruturas-relacionadas)
- [Trade-offs](#-tradeoffs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)

---

## 🎯 Contexto: Por Que Existe

### O Problema Real

Em aplicações web, paginação, ordenação e filtros são requisitos fundamentais. No entanto, as abordagens tradicionais apresentam sérios problemas de segurança e manutenção:

**Exemplo de desafios comuns:**

```csharp
❌ Abordagem 1: Parâmetros soltos na API
public async Task<IActionResult> GetUsers(
    int page,
    int pageSize,
    string? sortBy,      // ⚠️ String arbitrária!
    string? sortOrder,   // ⚠️ "asc" ou "desc" ou qualquer coisa...
    string? filterField, // ⚠️ Qual campo? Validado?
    string? filterValue  // ⚠️ SQL Injection possível!
)
{
    var query = _context.Users
        .OrderBy($"{sortBy} {sortOrder}")  // 💥 SQL INJECTION!
        .Skip((page - 1) * pageSize)
        .Take(pageSize);
}

❌ Problemas:
- SQL Injection via sortBy/filterField
- Sem validação de campos permitidos
- Fácil esquecer validações em novos endpoints
- Parâmetros repetidos em toda a aplicação
- Difícil manter consistência
```

```csharp
❌ Abordagem 2: Cálculos de offset repetidos
// Em 10 lugares diferentes do código:
var offset1 = (page - 1) * pageSize;
var offset2 = (currentPage - 1) * itemsPerPage;
var skip = (pageNumber - 1) * limit;  // ⚠️ Nomes inconsistentes!

❌ Problemas:
- Cálculo repetido em múltiplos lugares (DRY violado)
- Nomes inconsistentes (page, currentPage, pageNumber)
- Fácil errar o cálculo (page - 1 vs page)
- Sem validação de valores negativos
```

```csharp
❌ Abordagem 3: Ordenação via strings concatenadas
var sortField = request.SortBy;
var sortDirection = request.SortOrder;

// Tentativa de "validação"
if (sortField == "name" || sortField == "email" || sortField == "createdAt")
{
    query = query.OrderBy($"{sortField} {sortDirection}");
}

❌ Problemas:
- Whitelist espalhada pelo código
- Cada endpoint repete a validação
- Fácil esquecer de atualizar quando adicionar novo campo
- Sem Single Source of Truth
```

### A Solução

O `PaginationInfo` centraliza todas as informações de paginação, ordenação e filtros em um único objeto imutável e validado.

```csharp
✅ Abordagem com PaginationInfo:
public async Task<IActionResult> GetUsers([FromBody] QueryRequest request)
{
    // 1. Criar PaginationInfo validado
    var pagination = PaginationInfo.Create(
        page: request.Page,
        pageSize: request.PageSize,
        sortCollection: request.SortCollection,
        filterCollection: request.FilterCollection
    );

    // 2. Usar propriedades calculadas automaticamente
    var query = _context.Users
        .Skip(pagination.Offset)   // ✨ Calculado automaticamente!
        .Take(pagination.PageSize);

    // 3. Ordenação e filtros validados na camada Infra.Data
    return Ok(await _repository.QueryAsync(pagination));
}

✅ Benefícios:
- Imutável: readonly struct, sem modificações acidentais
- Validado: Page e PageSize sempre > 0
- Calculado: Index e Offset derivados automaticamente
- Type-safe: SortInfo e FilterInfo são structs tipadas
- Seguro: Validação de campos na camada Infra.Data
- DRY: Uma única definição, usada em toda aplicação
```

**Estrutura do PaginationInfo:**

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    ESTRUTURA DO PAGINATIONINFO                           │
└──────────────────────────────────────────────────────────────────────────┘
│                                                                          │
│  PROPRIEDADES OBRIGATÓRIAS:                                              │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ Page (int)      → Número da página (1-indexed, mínimo 1)          │  │
│  │ PageSize (int)  → Itens por página (mínimo 1)                     │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  PROPRIEDADES CALCULADAS:                                                │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ Index (int)     → Page - 1 (base-zero)                            │  │
│  │ Offset (int)    → Index * PageSize (itens a pular)                │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  PROPRIEDADES OPCIONAIS:                                                 │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ SortCollection (IReadOnlyList<SortInfo>?)                         │  │
│  │   → Lista de ordenações (Field + Direction)                       │  │
│  │   → Exemplo: [LastName ASC, FirstName ASC, CreatedAt DESC]        │  │
│  │                                                                    │  │
│  │ FilterCollection (IReadOnlyList<FilterInfo>?)                     │  │
│  │   → Lista de filtros (Field + Operator + Value)                   │  │
│  │   → Exemplo: [Status = "Active", Name Contains "Silva"]           │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  PROPRIEDADES DE CONVENIÊNCIA:                                           │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ HasSort (bool)     → SortCollection não é nulo e tem itens        │  │
│  │ HasFilter (bool)   → FilterCollection não é nulo e tem itens      │  │
│  │ IsUnbounded (bool) → PageSize == int.MaxValue (todos os registros)│  │
│  └────────────────────────────────────────────────────────────────────┘  │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Problemas Resolvidos

### 1. 🛡️ Strings Arbitrárias de Ordenação/Filtro (SQL Injection)

**Problema:** Receber strings diretamente do front-end para ordenação e filtros abre brechas de segurança.

#### 📚 Analogia: O Porteiro Descuidado

Imagine um prédio com um porteiro que aceita qualquer instrução:

**❌ Sem validação (porteiro descuidado):**

```
Visitante: "Quero ir ao apartamento 501"
Porteiro: "Ok, pode subir" ✅

Visitante: "Quero ir ao apartamento 501; DROP TABLE moradores;--"
Porteiro: "Ok, pode subir" 💥 SQL INJECTION!

Visitante: "Quero ir à sala do servidor"
Porteiro: "Ok, pode subir" 💥 Acesso não autorizado!
```

**✅ Com validação (porteiro rigoroso):**

```
Visitante: "Quero ir ao apartamento 501"
Porteiro: *Verifica lista de apartamentos válidos*
Porteiro: "Apartamento 501 existe. Pode subir" ✅

Visitante: "Quero ir ao apartamento 501; DROP TABLE moradores;--"
Porteiro: *Verifica lista de apartamentos válidos*
Porteiro: "Apartamento inválido. Acesso negado!" ❌

Visitante: "Quero ir à sala do servidor"
Porteiro: *Verifica lista de locais permitidos*
Porteiro: "Local não autorizado. Acesso negado!" ❌
```

#### 💻 Impacto Real na Aplicação

**❌ Código vulnerável:**

```csharp
public async Task<IEnumerable<User>> GetUsersAsync(string sortBy, string sortOrder)
{
    // ⚠️ PERIGO: String vinda diretamente do front-end!
    var sql = $"SELECT * FROM Users ORDER BY {sortBy} {sortOrder}";
    return await _connection.QueryAsync<User>(sql);
}

// Front-end malicioso envia:
// sortBy = "1; DROP TABLE Users;--"
// sortOrder = ""

// SQL resultante:
// SELECT * FROM Users ORDER BY 1; DROP TABLE Users;--
// 💥 TABELA DELETADA!
```

**✅ Código seguro com PaginationInfo + QueryContract:**

```csharp
// Infra.Data - Define campos permitidos (whitelist)
public enum UserSortField
{
    Name,
    Email,
    CreatedAt
}

public static class UserQueryContract
{
    private static readonly Dictionary<string, UserSortField> AllowedFields = new()
    {
        ["name"] = UserSortField.Name,
        ["email"] = UserSortField.Email,
        ["createdAt"] = UserSortField.CreatedAt
    };

    public static bool TryParseSortField(string? input, out UserSortField field)
    {
        field = default;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return AllowedFields.TryGetValue(input.ToLowerInvariant(), out field);
    }
}

// Repository - Valida antes de usar
public async Task<IEnumerable<User>> GetUsersAsync(PaginationInfo pagination)
{
    // Validar cada sort contra whitelist
    foreach (var sort in pagination.SortCollection ?? [])
    {
        if (!UserQueryContract.TryParseSortField(sort.Field, out var validField))
            throw new InvalidSortFieldException(sort.Field);

        // Usa enum validado, não string!
    }

    // Agora é seguro usar
    var columnMap = new Dictionary<UserSortField, string>
    {
        [UserSortField.Name] = "user_name",
        [UserSortField.Email] = "email_address",
        [UserSortField.CreatedAt] = "created_at"
    };

    // SQL seguro com valores do enum (impossível injeção)
}
```

---

### 2. 📦 Parâmetros de Paginação Espalhados

**Problema:** Cálculos de offset repetidos em múltiplos lugares do código.

```csharp
❌ Código repetitivo:

// UserController.cs
var offset = (request.Page - 1) * request.PageSize;
var users = await _userService.GetUsersAsync(offset, request.PageSize);

// ProductController.cs
var skip = (request.PageNumber - 1) * request.Limit;  // Nomes diferentes!
var products = await _productService.GetProductsAsync(skip, request.Limit);

// OrderController.cs
var startIndex = (request.CurrentPage - 1) * request.ItemsPerPage;  // Mais variação!
var orders = await _orderService.GetOrdersAsync(startIndex, request.ItemsPerPage);

// ReportService.cs
var offset = request.Page * request.PageSize;  // 💥 BUG! Faltou o -1!
var reports = await _reportRepository.GetReportsAsync(offset, request.PageSize);

❌ Problemas:
- Cálculo repetido 4+ vezes
- Nomes inconsistentes (page, pageNumber, currentPage)
- Bug introduzido no ReportService (esqueceu -1)
- Difícil manter sincronizado
```

**✅ Código com PaginationInfo:**

```csharp
✅ Código centralizado:

// Todos os controllers usam a mesma estrutura
public async Task<IActionResult> GetUsers([FromBody] QueryRequest request)
{
    var pagination = PaginationInfo.Create(request.Page, request.PageSize);

    // Offset calculado AUTOMATICAMENTE e CORRETAMENTE
    var users = await _userService.GetUsersAsync(pagination);

    return Ok(new
    {
        Data = users,
        Page = pagination.Page,
        PageSize = pagination.PageSize,
        Offset = pagination.Offset  // Sempre correto!
    });
}

// Service usa diretamente
public async Task<IEnumerable<User>> GetUsersAsync(PaginationInfo pagination)
{
    return await _context.Users
        .Skip(pagination.Offset)    // ✨ Sempre correto!
        .Take(pagination.PageSize)
        .ToListAsync();
}

✅ Benefícios:
- Cálculo em um único lugar (dentro do PaginationInfo)
- Nomenclatura consistente em toda aplicação
- Impossível errar o cálculo (é automático)
- Validação garantida (Page e PageSize > 0)
```

---

### 3. ✅ Falta de Validação Consistente

**Problema:** Validações de paginação esquecidas ou inconsistentes.

```csharp
❌ Sem validação consistente:

// Cenário 1: Sem validação
public async Task<IEnumerable<User>> GetUsersAsync(int page, int pageSize)
{
    var offset = (page - 1) * pageSize;  // page = 0 → offset = -pageSize! 💥
    return await _context.Users.Skip(offset).Take(pageSize).ToListAsync();
}

// Cenário 2: Validação parcial
public async Task<IEnumerable<Product>> GetProductsAsync(int page, int pageSize)
{
    if (page < 1) page = 1;  // Corrige silenciosamente
    // Esqueceu de validar pageSize! pageSize = -10 → Take(-10) 💥
    return await _context.Products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
}

// Cenário 3: Validação diferente
public async Task<IEnumerable<Order>> GetOrdersAsync(int page, int pageSize)
{
    if (page <= 0) throw new ArgumentException("Page must be positive");
    if (pageSize <= 0) pageSize = 10;  // Outro comportamento diferente!
    // ...
}

❌ Problemas:
- Comportamentos diferentes em cada método
- Alguns lançam exceção, outros corrigem silenciosamente
- Fácil esquecer validação em novos endpoints
- Bugs difíceis de rastrear
```

**✅ Validação centralizada com PaginationInfo:**

```csharp
✅ Validação consistente:

// PaginationInfo.Create() SEMPRE valida
public static PaginationInfo Create(int page, int pageSize)
{
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(page, 0, nameof(page));
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0, nameof(pageSize));

    return new PaginationInfo(page, pageSize, null, null);
}

// Uso em qualquer lugar - validação garantida!
var pagination = PaginationInfo.Create(request.Page, request.PageSize);
// Se page ou pageSize <= 0, lança exceção ANTES de chegar ao banco

// Comportamento CONSISTENTE em toda aplicação:
// - Sempre lança exceção para valores inválidos
// - Mensagem de erro padronizada
// - Fácil de tratar no middleware global
```

---

## 💚 Funcionalidades

### ✅ Paginação Básica

Cálculo automático de Index e Offset baseado em Page e PageSize.

```csharp
var pagination = PaginationInfo.Create(page: 3, pageSize: 10);

Console.WriteLine($"Page: {pagination.Page}");         // 3
Console.WriteLine($"PageSize: {pagination.PageSize}"); // 10
Console.WriteLine($"Index: {pagination.Index}");       // 2 (Page - 1)
Console.WriteLine($"Offset: {pagination.Offset}");     // 20 (Index * PageSize)

// Uso com LINQ
var users = await _context.Users
    .Skip(pagination.Offset)   // Skip(20)
    .Take(pagination.PageSize) // Take(10)
    .ToListAsync();
```

### ✅ Ordenação Múltipla

Suporte a múltiplos campos de ordenação com prioridade definida pela ordem.

```csharp
var sortCollection = new[]
{
    SortInfo.Create("LastName", SortDirection.Ascending),
    SortInfo.Create("FirstName", SortDirection.Ascending),
    SortInfo.Create("CreatedAt", SortDirection.Descending)
};

var pagination = PaginationInfo.Create(
    page: 1,
    pageSize: 20,
    sortCollection: sortCollection,
    filterCollection: null
);

// Resultado SQL equivalente:
// ORDER BY LastName ASC, FirstName ASC, CreatedAt DESC

// Verificar se tem ordenação
if (pagination.HasSort)
{
    foreach (var sort in pagination.SortCollection!)
    {
        Console.WriteLine($"Ordenar por: {sort.Field} {sort.Direction}");
    }
}
```

### ✅ Filtros Dinâmicos

Suporte a múltiplos filtros com operadores variados.

```csharp
var filterCollection = new[]
{
    FilterInfo.Create("Status", FilterOperator.Equals, "Active"),
    FilterInfo.Create("Name", FilterOperator.Contains, "Silva"),
    FilterInfo.CreateBetween("CreatedAt", "2024-01-01", "2024-12-31"),
    FilterInfo.CreateIn("Department", new[] { "IT", "HR", "Finance" })
};

var pagination = PaginationInfo.Create(
    page: 1,
    pageSize: 20,
    sortCollection: null,
    filterCollection: filterCollection
);

// Resultado SQL equivalente:
// WHERE Status = 'Active'
//   AND Name LIKE '%Silva%'
//   AND CreatedAt BETWEEN '2024-01-01' AND '2024-12-31'
//   AND Department IN ('IT', 'HR', 'Finance')

// Verificar se tem filtros
if (pagination.HasFilter)
{
    foreach (var filter in pagination.FilterCollection!)
    {
        Console.WriteLine($"Filtrar: {filter.Field} {filter.Operator} {filter.Value}");
    }
}
```

### ✅ Fluent API

Métodos encadeados para adicionar ordenação e filtros.

```csharp
var pagination = PaginationInfo.Create(1, 20)
    .WithSortCollection(new[]
    {
        SortInfo.Create("Name", SortDirection.Ascending)
    })
    .WithFilterCollection(new[]
    {
        FilterInfo.Create("Active", FilterOperator.Equals, "true")
    });
```

### ✅ Reconstituição (sem validação)

Para carregar dados já validados (do banco, cache, etc.).

```csharp
// Dados vindo do banco (já validados anteriormente)
var pagination = PaginationInfo.CreateFromExistingInfo(
    page: storedPage,
    pageSize: storedPageSize,
    sortCollection: storedSorts,
    filterCollection: storedFilters
);
// NÃO valida - assume que dados são válidos
```

### ✅ Todos os Registros (Unbounded)

Para recuperar todos os registros sem limite de paginação, mantendo suporte a ordenação e filtros.

```csharp
// Todos os registros, sem filtro/ordenação
var allItems = PaginationInfo.All;

// Todos os registros com ordenação (fluent API)
var allSorted = PaginationInfo.All
    .WithSortCollection(new[] { SortInfo.Create("Name", SortDirection.Ascending) });

// Todos os registros com filtro e ordenação (direto)
var allFiltered = PaginationInfo.CreateAll(
    sortCollection: new[] { SortInfo.Create("Name", SortDirection.Ascending) },
    filterCollection: new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") }
);

// Verificar se é unbounded no consumidor
if (pagination.IsUnbounded)
{
    // Lógica especial para queries sem limite
    // Ex: aplicar limite máximo, logar, etc.
}
```

> ⚠️ **ATENÇÃO:** Use `All` e `CreateAll()` com cuidado em coleções grandes para evitar problemas de memória. A camada Infra.Data pode impor limites adicionais se necessário.

---

## 🏗️ Arquitetura: Fluxo Seguro Front-to-Back

A arquitetura recomendada usa um fluxo em duas etapas: **OPTIONS** para descobrir campos permitidos, e **POST** para enviar a query tipada.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            FLUXO COMPLETO                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1️⃣ Front faz OPTIONS para descobrir campos permitidos                     │
│  ┌──────────┐         OPTIONS /api/users/query           ┌──────────┐      │
│  │  Front   │ ─────────────────────────────────────────► │   BFF    │      │
│  └──────────┘                                            └────┬─────┘      │
│       ▲                                                       │            │
│       │                                                       ▼            │
│       │                                              ┌─────────────────┐   │
│       │                                              │   Infra.Data    │   │
│       │                                              │  QueryContract  │   │
│       │                                              │   (whitelist)   │   │
│       │                                              └────────┬────────┘   │
│       │                                                       │            │
│       │         200 OK                                        │            │
│       │         {                                             │            │
│       │           "sortableFields": [                         │            │
│       │             { "name": "Name", "type": "string" },     │            │
│       │             { "name": "Email", "type": "string" },    │            │
│       │             { "name": "CreatedAt", "type": "date" }   │            │
│       │           ],                                          │            │
│       │           "filterableFields": [...],                  │            │
│       │           "filterOperators": [...]                    │            │
│       │         }                                             │            │
│       └───────────────────────────────────────────────────────┘            │
│                                                                             │
│  2️⃣ Front monta UI baseado no metadado (dropdowns tipados)                 │
│                                                                             │
│  3️⃣ Front envia objeto tipado (não string arbitrária)                      │
│  ┌──────────┐         POST /api/users/query              ┌──────────┐      │
│  │  Front   │ ─────────────────────────────────────────► │   BFF    │      │
│  └──────────┘         {                                  └────┬─────┘      │
│                         "page": 1,                            │            │
│                         "pageSize": 10,                       │            │
│                         "sortCollection": [                   │            │
│                           { "field": "Name",                  │            │
│                             "direction": "Ascending" }        │            │
│                         ],                                    │            │
│                         "filterCollection": [                 │            │
│                           { "field": "Status",                │            │
│                             "operator": "Equals",             ▼            │
│                             "value": "Active" }          ┌─────────────┐   │
│                         ]                                │ Infra.Data  │   │
│                       }                                  │  Valida     │   │
│                                                          │  Traduz     │   │
│                                                          │  Executa    │   │
│                                                          └─────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Benefícios desta Arquitetura

| Aspecto | Benefício |
|---------|-----------|
| **Segurança** | Front só envia valores que vieram do OPTIONS (whitelist) |
| **DRY** | Metadado vem do QueryContract (Single Source of Truth) |
| **UX** | Front monta dropdowns/filtros dinamicamente |
| **Validação** | Backend valida que valor recebido está na whitelist |
| **Type-safe** | Objeto serializado, não string concatenada |
| **Evoluível** | Adicionar campo = atualizar QueryContract, front se adapta |

---

## 🚀 Como Usar

### 1️⃣ Paginação Simples

```csharp
// Criar paginação apenas com page e pageSize
var pagination = PaginationInfo.Create(page: 1, pageSize: 20);

// Usar no repositório
var users = await _context.Users
    .Skip(pagination.Offset)
    .Take(pagination.PageSize)
    .ToListAsync();
```

### 2️⃣ Com Ordenação

```csharp
// Criar ordenação
var sortCollection = new[]
{
    SortInfo.Create("LastName", SortDirection.Ascending),
    SortInfo.Create("FirstName", SortDirection.Ascending)
};

// Criar paginação com ordenação
var pagination = PaginationInfo.Create(
    page: 1,
    pageSize: 20,
    sortCollection: sortCollection,
    filterCollection: null
);

// Usar no repositório
if (pagination.HasSort)
{
    // Aplicar ordenação (após validação no Infra.Data)
}
```

### 3️⃣ Com Filtros

```csharp
// Criar filtros
var filterCollection = new[]
{
    FilterInfo.Create("Status", FilterOperator.Equals, "Active"),
    FilterInfo.Create("Name", FilterOperator.Contains, "Silva")
};

// Criar paginação com filtros
var pagination = PaginationInfo.Create(
    page: 1,
    pageSize: 20,
    sortCollection: null,
    filterCollection: filterCollection
);

// Usar no repositório
if (pagination.HasFilter)
{
    // Aplicar filtros (após validação no Infra.Data)
}
```

### 4️⃣ Completo (Paginação + Ordenação + Filtros)

```csharp
var sortCollection = new[]
{
    SortInfo.Create("CreatedAt", SortDirection.Descending)
};

var filterCollection = new[]
{
    FilterInfo.Create("Active", FilterOperator.Equals, "true"),
    FilterInfo.CreateBetween("CreatedAt", "2024-01-01", "2024-12-31")
};

var pagination = PaginationInfo.Create(
    page: 1,
    pageSize: 50,
    sortCollection: sortCollection,
    filterCollection: filterCollection
);

// Resultado:
// - Página 1, 50 itens por página
// - Ordenado por CreatedAt DESC
// - Filtrado por Active = true E CreatedAt entre 2024-01-01 e 2024-12-31
```

### 5️⃣ Fluent API

```csharp
var pagination = PaginationInfo.Create(1, 20)
    .WithSortCollection(new[] { SortInfo.Create("Name", SortDirection.Ascending) })
    .WithFilterCollection(new[] { FilterInfo.Create("Active", FilterOperator.Equals, "true") });
```

### 6️⃣ Todos os Registros (Unbounded)

Quando você precisa recuperar todos os registros sem paginação, mas ainda com suporte a ordenação e filtros.

```csharp
// Forma mais simples - todos os registros
var allItems = PaginationInfo.All;

// Com ordenação via fluent API
var allSorted = PaginationInfo.All
    .WithSortCollection(new[] { SortInfo.Create("Name", SortDirection.Ascending) });

// Com filtro e ordenação diretamente
var allFiltered = PaginationInfo.CreateAll(
    sortCollection: new[] { SortInfo.Create("CreatedAt", SortDirection.Descending) },
    filterCollection: new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") }
);

// Verificar se é unbounded no consumidor (repositório/infra)
if (pagination.IsUnbounded)
{
    _logger.LogWarning("Query unbounded solicitada - considere aplicar limite máximo");
}
```

> ⚠️ **ATENÇÃO:** Queries unbounded podem retornar grandes volumes de dados. Use com cuidado e considere aplicar limites na camada Infra.Data.

---

## 📦 Estruturas Relacionadas

### SortInfo

```csharp
// Criar ordenação
var sort = SortInfo.Create("FieldName", SortDirection.Ascending);

// Propriedades
sort.Field      // "FieldName"
sort.Direction  // SortDirection.Ascending

// Reconstituição (sem validação)
var sort = SortInfo.CreateFromExistingInfo("FieldName", SortDirection.Descending);
```

### SortDirection

```csharp
public enum SortDirection
{
    Ascending,  // A-Z, 0-9, menor para maior
    Descending  // Z-A, 9-0, maior para menor
}
```

### FilterInfo

```csharp
// Filtro simples
var filter = FilterInfo.Create("Status", FilterOperator.Equals, "Active");

// Filtro de intervalo (Between)
var filter = FilterInfo.CreateBetween("CreatedAt", "2024-01-01", "2024-12-31");

// Filtro de lista (In)
var filter = FilterInfo.CreateIn("Department", new[] { "IT", "HR" });

// Filtro de exclusão (NotIn)
var filter = FilterInfo.CreateNotIn("Status", new[] { "Deleted", "Archived" });

// Propriedades
filter.Field     // Nome do campo
filter.Operator  // Operador (Equals, Contains, Between, etc.)
filter.Value     // Valor (para operadores simples)
filter.ValueEnd  // Valor final (para Between)
filter.Values    // Lista de valores (para In/NotIn)
```

### FilterOperator

```csharp
public enum FilterOperator
{
    // Igualdade
    Equals,              // =
    NotEquals,           // !=

    // Texto
    Contains,            // LIKE '%value%'
    StartsWith,          // LIKE 'value%'
    EndsWith,            // LIKE '%value'

    // Comparação
    GreaterThan,         // >
    GreaterThanOrEquals, // >=
    LessThan,            // <
    LessThanOrEquals,    // <=

    // Intervalo
    Between,             // BETWEEN value AND valueEnd

    // Lista
    In,                  // IN (values)
    NotIn                // NOT IN (values)
}
```

---

## ⚖️ Trade-offs

### ✅ Vantagens

| Aspecto | Benefício |
|---------|-----------|
| **Segurança** | Campos validados contra whitelist, sem SQL injection |
| **Imutabilidade** | readonly struct previne modificações acidentais |
| **Validação** | Page e PageSize sempre > 0, garantido pelo Create() |
| **Unbounded** | Suporte a queries sem limite via `All` e `CreateAll()` |
| **Cálculos** | Index e Offset calculados automaticamente |
| **Consistência** | Mesma estrutura em toda aplicação |
| **Testabilidade** | Fácil criar instâncias para testes |
| **Performance** | Stack allocation (struct), sem GC pressure |

### ⚠️ Limitações

| Aspecto | Limitação | Mitigação |
|---------|-----------|-----------|
| **Validação de campos** | PaginationInfo não valida se campo existe | Validar na camada Infra.Data com QueryContract |
| **Expressões complexas** | Não suporta OR entre filtros | Criar FilterGroup se necessário |
| **Joins** | Não sabe de relacionamentos | Tratado no repositório/query builder |
| **Unbounded** | `All`/`CreateAll()` pode retornar grandes volumes | Verificar `IsUnbounded` e aplicar limites na Infra.Data |

---

## 📚 Exemplos Avançados

### Implementação do QueryContract no Infra.Data

```csharp
// Infra.Data/QueryContracts/UserQueryContract.cs
public static class UserQueryContract
{
    // Enum define campos permitidos (Single Source of Truth)
    public enum SortField
    {
        Name,
        Email,
        CreatedAt,
        UpdatedAt
    }

    public enum FilterField
    {
        Name,
        Email,
        Status,
        CreatedAt
    }

    // Mapeamento para colunas reais (encapsulado)
    private static readonly Dictionary<SortField, string> SortColumnMap = new()
    {
        [SortField.Name] = "user_name",
        [SortField.Email] = "email_address",
        [SortField.CreatedAt] = "created_at",
        [SortField.UpdatedAt] = "updated_at"
    };

    private static readonly Dictionary<FilterField, string> FilterColumnMap = new()
    {
        [FilterField.Name] = "user_name",
        [FilterField.Email] = "email_address",
        [FilterField.Status] = "status_code",
        [FilterField.CreatedAt] = "created_at"
    };

    // Validação e conversão (string → enum)
    public static bool TryParseSortField(string? input, out SortField field)
    {
        field = default;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return Enum.TryParse(input, ignoreCase: true, out field);
    }

    public static bool TryParseFilterField(string? input, out FilterField field)
    {
        field = default;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return Enum.TryParse(input, ignoreCase: true, out field);
    }

    // Obter coluna real (após validação)
    public static string GetSortColumn(SortField field) => SortColumnMap[field];
    public static string GetFilterColumn(FilterField field) => FilterColumnMap[field];

    // Metadado para OPTIONS (exposto ao front)
    public static QueryMetadata GetMetadata() => new()
    {
        SortableFields = Enum.GetNames<SortField>(),
        FilterableFields = Enum.GetNames<FilterField>(),
        SupportedOperators = Enum.GetNames<FilterOperator>()
    };
}
```

### Uso no Repository

```csharp
// Infra.Data/Repositories/UserRepository.cs
public class UserRepository : IUserRepository
{
    public async Task<PagedResult<User>> QueryAsync(PaginationInfo pagination)
    {
        var query = _context.Users.AsQueryable();

        // Aplicar filtros (após validação)
        if (pagination.HasFilter)
        {
            foreach (var filter in pagination.FilterCollection!)
            {
                if (!UserQueryContract.TryParseFilterField(filter.Field, out var validField))
                    throw new InvalidFilterFieldException(filter.Field);

                var column = UserQueryContract.GetFilterColumn(validField);
                query = ApplyFilter(query, column, filter);
            }
        }

        // Aplicar ordenação (após validação)
        if (pagination.HasSort)
        {
            bool isFirst = true;
            foreach (var sort in pagination.SortCollection!)
            {
                if (!UserQueryContract.TryParseSortField(sort.Field, out var validField))
                    throw new InvalidSortFieldException(sort.Field);

                var column = UserQueryContract.GetSortColumn(validField);
                query = ApplySort(query, column, sort.Direction, isFirst);
                isFirst = false;
            }
        }

        // Contar total
        var totalCount = await query.CountAsync();

        // Aplicar paginação
        var items = await query
            .Skip(pagination.Offset)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
```

### Endpoint OPTIONS

```csharp
// API/Controllers/UsersController.cs
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpOptions("query")]
    public IActionResult GetQueryMetadata()
    {
        var metadata = UserQueryContract.GetMetadata();
        return Ok(metadata);
    }

    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] QueryRequest request)
    {
        var pagination = PaginationInfo.Create(
            request.Page,
            request.PageSize,
            request.SortCollection,
            request.FilterCollection
        );

        var result = await _userRepository.QueryAsync(pagination);
        return Ok(result);
    }
}
```

---

## 📖 Referências

- [OWASP - SQL Injection Prevention](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)
- [Microsoft - Pagination Best Practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design#pagination)
- [REST API Design - Filtering and Sorting](https://www.moesif.com/blog/technical/api-design/REST-API-Design-Filtering-Sorting-and-Pagination/)
