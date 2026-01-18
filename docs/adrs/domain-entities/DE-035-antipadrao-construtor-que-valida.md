# DE-035: Antipadrão: Construtor que Valida

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **cartório que emite certidões** com duas políticas:

**Política "validação rígida no balcão" (construtor que valida)**:
- Cliente chega com documento de 1985
- Atendente: "CPF precisa ter 11 dígitos" (regra de 2025)
- Cliente: "Mas em 1985 era 9 dígitos!"
- Atendente: "Não importa, regra atual exige 11"
- Certidão histórica NEGADA - documento antigo não pode ser reproduzido
- Sistema não consegue lidar com dados do próprio passado

**Política "validação apenas para novos" (nossa abordagem)**:
- Novo documento → valida com regras atuais (11 dígitos)
- Documento histórico → aceita como está (9 dígitos era válido na época)
- Cartório consegue emitir certidões de qualquer período
- Histórico preservado fielmente

O construtor que valida é como o atendente rígido - não consegue lidar com dados históricos que eram válidos quando foram criados, mas não passam nas regras atuais.

---

### O Problema Técnico

Construtores que validam PARECEM seguros, mas QUEBRAM reconstitution de dados históricos:

```csharp
// ❌ ANTIPADRÃO: Construtor público que valida
public class Person
{
    public string FirstName { get; }

    // Metadados: regra de 2025
    public const int MaxLength = 20;

    public Person(string firstName)
    {
        // Validação no construtor
        if (firstName.Length > MaxLength)
            throw new ArgumentException($"Name cannot exceed {MaxLength} characters");

        FirstName = firstName;
    }
}

// Cenário: Banco de dados tem registro de 2020
// Na época, MaxLength era 50 (regra mudou em 2023)
// Nome salvo: "Maria Auxiliadora dos Santos" (28 caracteres)

// Tentativa de carregar dados históricos:
var dto = await database.QueryAsync<PersonDto>("SELECT * FROM Persons WHERE Id = @id");

var person = new Person(dto.FirstName);  // 💥 EXCEÇÃO!
// ArgumentException: "Name cannot exceed 20 characters"

// ❌ SISTEMA NÃO CONSEGUE LER SEUS PRÓPRIOS DADOS!
```

**Problemas graves**:

1. **Reconstitution quebrado**: Dados válidos no passado falham hoje
2. **Migração impossível**: Não consegue ler dados para migrar
3. **Event sourcing quebrado**: Eventos históricos não podem ser "replayed"
4. **Sistema frágil**: Mudar uma regra pode quebrar todo o histórico

---

### Por Que Desenvolvedores Fazem Isso

```csharp
// Parece uma boa prática: "Validar entrada no construtor"
public class Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format");

        Value = value;
    }
}

// Motivações:
// - "Objeto nunca estará em estado inválido"
// - "Fail fast - descobrir erros cedo"
// - "Construtores devem validar seus parâmetros"
```

Essas motivações são válidas para NOVOS dados. O problema é aplicá-las a dados EXISTENTES.

## A Decisão

### Nossa Abordagem: Separar Criação de Reconstitution

