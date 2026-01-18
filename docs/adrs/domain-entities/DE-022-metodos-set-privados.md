# DE-022: Métodos Set* Privados

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **cofre de banco**:

**Cenário 1 - Acesso direto ao cofre**:
Qualquer funcionário pode abrir o cofre, pegar dinheiro e colocar de volta. Resultado: impossível rastrear quem fez o quê, quando e por quê. Dinheiro pode "desaparecer" sem rastro.

**Cenário 2 - Acesso via caixa registradora**:
Todo acesso ao cofre passa pela caixa registradora que valida a operação, registra quem fez, e só então permite o acesso. Resultado: auditoria completa, impossível "esquecer" de registrar.

Em software, setters públicos são como acesso direto ao cofre - qualquer código pode modificar o estado sem validação ou auditoria.

---

### O Problema Técnico

Setters públicos permitem que qualquer código modifique o estado da entidade:

```csharp
// ❌ ANTIPATTERN: Setter público
public class Person
{
    public string FirstName { get; set; }  // Qualquer código pode modificar
    public string LastName { get; set; }
}

// Uso problemático
var person = new Person();
person.FirstName = null;           // ❌ Estado inválido
person.FirstName = "";             // ❌ String vazia
person.FirstName = new string('A', 1000);  // ❌ Muito longo
// A entidade está em estado inválido e ninguém percebeu
```

**Consequências**:
- Estado inválido pode existir em memória
- Validações precisam ser feitas em múltiplos lugares
- Impossível garantir invariantes de negócio
- Bugs difíceis de rastrear (quem modificou? quando?)
- Testes frágeis que dependem de ordem de chamadas

### Como Normalmente é Feito (e Por Que Não é Ideal)

```csharp
// ⚠️ COMUM: Property com validação no setter
public class Person
{
    private string _firstName;

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Nome é obrigatório");
            if (value.Length > 50)
                throw new ArgumentException("Nome muito longo");
            _firstName = value;
        }
    }
}
```

**Problemas**:
- Lança exceções para validação de negócio (custo de performance, stack trace desnecessário)
- Não coleta múltiplos erros (para no primeiro)
- Não tem acesso ao ExecutionContext para validação contextual
- Acoplamento: regras de validação duplicadas se usadas em outros lugares

## A Decisão

### Nossa Abordagem

**Métodos Set* privados** que validam antes de atribuir:

```csharp
public sealed class SimpleAggregateRoot
{
    // Propriedade com setter privado
    public string FirstName { get; private set; }

    // ✅ Método Set* privado que valida e atribui
    private bool SetFirstName(
        ExecutionContext executionContext,
        string firstName
    )
    {
        // 1. Valida usando método público de validação
        bool isValid = ValidateFirstName(
            executionContext,
            firstName
        );

        // 2. Se inválido, não atribui
        if (!isValid)
            return false;

        // 3. Atribui SOMENTE se válido
        FirstName = firstName;

        return true;
    }
}
```

### Estrutura do Método Set*

Cada método Set* segue este template:

```csharp
private bool Set<PropertyName>(
    ExecutionContext executionContext,
    <PropertyType> <propertyName>
)
{
    // PASSO 1: Validar usando método público Validate*
    bool isValid = Validate<PropertyName>(
        executionContext,
        <propertyName>
    );

    // PASSO 2: Early return se inválido
    if (!isValid)
        return false;

    // PASSO 3: Atribuir valor validado
    <PropertyName> = <propertyName>;

    // PASSO 4: Retornar sucesso
    return true;
}
```

### Responsabilidades Separadas

**Set* (ex: SetFirstName)**:
- Valida e atribui **UMA ÚNICA** propriedade
- Responsabilidade atômica e isolada
- Não conhece outras propriedades
- Usa método Validate* público para validação

**\*Internal (ex: ChangeNameInternal)**:
- Orquestra **MÚLTIPLOS** Set* em conjunto
- Combina resultados com operador `&`
- Representa operação de negócio completa
- Pode calcular valores derivados (ex: FullName)

