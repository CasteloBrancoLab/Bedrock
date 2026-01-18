# DE-009: Métodos Validate* Públicos e Estáticos

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você está em um aeroporto. Existem dois pontos de verificação de documentos:

1. **Check-in (antes de entrar na área restrita)**: Atendente verifica se você tem todos os documentos necessários. Se faltou algo, você volta para casa buscar - não perdeu tempo na fila de segurança.

2. **Embarque (já na área restrita)**: Comissário verifica documentos novamente. Se faltou algo agora, você perde o voo - é tarde demais para buscar.

Validar dados **apenas** no momento de criar/modificar a entidade é como verificar documentos só no embarque. O ideal é ter a **mesma verificação** disponível no check-in (camadas externas) para falhar o mais cedo possível.

### O Problema Técnico

Quando a lógica de validação está apenas dentro da entidade, camadas externas não conseguem validar inputs antes de tentar operações:

```csharp
// Validação encapsulada - só descobre erros ao tentar criar
public sealed class Person
{
    private Person(string firstName, string lastName) { ... }

    public static Person? RegisterNew(ExecutionContext context, RegisterNewInput input)
    {
        // Validação interna - não acessível externamente
        if (string.IsNullOrEmpty(input.FirstName))
        {
            context.AddMessage(...);
            return null;
        }
        // ...
    }
}

// Controller não consegue validar ANTES de chamar RegisterNew
[HttpPost]
public IActionResult Create(CreatePersonRequest request)
{
    var context = new ExecutionContext();

    // Não tem como saber se FirstName é válido sem tentar criar
    var person = Person.RegisterNew(context, request.ToInput());

    if (person == null)
        return BadRequest(context.Messages);

    // ...
}
```

Problemas desta abordagem:
- Controller não pode dar feedback imediato ao usuário
- Validação está duplicada se precisar checar em múltiplos lugares
- Não há como validar um campo isoladamente

## Como Normalmente é Feito

### Abordagem Tradicional

A maioria dos projetos usa uma das seguintes estratégias:

**1. Validadores externos (FluentValidation, DataAnnotations)**:

```csharp
public class PersonValidator : AbstractValidator<CreatePersonRequest>
{
    public PersonValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);
    }
}

// Problema: regras DUPLICADAS entre validador e entidade
public sealed class Person
{
    public static Person? RegisterNew(ExecutionContext context, RegisterNewInput input)
    {
        // Mesmas regras repetidas aqui
        if (string.IsNullOrEmpty(input.FirstName)) { ... }
        if (input.FirstName.Length < 3) { ... }
        if (input.FirstName.Length > 100) { ... }
    }
}
```

**2. Validação apenas na entidade**:

```csharp
// Toda validação encapsulada
public sealed class Person
{
    public static Person? RegisterNew(ExecutionContext context, RegisterNewInput input)
    {
        // Controller não tem acesso a esta lógica
        if (!IsValidFirstName(input.FirstName)) { ... }
    }

    private static bool IsValidFirstName(string? firstName) { ... } // Private!
}
```

### Por Que Não Funciona Bem

1. **Duplicação de regras**: Validador externo + entidade = duas fontes de verdade

```csharp
// FluentValidation diz: MinLength = 3
RuleFor(x => x.FirstName).MinimumLength(3);

// Entidade diz: MinLength = 2 (bug de sincronização!)
if (firstName.Length < 2) { ... }
```

2. **Validação tardia**: Descobrir erros apenas ao criar a entidade desperdiça processamento

```csharp
// Controller faz várias operações antes de descobrir erro
var enrichedData = await EnrichDataAsync(request); // Processamento pesado
var externalCheck = await CheckExternalServiceAsync(request); // Chamada HTTP

var person = Person.RegisterNew(context, input);
if (person == null) // Só agora descobre que FirstName estava vazio!
    return BadRequest(context.Messages);
```

3. **Impossível validar campos isoladamente**: Tudo ou nada

```csharp
// Quero validar só o FirstName (ex: validação em tempo real no frontend)
// Mas a única forma é tentar criar a entidade inteira
var person = Person.RegisterNew(context, new RegisterNewInput(
    firstName: request.FirstName,
    lastName: ???, // Preciso passar algo!
    birthDate: ??? // Preciso passar algo!
));
```

