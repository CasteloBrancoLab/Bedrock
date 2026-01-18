# DE-003: Imutabilidade Controlada (Clone-Modify-Return)

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um documento oficial, como uma certidão de nascimento. Quando você precisa corrigir um erro, o cartório não risca o documento original e escreve por cima. Em vez disso, emite uma **nova certidão** com a correção, mantendo o original arquivado para histórico.

Se modificássemos o original diretamente, teríamos problemas:
- Se a correção der errado no meio do processo, o documento ficaria parcialmente alterado
- Não haveria rastro do que era antes
- Múltiplas pessoas tentando corrigir ao mesmo tempo causariam confusão

Na programação, modificar objetos diretamente causa os mesmos problemas: estado inconsistente, dificuldade de rastrear mudanças, e conflitos de concorrência.

### O Problema Técnico

Quando um método modifica uma entidade diretamente (`this`), problemas ocorrem se a operação falhar no meio:

```csharp
public void Anonymize()
{
    FirstName = "Anonymous";  // ✅ Modificado
    LastName = "User";        // ✅ Modificado
    if (!SetBirthDate(...))   // ❌ Falha aqui
        throw new Exception();
    // PROBLEMA: FirstName e LastName JÁ mudaram!
    // Entidade está em estado inconsistente
}
```

A entidade fica em um estado parcialmente modificado - nem o original, nem o desejado. Isso viola a invariante de "estado sempre válido".

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos modifica entidades diretamente, confiando em transações de banco de dados para garantir consistência:

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }

    // Modifica this diretamente
    public void UpdateProfile(string firstName, string lastName, DateTime birthDate)
    {
        // Valida primeiro
        if (string.IsNullOrEmpty(firstName))
            throw new ArgumentException("FirstName required");

        // Modifica
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }
}

// Uso com Unit of Work
using var transaction = _db.BeginTransaction();
try
{
    person.UpdateProfile("Jane", "Doe", newBirthDate);
    _db.Save(person);
    transaction.Commit();
}
catch
{
    transaction.Rollback();
    // Mas 'person' em memória continua modificado!
}
```

### Por Que Não Funciona Bem

1. **Estado inconsistente em memória**: Mesmo com rollback no banco, o objeto em memória permanece modificado:

```csharp
person.FirstName = "John";  // Original

try
{
    person.UpdateProfile("Jane", "", invalidDate); // Falha
}
catch { }

// person.FirstName agora é "Jane"!
// O banco fez rollback, mas a memória não
```

2. **Validação parcial antes da modificação não resolve**:

```csharp
public void UpdateProfile(...)
{
    // Validar TUDO antes...
    Validate(firstName);
    Validate(lastName);
    Validate(birthDate);

    // ...depois modificar
    FirstName = firstName;
    LastName = lastName;
    BirthDate = birthDate;
}
// Problema: E se uma regra de validação dependesse do estado atual?
// Ex: "Nome só pode ser alterado uma vez por dia"
```

3. **Thread-safety inexistente**: Dois threads modificando a mesma entidade simultaneamente podem criar estados imprevisíveis.

4. **Difícil de testar**: Testes precisam restaurar o estado original após cada teste que modifica.

5. **Event sourcing incompatível**: Modificar in-place perde o histórico de mudanças.

## A Decisão

### Nossa Abordagem

Métodos públicos NUNCA modificam a instância atual. Em vez disso, seguem o padrão **Clone-Modify-Return**:

1. **CLONE** - Cria uma cópia da instância atual
2. **MODIFY** - Aplica as mudanças na cópia
3. **RETURN** - Retorna a cópia (sucesso) ou null (falha)

```csharp
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    public SimpleAggregateRoot? ChangeName(
        ExecutionContext executionContext,
        ChangeNameInput input
    )
    {
        // Clone-Modify-Return via RegisterChangeInternal
        // IMPORTANTE: handler DEVE ser static para evitar closures acidentais
        return RegisterChangeInternal<SimpleAggregateRoot, ChangeNameInput>(
            executionContext,
            instance: this,  // Instância original (não modificada)
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                // newInstance é o CLONE
                // this permanece intacto
                return newInstance.ChangeNameInternal(
                    executionContext,
                    input.FirstName,
                    input.LastName
                );
            }
        );
    }

    // Clone é a base do padrão
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
}
```

**Uso**:

```csharp
var person = SimpleAggregateRoot.RegisterNew(context, input);

// Tentativa de modificação
var updatedPerson = person.ChangeName(
    context,
    new ChangeNameInput("Jane", "Smith")
);

