# DE-055: RegisterNewBase em Classes Abstratas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fábrica de veículos onde a linha de montagem base (classe abstrata) precisa instalar motor e transmissão em todo veículo. Se cada modelo específico (sedan, SUV, pickup) pudesse pular a linha base e montar diretamente, alguns veículos poderiam sair sem motor instalado corretamente.

A solução é que a linha base **encapsule** seu próprio processo de montagem. Os modelos específicos passam seus componentes extras, mas a instalação do motor é **sempre** feita pela linha base, garantindo que nenhum veículo saia sem motor.

### O Problema Técnico

Em hierarquias de entidades, a classe abstrata define propriedades que precisam ser validadas e inicializadas durante o registro de novas instâncias. Se a classe filha chamar `RegisterNewInternal` diretamente, o desenvolvedor pode esquecer de chamar os métodos `*Internal` da classe pai:

```csharp
public abstract class Person : EntityBase<Person>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // Classe filha precisa chamar ChangeNameInternal... mas pode esquecer!
    protected bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName) { ... }
}

public sealed class Employee : Person
{
    public string EmployeeNumber { get; private set; } = string.Empty;

    public static Employee? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        return RegisterNewInternal(
            ctx,
            input,
            entityFactory: static (ctx, input) => new Employee(),
            handler: static (ctx, input, instance) =>
            {
                // ❌ PERIGO: Desenvolvedor pode esquecer de chamar ChangeNameInternal!
                // Apenas chama ChangeEmployeeNumberInternal
                return instance.ChangeEmployeeNumberInternal(ctx, input.EmployeeNumber);
            }
        );
    }
}
```

O resultado é uma entidade com `FirstName` e `LastName` vazios - estado inválido que viola DE-004.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos delega toda responsabilidade para a classe filha:

**1. Classe Filha Chama Todos os Métodos Internos**

```csharp
public sealed class Employee : Person
{
    public static Employee? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        return RegisterNewInternal(
            ctx,
            input,
            entityFactory: static (ctx, input) => new Employee(),
            handler: static (ctx, input, instance) =>
            {
                return
                    instance.ChangeNameInternal(ctx, input.FirstName, input.LastName)  // Pai
                    & instance.ChangeEmployeeNumberInternal(ctx, input.EmployeeNumber); // Filha
            }
        );
    }
}
```

**2. Documentação e Code Review**

Confiar que desenvolvedores sempre lembrem de chamar métodos da classe pai, validando via code review.

### Por Que Não Funciona Bem

**Problema 1: Esquecimento É Inevitável**

Em hierarquias com múltiplas propriedades na classe pai, é fácil esquecer de chamar um dos métodos `*Internal`:

```csharp
handler: static (ctx, input, instance) =>
{
    return
        instance.ChangeNameInternal(ctx, input.FirstName, input.LastName)
        // ❌ Esqueceu ChangeAddressInternal!
        // ❌ Esqueceu ChangeBirthDateInternal!
        & instance.ChangeEmployeeNumberInternal(ctx, input.EmployeeNumber);
}
```

**Problema 2: Mudanças na Classe Pai Quebram Filhas**

Se a classe pai adicionar uma nova propriedade obrigatória, **todas** as classes filhas precisam ser atualizadas. Sem um mecanismo de enforcement, algumas podem ser esquecidas.

**Problema 3: Code Review Não Escala**

Em projetos grandes com múltiplas hierarquias e equipes, é impossível garantir via code review que todos os métodos foram chamados.

## A Decisão

### Nossa Abordagem

A classe abstrata encapsula seu próprio processo de registro através do método `RegisterNewBase`:

```csharp
public abstract class Person : EntityBase<Person>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════════════════════
    // RegisterNewBase - Classe abstrata CONTROLA seu próprio registro
    // ═══════════════════════════════════════════════════════════════════════════
    public static TConcreteType? RegisterNewBase<TConcreteType, TInput>(
        ExecutionContext executionContext,
        TInput input,
        Func<ExecutionContext, TInput, TConcreteType> concreteTypeFactory,
        Func<ExecutionContext, TInput, RegisterNewPersonInput> baseInputFactory,
        Func<ExecutionContext, TInput, TConcreteType, bool> handler
    ) where TConcreteType : Person
    {
        // Extrai dados da classe pai do input da filha
        RegisterNewPersonInput baseInput = baseInputFactory(executionContext, input);

        return RegisterNewInternal(
            executionContext,
            input: (Input: input, BaseInput: baseInput, ConcreteTypeFactory: concreteTypeFactory, Handler: handler),
            entityFactory: static (ctx, i) => i.ConcreteTypeFactory(ctx, i.Input),
            handler: static (ctx, i, instance) =>
            {
                // ✅ Classe pai SEMPRE executa suas próprias validações
                return
                    instance.ChangeNameInternal(ctx, i.BaseInput.FirstName, i.BaseInput.LastName)
                    & i.Handler(ctx, i.Input, instance)  // Depois executa handler da filha
                    ;
            }
        );
    }

    protected bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName) { ... }
}
```

A classe filha chama `RegisterNewBase` ao invés de `RegisterNewInternal`:

```csharp
public sealed class Employee : Person
{
    public string EmployeeNumber { get; private set; } = string.Empty;

    public static Employee? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        return RegisterNewBase(
            ctx,
            input,
            concreteTypeFactory: static (ctx, input) => new Employee(),
            baseInputFactory: static (ctx, input) => new RegisterNewPersonInput
            {
                FirstName = input.FirstName,
                LastName = input.LastName
            },
            handler: static (ctx, input, instance) =>
            {
                // ✅ Filha só precisa cuidar das suas propriedades
                return instance.ChangeEmployeeNumberInternal(ctx, input.EmployeeNumber);
            }
        );
    }
}
```

### Parâmetros do RegisterNewBase

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `concreteTypeFactory` | `Func<ExecutionContext, TInput, TConcreteType>` | Cria instância vazia do tipo concreto |
| `baseInputFactory` | `Func<ExecutionContext, TInput, RegisterNew*Input>` | Mapeia input da filha para input da classe base |
| `handler` | `Func<ExecutionContext, TInput, TConcreteType, bool>` | Lógica específica da classe filha (propriedades da filha) |

### Por Que Usar Tuple Para Carregar Delegates

O método `RegisterNewInternal` exige handlers estáticos para evitar closures. Para passar múltiplos valores ao handler, usamos uma tupla:

```csharp
return RegisterNewInternal(
    executionContext,
    // Tupla carrega todos os valores necessários
    input: (Input: input, BaseInput: baseInput, ConcreteTypeFactory: concreteTypeFactory, Handler: handler),
    entityFactory: static (ctx, i) => i.ConcreteTypeFactory(ctx, i.Input),
    handler: static (ctx, i, instance) =>
    {
        return
            instance.ChangeNameInternal(ctx, i.BaseInput.FirstName, i.BaseInput.LastName)
            & i.Handler(ctx, i.Input, instance);
    }
);
```

### Fluxo de Execução

```
┌─────────────────────────────────────────────────────────────────┐
│                    Employee.RegisterNew                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Employee.RegisterNew(ctx, input)                               │
│      │                                                          │
│      ▼                                                          │
│  Person.RegisterNewBase(                                        │
│      ctx,                                                       │
│      input,                                                     │
│      concreteTypeFactory,                                       │
│      baseInputFactory,     ◄── Mapeia input para dados da pai   │
│      handler               ◄── Lógica específica da filha       │
│  )                                                              │
│      │                                                          │
│      ▼                                                          │
│  RegisterNewInternal(                                           │
│      ctx,                                                       │
│      (Input, BaseInput, ConcreteTypeFactory, Handler),          │
│      entityFactory,                                             │
│      handler                                                    │
│  )                                                              │
│      │                                                          │
│      ▼                                                          │
│  new Employee()  ◄── concreteTypeFactory cria instância         │
│      │                                                          │
│      ▼                                                          │
│  instance.ChangeNameInternal(...)  ◄── PAI executa SEMPRE       │
│      │                                                          │
│      ▼                                                          │
│  handler(...)  ◄── FILHA executa sua lógica específica          │
│      │                                                          │
│      ▼                                                          │
│  return instance;  // Estado válido garantido                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Input Object Para Classe Base

Cada classe abstrata define seu próprio Input Object para receber dados das classes filhas:

```csharp
namespace Templates.Domain.Entities.AbstractAggregateRoots.Base.Inputs;

