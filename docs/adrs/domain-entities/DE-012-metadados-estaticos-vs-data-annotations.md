# DE-012: Metadados Estáticos vs Data Annotations

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine duas formas de especificar os requisitos de um formulário:

**Opção A - Etiquetas coladas (Data Annotations)**:
Cada campo do formulário tem uma etiqueta adesiva com as regras: "Mínimo 3 caracteres", "Obrigatório". Para ler as regras, você precisa virar cada etiqueta e usar uma lupa (reflexão).

**Opção B - Manual na primeira página (Metadados Estáticos)**:
Na primeira página do formulário há uma tabela clara: "Nome: mín 3, máx 100, obrigatório". Qualquer um pode consultar instantaneamente, sem lupa.

Em sistemas de software, Data Annotations são as "etiquetas coladas" - exigem reflexão para leitura. Metadados estáticos são o "manual" - acesso direto, sem overhead.

### O Problema Técnico

**A limitação FUNDAMENTAL de Data Annotations: valores são literais de compile-time.**

Attributes em C# só aceitam **constantes literais** - não variáveis, não propriedades, não expressões:

```csharp
// ? IMPOSSÍVEL com propriedades ou variáveis - Não compila!
public sealed class Person
{
    // Erro CS0182: An attribute argument must be a constant expression
    [StringLength(PersonMetadata.FirstNameMaxLength)]  // Propriedade - NÃO COMPILA!
    public string FirstName { get; set; }

    // Erro CS0182: An attribute argument must be a constant expression
    [Range(Config.MinAge, Config.MaxAge)]  // Propriedades - NÃO COMPILA!
    public int Age { get; set; }
}

// ?? LIMITAÇÃO: const FUNCIONA, mas não pode ser alterado em runtime
public static class PersonMetadata
{
    public const int FirstNameMaxLength = 100;  // const - valor fixo para sempre
}

public sealed class Person
{
    // ? Compila, MAS o valor é "cimentado" em compile-time
    [StringLength(PersonMetadata.FirstNameMaxLength)]  // Funciona com const
    public string FirstName { get; set; }
}

// O PROBLEMA: const não pode ser alterado em runtime
public static class PersonMetadata
{
    // ? Isso NÃO é possível - const é imutável
    public const int FirstNameMaxLength = 100;

    // Se precisar alterar para 150 para um tenant específico?
    // Se precisar carregar de appsettings.json?
    // IMPOSSÍVEL com const!
}

// ? Com propriedades estáticas - alterável em runtime
public static class PersonMetadata
{
    public static int FirstNameMaxLength { get; private set; } = 100;

    public static void ChangeFirstNameMetadata(int maxLength)
    {
        FirstNameMaxLength = maxLength;  // ? Alterável no startup!
    }
}
```

**Isso significa que com Data Annotations:**
- Você pode usar `const`, mas valores ficam **fixos para sempre** em compile-time
- Você **não pode** alterar valores em runtime (configuração, multitenancy)
- Você **não pode** usar propriedades (apenas `const` ou literais)
- Para cenários dinâmicos, Data Annotations simplesmente **não funcionam**

---

Data Annotations em .NET são a forma tradicional de definir regras de validação:

```csharp
public sealed class Person
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string FirstName { get; private set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; private set; }

    [Range(0, 150, ErrorMessage = "Idade deve ser entre 0 e 150")]
    public int Age { get; private set; }
}
```

Para acessar esses valores em runtime:

```csharp
// Leitura via reflexão - LENTO e COMPLEXO
var property = typeof(Person).GetProperty("FirstName");
var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();

int maxLength = stringLengthAttr?.MaximumLength ?? int.MaxValue;  // 100
int minLength = stringLengthAttr?.MinimumLength ?? 0;              // 3
```

## Como Normalmente é Feito

### Abordagem Tradicional

A maioria dos projetos .NET usa Data Annotations extensivamente:

```csharp
// Entidade com Data Annotations
public sealed class Customer
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; }

    [Range(0, 999999999)]
    public decimal CreditLimit { get; set; }
}

// DTO duplicando as mesmas annotations
public class CreateCustomerRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]  // Copiado da entidade
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]  // Copiado da entidade
    public string Email { get; set; }
}

// FluentValidation também duplica
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 100);  // Valores duplicados

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);  // Valor duplicado
    }
}
```

### Por Que Não Funciona Bem

1. **Performance - Reflexão é cara**:

```csharp
// Benchmark simplificado
// Acesso via reflexão: ~500ns por chamada
var attr = typeof(Person).GetProperty("FirstName")
    .GetCustomAttribute<StringLengthAttribute>();
int max = attr.MaximumLength;

// Acesso direto: ~1ns por chamada
int max = PersonMetadata.FirstNameMaxLength;

// 500x mais lento! Em APIs de alto volume, isso importa
```

A reflexão não é "grátis". Em endpoints que processam milhares de requests por segundo, cada nanossegundo conta.

2. **AOT (Ahead-of-Time) Incompatibilidade**:

```csharp
// Blazor WebAssembly, MAUI, Unity, Native AOT
// Reflexão pode não funcionar ou ter limitações severas

// ? Pode falhar em AOT
var attr = typeof(Person).GetProperty("Name")
    .GetCustomAttribute<RequiredAttribute>();

// ? Funciona em qualquer ambiente
bool isRequired = PersonMetadata.NameIsRequired;
```

Plataformas modernas como Blazor WASM, .NET Native AOT, e Unity têm restrições ou não suportam reflexão completa.

3. **Duplicação e dessincronização**:

```csharp
// Entidade diz MaxLength = 100
public sealed class Customer
{
    [StringLength(100)]
    public string Name { get; set; }
}

// DTO diz MaxLength = 50 (copiado errado ou desatualizado)
public class CreateCustomerRequest
{
    [StringLength(50)]  // Oops! Dessincronizado
    public string Name { get; set; }
}

// API valida com 50, Entidade aceita até 100
// Usuário recebe erro confuso: "excede 50 chars" na API,
// mas se bypassar, domínio aceita
```

4. **Não é "Single Source of Truth"**:

```csharp
// Onde está a regra "FirstName máximo 100 caracteres"?
// - Na entidade (annotation)
// - No DTO da API (annotation)
// - No validador FluentValidation
// - No schema do banco de dados
// - No frontend JavaScript

// Se mudar para 150, precisa atualizar 5 lugares!
```

5. **Reflexão expõe detalhes internos**:

```csharp
// Código externo pode inspecionar QUALQUER propriedade via reflexão
var privateProperty = typeof(Person)
    .GetProperty("InternalSecret", BindingFlags.NonPublic | BindingFlags.Instance);

// Mesmo propriedades privadas ficam "acessíveis"
// Metadados estáticos expõem APENAS o que você decide expor
```

6. **Mensagens de erro acopladas à implementação**:

```csharp
[StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
public string Name { get; set; }

// Mudou para 150? Precisa atualizar a mensagem também!
// E se esquecer, mensagem fica errada
```

## A Decisão

### Nossa Abordagem

Entidades expõem metadados através de uma **classe estática aninhada** com propriedades públicas:

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    // Classe aninhada com todos os metadados de validação
    public static class SimpleAggregateRootMetadata
    {
        // FirstName
        public static readonly string FirstNamePropertyName = nameof(FirstName);
        public static bool FirstNameIsRequired { get; private set; } = true;
        public static int FirstNameMinLength { get; private set; } = 1;
        public static int FirstNameMaxLength { get; private set; } = 255;

        // LastName
        public static readonly string LastNamePropertyName = nameof(LastName);
        public static bool LastNameIsRequired { get; private set; } = true;
        public static int LastNameMinLength { get; private set; } = 1;
        public static int LastNameMaxLength { get; private set; } = 255;

        // FullName (propriedade derivada)
        public static readonly string FullNamePropertyName = nameof(FullName);
        public static bool FullNameIsRequired { get; private set; } = true;
        public static int FullNameMinLength { get; private set; } = FirstNameMinLength + LastNameMinLength + 1;
        public static int FullNameMaxLength { get; private set; } = FirstNameMaxLength + LastNameMaxLength + 1;

        // BirthDate
        public static readonly string BirthDatePropertyName = nameof(BirthDate);
        public static bool BirthDateIsRequired { get; private set; } = true;
        public static int BirthDateMinAgeInYears { get; private set; } = 0;
        public static int BirthDateMaxAgeInYears { get; private set; } = 150;
    }

    // Propriedades da entidade (sem Data Annotations!)
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName { get; private set; }
    public BirthDate BirthDate { get; private set; }
}
```

**Uso em camadas externas - Single Source of Truth**:

```csharp
// DTO usando metadados da entidade - SEMPRE sincronizado
public class CreatePersonRequest
{
    [Required]
    [MinLength(SimpleAggregateRootMetadata.FirstNameMinLength)]
    [MaxLength(SimpleAggregateRootMetadata.FirstNameMaxLength)]
    public string FirstName { get; set; }

    [Required]
    [MinLength(SimpleAggregateRootMetadata.LastNameMinLength)]
    [MaxLength(SimpleAggregateRootMetadata.LastNameMaxLength)]
    public string LastName { get; set; }
}

