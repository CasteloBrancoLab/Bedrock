# DE-002: Construtores Privados com Factory Methods

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fábrica de automóveis. Qualquer pessoa pode entrar e montar um carro? Claro que não! Existe uma linha de montagem com inspeções de qualidade em cada etapa. Só carros que passam por TODAS as verificações saem da fábrica.

Agora imagine se qualquer funcionário pudesse montar um carro em qualquer lugar da fábrica, sem seguir a linha de montagem. Alguns carros sairiam sem freios, outros sem airbags, outros com peças incompatíveis. O controle de qualidade seria impossível.

Na programação, construtores públicos são como "portas abertas" na fábrica - qualquer código pode criar objetos, pulando validações.

### O Problema Técnico

Construtores em C# têm uma limitação fundamental: **sempre retornam uma instância**. Não é possível retornar `null` de um construtor para indicar que a criação falhou.

Isso força duas escolhas ruins:
1. **Lançar exceção** - caro em performance, inadequado para validação de negócio
2. **Criar objeto inválido** - estado corrompido na memória

Além disso, construtores públicos permitem criação descontrolada de objetos em qualquer lugar do código.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos usa construtores públicos com exceções:

```csharp
public class Person
{
    public string FirstName { get; }
    public string LastName { get; }

    // Construtor público - qualquer código pode chamar
    public Person(string firstName, string lastName)
    {
        // Validação com exceções
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName is required", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
    }
}

// Uso - precisa de try/catch para cada criação
try
{
    var person = new Person(request.FirstName, request.LastName);
}
catch (ArgumentException ex)
{
    // Tratar erro... mas qual campo falhou?
    // E se múltiplos campos estiverem errados?
}
```

### Por Que Não Funciona Bem

1. **Uma exceção por vez**: Usuário vê "FirstName inválido", corrige, submete, vê "LastName inválido". UX terrível.

2. **Performance**: Exceções são caras - stack trace, unwinding. Validação de negócio é esperada, não excepcional.

3. **Controle de fluxo com exceções**: Anti-pattern reconhecido. Exceções são para situações excepcionais.

4. **Sem contexto**: Difícil passar informações contextuais (tenant, usuário, feature flags) para validação.

```csharp
// Problema: múltiplos erros, mas só vemos um
var person = new Person("", ""); // Lança exceção só para FirstName
// Usuário nunca descobre que LastName também está errado
```

5. **Encapsulamento incompleto**: De nada adianta criar propriedades para não expor fields e privar os setters, se o construtor público aceita qualquer coisa:

```csharp
public class Person
{
    public string FirstName { get; private set; } // Setter privado ✅

    public Person(string firstName) // Construtor público = BURACO
    {
        FirstName = firstName; // Bypass de TODA validação!
    }
}

var person = new Person(null); // Compila! Estado inválido criado.
```

6. **Reconstitution impossível**: Construtores públicos que validam PARECEM seguros, mas QUEBRAM reconstitution de dados históricos:

```csharp
public Person(string firstName)
{
    if (firstName.Length > MaxLength) // MaxLength = 20 (regra de 2025)
        throw new ArgumentException("Nome muito longo");
    FirstName = firstName;
}

// 2020: MaxLength era 100, usuário cadastrou "João da Silva Pereira Santos" (30 chars)
// 2025: MaxLength mudou para 20

// Repository tenta carregar do banco:
var dto = _db.Query("SELECT * FROM Persons WHERE Id = @id");
var person = new Person(dto.FirstName); // 💥 EXCEÇÃO! Nome tem 30 chars, max é 20
```

**Consequências desastrosas**:
- Dados válidos no passado não podem ser reconstituídos
- Event sourcing quebra (eventos históricos falham replay)
- Migração de dados impossível sem "limpar" dados antigos
- Sistema para de funcionar quando regras mudam

## A Decisão

### Nossa Abordagem

