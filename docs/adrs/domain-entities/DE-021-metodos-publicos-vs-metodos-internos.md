# DE-021: Métodos Públicos vs Métodos Internos (*Internal)

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **restaurante** com dois tipos de profissionais:

**Garçom (método público)**:
- Recebe o pedido do cliente
- Coordena a entrega entre cozinha e mesa
- Gerencia a conta e pagamento
- É a "face pública" do restaurante

**Cozinheiro (método *Internal)**:
- Prepara os pratos seguindo receitas
- Foca apenas na qualidade da comida
- Não interage com clientes
- Reutilizado por múltiplos garçons

Se o garçom tentasse cozinhar E servir, seria ineficiente. Se o cozinheiro tentasse cobrar, seria confuso. Cada um tem responsabilidade clara.

Em entidades de domínio, métodos públicos **orquestram** (como garçons), métodos *Internal **executam lógica** (como cozinheiros).

---

### O Problema Técnico

Sem separação clara, código fica duplicado e difícil de manter:

```csharp
// ❌ Sem métodos *Internal - duplicação
public static Person? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
{
    var instance = new Person();

    // Validação de nome AQUI
    if (!ValidateFirstName(ctx, input.FirstName)) return null;
    if (!ValidateLastName(ctx, input.LastName)) return null;
    instance.FirstName = input.FirstName;
    instance.LastName = input.LastName;

    return instance;
}

public Person? ChangeName(ExecutionContext ctx, ChangeNameInput input)
{
    var clone = this.Clone();

    // Mesma validação DUPLICADA aqui
    if (!ValidateFirstName(ctx, input.FirstName)) return null;
    if (!ValidateLastName(ctx, input.LastName)) return null;
    clone.FirstName = input.FirstName;
    clone.LastName = input.LastName;

    return clone;
}

// Mudou regra de nome? Precisa atualizar em DOIS lugares!
```

## A Decisão

### Nossa Abordagem

**Métodos públicos orquestram**, **métodos *Internal executam lógica**:

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // -----------------------------------------------------------------------
    // MÉTODOS PÚBLICOS - Orquestração (handlers devem ser static)
    // -----------------------------------------------------------------------

    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (ctx, inp) => new SimpleAggregateRoot(),
            handler: static (ctx, inp, instance) =>
            {
                // Chama métodos *Internal para lógica de negócio
                return
                    instance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
                    & instance.ChangeBirthDateInternal(ctx, inp.BirthDate);
            }
        );
    }

    public SimpleAggregateRoot? ChangeName(
        ExecutionContext executionContext,
        ChangeNameInput input
    )
    {
        return RegisterChangeInternal(
            executionContext,
            instance: this,
            input,
            handler: static (ctx, inp, newInstance) =>
            {
                // REUTILIZA o mesmo método *Internal
                return newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName);
            }
        );
    }

    // -----------------------------------------------------------------------
    // MÉTODOS *INTERNAL - Lógica de Negócio
    // -----------------------------------------------------------------------

    private bool ChangeNameInternal(
        ExecutionContext executionContext,
        string firstName,
        string lastName
    )
    {
        string fullName = $"{firstName} {lastName}";

        // Lógica de validação e atribuição
        bool isSuccess =
            SetFirstName(executionContext, firstName)
            & SetLastName(executionContext, lastName)
            & SetFullName(executionContext, fullName);

        return isSuccess;
    }
}
```

### Responsabilidades de Cada Tipo

| Aspecto | Método Público | Método *Internal |
|---------|----------------|------------------|
| **Responsabilidade** | Orquestra operação | Executa lógica de negócio |
| **Gerencia ciclo de vida** | ✅ Sim (clone, versão, eventos) | ❌ Não |
| **Recebe** | Input Object | Parâmetros diretos |
| **Retorna** | `T?` (instância ou null) | `bool` (sucesso/falha) |
| **Cria clones** | ✅ Sim (via Register*Internal) | ❌ Não |
| **Reutilizável** | ❌ Não (cada um é único) | ✅ Sim (por múltiplos públicos) |
| **Side-effects** | ✅ Pode ter (eventos, logging) | ❌ Não deve ter |

### Por Que Métodos Públicos Chamam Register*Internal

`RegisterNewInternal` e `RegisterChangeInternal` gerenciam automaticamente:

```csharp
// O que RegisterChangeInternal faz por você:
public SimpleAggregateRoot? ChangeName(ExecutionContext ctx, ChangeNameInput input)
{
    return RegisterChangeInternal(ctx, this, input, handler: static (ctx, inp, newInstance) =>
    {
        // 1. ✅ Clone já foi criado (newInstance)
        // 2. ✅ Versão e auditoria JÁ foram atualizadas em newInstance
        //    (newInstance.EntityInfo já contém versão+1 e timestamps atualizados)

        // Você só foca na LÓGICA de negócio:
        return newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName);
    });
    // Se handler retornar false, newInstance é descartado
    // Se handler retornar true, newInstance é retornado
}

