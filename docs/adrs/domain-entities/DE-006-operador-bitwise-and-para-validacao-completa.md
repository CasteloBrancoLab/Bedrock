# DE-006: Operador Bitwise AND para Validação Completa

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você está preenchendo um formulário de imposto de renda com 50 campos. Você preenche tudo, clica em "Enviar", e recebe: "Erro: CPF inválido". Você corrige, envia novamente: "Erro: Data de nascimento inválida". Corrige, envia: "Erro: CEP não encontrado". Após 10 tentativas frustradas, você finalmente consegue.

Agora imagine o mesmo formulário que, ao clicar em "Enviar", mostra: "Encontramos 3 problemas: CPF inválido, Data de nascimento inválida, CEP não encontrado". Você corrige tudo de uma vez e envia com sucesso.

A diferença entre essas duas experiências é exatamente a diferença entre usar `&&` (logical AND) e `&` (bitwise AND) em validações.

### O Problema Técnico

Em C#, `&&` usa **short-circuit evaluation**: se a primeira condição for `false`, as demais nem são avaliadas:

```csharp
// Com && (logical AND) - short-circuit
bool isValid =
    ValidateFirstName(context, firstName)   // Falha ? retorna false
    && ValidateLastName(context, lastName)  // NUNCA EXECUTA
    && ValidateEmail(context, email);       // NUNCA EXECUTA

// context.Messages contém apenas 1 erro
// Usuário precisa corrigir, reenviar, descobrir próximo erro...
```

O `&` (bitwise AND) **sempre avalia todos os operandos**:

```csharp
// Com & (bitwise AND) - avalia tudo
bool isValid =
    ValidateFirstName(context, firstName)   // Falha ? adiciona mensagem
    & ValidateLastName(context, lastName)   // Executa ? adiciona mensagem se falhar
    & ValidateEmail(context, email);        // Executa ? adiciona mensagem se falhar

// context.Messages contém TODOS os erros
// Usuário corrige tudo de uma vez
```

## Como Normalmente é Feito

### Abordagem Tradicional

A maioria dos projetos usa `&&` por ser o "padrão" para condições booleanas:

```csharp
public class PersonValidator
{
    public ValidationResult Validate(Person person)
    {
        var result = new ValidationResult();

        // Abordagem 1: if/else encadeados
        if (string.IsNullOrEmpty(person.FirstName))
        {
            result.AddError("FirstName é obrigatório");
        }
        else if (person.FirstName.Length < 3)  // Nunca executa se FirstName vazio
        {
            result.AddError("FirstName deve ter pelo menos 3 caracteres");
        }

        // Abordagem 2: && em condição
        if (!ValidateFirstName(person) && !ValidateLastName(person))  // Short-circuit!
        {
            // Só entra aqui se AMBOS falharem, mas ValidateLastName
            // não executa se ValidateFirstName falhar
        }

        return result;
    }
}
```

### Por Que Não Funciona Bem

1. **UX degradada**: Usuário descobre erros um por um, aumentando frustração e tempo de correção

2. **Mais roundtrips**: Em APIs, cada erro requer nova requisição:

```csharp
// Roundtrip 1
POST /api/persons ? 400 { error: "FirstName inválido" }

// Roundtrip 2 (após correção)
POST /api/persons ? 400 { error: "LastName inválido" }

// Roundtrip 3 (após correção)
POST /api/persons ? 400 { error: "Email inválido" }

// Roundtrip 4 (finalmente!)
POST /api/persons ? 201 Created
```

3. **Testes incompletos**: Desenvolvedores não percebem que validações posteriores nunca executam em cenários de falha

4. **Debug confuso**: Ao depurar, parece que "pulou" validações - porque realmente pulou

5. **Inconsistência com FluentValidation**: Bibliotecas como FluentValidation executam TODAS as regras por padrão, criando inconsistência quando misturado com `&&`

## A Decisão

### Nossa Abordagem