if (updatedPerson == null)
{
    // Validação falhou
    // 'person' continua EXATAMENTE como estava antes
    // Consultar context.Messages para saber o que falhou
}
else
{
    // 'updatedPerson' é a nova versão válida
    // 'person' continua existindo com os valores originais
}
```

### Por Que Funciona Melhor

1. **Atomicidade garantida**: Ou a operação completa com sucesso, ou nada muda

```csharp
// Se falhar no meio do processo...
var updated = person.ChangeName(context, input);

// ...person permanece exatamente como era
Console.WriteLine(person.FirstName); // Valor original, sempre
```

2. **Rollback automático**: Se qualquer validação falhar, o clone é descartado

```csharp
// Dentro de ChangeNameInternal
bool isSuccess =
    SetFirstName(executionContext, firstName)    // ✅ OK no clone
    & SetLastName(executionContext, lastName)    // ❌ Falha no clone
    & SetFullName(executionContext, fullName);   // ✅ Executa mesmo assim

// Clone é descartado, original intacto
```

3. **Thread-safety natural**: Cada thread trabalha com sua própria cópia

```csharp
// Thread 1
var v1 = person.ChangeName(context, input1);

// Thread 2 (simultâneo)
var v2 = person.ChangeName(context, input2);

// Ambos operam em clones diferentes
// 'person' original nunca foi tocado
// Sem race conditions
```

4. **Histórico preservado**: Fácil implementar event sourcing

```csharp
// Cada versão é uma instância separada
versions.Add(person);                           // v1
versions.Add(person.ChangeName(ctx, input1));   // v2
versions.Add(versions[1].ChangeBirthDate(...)); // v3
// Histórico completo mantido
```

5. **Testes determinísticos**: Estado original preservado entre assertions

```csharp
[Fact]
public void ChangeName_WithInvalidInput_ReturnsNull()
{
    var person = CreateValidPerson();
    var originalName = person.FirstName;

    var result = person.ChangeName(context, invalidInput);

    Assert.Null(result);
    Assert.Equal(originalName, person.FirstName); // Sempre passa
}
```

## Consequências

### Benefícios

- **Estado sempre consistente**: Nunca existe entidade parcialmente modificada
- **Rollback automático**: Falhas descartam o clone, original intacto
- **Thread-safety**: Cada operação trabalha em cópia independente
- **Facilita auditoria**: Versões anteriores podem ser mantidas
- **Event sourcing ready**: Cada modificação pode gerar evento
- **Testabilidade**: Estado previsível, fácil de verificar
- **Debug simplificado**: Breakpoints mostram ambas versões (original e clone)

### Trade-offs (Com Perspectiva)

- **API diferente**: Chamadores devem tratar retorno nullable
- **Curva de aprendizado**: Padrão menos comum que mutabilidade direta

### Trade-offs Frequentemente Superestimados

**"Cada modificação cria nova instância"**

Sim, mas para contextualizar:

```csharp
// Uma operação LINQ comum cria MUITO mais alocações:
var filtered = persons
    .Where(p => p.IsActive)      // Iterator + closure
    .Select(p => p.Name)         // Iterator + closure
    .OrderBy(n => n)             // Buffer interno
    .ToList();                   // Lista final + array redimensionado

