# DE-016: Single Source of Truth para Regras de Validação

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma empresa com **três departamentos que mantêm listas de preços separadas**:

**Cenário problemático**:
- Marketing: Produto X = R$ 100
- Vendas: Produto X = R$ 95
- Financeiro: Produto X = R$ 110

Cliente liga para Marketing, ouve R$ 100. Vai até a loja, vendedor diz R$ 95. Chega a fatura: R$ 110. Caos, desconfiança, retrabalho.

**Cenário correto**:
Existe **uma única tabela de preços** no sistema. Marketing, Vendas e Financeiro consultam a mesma fonte. Preço mudou? Atualiza em um lugar só.

Em sistemas de software, ter regras de validação duplicadas em API, Domain e UI é como ter três tabelas de preços. Os valores vão dessincronizar.

---

### O Problema Técnico

Em arquiteturas em camadas, é comum duplicar regras de validação:

```csharp
// ❌ ANTIPATTERN: Cada camada define suas próprias regras

// Camada API (Controller/DTO)
public class CreatePersonRequest
{
    [Required]
    [MinLength(3)]   // Hardcoded
    [MaxLength(100)] // Hardcoded
    public string FirstName { get; set; }
}

// Camada Application Service
public class PersonService
{
    public Person CreatePerson(CreatePersonRequest request)
    {
        if (request.FirstName.Length < 2)  // Diferente da API!
            throw new ValidationException("Nome muito curto");

        if (request.FirstName.Length > 50) // Diferente da API!
            throw new ValidationException("Nome muito longo");
    }
}

// Camada Domain
public sealed class Person
{
    public static Person? RegisterNew(ExecutionContext context, RegisterNewInput input)
    {
        if (input.FirstName.Length < 1)    // Diferente de AMBOS!
            context.AddError("Nome obrigatório");

        if (input.FirstName.Length > 255)  // Diferente de AMBOS!
            context.AddError("Nome muito longo");
    }
}
```

**Problemas**:
1. **Validação inconsistente**: API aceita 100 chars, Domain aceita 255
2. **Bugs silenciosos**: API rejeita nomes válidos para o Domain
3. **Manutenção pesadelo**: Mudou regra? Precisa atualizar N lugares
4. **Experiência ruim**: Usuário passa pela API, falha no Domain

## A Decisão

### Nossa Abordagem

**O Domain é a Single Source of Truth**. Todas as outras camadas **leem** as regras do Domain:

```csharp
// ✅ PATTERN: Domain define, outras camadas leem

// 1. Domain define os metadados (ÚNICA FONTE)
public static class SimpleAggregateRootMetadata
{
    public static readonly string FirstNamePropertyName = nameof(FirstName);
    public static bool FirstNameIsRequired { get; private set; } = true;
    public static int FirstNameMinLength { get; private set; } = 1;
    public static int FirstNameMaxLength { get; private set; } = 255;
}

// 2. API lê do Domain
public class CreatePersonRequest
{
    [Required]
    [MinLength(SimpleAggregateRootMetadata.FirstNameMinLength)]  // Da entidade!
    [MaxLength(SimpleAggregateRootMetadata.FirstNameMaxLength)]  // Da entidade!
    public string FirstName { get; set; }
}

// 3. Frontend (Blazor) lê do Domain
<input maxlength="@SimpleAggregateRootMetadata.FirstNameMaxLength" />
<span>Máximo @SimpleAggregateRootMetadata.FirstNameMaxLength caracteres</span>

// 4. Mensagens de erro leem do Domain
var message = $"Nome deve ter entre {SimpleAggregateRootMetadata.FirstNameMinLength} " +
              $"e {SimpleAggregateRootMetadata.FirstNameMaxLength} caracteres";
```

### Fluxo de Validação com Single Source of Truth