Métodos `*Internal` e validações compostas DEVEM usar `&` (bitwise AND):

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    private bool ChangeNameInternal(
        ExecutionContext executionContext,
        string firstName,
        string lastName
    )
    {
        string fullName = $"{firstName} {lastName}";

        // & garante que TODAS as validações executam
        bool isSuccess =
            SetFirstName(executionContext, firstName)
            & SetLastName(executionContext, lastName)
            & SetFullName(executionContext, fullName);

        return isSuccess;
    }

    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? firstName,
        string? lastName,
        BirthDate? birthDate
    )
    {
        // Todas as validações executam, todas as mensagens são coletadas
        return
            ValidateEntityInfo(executionContext, entityInfo)
            & ValidateFirstName(executionContext, firstName)
            & ValidateLastName(executionContext, lastName)
            & ValidateBirthDate(executionContext, birthDate);
    }
}
```

**Uso em Controller (UI)**:

```csharp
[HttpPost]
public IActionResult Create(CreatePersonRequest request)
{
    var context = new ExecutionContext();

    // & retorna TODOS os erros para o usuário
    bool isValid =
        SimpleAggregateRoot.ValidateFirstName(context, request.FirstName)
        & SimpleAggregateRoot.ValidateLastName(context, request.LastName)
        & SimpleAggregateRoot.ValidateEmail(context, request.Email);

    if (!isValid)
    {
        // context.Messages contém TODOS os erros
        return BadRequest(context.Messages);
    }

    // Continuar com criação...
}
```

### Por Que é Seguro Usar `&` com Booleanos

Muitos desenvolvedores evitam `&` por medo de "comportamento inesperado". Mas com booleanos, `&` é perfeitamente seguro:

```csharp
// Com booleanos, & e && diferem APENAS no short-circuit
true & true   == true   // Mesmo que true && true
true & false  == false  // Mesmo que true && false
false & true  == false  // Diferença: com && o segundo não executa
false & false == false  // Diferença: com && o segundo não executa
```

A única diferença é que `&` sempre avalia ambos os lados - exatamente o que queremos para validação completa.

### Quando Usar `&&` (Short-Circuit)

Short-circuit é apropriado quando:

1. **Dependência entre validações**: Segunda validação só faz sentido se primeira passar

```csharp
// Correto usar && aqui: não faz sentido validar formato se for null
if (email != null && IsValidEmailFormat(email))
```

2. **Performance crítica**: Validação posterior é cara e primeira já falhou

```csharp
// Correto usar && aqui: não consultar banco se validação básica falhou
if (IsValidFormat(cpf) && await ExistsInDatabase(cpf))
```

3. **Input sistêmico**: Processamento de filas onde fail-fast é aceitável

```csharp
// Consumer de fila: fail-fast é OK
if (!ValidateFirstName(context, message.FirstName))
{
    _logger.LogWarning("Mensagem rejeitada: {Messages}", context.Messages);
    return;
}
```

### Padrão de Decisão

| Contexto | Operador | Razão |
|----------|----------|-------|
| Input de usuário (UI, API) | `&` | UX: mostrar todos os erros |
| Métodos `*Internal` | `&` | Consistência e feedback completo |
| Validação com dependência | `&&` | Segunda depende da primeira |
| Consumer de fila | `&&` | Fail-fast é aceitável |
| Validação cara (I/O) | `&&` | Evitar I/O desnecessário |

## Consequências

### Benefícios

- **UX superior**: Usuários corrigem todos os erros de uma vez
- **Menos roundtrips**: APIs retornam lista completa de erros
- **Debug previsível**: Todas as validações sempre executam
- **Consistência**: Mesmo comportamento que FluentValidation e similares
- **Testes confiáveis**: Todas as validações são exercitadas

### Trade-offs (Com Perspectiva)

- **Execução "desnecessária"**: Validações posteriores executam mesmo se anteriores falharam
- **Curva de aprendizado**: Desenvolvedores precisam entender a diferença entre `&` e `&&`

### Trade-offs Frequentemente Superestimados

**"Executar validações após falha é desperdício"**

Na prática, validações são operações extremamente baratas:

```csharp
// Custo de 3 validações de string (~nanosegundos):
bool isValid =
    ValidateFirstName(context, firstName)   // ~50ns
    & ValidateLastName(context, lastName)   // ~50ns
    & ValidateEmail(context, email);        // ~100ns
// Total: ~200ns