Construtores são **privados**. A criação só acontece via **factory methods estáticos**:

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    // Propriedades
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // Construtor vazio - usado por RegisterNew para validação incremental
    private SimpleAggregateRoot()
    {
    }

    // Construtor completo - usado por CreateFromExistingInfo e Clone
    private SimpleAggregateRoot(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        string fullName,
        BirthDate birthDate
    ) : base(entityInfo)
    {
        FirstName = firstName;
        LastName = lastName;
        FullName = fullName;
        BirthDate = birthDate;
    }

    // Factory method para CRIAÇÃO DE NEGÓCIO
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        var instance = new SimpleAggregateRoot();

        // Valida TODOS os campos (operador &)
        bool isSuccess =
            instance.ChangeNameInternal(executionContext, input.FirstName, input.LastName)
            & instance.ChangeBirthDateInternal(executionContext, input.BirthDate);

        // Retorna null se qualquer validação falhar
        // ExecutionContext contém TODAS as mensagens de erro
        return isSuccess ? instance : null;
    }

    // Factory method para RECONSTITUTION (banco, eventos)
    public static SimpleAggregateRoot CreateFromExistingInfo(
        CreateFromExistingInfoInput input
    )
    {
        // NÃO valida - dados já foram validados no passado
        return new SimpleAggregateRoot(
            input.EntityInfo,
            input.FirstName,
            input.LastName,
            input.FullName,
            input.BirthDate
        );
    }
}
```

### Por Que Funciona Melhor

```csharp
// Uso - sem try/catch, feedback completo
var person = SimpleAggregateRoot.RegisterNew(
    executionContext,
    new RegisterNewInput("", "", birthDate) // Ambos vazios
);

if (person == null)
{
    // ExecutionContext contém TODAS as mensagens:
    // - "FirstName is required"
    // - "LastName is required"
    // Usuário corrige tudo de uma vez!
    return BadRequest(executionContext.Messages);
}

// person está garantidamente válido aqui
```

**Benefícios**:
1. **Feedback completo**: Todas as validações executam, todas as mensagens coletadas
2. **Sem exceções**: Validação de negócio não usa exceções (performance)
3. **Nomes expressivos**: `RegisterNew` vs `CreateFromExistingInfo` deixam a intenção clara
4. **Contexto disponível**: ExecutionContext traz tenant, usuário, TimeProvider
5. **Impossível criar inválido**: Construtores privados, factory é o único caminho

## Consequências

### Benefícios

- **Encapsulamento completo**: Construtores private + factory methods = impossível criar estado inválido
- **Reconstitution funciona**: Dados históricos carregam mesmo com regras diferentes
- **UX superior**: Usuário vê todos os erros de uma vez
- **Performance**: Sem overhead de exceções para casos esperados
- **Semântica clara**: Nome do factory method indica o propósito
- **Testabilidade**: Fácil mockar ExecutionContext para testes
- **Evoluibilidade**: Adicionar parâmetros ao Input sem quebrar assinatura
- **Event sourcing compatível**: Replay de eventos nunca falha por mudança de regras

### Trade-offs (Com Perspectiva)

- **Null check obrigatório**: Chamador deve verificar retorno null
- **Disciplina necessária**: Equipe deve entender por que construtores são privados

### Trade-offs Frequentemente Superestimados

**"Factory methods são mais verbosos que construtores"**

Sim, o factory method tem mais linhas. Mas compare o código TOTAL incluindo o uso:

```csharp
// Construtor com exceções - parece menor...
public Person(string firstName, string lastName)
{
    if (string.IsNullOrWhiteSpace(firstName))
        throw new ArgumentException("FirstName is required");
    if (string.IsNullOrWhiteSpace(lastName))
        throw new ArgumentException("LastName is required");
    FirstName = firstName;
    LastName = lastName;
}

// ...mas o CONSUMIDOR paga o preço:
try
{
    var person = new Person(input.FirstName, input.LastName);
    // usar person
}
catch (ArgumentException ex)
{
    // E se tiver múltiplos erros? Só vejo um
    // Preciso de try/catch em TODO lugar que cria Person
    return BadRequest(ex.Message);
}
```

Com factory method, o consumidor é mais simples:

```csharp
var person = Person.RegisterNew(context, input);
if (person == null)
    return BadRequest(context.Messages);  // TODOS os erros de uma vez
