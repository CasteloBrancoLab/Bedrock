# DE-011: Parâmetros Validate* Nullable por Design

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um formulário de cadastro onde o campo "Telefone" é opcional para clientes pessoa física, mas obrigatório para clientes pessoa jurídica. A regra de obrigatoriedade depende do **contexto de negócio**, não é fixa.

Se o sistema forçar que telefone seja sempre preenchido no frontend (validação de compile-time), ele quebraria o fluxo de pessoa física. Se permitir null no frontend mas forçar no backend, gera inconsistência.

A solução é: o sistema ACEITA null, e a **regra de negócio em runtime** decide se null é válido ou não.

### O Problema Técnico

Em C# com nullable reference types habilitado, a assinatura do método influencia o comportamento do compilador:

```csharp
// Opção 1: Parâmetro non-null
public static bool ValidateFirstName(
    ExecutionContext executionContext,
    string firstName  // Compilador força que caller passe non-null
)

// Opção 2: Parâmetro nullable
public static bool ValidateFirstName(
    ExecutionContext executionContext,
    string? firstName  // Compilador permite null, validação em runtime
)
```

A escolha entre essas opções tem implicações profundas para o design do sistema.

## Como Normalmente é Feito

### Abordagem Tradicional

A maioria dos projetos usa parâmetros non-null quando "o campo é obrigatório":

```csharp
public sealed class Person
{
    // "FirstName é obrigatório, então não aceita null"
    public static bool ValidateFirstName(
        ExecutionContext context,
        string firstName  // Non-null - compile-time enforcement
    )
    {
        if (firstName.Length < 3)
        {
            context.AddErrorMessage("FirstName muito curto");
            return false;
        }
        return true;
    }
}

// Uso - compilador força non-null
string? userInput = GetUserInput();

// ? Compilador reclama: cannot convert string? to string
Person.ValidateFirstName(context, userInput);

// "Solução" comum: null-coalescing ou assertion
Person.ValidateFirstName(context, userInput ?? "");  // Esconde o problema!
Person.ValidateFirstName(context, userInput!);      // Silencia o compilador!
```

### Por Que Não Funciona Bem

1. **Regras de obrigatoriedade são dinâmicas, não estáticas**:

```csharp
// A obrigatoriedade pode variar por:

// Tenant/Cliente
// - Tenant A: FirstName obrigatório
// - Tenant B: FirstName opcional (cadastro simplificado)

// Configuração
// - appsettings.Development.json: IsRequired = false (testes mais fáceis)
// - appsettings.Production.json: IsRequired = true

// Região/Compliance
// - Brasil: CPF obrigatório
// - Internacional: CPF não existe

// Se o parâmetro for non-null, como modelar isso?
public static bool ValidateFirstName(string firstName)  // Non-null
{
    // Se IsRequired = false, firstName = "" é válido
    // Mas o chamador não pode passar null - inconsistência!
}
```

2. **Força workarounds que escondem bugs**:

```csharp
// Input do usuário é SEMPRE nullable (pode deixar campo vazio)
string? userFirstName = request.FirstName;

// Com parâmetro non-null, caller precisa fazer algo:
ValidateFirstName(context, userFirstName ?? "");  // Converte null para ""
ValidateFirstName(context, userFirstName!);       // Ignora nullability

// Problema: se firstName for null E IsRequired = true, qual erro aparece?
// - Com ?? "": erro será "muito curto" (Length < 3), não "obrigatório"
// - Com !: NullReferenceException em runtime se acessar firstName.Length

// O ERRO REAL (campo obrigatório não preenchido) foi mascarado
```

3. **Quebra o contrato semântico do método Validate***:

```csharp
// O nome "ValidateFirstName" sugere:
// "Valide se este valor é um FirstName válido"

// Se o parâmetro é non-null:
// - Null já foi tratado ANTES de chamar
// - O método não pode validar obrigatoriedade
// - Parte da validação está espalhada no chamador

// Se o parâmetro é nullable:
// - O método recebe QUALQUER input
// - O método decide se null é válido ou não
// - Validação completa em UM lugar
```

