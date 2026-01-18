# DE-015: Customização de Metadados Apenas no Startup

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fábrica de móveis com **manuais de montagem**:

**Opção A - Trocar manual durante a montagem**:
Enquanto um funcionário está montando uma mesa, outro muda o manual de instruções. Caos total - alguns móveis com pernas de 50cm, outros de 60cm.

**Opção B - Definir manual no início do turno**:
O gerente define qual manual usar antes de começar a produção. Durante o turno, todos seguem o mesmo manual. Quer mudar? Espere o próximo turno.

Em sistemas de software, alterar metadados durante processamento de requests é como trocar o manual durante a montagem. Alterar apenas no startup garante consistência.

---

### O Problema Técnico

Metadados são variáveis estáticas compartilhadas entre todas as threads. Alterá-los durante o processamento de requests causa race conditions:

```csharp
// Thread 1: Processando request do Tenant A
var firstName = "Jo"; // 2 caracteres
// Valida: FirstNameMinLength = 1 ? VÁLIDO ?

// Thread 2: Altera metadados para Tenant B (? ERRADO!)
SimpleAggregateRootMetadata.ChangeFirstNameMetadata(
    isRequired: true,
    minLength: 3,  // Agora exige 3!
    maxLength: 100
);

// Thread 1: Continua processando (usando metadados ALTERADOS!)
// Cria entidade com firstName = "Jo"
// Mas agora MinLength = 3... inconsistência!
```

Este é um bug sutil e difícil de reproduzir - só acontece em condições de race.

## A Decisão

### Nossa Abordagem

Métodos `Change*Metadata()` devem ser chamados **APENAS no startup**, nunca durante processamento de requests:

```csharp
// Program.cs ou Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // ? Startup - antes de qualquer request
    var config = _configuration.GetSection("ValidationRules");

    int minLength = config.GetValue<int>("FirstNameMinLength");
    int maxLength = config.GetValue<int>("FirstNameMaxLength");

    // Validação de sanidade
    if (minLength > maxLength)
        throw new InvalidOperationException(
            $"FirstNameMinLength ({minLength}) cannot exceed MaxLength ({maxLength})"
        );

    // Altera metadados ANTES de processar requests
    SimpleAggregateRootMetadata.ChangeFirstNameMetadata(
        isRequired: true,
        minLength: minLength,
        maxLength: maxLength
    );

    // Agora sim, configura o resto dos serviços
    services.AddControllers();
}
```

### Cenários Válidos para Change*Metadata()

1. **Deployment específico** (on-premises vs cloud):

```csharp
// appsettings.OnPremises.json
{
    "ValidationRules": {
        "FirstNameMaxLength": 50  // Limitação do sistema legado
    }
}

// appsettings.Cloud.json
{
    "ValidationRules": {
        "FirstNameMaxLength": 255  // Sem limitação
    }
}

// Program.cs
var environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"appsettings.{environment}.json");

var rules = builder.Configuration.GetSection("ValidationRules");
SimpleAggregateRootMetadata.ChangeFirstNameMetadata(
    isRequired: true,
    minLength: 1,
    maxLength: rules.GetValue<int>("FirstNameMaxLength")
);
```

2. **Compliance regional** (GDPR, LGPD):

```csharp
// Startup.cs - Deployment na Europa
if (config.GetValue<string>("Region") == "EU")
{
    // GDPR exige menos dados pessoais
    PersonMetadata.ChangeBirthDateMetadata(
        isRequired: false,  // Opcional na Europa
        minAgeInYears: 0,
        maxAgeInYears: 150
    );
}
```

3. **Planos comerciais** (básico, premium, enterprise):

```csharp
// Deployment separado para cada plano
var plan = config.GetValue<string>("CommercialPlan");

if (plan == "Enterprise")
{
    ProductMetadata.ChangeDescriptionMetadata(
        isRequired: true,
        minLength: 1,
        maxLength: 10000  // Descrições mais longas
    );
}
else // Basic, Premium
{
    ProductMetadata.ChangeDescriptionMetadata(
        isRequired: true,
        minLength: 1,
        maxLength: 1000  // Limite padrão
    );
}
```

### Antipadrão: Alterar Por Tenant em Runtime

```csharp
// ? ERRADO: Alterando metadados por request
[HttpPost]
public async Task<IActionResult> CreatePerson(
    [FromHeader] string tenantId,
    CreatePersonRequest request
)
{
    // ? Race condition! Outro request pode estar usando os metadados
    var tenantConfig = await _tenantService.GetConfigAsync(tenantId);
    SimpleAggregateRootMetadata.ChangeFirstNameMetadata(
        isRequired: tenantConfig.FirstNameIsRequired,
        minLength: tenantConfig.FirstNameMinLength,
        maxLength: tenantConfig.FirstNameMaxLength
    );

    // ... resto do código
}
```

### Solução para Regras por Tenant: Strategy Pattern

