# DE-018: Reconstitution Não Valida Dados

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **museu de história**:

**Cenário problemático**:
O museu recebe uma carta de 1800 escrita em português arcaico. O curador diz: "Esta carta não segue as regras ortográficas atuais do Acordo Ortográfico de 1990. Vamos corrigi-la ou rejeitá-la."

**Cenário correto**:
O museu preserva a carta **exatamente como foi escrita**. A ortografia de 1800 era válida em 1800. Alterar seria falsificar história.

Em software, dados históricos são como artefatos de museu. Revalidá-los com regras atuais é como "corrigir" a ortografia de uma carta histórica.

---

### O Problema Técnico

Quando regras de validação mudam, dados históricos podem se tornar "inválidos" pelas novas regras:

```csharp
// Evolução das regras ao longo do tempo:

// 2020: Sistema aceita nomes de até 100 caracteres
public static int FirstNameMaxLength = 100;
// Usuário cadastra: "Maria Eduarda dos Santos Ferreira" (35 chars) ✅

// 2022: Integração com sistema legado limita a 50 caracteres
public static int FirstNameMaxLength = 50;
// Mesmo nome ainda válido ✅

// 2024: Nova política de privacidade limita a 20 caracteres
public static int FirstNameMaxLength = 20;
// Nome de 35 chars agora é "inválido"? ❌
```

Se reconstitution validar com regras atuais:

```csharp
// Repository tenta carregar do banco:
var dto = await _db.QueryAsync("SELECT * FROM Persons WHERE Id = @id");

// ❌ Se CreateFromExistingInfo validasse:
var person = Person.CreateFromExistingInfo(dto);
// EXCEÇÃO! "Maria Eduarda dos Santos Ferreira" tem 35 chars, max é 20!

// O que fazer agora?
// - Truncar o nome? ❌ Perda de dados, possível problema legal
// - Deletar o registro? ❌ Perda de dados, violação de auditoria
// - Atualizar o banco? ❌ Quem autoriza? E o histórico?
```

## A Decisão

### Nossa Abordagem

**CreateFromExistingInfo NUNCA valida dados**:

```csharp
public static SimpleAggregateRoot CreateFromExistingInfo(
    CreateFromExistingInfoInput input
)
{
    // ✅ Direto ao construtor - SEM validação
    return new SimpleAggregateRoot(
        input.EntityInfo,
        input.FirstName,
        input.LastName,
        input.FullName,
        input.BirthDate
    );
}
```

### Razões Técnicas

#### 1. Evolução de Regras de Negócio

Regras mudam constantemente. Dados criados com regras antigas permanecem válidos:

```csharp
// Linha do tempo de uma entidade Person:

// 2020-01-15: Criada com regras de 2020
var person = Person.RegisterNew(context, new RegisterNewInput(
    firstName: "João",
    email: null  // Email era opcional em 2020
));

// 2022-06-01: Regra muda - email agora é obrigatório
PersonMetadata.ChangeEmailMetadata(isRequired: true, ...);

// 2024-03-20: Repository carrega a pessoa de 2020
var person = await _repository.GetByIdAsync(personId);
// ✅ Funciona! Email null é preservado como estava

// Se validasse, o sistema quebraria:
// ❌ "Email é obrigatório" - mas em 2020 não era!
```

#### 2. Event Sourcing

Em Event Sourcing, eventos são **fatos imutáveis**. Replay deve sempre funcionar:

```csharp
// Stream de eventos de um aggregate:
// 1. PersonCreated (2020-01-15) - firstName: "Ana", lastName: "Silva"
// 2. PersonNameChanged (2021-03-20) - firstName: "Ana Maria"
// 3. PersonEmailAdded (2022-06-01) - email: "ana@email.com"

public class PersonAggregate
{
    private Person _state;

    public void ReplayEvents(IEnumerable<DomainEvent> events)
    {
        foreach (var @event in events)
        {
            // ✅ Cada evento é aplicado SEM revalidação
            Apply(@event);
        }
    }

    private void Apply(PersonCreatedEvent @event)
    {
        // Se validasse com regras de 2024, eventos de 2020 falhariam
        _state = Person.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(@event.Data)
        );
    }
}
```