// Clone de entidade: UMA alocação direta via construtor
var updated = person.ChangeName(context, input);
```

O clone é uma única alocação via construtor - operação que o runtime .NET executa bilhões de vezes por segundo em qualquer aplicação.

**"Clone tem custo de performance"**

O clone chama apenas um construtor com atribuições diretas:

```csharp
public override SimpleAggregateRoot Clone()
{
    return new SimpleAggregateRoot(
        EntityInfo,      // Cópia de referência
        FirstName,       // Cópia de referência (string é imutável)
        LastName,        // Cópia de referência
        FullName,        // Cópia de referência
        BirthDate        // Cópia de struct pequeno
    );
}
```

Para comparação, operações comuns que custam MAIS que este clone:
- `List<T>.Add()` quando precisa redimensionar o array interno
- `Dictionary<K,V>.Add()` quando precisa rehash
- Qualquer acesso a banco de dados (ordens de magnitude maior)
- Serialização JSON de um objeto pequeno
- Uma única chamada HTTP

**Quando o custo SERIA relevante**:
- Loops com milhões de iterações clonando a mesma entidade (cenário artificial)
- Entidades com dezenas de propriedades de tipos complexos (viola princípio de entidade focada)

Na prática, o custo do clone é imperceptível comparado a qualquer operação de I/O que a aplicação faz.

### Padrão de Uso da API

```csharp
// Padrão esperado pelos consumidores
var updated = person.ChangeName(context, input);
if (updated == null)
{
    // Tratar erro
    return;
}
// Continuar com 'updated'
```

## Fundamentação Teórica

### Padrões de Design Relacionados

**Prototype Pattern (GoF)** - O método `Clone()` é a implementação direta deste padrão. Permite criar cópias de objetos sem acoplar ao tipo concreto.

**Command Pattern (GoF) - variação** - Cada operação de modificação pode ser vista como um comando que, ao invés de modificar o objeto, retorna uma nova versão. Isso facilita undo/redo e event sourcing.

**Copy-on-Write (COW)** - Padrão de otimização onde cópias são feitas apenas quando necessário. Nossa implementação sempre copia (não é lazy), mas o princípio de "não modificar o original" é o mesmo.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) não prescreve imutabilidade para Entities, mas enfatiza **Value Objects imutáveis**:

> "An object that represents a descriptive aspect of the domain with no conceptual identity is called a VALUE OBJECT. VALUE OBJECTS are instantiated to represent elements of the design that we care about only for what they are, not who or which they are."
>
> *Um objeto que representa um aspecto descritivo do domínio sem identidade conceitual é chamado de VALUE OBJECT. VALUE OBJECTS são instanciados para representar elementos do design que nos importam apenas pelo que são, não por quem ou qual são.*

Nossa abordagem **estende** esse princípio para Entities: embora tenham identidade, o estado é tratado de forma similar a Value Objects (novas instâncias ao invés de mutação).

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) discute event sourcing, onde cada mudança gera um evento:

> "Event Sourcing ensures that all changes to application state are stored as a sequence of events."
>
> *Event Sourcing garante que todas as mudanças no estado da aplicação sejam armazenadas como uma sequência de eventos.*

Clone-Modify-Return é naturalmente compatível com event sourcing: cada nova instância representa o estado após aplicar um evento.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) defende **minimizar side effects**:

> "Side effects are lies. Your function promises to do one thing, but it also does other hidden things."
>
> *Efeitos colaterais são mentiras. Sua função promete fazer uma coisa, mas também faz outras coisas escondidas.*

Métodos que modificam `this` têm side effect implícito. Clone-Modify-Return torna a mudança **explícita**: o chamador recebe um novo objeto, não há modificação oculta.

O princípio **"Functions should do one thing"** (Funções devem fazer uma coisa só) também se aplica:
- Método mutável: valida E modifica E pode falhar parcialmente
- Clone-Modify-Return: cria nova versão OU retorna null (uma coisa por caminho)

### O Que o Clean Architecture Diz

Clean Architecture enfatiza que **Entities devem ser independentes de frameworks e detalhes de infraestrutura**.

Mutabilidade cria dependência implícita de mecanismos de sincronização (locks, transações). Imutabilidade controlada elimina essa dependência - a entidade funciona corretamente independente do contexto de concorrência.

### Outros Fundamentos

**Effective Java - Item 17** (Joshua Bloch):
> "Minimize mutability. [...] Immutable objects are simple. [...] Immutable objects are inherently thread-safe."
>
> *Minimize mutabilidade. [...] Objetos imutáveis são simples. [...] Objetos imutáveis são inerentemente thread-safe.*

Bloch argumenta que classes devem ser imutáveis a menos que haja razão forte para mutabilidade. Nossa "imutabilidade controlada" captura os benefícios (thread-safety, simplicidade) enquanto permite o padrão Entity do DDD.

**Programação Funcional**:

O padrão Clone-Modify-Return é essencialmente **programação funcional aplicada a OOP**:
- Funções puras (sem side effects)
- Dados imutáveis
- Transformações que retornam novos valores

Linguagens como F#, Scala, e Haskell usam esse padrão nativamente. C# Records com `with` expressions são a evolução da linguagem nessa direção.

**CQRS (Command Query Responsibility Segregation)**:

Em CQRS, comandos (que modificam) são separados de queries (que leem). Clone-Modify-Return reforça essa separação:
- O método de modificação é claramente um "comando" (retorna nova entidade)
- Propriedades são claramente "queries" (leitura sem modificação)

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre imutabilidade total e imutabilidade controlada?"
- "Como implementar copy-on-write em C# de forma eficiente?"
- "Por que linguagens funcionais preferem imutabilidade?"
- "Como o padrão Clone-Modify-Return se relaciona com event sourcing?"
- "Quais são as implicações de GC para objetos imutáveis de curta duração?"

### Leitura Recomendada

- [Effective Java - Item 17: Minimize mutability](https://www.oreilly.com/library/view/effective-java/9780134686097/)
- [C# Records and Immutability](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records)
- [Domain-Driven Design - Value Objects](https://martinfowler.com/bliki/ValueObject.html)
- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Implementa a infraestrutura para Clone-Modify-Return através do método RegisterChangeInternal e do padrão de clonagem |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Clone-Modify-Return Pattern no método ChangeName
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_ANTIPATTERN: Mutabilidade Direta
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - método Clone
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Usar Operador & em Métodos *Internal