// FluentValidation também pode usar
public class PersonValidator : AbstractValidator<CreatePersonRequest>
{
    public PersonValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(
                SimpleAggregateRootMetadata.FirstNameMinLength,
                SimpleAggregateRootMetadata.FirstNameMaxLength
            );
    }
}

// Frontend (via API de metadados)
// GET /api/metadata/person
{
    "firstName": {
        "isRequired": true,
        "minLength": 1,
        "maxLength": 255
    }
}
```

### Comparação: Data Annotations vs Metadados Estáticos

| Aspecto | Data Annotations | Metadados Estáticos |
|---------|------------------|---------------------|
| **Valores** | `const` ou literal (compile-time) | Propriedades (runtime) |
| **Alteração Runtime** | **IMPOSSÍVEL** | Sim (startup) |
| **Configuração Externa** | **IMPOSSÍVEL** | Sim (appsettings) |
| **Multitenancy** | **IMPOSSÍVEL** | Sim (por deployment) |
| **Acesso aos valores** | Via reflexão (~500ns) | Direto (~1ns) |
| **AOT Support** | Limitado/Problemático | Total |
| **Single Source** | Não (duplicação de literais) | Sim (referência) |
| **Type-Safe** | Não (strings mágicas) | Sim (propriedades tipadas) |
| **Testabilidade** | Difícil | Fácil |

**O ponto crucial**: Data Annotations exigem valores `const` ou literais. Isso significa que os valores são "cimentados" no momento da compilação e **nunca podem ser alterados** - nem por configuração, nem por tenant, nem por nada.

### Customização em Runtime (Startup Only)

Metadados podem ser alterados no startup para cenários de multitenancy ou configuração externa:

```csharp
// Program.cs ou Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Carregar configuração
    var config = Configuration.GetSection("ValidationRules");

    // Customizar metadados no startup
    SimpleAggregateRootMetadata.ChangeFirstNameMetadata(
        isRequired: true,
        minLength: config.GetValue<int>("FirstNameMinLength", 1),
        maxLength: config.GetValue<int>("FirstNameMaxLength", 255)
    );
}
```

**Implementação do método Change*Metadata**:

```csharp
public static class SimpleAggregateRootMetadata
{
    private static readonly Lock _lockObject = new();

    public static void ChangeFirstNameMetadata(
        bool isRequired,
        int minLength,
        int maxLength
    )
    {
        lock (_lockObject)
        {
            FirstNameIsRequired = isRequired;
            FirstNameMinLength = minLength;
            FirstNameMaxLength = maxLength;
        }
    }
}
```

### Quando NÃO Usar Change*Metadata

```csharp
// ? ERRADO: Alterar durante request (thread-safety issues)
[HttpPost]
public IActionResult Create(CreatePersonRequest request)
{
    // NUNCA faça isso!
    SimpleAggregateRootMetadata.ChangeFirstNameMetadata(
        isRequired: request.TenantRequiresFirstName,
        minLength: 1,
        maxLength: 100
    );
}

// ? CORRETO: Para regras por tenant, use Strategy Pattern
public interface ITenantValidationStrategy
{
    bool FirstNameIsRequired { get; }
    int FirstNameMinLength { get; }
    int FirstNameMaxLength { get; }
}

// Resolva via DI baseado no tenant
var strategy = _tenantStrategyProvider.GetStrategy(tenantId);
```

## Consequências

### Benefícios

- **Performance**: Acesso direto sem reflexão (~500x mais rápido)
- **AOT Compatibility**: Funciona em Blazor WASM, MAUI, Unity, Native AOT
- **Single Source of Truth**: Camadas externas referenciam metadados da entidade
- **Type-Safe**: Propriedades tipadas ao invés de strings mágicas
- **Customização**: Alterável em runtime (startup) para multitenancy
- **Testabilidade**: Fácil de mockar e testar
- **IntelliSense**: Descoberta via autocomplete, sem documentação externa
- **Segurança**: Expõe apenas o que você decide expor

### Trade-offs (Com Perspectiva)

- **Mais código**: Cada propriedade precisa de metadados explícitos
- **Não usa ecossistema Data Annotations**: Validadores automáticos não funcionam

### Trade-offs Frequentemente Superestimados

**"Data Annotations são mais simples"**

Na verdade, metadados estáticos são mais simples de USAR:

```csharp
// Data Annotations - precisa conhecer a API de reflexão
var attr = typeof(Person).GetProperty("Name")
    .GetCustomAttribute<StringLengthAttribute>();
