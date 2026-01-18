# DE-026: Propriedades Derivadas: Persistidas vs Calculadas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **cartório de registro civil**:

**Cenário 1 - Nome composto registrado**:
Em 1990, João Silva registrou seu filho como "João Silva Junior". O nome completo foi **escrito no documento** naquele formato específico. Em 2025, mesmo que a regra de composição de nomes mude (ex: obrigar sobrenome materno), o registro de 1990 permanece intacto: "João Silva Junior".

**Cenário 2 - Nome composto calculado na hora**:
Imagine se o cartório guardasse apenas "João" + "Silva" + "Junior" separadamente, e toda vez que alguém pedisse o nome completo, aplicasse a **regra atual** para montar. Em 2025, o documento de 1990 apareceria como "Silva João Junior" (nova regra). O histórico foi **corrompido**.

Em entidades de domínio, propriedades derivadas como `FullName` (composta de `FirstName` + `LastName`) enfrentam esse mesmo dilema: **armazenar** o valor derivado (como cartório) ou **calcular** sob demanda (correndo risco de corrupção histórica).

---

### O Problema Técnico

Propriedades derivadas podem ser implementadas de duas formas:

**Opção 1 - Calculada (expression body)**:
```csharp
// ⚠ PERIGOSO para valores que precisam de preservação histórica
public class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    // Calculado sob demanda - aplica regra ATUAL
    public string FullName => $"{FirstName} {LastName}";
}
```

**Opção 2 - Armazenada (backed property)**:
```csharp
// ✅ SEGURO para valores que precisam de preservação histórica
public class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    // Armazenado - preserva valor original
    public string FullName { get; private set; }
}
```

### Por Que Propriedades Calculadas São Perigosas

```csharp
// Exemplo: Regra de FullName mudou ao longo do tempo

// 2020: Regra era "FirstName LastName"
public string FullName => $"{FirstName} {LastName}";  // "João Silva"

// 2025: Nova regra é "LastName, FirstName"
public string FullName => $"{LastName}, {FirstName}"; // "Silva, João"

// Problema no reconstitution:
var person2020 = CreateFromExistingInfo(
    entityInfo: existingInfo,     // Criado em 2020
    firstName: "João",
    lastName: "Silva"
    // FullName não foi salvo - será recalculado
);

// person2020.FullName retorna "Silva, João" (regra 2025)
// mas o valor ORIGINAL era "João Silva" (regra 2020)
// DADOS HISTÓRICOS CORROMPIDOS!
```

**Consequências graves**:
- Relatórios históricos mostram dados incorretos
- Auditoria comprometida (LGPD/GDPR/HIPAA)
- Event sourcing quebra (eventos antigos não reproduzem corretamente)
- Testes intermitentes (dependem de regra atual)
- Impossível rastrear mudanças reais de dados

### Como Normalmente é Feito (e Por Que Não é Ideal)

```csharp
// ❌ COMUM: Sempre calcular para "simplificar"
public class Order
{
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal DiscountPercent { get; private set; }

    // Tudo calculado - parece elegante, mas...
    public decimal Subtotal => UnitPrice * Quantity;
    public decimal DiscountAmount => Subtotal * (DiscountPercent / 100);
    public decimal Total => Subtotal - DiscountAmount;
}

// Problemas:
// 1. Se regra de arredondamento mudar, pedidos antigos mostram valores diferentes
// 2. Se DiscountPercent agora tiver cap de 50%, pedidos antigos com 70% quebram
// 3. Relatório financeiro de 2023 mostra valores de 2025
// 4. Auditoria fiscal impossível - valores mudam retroativamente
```

## A Decisão

### Nossa Abordagem

