# DE-025: Variáveis Intermediárias para Legibilidade e Debug

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **inspetor de qualidade** em uma linha de produção industrial. Ao final da linha, o produto pode ser aprovado ou rejeitado.

**Inspeção direta (sem checkpoints intermediários)**:
- Produto chega ao final
- Inspetor verifica tudo de uma vez
- Se rejeitar, não sabe qual etapa falhou
- Para investigar, precisa refazer todo o processo

**Inspeção com checkpoints (medições intermediárias)**:
- Produto passa por 5 checkpoints
- Cada checkpoint registra resultado específico
- Ao final, sabe exatamente: "Checkpoint 3 falhou - pintura irregular"
- Pode ir direto ao problema, sem refazer tudo

Em código de validação, **variáveis intermediárias** são esses checkpoints. Cada resultado é capturado, nomeado e pode ser inspecionado. Expressões inline são como inspeção ao final - funcionam, mas dificultam debug e manutenção.

---

### O Problema Técnico

Validações complexas podem ser escritas de duas formas:

**Expressão inline (tudo em uma linha)**:
```csharp
// ❌ Difícil de debugar, difícil de ler
return
    ValidationUtils.ValidateIsRequired(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameIsRequired, firstName)
    && ValidationUtils.ValidateMinLength(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameMinLength, firstName!.Length)
    && ValidationUtils.ValidateMaxLength(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameMaxLength, firstName!.Length);

// Problemas:
// 1. Não dá para colocar breakpoint em cada validação
// 2. Não dá para inspecionar resultado individual
// 3. Linha com 150+ caracteres - ilegível
// 4. Difícil identificar qual validação falhou
```

**Variáveis intermediárias (cada resultado nomeado)**:
```csharp
// ✅ Fácil de debugar, fácil de ler
bool firstNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
    ctx,
    CreateMessageCode<T>("FirstName"),
    Metadata.FirstNameIsRequired,
    firstName
);

bool firstNameMinLengthValidation = ValidationUtils.ValidateMinLength(
    ctx,
    CreateMessageCode<T>("FirstName"),
    Metadata.FirstNameMinLength,
    firstName!.Length
);

bool firstNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
    ctx,
    CreateMessageCode<T>("FirstName"),
    Metadata.FirstNameMaxLength,
    firstName!.Length
);

return firstNameIsRequiredValidation
    && firstNameMinLengthValidation
    && firstNameMaxLengthValidation;

// Benefícios:
// ✅ Breakpoint em cada validação
// ✅ Inspecionar cada resultado no debugger
// ✅ Nome descritivo indica o que está sendo testado
// ✅ Legibilidade muito superior
```

## Como Normalmente é Feito

### Abordagem Tradicional

A maioria dos projetos usa expressões inline por considerarem "mais conciso":

```csharp
// Abordagem 1: Tudo inline (comum em projetos C#)
public bool Validate(string name, string email, int age)
{
    return !string.IsNullOrEmpty(name) && name.Length >= 3 && name.Length <= 100
        && !string.IsNullOrEmpty(email) && Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")
        && age >= 18 && age <= 150;
}

// Abordagem 2: if/else encadeados (comum em código legado)
public bool Validate(string name, string email, int age)
{
    if (string.IsNullOrEmpty(name)) return false;
    if (name.Length < 3) return false;
    if (name.Length > 100) return false;
    if (string.IsNullOrEmpty(email)) return false;
    if (!Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")) return false;
    if (age < 18) return false;
    if (age > 150) return false;
    return true;
}

// Abordagem 3: Guard clauses (comum em validation frameworks)
public void Validate(string name, string email, int age)
{
    Guard.Against.NullOrEmpty(name, nameof(name));
    Guard.Against.LengthOutOfRange(name, nameof(name), 3, 100);
    Guard.Against.NullOrEmpty(email, nameof(email));
    Guard.Against.InvalidFormat(email, nameof(email), emailPattern);
    Guard.Against.OutOfRange(age, nameof(age), 18, 150);
}
```

### Por Que Não Funciona Bem

**Expressões inline**:
1. **Debug difícil**: Qual parte da expressão falhou? Impossível saber sem refatorar
2. **Breakpoints limitados**: Só pode colocar breakpoint na linha inteira
3. **Inspeção impossível**: Não dá para ver resultado de cada validação no debugger
4. **Legibilidade ruim**: Linhas com 100+ caracteres, múltiplas chamadas aninhadas