4. **Data Annotations com valores hardcoded**: Validação duplicada e dessincronizada

```csharp
// DTO com Data Annotations - valores hardcoded
public class CreatePersonRequest
{
    [Required(ErrorMessage = "FirstName é obrigatório")]
    [MinLength(3, ErrorMessage = "FirstName deve ter no mínimo 3 caracteres")]  // Hardcoded!
    [MaxLength(100, ErrorMessage = "FirstName deve ter no máximo 100 caracteres")] // Hardcoded!
    public string FirstName { get; set; }

    [Required]
    [MinLength(2)]  // E se a entidade exige 3? Dessincronizado!
    [MaxLength(50)] // E se a entidade permite 100? Bug silencioso!
    public string LastName { get; set; }
}

// Entidade com regras diferentes (ou iguais por coincidência)
public sealed class Person
{
    public static Person? RegisterNew(ExecutionContext context, RegisterNewInput input)
    {
        if (input.FirstName.Length < 3) { ... }  // 3 aqui
        if (input.FirstName.Length > 100) { ... } // 100 aqui
        if (input.LastName.Length < 3) { ... }   // 3 aqui, mas DTO diz 2!
    }
}
```

**Com metadados expostos, Data Annotations podem usar a fonte única de verdade**:

```csharp
// DTO usando metadados da entidade - sempre sincronizado
public class CreatePersonRequest
{
    [Required(ErrorMessage = "FirstName é obrigatório")]
    [MinLength(SimpleAggregateRootMetadata.FirstNameMinLength)] // Da entidade!
    [MaxLength(SimpleAggregateRootMetadata.FirstNameMaxLength)] // Da entidade!
    public string FirstName { get; set; }

    [Required]
    [MinLength(SimpleAggregateRootMetadata.LastNameMinLength)]  // Sempre correto
    [MaxLength(SimpleAggregateRootMetadata.LastNameMaxLength)]  // Sempre correto
    public string LastName { get; set; }
}

// Metadados são a single source of truth
public static class SimpleAggregateRootMetadata
{
    public static int FirstNameMinLength { get; private set; } = 3;
    public static int FirstNameMaxLength { get; private set; } = 100;
    public static int LastNameMinLength { get; private set; } = 3;
    public static int LastNameMaxLength { get; private set; } = 50;
}
```

Agora mudou a regra? Altera em **um lugar só** (metadata) e tanto o DTO quanto a entidade usam o valor correto.

## A Decisão

### Nossa Abordagem