```csharp
// ✅ CORRETO: Dois caminhos distintos
public sealed class Person : EntityBase<Person>
{
    public string FirstName { get; private set; } = string.Empty;

    public static class PersonMetadata
    {
        public static int FirstNameMaxLength { get; private set; } = 20;
    }

    // Construtor PRIVATE - ninguém usa diretamente
    private Person() { }

    // -------------------------------------------------------------------------
    // CAMINHO 1: Dados NOVOS - RegisterNew() VALIDA
    // -------------------------------------------------------------------------
    public static Person? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (ctx, inp) => new Person(),
            handler: (ctx, inp, instance) =>
            {
                // VALIDA com regras ATUAIS
                return instance.SetFirstName(ctx, inp.FirstName);
            }
        );
    }

    // -------------------------------------------------------------------------
    // CAMINHO 2: Dados EXISTENTES - CreateFromExistingInfo() NÃO VALIDA
    // -------------------------------------------------------------------------
    public static Person CreateFromExistingInfo(CreateFromExistingInfoInput input)
    {
        // NÃO valida - dados já foram validados quando criados
        return new Person
        {
            FirstName = input.FirstName  // Aceita como está
        };
    }

    private bool SetFirstName(ExecutionContext ctx, string? firstName)
    {
        if (!ValidateFirstName(ctx, firstName))
            return false;

        FirstName = firstName!;
        return true;
    }

    public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            ctx.AddErrorMessage("FIRST_NAME_REQUIRED", "First name is required");
            return false;
        }

        if (firstName.Length > PersonMetadata.FirstNameMaxLength)
        {
            ctx.AddErrorMessage(
                "FIRST_NAME_TOO_LONG",
                $"First name cannot exceed {PersonMetadata.FirstNameMaxLength} characters"
            );
            return false;
        }

        return true;
    }
}
```

### Fluxo de Dados

```
+-------------------------------------------------------------------------+
│                         DADOS NOVOS                                      │
│                                                                          │
│  API Request → RegisterNew() → VALIDA → Salva no Banco                  │
│                                                                          │
│  Regras ATUAIS aplicadas:                                                │
│  - FirstName max 20 caracteres (regra de 2025)                          │
│  - Email formato válido                                                  │
│  - BirthDate no passado                                                  │
│                                                                          │
│  Se inválido → retorna null + mensagens de erro                         │
│  Se válido → salva com EntityInfo (Id, CreatedAt, etc.)                 │
+-------------------------------------------------------------------------+

+-------------------------------------------------------------------------+
│                         DADOS EXISTENTES                                 │
│                                                                          │
│  Banco → CreateFromExistingInfo() → NÃO VALIDA → Entidade em memória    │
│                                                                          │
│  Dados aceitos COMO ESTÃO:                                               │
│  - FirstName "Maria Auxiliadora dos Santos" (28 chars) → OK             │
│  - Email antigo sem TLD → OK                                             │
│  - Qualquer dado histórico → OK                                          │
│                                                                          │
│  Razão: dados JÁ FORAM validados quando criados                         │
│  Regras podem ter mudado, mas dados são válidos por definição           │
+-------------------------------------------------------------------------+
```

### Por Que Dados Históricos São Válidos Por Definição

```csharp
// Cenário real: evolução de regras ao longo do tempo

// 2020: MaxLength = 50
// Usuário cadastra: "Maria Auxiliadora dos Santos Silva" (35 chars)
// → Válido na época → salvo no banco

// 2023: MaxLength reduzido para 20 (nova política de UX)
// Usuários NOVOS: máximo 20 caracteres
// Usuários EXISTENTES: mantêm seus nomes (direito adquirido)

// 2025: Sistema tenta carregar Maria
// ❌ COM construtor que valida: ERRO - 35 > 20
// ✅ COM CreateFromExistingInfo: OK - dado histórico preservado

// Maria pode continuar usando o sistema!
// Se ela EDITAR o nome, aí sim aplicamos regra nova.
```

### Uso no Repository

```csharp
public class PersonRepository : IPersonRepository
{
    public async Task<Person?> GetByIdAsync(Guid id)
    {
        var dto = await _db.QuerySingleOrDefaultAsync<PersonDto>(
            "SELECT * FROM Persons WHERE Id = @Id",
            new { Id = id }
        );

        if (dto is null)
            return null;

        // ✅ Usa CreateFromExistingInfo - NÃO valida
        return Person.CreateFromExistingInfo(new CreateFromExistingInfoInput(
            entityInfo: dto.EntityInfo,
            firstName: dto.FirstName,
            lastName: dto.LastName,
            birthDate: dto.BirthDate
        ));
    }

    public async Task SaveAsync(Person person)
    {
        // Person JÁ foi validado no RegisterNew ou ChangeName
        // Apenas persiste
        await _db.ExecuteAsync(
            "INSERT INTO Persons (...) VALUES (...)",
            new { ... }
        );
    }
}
```