**Analisar cada propriedade derivada** e decidir com base em critérios claros:

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // Propriedades base
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public BirthDate BirthDate { get; private set; }

    // ✅ ARMAZENADA - Regra de composição pode mudar
    public string FullName { get; private set; }

    // ✅ CALCULADA - Idade é sempre "hoje - data de nascimento"
    // (mas Age não é armazenada - é transitória e contextual)
}
```

### Critérios para Decidir

| Critério | Armazenar | Calcular |
|----------|-----------|----------|
| **Regra pode mudar no futuro?** | ✅ Sim | ❌ Não |
| **Valor precisa de auditoria/rastreabilidade?** | ✅ Sim | ❌ Não |
| **Reconstitution deve preservar valor original?** | ✅ Sim | ❌ Não |
| **Valor é usado em relatórios históricos?** | ✅ Sim | ❌ Não |
| **Cálculo é IMUTÁVEL e UNIVERSAL?** | ❌ Não | ✅ Sim |
| **Valor é transitório/contextual?** | ❌ Não | ✅ Sim |

### Exemplos de Cada Categoria

**ARMAZENAR (propriedade com backing field)**:

```csharp
// FullName - regra de composição pode mudar
public string FullName { get; private set; }

// TotalAmount - regras de desconto/imposto podem mudar
public decimal TotalAmount { get; private set; }

// DisplayName - formato de exibição pode mudar
public string DisplayName { get; private set; }

// FormattedAddress - formato regional pode mudar
public string FormattedAddress { get; private set; }

// CalculatedScore - algoritmo de scoring pode mudar
public decimal CalculatedScore { get; private set; }
```

**CALCULAR (expression body ou método)**:

```csharp
// Age - sempre é "hoje - nascimento" (lei da física, não muda)
// NOTA: Não é propriedade armazenada, é método que recebe contexto
public int GetAge(DateOnly referenceDate) =>
    referenceDate.Year - BirthDate.Value.Year;

// IsExpired - sempre é "hoje > dataExpiracao"
public bool IsExpired(DateTimeOffset now) => ExpirationDate < now;

// ItemCount - sempre é "lista.Count"
public int ItemCount => _items.Count;

// HasItems - sempre é "lista.Any()"
public bool HasItems => _items.Count > 0;
```

### Fluxo de Atualização de Propriedade Derivada

```csharp
private bool ChangeNameInternal(
    ExecutionContext executionContext,
    string firstName,
    string lastName
)
{
    // 1. Calcular valor derivado com regra ATUAL
    string fullName = $"{firstName} {lastName}";

    // 2. Validar e atribuir TODAS as propriedades juntas
    bool isSuccess =
        SetFirstName(executionContext, firstName)
        & SetLastName(executionContext, lastName)
        & SetFullName(executionContext, fullName);  // ✅ Derivada atualizada junto

    return isSuccess;
}
```

**Importante**: O valor derivado é calculado **no momento da operação** e armazenado. A partir daí, é tratado como qualquer outra propriedade - preservado em reconstitution, não recalculado.

### Metadados de Propriedades Derivadas

Propriedades derivadas **TÊM** seus próprios metadados:

```csharp
public static class Metadata
{
    // FirstName
    public static bool FirstNameIsRequired { get; private set; } = true;
    public static int FirstNameMinLength { get; private set; } = 1;
    public static int FirstNameMaxLength { get; private set; } = 255;

    // LastName
    public static bool LastNameIsRequired { get; private set; } = true;
    public static int LastNameMinLength { get; private set; } = 1;
    public static int LastNameMaxLength { get; private set; } = 255;