// Sem RegisterChangeInternal, você teria que fazer TUDO manualmente:
public SimpleAggregateRoot? ChangeNameManual(ExecutionContext ctx, ChangeNameInput input)
{
    var clone = this.Clone();                           // Manual
    clone.IncrementVersion();                           // Manual
    clone.SetModifiedAt(ctx.TimeProvider.GetUtcNow()); // Manual
    clone.SetModifiedBy(ctx.CurrentUser);              // Manual

    bool success = clone.ChangeNameInternal(ctx, input.FirstName, input.LastName);

    if (!success) return null;  // Clone descartado

    return clone;
}
// Fácil esquecer algo, duplicação entre métodos, bugs sutis
```

### Por Que Métodos *Internal Recebem Parâmetros Diretos

Métodos *Internal recebem parâmetros individuais, **não** Input Objects:

```csharp
// ✅ CORRETO - Parâmetros diretos
private bool ChangeNameInternal(
    ExecutionContext executionContext,
    string firstName,      // Parâmetro direto
    string lastName        // Parâmetro direto
)
{
    // ...
}

// ❌ ERRADO - Input Object em método privado
private bool ChangeNameInternal(
    ExecutionContext executionContext,
    ChangeNameInput input  // Desnecessário - não há extensibilidade
)
{
    // ...
}
```

**Razões**:

1. **Escopo mínimo de argumentos**: Propagar Input Objects para métodos privados daria a esses métodos acesso a mais dados do que eles realmente precisam. `ChangeNameInternal` precisa apenas de `firstName` e `lastName`, não de todo o `RegisterNewInput` que também contém `BirthDate`. Parâmetros diretos garantem que cada método receba **exatamente** o que precisa - nem mais, nem menos.

2. **Sem ponto de extensão**: Input Objects existem para customização via factories (IOC) em métodos públicos. Métodos privados são chamados apenas internamente - não há ponto de extensão. Mudanças em regras internas são feitas via código, não injeção de factories.

3. **Clareza de dependências**: Ao olhar a assinatura `ChangeNameInternal(ctx, firstName, lastName)`, fica imediatamente claro quais dados o método usa. Com `ChangeNameInternal(ctx, input)`, seria necessário ler a implementação para saber quais campos são acessados.

### Regra: Método Público NUNCA Chama Outro Método Público

```csharp
// ❌ ERRADO - Método público chamando outro método público
public override SimpleAggregateRoot Clone()
{
    // NÃO faça isso!
    return CreateFromExistingInfo(new CreateFromExistingInfoInput(
        EntityInfo, FirstName, LastName, FullName, BirthDate
    ));
}

