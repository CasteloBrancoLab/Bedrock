# DE-004: Estado Inválido Nunca Existe na Memória

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um cofre de banco que só abre quando você digita a senha correta. Agora imagine um cofre "flexível" que abre primeiro, deixa você entrar, e só depois verifica se a senha estava certa. Se estivesse errada... bem, você já está dentro.

Muitos sistemas funcionam assim: criam o objeto primeiro, depois validam. O problema é que entre a criação e a validação, o objeto inválido existe na memória - e pode escapar para outras partes do sistema antes de ser validado.

### O Problema Técnico

Código que permite estado inválido temporário cria brechas para bugs:

```csharp
// Abordagem perigosa - objeto inválido existe temporariamente
var person = new Person();           // Estado inválido: FirstName é null
person.FirstName = "";               // Estado inválido: FirstName é vazio
person.FirstName = "Jo";             // Estado inválido: muito curto (min 3)
person.FirstName = "John";           // Finalmente válido

// MAS: e se alguém fizer isso?
var person = new Person();
_repository.Save(person);            // 💥 Salvou objeto inválido!
```

O objeto existiu em estado inválido por tempo suficiente para ser usado incorretamente. Não há garantia de que o código chamador validará antes de usar.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos usa uma das seguintes abordagens:

**1. Validação manual pelo chamador**:

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// Chamador deve lembrar de validar
var person = new Person { FirstName = "", LastName = "" };

var validator = new PersonValidator();
var result = validator.Validate(person);

if (!result.IsValid)
{
    // Tratar erros...
    // Mas 'person' inválido já existe na memória!
}
```

**2. Validação lazy com IsValid()**:

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(FirstName)
            && !string.IsNullOrEmpty(LastName);
    }
}

// Chamador pode ignorar IsValid() completamente
var person = new Person();  // Inválido, mas compila
DoSomething(person);        // Usa objeto inválido
```

**3. Exceções no setter**:

```csharp
public class Person
{
    private string _firstName;
    public string FirstName
    {
        get => _firstName;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Required");
            _firstName = value;
        }
    }
}

// Problema 1: O que acontece entre new e set?
var person = new Person();  // _firstName é null - inválido!

// Problema 2: Uma exceção por vez
person.FirstName = "";  // Exceção
person.LastName = "";   // Nunca executa - usuário não sabe que também está errado
```

### Por Que Não Funciona Bem

1. **Janela de vulnerabilidade**: Entre criação e validação, o objeto pode ser usado incorretamente

2. **Dependência de disciplina**: Requer que TODO código que cria objetos siga o protocolo de validação

3. **Impossível garantir em compile-time**: O compilador não impede uso de objeto inválido

4. **Propagação silenciosa**: Objeto inválido pode ser passado para outros métodos, armazenado em coleções, persistido no banco

5. **Debug difícil**: Quando o erro aparece, pode estar longe do ponto onde o objeto foi criado inválido

```csharp
// O bug está aqui...
var person = CreatePersonFromRequest(request);  // Retorna Person inválido

// ...mas o erro aparece aqui, muito depois
await _emailService.SendWelcome(person);  // NullReferenceException em person.Email
```

## A Decisão

### Nossa Abordagem

Estado inválido **nunca existe na memória**. Isso é garantido por design através de:

1. **Construtores privados** - Ninguém pode criar instância diretamente
2. **Factory methods que validam** - `RegisterNew` só retorna se válido
3. **Retorno nullable** - Factory retorna `null` se validação falhar
4. **Clone-Modify-Return** - Modificações criam nova instância validada

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // Construtores privados - impossível criar externamente
    private SimpleAggregateRoot() { }

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

    // ÚNICO caminho para criar: factory method que valida
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        var instance = new SimpleAggregateRoot();

        bool isSuccess =
            instance.ChangeNameInternal(executionContext, input.FirstName, input.LastName)
            & instance.ChangeBirthDateInternal(executionContext, input.BirthDate);

        // Só retorna se TODAS as validações passaram
        // Caso contrário, retorna null - objeto inválido nunca "escapa"
        return isSuccess ? instance : null;
    }
}
```

**Uso**:

```csharp
var person = SimpleAggregateRoot.RegisterNew(context, input);