```csharp
// *Internal orquestra múltiplos Set*
private bool ChangeNameInternal(
    ExecutionContext executionContext,
    string firstName,
    string lastName
)
{
    // Calcula valor derivado
    string fullName = $"{firstName} {lastName}";

    // Orquestra múltiplos Set* com operador &
    bool isSuccess =
        SetFirstName(executionContext, firstName)
        & SetLastName(executionContext, lastName)
        & SetFullName(executionContext, fullName);

    return isSuccess;
}
```

### Nullability dos Parâmetros

**Validate*** → Parâmetro **SEMPRE nullable** (regra IsRequired é dinâmica)
**Set*** → Parâmetro **SEGUE o tipo da propriedade**

```csharp
// Validate* - parâmetro nullable
public static bool ValidateFirstName(
    ExecutionContext executionContext,
    string? firstName  // ✅ Nullable porque IsRequired pode ser false
)

// Set* - parâmetro segue a propriedade
private bool SetFirstName(
    ExecutionContext executionContext,
    string firstName  // ✅ Non-null porque propriedade é non-null
)
```

**Razão**: Se Validate* passou e IsRequired=true, o valor NÃO é null. O compilador garante type-safety no momento da atribuição.

### Única Exceção: Construtor Privado

O construtor privado atribui diretamente **sem** usar Set*:

```csharp
private SimpleAggregateRoot(
    EntityInfo entityInfo,
    string firstName,
    string lastName,
    string fullName,
    BirthDate birthDate
)
{
    // Atribuição direta - SEM Set*
    EntityInfo = entityInfo;
    FirstName = firstName;
    LastName = lastName;
    FullName = fullName;
    BirthDate = birthDate;
}
```

**Razões**:
- `CreateFromExistingInfo` assume dados já validados no passado
- `Clone` assume dados já validados na instância original
- Não há ExecutionContext disponível (reconstitution não valida)

### Exemplos de Uso

**1. Método *Internal usando Set***:

```csharp
private bool ChangeNameInternal(
    ExecutionContext executionContext,
    string firstName,
    string lastName
)
{
    string fullName = $"{firstName} {lastName}";

    bool isSuccess =
        SetFirstName(executionContext, firstName)
        & SetLastName(executionContext, lastName)
        & SetFullName(executionContext, fullName);

    return isSuccess;
}
```

**2. Método público usando *Internal**:

```csharp
public SimpleAggregateRoot? ChangeName(
    ExecutionContext executionContext,
    ChangeNameInput input
)
{
    return RegisterChangeInternal(
        executionContext,
        input,
        handler: (ctx, inp, inst) =>
            inst.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
    );
}
```

**3. RegisterNew usando múltiplos *Internal**:

```csharp
public static SimpleAggregateRoot? RegisterNew(
    ExecutionContext executionContext,
    RegisterNewInput input
)
{
    return RegisterNewInternal(
        executionContext,
        input,
        entityFactory: (ctx, inp) => new SimpleAggregateRoot(),
        handler: (ctx, inp, inst) =>
        {
            return
                inst.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
                & inst.ChangeBirthDateInternal(ctx, inp.BirthDate);
        }
    );
}
```

### Comparação

| Aspecto | Setter Público | Set* Privado |
|---------|---------------|--------------|
| **Acesso** | Qualquer código | Apenas a própria classe |
| **Validação** | Manual/opcional | Obrigatória |
| **Estado inválido** | Possível | Impossível |
| **ExecutionContext** | Não disponível | Sempre disponível |
| **Coleta de erros** | Um por vez | Todos de uma vez |
| **Auditoria** | Difícil | Centralizada |

### Benefícios

1. **Estado sempre válido**: Propriedade só é atribuída após validação passar
2. **Validação contextual**: ExecutionContext permite regras por tenant, cultura, etc.
3. **Coleta completa de erros**: Operador `&` executa todas validações
4. **Single Responsibility**: Set* valida UMA propriedade, *Internal orquestra múltiplas
5. **Type-safety**: Compilador garante tipos corretos após validação
6. **Encapsulamento**: Setters privados impedem modificação externa

