# DE-017: Separação RegisterNew vs CreateFromExistingInfo

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **cartório de registro de nascimento**:

**Cenário 1 - Registro de NOVO nascimento**:
Um bebê nasceu hoje. O cartório verifica se todos os dados estão corretos conforme as **regras atuais**: nome com no mínimo 2 caracteres, CPF válido, data de nascimento não futura, etc. Se algo estiver errado, o registro é recusado.

**Cenário 2 - Emissão de segunda via**:
Uma pessoa de 80 anos precisa de segunda via da certidão. O cartório **não reavalia** se o nome dela (registrado em 1944) atende às regras de 2024. O registro original é respeitado exatamente como foi feito.

Em software, precisamos da mesma separação: **criar novos registros** com validação atual, **reconstituir registros existentes** sem revalidação.

---

### O Problema Técnico

Muitos sistemas usam um único método de criação que sempre valida:

```csharp
// ❌ ANTIPATTERN: Um único método que SEMPRE valida
public static Person Create(string firstName, string lastName, BirthDate birthDate)
{
    if (firstName.Length < 3)  // Regra de 2024
        throw new ValidationException("Nome muito curto");

    if (firstName.Length > 50) // Regra de 2024
        throw new ValidationException("Nome muito longo");

    return new Person(firstName, lastName, birthDate);
}
```

**Problemas que surgem quando as regras mudam**:

```csharp
// 2020: MaxLength era 100, pessoa cadastrada com nome de 80 caracteres
// 2024: MaxLength mudou para 50

// Ao carregar do banco de dados:
var dto = database.Query("SELECT * FROM Persons WHERE Id = @id");
var person = Person.Create(dto.FirstName, dto.LastName, dto.BirthDate);
// ❌ EXCEÇÃO! Nome com 80 caracteres é "muito longo" pelas regras de 2024

// Event Sourcing - replay de eventos:
foreach (var @event in eventStore.GetEvents(aggregateId))
{
    aggregate.Apply(@event); // Chama Create internamente
    // ❌ Eventos de 2020 FALHAM com regras de 2024!
}
```

**Consequências**:
- Dados válidos no passado não podem ser reconstituídos
- Event sourcing quebra (eventos históricos falham replay)
- Migração de dados impossível sem "limpar" dados antigos
- Sistema para de funcionar quando regras mudam

## A Decisão

### Nossa Abordagem

**Dois métodos distintos** para criação e reconstitution:

```csharp
public sealed class SimpleAggregateRoot
{
    // ✅ Para dados NOVOS - valida com regras ATUAIS
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        // Valida TUDO com regras atuais
        // Retorna null se inválido
        // Coleta TODAS as mensagens de erro
    }

    // ✅ Para dados EXISTENTES - NÃO valida
    public static SimpleAggregateRoot CreateFromExistingInfo(
        CreateFromExistingInfoInput input
    )
    {
        // NÃO valida - assume dados validados no passado
        // NUNCA retorna null
        // Usado para reconstitution
    }
}
```

### RegisterNew - Para Dados Novos

```csharp
public static SimpleAggregateRoot? RegisterNew(
    ExecutionContext executionContext,
    RegisterNewInput input
)
{
    var instance = new SimpleAggregateRoot();

    // Chama métodos *Internal que fazem validação
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
        }
    );
}
```

**Características**:
- Valida com regras **atuais e contextuais** (podem variar por tenant)
- Retorna `null` se validação falhar
- Coleta **todas** as mensagens de erro via ExecutionContext
- Requer ExecutionContext para contexto de validação
- Gerencia automaticamente: ID, auditoria, eventos de criação

**Quando usar**:
- Cadastro de novos registros via API/UI
- Input de usuário
- Criação programática de novas entidades

### CreateFromExistingInfo - Para Reconstitution

```csharp
public static SimpleAggregateRoot CreateFromExistingInfo(
    CreateFromExistingInfoInput input
)
{
    // Direto ao construtor privado - SEM validação
    return new SimpleAggregateRoot(
        input.EntityInfo,
        input.FirstName,
        input.LastName,
        input.FullName,
        input.BirthDate
    );
}
```

**Características**:
- **NÃO valida** dados - assume validados no passado
- **Nunca retorna null** - dados existentes são sempre "válidos"
- Não requer ExecutionContext
- Apenas reconstrói a entidade a partir de dados persistidos

**Quando usar**:
- Repositories carregando do banco de dados
- Event handlers aplicando eventos (replay)
- Cache/deserialização
- Importação de dados legados

### Exemplos de Uso

**1. Controller criando nova entidade**:

```csharp
[HttpPost]
public async Task<IActionResult> CreatePerson(CreatePersonRequest request)
{
    var context = new ExecutionContext(_timeProvider);

    // ✅ RegisterNew - valida com regras atuais
    var person = SimpleAggregateRoot.RegisterNew(context, new RegisterNewInput(
        firstName: request.FirstName,
        lastName: request.LastName,
        birthDate: request.BirthDate
    ));

    if (person == null)
        return BadRequest(context.Messages);

    await _repository.SaveAsync(person);
    return Ok(person.EntityInfo.Id);
}
```