#### 3. Compliance e Auditoria

Regulamentações exigem preservação exata de dados históricos:

```csharp
// LGPD/GDPR: Direito de acesso aos dados
// Usuário solicita: "Quero ver todos os meus dados como estavam em 2021"

public async Task<PersonSnapshot> GetHistoricalDataAsync(Guid personId, DateTime asOf)
{
    // Carrega eventos até a data especificada
    var events = await _eventStore.GetEventsAsync(personId, upTo: asOf);

    // Reconstrói estado EXATO daquela época
    var person = ReplayEvents(events);

    // ✅ Dados preservados exatamente como eram
    // ❌ Se validasse, dados poderiam ser rejeitados ou alterados
    return person.ToSnapshot();
}
```

#### 4. Temporal Queries

Consultas temporais precisam do estado exato em cada momento:

```csharp
// Relatório de auditoria: "Qual era o endereço do cliente em 01/2023?"

public async Task<Address> GetAddressAtDateAsync(Guid customerId, DateTime date)
{
    var snapshot = await _temporalRepository.GetSnapshotAsync(customerId, date);

    // ✅ Retorna endereço EXATO daquela data
    // Mesmo que formato de CEP tenha mudado, endereço é preservado
    return Customer.CreateFromExistingInfo(snapshot).Address;
}
```

#### 5. Migração de Dados

Dados legados podem não atender regras atuais:

```csharp
// Migração de sistema legado de 15 anos

public async Task MigrateFromLegacySystemAsync()
{
    var legacyRecords = await _legacyDb.GetAllCustomersAsync();

    foreach (var legacy in legacyRecords)
    {
        // ✅ Importa dados COMO ESTÃO
        var customer = Customer.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo: EntityInfo.CreateForMigration(legacy.Id, legacy.CreatedAt),
                name: legacy.Name,  // Pode ter 200 chars (regra atual: 100)
                email: legacy.Email // Pode ser null (regra atual: obrigatório)
            )
        );

        await _repository.SaveAsync(customer);
    }

    // Depois da migração, NOVOS registros seguem regras atuais
}
```

### O Que "Não Validar" Significa

**NÃO significa** aceitar qualquer lixo:

```csharp
// Reconstitution ainda tem CONTRATOS básicos:
public static SimpleAggregateRoot CreateFromExistingInfo(
    CreateFromExistingInfoInput input
)
{
    // ✅ Tipos são verificados em compile-time
    // input.FirstName é string, não pode ser int

    // ✅ Nullability é verificada em compile-time
    // Se FirstName é non-nullable, input deve fornecer valor

    // ❌ NÃO valida regras de NEGÓCIO:
    // - Não verifica se FirstName.Length <= MaxLength
    // - Não verifica se BirthDate resulta em idade válida
    // - Não verifica se Email tem formato correto

    return new SimpleAggregateRoot(
        input.EntityInfo,
        input.FirstName,
        input.LastName,
        input.FullName,
        input.BirthDate
    );
}
```

**A diferença**:

| Tipo de Verificação | Reconstitution | Observação |
|---------------------|----------------|------------|
| Tipos (compile-time) | ✅ Sim | Garantido pelo compilador |
| Nullability | ✅ Sim | Garantido pelo compilador |
| Regras de negócio | ❌ Não | Podem mudar ao longo do tempo |
| Formato/padrões | ❌ Não | Podem mudar ao longo do tempo |
| Ranges/limites | ❌ Não | Podem mudar ao longo do tempo |

### Quando Dados Realmente Estão Corrompidos