// ✅ CORRETO - Método público usa construtor privado diretamente
public override SimpleAggregateRoot Clone()
{
    return new SimpleAggregateRoot(
        EntityInfo,
        FirstName,
        LastName,
        FullName,
        BirthDate
    );
}
```

**Problemas de métodos públicos chamando outros públicos**:

1. **Operações duplicadas acidentais**: Se `CreateFromExistingInfo()` registrar evento de "entidade carregada", `Clone()` também registraria esse evento durante modificações normais.

2. **Efeitos colaterais imprevisíveis**: Métodos públicos podem ter side-effects (logging, eventos, métricas). Chamar um de outro acumula side-effects inesperados.

3. **Dificuldade de manutenção**: Mudança em `CreateFromExistingInfo()` afetaria `Clone()` sem intenção. Rastrear bugs se torna difícil.

4. **Violação do princípio de responsabilidade única**: Cada método público deve ter caminho de execução isolado e previsível.

**Solução**: Se dois métodos públicos precisam da mesma lógica, extraia para um método *Internal compartilhado.

### Regra: Register*Internal Chamado UMA ÚNICA VEZ

```csharp
// ✅ CORRETO - Uma chamada, múltiplos métodos internos, handler static
public SimpleAggregateRoot? UpdateProfile(
    ExecutionContext ctx,
    UpdateProfileInput input
)
{
    return RegisterChangeInternal(ctx, this, input, handler: static (ctx, inp, newInstance) =>
    {
        // TODOS os métodos *Internal dentro de UMA ÚNICA chamada
        return
            newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
            & newInstance.ChangeBirthDateInternal(ctx, inp.BirthDate)
            & newInstance.ChangeAddressInternal(ctx, inp.Address);
    });
}