Para regras diferentes por tenant **em runtime**, use Strategy Pattern (ver [DE-033](./DE-033-strategy-pattern-para-regras-por-tenant-em-runtime.md)):

```csharp
// Interface de estratégia
public interface ITenantValidationStrategy
{
    int FirstNameMinLength { get; }
    int FirstNameMaxLength { get; }
    bool FirstNameIsRequired { get; }

    bool ValidateFirstName(ExecutionContext context, string? firstName);
}

// Implementação por tenant
public class EnterpriseTenantStrategy : ITenantValidationStrategy
{
    public int FirstNameMinLength => 1;
    public int FirstNameMaxLength => 500;  // Mais flexível
    public bool FirstNameIsRequired => true;

    public bool ValidateFirstName(ExecutionContext context, string? firstName)
    {
        // Usa os valores DESTA estratégia, não os metadados globais
        return ValidationUtils.ValidateMinLength(context, "FirstName", FirstNameMinLength, firstName?.Length ?? 0)
            & ValidationUtils.ValidateMaxLength(context, "FirstName", FirstNameMaxLength, firstName?.Length ?? 0);
    }
}

// Provider resolve por tenant
public class TenantValidationStrategyProvider
{
    private readonly ITenantService _tenantService;
    private readonly IServiceProvider _serviceProvider;

    public ITenantValidationStrategy GetStrategy(string tenantId)
    {
        var tenantType = _tenantService.GetTenantType(tenantId);

        return tenantType switch
        {
            "Enterprise" => _serviceProvider.GetService<EnterpriseTenantStrategy>(),
            "Premium" => _serviceProvider.GetService<PremiumTenantStrategy>(),
            _ => _serviceProvider.GetService<BasicTenantStrategy>()
        };
    }
}

// Controller usa estratégia, não metadados globais
[HttpPost]
public async Task<IActionResult> CreatePerson(
    [FromHeader] string tenantId,
    CreatePersonRequest request
)
{
    var strategy = _strategyProvider.GetStrategy(tenantId);

    // ? Thread-safe: cada request usa sua própria estratégia
    if (!strategy.ValidateFirstName(_context, request.FirstName))
        return BadRequest(_context.Messages);

    // ...
}
```

### Implementação dos Métodos Change*Metadata()

Os métodos usam `lock` para garantir atomicidade das alterações:

```csharp
public static class SimpleAggregateRootMetadata
{
    private static readonly Lock _lockObject = new();

    public static bool FirstNameIsRequired { get; private set; } = true;
    public static int FirstNameMinLength { get; private set; } = 1;
    public static int FirstNameMaxLength { get; private set; } = 255;

    public static void ChangeFirstNameMetadata(
        bool isRequired,
        int minLength,
        int maxLength
    )
    {
        lock (_lockObject)
        {
            // Todas as alterações são atômicas
            FirstNameIsRequired = isRequired;
            FirstNameMinLength = minLength;
            FirstNameMaxLength = maxLength;
        }
    }
}
```

O `lock` garante que todas as propriedades são alteradas juntas - não há estado intermediário inconsistente.

### Validação de Sanidade no Startup

Sempre valide os valores antes de aplicar:

```csharp
public static void ChangeFirstNameMetadata(
    bool isRequired,
    int minLength,
    int maxLength
)
{
    // Validações de sanidade
    if (minLength < 0)
        throw new ArgumentException("MinLength cannot be negative", nameof(minLength));

    if (maxLength < minLength)
        throw new ArgumentException(
            $"MaxLength ({maxLength}) cannot be less than MinLength ({minLength})"
        );

    lock (_lockObject)
    {
        FirstNameIsRequired = isRequired;
        FirstNameMinLength = minLength;
        FirstNameMaxLength = maxLength;
    }
}
```

### Benefícios

- **Thread-safety garantido**: Alterações apenas no startup, single-threaded
- **Consistência**: Todos os requests usam os mesmos metadados
- **Previsibilidade**: Comportamento determinístico durante toda a execução
- **Debugabilidade**: Valores definidos uma vez, fácil de rastrear
- **Fail-fast**: Erros de configuração detectados no startup, não em runtime

### Trade-offs (Com Perspectiva)

- **Requer redeploy para alterar**: Mudanças em metadados exigem restart
  - **Mitigação**: Use Strategy Pattern para regras por tenant em runtime

### Trade-offs Frequentemente Superestimados

**"Preciso alterar metadados em runtime sem redeploy"**

Na prática, alterações em regras de validação de domínio são **raras** e geralmente requerem:
- Testes em ambiente de staging
- Validação de dados existentes
- Comunicação com stakeholders

Um redeploy controlado é mais seguro que hot-swap de regras de validação.

**"O lock causa contenção"**

O `lock` só é usado no **startup** (uma vez). Durante processamento de requests, os metadados são apenas lidos - sem contenção.

## Fundamentação Teórica

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre configuração:

> "Configuration data is volatile data that the system needs at runtime. It should be loaded at startup time and should not change during the lifetime of the application."
>
> *Dados de configuração são dados voláteis que o sistema precisa em runtime. Devem ser carregados no startup e não devem mudar durante o tempo de vida da aplicação.*