```
+-------------------------------------------------------------------------+
│                         FRONTEND (Blazor/React)                        │
│  +-----------------------------------------------------------------+   │
│  │ <input maxlength={SimpleAggregateRootMetadata.FirstNameMaxLength} │   │
│  │        minlength={SimpleAggregateRootMetadata.FirstNameMinLength} │   │
│  +-----------------------------------------------------------------+   │
│                                  │                                     │
│                                  ▼                                     │
+-------------------------------------------------------------------------+
                                   │
                                   ▼
+-------------------------------------------------------------------------+
│                              API (DTOs)                                │
│  +-----------------------------------------------------------------+   │
│  │ [MaxLength(SimpleAggregateRootMetadata.FirstNameMaxLength)]     │   │
│  │ [MinLength(SimpleAggregateRootMetadata.FirstNameMinLength)]     │   │
│  +-----------------------------------------------------------------+   │
│                                  │                                     │
│                                  ▼                                     │
+-------------------------------------------------------------------------+
                                   │
                                   ▼
+-------------------------------------------------------------------------+
│                    DOMAIN (Single Source of Truth)                     │
│  +-----------------------------------------------------------------+   │
│  │ SimpleAggregateRootMetadata.FirstNameMinLength = 1              │   │
│  │ SimpleAggregateRootMetadata.FirstNameMaxLength = 255            │   │
│  │ SimpleAggregateRootMetadata.FirstNameIsRequired = true          │   │
│  +-----------------------------------------------------------------+   │
│                                  │                                     │
│                                  ▼                                     │
│  +-----------------------------------------------------------------+   │
│  │ ValidateFirstName() usa os metadados para validar               │   │
│  +-----------------------------------------------------------------+   │
+-------------------------------------------------------------------------+
```

### Exemplos de Uso em Cada Camada

**1. API/Controller com Data Annotations**:

```csharp
public class CreatePersonRequest
{
    [Required(ErrorMessage = "FirstName é obrigatório")]
    [MinLength(SimpleAggregateRootMetadata.FirstNameMinLength,
        ErrorMessage = "FirstName muito curto")]
    [MaxLength(SimpleAggregateRootMetadata.FirstNameMaxLength,
        ErrorMessage = "FirstName muito longo")]
    public string FirstName { get; set; }

    [Required]
    [MinLength(SimpleAggregateRootMetadata.LastNameMinLength)]
    [MaxLength(SimpleAggregateRootMetadata.LastNameMaxLength)]
    public string LastName { get; set; }
}
```

**2. API/Controller com FluentValidation**:

```csharp
public class CreatePersonRequestValidator : AbstractValidator<CreatePersonRequest>
{
    public CreatePersonRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
                .When(_ => SimpleAggregateRootMetadata.FirstNameIsRequired)
            .MinimumLength(SimpleAggregateRootMetadata.FirstNameMinLength)
            .MaximumLength(SimpleAggregateRootMetadata.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
                .When(_ => SimpleAggregateRootMetadata.LastNameIsRequired)
            .MinimumLength(SimpleAggregateRootMetadata.LastNameMinLength)
            .MaximumLength(SimpleAggregateRootMetadata.LastNameMaxLength);
    }
}
```

**3. Frontend Blazor**:

```razor
@* Componente de input com validação automática *@
<div class="form-group">
    <label for="firstName">Nome</label>
    <input id="firstName"
           @bind="Model.FirstName"
           maxlength="@SimpleAggregateRootMetadata.FirstNameMaxLength"
           required="@SimpleAggregateRootMetadata.FirstNameIsRequired" />
    <small class="form-text text-muted">
        @if (SimpleAggregateRootMetadata.FirstNameIsRequired)
        {
            <span>Obrigatório. </span>
        }
        Entre @SimpleAggregateRootMetadata.FirstNameMinLength
        e @SimpleAggregateRootMetadata.FirstNameMaxLength caracteres.
    </small>
</div>
```

**4. Frontend React/TypeScript** (via API de metadados):

```typescript
// API expõe metadados como endpoint
// GET /api/metadata/person
{
    "firstName": {
        "isRequired": true,
        "minLength": 1,
        "maxLength": 255
    }
}

// React usa os metadados
const PersonForm = () => {
    const { data: metadata } = useQuery('/api/metadata/person');

    return (
        <input
            name="firstName"
            maxLength={metadata.firstName.maxLength}
            required={metadata.firstName.isRequired}
        />
    );
};
```

**5. Geração de Schema (OpenAPI/Swagger)**:

```csharp
// Swagger/OpenAPI gera schema a partir dos metadados
public class PersonMetadataSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CreatePersonRequest))
        {
            schema.Properties["firstName"].MaxLength =
                SimpleAggregateRootMetadata.FirstNameMaxLength;
            schema.Properties["firstName"].MinLength =
                SimpleAggregateRootMetadata.FirstNameMinLength;
        }
    }
}
```