if (person == null)
{
    // Não existe Person inválido - apenas null
    // Impossível usar person.FirstName aqui - compilador impede
    return BadRequest(context.Messages);
}

// Aqui, person é GARANTIDAMENTE válido
// O compilador sabe que person não é null
await _repository.Save(person);  // Seguro
```

### Por Que Funciona Melhor

1. **Garantia em compile-time**: Compilador força null-check antes de usar

```csharp
var person = SimpleAggregateRoot.RegisterNew(context, input);
person.FirstName;  // ⚠️ Warning: person pode ser null

if (person != null)
{
    person.FirstName;  // ✅ OK - compilador sabe que não é null
}
```

2. **Impossível ignorar validação**: Não existe caminho para criar objeto sem validar

```csharp
// Todas essas tentativas FALHAM em compile-time:
var p1 = new SimpleAggregateRoot();           // ❌ Construtor é private
var p2 = new SimpleAggregateRoot("John", ..); // ❌ Construtor é private
SimpleAggregateRoot p3;
p3.FirstName = "John";                        // ❌ Variável não inicializada
```

3. **Feedback completo**: Operador `&` garante que TODAS as validações executam

```csharp
bool isSuccess =
    SetFirstName(context, input.FirstName)  // Falha: muito curto
    & SetLastName(context, input.LastName)  // Falha: vazio
    & SetBirthDate(context, input.BirthDate); // Falha: data futura

// context.Messages contém TODOS os 3 erros
// Usuário corrige tudo de uma vez
```

4. **Modificações também são seguras**: Clone-Modify-Return mantém a garantia

```csharp
var updated = person.ChangeName(context, new ChangeNameInput("", ""));

if (updated == null)
{
    // 'person' original continua válido
    // Não existe 'updated' inválido
}
```

## Consequências

### Benefícios

- **Invariante forte**: Se você tem uma referência não-null, ela é válida
- **Menos bugs**: Impossível usar objeto inválido por acidente
- **Debug simplificado**: Se há erro de validação, está no ponto de criação
- **Código defensivo desnecessário**: Não precisa validar em cada método que recebe a entidade
- **Documentação implícita**: A API comunica que validação é obrigatória

### Trade-offs (Com Perspectiva)

- **Null-check obrigatório**: Chamador deve verificar retorno de `RegisterNew`
- **Mudança de paradigma**: Desenvolvedores acostumados com `new` + validação separada precisam adaptar

### Trade-offs Frequentemente Superestimados

**"Null-check é verboso"**

Compare a verbosidade real:

```csharp
// Abordagem tradicional (parece menor, mas esconde complexidade)
var person = new Person(input.FirstName, input.LastName);
var result = _validator.Validate(person);
if (!result.IsValid)
{
    // Tratar erros...
    // Mas person inválido ainda existe!
    // Precisa garantir que não escape
}

// Nossa abordagem (verbosidade similar, mais segura)
var person = Person.RegisterNew(context, input);
if (person == null)
{
    return BadRequest(context.Messages);
}
// person é garantidamente válido
```

A verbosidade é similar, mas nossa abordagem tem garantia em compile-time.

**"Preciso de objeto parcialmente construído para testes"**

Para testes, use `CreateFromExistingInfo` que não valida:

```csharp
// Em testes, você pode criar com qualquer estado
var person = SimpleAggregateRoot.CreateFromExistingInfo(
    new CreateFromExistingInfoInput(
        entityInfo,
        firstName: "",  // Inválido - OK para teste
        lastName: "",   // Inválido - OK para teste
        birthDate
    )
);
```

`CreateFromExistingInfo` existe para reconstitution (banco, eventos), e testes são um caso válido de "dados que já existem".

**"E se eu precisar construir em etapas?"**

Use um Builder separado que não é a entidade:

```csharp
// Builder é mutável e pode ter estado inválido
var builder = new PersonBuilder()
    .WithFirstName("John")
    .WithLastName("Doe")
    .WithBirthDate(birthDate);