// Custo de 1 roundtrip HTTP adicional (~milissegundos):
// POST /api/persons ? 400 ? correção ? POST novamente
// Total: ~100-500ms (500.000x mais lento)
```

O "desperdício" de executar 2 validações extras é irrisório comparado ao custo de um roundtrip adicional.

**"& é confuso e não-idiomático"**

O uso de `&` para validação completa é um padrão estabelecido:
- FluentValidation usa internamente
- ASP.NET Model Binding valida todos os campos
- Formulários HTML5 mostram todos os erros

O que é realmente confuso é o comportamento inconsistente: "às vezes mostra todos os erros, às vezes só um".

**"Pode causar NullReferenceException"**

Isso é mito quando os métodos são projetados corretamente:

```csharp
// Método Validate* trata null internamente
public static bool ValidateFirstName(ExecutionContext context, string? firstName)
{
    if (firstName == null)
    {
        if (FirstNameMetadata.IsRequired)
            context.AddMessage("FirstName é obrigatório");
        return !FirstNameMetadata.IsRequired;
    }

    // Validações de formato só executam se não for null
    // ...
}
```

Cada método `Validate*` é responsável por tratar `null` - não há risco de NullReferenceException.

## Fundamentação Teórica

### Padrões de Design Relacionados

**Notification Pattern** - Ao invés de lançar exceção na primeira falha, coletamos todas as falhas em um objeto de notificação (nosso `ExecutionContext`). O operador `&` é essencial para esse padrão funcionar.

**Fail-Fast vs Fail-Safe** - Nossa abordagem é "fail-complete": falha rápido (na validação), mas com informação completa. É o melhor dos dois mundos.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) discute **Specification Pattern** para validações compostas:

> "A SPECIFICATION is a predicate that determines if an object does or does not satisfy some criteria."
>
> *Uma SPECIFICATION é um predicado que determina se um objeto satisfaz ou não algum critério.*

Specifications são tipicamente compostas com AND, OR, NOT. Nosso uso de `&` implementa a composição AND de forma que todas as specifications são avaliadas.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) enfatiza feedback rico para o usuário:

> "The Application Service should translate any domain errors into a form that the user interface can display to the user in a meaningful way."
>
> *O Application Service deve traduzir quaisquer erros de domínio em uma forma que a interface do usuário possa exibir para o usuário de maneira significativa.*

Mostrar apenas um erro por vez não é "meaningful" - o usuário não sabe quantos problemas ainda existem.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende o **Principle of Least Surprise** (Princípio da Menor Surpresa):

> "Functions should do what their name suggests, and no more."
>
> *Funções devem fazer o que seu nome sugere, e nada mais.*

Um método chamado `IsValid` sugere que verificará se algo é válido - **completamente**. Parar na primeira falha é surpreendente e inconsistente com a expectativa do nome.

O princípio **"Error Handling Is One Thing"** (Tratamento de Erro é Uma Coisa Só) também se aplica:

> "Functions should do one thing. Error handling is one thing."
>
> *Funções devem fazer uma coisa só. Tratamento de erro é uma coisa só.*

Se validação é "uma coisa", então deve ser feita completamente, não parcialmente.

### O Que o Clean Architecture Diz

Clean Architecture coloca **Presenters** como responsáveis por formatar dados para a UI. Para formatar bem, o Presenter precisa de dados completos.

Se a camada de domínio retorna apenas um erro, o Presenter não tem como mostrar uma experiência melhor. A completude da validação deve vir do domínio.

### Outros Fundamentos

**UX Research** - Estudos de usabilidade mostram que formulários com validação completa têm:
- Taxa de conclusão mais alta
- Menor tempo total de preenchimento
- Menor frustração do usuário
- Menos abandono

**Nielsen Norman Group** sobre validação de formulários:

> "Show all errors at once. Revealing errors one at a time forces users to resubmit the form multiple times."
>
> *Mostre todos os erros de uma vez. Revelar erros um por vez força usuários a reenviar o formulário múltiplas vezes.*

**FluentValidation** (biblioteca popular de validação .NET):

A biblioteca executa todas as regras por padrão, coletando todas as falhas. Nosso uso de `&` alinha com esse comportamento padrão da indústria.

**HTML5 Form Validation**:

Navegadores modernos mostram todos os erros de validação de formulário simultaneamente, não um por vez. Este é o comportamento que usuários esperam.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre operadores bitwise e logical em C#?"
- "O que é short-circuit evaluation e quando é desejável?"
- "Como o Notification Pattern funciona para validação?"
- "Por que FluentValidation executa todas as regras por padrão?"

### Leitura Recomendada

- [C# Operators - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators)
- [Notification Pattern - Martin Fowler](https://martinfowler.com/eaaDev/Notification.html)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Nielsen Norman Group - Form Validation](https://www.nngroup.com/articles/errors-forms-design-guidelines/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Fornece métodos de validação padronizados que devem ser combinados com operador & para coletar todos os erros |
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Coleta todas as mensagens de validação quando operador & é usado, permitindo feedback completo ao usuário |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Usar Operador & em Métodos *Internal
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ChangeNameInternal usando &
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Estratégia de Validação Depende da Origem
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_TEMPLATE: Validação em Controller vs Consumer
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - método IsValid estático usando &