### Event Sourcing

```csharp
// Event Sourcing: eventos são IMUTÁVEIS e podem ter dados "antigos"

public record PersonCreatedEvent(
    Guid PersonId,
    string FirstName,    // Pode ter 35 caracteres (regra de 2020)
    DateTime CreatedAt
);

public class PersonAggregate
{
    private string _firstName = string.Empty;

    // Aplicar evento histórico - NÃO VALIDA
    public void Apply(PersonCreatedEvent @event)
    {
        // ❌ ERRADO: Validar evento histórico
        // if (@event.FirstName.Length > 20) throw new Exception();

        // ✅ CORRETO: Aceitar evento como está
        _firstName = @event.FirstName;
    }

    // Criar novo evento - VALIDA
    public PersonCreatedEvent Create(ExecutionContext ctx, string firstName)
    {
        // Valida com regras ATUAIS
        if (!ValidateFirstName(ctx, firstName))
            throw new ValidationException(ctx.Messages);

        return new PersonCreatedEvent(Guid.NewGuid(), firstName, DateTime.UtcNow);
    }
}
```

### Comparação

| Cenário | Construtor que Valida | RegisterNew + CreateFromExistingInfo |
|---------|----------------------|--------------------------------------|
| **Dados novos** | Valida ✅ | Valida (RegisterNew) ✅ |
| **Dados históricos** | QUEBRA ❌ | Funciona (CreateFromExistingInfo) ✅ |
| **Event sourcing** | QUEBRA ❌ | Funciona ✅ |
| **Migração de dados** | QUEBRA ❌ | Funciona ✅ |
| **Mudança de regras** | Risco alto | Seguro ✅ |

### Trade-offs (Com Perspectiva)

- **Dois métodos de criação**: RegisterNew e CreateFromExistingInfo
  - **Mitigação**: São responsabilidades DIFERENTES. Misturá-las em um construtor é que causa problemas.

- **Validação pode ser "pulada"**: CreateFromExistingInfo não valida
  - **Mitigação**: Dados já foram validados quando criados. Re-validar é redundante e perigoso.

### Trade-offs Frequentemente Superestimados

**"E se alguém usar CreateFromExistingInfo para dados novos?"**

```csharp
// Preocupação: bypass intencional de validação
var person = Person.CreateFromExistingInfo(new CreateFromExistingInfoInput(
    entityInfo: EntityInfo.RegisterNew(...),  // Gera novo Id!
    firstName: "X"  // Nome inválido
));

// Realidade:
// 1. CreateFromExistingInfo requer EntityInfo COMPLETO (Id, TenantInfo, etc.)
// 2. É mais difícil de usar "errado" do que RegisterNew
// 3. Code review pega isso facilmente
// 4. Testes cobrem os caminhos corretos

// Se um desenvolvedor DELIBERADAMENTE quer burlar validação,
// nenhum design vai impedi-lo 100%
```

**"Construtor que valida é mais seguro"**

```csharp
// Construtor que valida parece seguro, mas é FRÁGIL:

// Dia 1: Tudo funciona
public Person(string firstName) { /* valida */ }

// Dia 100: Mudou regra de negócio
PersonMetadata.FirstNameMaxLength = 20;  // Era 50

// Dia 101: 10.000 usuários não conseguem fazer login
// Seus nomes "antigos" não passam na validação "nova"
// INCIDENTE DE PRODUÇÃO!

// Com RegisterNew + CreateFromExistingInfo:
// - Novos usuários: regra nova (20 chars)
// - Usuários existentes: continuam funcionando
// - Zero incidentes
```

## Fundamentação Teórica

### O Que o DDD Diz

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre reconstitution:

> "Reconstitution must not validate invariants. The data being reconstituted was valid when it was stored. Validation during reconstitution would reject historically valid data that no longer meets current rules."
>
> *Reconstitution não deve validar invariantes. Os dados sendo reconstituídos eram válidos quando foram armazenados. Validação durante reconstitution rejeitaria dados historicamente válidos que não atendem mais às regras atuais.*

### O Que o Event Sourcing Diz

Greg Young sobre eventos imutáveis:

> "Events are facts. They represent something that happened. You cannot change history. If business rules change, old events must still be applicable."
>
> *Eventos são fatos. Eles representam algo que aconteceu. Você não pode mudar a história. Se regras de negócio mudam, eventos antigos ainda devem ser aplicáveis.*

### O Que o CQRS Diz

Martin Fowler sobre separação de responsabilidades:

> "The key insight is that commands (writes) have different requirements than queries (reads). Commands need validation; queries need to return data as stored."
>
> *O insight chave é que comandos (escritas) têm requisitos diferentes de queries (leituras). Comandos precisam de validação; queries precisam retornar dados como armazenados.*

- **RegisterNew** = Command (valida)
- **CreateFromExistingInfo** = Read model (não valida)

### Princípio da Responsabilidade Única

O construtor tem UMA responsabilidade: criar objeto. Validação é OUTRA responsabilidade.

```csharp
// ❌ Construtor com duas responsabilidades
public Person(string firstName)
{
    Validate(firstName);  // Responsabilidade 1
    Initialize(firstName); // Responsabilidade 2
}

// ✅ Responsabilidades separadas
public static Person? RegisterNew(...) { /* valida + cria */ }
public static Person CreateFromExistingInfo(...) { /* só cria */ }
```

## Antipadrões Relacionados

### Antipadrão: Construtor Público com Validação

```csharp
// ❌ Construtor público que valida
public Person(string firstName)
{
    if (string.IsNullOrWhiteSpace(firstName))
        throw new ArgumentException("First name required");

    FirstName = firstName;
}

// Problemas:
// - Exceção para validação de negócio
// - Reconstitution usa mesmo caminho que criação
// - Regras antigas vs novas não são diferenciadas
```

### Antipadrão: Validação Condicional no Construtor

```csharp
// ❌ Tentar resolver com flag
public Person(string firstName, bool skipValidation = false)
{
    if (!skipValidation)
    {
        if (firstName.Length > MaxLength)
            throw new ArgumentException("Too long");
    }

    FirstName = firstName;
}

// Problemas:
// - Flag "mágica" obscurece intenção
// - Fácil esquecer de passar true
// - Mistura responsabilidades
// - Qualquer um pode "pular" validação
```

### Antipadrão: Validação no Setter

```csharp
// ❌ Setter público que valida
public class Person
{
    private string _firstName = string.Empty;

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (value.Length > MaxLength)
                throw new ArgumentException("Too long");
            _firstName = value;
        }
    }
}

// Problemas:
// - Reconstitution atribui ao setter → validação dispara
// - Mesmos problemas do construtor que valida
```

## Decisões Relacionadas

- [DE-002](./DE-002-construtores-privados-com-factory-methods.md) - Construtores Privados com Factory Methods
- [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) - Separação RegisterNew vs CreateFromExistingInfo
- [DE-018](./DE-018-reconstitution-nao-valida-dados.md) - Reconstitution Não Valida Dados
- [DE-020](./DE-020-dois-construtores-privados.md) - Dois Construtores Privados

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Implementa padrão RegisterNew + CreateFromExistingInfo |
| [EntityInfo](../../building-blocks/domain-entities/models/entity-info.md) | Distingue dados novos (RegisterNew gera Id) de existentes (CreateFromExistingInfo recebe Id) |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - comentário LLM_RULE sobre construtor que valida
- [EntityBase.cs](../../../src/BuildingBlocks/Domain.Entities/EntityBase.cs) - Implementação de RegisterNewInternal e CreateFromExistingInfo