4. **Gera inconsistência entre camadas**:

```csharp
// Controller recebe DTO com tudo nullable (realidade da API)
public class CreatePersonRequest
{
    public string? FirstName { get; set; }  // JSON permite null/ausente
    public string? LastName { get; set; }
}

// Se Validate* exige non-null:
[HttpPost]
public IActionResult Create(CreatePersonRequest request)
{
    // Precisa converter nullable ? non-null ANTES de validar
    if (request.FirstName == null)
    {
        // Erro manual aqui...
    }

    // Agora pode chamar
    Person.ValidateFirstName(context, request.FirstName);  // Já verificou null
}

// Validação de obrigatoriedade está NO CONTROLLER, não na ENTIDADE
// Se outro controller esquecer essa verificação = bug
```

## A Decisão

### Nossa Abordagem

Métodos `Validate*` DEVEM declarar parâmetros como **nullable**, delegando a decisão de obrigatoriedade para runtime:

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    public static bool ValidateFirstName(
        ExecutionContext executionContext,
        string? firstName  // ? Nullable por design
    )
    {
        // propertyName para mensagens de erro
        string propertyName = CreateMessageCode<SimpleAggregateRoot>(
            propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
        );

        // 1. Validação de obrigatoriedade em RUNTIME
        // A regra IsRequired vem do metadata, pode ser true ou false
        bool isRequiredValid = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: propertyName,
            isRequired: SimpleAggregateRootMetadata.FirstNameIsRequired,  // Dinâmico!
            value: firstName
        );

        // Se IsRequired=true e firstName=null ? isRequiredValid=false
        // Se IsRequired=false e firstName=null ? isRequiredValid=true (null é válido!)

        if (!isRequiredValid)
            return false;

        // 2. Validações de formato só executam se valor existe
        // Neste ponto, firstName não é null (ou IsRequired=false e null é ok)
        bool minLengthValid = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: propertyName,
            minLength: SimpleAggregateRootMetadata.FirstNameMinLength,
            value: firstName!.Length  // Safe: já validou que não é null se necessário
        );

        bool maxLengthValid = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: propertyName,
            maxLength: SimpleAggregateRootMetadata.FirstNameMaxLength,
            value: firstName!.Length
        );

        return isRequiredValid && minLengthValid && maxLengthValid;
    }
}
```

**Metadata define a obrigatoriedade (pode ser alterada em startup)**:

```csharp
public static class SimpleAggregateRootMetadata
{
    // Valores padrão - podem ser alterados via ChangeFirstNameMetadata()
    public static string FirstNamePropertyName { get; } = nameof(SimpleAggregateRoot.FirstName);
    public static bool FirstNameIsRequired { get; private set; } = true;
    public static int FirstNameMinLength { get; private set; } = 1;
    public static int FirstNameMaxLength { get; private set; } = 255;

    // Customização em startup (multitenancy, configuração)
    public static void ChangeFirstNameMetadata(
        bool? isRequired = null,
        int? minLength = null,
        int? maxLength = null
    )
    {
        lock (_lockObject)
        {
            if (isRequired.HasValue)
                FirstNameIsRequired = isRequired.Value;
            // ...
        }
    }
}
```

**Uso no Controller - simples e direto**:

```csharp
[HttpPost]
public IActionResult Create(CreatePersonRequest request)
{
    var context = new ExecutionContext();

    // ? Passa diretamente o valor nullable do DTO
    // Validate* decide se null é erro ou não baseado no metadata
    bool isValid =
        SimpleAggregateRoot.ValidateFirstName(context, request.FirstName)
        & SimpleAggregateRoot.ValidateLastName(context, request.LastName)
        & SimpleAggregateRoot.ValidateBirthDate(context, request.BirthDate);

    if (!isValid)
        return BadRequest(context.Messages);

    // Criar entidade...
}
```

### Fluxo de Validação

```
                    +-----------------------------------------+
                    |        ValidateFirstName(string?)       |
                    +-----------------------------------------+
                                       |
                                       v
                    +-----------------------------------------+
                    |    firstName == null?                   |
                    +-----------------------------------------+
                              |                    |
                        SIM   |                    |   NÃO
                              v                    v
              +-------------------------+  +-------------------------+
              | IsRequired == true?     |  | Validar MinLength       |
              +-------------------------+  | Validar MaxLength       |
                    |            |         | Validar Formato         |
              SIM   |            | NÃO     +-------------------------+
                    v            v
          +--------------+  +--------------+
          | return false |  | return true  |
          | (erro)       |  | (null ok!)   |
          +--------------+  +--------------+