Se dados no banco estão **realmente** corrompidos (não apenas "inválidos" por regras novas), isso é um problema de infraestrutura, não de domínio:

```csharp
// Dados corrompidos vs dados históricos:

// ✅ Dado histórico válido (não revalidar):
// firstName: "Maria Eduarda dos Santos" (35 chars, regra antiga: max 100)

// ❌ Dado corrompido (problema de infra):
// firstName: null quando coluna é NOT NULL
// firstName: "\x00\x00\x00" (bytes inválidos)
// entityInfo.Id: Guid.Empty

// Corrupção deve ser tratada na camada de infraestrutura:
public async Task<Person?> GetByIdAsync(Guid id)
{
    var dto = await _db.QueryAsync(id);

    // Verificações de INTEGRIDADE (não regras de negócio)
    if (dto.Id == Guid.Empty)
    {
        _logger.LogError("Corrupted record: empty ID for {Id}", id);
        return null; // Ou throw, dependendo da política
    }

    return Person.CreateFromExistingInfo(dto);
}
```

### Benefícios

1. **Resiliência**: Sistema não quebra quando regras mudam
2. **Event Sourcing**: Replay sempre funciona
3. **Auditoria**: Dados históricos preservados exatamente
4. **Migração**: Dados legados podem ser importados
5. **Temporal queries**: Estado exato em qualquer ponto no tempo
6. **Simplicidade**: CreateFromExistingInfo é direto, sem lógica condicional

### Trade-offs (Com Perspectiva)

- **Dados "inválidos" em memória**: Entidades podem ter dados que não passariam na validação atual
  - **Mitigação**: Isso é intencional - são dados **historicamente válidos**

### Trade-offs Frequentemente Superestimados

**"Mas e se alguém inserir dados inválidos diretamente no banco?"**

1. **Isso é bypass de domínio** - problema de governança, não de código
2. **Constraints do banco** devem garantir integridade básica
3. **Auditoria de acesso** deve detectar modificações diretas
4. **Validação em reconstitution não previne** - dados já estão lá

**"Deveria pelo menos logar quando dados são 'inválidos'"**

Isso cria ruído desnecessário:

```csharp
// ❌ Log desnecessário
if (firstName.Length > CurrentMaxLength)
    _logger.LogWarning("FirstName exceeds current max length");
// Milhares de logs para dados históricos perfeitamente válidos

// ✅ Log apenas para corrupção real
if (firstName == null && !columnAllowsNull)
    _logger.LogError("Data corruption detected");
```

## Fundamentação Teórica

### Imutabilidade de Eventos (Event Sourcing)

Greg Young, criador do Event Sourcing moderno:

> "An event is a fact. Facts don't change. Once something has happened, it has happened."
>
> *Um evento é um fato. Fatos não mudam. Uma vez que algo aconteceu, aconteceu.*

Revalidar eventos históricos viola este princípio fundamental.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre consistência:

> "AGGREGATES mark off the scope within which invariants have to be maintained at every point in the transaction."
>
> *AGGREGATES demarcam o escopo dentro do qual invariantes devem ser mantidas em cada ponto da transação.*

A palavra-chave é **transação**. Invariantes são verificadas durante a **criação** (transação original), não durante a **reconstituição** (leitura).

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) explicitamente sobre reconstitution:

> "When Aggregates are reconstituted from a persistence store, their invariants need not be checked because they were validated before they were originally persisted."
>
> *Quando Aggregates são reconstituídos de um repositório de persistência, suas invariantes não precisam ser verificadas porque foram validadas antes de serem originalmente persistidos.*

Esta citação é a fundamentação direta desta ADR.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre responsabilidade:

> "A function should do one thing. If a function does only those steps that are one level below the stated name of the function, then the function is doing one thing."
>
> *Uma função deve fazer uma coisa. Se uma função faz apenas os passos que estão um nível abaixo do nome declarado da função, então a função está fazendo uma coisa.*