    // FullName (derivada) - metadados derivados das partes
    public static bool FullNameIsRequired { get; private set; } = true;
    public static int FullNameMinLength { get; private set; } =
        FirstNameMinLength + LastNameMinLength + 1; // Soma dos mínimos + espaço
    public static int FullNameMaxLength { get; private set; } =
        FirstNameMaxLength + LastNameMaxLength + 1; // Soma dos máximos + espaço
}
```

### Validação de Propriedades Derivadas

Propriedades derivadas **TÊM** método de validação próprio:

```csharp
public static bool ValidateFullName(
    ExecutionContext executionContext,
    string? fullName
)
{
    bool fullNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
        executionContext,
        propertyName: CreateMessageCode<SimpleAggregateRoot>(Metadata.FullNamePropertyName),
        isRequired: Metadata.FullNameIsRequired,
        value: fullName
    );

    if (!fullNameIsRequiredValidation)
        return false;

    bool fullNameMinLengthValidation = ValidationUtils.ValidateMinLength(
        executionContext,
        propertyName: CreateMessageCode<SimpleAggregateRoot>(Metadata.FullNamePropertyName),
        minLength: Metadata.FullNameMinLength,
        value: fullName!.Length
    );

    bool fullNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
        executionContext,
        propertyName: CreateMessageCode<SimpleAggregateRoot>(Metadata.FullNamePropertyName),
        maxLength: Metadata.FullNameMaxLength,
        value: fullName!.Length
    );

    return fullNameIsRequiredValidation
        && fullNameMinLengthValidation
        && fullNameMaxLengthValidation;
}
```

**Por que validar se é derivada?**
- A regra de composição pode gerar valores inválidos em edge cases
- Exemplo: `FirstName="A"` + `LastName="B"` → `FullName="A B"` (3 chars) pode violar `MinLength=5`
- A validação garante que o valor derivado também respeita suas próprias constraints

### Set* para Propriedades Derivadas

Propriedades derivadas **TÊM** método Set* próprio:

```csharp
private bool SetFullName(
    ExecutionContext executionContext,
    string fullName
)
{
    bool isValid = ValidateFullName(
        executionContext,
        fullName
    );

    if (!isValid)
        return false;

    FullName = fullName;

    return true;
}
```

### Reconstitution de Propriedades Derivadas

Em `CreateFromExistingInfo`, a propriedade derivada é carregada diretamente do input, **sem recálculo**:

```csharp
public static SimpleAggregateRoot CreateFromExistingInfo(
    CreateFromExistingInfoInput input
)
{
    return new SimpleAggregateRoot(
        input.EntityInfo,
        input.FirstName,
        input.LastName,
        input.FullName,    // ✅ Valor original preservado, NÃO recalculado
        input.BirthDate
    );
}

// Construtor privado recebe FullName como parâmetro
private SimpleAggregateRoot(
    EntityInfo entityInfo,
    string firstName,
    string lastName,
    string fullName,       // ✅ Parâmetro separado
    BirthDate birthDate
) : base(entityInfo)
{
    FirstName = firstName;
    LastName = lastName;
    FullName = fullName;   // ✅ Atribuição direta, sem cálculo
    BirthDate = birthDate;
}
```

### Comparação Visual

```
+-------------------------------------------------------------------------+
│                    PROPRIEDADE ARMAZENADA (FullName)                    │
│                                                                         │
│  RegisterNew/ChangeName                                                 │
│       │                                                                 │
│       ▼                                                                 │
│  +-----------------------------------------+                           │
│  │ fullName = $"{firstName} {lastName}"    │ → Calcula com regra ATUAL │
│  │ SetFullName(ctx, fullName)              │ → Armazena o valor        │
│  +-----------------------------------------+                           │
│       │                                                                 │
│       ▼                                                                 │
│  Valor armazenado no banco: "João Silva"                               │
│                                                                         │
│  CreateFromExistingInfo (5 anos depois)                                │
│       │                                                                 │
│       ▼                                                                 │
│  +-----------------------------------------+                           │
│  │ FullName = input.FullName               │ → Carrega valor ORIGINAL  │
│  │ (NÃO recalcula!)                        │                           │
│  +-----------------------------------------+                           │
│       │                                                                 │
│       ▼                                                                 │
│  entity.FullName = "João Silva" (preservado!)                          │
+-------------------------------------------------------------------------+