```

O "custo" do factory method é pago UMA VEZ na entidade. O benefício é colhido em TODOS os lugares que a usam.

**"Dois construtores é complexidade desnecessária"**

Os dois construtores existem por razões fundamentalmente diferentes:

```csharp
// Construtor VAZIO - permite validação incremental
private Person() { }
// Usado em: RegisterNew (valida campo por campo, coleta TODAS as mensagens)

// Construtor COMPLETO - atribuição direta, sem validação
private Person(EntityInfo info, string firstName, ...) { ... }
// Usado em: CreateFromExistingInfo (dados já validados no passado)
//           Clone (dados já validados na instância original)
```

Sem essa separação, você teria que escolher entre:
- Validar sempre (quebra reconstitution de dados históricos)
- Nunca validar (permite criar entidades inválidas)

**"Null check é verboso"**

O null check é explícito e seguro. Compare com a alternativa de exceções:

```csharp
// Com null check - fluxo claro e previsível
var person = Person.RegisterNew(context, input);
if (person == null)
{
    // Sei exatamente o que fazer aqui
    // context.Messages tem todos os detalhes
    return BadRequest(context.Messages);
}

// Com exceções - try/catch esconde o fluxo
try
{
    var person = new Person(input.FirstName, input.LastName);
}
catch (ArgumentException ex)
{
    // Qual campo falhou? Múltiplos falharam?
    // Preciso parsear a mensagem?
}
```

Além disso, o compilador C# com nullable reference types AVISA se você esquecer o null check. Exceções não têm essa proteção.

### Quando Usar Cada Factory Method

| Cenário | Factory Method | Valida? |
|---------|---------------|---------|
| Criar nova entidade (UI, API) | `RegisterNew` | ✅ Sim |
| Carregar do banco de dados | `CreateFromExistingInfo` | ❌ Não |
| Aplicar evento (event sourcing) | `CreateFromExistingInfo` | ❌ Não |
| Deserializar de cache | `CreateFromExistingInfo` | ❌ Não |
| Importar dados legados | `CreateFromExistingInfo` | ❌ Não |

## Fundamentação Teórica

### Padrões de Design Relacionados

**Factory Method Pattern (GoF)** - Este é literalmente o padrão sendo aplicado. Factory methods encapsulam a lógica de criação, permitindo:
- Retornar null quando criação falha (construtores não podem)
- Ter nomes expressivos (`RegisterNew` vs `CreateFromExistingInfo`)
- Evoluir a lógica de criação sem quebrar clientes

**Retorno Nullable vs Result Pattern** - Retornamos `null` para indicar falha, com mensagens coletadas no `ExecutionContext`.

Por que NÃO usamos `Result<T>`? Não é por "simplicidade" - há razões técnicas concretas:

1. **Compatibilidade com `yield return` e `IAsyncEnumerable<T>`**: Result Pattern força callbacks para transformações, quebrando a sintaxe fluente de generators.

2. **Evitar closures implícitas**: Com Result Pattern, seria necessário propagar contexto via callbacks:
   ```csharp
   // Result Pattern força callbacks e closures
   return result.Match(
       onSuccess: entity => ProcessEntity(entity, context), // closure sobre 'context'
       onFailure: errors => HandleErrors(errors, context)   // outra closure
   );
   ```

   Muitos desenvolvedores, acostumados com LINQ, criam closures sem entender as implicações (alocações, captura de variáveis, lifetime). Evitar essa armadilha é intencional.

3. **ExecutionContext já existe**: As mensagens precisam ser coletadas em algum lugar. Com Result Pattern, teríamos redundância (mensagens no Result E no contexto) ou teríamos que escolher um dos dois.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) dedica um capítulo inteiro a **Factories**:

> "When creation of an object, or an entire AGGREGATE, becomes complicated or reveals too much of the internal structure, FACTORIES provide encapsulation."
>
> *Quando a criação de um objeto, ou de um AGGREGATE inteiro, se torna complicada ou revela demais da estrutura interna, FACTORIES fornecem encapsulamento.*

E especificamente sobre separar criação de reconstitution:

> "A FACTORY reconstituting an object will handle it differently from one creating one from scratch. [...] The reconstituted object should not be validated."
>
> *Uma FACTORY reconstituindo um objeto vai tratá-lo diferentemente de uma criando do zero. [...] O objeto reconstituído não deve ser validado.*

Nossa separação `RegisterNew` (valida) vs `CreateFromExistingInfo` (não valida) segue exatamente essa orientação.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013):

> "Factories are used to create Aggregates. [...] The Factory encapsulates the knowledge of what it takes to properly create a valid Aggregate."
>
> *Factories são usadas para criar Aggregates. [...] A Factory encapsula o conhecimento do que é necessário para criar corretamente um Aggregate válido.*

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende que **funções devem fazer uma coisa só**. Construtores que validam E criam violam isso.

O princípio **"Don't Use Exceptions for Flow Control"** (Não Use Exceções para Controle de Fluxo) também se aplica:

> "Exceptions should be used for exceptional conditions. They should not be used as a mechanism for normal program flow."
>
> *Exceções devem ser usadas para condições excepcionais. Não devem ser usadas como mecanismo para fluxo normal do programa.*

Validação de input é **esperada**, não excepcional. Usar exceções para isso é anti-pattern.

### O Que o Clean Architecture Diz

Clean Architecture coloca **Entities** no centro, protegidas de detalhes externos. Factory methods estáticos na própria entidade mantêm a lógica de criação junto com a entidade, sem dependências externas.

Se usássemos uma Factory externa, teríamos que decidir em qual camada ela ficaria. Com factory methods estáticos, não há essa dúvida - a entidade é auto-suficiente.

### Outros Fundamentos

**Effective Java - Item 1** (Joshua Bloch):
> "Consider static factory methods instead of constructors."
>
> *Considere métodos factory estáticos ao invés de construtores.*

Bloch lista vantagens que aplicamos:
1. Têm nomes (RegisterNew é mais claro que `new Person()`)
2. Não precisam criar novo objeto (podemos retornar null)
3. Podem retornar subtipos (não usamos, mas é possível)
4. Podem variar o retorno baseado em parâmetros

**SOLID - Single Responsibility Principle (SRP)**:

O construtor tem UMA responsabilidade: inicializar campos. A validação é responsabilidade do factory method. Essa separação permite:
- Construtor usado em Clone/Reconstitution (sem validação)
- Factory method usado em criação de negócio (com validação)

**GRASP - Creator Pattern**:

GRASP sugere que a responsabilidade de criar um objeto deve estar com quem tem as informações necessárias. A própria entidade tem todas as informações sobre suas invariantes, então ela deve ter o factory method.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que exceções não devem ser usadas para controle de fluxo?"
- "Qual a diferença entre factory method e abstract factory?"
- "Como implementar factory methods que retornam Result<T> ao invés de T?"
- "Por que reconstitution de entidades não deve validar dados?"

### Leitura Recomendada

- [Effective Java - Item 1: Consider static factory methods instead of constructors](https://www.oreilly.com/library/view/effective-java/9780134686097/)
- [Domain-Driven Design - Factory Pattern](https://martinfowler.com/bliki/EvansClassification.html)
- [C# Exception Performance](https://mattwarren.org/2016/12/20/Why-Exceptions-should-be-Exceptional/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece a infraestrutura para factory methods (RegisterNew e CreateFromExistingInfo) e encapsula a lógica de validação com construtores privados |
| [EntityInfo](../../building-blocks/domain-entities/models/entity-info.md) | Modelo de metadados usado no construtor privado completo, contendo informações de identidade, tenant e versionamento |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - construtores privados e regras de encapsulamento
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Construtores DEVEM Ser Privados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Construtor Público com Validação Impede Reconstitution
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - RegisterNew factory method
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - CreateFromExistingInfo factory method