**if/else encadeados**:
1. **Short-circuit implícito**: Para na primeira falha (problema resolvido por DE-006)
2. **Verbosidade excessiva**: Muito código para lógica simples
3. **Falta de composição**: Difícil combinar resultados

**Guard clauses**:
1. **Exceções para validação de negócio**: Lança exceções (problema discutido em DE-008)
2. **Fail-fast forçado**: Para na primeira falha
3. **Sem composição**: Não retorna bool combinável

## A Decisão

### Nossa Abordagem

**Sempre use variáveis intermediárias com nomes descritivos** para armazenar resultados de validação:

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    public static bool ValidateFirstName(
        ExecutionContext executionContext,
        string? firstName
    )
    {
        // -----------------------------------------------------------------------
        // VARIÁVEIS INTERMEDIÁRIAS - Nome descritivo para cada validação
        // -----------------------------------------------------------------------

        bool firstNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(
                propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName
            ),
            isRequired: SimpleAggregateRootMetadata.FirstNameIsRequired,
            value: firstName
        );

        // Early return se obrigatório e ausente
        if (!firstNameIsRequiredValidation)
            return false;

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

        // -----------------------------------------------------------------------
        // COMPOSIÇÃO CLARA - && combina resultados já computados
        // -----------------------------------------------------------------------

        // Aqui usamos && porque TODAS as validações já executaram acima
        // Não é short-circuit durante execução - é apenas combinação lógica final
        return firstNameIsRequiredValidation
            && firstNameMinLengthValidation
            && firstNameMaxLengthValidation;
    }
}
```

### Convenção de Nomenclatura

Variáveis intermediárias DEVEM seguir o padrão:

```
<propertyName><ValidationRule>Validation
```

**Exemplos**:
```csharp
// ✅ CORRETO - Nome descritivo e consistente
bool firstNameIsRequiredValidation = ValidateIsRequired(...);
bool firstNameMinLengthValidation = ValidateMinLength(...);
bool firstNameMaxLengthValidation = ValidateMaxLength(...);
bool birthDateMinAgeValidation = ValidateMinAge(...);
bool emailPatternValidation = ValidatePattern(...);

// ❌ ERRADO - Nomes genéricos ou abreviados
bool isValid = ValidateIsRequired(...);
bool valid1 = ValidateMinLength(...);
bool fnMinLen = ValidateMinLength(...);
bool x = ValidateMaxLength(...);
```

**Benefícios da convenção**:
- **Consistência**: Todos os métodos seguem o mesmo padrão
- **Legibilidade**: Nome revela exatamente o que está sendo validado
- **Searchability**: Fácil encontrar todas as validações de uma propriedade
- **LLM-friendly**: LLMs podem gerar código seguindo o padrão consistentemente

### Uso em Métodos *Internal

Métodos `*Internal` também usam variáveis intermediárias ao combinar validações:

```csharp
private bool ChangeNameInternal(
    ExecutionContext executionContext,
    string firstName,
    string lastName
)
{
    string fullName = $"{firstName} {lastName}";

    // Variável intermediária para resultado composto
    bool isSuccess =
        SetFirstName(executionContext, firstName)
        & SetLastName(executionContext, lastName)
        & SetFullName(executionContext, fullName);

    return isSuccess;
}
```

**Por que `isSuccess` aqui?**
- **Breakpoint**: Pode parar e inspecionar resultado antes do return
- **Debug**: Ver no debugger se validação passou ou falhou
- **Análise estática**: Roslyn/SonarQube rastreiam uso da variável
- **Consistência**: Mesmo padrão em todo o código

### Contraste: Inline vs Variáveis Intermediárias

```csharp
// ❌ INLINE - Difícil de debugar
public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
{
    return ValidationUtils.ValidateIsRequired(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameIsRequired, firstName)
        && (!Metadata.FirstNameIsRequired || firstName != null)
        && ValidationUtils.ValidateMinLength(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameMinLength, firstName!.Length)
        && ValidationUtils.ValidateMaxLength(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameMaxLength, firstName!.Length);
}

