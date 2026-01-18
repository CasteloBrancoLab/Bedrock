
# DE-010: ValidationUtils para Validações Padrão

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fábrica que produz diferentes produtos (carros, motos, caminhões). Cada produto precisa passar por verificações de qualidade: peso está dentro do limite? Dimensões estão corretas? Documentação está completa?

**Abordagem ruim**: Cada linha de produção implementa suas próprias verificações. A linha de carros verifica peso de um jeito, a de motos de outro. Se descobrem um bug na verificação de peso, precisam corrigir em três lugares. Se esquecem de um, produtos defeituosos passam.

**Abordagem boa**: Um laboratório de qualidade centralizado fornece os testes padronizados. Todas as linhas usam os mesmos equipamentos de medição. Correção em um lugar propaga para todos.

`ValidationUtils` é nosso "laboratório de qualidade" - métodos padronizados que todas as entidades usam.

### O Problema Técnico

Quando cada entidade implementa suas próprias validações básicas, surgem inconsistências:

```csharp
// Entidade Person - validação de required
public static bool ValidateFirstName(ExecutionContext context, string? firstName)
{
    if (string.IsNullOrEmpty(firstName))
    {
        context.AddMessage(new Message(
            MessageType.Error,
            "PERSON_FIRSTNAME_REQUIRED",
            "First name is required"
        ));
        return false;
    }
    return true;
}

// Entidade Product - validação de required (implementada diferente!)
public static bool ValidateName(ExecutionContext context, string? name)
{
    if (name == null)  // Bug: não verifica string vazia!
    {
        context.AddMessage(new Message(
            MessageType.Error,
            "PRODUCT.NAME.REQUIRED",  // Formato diferente!
            "Name cannot be null"  // Mensagem diferente!
        ));
        return false;
    }
    return true;
}

// Entidade Order - validação de required (outra variação!)
public static bool ValidateDescription(ExecutionContext context, string? description)
{
    if (string.IsNullOrWhiteSpace(description))  // Diferente: verifica whitespace
    {
        context.AddError("Description is mandatory");  // API diferente!
        return false;
    }
    return true;
}
```

Problemas desta abordagem:

1. **Inconsistência de comportamento**: `null`, `""`, `"   "` tratados diferente em cada entidade
2. **Mensagens não padronizadas**: Formatos de código e texto variam
3. **Bugs duplicados**: Mesma lógica incorreta copiada entre entidades
4. **Manutenção exponencial**: N entidades × M validações = N×M implementações

## Como Normalmente é Feito

### Abordagem Tradicional

**1. Cada entidade implementa suas validações**:

```csharp
public sealed class Person
{
    public static bool ValidateFirstName(ExecutionContext context, string? firstName)
    {
        // Implementação local
        if (string.IsNullOrEmpty(firstName))
        {
            context.AddMessage(...);
            return false;
        }

        if (firstName.Length < 3)
        {
            context.AddMessage(...);
            return false;
        }

        if (firstName.Length > 100)
        {
            context.AddMessage(...);
            return false;
        }

        return true;
    }
}
```

**2. FluentValidation com regras inline**:

```csharp
public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(3).WithMessage("First name must be at least 3 characters")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");
    }
}
```

### Por Que Não Funciona Bem

1. **Código duplicado em escala**:

```csharp
// Person.ValidateFirstName - 20 linhas
// Person.ValidateLastName - 20 linhas (quase idênticas)
// Person.ValidateEmail - 25 linhas
// Product.ValidateName - 20 linhas (cópia de FirstName)
// Product.ValidateDescription - 20 linhas
// Order.ValidateCustomerName - 20 linhas (outra cópia)
// ... × 50 entidades × 5 propriedades cada = 5000 linhas repetidas
```

2. **Propagação de bugs**:

```csharp
// Bug: esqueceu de verificar string vazia em ValidateMinLength
public static bool ValidateFirstName(...)
{
    if (firstName == null) return false;  // OK
    if (firstName.Length < 3) { ... }     // Bug: firstName pode ser ""!
}

// Mesmo bug copiado para outras 30 propriedades...
```

3. **Mensagens inconsistentes**:

```csharp
// API retorna erros com formatos diferentes
{
    "errors": [
        { "code": "PERSON_FIRSTNAME_REQUIRED", "message": "First name is required" },
        { "code": "PRODUCT.NAME.REQUIRED", "message": "Name cannot be null" },
        { "code": "ORDER-DESC-MANDATORY", "message": "Description is mandatory" }
    ]
}
// Frontend precisa tratar 3 formatos diferentes para a mesma validação
```

4. **Dificuldade de i18n**:

```csharp
// Sem padrão, cada entidade formata mensagens diferente
context.AddMessage("First name is required");  // Hardcoded em inglês
context.AddMessage($"O campo {fieldName} é obrigatório");  // Português inline
context.AddMessage(Resources.FieldRequired);  // Resource file
// Qual usar? Como padronizar?
```

## A Decisão

### Nossa Abordagem

`ValidationUtils` fornece métodos de validação reutilizáveis e padronizados. O método recebe o `propertyName` e **injeta automaticamente** o sufixo do tipo de validação usando o enum `ValidationType`:

```csharp
public enum ValidationType
{
    IsRequired = 1,
    MinLength = 2,
    MaxLength = 3,
}

public static class ValidationUtils
{
    /// <summary>
    /// Valida se um valor obrigatório foi fornecido.
    /// O método INJETA o sufixo ".IsRequired" automaticamente.
    /// </summary>
    public static bool ValidateIsRequired<TValue>(
        ExecutionContext executionContext,
        string propertyName,  // Ex: "SimpleAggregateRoot.FirstName"
        bool isRequired,
        TValue? value
    )
    {
        if (isRequired && (value is null || value.Equals(default(TValue))))
        {
            executionContext.AddErrorMessage(
                code: $"{propertyName}.{ValidationType.IsRequired}"  // Injeta sufixo!
            );
            // Resultado: "SimpleAggregateRoot.FirstName.IsRequired"

            return false;
        }

        return true;
    }

    /// <summary>
    /// Valida comprimento/valor mínimo.
    /// O método INJETA o sufixo ".MinLength" automaticamente.
    /// </summary>
    public static bool ValidateMinLength<TValue>(
        ExecutionContext executionContext,
        string propertyName,
        TValue minLength,
        TValue? value
    ) where TValue : IComparable<TValue>
    {
        if (value is null)
            return true;

        if (value.CompareTo(minLength) < 0)
        {
            executionContext.AddErrorMessage(
                code: $"{propertyName}.{ValidationType.MinLength}"  // Injeta sufixo!
            );
            // Resultado: "SimpleAggregateRoot.FirstName.MinLength"

            return false;
        }

        return true;
    }

    /// <summary>
    /// Valida comprimento/valor máximo.
    /// O método INJETA o sufixo ".MaxLength" automaticamente.
    /// </summary>
    public static bool ValidateMaxLength<TValue>(
        ExecutionContext executionContext,
        string propertyName,
        TValue maxLength,
        TValue? value
    ) where TValue : IComparable<TValue>
    {
        if (value is null)
            return true;

        if (value.CompareTo(maxLength) > 0)
        {
            executionContext.AddErrorMessage(
                code: $"{propertyName}.{ValidationType.MaxLength}"  // Injeta sufixo!
            );
            // Resultado: "SimpleAggregateRoot.FirstName.MaxLength"

            return false;
        }

        return true;
    }
}
```

**Uso nas entidades**:

```csharp
public sealed class SimpleAggregateRoot
{
    public static bool ValidateFirstName(
        ExecutionContext executionContext,
        string? firstName
    )
    {
        // propertyName é gerado UMA vez - sufixo será injetado pelo ValidationUtils
        string propertyName = CreateMessageCode<SimpleAggregateRoot>(
            propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
        );
        // propertyName = "SimpleAggregateRoot.FirstName"

        // 1. Validação de obrigatoriedade
        // ValidationUtils injeta ".IsRequired" ? "SimpleAggregateRoot.FirstName.IsRequired"
        bool firstNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: propertyName,
            isRequired: SimpleAggregateRootMetadata.FirstNameIsRequired,
            value: firstName
        );

        // 2. Validação de tamanho mínimo
        // ValidationUtils injeta ".MinLength" ? "SimpleAggregateRoot.FirstName.MinLength"
        bool firstNameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: propertyName,
            minLength: SimpleAggregateRootMetadata.FirstNameMinLength,
            value: firstName?.Length ?? 0
        );

        // 3. Validação de tamanho máximo
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
}
```

