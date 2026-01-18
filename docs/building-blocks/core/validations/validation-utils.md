# 🛡️ ValidationUtils - Validações Padronizadas com ExecutionContext

A classe estática `ValidationUtils` fornece métodos utilitários para validações comuns (obrigatoriedade, tamanho mínimo, tamanho máximo) que registram erros automaticamente no `ExecutionContext`. Centraliza a lógica de validação e padroniza os códigos de erro.

> 💡 **Visão Geral:** Valide propriedades com métodos simples que **retornam bool** e **registram erros automaticamente** no ExecutionContext, usando códigos padronizados no formato `{PropertyName}.{ValidationType}`.

---

## 📋 Sumário

- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
  - [Validação Manual Repetitiva](#1--validação-manual-repetitiva)
  - [Códigos de Erro Inconsistentes](#2-️-códigos-de-erro-inconsistentes)
- [Funcionalidades](#-funcionalidades)
- [Como Usar](#-como-usar)
- [Trade-offs](#️-tradeoffs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)

---

## 🎯 Contexto: Por Que Existe

### O Problema Real

Em aplicações empresariais, validações de propriedades são extremamente comuns: campos obrigatórios, tamanhos mínimos e máximos. As abordagens tradicionais apresentam problemas de repetição e inconsistência:

**Exemplo de desafios comuns:**

```csharp
❌ Abordagem 1: Validação manual em cada método
public class CustomerService
{
    public Customer? CreateCustomer(ExecutionContext context, CreateCustomerRequest request)
    {
        // Validar nome obrigatório
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            context.AddErrorMessage("CUSTOMER_NAME_REQUIRED", "Nome é obrigatório");
            return null;
        }

        // Validar tamanho mínimo do nome
        if (request.Name.Length < 2)
        {
            context.AddErrorMessage("CUSTOMER_NAME_TOO_SHORT", "Nome deve ter pelo menos 2 caracteres");
            return null;
        }

        // Validar tamanho máximo do nome
        if (request.Name.Length > 100)
        {
            context.AddErrorMessage("CUSTOMER_NAME_TOO_LONG", "Nome deve ter no máximo 100 caracteres");
            return null;
        }

        // Validar email obrigatório
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            context.AddErrorMessage("CUSTOMER_EMAIL_REQUIRED", "Email é obrigatório");
            return null;
        }

        // ... mais 10 validações similares ...

        return new Customer(request.Name, request.Email);
    }
}

❌ Problemas:
- Código repetitivo para cada propriedade
- Códigos de erro inventados ad-hoc (inconsistentes)
- Lógica de validação duplicada em vários lugares
- Fácil esquecer de registrar erro no context
- Retorno antecipado impede validar todas as propriedades
```

```csharp
❌ Abordagem 2: Validação sem registrar no context
public class CustomerService
{
    public Customer? CreateCustomer(ExecutionContext context, CreateCustomerRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Nome é obrigatório");  // ⚠️ Não registra no context!

        if (request.Name?.Length < 2)
            errors.Add("Nome muito curto");  // ⚠️ Context não sabe dos erros!

        if (errors.Any())
        {
            // ⚠️ Precisa registrar manualmente cada erro
            foreach (var error in errors)
                context.AddErrorMessage("VALIDATION_ERROR", error);

            return null;
        }

        return new Customer(request.Name, request.Email);
    }
}

❌ Problemas:
- Duas estruturas paralelas (List + ExecutionContext)
- Códigos genéricos não identificam a propriedade
- Fácil esquecer de sincronizar com o context
- Lógica de validação ainda duplicada
```

### A Solução

O `ValidationUtils` fornece métodos que **validam** e **registram erros** em uma única operação.

```csharp
✅ Abordagem com ValidationUtils:
public class CustomerService
{
    public Customer? CreateCustomer(ExecutionContext context, CreateCustomerRequest request)
    {
        // Validar nome
        bool nameRequired = ValidationUtils.ValidateIsRequired(
            context,
            propertyName: "Customer.Name",
            isRequired: true,
            value: request.Name
        );

        bool nameMinLength = ValidationUtils.ValidateMinLength(
            context,
            propertyName: "Customer.Name",
            minLength: 2,
            value: request.Name?.Length
        );

        bool nameMaxLength = ValidationUtils.ValidateMaxLength(
            context,
            propertyName: "Customer.Name",
            maxLength: 100,
            value: request.Name?.Length
        );

        // Validar email
        bool emailRequired = ValidationUtils.ValidateIsRequired(
            context,
            propertyName: "Customer.Email",
            isRequired: true,
            value: request.Email
        );

        // Verificar se todas as validações passaram
        if (!nameRequired || !nameMinLength || !nameMaxLength || !emailRequired)
            return null;  // ✅ Erros já registrados no context!

        return new Customer(request.Name!, request.Email!);
    }
}

// Erros registrados automaticamente:
// - "Customer.Name.IsRequired" (se nome for null/empty)
// - "Customer.Name.MinLength" (se nome < 2 caracteres)
// - "Customer.Name.MaxLength" (se nome > 100 caracteres)
// - "Customer.Email.IsRequired" (se email for null/empty)

✅ Benefícios:
- Validação e registro de erro em uma operação
- Códigos padronizados: {PropertyName}.{ValidationType}
- Retorno bool permite combinar validações
- Todas as validações executam (não para no primeiro erro)
- Lógica centralizada e reutilizável
```

**Estrutura do ValidationUtils:**

```
┌──────────────────────────────────────────────────────────────────────────┐
│                     ESTRUTURA DO VALIDATIONUTILS                         │
└──────────────────────────────────────────────────────────────────────────┘
│                                                                           │
│   static class ValidationUtils                                           │
│   │                                                                       │
│   ├── ValidateIsRequired<TValue>()                                       │
│   │   └── Valida se valor não é null/default quando isRequired=true     │
│   │                                                                       │
│   ├── ValidateMinLength<TValue>()                                        │
│   │   └── Valida se valor >= minLength (ignora null)                    │
│   │                                                                       │
│   └── ValidateMaxLength<TValue>()                                        │
│       └── Valida se valor <= maxLength (ignora null)                    │
│                                                                           │
│   Padrão de código de erro: {propertyName}.{ValidationType}              │
│   Exemplo: "Customer.Name.IsRequired"                                     │
│                                                                           │
│   ValidationType (enum):                                                  │
│   ├── IsRequired = 1                                                      │
│   ├── MinLength = 2                                                       │
│   └── MaxLength = 3                                                       │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Problemas Resolvidos

### 1. 🔄 Validação Manual Repetitiva

**Problema:** Cada validação requer múltiplas linhas de código com if/else e registro manual de erro.

#### 📚 Analogia: O Checklist do Piloto

Imagine um piloto de avião fazendo verificações pré-voo:

**❌ Sem checklist padronizado:**

```
Piloto verifica combustível:
  - "Hmm, parece OK"
  - Anota em papel "combustível verificado"

Piloto verifica pneus:
  - "Acho que estão bons"
  - Esquece de anotar!

Piloto verifica instrumentos:
  - "Funcionando"
  - Anota em outro papel "instrumentos OK"

⚠️ PROBLEMA: Verificações inconsistentes, registros espalhados
```

**✅ Com checklist padronizado (ValidationUtils):**

```
Sistema de checklist:
┌────────────────────────────────────────────────────────┐
│ ☑️ Combustível.Nivel >= Minimo     → OK               │
│ ☑️ Pneus.Pressao >= Minima         → OK               │
│ ☑️ Instrumentos.Status = Ativo     → OK               │
│ ☐ Motor.Temperatura <= Maxima      → FALHA REGISTRADA │
└────────────────────────────────────────────────────────┘

✅ CORRETO: Verificação padronizada, registro automático
```

#### 💻 Impacto Real no Código

**❌ Código com validação manual:**

```csharp
public class ProductService
{
    public Product? CreateProduct(ExecutionContext context, CreateProductRequest request)
    {
        bool isValid = true;

        // Nome obrigatório
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            context.AddErrorMessage("PRODUCT_NAME_REQUIRED", "Nome é obrigatório");
            isValid = false;
        }
        else
        {
            // Tamanho mínimo
            if (request.Name.Length < 3)
            {
                context.AddErrorMessage("PRODUCT_NAME_MIN", "Nome deve ter pelo menos 3 caracteres");
                isValid = false;
            }

            // Tamanho máximo
            if (request.Name.Length > 200)
            {
                context.AddErrorMessage("PRODUCT_NAME_MAX", "Nome deve ter no máximo 200 caracteres");
                isValid = false;
            }
        }

        // Preço obrigatório
        if (request.Price <= 0)
        {
            context.AddErrorMessage("PRODUCT_PRICE_REQUIRED", "Preço deve ser maior que zero");
            isValid = false;
        }

        // SKU obrigatório
        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            context.AddErrorMessage("PRODUCT_SKU_REQUIRED", "SKU é obrigatório");
            isValid = false;
        }
        else if (request.Sku.Length < 5)
        {
            context.AddErrorMessage("PRODUCT_SKU_MIN", "SKU deve ter pelo menos 5 caracteres");
            isValid = false;
        }

        if (!isValid)
            return null;

        return new Product(request.Name!, request.Price, request.Sku!);
    }
}

❌ Problemas:
- ~40 linhas só para validação
- Códigos de erro inventados (PRODUCT_NAME_MIN vs PRODUCT_SKU_MIN)
- Estrutura if/else aninhada complexa
- Fácil introduzir bugs
```

**✅ Código com ValidationUtils:**

```csharp
public class ProductService
{
    public Product? CreateProduct(ExecutionContext context, CreateProductRequest request)
    {
        // Nome
        bool nameValid =
            ValidationUtils.ValidateIsRequired(context, "Product.Name", true, request.Name) &&
            ValidationUtils.ValidateMinLength(context, "Product.Name", 3, request.Name?.Length) &&
            ValidationUtils.ValidateMaxLength(context, "Product.Name", 200, request.Name?.Length);

        // Preço (usando IsRequired com valor numérico)
        bool priceValid =
            ValidationUtils.ValidateIsRequired(context, "Product.Price", true, request.Price) &&
            ValidationUtils.ValidateMinLength(context, "Product.Price", 0.01m, request.Price);

        // SKU
        bool skuValid =
            ValidationUtils.ValidateIsRequired(context, "Product.Sku", true, request.Sku) &&
            ValidationUtils.ValidateMinLength(context, "Product.Sku", 5, request.Sku?.Length);

        if (!nameValid || !priceValid || !skuValid)
            return null;

        return new Product(request.Name!, request.Price, request.Sku!);
    }
}

// Códigos de erro padronizados gerados:
// - "Product.Name.IsRequired"
// - "Product.Name.MinLength"
// - "Product.Name.MaxLength"
// - "Product.Price.IsRequired"
// - "Product.Price.MinLength"
// - "Product.Sku.IsRequired"
// - "Product.Sku.MinLength"

✅ Benefícios:
- ~15 linhas (62% menos código)
- Códigos de erro consistentes e previsíveis
- Lógica simples e linear
- Fácil adicionar/remover validações
```

---

### 2. 🏷️ Códigos de Erro Inconsistentes

**Problema:** Cada desenvolvedor inventa códigos de erro diferentes, dificultando tratamento no frontend e internacionalização.

#### 📚 Analogia: O Código de Barras

Imagine um supermercado sem padronização de códigos:

**❌ Sem padrão:**

```
Produto 1: "LEITE_FALTA" (criado por João)
Produto 2: "OUT_OF_STOCK_BREAD" (criado por Maria)
Produto 3: "erro_estoque_arroz" (criado por Pedro)

⚠️ PROBLEMA: Frontend não consegue tratar genericamente
```

**✅ Com padrão (ValidationUtils):**

```
Produto 1: "Produto.Estoque.MinLength"
Produto 2: "Produto.Estoque.MinLength"
Produto 3: "Produto.Estoque.MinLength"

✅ CORRETO: Padrão previsível, fácil de tratar
```

#### 💻 Impacto Real no Código

**❌ Código com códigos inconsistentes:**

```csharp
// Desenvolvedor 1
context.AddErrorMessage("CUSTOMER_NAME_REQUIRED", "Nome obrigatório");

// Desenvolvedor 2
context.AddErrorMessage("ERR_EMAIL_MISSING", "Email faltando");

// Desenvolvedor 3
context.AddErrorMessage("validation.phone.empty", "Telefone vazio");

// Frontend tentando tratar:
switch (errorCode)
{
    case "CUSTOMER_NAME_REQUIRED":
    case "ERR_EMAIL_MISSING":
    case "validation.phone.empty":
    case "NAME_IS_REQUIRED":  // Esqueceu esse!
        ShowValidationError();
        break;
}

❌ Problemas:
- Padrões diferentes por desenvolvedor
- Frontend precisa conhecer todos os códigos
- Internacionalização impossível
- Novos códigos quebram o frontend
```

**✅ Código com ValidationUtils:**

```csharp
// Desenvolvedor 1
ValidationUtils.ValidateIsRequired(context, "Customer.Name", true, name);
// Gera: "Customer.Name.IsRequired"

// Desenvolvedor 2
ValidationUtils.ValidateIsRequired(context, "Customer.Email", true, email);
// Gera: "Customer.Email.IsRequired"

// Desenvolvedor 3
ValidationUtils.ValidateIsRequired(context, "Customer.Phone", true, phone);
// Gera: "Customer.Phone.IsRequired"

// Frontend tratando genericamente:
if (errorCode.EndsWith(".IsRequired"))
{
    var propertyName = errorCode.Replace(".IsRequired", "");
    ShowRequiredFieldError(propertyName);
}
else if (errorCode.EndsWith(".MinLength"))
{
    var propertyName = errorCode.Replace(".MinLength", "");
    ShowMinLengthError(propertyName);
}

// Internacionalização:
var translations = new Dictionary<string, string>
{
    ["Customer.Name.IsRequired"] = "O nome do cliente é obrigatório",
    ["Customer.Email.IsRequired"] = "O email do cliente é obrigatório",
    // Ou padrão genérico:
    [".IsRequired"] = "O campo {0} é obrigatório"
};

✅ Benefícios:
- Padrão único: {Entity}.{Property}.{ValidationType}
- Frontend trata por sufixo (.IsRequired, .MinLength, .MaxLength)
- Internacionalização previsível
- Novos campos funcionam automaticamente
```

---

## ✨ Funcionalidades

### 📋 ValidateIsRequired

Valida se um valor é obrigatório (não null e não default).

```csharp
// Valida string obrigatória
bool isValid = ValidationUtils.ValidateIsRequired(
    executionContext,
    propertyName: "Customer.Name",
    isRequired: true,
    value: customerName  // null ou "" → inválido
);
// Se inválido, registra: "Customer.Name.IsRequired"

// Valida Guid obrigatório
bool guidValid = ValidationUtils.ValidateIsRequired(
    executionContext,
    propertyName: "Order.CustomerId",
    isRequired: true,
    value: customerId  // Guid.Empty → inválido
);

// Validação condicional (isRequired = false → sempre válido)
bool optionalValid = ValidationUtils.ValidateIsRequired(
    executionContext,
    propertyName: "Customer.MiddleName",
    isRequired: false,  // ✨ Não valida
    value: middleName
);
// Retorna true mesmo se middleName for null
```

**Comportamento:**
- Retorna `true` se `isRequired = false` (campo opcional)
- Retorna `true` se valor não é `null` e não é `default(T)`
- Retorna `false` e registra erro se valor é `null` ou `default(T)` quando `isRequired = true`

---

### 📏 ValidateMinLength

Valida se um valor é maior ou igual ao mínimo.

```csharp
// Valida tamanho mínimo de string
bool minLengthValid = ValidationUtils.ValidateMinLength(
    executionContext,
    propertyName: "Customer.Name",
    minLength: 2,
    value: customerName?.Length  // 1 → inválido
);
// Se inválido, registra: "Customer.Name.MinLength"

// Valida valor mínimo numérico
bool minPriceValid = ValidationUtils.ValidateMinLength(
    executionContext,
    propertyName: "Product.Price",
    minLength: 0.01m,
    value: price  // 0 → inválido
);

// Valor null é considerado válido (use IsRequired para obrigatoriedade)
bool nullValid = ValidationUtils.ValidateMinLength(
    executionContext,
    propertyName: "Customer.Name",
    minLength: 2,
    value: (int?)null  // ✨ Retorna true (null é válido)
);
```

**Comportamento:**
- Retorna `true` se valor é `null` (validação de obrigatoriedade é separada)
- Retorna `true` se `value >= minLength`
- Retorna `false` e registra erro se `value < minLength`

---

### 📐 ValidateMaxLength

Valida se um valor é menor ou igual ao máximo.

```csharp
// Valida tamanho máximo de string
bool maxLengthValid = ValidationUtils.ValidateMaxLength(
    executionContext,
    propertyName: "Customer.Name",
    maxLength: 100,
    value: customerName?.Length  // 150 → inválido
);
// Se inválido, registra: "Customer.Name.MaxLength"

// Valida valor máximo numérico
bool maxQuantityValid = ValidationUtils.ValidateMaxLength(
    executionContext,
    propertyName: "Order.Quantity",
    maxLength: 1000,
    value: quantity  // 1500 → inválido
);

// Valor null é considerado válido
bool nullValid = ValidationUtils.ValidateMaxLength(
    executionContext,
    propertyName: "Customer.Name",
    maxLength: 100,
    value: (int?)null  // ✨ Retorna true (null é válido)
);
```

**Comportamento:**
- Retorna `true` se valor é `null` (validação de obrigatoriedade é separada)
- Retorna `true` se `value <= maxLength`
- Retorna `false` e registra erro se `value > maxLength`

---

## 🚀 Como Usar

### 1️⃣ Uso Básico - Validação Simples

```csharp
using Bedrock.BuildingBlocks.Core.Validations;

public class UserService
{
    public User? CreateUser(ExecutionContext context, string username, string email)
    {
        // Validar username obrigatório
        bool usernameValid = ValidationUtils.ValidateIsRequired(
            context,
            propertyName: "User.Username",
            isRequired: true,
            value: username
        );

        // Validar email obrigatório
        bool emailValid = ValidationUtils.ValidateIsRequired(
            context,
            propertyName: "User.Email",
            isRequired: true,
            value: email
        );

        if (!usernameValid || !emailValid)
            return null;

        return new User(username, email);
    }
}
```

**Quando usar:** Validações simples de obrigatoriedade.

---

### 2️⃣ Uso com Validações Combinadas

```csharp
public class ProductService
{
    public Product? CreateProduct(ExecutionContext context, CreateProductRequest request)
    {
        // Encadear validações com && (short-circuit)
        bool nameValid =
            ValidationUtils.ValidateIsRequired(context, "Product.Name", true, request.Name) &&
            ValidationUtils.ValidateMinLength(context, "Product.Name", 3, request.Name?.Length) &&
            ValidationUtils.ValidateMaxLength(context, "Product.Name", 200, request.Name?.Length);

        bool descriptionValid =
            ValidationUtils.ValidateMaxLength(context, "Product.Description", 1000, request.Description?.Length);
            // Descrição é opcional, não precisa de IsRequired

        bool priceValid =
            ValidationUtils.ValidateIsRequired(context, "Product.Price", true, request.Price) &&
            ValidationUtils.ValidateMinLength(context, "Product.Price", 0.01m, request.Price);

        if (!nameValid || !descriptionValid || !priceValid)
            return null;

        return new Product(request.Name!, request.Description, request.Price);
    }
}
```

**Quando usar:** Propriedades que precisam de múltiplas validações.

---

### 3️⃣ Uso em Entidades de Domínio

```csharp
public class Customer : EntityBase<Customer>
{
    public string Name { get; private set; }
    public string Email { get; private set; }

    public static Customer? Create(ExecutionContext context, string name, string email)
    {
        // Validar usando o padrão do framework
        bool nameValid =
            ValidationUtils.ValidateIsRequired(context, "Customer.Name", true, name) &&
            ValidationUtils.ValidateMinLength(context, "Customer.Name", 2, name?.Length) &&
            ValidationUtils.ValidateMaxLength(context, "Customer.Name", 100, name?.Length);

        bool emailValid =
            ValidationUtils.ValidateIsRequired(context, "Customer.Email", true, email) &&
            ValidationUtils.ValidateMinLength(context, "Customer.Email", 5, email?.Length) &&
            ValidationUtils.ValidateMaxLength(context, "Customer.Email", 255, email?.Length);

        if (!nameValid || !emailValid)
            return null;

        return new Customer { Name = name!, Email = email! };
    }
}
```

**Quando usar:** Factory methods de entidades de domínio.

---

### 4️⃣ Uso com Metadados Configuráveis

```csharp
public abstract class EntityBase
{
    public static class EntityBaseMetadata
    {
        public static bool IdIsRequired { get; private set; } = true;
        public static bool CreatedByIsRequired { get; private set; } = true;
        public static int CreatedByMinLength { get; private set; } = 1;
        public static int CreatedByMaxLength { get; private set; } = 255;
    }

    public static bool ValidateEntityInfo(ExecutionContext context, EntityInfo entityInfo)
    {
        bool idValid = ValidationUtils.ValidateIsRequired(
            context,
            propertyName: "EntityBase.Id",
            isRequired: EntityBaseMetadata.IdIsRequired,  // ✨ Configurável
            value: entityInfo.Id
        );

        bool createdByValid =
            ValidationUtils.ValidateIsRequired(
                context,
                propertyName: "EntityBase.CreatedBy",
                isRequired: EntityBaseMetadata.CreatedByIsRequired,
                value: entityInfo.EntityChangeInfo.CreatedBy
            );

        // Validar min/max apenas se valor não é null
        if (entityInfo.EntityChangeInfo.CreatedBy is not null)
        {
            createdByValid = createdByValid &&
                ValidationUtils.ValidateMinLength(
                    context,
                    propertyName: "EntityBase.CreatedBy",
                    minLength: EntityBaseMetadata.CreatedByMinLength,
                    value: entityInfo.EntityChangeInfo.CreatedBy.Length
                ) &&
                ValidationUtils.ValidateMaxLength(
                    context,
                    propertyName: "EntityBase.CreatedBy",
                    maxLength: EntityBaseMetadata.CreatedByMaxLength,
                    value: entityInfo.EntityChangeInfo.CreatedBy.Length
                );
        }

        return idValid && createdByValid;
    }
}
```

**Quando usar:** Validações com regras configuráveis por ambiente ou cliente.

---

## ⚖️ Trade-offs

### Benefícios

| Benefício | Impacto | Análise |
|-----------|---------|---------|
| **Códigos padronizados** | ✅ Alto | `{PropertyName}.{ValidationType}` previsível |
| **Registro automático** | ✅ Alto | Erro adicionado ao ExecutionContext automaticamente |
| **Retorno bool** | ✅ Médio | Permite encadear com && e \|\| |
| **Genérico** | ✅ Médio | Funciona com qualquer tipo `IComparable<T>` |
| **Null-safe** | ✅ Médio | MinLength/MaxLength retornam true para null |

### Custos

| Custo | Impacto | Mitigação |
|-------|---------|-----------|
| **Apenas 3 tipos de validação** | ⚠️ Médio | Adicionar novos métodos conforme necessário |
| **Código de erro sem texto** | ⚠️ Baixo | Usar dicionário de tradução no frontend |

### Quando Usar vs Quando Evitar

#### ✅ Use quando:
1. Precisa validar obrigatoriedade de campos
2. Precisa validar tamanho mínimo/máximo
3. Quer códigos de erro padronizados
4. Já usa ExecutionContext para rastrear operações
5. Quer validar todas as propriedades (não parar no primeiro erro)

#### ❌ Evite quando:
1. Precisa de validações complexas (regex, formato específico)
2. Não usa ExecutionContext
3. Quer mensagens de erro personalizadas (usar AddErrorMessage diretamente)

---

## 🔬 Exemplos Avançados

### 🏭 Validação de Request Completo

```csharp
public class OrderService
{
    public Order? CreateOrder(ExecutionContext context, CreateOrderRequest request)
    {
        // Validar cabeçalho do pedido
        bool customerIdValid = ValidationUtils.ValidateIsRequired(
            context, "Order.CustomerId", true, request.CustomerId);

        bool shippingAddressValid = ValidationUtils.ValidateIsRequired(
            context, "Order.ShippingAddress", true, request.ShippingAddress);

        // Validar itens do pedido
        bool hasItems = ValidationUtils.ValidateIsRequired(
            context, "Order.Items", true, request.Items);

        bool itemsMinCount = ValidationUtils.ValidateMinLength(
            context, "Order.Items", 1, request.Items?.Count);

        bool itemsMaxCount = ValidationUtils.ValidateMaxLength(
            context, "Order.Items", 100, request.Items?.Count);

        // Validar cada item
        bool allItemsValid = true;
        if (request.Items != null)
        {
            for (int i = 0; i < request.Items.Count; i++)
            {
                var item = request.Items[i];
                var prefix = $"Order.Items[{i}]";

                bool itemProductValid = ValidationUtils.ValidateIsRequired(
                    context, $"{prefix}.ProductId", true, item.ProductId);

                bool itemQuantityValid =
                    ValidationUtils.ValidateIsRequired(context, $"{prefix}.Quantity", true, item.Quantity) &&
                    ValidationUtils.ValidateMinLength(context, $"{prefix}.Quantity", 1, item.Quantity) &&
                    ValidationUtils.ValidateMaxLength(context, $"{prefix}.Quantity", 1000, item.Quantity);

                allItemsValid = allItemsValid && itemProductValid && itemQuantityValid;
            }
        }

        // Verificar todas as validações
        if (!customerIdValid || !shippingAddressValid || !hasItems ||
            !itemsMinCount || !itemsMaxCount || !allItemsValid)
        {
            return null;
        }

        return Order.Create(context, request);
    }
}

// Códigos de erro gerados para um pedido inválido:
// - "Order.CustomerId.IsRequired"
// - "Order.Items[0].Quantity.MinLength"
// - "Order.Items[2].ProductId.IsRequired"
```

**Pontos importantes:**
- Propriedades indexadas usam `[i]` no nome
- Todas as validações executam (coleta todos os erros)
- Frontend pode identificar exatamente qual item tem problema

---

### 🧪 Testando Validações

```csharp
public class ProductValidationTests
{
    [Fact]
    public void CreateProduct_WithEmptyName_ShouldAddIsRequiredError()
    {
        // Arrange
        var context = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test"),
            executionUser: "test@test.com",
            executionOrigin: "Test",
            minimumMessageType: MessageType.Information,
            timeProvider: TimeProvider.System
        );

        // Act
        var result = ValidationUtils.ValidateIsRequired(
            context,
            propertyName: "Product.Name",
            isRequired: true,
            value: (string?)null
        );

        // Assert
        Assert.False(result);
        Assert.True(context.HasErrorMessages);
        Assert.Contains(context.Messages, m => m.Code == "Product.Name.IsRequired");
    }

    [Theory]
    [InlineData("AB", 3, false)]   // 2 < 3 → inválido
    [InlineData("ABC", 3, true)]   // 3 >= 3 → válido
    [InlineData("ABCD", 3, true)]  // 4 >= 3 → válido
    public void ValidateMinLength_ShouldValidateCorrectly(
        string value, int minLength, bool expectedValid)
    {
        // Arrange
        var context = CreateTestContext();

        // Act
        var result = ValidationUtils.ValidateMinLength(
            context,
            propertyName: "Test.Value",
            minLength: minLength,
            value: value?.Length
        );

        // Assert
        Assert.Equal(expectedValid, result);

        if (!expectedValid)
            Assert.Contains(context.Messages, m => m.Code == "Test.Value.MinLength");
    }
}
```

**Pontos importantes:**
- Testar retorno bool E mensagens no context
- Usar Theory para testar boundary conditions
- Verificar código de erro exato

---

## 📚 Referências

- [ExecutionContext](../execution-contexts/execution-context.md) - Contexto de execução que recebe os erros
- [Validation Pattern](https://martinfowler.com/articles/replaceThrowWithNotification.html) - Martin Fowler: Replace Throwing Exceptions with Notification
- [Guard Clauses](https://deviq.com/design-patterns/guard-clause) - Padrão de validação de entrada