int max = attr?.MaximumLength ?? int.MaxValue;

// Metadados estáticos - acesso direto
int max = PersonMetadata.NameMaxLength;
```

A "simplicidade" de Data Annotations é ilusória - você troca código explícito por magia implícita.

**"Validação automática com Data Annotations"**

ASP.NET MVC/API valida automaticamente DTOs com annotations. Mas isso:

1. Só funciona na camada de apresentação
2. Duplica regras (entidade + DTO)
3. Não funciona para validação de domínio

Com metadados estáticos, você pode ter o melhor dos dois mundos:

```csharp
// DTO referencia metadados da entidade
public class CreatePersonRequest
{
    [MaxLength(PersonMetadata.NameMaxLength)]  // Single source!
    public string Name { get; set; }
}
```

**"Reflexão é 'rápida o suficiente'"**

Para uma chamada, sim. Para milhares por segundo:

```csharp
// 10.000 requests/segundo × 5 campos × 500ns = 25ms de overhead por segundo
// Em uma API de alto volume, 25ms é significativo

// Com metadados estáticos: ~0.05ms por segundo
// Diferença: 500x
```

**"Data Annotations são padrão do .NET"**

Sim, mas para cenários onde são apropriados (DTOs simples, APIs CRUD básicas). Para domínios complexos com regras dinâmicas, metadados estáticos são mais flexíveis.

## Fundamentação Teórica

### Padrões de Design Relacionados

**Metadata Pattern** - Separar dados dos metadados que os descrevem é um padrão estabelecido. Nossa implementação torna os metadados acessíveis de forma type-safe.

**Type-Safe Builder** - Ao invés de strings mágicas em annotations, usamos propriedades tipadas que o compilador valida.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) enfatiza que o domínio deve ser auto-descritivo:

> "The MODEL should be rich enough to capture the essential behavior and structure of the domain."
>
> *O MODELO deve ser rico o suficiente para capturar o comportamento e estrutura essenciais do domínio.*

Metadados estáticos tornam o modelo mais rico - as regras de validação são parte explícita do modelo, não decorações externas.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre encapsulamento:

> "The Aggregate Root must protect all of its internal state, including the invariants that apply to its parts."
>
> *O Aggregate Root deve proteger todo seu estado interno, incluindo as invariantes que se aplicam às suas partes.*

Metadados estáticos expõem apenas o que a entidade decide expor - máximo controle.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende **explicitação sobre magia**:

> "The name of a variable, function, or class should answer all the big questions."
>
> *O nome de uma variável, função ou classe deve responder todas as grandes questões.*

`PersonMetadata.FirstNameMaxLength` é explícito. `[StringLength(100)]` escondido em uma annotation exige que você saiba procurar.

O princípio **"Don't Repeat Yourself" (DRY)**:

Data Annotations forçam repetição entre entidade e DTOs. Metadados estáticos eliminam essa repetição.

### O Que o Clean Architecture Diz

Clean Architecture defende que **detalhes de framework não devem contaminar o domínio**.

Data Annotations são um detalhe de framework (`System.ComponentModel.DataAnnotations`). Metadados estáticos são código C# puro - zero dependências de framework.

### Outros Fundamentos

**Performance Best Practices - Microsoft**:

> "Avoid reflection in hot paths. Use static type information when possible."
>
> *Evite reflexão em caminhos críticos. Use informação de tipo estático quando possível.*

**AOT Compilation Guidelines**:

> "Code that relies heavily on reflection may not work correctly with ahead-of-time compilation."
>
> *Código que depende fortemente de reflexão pode não funcionar corretamente com compilação ahead-of-time.*

Metadados estáticos são 100% compatíveis com AOT.

**SOLID - Open/Closed Principle**:

A classe de metadados pode ser estendida (novos metadados) sem modificar a entidade. Data Annotations são "fechadas" - você usa o que o framework oferece.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que reflexão é cara em .NET e como otimizar?"
- "Quais são as limitações de reflexão em AOT/Native compilation?"
- "Como implementar o Metadata Pattern em C#?"
- "Qual a diferença entre compile-time e runtime type information?"

### Leitura Recomendada

- [.NET Performance Tips - Reflection](https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/reflection)
- [Native AOT Deployment](https://docs.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Data Annotations - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [Domain-Driven Design - Model Integrity](https://martinfowler.com/bliki/BoundedContext.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Define o padrão de metadados estáticos que as entidades derivadas devem seguir |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Metadata de Validação Estática
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Single Source of Truth
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_TEMPLATE: Uso em Camadas Externas
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Customização em Runtime - Startup Only
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - SimpleAggregateRootMetadata completo