```

### Por Que `string?` e não `string`

| Aspecto | `string` (non-null) | `string?` (nullable) |
|---------|---------------------|----------------------|
| Obrigatoriedade | Forçada em compile-time | Decidida em runtime |
| Multitenancy | Não suporta variação | Suporta via metadata |
| Input de API | Requer conversão manual | Aceita diretamente |
| Responsabilidade | Espalhada (caller + method) | Centralizada (method) |
| Mensagem de erro | "Cannot be null" (genérico) | "FirstName.IsRequired" (específico) |

### Padrão de Decisão

| Parâmetro do Método | Nullability | Razão |
|---------------------|-------------|-------|
| `ExecutionContext` | Non-null | Sempre obrigatório, não é dado de entrada |
| Valor a validar (string, int, etc.) | Nullable | Obrigatoriedade é regra de negócio |
| Value Objects (BirthDate, Email) | Nullable | Mesmo princípio - regra dinâmica |

## Consequências

### Benefícios

- **Regras dinâmicas**: Obrigatoriedade pode variar por tenant, configuração ou região
- **Single source of truth**: Validação de obrigatoriedade na entidade, não espalhada
- **API consistente**: DTOs nullable fluem diretamente para Validate*
- **Mensagens precisas**: "FirstName.IsRequired" ao invés de NullReferenceException
- **Testabilidade**: Fácil testar cenário "campo opcional" vs "campo obrigatório"

### Trade-offs (Com Perspectiva)

- **Null-forgiving operator (`!`)**: Após validar IsRequired, usamos `firstName!.Length`
- **Desenvolvedor precisa entender o fluxo**: Validação em etapas (IsRequired ? formato)

### Trade-offs Frequentemente Superestimados

**"Parâmetro nullable é menos type-safe"**

Na verdade, é MAIS type-safe porque reflete a realidade:

```csharp
// Com parâmetro non-null - caller esconde nulls
ValidateFirstName(context, userInput ?? "");  // Bug mascarado!

// Com parâmetro nullable - nulls são tratados explicitamente
ValidateFirstName(context, userInput);  // Método decide se é erro
```

O tipo `string?` é honesto sobre o que o método aceita. O tipo `string` força o caller a mentir (converter null para algo).

**"Vai causar NullReferenceException"**

Somente se o método for implementado incorretamente:

```csharp
// ? ERRADO - não verifica null antes de acessar propriedade
public static bool ValidateFirstName(ExecutionContext context, string? firstName)
{
    if (firstName.Length < 3)  // NullReferenceException se null!
    { ... }
}

// ? CORRETO - valida IsRequired ANTES de acessar propriedades
public static bool ValidateFirstName(ExecutionContext context, string? firstName)
{
    bool isRequiredValid = ValidationUtils.ValidateIsRequired(..., firstName);

    if (!isRequiredValid)
        return false;  // Não continua se null e obrigatório

    // Agora é safe acessar firstName.Length
    if (firstName!.Length < 3)
    { ... }
}
```

O padrão de validar `IsRequired` primeiro elimina o risco.

**"É mais trabalho para o desenvolvedor"**

Na verdade, é MENOS trabalho total:

```csharp
// Com non-null - trabalho no CALLER (espalhado em N lugares)
if (request.FirstName == null)
    return BadRequest("FirstName required");
Person.ValidateFirstName(context, request.FirstName);