### Trade-offs (Com Perspectiva)

- **Mais código**: Um método Set* por propriedade
  - **Mitigação**: Template consistente, fácil de gerar. O código adicional garante invariantes.

- **Indireção**: Atribuição não é direta
  - **Mitigação**: Estrutura previsível. Debugger para em pontos claros.

### Trade-offs Frequentemente Superestimados

**"É muito código boilerplate"**

Na verdade, o template é simples e consistente:

```csharp
private bool SetX(ExecutionContext ctx, T value)
{
    if (!ValidateX(ctx, value))
        return false;
    X = value;
    return true;
}
```

São 6 linhas por propriedade. O custo é baixo comparado ao benefício de nunca ter estado inválido.

**"Performance do método adicional"**

Métodos privados são candidatos a inlining pelo JIT. Em builds Release, a performance é praticamente idêntica a atribuição direta.

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre encapsulamento:

> "The internal mechanisms of an object should be hidden; only the behavior that is meaningful to clients should be exposed."
>
> *Os mecanismos internos de um objeto devem ser escondidos; apenas o comportamento significativo para clientes deve ser exposto.*

Setters públicos expõem mecanismos internos. Métodos Set* privados escondem a atribuição, expondo apenas operações de negócio significativas.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre invariantes:

> "An Aggregate must always be in a consistent state. The Aggregate Root must enforce all invariants within the Aggregate boundary."
>
> *Um Aggregate deve estar sempre em um estado consistente. O Aggregate Root deve impor todas as invariantes dentro da fronteira do Aggregate.*

Métodos Set* garantem que cada atribuição respeita invariantes.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre encapsulamento de dados:

> "Hiding implementation is not just a matter of putting a layer of functions between the variables. Hiding implementation is about abstractions! A class does not simply push its variables out through getters and setters."
>
> *Esconder implementação não é simplesmente colocar uma camada de funções entre as variáveis. Esconder implementação é sobre abstrações! Uma classe não simplesmente empurra suas variáveis através de getters e setters.*

Setters públicos são "empurrar variáveis". Métodos Set* privados com validação são abstração real.

Robert C. Martin sobre funções pequenas:

> "Functions should do one thing. They should do it well. They should do it only."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso.*

SetFirstName faz uma coisa: validar e atribuir FirstName. ChangeNameInternal faz outra: orquestrar a mudança de nome completa.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre entidades:

> "An Entity is an object within our application that embodies a small set of critical business rules operating on Critical Business Data."
>
> *Uma Entidade é um objeto dentro da nossa aplicação que incorpora um pequeno conjunto de regras de negócio críticas operando sobre Dados Críticos de Negócio.*

Métodos Set* incorporam as regras de negócio (validação) que protegem os dados críticos (propriedades).

### Princípio de Design por Contrato

Bertrand Meyer em "Object-Oriented Software Construction" (1997) sobre invariantes:

> "A class invariant is a property that characterizes valid states. It must be satisfied after creation of any instance, and preserved by every exported routine."
>
> *Uma invariante de classe é uma propriedade que caracteriza estados válidos. Deve ser satisfeita após a criação de qualquer instância, e preservada por toda rotina exportada.*

Métodos Set* preservam invariantes em cada atribuição.

### Tell, Don't Ask

Martin Fowler sobre o princípio Tell, Don't Ask:

> "Rather than asking an object for data and acting on that data, we should instead tell an object what to do."
>
> *Ao invés de pedir dados a um objeto e agir sobre esses dados, devemos dizer ao objeto o que fazer.*

Com Set* privados, não "pedimos" para modificar uma propriedade diretamente. "Dizemos" à entidade para fazer uma operação de negócio que internamente modifica o que for necessário.

## Antipadrões Documentados

### Antipadrão 1: Setter Público

```csharp
// ❌ Setter público permite estado inválido
public class Person
{
    public string FirstName { get; set; }
}

// Uso problemático
person.FirstName = null;  // Estado inválido!
```

### Antipadrão 2: Validação no Setter com Exceção

