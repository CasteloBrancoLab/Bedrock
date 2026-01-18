# DE-030: Message Codes com CreateMessageCode<T>

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **hospital** com dois sistemas de registro de ocorrências:

**Modelo "texto livre" (sem padronização)**:
- Médico A escreve: "Paciente com dor de cabeça"
- Médico B escreve: "Cefaleia"
- Médico C escreve: "Headache"
- Três formas de dizer a mesma coisa
- Impossível gerar relatórios, buscar padrões, traduzir

**Modelo "código CID" (padronizado)**:
- Todos usam: "R51" (código internacional para cefaleia)
- Sistema traduz R51 para o idioma do leitor
- Relatórios agregam todas as R51 automaticamente
- Busca por padrões é trivial

Códigos de mensagem padronizados (`SimpleAggregateRoot.FirstName`) são como códigos CID - identificadores únicos que permitem tradução, agregação e rastreabilidade.

---

### O Problema Técnico

Mensagens de validação sem códigos padronizados causam problemas:

```csharp
// ❌ ANTIPATTERN: Strings hardcoded para mensagens
public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
{
    if (string.IsNullOrWhiteSpace(firstName))
    {
        ctx.AddErrorMessage(
            code: "FIRST_NAME_REQUIRED",  // String hardcoded
            text: "First name is required"
        );
        return false;
    }

    if (firstName.Length < 2)
    {
        ctx.AddErrorMessage(
            code: "FIRST_NAME_TOO_SHORT",  // Outro formato
            text: "First name must be at least 2 characters"
        );
        return false;
    }

    return true;
}
```

**Problemas graves**:

1. **Inconsistência de formato**: Cada desenvolvedor inventa seu formato
   ```csharp
   "FIRST_NAME_REQUIRED"      // UPPER_SNAKE
   "firstName.required"       // camelCase.dot
   "FirstNameRequired"        // PascalCase
   "first-name-required"      // kebab-case

   // Qual é o padrão? Depende de quem escreveu
   ```

2. **Sem contexto de origem**: Qual entidade gerou o erro?
   ```csharp
   ctx.AddErrorMessage("FIRST_NAME_REQUIRED", ...);

   // FirstName de qual entidade?
   // Person? Customer? Employee? Order.ContactName?
   ```

3. **Difícil i18n (internacionalização)**:
   ```csharp
   // Como mapear "FIRST_NAME_REQUIRED" para traduções?
   // E se mudar o código? Quebra as traduções existentes
   ```

4. **Difícil agregar/analisar**:
   ```csharp
   // Quantos erros de FirstName tivemos este mês?
   // Precisa buscar por várias variações do código
   ```

5. **Typos causam bugs silenciosos**:
   ```csharp
   ctx.AddErrorMessage("FISRT_NAME_REQUIRED", ...);  // Typo!
   // Funciona, mas i18n não encontra tradução
   ```

---

### Como Normalmente é Feito (e Por Que Não é Ideal)

**Opção 1: Constantes string**
```csharp
// ⚠️ Constantes por propriedade
public static class MessageCodes
{
    public const string FirstNameRequired = "FIRST_NAME_REQUIRED";
    public const string FirstNameMinLength = "FIRST_NAME_MIN_LENGTH";
    public const string FirstNameMaxLength = "FIRST_NAME_MAX_LENGTH";
    public const string LastNameRequired = "LAST_NAME_REQUIRED";
    // ... centenas de constantes
}
```

**Problemas**:
- Explosão de constantes (3+ por propriedade × N propriedades × M entidades)
- Ainda sem contexto de entidade
- Manutenção pesada

**Opção 2: Enums**
```csharp
// ⚠️ Enum com todos os códigos
public enum MessageCode
{
    FirstNameRequired,
    FirstNameMinLength,
    LastNameRequired,
    // ... centenas de valores
}
```

**Problemas**:
- Enum gigante
- Difícil descobrir quais códigos uma entidade usa
- Sem relação explícita entre entidade e mensagens

## A Decisão

### Nossa Abordagem

Usar `CreateMessageCode<T>(propertyName)` para gerar códigos no formato `{EntityName}.{PropertyName}`:

```csharp
public abstract class EntityBase
{
    // Método protegido genérico para criar códigos
    protected static string CreateMessageCode<TEntityType>(string propertyName)
    {
        return $"{typeof(TEntityType).Name}.{propertyName}";
    }

    // Método abstrato para código específico da entidade
    protected abstract string CreateMessageCode(string messageSuffix);
}

public sealed class EntityBase<TEntity> : EntityBase
{
    private static readonly Type _entityType = typeof(TEntity);

    protected override string CreateMessageCode(string messageSuffix)
    {
        return $"{_entityType.Name}.{messageSuffix}";
    }
}
```

### Como Usar

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    public static bool ValidateFirstName(
        ExecutionContext executionContext,
        string? firstName
    )
    {
        // ✅ Código gerado: "SimpleAggregateRoot.FirstName"
        bool firstNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(
                propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
            ),
            isRequired: SimpleAggregateRootMetadata.FirstNameIsRequired,
            value: firstName
        );

        if (!firstNameIsRequiredValidation)
            return false;

        // ✅ Mesmo código para min/max length
        bool firstNameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(
                propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
            ),
            minLength: SimpleAggregateRootMetadata.FirstNameMinLength,
            value: firstName!.Length
        );

        bool firstNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(
                propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
            ),
            maxLength: SimpleAggregateRootMetadata.FirstNameMaxLength,
            value: firstName!.Length
        );

        return firstNameMinLengthValidation & firstNameMaxLengthValidation;
    }
}
```

### Formato do Código

```
{EntityTypeName}.{PropertyName}

Exemplos:
- SimpleAggregateRoot.FirstName
- SimpleAggregateRoot.LastName
- SimpleAggregateRoot.BirthDate
- Order.CustomerName
- EntityBase.Id
- EntityBase.CreatedAt
```

### ValidationUtils Adiciona Sufixo de Tipo de Validação

O `ValidationUtils` adiciona automaticamente o tipo de validação ao código:

```csharp
// ValidationUtils.ValidateIsRequired adiciona ".IsRequired"
ValidationUtils.ValidateIsRequired(ctx, "SimpleAggregateRoot.FirstName", ...)
// → Mensagem com código: "SimpleAggregateRoot.FirstName.IsRequired"

// ValidationUtils.ValidateMinLength adiciona ".MinLength"
ValidationUtils.ValidateMinLength(ctx, "SimpleAggregateRoot.FirstName", ...)
// → Mensagem com código: "SimpleAggregateRoot.FirstName.MinLength"

// ValidationUtils.ValidateMaxLength adiciona ".MaxLength"
ValidationUtils.ValidateMaxLength(ctx, "SimpleAggregateRoot.FirstName", ...)
// → Mensagem com código: "SimpleAggregateRoot.FirstName.MaxLength"
```

### Código Final Completo

```
{EntityTypeName}.{PropertyName}.{ValidationType}

Exemplos:
- SimpleAggregateRoot.FirstName.IsRequired
- SimpleAggregateRoot.FirstName.MinLength
- SimpleAggregateRoot.FirstName.MaxLength
- Order.Total.MinValue
- Order.Total.MaxValue
```

### Benefícios

1. **Consistência garantida**: Formato sempre igual
   ```csharp
   // Todos os códigos seguem o padrão:
   // {Entity}.{Property}.{ValidationType}
   ```

2. **Contexto de origem**: Sabe-se exatamente qual entidade/propriedade
   ```csharp
   // "SimpleAggregateRoot.FirstName.IsRequired"
   // vs
   // "Order.CustomerName.IsRequired"
   // Claramente diferentes, mesmo sendo "nome obrigatório"
   ```

3. **Type-safe**: Usa `typeof(T).Name`, não strings
   ```csharp
   CreateMessageCode<SimpleAggregateRoot>(nameof(FirstName))
   // Se renomear a entidade/propriedade, compilador avisa
   ```

4. **Facilita i18n**:
   ```json
   {
     "SimpleAggregateRoot.FirstName.IsRequired": {
       "en": "First name is required",
       "pt-BR": "Nome é obrigatório",
       "es": "El nombre es obligatorio"
     }
   }
   ```

5. **Facilita análise/métricas**:
   ```sql
   -- Quantos erros de FirstName por entidade?
   SELECT
     SUBSTRING(MessageCode, 1, CHARINDEX('.', MessageCode) - 1) AS Entity,
     COUNT(*) AS ErrorCount
   FROM ValidationErrors
   WHERE MessageCode LIKE '%.FirstName.%'
   GROUP BY SUBSTRING(MessageCode, 1, CHARINDEX('.', MessageCode) - 1)
   ```

6. **Hierarquia natural**:
   ```
   SimpleAggregateRoot
   ├── FirstName
   │   ├── IsRequired
   │   ├── MinLength
   │   └── MaxLength
   ├── LastName
   │   ├── IsRequired
   │   ├── MinLength
   │   └── MaxLength
   └── BirthDate
       ├── IsRequired
       ├── MinValue
       └── MaxValue
   ```

### Usando com Metadados

Combina bem com metadados estáticos (DE-012, DE-013):

```csharp
public static class SimpleAggregateRootMetadata
{
    // Nomes das propriedades como constantes
    public const string FirstNamePropertyName = nameof(SimpleAggregateRoot.FirstName);
    public const string LastNamePropertyName = nameof(SimpleAggregateRoot.LastName);
    public const string BirthDatePropertyName = nameof(SimpleAggregateRoot.BirthDate);