+-------------------------------------------------------------------------+
│                    PROPRIEDADE CALCULADA (IsExpired)                    │
│                                                                         │
│  Qualquer momento                                                       │
│       │                                                                 │
│       ▼                                                                 │
│  +-----------------------------------------+                           │
│  │ IsExpired(now) => ExpirationDate < now  │ → Sempre calcula          │
│  +-----------------------------------------+                           │
│       │                                                                 │
│       ▼                                                                 │
│  Resultado depende do momento da chamada                               │
│  (valor não é armazenado, é transitório)                               │
+-------------------------------------------------------------------------+
```

### Benefícios

1. **Integridade histórica**: Dados antigos permanecem exatamente como eram
2. **Auditoria completa**: Possível rastrear o valor exato em qualquer momento
3. **Reconstitution confiável**: Entidades reconstituídas são idênticas às originais
4. **Event Sourcing funciona**: Eventos históricos reproduzem estado correto
5. **Relatórios precisos**: Relatórios de 2020 mostram valores de 2020
6. **Compliance**: LGPD/GDPR/HIPAA exigem preservação exata de dados

### Trade-offs (Com Perspectiva)

- **Mais espaço em banco**: Armazena valor que poderia ser calculado
  - **Mitigação**: Custo de storage é insignificante comparado ao risco de corrupção de dados

- **Redundância aparente**: FirstName + LastName + FullName parecem redundantes
  - **Mitigação**: Não é redundância - são três informações distintas (partes + composição específica)

- **Atualização em múltiplos lugares**: Mudar nome requer atualizar FullName também
  - **Mitigação**: `ChangeNameInternal` encapsula isso - um lugar para manter

### Trade-offs Frequentemente Superestimados

**"É mais elegante calcular sob demanda"**

Elegância não vale corrupção de dados:

```csharp
// "Elegante" mas perigoso
public string FullName => $"{FirstName} {LastName}";

// "Verboso" mas seguro
public string FullName { get; private set; }

// A "verbosidade" são 5 caracteres extras ({ get; private set; })
// O custo de corrupção de dados é incalculável
```

**"Regra nunca vai mudar"**

Regras SEMPRE mudam. Exemplos reais:
- Formato de endereço mudou quando CEP passou de 5 para 8 dígitos
- Formato de CPF mudou quando incluiu dígito verificador
- Formato de nome mudou para incluir nome social
- Cálculo de imposto muda todo ano

**"Posso recalcular com a regra antiga se precisar"**

Isso exige:
- Guardar versão da regra junto com o dado
- Manter todas as versões antigas da regra no código
- Saber qual regra aplicar em cada reconstitution
- Complexidade muito maior que simplesmente armazenar o valor

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre integridade de dados:

> "A MODEL represents knowledge. When you change a MODEL, you should be changing knowledge, not accidentally corrupting history."
>
> *Um MODELO representa conhecimento. Quando você muda um MODELO, deveria estar mudando conhecimento, não corrompendo história acidentalmente.*

Propriedades calculadas que dependem de regras mutáveis corrompem história silenciosamente.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre Event Sourcing:

> "Events are facts. They represent something that happened. You cannot change what happened - you can only record new facts."
>
> *Eventos são fatos. Eles representam algo que aconteceu. Você não pode mudar o que aconteceu - você pode apenas registrar novos fatos.*

Se `FullName` é calculado, ao reconstituir eventos antigos, você está "reescrevendo a história" com regras atuais - violando o princípio de imutabilidade de eventos.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre previsibilidade:

> "Functions should do one thing. They should do it well. They should do it only."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso.*

Uma propriedade calculada faz duas coisas: retorna um valor E aplica uma regra de negócio. Isso viola o princípio de responsabilidade única - a regra deveria ser aplicada no momento da criação/modificação, não a cada acesso.

Robert C. Martin sobre clareza de intenção:

> "The name of a variable, function, or class, should answer all the big questions. It should tell you why it exists, what it does, and how it is used."
>
> *O nome de uma variável, função ou classe deveria responder todas as grandes questões. Deveria dizer por que existe, o que faz, e como é usada.*

`FullName` como propriedade armazenada é clara: é o nome completo **como foi registrado**. `FullName` como getter calculado é ambígua: é o nome completo **segundo a regra atual** - que pode não ser a mesma de quando foi criado.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre dados imutáveis:

> "The best way to deal with data is to treat it as immutable. Once created, data should not be changed."
>
> *A melhor forma de lidar com dados é tratá-los como imutáveis. Uma vez criados, dados não deveriam ser alterados.*

Propriedades calculadas violam imutabilidade conceitual - o mesmo registro "muda" ao longo do tempo conforme regras evoluem, mesmo que os dados base permaneçam iguais.

### Temporal Data Patterns

Martin Fowler em "Patterns of Enterprise Application Architecture" (2002) sobre dados temporais:

> "The value of data often depends on when you're looking at it. A temporal pattern captures the value as it was at a particular point in time."
>
> *O valor de dados frequentemente depende de quando você está olhando. Um padrão temporal captura o valor como era em um ponto específico no tempo.*

Propriedades derivadas armazenadas implementam naturalmente o padrão "Snapshot" - capturam o valor derivado no momento da operação.

### Princípio da Menor Surpresa

Bertrand Meyer em "Object-Oriented Software Construction" (1997):

> "A component should behave in a way that most users will expect it to behave."
>
> *Um componente deveria se comportar da forma que a maioria dos usuários espera que ele se comporte.*

Usuários esperam que `person.FullName` retorne o nome completo **como foi registrado**, não uma recomposição com regras atuais que pode diferir do original.

## Antipadrões Documentados

### Antipadrão 1: Expression Body para Valor Histórico

```csharp
// ❌ Corrompe dados históricos silenciosamente
public class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    // Valor muda conforme regra muda
    public string FullName => $"{FirstName} {LastName}";
}
```

### Antipadrão 2: Recálculo em Reconstitution

```csharp
// ❌ Reconstitution recalcula valor derivado
public static Person CreateFromExistingInfo(CreateFromExistingInfoInput input)
{
    var person = new Person
    {
        FirstName = input.FirstName,
        LastName = input.LastName
    };

    // Recalcula com regra ATUAL - ERRADO!
    person.FullName = $"{input.FirstName} {input.LastName}";

    return person;
}
```

### Antipadrão 3: Propriedade Calculada com Regra de Negócio

```csharp
// ❌ Regra de negócio em getter - muda ao longo do tempo
public class Order
{
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    // Regra de desconto pode mudar
    public decimal Total => UnitPrice * Quantity * GetCurrentDiscountRate();