// ✅ VARIÁVEIS INTERMEDIÁRIAS - Fácil de debugar
public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
{
    bool firstNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
        ctx,
        CreateMessageCode<T>("FirstName"),
        Metadata.FirstNameIsRequired,
        firstName
    );

    if (!firstNameIsRequiredValidation)
        return false;

    bool firstNameMinLengthValidation = ValidationUtils.ValidateMinLength(
        ctx,
        CreateMessageCode<T>("FirstName"),
        Metadata.FirstNameMinLength,
        firstName!.Length
    );

    bool firstNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
        ctx,
        CreateMessageCode<T>("FirstName"),
        Metadata.FirstNameMaxLength,
        firstName!.Length
    );

    return firstNameIsRequiredValidation
        && firstNameMinLengthValidation
        && firstNameMaxLengthValidation;
}
```

### Cenário de Debug Real

**Com inline** (difícil):
```
1. Coloca breakpoint na linha do return
2. Vê que retornou false
3. Não sabe qual validação falhou
4. Precisa adicionar variáveis intermediárias para investigar
5. Recompila, executa novamente
6. Finalmente descobre qual validação falhou
```

**Com variáveis intermediárias** (fácil):
```
1. Coloca breakpoint em qualquer validação de interesse
2. Executa e para na linha específica
3. Vê imediatamente qual validação falhou
4. Inspeciona parâmetros e resultado
5. Entende o problema na primeira execução
```

## Consequências

### Benefícios

1. **Debug superior**: Breakpoint em cada validação, inspeção de cada resultado
2. **Legibilidade**: Código se lê como narrativa - cada passo nomeado e claro
3. **Manutenção**: Adicionar/remover validações é trivial - uma linha por validação
4. **Análise estática**: Ferramentas como Roslyn e SonarQube rastreiam fluxo de dados
5. **Onboarding**: Novos desenvolvedores entendem facilmente o fluxo de validação
6. **Code review**: Revisar mudanças é fácil - diff mostra exatamente qual validação mudou
7. **Testabilidade**: Pode testar comportamento de cada validação isoladamente

### Trade-offs (Com Perspectiva)

- **Mais linhas de código**: Uma validação inline de 1 linha vira 6-8 linhas
  - **Mitigação**: Legibilidade e manutenibilidade valem muito mais que brevidade
  - **Perspectiva**: "Código é lido 10x mais vezes do que é escrito" - priorizar leitura

- **Variáveis "descartáveis"**: Variáveis usadas apenas uma vez no return
  - **Mitigação**: Não são descartáveis - são essenciais para debug e análise
  - **Perspectiva**: Em modo Release, o compilador otimiza variáveis intermediárias (zero overhead)

### Trade-offs Frequentemente Superestimados

**"Variáveis intermediárias causam overhead de performance"**

Mito. O compilador C# otimiza:

```csharp
// Código escrito (com variáveis intermediárias):
bool isRequiredValidation = ValidateIsRequired(...);
bool minLengthValidation = ValidateMinLength(...);
return isRequiredValidation && minLengthValidation;

// Código compilado em Release (otimizado):
return ValidateIsRequired(...) && ValidateMinLength(...);
// As variáveis intermediárias desaparecem completamente
```

Performance é **idêntica** entre inline e variáveis intermediárias após otimização do compilador.

**"Torna o código muito verboso"**

Verbosidade tem propósito:

```csharp
// "Conciso" mas ilegível:
return a && b && c && d && e && f;

// "Verboso" mas claro:
bool nameIsValid = ValidateName(...);
bool emailIsValid = ValidateEmail(...);
bool ageIsValid = ValidateAge(...);
return nameIsValid && emailIsValid && ageIsValid;
```

**Clean Code**: "Código é escrito para humanos lerem, não para máquinas executarem."

**"Mais código = mais bugs"**

Mito. Bugs surgem de **complexidade**, não de linhas:

```csharp
// 1 linha - complexa, propensa a bugs:
return x != null && x.Length > 0 && int.Parse(x) >= 18 && int.Parse(x) <= 150 && regex.IsMatch(x);

