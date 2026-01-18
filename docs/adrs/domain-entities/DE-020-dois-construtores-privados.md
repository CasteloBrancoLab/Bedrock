# DE-020: Dois Construtores Privados (Vazio e Completo)

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma **fábrica de carros** com duas linhas de montagem:

**Linha 1 - Carros novos**:
Cada peça é inspecionada antes de ser montada. Se o motor falhar no teste, o carro não sai da linha. Inspeção completa, carro por carro.

**Linha 2 - Carros usados (revisão)**:
Carros que já foram fabricados antes voltam para manutenção. Não faz sentido reinspecionar cada parafuso - o carro já existe e funcionou por anos. Apenas remontamos.

Em entidades de domínio, precisamos de duas "linhas de montagem":
- **Construtor vazio** + validação incremental: para dados **novos** (inspeção completa)
- **Construtor completo** sem validação: para dados **existentes** (remontagem)

---

### O Problema Técnico

Um único construtor não atende os dois cenários:

```csharp
// ❌ Construtor único com validação - quebra reconstitution
public Person(string firstName, string lastName)
{
    if (firstName.Length > MaxLength)
        throw new ArgumentException("Nome muito longo");
    FirstName = firstName;
    LastName = lastName;
}

// Problema 1: Reconstitution falha com dados históricos
var dto = db.Query("SELECT * FROM Persons WHERE Id = @id");
var person = new Person(dto.FirstName, dto.LastName);
// 💥 EXCEÇÃO se MaxLength mudou desde a criação!

// Problema 2: Sem validação incremental
// Se firstName E lastName são inválidos, só vejo erro do firstName
```

```csharp
// ❌ Construtor único SEM validação - permite estado inválido
public Person(string firstName, string lastName)
{
    FirstName = firstName;
    LastName = lastName;
}

// Problema: Qualquer um pode criar estado inválido
var person = new Person(null, ""); // Compila! Estado inválido criado.
```

## A Decisão

### Nossa Abordagem

**Dois construtores privados**, cada um com propósito específico:

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // -----------------------------------------------------------------------
    // CONSTRUTOR 1: VAZIO - para validação incremental (RegisterNew)
    // -----------------------------------------------------------------------
    private SimpleAggregateRoot()
    {
        // Vazio intencionalmente
        // Propriedades serão atribuídas via Set* após validação individual
    }

    // -----------------------------------------------------------------------
    // CONSTRUTOR 2: COMPLETO - para reconstitution e clone
    // -----------------------------------------------------------------------
    private SimpleAggregateRoot(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        string fullName,
        BirthDate birthDate
    ) : base(entityInfo)
    {
        // Atribuição direta - assume valores já validados
        FirstName = firstName;
        LastName = lastName;
        FullName = fullName;
        BirthDate = birthDate;
    }
}
```

### Construtor Vazio - Para Validação Incremental

Usado em `RegisterNew()` para permitir coleta de **todas** as mensagens de erro:

```csharp
public static SimpleAggregateRoot? RegisterNew(
    ExecutionContext executionContext,
    RegisterNewInput input
)
{
    // 1. Cria instância vazia
    var instance = new SimpleAggregateRoot();

    // 2. Valida e atribui cada propriedade incrementalmente
    //    Operador & garante que TODAS as validações executam
    return RegisterNewInternal(
        executionContext,
        input,
        entityFactory: (ctx, inp) => new SimpleAggregateRoot(),
        handler: (ctx, inp, inst) =>
        {
            // Validação completa com regras ATUAIS
            return
                inst.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
                & inst.ChangeBirthDateInternal(ctx, inp.BirthDate);
            // Se FirstName E BirthDate são inválidos, AMBOS os erros são coletados
        }
    );
}
```

**Por que validação incremental é importante**:

```csharp
// ❌ Sem validação incremental - usuário vê 1 erro por vez
// Tentativa 1: "Nome é obrigatório"
// Tentativa 2: "Sobrenome é obrigatório"
// Tentativa 3: "Data de nascimento inválida"
// Frustração!