### Validação Antecipada (Fail-Fast)

Com Single Source of Truth, camadas externas podem rejeitar dados inválidos **antes** de chegarem ao Domain:

```csharp
[HttpPost]
public async Task<IActionResult> CreatePerson(CreatePersonRequest request)
{
    // 1. ModelState já validou usando metadados do Domain
    if (!ModelState.IsValid)
        return BadRequest(ModelState); // Fail-fast na API

    // 2. Se chegou aqui, os dados básicos são válidos
    var context = new ExecutionContext(_timeProvider);

    // 3. Domain faz validações adicionais (regras de negócio)
    var person = SimpleAggregateRoot.RegisterNew(context, new RegisterNewInput(
        firstName: request.FirstName,
        lastName: request.LastName,
        birthDate: request.BirthDate
    ));

    if (person == null)
        return BadRequest(context.Messages);

    return Ok(person);
}
```

### Benefícios

1. **Consistência garantida**: Mesmas regras em todas as camadas
2. **Manutenção simplificada**: Mudou regra? Um lugar só
3. **Experiência do usuário**: Feedback imediato no frontend
4. **Fail-fast**: Erros detectados o mais cedo possível
5. **Documentação automática**: OpenAPI/Swagger reflete regras reais
6. **Testabilidade**: Testes unitários contra uma única fonte

### Trade-offs (Com Perspectiva)

- **Acoplamento entre camadas**: API referencia Domain.Entities
  - **Mitigação**: Este acoplamento é **intencional** e benéfico - garante consistência

### Trade-offs Frequentemente Superestimados

**"API não deveria conhecer o Domain"**

Na verdade, a API **precisa** conhecer as regras do Domain para validar corretamente. A alternativa (duplicar regras) é pior:

```csharp
// ❌ "Desacoplado" mas inconsistente
public class CreatePersonRequest
{
    [MaxLength(100)] // De onde veio esse número?
    public string FirstName { get; set; }
}

// ✅ "Acoplado" mas consistente
public class CreatePersonRequest
{
    [MaxLength(SimpleAggregateRootMetadata.FirstNameMaxLength)] // Fonte clara
    public string FirstName { get; set; }
}
```

O acoplamento a **metadados** não é o mesmo que acoplamento a **lógica de negócio**.

**"Frontend não consegue acessar metadados do backend"**

Soluções:

1. **Blazor/MAUI**: Acesso direto (mesmo assembly)
2. **React/Angular**: Endpoint de metadados (`GET /api/metadata`)
3. **Geração de código**: Gerar TypeScript a partir dos metadados

**"Data Annotations não aceitam propriedades, só const"**

Isso é verdade para valores dinâmicos, mas metadados são definidos no startup e não mudam durante a execução. Se usar `const`:

```csharp
// Funciona se os metadados forem const
public const int FirstNameMaxLength = 255;

[MaxLength(SimpleAggregateRootMetadata.FirstNameMaxLength)] // ✅ Compila
```

Se precisar de valores alteráveis no startup, use FluentValidation ou validação manual.

## Fundamentação Teórica

### DRY (Don't Repeat Yourself - Não Se Repita)

Andrew Hunt e David Thomas em "The Pragmatic Programmer" (1999):

> "Every piece of knowledge must have a single, unambiguous, authoritative representation within a system."
>
> *Todo pedaço de conhecimento deve ter uma única representação, não ambígua e oficial dentro de um sistema.*

Regras de validação são **conhecimento**. Duplicá-las viola DRY.

> "DRY is about the duplication of knowledge, of intent. It's about expressing the same thing in two different places, possibly in two totally different ways."
>
> *DRY é sobre a duplicação de conhecimento, de intenção. É sobre expressar a mesma coisa em dois lugares diferentes, possivelmente de duas formas totalmente diferentes.*

`MaxLength = 100` na API e `if (length > 100)` no Domain são a mesma regra expressa de formas diferentes - violação de DRY.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre o domínio como fonte de verdade:

> "The model is the backbone of a language used by all team members. [...] The model is distilled knowledge."
>
> *O modelo é a espinha dorsal de uma linguagem usada por todos os membros da equipe. [...] O modelo é conhecimento destilado.*