// 5 linhas - simples, menos propensa a bugs:
bool isNotEmpty = x != null && x.Length > 0;
bool isParseable = int.TryParse(x, out int age);
bool isValidAge = age >= 18 && age <= 150;
bool matchesPattern = regex.IsMatch(x);
return isNotEmpty && isParseable && isValidAge && matchesPattern;
```

Variáveis intermediárias **reduzem** complexidade cognitiva.

## Fundamentação Teórica

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre nomes de variáveis:

> "The name of a variable, function, or class, should answer all the big questions. It should tell you why it exists, what it does, and how it is used."
>
> *O nome de uma variável, função ou classe deve responder a todas as grandes questões. Deve dizer por que existe, o que faz e como é usada.*

Variáveis intermediárias com nomes como `firstNameIsRequiredValidation` respondem perfeitamente:
- **Por que existe**: Para armazenar resultado da validação de obrigatoriedade do FirstName
- **O que faz**: Valida se FirstName atende a regra IsRequired
- **Como é usada**: Composta com outras validações para resultado final

Sobre legibilidade de expressões:

> "You should name a variable using the same care with which you name a first-born child."
>
> *Você deve nomear uma variável com o mesmo cuidado que nomeia um filho primogênito.*

E sobre clareza:

> "Indeed, the ratio of time spent reading versus writing is well over 10 to 1. We are constantly reading old code as part of the effort to write new code. [...] Therefore, making it easy to read makes it easier to write."
>
> *De fato, a proporção de tempo gasto lendo versus escrevendo é bem acima de 10 para 1. Estamos constantemente lendo código antigo como parte do esforço de escrever código novo. [...] Portanto, tornar fácil de ler torna mais fácil de escrever.*

### O Que o Code Complete Diz

Steve McConnell em "Code Complete, 2nd Edition" (2004) sobre variáveis intermediárias:

> "Use intermediate variables to clarify complicated expressions. [...] Breaking a complicated calculation into intermediate steps can clarify the calculation and help to document it."
>
> *Use variáveis intermediárias para clarificar expressões complicadas. [...] Quebrar um cálculo complicado em passos intermediários pode clarificar o cálculo e ajudar a documentá-lo.*

Sobre debugging:

> "Good variable names are a key element of program self-documentation. [...] They improve readability and make debugging easier."
>
> *Bons nomes de variáveis são um elemento-chave da auto-documentação do programa. [...] Eles melhoram a legibilidade e tornam o debug mais fácil.*

### O Que o Refactoring Diz

Martin Fowler em "Refactoring: Improving the Design of Existing Code" (2019) descreve o refactoring **Extract Variable**:

> "Extract Variable: Take a complicated expression and put the result or part of the result in a temporary variable with a name that explains its purpose."
>
> *Extrair Variável: Pegue uma expressão complicada e coloque o resultado ou parte do resultado em uma variável temporária com um nome que explique seu propósito.*

E explica quando aplicar:

> "I consider extracting a variable when an expression is hard to read. [...] The main motivation for Extract Variable is to give names to parts of a more complex piece of logic, so readers can see what each part is doing."
>
> *Eu considero extrair uma variável quando uma expressão é difícil de ler. [...] A principal motivação para Extrair Variável é dar nomes às partes de uma lógica mais complexa, para que leitores possam ver o que cada parte está fazendo.*

### O Que o Pragmatic Programmer Diz

Andy Hunt e Dave Thomas em "The Pragmatic Programmer, 20th Anniversary Edition" (2019):

> "Remember, code is read far more often than it is written. Optimizing for readability is almost always the right choice."
>
> *Lembre-se, código é lido muito mais frequentemente do que é escrito. Otimizar para legibilidade é quase sempre a escolha certa.*

Sobre debug:

> "When debugging, be systematic. Don't guess. Collect data, form hypotheses, test them. [...] Good variable names make it easier to collect data and form hypotheses."
>
> *Ao debugar, seja sistemático. Não adivinhe. Colete dados, forme hipóteses, teste-as. [...] Bons nomes de variáveis tornam mais fácil coletar dados e formar hipóteses.*

## Antipadrões Documentados

### Antipadrão 1: Expressões Inline Complexas

```csharp
// ❌ Ilegível, impossível debugar cada parte
public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
{
    return ValidationUtils.ValidateIsRequired(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameIsRequired, firstName)
        && ValidationUtils.ValidateMinLength(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameMinLength, firstName!.Length)
        && ValidationUtils.ValidateMaxLength(ctx, CreateMessageCode<T>("FirstName"), Metadata.FirstNameMaxLength, firstName!.Length);
}