// ✅ Com validação incremental - todos os erros de uma vez
// Tentativa 1:
//   - "Nome é obrigatório"
//   - "Sobrenome é obrigatório"
//   - "Data de nascimento inválida"
// Usuário corrige tudo de uma vez!
```

### Construtor Completo - Para Reconstitution e Clone

Usado em `CreateFromExistingInfo()` e `Clone()`:

```csharp
// Reconstitution - dados do banco/event store
public static SimpleAggregateRoot CreateFromExistingInfo(
    CreateFromExistingInfoInput input
)
{
    // Direto ao construtor completo - SEM validação
    return new SimpleAggregateRoot(
        input.EntityInfo,
        input.FirstName,
        input.LastName,
        input.FullName,
        input.BirthDate
    );
}

// Clone - para imutabilidade (Clone-Modify-Return)
public override SimpleAggregateRoot Clone()
{
    // Direto ao construtor completo - cópia exata
    return new SimpleAggregateRoot(
        EntityInfo,
        FirstName,
        LastName,
        FullName,
        BirthDate
    );
}
```

### Por Que Ambos DEVEM Ser Privados

De nada adianta:
- ✅ Criar propriedades para não expor fields
- ✅ Privar os setters das propriedades
- ❌ ...e deixar o construtor público aceitando qualquer coisa

```csharp
// ❌ Construtor público = buraco no encapsulamento
public class Person
{
    public string FirstName { get; private set; }  // Setter privado, ótimo!
    public string LastName { get; private set; }   // Setter privado, ótimo!

    public Person(string firstName, string lastName)  // Público = problema!
    {
        FirstName = firstName;
        LastName = lastName;
    }
}

// Qualquer código pode fazer:
var person = new Person(null, "");  // Compila! Estado inválido criado.
// Todo o esforço de encapsulamento foi desperdiçado!
```

**Construtores privados + Factory methods = encapsulamento COMPLETO**:

```csharp
// ✅ Construtores privados - encapsulamento real
public sealed class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    private Person() { }  // Privado!
    private Person(string firstName, string lastName)  // Privado!
    {
        FirstName = firstName;
        LastName = lastName;
    }

    // Único ponto de entrada controlado
    public static Person? RegisterNew(ExecutionContext ctx, RegisterNewInput input)
    {
        // Validação obrigatória aqui
    }
}

// Agora é impossível criar estado inválido:
var person = new Person(null, "");  // ❌ NÃO COMPILA - construtor privado!
```

### Por Que Construtor Completo NÃO Valida

| Razão | Explicação |
|-------|------------|
| **Limitação técnica** | Construtor sempre retorna instância, não pode retornar `null` |
| **Performance** | Exceções são caras; validação de negócio é esperada (não excepcional) |
| **Contexto** | Regras variam por tenant/usuário/feature flag (ExecutionContext) |
| **Dados históricos** | Regras mudam ao longo do tempo, dados antigos permanecem válidos |
| **Event Sourcing** | Eventos passados são imutáveis, devem ser aplicáveis sempre |
| **Compliance** | LGPD/GDPR/HIPAA exigem preservação exata de dados históricos |

### Fluxo de Cada Cenário

```
+-------------------------------------------------------------------------+
│                      CRIAR NOVA ENTIDADE                                │
│                                                                         │
│  RegisterNew(context, input)                                            │
│       │                                                                 │
│       ▼                                                                 │
│  +-------------------------------------+                                │
│  │ new SimpleAggregateRoot()           │ → Construtor VAZIO             │
│  │ (instância sem dados)               │                                │
│  +-------------------------------------+                                │
│       │                                                                 │
│       ▼                                                                 │
│  +-------------------------------------+                                │
│  │ SetEntityInfo() com:                │ → EntityInfo ANTES do handler  │
│  │   - Id gerado                       │                                │
│  │   - Versão = 1                      │                                │
│  │   - CreatedAt/By preenchidos        │                                │
│  +-------------------------------------+                                │
│       │                                                                 │
│       ▼                                                                 │
│  +-------------------------------------+                                │
│  │ handler():                          │ → Validação INCREMENTAL        │
│  │   ChangeNameInternal() &            │   (coleta TODOS os erros)      │
│  │   ChangeBirthDateInternal()         │                                │
│  +-------------------------------------+                                │
│       │                                                                 │
│       ▼                                                                 │
│  Retorna instância válida OU null + mensagens                           │
+-------------------------------------------------------------------------+