```csharp
// ❌ Exceção para validação de negócio
public string FirstName
{
    get => _firstName;
    set
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Nome é obrigatório");
        _firstName = value;
    }
}
```

### Antipadrão 3: Atribuição Direta em Métodos Públicos

```csharp
// ❌ Método público atribui diretamente
public void ChangeName(string firstName, string lastName)
{
    // Validação...
    FirstName = firstName;  // Atribuição direta, não via Set*
    LastName = lastName;
}
```

### Antipadrão 4: Set* que Não Valida

```csharp
// ❌ Set* que apenas atribui, sem validar
private void SetFirstName(string firstName)
{
    FirstName = firstName;  // Cadê a validação?
}
```

### Antipadrão 5: Set* Público

```csharp
// ❌ Set* público permite bypass de orquestração
public class Person
{
    public string FirstName { get; private set; }

    // ❌ Público = qualquer código pode chamar
    public bool SetFirstName(ExecutionContext ctx, string firstName)
    {
        if (!ValidateFirstName(ctx, firstName))
            return false;

        FirstName = firstName;
        return true;
    }
}

// Problema: Código externo pode modificar sem passar por
// RegisterChangeInternal (sem clone, versão, auditoria)
person.SetFirstName(ctx, "Jane");  // Bypass de imutabilidade!
```

### Antipadrão 6: Validação com Side-Effects em Set*

```csharp
// ❌ Set* com side-effects - viola CQS
private bool SetFirstName(ExecutionContext ctx, string firstName)
{
    bool isValid = ValidateFirstName(ctx, firstName);

    if (!isValid)
        return false;

    FirstName = firstName;

    // ❌ Side-effects aqui - ERRADO!
    RaiseEvent(new FirstNameChangedEvent(...));  // Evento prematuro
    _logger.LogInformation("FirstName changed");

    return true;
}

// Problema: Se ChangeNameInternal chamar SetFirstName + SetLastName,
// e SetLastName falhar, eventos/logs já foram emitidos para SetFirstName
// mas a operação completa falhou (clone descartado)
```

### Antipadrão 7: Atribuição Antes da Validação

```csharp
// ❌ Valida DEPOIS de atribuir - janela de estado inválido
private bool SetFirstName(ExecutionContext ctx, string firstName)
{
    // Atribui ANTES de validar - ERRADO!
    FirstName = firstName;

    bool isValid = ValidateFirstName(ctx, firstName);

    if (!isValid)
    {
        // Oops! FirstName já foi modificado
        // Se exception for lançada aqui, estado fica inconsistente
        return false;
    }

    return true;
}
```

## Decisões Relacionadas

- [DE-004](./DE-004-estado-invalido-nunca-existe-na-memoria.md) - Estado inválido nunca existe
- [DE-006](./DE-006-operador-bitwise-and-para-validacao-completa.md) - Operador & para validação completa
- [DE-009](./DE-009-metodos-validate-publicos-e-estaticos.md) - Métodos Validate* públicos e estáticos
- [DE-016](./DE-016-single-source-of-truth-para-regras-de-validacao.md) - Single Source of Truth para validação
- [DE-021](./DE-021-metodos-publicos-vs-metodos-internos.md) - Métodos públicos vs internos

## Leitura Recomendada

- [Clean Code - Robert C. Martin](https://blog.cleancoder.com/)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Implementing Domain-Driven Design - Vaughn Vernon](https://vaughnvernon.com/)
- [Tell, Don't Ask - Martin Fowler](https://martinfowler.com/bliki/TellDontAsk.html)
- [Command-Query Separation - Martin Fowler](https://martinfowler.com/bliki/CommandQuerySeparation.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Define o padrão de métodos Set* privados para encapsular validação e atribuição de propriedades |
| [ValidationUtils](../../building-blocks/core/validations/validation-utils.md) | Utilizado pelos métodos Set* para realizar validações padronizadas antes da atribuição |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Métodos Set* Privados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - SetFirstName - exemplo de implementação
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - SetLastName - exemplo de implementação
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - SetBirthDate - exemplo de implementação
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ChangeNameInternal - uso de Set* com operador &