Metadados de validação são configuração - devem ser estáveis após o startup.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre separação de responsabilidades:

> "Partition a complex program into layers. [...] Concentrate all the code related to the domain model in one layer and isolate it from the user interface, application, and infrastructure code."
>
> *Particione um programa complexo em camadas. [...] Concentre todo o código relacionado ao modelo de domínio em uma camada e isole-o da interface de usuário, aplicação e código de infraestrutura.*

Metadados pertencem ao domínio. Alterá-los por request (camada de aplicação) viola esta separação.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre invariantes:

> "The Aggregate Root is responsible for maintaining the invariants of the entire Aggregate."
>
> *O Aggregate Root é responsável por manter as invariantes de todo o Aggregate.*

Metadados definem as invariantes. Se mudam durante a execução, as invariantes se tornam imprevisíveis.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre previsibilidade:

> "Functions should have no side effects. [...] Side effects are lies. Your function promises to do one thing, but it also does other hidden things."
>
> *Funções não devem ter efeitos colaterais. [...] Efeitos colaterais são mentiras. Sua função promete fazer uma coisa, mas também faz outras coisas escondidas.*

Alterar metadados globais durante processamento de request é um efeito colateral perigoso - afeta todas as operações subsequentes de forma imprevisível.

### Thread-Safety em Variáveis Estáticas

Variáveis estáticas em C# são compartilhadas entre todas as threads. Sem sincronização adequada:

1. **Visibility**: Uma thread pode não ver alterações feitas por outra (CPU caches)
2. **Atomicity**: Alterações em múltiplas propriedades não são atômicas
3. **Ordering**: O compilador/CPU pode reordenar instruções

O `lock` resolve todos esses problemas para o cenário de startup.

### Imutabilidade Efetiva (Effective Immutability)

> "Effectively immutable objects are objects that could be mutated but are never actually mutated after construction."
>
> *Objetos efetivamente imutáveis são objetos que poderiam ser mutados mas nunca são realmente mutados após construção.*
> — Java Concurrency in Practice, Brian Goetz

Após o startup, os metadados são **efetivamente imutáveis** - ninguém os altera. Isso é mais seguro e performático que:

1. **Volatile fields**: Overhead em cada leitura
2. **Interlocked operations**: Não funcionam para múltiplas propriedades
3. **ReaderWriterLock**: Overhead desnecessário se não há escritas

### Configuração vs Estado de Domínio

Metadados são **configuração**, não estado de domínio:

- **Configuração**: Definida no deploy, estável durante execução
- **Estado de domínio**: Muda a cada operação de negócio

Tratar metadados como estado de domínio (alterando por request) mistura responsabilidades e complica o sistema.

## Antipadrões Documentados

### Antipadrão 1: Alterar Metadados por Request

```csharp
// ? Race condition
app.Use(async (context, next) =>
{
    var tenantId = context.Request.Headers["X-Tenant-Id"];
    var config = await _tenantService.GetConfigAsync(tenantId);

    // Altera metadados GLOBAIS por request - ERRADO!
    PersonMetadata.ChangeFirstNameMetadata(...);

    await next();
});
```

### Antipadrão 2: Metadados em Scoped Service

```csharp
// ? Falsa sensação de segurança
public class ScopedMetadata
{
    public int FirstNameMaxLength { get; set; }
}

services.AddScoped<ScopedMetadata>(); // Parece safe, mas...

// Os métodos Validate* ainda usam os metadados ESTÁTICOS!
public static bool ValidateFirstName(...)
{
    // Usa SimpleAggregateRootMetadata.FirstNameMaxLength, não o scoped
}
```

### Antipadrão 3: Cache de Metadados por Tenant

```csharp
// ? Complexidade desnecessária
public class TenantMetadataCache
{
    private ConcurrentDictionary<string, TenantMetadata> _cache;

    // Agora TODA validação precisa passar por aqui
    // E os métodos Validate* da entidade não funcionam mais
}
```

Use Strategy Pattern ao invés de tentar fazer cache de metadados.

## Decisões Relacionadas

- [DE-012](./DE-012-metadados-estaticos-vs-data-annotations.md) - Por que usar propriedades estáticas
- [DE-014](./DE-014-inicializacao-inline-de-metadados.md) - Inicialização inline
- [DE-033](./DE-033-strategy-pattern-para-regras-por-tenant-em-runtime.md) - Strategy Pattern para multitenancy

## Leitura Recomendada

- [Thread-Safe Singleton Pattern](https://csharpindepth.com/articles/singleton)
- [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Strategy Pattern - Refactoring Guru](https://refactoring.guru/design-patterns/strategy)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece métodos Change*Metadata() que permitem customização controlada apenas durante startup |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Customização em Runtime - Startup Only
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Métodos Change*Metadata() - Startup Only
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Implementação dos métodos Change*Metadata