+-------------------------------------------------------------------------+
│                    RECONSTITUIR DO BANCO                                │
│                                                                         │
│  CreateFromExistingInfo(input)                                          │
│       │                                                                 │
│       ▼                                                                 │
│  +-------------------------------------+                                │
│  │ new SimpleAggregateRoot(            │ → Construtor COMPLETO          │
│  │     entityInfo,                     │   (atribuição direta)          │
│  │     firstName,                      │                                │
│  │     lastName,                       │                                │
│  │     fullName,                       │                                │
│  │     birthDate                       │                                │
│  │ )                                   │                                │
│  +-------------------------------------+                                │
│       │                                                                 │
│       ▼                                                                 │
│  Retorna instância (NUNCA null - dados já existem)                      │
+-------------------------------------------------------------------------+

+-------------------------------------------------------------------------+
│                    CLONE (Imutabilidade)                                │
│                                                                         │
│  instance.Clone()                                                       │
│       │                                                                 │
│       ▼                                                                 │
│  +-------------------------------------+                                │
│  │ new SimpleAggregateRoot(            │ → Construtor COMPLETO          │
│  │     this.EntityInfo,                │   (cópia exata)                │
│  │     this.FirstName,                 │                                │
│  │     this.LastName,                  │                                │
│  │     this.FullName,                  │                                │
│  │     this.BirthDate                  │                                │
│  │ )                                   │                                │
│  +-------------------------------------+                                │
│       │                                                                 │
│       ▼                                                                 │
│  Retorna cópia idêntica                                                 │
+-------------------------------------------------------------------------+
```

### Benefícios

1. **Encapsulamento completo**: Impossível criar estado inválido externamente
2. **Validação incremental**: Coleta todos os erros de uma vez (UX melhor)
3. **Reconstitution funciona**: Dados históricos carregam sem revalidação
4. **Clone eficiente**: Cópia direta sem overhead de validação
5. **Separação clara**: Cada construtor tem propósito específico
6. **Event Sourcing**: Eventos históricos sempre podem ser reaplicados

### Trade-offs (Com Perspectiva)

- **Dois construtores ao invés de um**: Complexidade adicional
  - **Mitigação**: Responsabilidades claras e bem definidas para cada um

### Trade-offs Frequentemente Superestimados

**"Por que não usar apenas o construtor completo para tudo?"**

Construtor completo não permite validação incremental:

```csharp
// Com construtor completo apenas
public static Person? RegisterNew(ExecutionContext ctx, string firstName, string lastName)
{
    // Preciso validar ANTES de chamar o construtor
    bool firstNameValid = ValidateFirstName(ctx, firstName);
    bool lastNameValid = ValidateLastName(ctx, lastName);

    if (!firstNameValid || !lastNameValid)
        return null;

    // Agora posso chamar o construtor
    return new Person(firstName, lastName);
}
// Funciona, mas é mais verboso e propenso a erros
```

**"Por que não usar init-only properties?"**

`init` não permite validação incremental nem coleta de múltiplos erros:

```csharp
// ❌ init-only - validação "tudo ou nada"
public class Person
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