Regras de validação são conhecimento destilado do domínio. Devem residir no domínio, não espalhadas pelas camadas.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013):

> "The Domain Model should be the definitive source of business rules and logic."
>
> *O Modelo de Domínio deve ser a fonte definitiva de regras e lógica de negócio.*

Outras camadas (API, Frontend) devem **consultar** o domínio, não **redefinir** suas regras.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre constantes mágicas:

> "In general it is a bad idea to have raw numbers in your code. They should be hidden behind well-named constants."
>
> *Em geral é uma má ideia ter números brutos no seu código. Eles devem ser escondidos atrás de constantes bem nomeadas.*

`[MaxLength(100)]` é um número mágico. `[MaxLength(PersonMetadata.FirstNameMaxLength)]` é uma constante nomeada que documenta a origem.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre direção de dependências:

> "Source code dependencies must point only inward, toward higher-level policies."
>
> *Dependências de código fonte devem apontar apenas para dentro, em direção a políticas de nível mais alto.*

O Domain é a política de nível mais alto. A API deve depender do Domain (consultando metadados), não o contrário.

### Fail-Fast Principle

> "Fail fast is about finding problems as early as possible in the development process."
>
> *Fail fast é sobre encontrar problemas o mais cedo possível no processo de desenvolvimento.*
> — Jim Shore

Quanto mais cedo um erro é detectado, menor o custo de corrigi-lo:

1. **Compile-time**: Custo zero (IDE mostra erro)
2. **Frontend**: Custo baixo (feedback imediato)
3. **API**: Custo médio (round-trip de rede)
4. **Domain**: Custo alto (processamento desperdiçado)
5. **Database**: Custo altíssimo (transação falha, rollback)

Single Source of Truth permite validação em todas as camadas, mas com regras consistentes.

## Antipadrões Documentados

### Antipadrão 1: Magic Numbers Duplicados

```csharp
// ❌ Números mágicos em cada camada
// API
[MaxLength(100)]
public string FirstName { get; set; }

// Service
if (firstName.Length > 100) throw new Exception();

// Domain
if (firstName.Length > 100) return null;

// De onde veio 100? E se precisar mudar?
```

### Antipadrão 2: Validação Mais Restritiva na API

```csharp
// ❌ API mais restritiva que Domain
// API
[MaxLength(50)]  // Restringe a 50
public string FirstName { get; set; }

// Domain
public static int FirstNameMaxLength = 255;  // Permite 255

// Usuário não consegue usar 100 caracteres mesmo sendo válido no Domain!
```

### Antipadrão 3: Validação Mais Permissiva na API

```csharp
// ❌ API mais permissiva que Domain
// API
[MaxLength(500)]  // Permite 500
public string FirstName { get; set; }

// Domain
public static int FirstNameMaxLength = 255;  // Só permite 255

// Usuário envia 400 caracteres, passa pela API, falha no Domain
// Experiência ruim + processamento desperdiçado
```

### Antipadrão 4: Ignorar Metadados e Hardcodar

```csharp
// ❌ Metadados existem mas são ignorados
public static class PersonMetadata
{
    public static int FirstNameMaxLength = 255; // Definido aqui
}

// Desenvolvedor ignora e hardcoda
[MaxLength(100)] // Hardcoded, ignorando metadados
public string FirstName { get; set; }
```

## Decisões Relacionadas

- [DE-009](./DE-009-metodos-validate-publicos-e-estaticos.md) - Métodos Validate* públicos
- [DE-012](./DE-012-metadados-estaticos-vs-data-annotations.md) - Por que metadados estáticos
- [DE-013](./DE-013-nomenclatura-de-metadados.md) - Nomenclatura de metadados
- [DE-015](./DE-015-customizacao-de-metadados-apenas-no-startup.md) - Customização no startup

## Leitura Recomendada

- [The Pragmatic Programmer - DRY Principle](https://pragprog.com/titles/tpp20/the-pragmatic-programmer-20th-anniversary-edition/)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Fail-Fast Systems](https://martinfowler.com/ieeeSoftware/failFast.pdf)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Implementa o padrão de metadados como single source of truth para regras de validação |
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Consome os metadados das entidades para aplicar validações consistentes em todo o sistema |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Single Source of Truth
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_TEMPLATE: Uso em Camadas Externas
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Metadados de FirstName, LastName, BirthDate