public readonly record struct RegisterNewPersonInput
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}
```

A classe filha mapeia seu input para o input da classe base via `baseInputFactory`:

```csharp
baseInputFactory: static (ctx, input) => new RegisterNewPersonInput
{
    FirstName = input.FirstName,
    LastName = input.LastName
}
```

## Consequências

### Benefícios

- **Validação Garantida**: Propriedades da classe pai SEMPRE são validadas e inicializadas
- **Impossível Esquecer**: Classe filha não tem acesso direto ao `RegisterNewInternal`, só ao `RegisterNewBase`
- **Manutenção Simplificada**: Adicionar propriedade na pai não requer mudança em todas as filhas
- **Separação de Responsabilidades**: Pai cuida das suas propriedades, filha cuida das suas
- **Handlers Estáticos**: Uso de tupla permite passar comportamento como dados sem closures

### Trade-offs

- **Complexidade Adicional**: Três parâmetros extras (`concreteTypeFactory`, `baseInputFactory`, `handler`)
- **Input Object Extra**: Classe base precisa de seu próprio Input Object
- **Curva de Aprendizado**: Padrão menos intuitivo que chamar métodos diretamente
- **Hierarquia Profunda**: Em hierarquias com 3+ níveis, cada nível precisa do seu `RegisterNewBase`

### Relação com Outras ADRs

| ADR | Relação |
|-----|---------|
| DE-004 | RegisterNewBase garante que estado inválido nunca exista |
| DE-006 | Usa operador `&` (bitwise AND) para executar TODAS as validações |
| DE-019 | Input Objects padrão usado para mapear dados entre hierarquias |
| DE-049 | Métodos `*Internal` protegidos são chamados pelo RegisterNewBase |
| DE-050 | Classe abstrata não expõe RegisterNew público, apenas RegisterNewBase |
| DE-052 | Construtor vazio protegido permite criar instância via `concreteTypeFactory` |

## Fundamentação Teórica

### Princípio de Hollywood (Don't Call Us, We'll Call You)

A classe pai controla o fluxo de registro. A classe filha "se registra" passando callbacks, mas quem decide **quando** e **como** executar é a classe pai. Isso inverte o controle tradicional onde a filha chamaria métodos da pai.

### Template Method Pattern (Variação)

`RegisterNewBase` é uma variação do Template Method onde o algoritmo (validar pai, depois filha) está fixo na classe base, mas os passos específicos (criar instância, mapear input, handler da filha) são fornecidos pela classe derivada via delegates.

### Composition Over Inheritance (Para Comportamento)

Ao invés de herdar comportamento e sobrescrever, a classe filha **compõe** seu comportamento passando delegates. Isso é mais flexível e evita problemas do "fragile base class".

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que usar tupla para passar múltiplos valores a um handler estático?"
- "Como garantir que todas as propriedades de uma classe abstrata sejam validadas?"
- "Qual a diferença entre Template Method e passar delegates como parâmetros?"

### Leitura Recomendada

- ADR DE-004: Estado Inválido Nunca Existe na Memória
- ADR DE-049: Métodos *Internal Protegidos em Classes Abstratas
- ADR DE-050: Classe Abstrata Não Expõe Métodos Públicos de Negócio
- ADR DE-052: Construtores Protegidos em Classes Abstratas

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que define `RegisterNewInternal` |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - implementação do `RegisterNewBase` com comentário LLM_GUIDANCE
- [LeafAggregateRootTypeA.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/LeafAggregateRootTypeA.cs) - exemplo de classe filha chamando `RegisterNewBase` com CategoryType.TypeA fixo
- [LeafAggregateRootTypeB.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/LeafAggregateRootTypeB.cs) - exemplo de classe filha chamando `RegisterNewBase` com CategoryType.TypeB fixo
- [RegisterNewAbstractAggregateRootInput.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/Inputs/RegisterNewAbstractAggregateRootInput.cs) - Input Object da classe base