Cada propriedade com regras de validação DEVE ter um método `Validate*` público e estático:

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    // Método público e estático - acessível de qualquer lugar
    public static bool ValidateFirstName(
        ExecutionContext executionContext,
        string? firstName
    )
    {
        // propertyName é gerado UMA vez - ValidationUtils injeta o sufixo
        string propertyName = CreateMessageCode<SimpleAggregateRoot>(
            propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
        );
        // propertyName = "SimpleAggregateRoot.FirstName"

        // ValidationUtils injeta ".IsRequired" ? "SimpleAggregateRoot.FirstName.IsRequired"
        bool firstNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: propertyName,
            isRequired: SimpleAggregateRootMetadata.FirstNameIsRequired,
            value: firstName
        );

        // ValidationUtils injeta ".MinLength" ? "SimpleAggregateRoot.FirstName.MinLength"
        bool firstNameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: propertyName,
            minLength: SimpleAggregateRootMetadata.FirstNameMinLength,
            value: firstName?.Length ?? 0
        );

        // ValidationUtils injeta ".MaxLength" ? "SimpleAggregateRoot.FirstName.MaxLength"
        bool firstNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: propertyName,
            maxLength: SimpleAggregateRootMetadata.FirstNameMaxLength,
            value: firstName?.Length ?? 0
        );

        // Bitwise AND para executar TODAS as validações (coleta todos os erros)
        return firstNameIsRequiredValidation
            & firstNameMinLengthValidation
            & firstNameMaxLengthValidation;
    }

    public static bool ValidateLastName(
        ExecutionContext executionContext,
        string? lastName
    ) { ... }

    public static bool ValidateBirthDate(
        ExecutionContext executionContext,
        BirthDate? birthDate
    ) { ... }
}
```

**Uso em Controller (validação antecipada)**:

```csharp
[HttpPost]
public IActionResult Create(CreatePersonRequest request)
{
    var context = new ExecutionContext(_timeProvider);

    // Validação ANTES de qualquer processamento pesado
    bool isValid =
        SimpleAggregateRoot.ValidateFirstName(context, request.FirstName)
        & SimpleAggregateRoot.ValidateLastName(context, request.LastName)
        & SimpleAggregateRoot.ValidateBirthDate(context, request.BirthDate);

    if (!isValid)
        return BadRequest(context.Messages); // Fail-fast!

    // Só processa se input válido
    var enrichedData = await EnrichDataAsync(request);
    var person = SimpleAggregateRoot.RegisterNew(context, request.ToInput());

    // ...
}
```

**Uso para validação em tempo real (campo individual)**:

```csharp
[HttpPost("validate-firstname")]
public IActionResult ValidateFirstName([FromBody] string firstName)
{
    var context = new ExecutionContext(_timeProvider);

    // Valida APENAS o FirstName - sem precisar de outros campos
    bool isValid = SimpleAggregateRoot.ValidateFirstName(context, firstName);

    return isValid
        ? Ok()
        : BadRequest(context.Messages);
}
```

### Por Que é Público

Métodos `Validate*` são públicos porque:

1. **Camadas externas precisam validar**: Controllers, serviços, consumers
2. **Single source of truth**: Mesma lógica usada em toda a aplicação
3. **Testabilidade**: Testes podem verificar validação isoladamente

### Por Que é Estático

Métodos `Validate*` são estáticos porque:

1. **Não dependem de instância**: Validação acontece ANTES da entidade existir
2. **Validação de input, não de estado**: O input pode nem se tornar uma entidade
3. **Performance**: Sem alocação de objeto para validar

```csharp
// Estático: valida input antes de criar entidade
bool isValid = SimpleAggregateRoot.ValidateFirstName(context, "John");

// Se fosse de instância, precisaria de uma entidade que ainda não existe!
var person = new SimpleAggregateRoot(); // ? Construtor privado
bool isValid = person.ValidateFirstName(context, "John"); // ? Impossível
```

### Características Importantes

**1. Métodos Validate* são puros (sem side-effects na entidade)**:

```csharp
// Validate* NUNCA modifica estado - apenas retorna bool e adiciona mensagens
public static bool ValidateFirstName(ExecutionContext context, string? firstName)
{
    // ? Adiciona mensagens ao context (permitido - é o propósito)
    // ? Nunca modifica campos da entidade
    // ? Nunca faz I/O
    // ? Nunca depende de estado externo (exceto metadata)
}
```

**2. Regras vêm dos Metadata (single source of truth)**:

```csharp
// Regras definidas em um só lugar
public static class SimpleAggregateRootMetadata
{
    public static bool FirstNameIsRequired { get; private set; } = true;
    public static int FirstNameMinLength { get; private set; } = 3;
    public static int FirstNameMaxLength { get; private set; } = 100;
}