// Com nullable - trabalho no METHOD (centralizado em 1 lugar)
Person.ValidateFirstName(context, request.FirstName);
```

Centralizar é menos código total e menos bugs.

## Fundamentação Teórica

### Padrões de Design Relacionados

**Null Object Pattern (variação)** - Ao invés de usar um objeto "nulo" especial, tratamos null explicitamente no método de validação. O método é o "guardião" que decide se null é válido.

**Strategy Pattern (implícito)** - A decisão de obrigatoriedade pode ser vista como uma estratégia injetada via metadata. O método não hardcoda a regra - consulta uma fonte externa.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) enfatiza que regras de negócio pertencem ao domínio:

> "Business rules often do not fit the responsibility of any of the obvious ENTITIES or VALUE OBJECTS, and their variety and combinations can overwhelm the basic meaning of the domain object."
>
> *Regras de negócio frequentemente não se encaixam na responsabilidade de nenhuma ENTITY ou VALUE OBJECT óbvia, e sua variedade e combinações podem sobrecarregar o significado básico do objeto de domínio.*

A regra "FirstName é obrigatório" é uma regra de negócio que pode variar. Colocá-la em metadata permite variação sem mudar o código.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre validação:

> "The means by which the objects of a domain model are validated is largely a matter of implementation."
>
> *Os meios pelos quais os objetos de um modelo de domínio são validados são em grande parte uma questão de implementação.*

Nossa implementação permite que a mesma validação suporte diferentes políticas de obrigatoriedade.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende **funções que fazem uma coisa**:

> "Functions should do one thing. They should do it well. They should do it only."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso.*

`ValidateFirstName` faz UMA coisa: validar firstName completamente. Se o caller precisasse verificar null antes, seriam DUAS coisas (verificar null + validar formato).

O princípio **"Don't Pass Null"** parece contradizer nossa abordagem, mas é diferente:

> "Returning null from methods is bad, but passing null into methods is worse."

Isso se refere a passar null quando **não deveria** ser null. No nosso caso, null é um valor válido potencial que precisa ser validado.

### O Que o Clean Architecture Diz

Clean Architecture coloca regras de negócio no centro. A regra "campo é obrigatório" é uma regra de negócio que deve estar na camada de domínio, não na camada de apresentação (controller).

Com parâmetros nullable, a regra de obrigatoriedade fica na entidade. Com parâmetros non-null, ela vaza para o controller.

### Outros Fundamentos

**Nullable Reference Types (C# 8+)**:

A feature foi projetada para expressar **intenção**, não para forçar valores. Um parâmetro `string?` diz: "este método aceita e trata null adequadamente".

**API Design Guidelines**:

APIs públicas devem ser **tolerantes no que aceitam** (Postel's Law):

> "Be conservative in what you send, be liberal in what you accept."
>
> *Seja conservador no que envia, seja liberal no que aceita.*

Aceitar null e validar em runtime é mais liberal (e mais robusto) que rejeitar em compile-time.

**Fail-Fast (correto)**:

Fail-fast significa detectar erros **o mais cedo possível**. Com nullable + `ValidateIsRequired`, o erro "campo obrigatório" é detectado imediatamente pelo método. Com non-null + workaround, o erro é mascarado e pode aparecer depois.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre nullable reference types e nullable value types em C#?"
- "Como Postel's Law (Robustness Principle) se aplica a design de APIs?"
- "Por que validação de obrigatoriedade deve ser em runtime vs compile-time?"
- "Como implementar regras de validação configuráveis por tenant?"

### Leitura Recomendada

- [Nullable Reference Types - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Postel's Law - Wikipedia](https://en.wikipedia.org/wiki/Robustness_principle)
- [Clean Code - Chapter 7: Error Handling](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)
- [Fail-Fast - Martin Fowler](https://www.martinfowler.com/ieeeSoftware/failFast.pdf)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Métodos de validação aceitam parâmetros nullable por design, permitindo validação robusta de inputs |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Parâmetros São Nullable Por Design
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ValidateFirstName com string?
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Customização em Runtime - Change*Metadata
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Métodos Change*Metadata() - Startup Only