`CreateFromExistingInfo` faz **uma coisa**: reconstituir. Adicionar validação seria fazer **duas coisas**.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre separação de políticas:

> "Business rules are the reason a software system exists. They are the core functionality."
>
> *Regras de negócio são a razão pela qual um sistema de software existe. São a funcionalidade central.*

Regras de **validação** e regras de **reconstituição** são políticas diferentes. Misturá-las viola a separação de responsabilidades.

### Temporal Data Management

Sistemas que lidam com dados temporais seguem o princípio de que **cada estado no tempo é válido no contexto daquele tempo**.

### Open-Closed Principle

Bertrand Meyer em "Object-Oriented Software Construction" (1988):

> "Software entities should be open for extension, but closed for modification."
>
> *Entidades de software devem ser abertas para extensão, mas fechadas para modificação.*

O sistema deve ser **aberto para extensão** (novas regras para novos dados) mas **fechado para modificação** (dados históricos não são alterados por regras novas).

## Antipadrões Documentados

### Antipadrão 1: Validação em Reconstitution

```csharp
// ❌ Valida em reconstitution
public static Person CreateFromExistingInfo(PersonDto dto)
{
    // Quebra quando regras mudam
    if (dto.FirstName.Length > MaxLength)
        throw new ValidationException("Nome muito longo");

    return new Person(dto);
}
```

### Antipadrão 2: "Correção" Automática de Dados

```csharp
// ❌ "Corrige" dados históricos
public static Person CreateFromExistingInfo(PersonDto dto)
{
    var firstName = dto.FirstName;

    // Trunca para caber na regra atual
    if (firstName.Length > MaxLength)
        firstName = firstName.Substring(0, MaxLength);

    return new Person(firstName); // Dados adulterados!
}
```

### Antipadrão 3: Validação com Flag de Ignorar

```csharp
// ❌ Flag para "tolerar" dados inválidos
public static Person CreateFromExistingInfo(PersonDto dto)
{
    try
    {
        return Person.Create(dto, tolerateInvalid: true);
    }
    catch (ValidationException)
    {
        // Swallow exception - dados "tolerados"
        return new Person(dto);
    }
}
```

### Antipadrão 4: Log de "Violações"

```csharp
// ❌ Log desnecessário para dados históricos válidos
public static Person CreateFromExistingInfo(PersonDto dto)
{
    if (dto.FirstName.Length > CurrentRules.MaxLength)
    {
        _logger.LogWarning(
            "Historical data violates current rules: {Name} exceeds {Max}",
            dto.FirstName, CurrentRules.MaxLength
        );
    }
    return new Person(dto);
}
// Resultado: milhares de warnings para dados perfeitamente válidos
```

## Decisões Relacionadas

- [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) - Separação RegisterNew vs CreateFromExistingInfo
- [DE-004](./DE-004-estado-invalido-nunca-existe-na-memoria.md) - Estado inválido nunca existe (para dados NOVOS)
- [DE-015](./DE-015-customizacao-de-metadados-apenas-no-startup.md) - Customização de metadados

## Leitura Recomendada

- [Event Sourcing - Martin Fowler](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Versioning in an Event Sourced System - Greg Young](https://leanpub.com/esversioning)
- [Temporal Data & The Relational Model - Date, Darwen, Lorentzos](https://www.elsevier.com/books/temporal-data-and-the-relational-model/date/978-1-55860-855-9)
- [CQRS Journey - Microsoft Patterns & Practices](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10))

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Implementa CreateFromExistingInfo sem validação, permitindo reconstitution de dados históricos |
| [EntityInfo](../../building-blocks/domain-entities/models/entity-info.md) | Modelo que carrega metadados de entidades existentes durante reconstitution |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Construtor Público com Validação Impede Reconstitution
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Reconstitution Pattern
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: CreateFromExistingInfo NÃO Valida Dados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Implementação de CreateFromExistingInfo