// Validate* usa os metadata - não hardcoda valores
public static bool ValidateFirstName(ExecutionContext context, string? firstName)
{
    string propertyName = CreateMessageCode<SimpleAggregateRoot>(
        SimpleAggregateRootMetadata.FirstNamePropertyName
    );

    // ? Usa metadata
    ValidationUtils.ValidateMinLength(
        context,
        propertyName: propertyName,
        minLength: SimpleAggregateRootMetadata.FirstNameMinLength, // Do metadata
        value: firstName?.Length ?? 0
    );

    // ? Não hardcoda
    ValidationUtils.ValidateMinLength(
        context,
        propertyName: propertyName,
        minLength: 3, // Hardcoded - NÃO FAÇA ISSO
        value: firstName?.Length ?? 0
    );
}
```

**3. Validação completa com IsValid estático**:

```csharp
// Valida TODOS os campos de uma vez
public static bool IsValid(
    ExecutionContext executionContext,
    EntityInfo entityInfo,
    string? firstName,
    string? lastName,
    BirthDate? birthDate
)
{
    return
        EntityBaseIsValid(executionContext, entityInfo)  // Valida propriedades da classe base
        & ValidateFirstName(executionContext, firstName)
        & ValidateLastName(executionContext, lastName)
        & ValidateBirthDate(executionContext, birthDate);
}
```

**4. EntityBaseIsValid - Validação da Classe Base**:

O método `EntityBaseIsValid` é definido em `EntityBase` e valida as propriedades herdadas (EntityInfo).
Classes filhas DEVEM chamar este método no seu `IsValid` estático para garantir validação completa.

⚠️ **VALIDAÇÃO VIA ROSLYN**: A presença da chamada `EntityBaseIsValid` no método `IsValid` estático
será validada por um Roslyn Analyzer. Entidades que não chamarem este método receberão um warning/error
em tempo de compilação.

```csharp
// EntityBase define o método de validação das propriedades base
public abstract class EntityBase
{
    public static bool EntityBaseIsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo
    )
    {
        return ValidateEntityInfo(executionContext, entityInfo);
    }
}

// Entidade concreta compõe validação da base + suas próprias
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? firstName,
        string? lastName,
        BirthDate? birthDate
    )
    {
        return
            EntityBaseIsValid(executionContext, entityInfo)  // ✅ OBRIGATÓRIO - Validado via Roslyn
            & ValidateFirstName(executionContext, firstName)
            & ValidateLastName(executionContext, lastName)
            & ValidateBirthDate(executionContext, birthDate);
    }
}
```

## Consequências

### Benefícios

- **Single source of truth**: Mesma lógica de validação em toda a aplicação
- **Fail-fast**: Erros detectados o mais cedo possível
- **Validação granular**: Cada campo pode ser validado isoladamente
- **Testabilidade**: Validação testável sem criar entidades
- **Reutilização**: Controllers, serviços, consumers usam os mesmos métodos

### Trade-offs (Com Perspectiva)

- **API surface maior**: Cada propriedade adiciona um método público

### Trade-offs Frequentemente Superestimados

**"Expõe lógica interna da entidade"**

Na verdade, expõe apenas as **regras de validação** - não a lógica de negócio:

```csharp
// Validate* expõe: "FirstName deve ter 3-100 caracteres"
// Não expõe: como a entidade usa FirstName internamente

public static bool ValidateFirstName(...) // ? OK expor

private void ProcessFirstNameForBusinessLogic(...) // Permanece privado
```

Regras de validação são parte do **contrato público** da entidade - faz sentido que sejam acessíveis.

**"FluentValidation faz isso melhor"**

FluentValidation é excelente para validação de **requests/DTOs**. Mas para validação de **regras de domínio**:

```csharp
// FluentValidation: regras separadas da entidade
public class PersonValidator : AbstractValidator<CreatePersonRequest>
{
    public PersonValidator()
    {
        // Regra aqui...
        RuleFor(x => x.FirstName).MinimumLength(3);
    }
}

// Entidade: mesma regra duplicada
public static Person? RegisterNew(...)
{
    // ...e aqui também
    if (firstName.Length < 3) { ... }
}
```

Com nossos métodos `Validate*`:

```csharp
// Regra em um só lugar - na entidade
public static bool ValidateFirstName(ExecutionContext context, string? firstName)
{
    string propertyName = CreateMessageCode<SimpleAggregateRoot>(
        SimpleAggregateRootMetadata.FirstNamePropertyName
    );

    // ValidationUtils injeta ".MinLength" automaticamente
    return ValidationUtils.ValidateMinLength(
        context,
        propertyName: propertyName,
        minLength: SimpleAggregateRootMetadata.FirstNameMinLength, // = 3
        value: firstName?.Length ?? 0
    );
}