// ❌ ERRADO - Múltiplas chamadas a RegisterChangeInternal
public SimpleAggregateRoot? UpdateProfileWrong(
    ExecutionContext ctx,
    UpdateProfileInput input
)
{
    // Problema 1: Múltiplos clones criados
    var result1 = RegisterChangeInternal(..., handler: static (ctx, inp, n) =>
        n.ChangeNameInternal(ctx, inp.FirstName, inp.LastName));

    // Problema 2: Versão incrementada múltiplas vezes
    var result2 = result1?.RegisterChangeInternal(..., handler: static (ctx, inp, n) =>
        n.ChangeBirthDateInternal(ctx, inp.BirthDate));

    // Problema 3: Estado inconsistente, performance ruim
    return result2;
}
```

### Fluxo de Execução

```
+-------------------------------------------------------------------------+
│                    MÉTODO PÚBLICO (ChangeName)                          │
│                                                                         │
│  ChangeName(ctx, input)                                                 │
│       │                                                                 │
│       ▼                                                                 │
│  +-----------------------------------------------------------------+    │
│  │ RegisterChangeInternal()                                        │    │
│  │   +---------------------------------------------------------+   │    │
│  │   │ 1. Clone da instância atual                             │   │    │
│  │   │ 2. SetEntityInfo() no clone com:                        │   │    │
│  │   │    - Versão incrementada (já aplicada)                  │   │    │
│  │   │    - Auditoria atualizada (já aplicada)                 │   │    │
│  │   +---------------------------------------------------------+   │    │
│  │       │                                                         │    │
│  │       ▼                                                         │    │
│  │   +---------------------------------------------------------+   │    │
│  │   │ handler(ctx, input, newInstance)                        │   │    │
│  │   │   (newInstance já possui EntityInfo atualizado!)        │   │    │
│  │   │       │                                                 │   │    │
│  │   │       ▼                                                 │   │    │
│  │   │   ChangeNameInternal(ctx, firstName, lastName)          │   │    │
│  │   │       │                                                 │   │    │
│  │   │       ▼                                                 │   │    │
│  │   │   SetFirstName() & SetLastName() & SetFullName()        │   │    │
│  │   │       │                                                 │   │    │
│  │   │       ▼                                                 │   │    │
│  │   │   return bool (sucesso/falha)                           │   │    │
│  │   +---------------------------------------------------------+   │    │
│  │       │                                                         │    │
│  │       ▼                                                         │    │
│  │   +---------------------------------------------------------+   │    │
│  │   │ Se sucesso:                                             │   │    │
│  │   │   - Retorna newInstance (já com versão/auditoria)       │   │    │
│  │   │ Se falha:                                               │   │    │
│  │   │   - Descarta newInstance                                │   │    │
│  │   │   - Retorna null                                        │   │    │
│  │   +---------------------------------------------------------+   │    │
│  +-----------------------------------------------------------------+    │
+-------------------------------------------------------------------------+
```

### Benefícios

1. **Zero duplicação**: Lógica de negócio em um lugar só (*Internal)
2. **Manutenção fácil**: Mudou regra? Altera em um lugar, reflete em todos
3. **Separação clara**: Público orquestra, *Internal executa
4. **Consistência**: Todos os métodos públicos seguem o mesmo padrão
5. **Previsibilidade**: Cada método público tem caminho de execução isolado
6. **Testabilidade natural**: Métodos privados são testados através dos métodos públicos que os chamam - se existe lógica no método privado que nenhum público exercita, essa lógica é dead code e deveria ser removida

### Trade-offs (Com Perspectiva)

- **Mais métodos para criar**: Cada operação tem público + *Internal
  - **Mitigação**: Responsabilidades claras, menos bugs, mais fácil de manter

### Trade-offs Frequentemente Superestimados

**"Por que não colocar tudo no método público?"**

Duplicação inevitável:

```csharp
// RegisterNew precisa validar nome
// ChangeName precisa validar nome
// Se a validação estiver em cada método público, está duplicada
```

**"Por que não fazer métodos *Internal públicos?"**

Métodos *Internal:
- Operam em instância já clonada
- Não gerenciam ciclo de vida
- Não são seguros para chamar diretamente de fora

Expô-los quebraria encapsulamento e levaria a uso incorreto.

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre responsabilidades:

> "AGGREGATES [...] should have narrow interfaces. [...] Keep the Aggregate Roots and their methods focused on expressing the domain logic."
>
> *AGGREGATES [...] devem ter interfaces estreitas. [...] Mantenha os Aggregate Roots e seus métodos focados em expressar a lógica de domínio.*

Métodos públicos expressam operações de domínio (`ChangeName`). Métodos *Internal expressam lógica reutilizável (`ChangeNameInternal`).

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre coesão:

> "Cohesion is all about keeping related things together and unrelated things separate."
>
> *Coesão é sobre manter coisas relacionadas juntas e coisas não relacionadas separadas.*

Lógica de validação de nome pertence a `ChangeNameInternal`. Ciclo de vida (clone, versão) pertence a `RegisterChangeInternal`. Separação = coesão.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre tamanho de funções:

> "The first rule of functions is that they should be small. The second rule of functions is that they should be smaller than that."
>
> *A primeira regra de funções é que devem ser pequenas. A segunda regra é que devem ser menores ainda.*

Métodos públicos são pequenos porque delegam para métodos *Internal. Métodos *Internal são pequenos porque fazem uma coisa específica.

O princípio de **níveis de abstração**:

> "Functions should do one thing. They should do it well. They should do it only. [...] We want the code to read like a top-down narrative."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso. [...] Queremos que o código seja lido como uma narrativa de cima para baixo.*

Método público (alto nível) → `RegisterChangeInternal` (médio nível) → `ChangeNameInternal` (baixo nível). Uma narrativa clara.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre separação de responsabilidades:

> "The Single Responsibility Principle (SRP) states that a module should have one, and only one, reason to change."
>
> *O Princípio de Responsabilidade Única (SRP) afirma que um módulo deve ter uma, e apenas uma, razão para mudar.*

Cada tipo de método tem uma razão para mudar:
- **Público**: Muda quando a API de operações muda
- ***Internal**: Muda quando a lógica de negócio muda

### Single Responsibility Principle (SRP)

Cada método deve ter uma única responsabilidade:
- **Público**: Orquestrar operação de negócio
- ***Internal**: Executar lógica específica

### Template Method Pattern (Variação)

Gang of Four em "Design Patterns" (1994):

> "Define the skeleton of an algorithm in an operation, deferring some steps to subclasses. Template Method lets subclasses redefine certain steps of an algorithm without changing the algorithm's structure."
>
> *Defina o esqueleto de um algoritmo em uma operação, adiando alguns passos para subclasses. Template Method permite que subclasses redefinam certos passos de um algoritmo sem mudar a estrutura do algoritmo.*

`RegisterNewInternal` e `RegisterChangeInternal` funcionam como Template Methods - definem o esqueleto da operação (clone, versão, auditoria), delegando passos específicos para métodos *Internal.

### Command Pattern (Influência)

Gang of Four em "Design Patterns" (1994):

> "Encapsulate a request as an object, thereby letting you parameterize clients with different requests, queue or log requests, and support undoable operations."
>
> *Encapsule uma requisição como um objeto, permitindo parametrizar clientes com diferentes requisições, enfileirar ou logar requisições, e suportar operações reversíveis.*

Métodos públicos funcionam como Commands - encapsulam uma operação completa com todos os side-effects necessários.

## Antipadrões Documentados

### Antipadrão 1: Lógica de Negócio em Método Público

```csharp
// ❌ Lógica de negócio diretamente no método público
public SimpleAggregateRoot? ChangeName(ExecutionContext ctx, ChangeNameInput input)
{
    return RegisterChangeInternal(ctx, this, input, handler: static (ctx, inp, newInstance) =>
    {
        // Lógica direta aqui - não reutilizável!
        string fullName = $"{inp.FirstName} {inp.LastName}";
        newInstance.FirstName = inp.FirstName;
        newInstance.LastName = inp.LastName;
        newInstance.FullName = fullName;
        return true;
    });
}
// RegisterNew teria que duplicar toda essa lógica
```

### Antipadrão 2: Método Público Chamando Outro Público

```csharp
// ❌ Cria acoplamento e side-effects inesperados
public SimpleAggregateRoot Clone()
{
    return CreateFromExistingInfo(...); // Pode ter eventos, logging, etc.
}
```

### Antipadrão 3: Múltiplas Chamadas a Register*Internal

```csharp
// ❌ Múltiplos clones, versões, estados inconsistentes
public SimpleAggregateRoot? Update(ExecutionContext ctx, UpdateInput input)
{
    var r1 = RegisterChangeInternal(...);
    var r2 = r1?.RegisterChangeInternal(...);
    var r3 = r2?.RegisterChangeInternal(...);
    return r3;
}
```

### Antipadrão 4: Input Object em Método *Internal

```csharp
// ❌ Input Object em método privado - sem benefício
private bool ChangeNameInternal(ExecutionContext ctx, ChangeNameInput input)
{
    // Não há factory/IOC para métodos privados
}
```

## Decisões Relacionadas

- [DE-006](./DE-006-operador-bitwise-and-para-validacao-completa.md) - Operador & para validação completa
- [DE-019](./DE-019-input-objects-pattern.md) - Input Objects para métodos públicos
- [DE-020](./DE-020-dois-construtores-privados.md) - Construtores privados
- [DE-022](./DE-022-metodos-set-privados.md) - Métodos Set* privados

## Leitura Recomendada

- [Clean Code - Robert C. Martin](https://blog.cleancoder.com/)
- [Template Method Pattern - GoF](https://refactoring.guru/design-patterns/template-method)
- [Single Responsibility Principle](https://blog.cleancoder.com/uncle-bob/2014/05/08/SingleReponsibilityPrinciple.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Estabelece a separação entre métodos públicos (com Clone-Modify-Return) e métodos *Internal (que modificam a instância atual) |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Métodos Públicos vs Métodos Internos
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Método Público NUNCA Chama Outro Público
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Métodos Privados *Internal
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ChangeNameInternal - exemplo de implementação