    // Regras de validação
    public const bool FirstNameIsRequired = true;
    public const int FirstNameMinLength = 2;
    public const int FirstNameMaxLength = 100;
}

// Uso consistente
CreateMessageCode<SimpleAggregateRoot>(
    propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
)
// Resultado: "SimpleAggregateRoot.FirstName"
```

### Classe Base com CreateMessageCode Concreto

Para métodos de instância, a classe base fornece `CreateMessageCode`:

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // Dentro da entidade, pode usar a versão não-genérica
    private bool ValidateSomethingInternal(ExecutionContext ctx)
    {
        // CreateMessageCode herdado de EntityBase<SimpleAggregateRoot>
        var code = CreateMessageCode("CustomValidation");
        // Resultado: "SimpleAggregateRoot.CustomValidation"

        ctx.AddErrorMessage(code, "Custom validation failed");
        return false;
    }
}
```

### Trade-offs (Com Perspectiva)

- **Mais verboso**: `CreateMessageCode<T>(propertyName)` vs string literal
  - **Mitigação**: A verbosidade traz type-safety e consistência. IDE autocompleta.

- **Acoplamento com nome da classe**: Se renomear a entidade, códigos mudam
  - **Mitigação**: Isso é intencional! Se a entidade mudou de nome, as mensagens devem refletir isso. Migrações de i18n são necessárias.

### Trade-offs Frequentemente Superestimados

**"É muito código para uma mensagem"**

Compare:

```csharp
// String literal - menos código, mais problemas
ctx.AddErrorMessage("FIRST_NAME_REQUIRED", "First name required");

// CreateMessageCode - mais código, menos problemas
ValidationUtils.ValidateIsRequired(
    ctx,
    propertyName: CreateMessageCode<SimpleAggregateRoot>(
        propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
    ),
    isRequired: SimpleAggregateRootMetadata.FirstNameIsRequired,
    value: firstName
);
```

O segundo é mais código, mas:
- Type-safe (renomear propriedade = erro de compilação)
- Consistente (formato sempre igual)
- Single Source of Truth (metadados)
- Suporta i18n nativamente

**"Posso usar constantes string"**

Constantes são melhores que strings inline, mas:
- Ainda precisam ser mantidas manualmente
- Não têm relação automática com o tipo
- Permitem inconsistência de formato

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre Ubiquitous Language:

> "Use the model as the backbone of a language. [...] Use the same language in diagrams, writing, and especially speech."
>
> *Use o modelo como a espinha dorsal de uma linguagem. [...] Use a mesma linguagem em diagramas, escrita, e especialmente na fala.*

Códigos de mensagem que refletem a estrutura do domínio (`Entity.Property.Validation`) são parte da Ubiquitous Language - todos entendem do que se trata.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre nomes significativos:

> "Use Intention-Revealing Names. [...] The name of a variable, function, or class, should answer all the big questions."
>
> *Use Nomes que Revelam Intenção. [...] O nome de uma variável, função, ou classe, deve responder todas as grandes questões.*

`SimpleAggregateRoot.FirstName.IsRequired` revela:
- Qual entidade (SimpleAggregateRoot)
- Qual propriedade (FirstName)
- Qual validação falhou (IsRequired)