// FluentValidation pode CHAMAR o método da entidade se necessário
public class PersonValidator : AbstractValidator<CreatePersonRequest>
{
    public PersonValidator()
    {
        RuleFor(x => x.FirstName)
            .Must((request, firstName, context) =>
            {
                var execContext = new ExecutionContext();
                return SimpleAggregateRoot.ValidateFirstName(execContext, firstName);
            });
    }
}
```

**"Aumenta acoplamento entre camadas"**

O acoplamento é **intencional e desejável**:

```csharp
// Controller depende da ENTIDADE para validação
// Isso é correto - a entidade DEFINE as regras

// Alternativa seria duplicar regras:
// - Controller conhece regras de FirstName
// - Entidade conhece regras de FirstName
// = Duas fontes de verdade = bugs de sincronização
```

## Fundamentação Teórica

### Padrões de Design Relacionados

**Specification Pattern** - Cada método `Validate*` pode ser visto como uma Specification que determina se um valor atende aos critérios. A diferença é que não criamos objetos Specification separados - o método estático já encapsula a especificação.

**Facade Pattern** - Os métodos `Validate*` atuam como facade para a lógica de validação interna, expondo uma interface simples para camadas externas.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) discute **Specification Pattern** para validações:

> "A SPECIFICATION is a predicate that determines if an object does or does not satisfy some criteria."
>
> *Uma SPECIFICATION é um predicado que determina se um objeto satisfaz ou não algum critério.*

Nossos métodos `Validate*` são specifications inline - predicados que determinam se um valor é válido.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) enfatiza que a **validação é responsabilidade do domínio**:

> "The domain model should be responsible for enforcing its own invariants and business rules."
>
> *O modelo de domínio deve ser responsável por impor suas próprias invariantes e regras de negócio.*

Métodos `Validate*` públicos permitem que o domínio **compartilhe** (não delegue) essa responsabilidade com camadas externas.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende **DRY (Don't Repeat Yourself)**:

> "Every piece of knowledge must have a single, unambiguous, authoritative representation within a system."
>
> *Cada pedaço de conhecimento deve ter uma única, não-ambígua, representação autoritativa dentro de um sistema.*

Métodos `Validate*` públicos eliminam a duplicação de regras entre validadores externos e entidades.

O princípio de **funções pequenas e focadas** também se aplica:

> "Functions should do one thing. They should do it well. They should do it only."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso.*

Cada `ValidateFirstName`, `ValidateLastName`, etc. faz exatamente uma coisa: validar aquele campo específico.

### O Que o Clean Architecture Diz

Clean Architecture coloca **Entities** no centro. As regras de validação são **Enterprise Business Rules** - pertencem à entidade.

Expor `Validate*` como públicos permite que camadas externas **consultem** as regras sem violá-las. A entidade permanece dona das regras; outros apenas as utilizam.

### Outros Fundamentos

**Fail-Fast Principle**:

> "Fail-fast systems are designed to immediately report any failure or condition that is likely to lead to failure."
>
> *Sistemas fail-fast são projetados para reportar imediatamente qualquer falha ou condição que provavelmente leve a falha.*

Métodos `Validate*` públicos permitem fail-fast na entrada do sistema, antes de processamento desnecessário.

**Information Expert (GRASP)**:

GRASP sugere que a responsabilidade deve estar com quem tem a informação necessária. A entidade tem as regras de validação, então ela deve expor a capacidade de validar.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre validação no domínio e validação na apresentação?"
- "Como o Specification Pattern se relaciona com métodos de validação?"
- "Por que DRY é importante para regras de validação?"
- "Como evitar duplicação de regras entre FluentValidation e entidades?"

### Leitura Recomendada

- [Domain-Driven Design - Specification Pattern](https://martinfowler.com/apsupp/spec.pdf)
- [Validation in DDD](https://enterprisecraftsmanship.com/posts/validation-and-ddd/)
- [Clean Code - Chapter 3: Functions](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Fornece métodos padronizados de validação que são chamados pelos métodos Validate* públicos e estáticos das entidades |
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Recebe as mensagens de validação dos métodos Validate*, permitindo validação antecipada em camadas externas |

## Referências no Código

- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - método EntityBaseIsValid para validação das propriedades da classe base
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Métodos de Validação Estáticos
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_TEMPLATE: Validação em Controller
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - método IsValid estático (chamando EntityBaseIsValid)
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Métodos Validate* São Puros
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ValidateFirstName completo