// Onde colocar a validação? No setter?
// Se FirstName falhar, LastName nem é avaliado
```

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre factories:

> "A FACTORY handles the beginning of an object's life. [...] When creation of an object, or an entire AGGREGATE, becomes complicated or reveals too much of the internal structure, FACTORIES provide encapsulation."
>
> *Uma FACTORY cuida do início da vida de um objeto. [...] Quando a criação de um objeto, ou de um AGGREGATE inteiro, se torna complicada ou revela demais da estrutura interna, FACTORIES fornecem encapsulamento.*

Nossos dois construtores privados + factory methods são exatamente esta abordagem.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre encapsulamento:

> "All construction of Aggregates is done through Factory methods on the Aggregate type itself or on a separate Factory. [...] Factories shield clients from the complexity of Aggregate creation."
>
> *Toda construção de Aggregates é feita através de métodos Factory no próprio tipo do Aggregate ou em uma Factory separada. [...] Factories protegem clientes da complexidade da criação de Aggregates.*

`RegisterNew` e `CreateFromExistingInfo` são as factories que protegem clientes.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre construtores:

> "Consider using static factory methods with names that describe the arguments."
>
> *Considere usar métodos factory estáticos com nomes que descrevem os argumentos.*

`RegisterNew` descreve claramente: "estou registrando algo novo". `CreateFromExistingInfo` descreve: "estou criando a partir de informações existentes".

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre encapsulamento:

> "Encapsulation is violated when we expose data, and even more so when we expose the internal structure of our objects."
>
> *Encapsulamento é violado quando expomos dados, e ainda mais quando expomos a estrutura interna de nossos objetos.*

Construtores públicos expõem a estrutura interna. Construtores privados + factories escondem.

### Encapsulamento (OOP)

Bertrand Meyer em "Object-Oriented Software Construction" (1988):

> "Encapsulation is the inclusion within a program object of all the resources needed for the object to function—basically, the methods and the data."
>
> *Encapsulamento é a inclusão dentro de um objeto de programa de todos os recursos necessários para o objeto funcionar—basicamente, os métodos e os dados.*

Encapsulamento não é apenas "esconder dados", mas **controlar acesso** a esses dados. Construtores públicos quebram o encapsulamento ao permitir criação de estado sem passar pela lógica de validação.

### Factory Method Pattern

Gang of Four em "Design Patterns" (1994):

> "Define an interface for creating an object, but let subclasses decide which class to instantiate. Factory Method lets a class defer instantiation to subclasses."
>
> *Defina uma interface para criar um objeto, mas deixe subclasses decidirem qual classe instanciar. Factory Method permite que uma classe adie a instanciação para subclasses.*

Nossos factory methods (RegisterNew, CreateFromExistingInfo) encapsulam a lógica de criação e permitem:
- Retornar `null` em caso de falha (construtores não podem)
- Nomes expressivos que comunicam intenção
- Diferentes estratégias de criação

### Tell, Don't Ask

Martin Fowler popularizou este princípio:

> "Rather than asking an object for data and acting on that data, we should instead tell an object what to do."
>
> *Ao invés de pedir dados a um objeto e agir sobre esses dados, devemos dizer ao objeto o que fazer.*

Ao invés de expor construtor e esperar que o chamador valide, a entidade **diz** como deve ser criada através de factory methods.

## Antipadrões Documentados

### Antipadrão 1: Construtor Público

```csharp
// ❌ Construtor público - qualquer um cria estado inválido
public class Person
{
    public Person(string firstName) { FirstName = firstName; }
}

var person = new Person(null); // Compila!
```

### Antipadrão 2: Construtor Único com Validação

```csharp
// ❌ Valida no construtor - quebra reconstitution
public class Person
{
    public Person(string firstName)
    {
        if (string.IsNullOrEmpty(firstName))
            throw new ArgumentException("Nome obrigatório");
        FirstName = firstName;
    }
}
```

### Antipadrão 3: Validação Fora da Entidade

```csharp
// ❌ Validação no caller - fácil de esquecer
public class PersonService
{
    public Person CreatePerson(string firstName)
    {
        // E se alguém esquecer de validar aqui?
        if (string.IsNullOrEmpty(firstName))
            throw new Exception();

        return new Person(firstName); // Construtor público
    }
}
```

### Antipadrão 4: Flag "skipValidation"

```csharp
// ❌ Flag para pular validação - confuso e perigoso
public Person(string firstName, bool skipValidation = false)
{
    if (!skipValidation && string.IsNullOrEmpty(firstName))
        throw new Exception();
    FirstName = firstName;
}
```

## Decisões Relacionadas

- [DE-002](./DE-002-construtores-privados-com-factory-methods.md) - Construtores privados com factory methods
- [DE-004](./DE-004-estado-invalido-nunca-existe-na-memoria.md) - Estado inválido nunca existe
- [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) - Separação RegisterNew vs CreateFromExistingInfo
- [DE-018](./DE-018-reconstitution-nao-valida-dados.md) - Reconstitution não valida

## Leitura Recomendada

- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Factory Method Pattern - GoF](https://refactoring.guru/design-patterns/factory-method)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Define o padrão de dois construtores privados (vazio para validação incremental e completo para atribuição direta) |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Dois Construtores Privados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Construtores DEVEM Ser Privados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Construtor vazio
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Construtor completo + LLM_RULE: Por Que Construtor Completo NÃO Valida
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Clone Usa Construtor Privado