// Validação acontece na conversão para entidade
var person = builder.Build(context);  // Retorna Person? (nullable)
```

O Builder pode ter estado inválido - a entidade nunca.

## Fundamentação Teórica

### Padrões de Design Relacionados

**Factory Method Pattern (GoF)** - Factory methods encapsulam criação e garantem que só objetos válidos são retornados.

**Null Object Pattern (variação)** - Retornamos `null` para indicar falha ao invés de objeto inválido. O "objeto nulo" aqui é literalmente `null`, forçando o chamador a tratar o caso de falha.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) enfatiza que **Aggregates protegem invariantes**:

> "Invariants, which are consistency rules that must be maintained whenever data changes, will involve relationships between members of the AGGREGATE. Any rule that spans AGGREGATES will not be expected to be up-to-date at all times."
>
> *Invariantes, que são regras de consistência que devem ser mantidas sempre que dados mudam, envolverão relacionamentos entre membros do AGGREGATE. Qualquer regra que cruza AGGREGATES não deve ser esperada como atualizada o tempo todo.*

O princípio "estado inválido nunca existe" é a implementação mais forte possível de proteção de invariantes - não apenas "invariantes são verificadas", mas "invariantes são impossíveis de violar".

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende o princípio de **fail fast**:

> "If you are going to fail, fail fast. The longer you wait to report a failure, the more code must be suspect."
>
> *Se você vai falhar, falhe rápido. Quanto mais você espera para reportar uma falha, mais código deve ser suspeito.*

Nossa abordagem é "fail at creation" - mais rápido que "fail fast". O objeto inválido nem chega a existir.

### O Que o Clean Architecture Diz

Clean Architecture coloca **Entities no centro**, como as regras de negócio mais importantes. Se as Entities podem existir em estado inválido, as regras de negócio já estão comprometidas no núcleo do sistema.

Garantir que Entities são sempre válidas significa que o núcleo do sistema é sempre consistente.

### Outros Fundamentos

**Making Illegal States Unrepresentable** (Yaron Minsky):

> "Make illegal states unrepresentable."
>
> *Faça estados ilegais irrepresentáveis.*

Este princípio de programação funcional é exatamente o que implementamos. O sistema de tipos (construtor privado + factory nullable) torna impossível representar um `Person` inválido.

**Parse, Don't Validate** (Alexis King):

> "The difference between validation and parsing is that parsing gives you a stronger type on success."
>
> *A diferença entre validação e parsing é que parsing te dá um tipo mais forte no sucesso.*

`RegisterNew` é parsing: transforma dados não-tipados (`RegisterNewInput`) em dados fortemente tipados (`SimpleAggregateRoot`). Se falha, retorna `null` - não existe tipo "Person inválido".

**Effective Java - Item 1** (Joshua Bloch):

Static factory methods permitem retornar `null` ou tipo diferente, o que construtores não podem. Isso é fundamental para a garantia de "só retorna se válido".

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "O que significa 'make illegal states unrepresentable'?"
- "Qual a diferença entre 'parse, don't validate' e validação tradicional?"
- "Como linguagens funcionais como F# implementam tipos que não podem ser inválidos?"
- "Por que fail fast é melhor que fail eventually?"

### Leitura Recomendada

- [Parse, Don't Validate](https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/) - Alexis King
- [Making Illegal States Unrepresentable](https://blog.janestreet.com/effective-ml-revisited/) - Yaron Minsky
- [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/) - Scott Wlaschin
- [Effective Java - Item 1: Static Factory Methods](https://www.oreilly.com/library/view/effective-java/9780134686097/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Implementa o padrão de factory methods que garantem que estado inválido nunca existe, retornando null em caso de falha de validação |
| [ExecutionContext](../../building-blocks/core/execution-contexts/execution-context.md) | Coleta mensagens de validação durante a criação/modificação de entidades, permitindo feedback completo sem criar estado inválido |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Princípios Fundamentais incluindo "Estado inválido NUNCA existe"
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_ANTIPATTERN: O Que Não Fazer
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - RegisterNew que só retorna se válido