### Benefícios de `ValidationUtils`

**1. Comportamento consistente**:

```csharp
// Toda entidade trata null e string vazia da mesma forma
ValidationUtils.ValidateIsRequired(context, code, true, null);     // false
ValidationUtils.ValidateIsRequired(context, code, true, "");       // false
ValidationUtils.ValidateIsRequired(context, code, true, "   ");    // true (whitespace é valor)
ValidationUtils.ValidateIsRequired(context, code, false, null);    // true (não obrigatório)
```

**2. Correções propagam automaticamente**:

```csharp
// Bug encontrado: precisamos também rejeitar strings com só whitespace
// Correção em UM lugar:
public static bool ValidateIsRequired<T>(...)
{
    if (value is string str && string.IsNullOrWhiteSpace(str))  // Corrigido!
    {
        ...
    }
}
// Todas as 50 entidades × 5 propriedades = 250 validações corrigidas automaticamente
```

**3. Mensagens padronizadas via código estruturado**:

```csharp
// Todas as mensagens seguem o padrão: {Entity}.{Property}.{Constraint}
"SimpleAggregateRoot.FirstName.IsRequired"
"SimpleAggregateRoot.FirstName.MinLength"
"SimpleAggregateRoot.FirstName.MaxLength"
"Product.Name.IsRequired"
"Product.Name.MinLength"

// Frontend/i18n pode mapear consistentemente
const translations = {
    "*.IsRequired": "Campo obrigatório",
    "*.MinLength": "Valor muito curto",
    "*.MaxLength": "Valor muito longo"
};
```

**4. Facilita i18n centralizado**:

```csharp
// ValidationUtils gera código padronizado que facilita lookup de traduções
public static bool ValidateIsRequired<TValue>(
    ExecutionContext executionContext,
    string propertyName,
    bool isRequired,
    TValue? value
)
{
    if (isRequired && (value is null || value.Equals(default(TValue))))
    {
        // Gera código: "SimpleAggregateRoot.FirstName.IsRequired"
        string messageCode = $"{propertyName}.{ValidationType.IsRequired}";

        // Lookup centralizado de tradução usando o código gerado
        string message = executionContext.GetLocalizedMessage(messageCode)
            ?? "Value is required";  // Fallback

        executionContext.AddErrorMessage(code: messageCode);
        return false;
    }

    return true;
}

// Arquivo de traduções pode usar wildcards no código estruturado
{
    "SimpleAggregateRoot.FirstName.IsRequired": "Primeiro nome é obrigatório",
    "*.*.IsRequired": "Campo obrigatório"  // Fallback genérico
}
```

### Pattern: `CreateMessageCode<T>`

Método helper para gerar o prefixo do código de mensagem (entidade + propriedade):

```csharp
public static string CreateMessageCode<T>(string propertyName)
    => $"{typeof(T).Name}.{propertyName}";

// Uso - com PropertyName da metadata
CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName)
// Resultado: "SimpleAggregateRoot.FirstName"

CreateMessageCode<Product>(propertyName: ProductMetadata.NamePropertyName)
// Resultado: "Product.Name"
```

**Fluxo completo**:

```csharp
// 1. Entidade gera o prefixo usando PropertyName da metadata
string propertyName = CreateMessageCode<SimpleAggregateRoot>(
    propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
);
// propertyName = "SimpleAggregateRoot.FirstName"

// 2. ValidationUtils recebe o prefixo e INJETA o sufixo
ValidationUtils.ValidateIsRequired(context, propertyName, true, null);
// Internamente: $"{propertyName}.{ValidationType.IsRequired}"
// Código final: "SimpleAggregateRoot.FirstName.IsRequired"
```

Benefícios:
- **Consistência**: Formato sempre `{Entity}.{Property}.{Constraint}`
- **Parseável**: Fácil quebrar em partes com `Split('.')` para agrupamento, filtragem ou i18n
- **Refactoring-safe**: Usa `PropertyName` da metadata (que usa `nameof()` internamente)
- **Rastreabilidade**: Código identifica origem exata do erro
- **i18n-friendly**: Código único para lookup de traduções
- **DRY**: Entidade não precisa repetir o tipo de validação - ValidationUtils sabe qual sufixo usar

### Validações Personalizadas

Para regras específicas de negócio que não são "padrão", a entidade implementa diretamente:

```csharp
public static bool ValidateBirthDate(
    ExecutionContext executionContext,
    BirthDate? birthDate
)
{
    // propertyName para validações padrão
    string propertyName = CreateMessageCode<SimpleAggregateRoot>(
        propertyName: SimpleAggregateRootMetadata.BirthDatePropertyName
    );
    // propertyName = "SimpleAggregateRoot.BirthDate"

    // Validação padrão: obrigatoriedade
    // ValidationUtils injeta ".IsRequired" ? "SimpleAggregateRoot.BirthDate.IsRequired"
    bool isRequiredValid = ValidationUtils.ValidateIsRequired(
        executionContext,
        propertyName: propertyName,
        isRequired: SimpleAggregateRootMetadata.BirthDateIsRequired,
        value: birthDate
    );

    if (!isRequiredValid)
        return false;

    // Validação CUSTOMIZADA: idade baseada em data de nascimento
    // Não existe ValidationUtils.ValidateAge - é específico desta entidade
    int ageInYears = birthDate!.Value.CalculateAgeInYears(executionContext.TimeProvider);

    // Para validações customizadas, a entidade gera o código completo
    if (ageInYears < SimpleAggregateRootMetadata.BirthDateMinAgeInYears)
    {
        executionContext.AddErrorMessage(
            code: $"{propertyName}.MinAgeInYears"  // Sufixo customizado
        );
        // Resultado: "SimpleAggregateRoot.BirthDate.MinAgeInYears"
    }

    if (ageInYears > SimpleAggregateRootMetadata.BirthDateMaxAgeInYears)
    {
        executionContext.AddErrorMessage(
            code: $"{propertyName}.MaxAgeInYears"  // Sufixo customizado
        );
        // Resultado: "SimpleAggregateRoot.BirthDate.MaxAgeInYears"
    }

    return isRequiredValid;  // Validações customizadas já adicionaram erros se necessário
}
```

## Consequências

### Benefícios

- **Consistência**: Todas as entidades validam da mesma forma
- **Manutenção centralizada**: Correções propagam automaticamente
- **Mensagens padronizadas**: Formato uniforme facilita i18n e frontend
- **Menos código**: Entidades focam em regras específicas, não em boilerplate
- **Testabilidade**: `ValidationUtils` pode ser testado isoladamente

### Trade-offs (Com Perspectiva)

- **Dependência de `ValidationUtils`**: Entidades dependem desta classe utilitária
- **Menos flexibilidade por validação**: Mensagens seguem padrão fixo

### Trade-offs Frequentemente Superestimados

**"Dependência de classe utilitária viola DDD"**

`ValidationUtils` é infraestrutura de **suporte ao domínio**, não uma dependência externa:

```csharp
// ValidationUtils está no mesmo assembly/camada que as entidades
// Não é um framework externo, é código do próprio domínio

namespace Domain.Validation
{
    public static class ValidationUtils { ... }
}

namespace Domain.Entities
{
    public sealed class Person
    {
        // Usa ValidationUtils do mesmo domínio
        public static bool ValidateFirstName(...)
        {
            return ValidationUtils.ValidateIsRequired(...);
        }
    }
}
```

É como usar `string.IsNullOrEmpty()` - uma utilidade que simplifica código, não uma violação arquitetural.

**"Perde flexibilidade de mensagens customizadas"**

O código de mensagem gerado permite customização total via i18n:

```csharp
// ValidationUtils gera código padronizado
string propertyName = CreateMessageCode<Person>(PersonMetadata.FirstNamePropertyName);
ValidationUtils.ValidateMinLength(context, propertyName, 3, firstName.Length);
// Código gerado: "Person.FirstName.MinLength"

// Sistema de i18n pode retornar mensagem totalmente customizada
{
    "Person.FirstName.MinLength": "O nome deve ter pelo menos {0} caracteres",
    "Product.Name.MinLength": "Nome do produto: mínimo {0} chars",
    "*.*.MinLength": "Valor muito curto"  // Fallback genérico
}
```

A mensagem padrão é fallback; o código permite qualquer customização.

**"Validações simples não precisam de utilitário"**

O valor de `ValidationUtils` cresce com a escala:

```csharp
// 1 entidade × 3 propriedades = pouca diferença
// 50 entidades × 5 propriedades = 250 validações padronizadas

// Custo de bug em validação inline:
// - Encontrar bug
// - Identificar todos os lugares com o mesmo código
// - Corrigir cada um manualmente
// - Testar cada correção
// - Esquecer de um lugar = bug em produção

// Custo de bug em ValidationUtils:
// - Encontrar bug
// - Corrigir em um lugar
// - Rodar testes
// - Todos os 250 usos corrigidos
```

## Fundamentação Teórica

### Padrões de Design Relacionados

**Template Method Pattern (variação)** - `ValidationUtils` fornece o "template" de como validar. Entidades preenchem os parâmetros específicos (código, limites, valor).

**Strategy Pattern (implícito)** - Cada método de `ValidationUtils` é uma "estratégia" de validação que pode ser composta nas entidades.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) discute **Building Blocks** do domínio:

> "The building blocks of a MODEL-DRIVEN DESIGN are a set of patterns that can be used in combination to express most models."
>
> *Os blocos de construção de um DESIGN ORIENTADO A MODELO são um conjunto de padrões que podem ser usados em combinação para expressar a maioria dos modelos.*

`ValidationUtils` é um building block que expressa regras de validação comuns de forma reutilizável.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre validação:

> "Validation is an essential part of protecting the invariants of an Entity or Value Object."
>
> *Validação é uma parte essencial da proteção das invariantes de uma Entity ou Value Object.*

`ValidationUtils` fornece as ferramentas para essa proteção de forma consistente.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende **DRY (Don't Repeat Yourself)**:

> "Every piece of knowledge must have a single, unambiguous, authoritative representation within a system."
>
> *Cada pedaço de conhecimento deve ter uma única, não-ambígua, representação autoritativa dentro de um sistema.*

A lógica de "verificar se valor obrigatório foi fornecido" é um pedaço de conhecimento. `ValidationUtils.ValidateIsRequired` é sua representação única.

O princípio de **funções pequenas e focadas**:

> "Functions should do one thing. They should do it well. They should do it only."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso.*

`ValidateIsRequired` faz uma coisa: verifica obrigatoriedade. `ValidateMinLength` faz uma coisa: verifica mínimo. Composição nas entidades cria validações complexas a partir de blocos simples.

### O Que o Clean Architecture Diz

Clean Architecture enfatiza que **código duplicado deve ser extraído para abstrações reutilizáveis**.

`ValidationUtils` extrai a duplicação de validações básicas, mantendo as entidades focadas em suas regras de negócio específicas.

### Outros Fundamentos

**SOLID - Single Responsibility Principle (SRP)**:

`ValidationUtils` tem uma responsabilidade: executar validações padrão. Entidades têm sua responsabilidade: definir quais validações aplicar com quais parâmetros.

**Código Defensivo**:

Centralizar validações permite implementar verificações defensivas em um lugar:

```csharp
public static bool ValidateMinLength<TValue>(
    ExecutionContext executionContext,
    string propertyName,
    TValue minLength,
    TValue? value
) where TValue : IComparable<TValue>
{
    // Verificação defensiva - centralizada
    if (string.IsNullOrEmpty(propertyName))
        throw new ArgumentNullException(nameof(propertyName));

    if (value is null)
        return true;

    if (value.CompareTo(minLength) < 0)
    {
        executionContext.AddErrorMessage(
            code: $"{propertyName}.{ValidationType.MinLength}"
        );
        return false;
    }

    return true;
}
```

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que centralizar validações melhora manutenção?"
- "Como implementar i18n em mensagens de validação?"
- "Qual a diferença entre validação de domínio e validação de apresentação?"
- "Como compor validações simples em validações complexas?"

### Leitura Recomendada

- [Domain-Driven Design - Building Blocks](https://martinfowler.com/bliki/DDD_Aggregate.html)
- [Clean Code - Chapter 17: Smells and Heuristics (DRY)](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)
- [Validation in Domain-Driven Design](https://enterprisecraftsmanship.com/posts/validation-and-ddd/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Implementa os métodos de validação padronizados (ValidateIsRequired, ValidateMinLength, ValidateMaxLength, etc.) que são usados pelas entidades |
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Recebe as mensagens de validação geradas pelos métodos ValidationUtils |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Usar ValidationUtils Para Validações Padrão
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Usar CreateMessageCode<T>
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_TEMPLATE: Padrão de Validação de Propriedade
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ValidateFirstName usando ValidationUtils