### Princípio DRY (Don't Repeat Yourself)

`CreateMessageCode<T>(propertyName)` evita repetição:

```csharp
// ❌ Repetição - nome da entidade em múltiplos lugares
"SimpleAggregateRoot.FirstName"
"SimpleAggregateRoot.LastName"
"SimpleAggregateRoot.BirthDate"

// ✅ DRY - nome vem do tipo
CreateMessageCode<SimpleAggregateRoot>(nameof(FirstName))
CreateMessageCode<SimpleAggregateRoot>(nameof(LastName))
CreateMessageCode<SimpleAggregateRoot>(nameof(BirthDate))

// Se renomear SimpleAggregateRoot → Person, código atualiza automaticamente
```

### Type Safety

Usar `typeof(T).Name` e `nameof()` dá segurança em tempo de compilação:

```csharp
// ❌ Typo não detectado
"SimpleAgregateRoot.FirstName"  // "Agregate" errado, compila OK

// ✅ Typo detectado
CreateMessageCode<SimpleAgregateRoot>(...)  // Erro de compilação!
```

## Antipadrões Documentados

### Antipadrão 1: Strings Hardcoded

```csharp
// ❌ String literal para código de mensagem
ctx.AddErrorMessage("FIRST_NAME_REQUIRED", "First name is required");

// Problemas:
// - Sem contexto de entidade
// - Typos não detectados
// - Formato inconsistente
```

### Antipadrão 2: Formato Inconsistente

```csharp
// ❌ Cada desenvolvedor usa um formato
ctx.AddErrorMessage("FIRST_NAME_REQUIRED", ...);  // UPPER_SNAKE
ctx.AddErrorMessage("lastName.required", ...);     // camelCase.dot
ctx.AddErrorMessage("BirthDateIsRequired", ...);   // PascalCase

// Impossível padronizar i18n ou métricas
```

### Antipadrão 3: Código Sem Contexto de Entidade

```csharp
// ❌ Código genérico sem saber a origem
ctx.AddErrorMessage("NAME_REQUIRED", "Name is required");

// Nome de qual entidade?
// Person? Customer? Product? Company?
```

### Antipadrão 4: Constantes em Classe Separada

```csharp
// ❌ Constantes desconectadas da entidade
public static class MessageCodes
{
    public const string PersonFirstNameRequired = "Person.FirstName.IsRequired";
    public const string CustomerFirstNameRequired = "Customer.FirstName.IsRequired";
    // Explosão combinatória...
}

// Problemas:
// - Manutenção manual
// - Fácil esquecer de criar constante
// - Nenhuma garantia de formato
```

### Antipadrão 5: Interpolação Manual

```csharp
// ❌ Interpolação manual do nome da entidade
ctx.AddErrorMessage(
    $"{nameof(SimpleAggregateRoot)}.{nameof(FirstName)}.IsRequired",
    "First name is required"
);

// Melhor que string literal, mas:
// - Repetitivo
// - Fácil errar o formato
// - Não usa o método padronizado
```

## Decisões Relacionadas

- [DE-009](./DE-009-metodos-validate-publicos-e-estaticos.md) - Métodos Validate* públicos e estáticos (usam CreateMessageCode)
- [DE-010](./DE-010-validationutils-para-validacoes-padrao.md) - ValidationUtils para validações padrão (recebe o código)
- [DE-012](./DE-012-metadados-estaticos-vs-data-annotations.md) - Metadados estáticos (PropertyName constantes)
- [DE-013](./DE-013-nomenclatura-de-metadados.md) - Nomenclatura de metadados (PropertyName + ConstraintType)

## Leitura Recomendada

- [Clean Code - Robert C. Martin](https://blog.cleancoder.com/)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Internationalization Best Practices](https://www.w3.org/International/articles/article-text-size)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Injeta sufixos padronizados nos códigos de mensagem criados por CreateMessageCode |
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Recebe mensagens com códigos gerados por CreateMessageCode, permitindo categorização e rastreamento |

## Referências no Código

- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - CreateMessageCode<TEntityType> estático
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - CreateMessageCode override em EntityBase<TEntity>
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Usar CreateMessageCode<T>
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ValidateFirstName usando CreateMessageCode