    private decimal GetCurrentDiscountRate()
    {
        // Lê do banco/config - muda ao longo do tempo!
        return _configService.GetDiscountRate();
    }
}
```

### Antipadrão 4: Lazy Calculation com Cache

```csharp
// ❌ Cache não resolve o problema - ainda usa regra atual
public class Person
{
    private string? _fullNameCache;

    public string FullName
    {
        get
        {
            // Cacheia, mas ainda calcula com regra atual
            _fullNameCache ??= $"{FirstName} {LastName}";
            return _fullNameCache;
        }
    }
}
```

### Antipadrão 5: Misturar Armazenada e Calculada Inconsistentemente

```csharp
// ❌ Inconsistência causa confusão
public class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    // FullName armazenado...
    public string FullName { get; private set; }

    // ...mas DisplayName calculado - Por quê diferente?
    public string DisplayName => $"Sr(a). {FullName}";
}
```

## Decisões Relacionadas

- [DE-004](./DE-004-estado-invalido-nunca-existe-na-memoria.md) - Estado inválido nunca existe
- [DE-018](./DE-018-reconstitution-nao-valida-dados.md) - Reconstitution não valida dados
- [DE-020](./DE-020-dois-construtores-privados.md) - Dois construtores privados
- [DE-022](./DE-022-metodos-set-privados.md) - Métodos Set* privados

## Leitura Recomendada

- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Implementing Domain-Driven Design - Vaughn Vernon](https://vaughnvernon.com/)
- [Clean Code - Robert C. Martin](https://blog.cleancoder.com/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Patterns of Enterprise Application Architecture - Martin Fowler](https://martinfowler.com/eaaCatalog/)
- [Temporal Patterns - Martin Fowler](https://martinfowler.com/eaaDev/timeNarrative.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Suporta tanto propriedades derivadas persistidas quanto calculadas, deixando a decisão para a entidade concreta |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_GUIDANCE: Propriedades Derivadas Persistidas vs Calculadas
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Metadata de FullName com valores derivados
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - FullName como propriedade armazenada
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ChangeNameInternal - atualização de FullName junto com nome
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ValidateFullName
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - SetFullName