**2. Repository carregando do banco**:

```csharp
public async Task<SimpleAggregateRoot?> GetByIdAsync(Guid id)
{
    var dto = await _database.QuerySingleOrDefaultAsync<PersonDto>(
        "SELECT * FROM Persons WHERE Id = @Id",
        new { Id = id }
    );

    if (dto == null)
        return null;

    // ✅ CreateFromExistingInfo - NÃO revalida
    return SimpleAggregateRoot.CreateFromExistingInfo(
        new CreateFromExistingInfoInput(
            entityInfo: dto.EntityInfo,
            firstName: dto.FirstName,
            lastName: dto.LastName,
            fullName: dto.FullName,
            birthDate: dto.BirthDate
        )
    );
}
```

**3. Event Sourcing - replay de eventos**:

```csharp
public class PersonAggregate
{
    private SimpleAggregateRoot? _state;

    public void Apply(PersonCreatedEvent @event)
    {
        // ✅ CreateFromExistingInfo - eventos históricos sempre funcionam
        _state = SimpleAggregateRoot.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo: @event.EntityInfo,
                firstName: @event.FirstName,
                lastName: @event.LastName,
                fullName: @event.FullName,
                birthDate: @event.BirthDate
            )
        );
    }

    public void Apply(PersonNameChangedEvent @event)
    {
        // Mesmo para alterações históricas
        _state = _state!.ApplyNameChange(@event.FirstName, @event.LastName);
    }
}
```

**4. Cache/Serialização**:

```csharp
public class PersonCacheService
{
    public async Task<SimpleAggregateRoot?> GetFromCacheAsync(Guid id)
    {
        var json = await _cache.GetStringAsync($"person:{id}");
        if (json == null)
            return null;

        var dto = JsonSerializer.Deserialize<PersonCacheDto>(json);

        // ✅ CreateFromExistingInfo - dados do cache já eram válidos
        return SimpleAggregateRoot.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo: dto!.EntityInfo,
                firstName: dto.FirstName,
                lastName: dto.LastName,
                fullName: dto.FullName,
                birthDate: dto.BirthDate
            )
        );
    }
}
```

### Comparação

| Aspecto | RegisterNew | CreateFromExistingInfo |
|---------|-------------|------------------------|
| **Propósito** | Criar novas entidades | Reconstituir existentes |
| **Validação** | Sim (regras atuais) | Não |
| **Retorno** | `T?` (nullable) | `T` (nunca null) |
| **ExecutionContext** | Obrigatório | Não necessário |
| **Uso típico** | API, UI, novos cadastros | Repository, Event Sourcing, Cache |
| **Falha possível** | Sim (validação) | Não (dados já validados) |

### Benefícios

1. **Evolução de regras sem quebrar histórico**: Regras podem mudar sem invalidar dados existentes
2. **Event Sourcing funciona**: Eventos históricos sempre podem ser reaplicados
3. **Compliance**: Dados históricos preservados exatamente como foram criados (LGPD/GDPR/HIPAA)
4. **Temporal queries**: Possível consultar estado em momento passado
5. **Migração de dados**: Dados legados podem ser importados mesmo com regras diferentes
6. **Separação clara de responsabilidades**: Nome do método indica a intenção

### Trade-offs (Com Perspectiva)

- **Dois métodos ao invés de um**: Complexidade adicional na API da entidade
  - **Mitigação**: Nomes claros (`RegisterNew` vs `CreateFromExistingInfo`) comunicam a intenção

### Trade-offs Frequentemente Superestimados

**"CreateFromExistingInfo pode ser usado para burlar validação"**

Na verdade, o uso incorreto é facilmente detectável em code review:

```csharp
// ❌ Uso incorreto - óbvio em code review
var person = SimpleAggregateRoot.CreateFromExistingInfo(
    new CreateFromExistingInfoInput(
        entityInfo: EntityInfo.CreateNew(), // 🚨 EntityInfo.CreateNew() em CreateFromExistingInfo?
        firstName: userInput.FirstName,     // 🚨 Input de usuário?
        ...
    )
);
```

Além disso, convenções de projeto e analyzers podem detectar uso incorreto automaticamente.

**"Deveria validar sempre para garantir consistência"**

Validar sempre é uma armadilha. Quando regras mudam:

```csharp
// 2020: Cliente cadastrado com email opcional (null)
// 2024: Email agora é obrigatório

// Com validação sempre:
var customer = Customer.Create(dto); // ❌ Falha! Email é null

// O que fazer? Inventar um email fake? Deletar o cliente?
// Ambas as opções são PIORES que simplesmente carregar o dado como está.
```

## Fundamentação Teórica

### Event Sourcing

Greg Young, criador do Event Sourcing moderno, sobre imutabilidade de eventos:

> "Events are facts. Facts don't change. The interpretation of facts may change, but the facts themselves are immutable."
>
> *Eventos são fatos. Fatos não mudam. A interpretação dos fatos pode mudar, mas os fatos em si são imutáveis.*

Revalidar eventos históricos viola este princípio fundamental.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre criação de entidades:

> "A FACTORY handles the beginning of an object's life. [...] Complex assemblies, especially of AGGREGATES, call for FACTORIES."
>
> *Uma FACTORY cuida do início da vida de um objeto. [...] Montagens complexas, especialmente de AGGREGATES, pedem FACTORIES.*

`RegisterNew` e `CreateFromExistingInfo` são **factories especializadas** - cada uma para um ciclo de vida diferente do objeto.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre reconstitution:

> "When Aggregates are reconstituted from a persistence store, their invariants need not be checked because they were validated before they were originally persisted."
>
> *Quando Aggregates são reconstituídos de um repositório de persistência, suas invariantes não precisam ser verificadas porque foram validadas antes de serem originalmente persistidos.*

Esta citação fundamenta diretamente nossa separação de métodos.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre funções com um único propósito:

> "Functions should do one thing. They should do it well. They should do it only."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso.*

`RegisterNew` faz uma coisa: criar e validar novas entidades. `CreateFromExistingInfo` faz uma coisa: reconstituir entidades existentes. Um método único que faz ambas viola SRP.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre separação de responsabilidades:

> "The Single Responsibility Principle (SRP) states that a module should have one, and only one, reason to change."
>
> *O Princípio de Responsabilidade Única (SRP) afirma que um módulo deve ter uma, e apenas uma, razão para mudar.*

`RegisterNew` muda quando regras de validação mudam. `CreateFromExistingInfo` muda quando a estrutura de dados muda. Razões diferentes = métodos diferentes.

### Reconstitution Pattern

O padrão de Reconstitution (ou Rehydration) é amplamente usado em DDD para separar a criação de novas entidades da reconstrução de entidades existentes a partir de dados persistidos.

### Temporal Data

Sistemas que lidam com dados temporais precisam preservar o estado exato em cada ponto no tempo. Revalidar com regras atuais distorceria o histórico.

## Antipadrões Documentados

### Antipadrão 1: Método Único que Sempre Valida

```csharp
// ❌ Um único método para tudo
public static Person Create(string firstName, string lastName)
{
    // Sempre valida - quebra quando regras mudam
    if (firstName.Length > 50)
        throw new ValidationException("Nome muito longo");

    return new Person(firstName, lastName);
}
```

### Antipadrão 2: Flag "skipValidation"

```csharp
// ❌ Flag booleana para pular validação
public static Person Create(string firstName, bool skipValidation = false)
{
    if (!skipValidation)
    {
        // Validação
    }
    return new Person(firstName);
}

// Uso obscuro - o que significa "true" aqui?
var person = Person.Create(dto.FirstName, true);
```

### Antipadrão 3: Validação Condicional por Tipo de Input

```csharp
// ❌ Lógica condicional complexa dentro do mesmo método
public static Person Create(PersonInput input)
{
    if (input is ExistingPersonInput existing)
    {
        // Não valida
    }
    else if (input is NewPersonInput newPerson)
    {
        // Valida
    }
    // Confuso e difícil de manter
}
```

## Decisões Relacionadas

- [DE-002](./DE-002-construtores-privados-com-factory-methods.md) - Construtores privados
- [DE-004](./DE-004-estado-invalido-nunca-existe-na-memoria.md) - Estado inválido nunca existe
- [DE-018](./DE-018-reconstitution-nao-valida-dados.md) - Reconstitution não valida
- [DE-019](./DE-019-input-objects-pattern.md) - Input Objects Pattern

## Building Blocks Relacionados

- **[Id](../../building-blocks/core/ids/id.md)** - Documentação completa sobre identificadores únicos baseados em UUIDv7, usados no EntityInfo durante RegisterNew.
- **[CustomTimeProvider](../../building-blocks/core/time-providers/custom-time-provider.md)** - TimeProvider customizável usado para gerar timestamps de auditoria no EntityInfo durante RegisterNew.

## Leitura Recomendada

- [Event Sourcing - Martin Fowler](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Implementing Domain-Driven Design - Vaughn Vernon](https://www.informit.com/store/implementing-domain-driven-design-9780321834577)
- [Versioning in an Event Sourced System - Greg Young](https://leanpub.com/esversioning)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Implementa a separação entre RegisterNew (com validação) e CreateFromExistingInfo (sem validação) |
| [EntityInfo](../../building-blocks/domain-entities/models/entity-info.md) | Modelo usado no CreateFromExistingInfo para reconstituir entidades com seus metadados completos |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - SOLUÇÃO: Separar criação de reconstitution
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Separação de Responsabilidades
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - RegisterNew
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: CreateFromExistingInfo NÃO Valida
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - CreateFromExistingInfo