// Problemas:
// 1. Uma linha com 150+ caracteres
// 2. Não dá para breakpoint em validação específica
// 3. Não dá para inspecionar resultado individual
// 4. Difícil entender qual validação falhou
```

### Antipadrão 2: Nomes de Variáveis Genéricos

```csharp
// ❌ Nomes genéricos não comunicam intenção
public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
{
    bool isValid = ValidateIsRequired(ctx, ...);
    bool valid1 = ValidateMinLength(ctx, ...);
    bool valid2 = ValidateMaxLength(ctx, ...);

    return isValid && valid1 && valid2;
}

// Problemas:
// 1. isValid, valid1, valid2 não dizem QUAL validação
// 2. Requer olhar chamada do método para entender
// 3. Inconsistente entre diferentes propriedades
```

### Antipadrão 3: Composição Inline com Operador &

```csharp
// ❌ Perde benefício de variáveis intermediárias
private bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName)
{
    // Variável intermediária, mas operador & inline - não dá para inspecionar cada Set*
    bool isSuccess = SetFirstName(ctx, firstName) & SetLastName(ctx, lastName) & SetFullName(ctx, fullName);
    return isSuccess;
}

// Problema: Não consegue ver resultado individual de cada SetFirstName, SetLastName, SetFullName
```

**Solução** - Quando precisa debugar cada Set* individualmente:

```csharp
// ✅ Variável para cada operação quando debug detalhado é necessário
private bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName)
{
    string fullName = $"{firstName} {lastName}";

    bool firstNameIsValid = SetFirstName(ctx, firstName);
    bool lastNameIsValid = SetLastName(ctx, lastName);
    bool fullNameIsValid = SetFullName(ctx, fullName);

    // Agora pode inspecionar cada resultado individualmente
    return firstNameIsValid & lastNameIsValid & fullNameIsValid;
}
```

**Nota**: Na prática, `isSuccess` como usado no código de produção é suficiente para a maioria dos casos. Expandir em múltiplas variáveis é útil em debug ativo.

### Antipadrão 4: Retornar Expressão Diretamente

```csharp
// ❌ Não permite breakpoint antes do return
private bool SetFirstName(ExecutionContext ctx, string firstName)
{
    bool isValid = ValidateFirstName(ctx, firstName);

    if (!isValid)
        return false;

    FirstName = firstName;

    // Não tem como colocar breakpoint aqui para ver que vai retornar true
    return true;
}
```

**Solução** (já usada no código):

```csharp
// ✅ Permite breakpoint e inspeção antes do return
private bool SetFirstName(ExecutionContext ctx, string firstName)
{
    bool isValid = ValidateFirstName(ctx, firstName);

    if (!isValid)
        return false;

    FirstName = firstName;

    return true; // Breakpoint aqui mostra que validou e atribuiu com sucesso
}
```

## Decisões Relacionadas

- [DE-006](./DE-006-operador-bitwise-and-para-validacao-completa.md) - Operador & para validação completa (composição das variáveis intermediárias)
- [DE-009](./DE-009-metodos-validate-publicos-e-estaticos.md) - Métodos Validate* públicos e estáticos (que usam variáveis intermediárias)
- [DE-010](./DE-010-validationutils-para-validacoes-padrao.md) - ValidationUtils (chamadas armazenadas em variáveis intermediárias)
- [DE-021](./DE-021-metodos-publicos-vs-metodos-internos.md) - Métodos públicos vs *Internal (ambos usam variáveis intermediárias)

## Leitura Recomendada

- [Clean Code - Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
- [Code Complete - Steve McConnell](https://www.amazon.com/Code-Complete-Practical-Handbook-Construction/dp/0735619670)
- [Refactoring - Martin Fowler](https://martinfowler.com/books/refactoring.html) - Capítulo "Extract Variable"
- [The Pragmatic Programmer - Andy Hunt, Dave Thomas](https://pragprog.com/titles/tpp20/the-pragmatic-programmer-20th-anniversary-edition/)
- [C# Compiler Optimizations - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Métodos de validação retornam bool que devem ser armazenados em variáveis intermediárias para legibilidade e debug |
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Utilizado nos métodos de validação para coletar mensagens, facilitando debug com variáveis intermediárias |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Variáveis Intermediárias Para Legibilidade e Análise Estática
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ValidateFirstName - exemplo completo de uso
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ValidateLastName - exemplo consistente do padrão
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ChangeNameInternal - variável intermediária isSuccess
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Variável intermediária isSuccess facilita debug e análise estática
