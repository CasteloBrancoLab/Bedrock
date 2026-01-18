# DE-024: Método Público Nunca Chama Outro Método Público

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um **restaurante** com diversos pratos no cardápio:

**Situação problemática**:
O chef recebe um pedido de "Filé Mignon com Fritas". Ele pensa: "Já tenho o processo completo de preparar 'Prato Executivo Completo' que inclui tudo - carne, batatas, salada, molho, e até registro da venda, controle de estoque, e notificação ao garçom. Vou só reutilizar esse processo todo!"

**Problemas que surgem**:
- O cliente pediu apenas filé com fritas, mas o sistema registrou uma venda de "Prato Executivo"
- O estoque foi debitado com itens que não foram servidos
- O garçom foi notificado erroneamente sobre um prato diferente
- A conta ficou incorreta
- O cozinheiro não consegue mais saber quais passos são essenciais e quais são efeitos colaterais

**Solução correta**:
Cada prato do cardápio (método público) orquestra ingredientes e técnicas básicas (métodos privados ou construtor privado), sem chamar outros pratos completos. Se dois pratos precisam da mesma técnica (por exemplo, "grelhar carne ao ponto"), essa técnica vira um método auxiliar reutilizável.

Em entidades de domínio, métodos públicos são como "pratos do cardápio" - cada um tem sua responsabilidade específica e side-effects próprios. Eles não devem chamar outros "pratos" que trazem side-effects adicionais.

---

### O Problema Técnico

Quando um método público chama outro método público, side-effects são acumulados de forma imprevisível:

```csharp
// ❌ PROBLEMA: Clone() chamando CreateFromExistingInfo()
public override SimpleAggregateRoot Clone()
{
    return CreateFromExistingInfo(new CreateFromExistingInfoInput(
        EntityInfo, FirstName, LastName, FullName, BirthDate
    ));
}

// Se CreateFromExistingInfo() tiver side-effects...
public static SimpleAggregateRoot CreateFromExistingInfo(CreateFromExistingInfoInput input)
{
    var instance = new SimpleAggregateRoot(...);

    // Side-effects que fazem sentido para reconstitution
    _logger.LogInformation("Entity {Id} loaded from persistence", instance.EntityInfo.Id);
    _metrics.RecordEntityLoaded();
    RaiseEvent(new EntityReconstitutedEvent(...));

    return instance;
}

// Problema: Clone() agora TAMBÉM registra log, métricas e evento!
// Clone é usado em TODA modificação de entidade (Clone-Modify-Return).
// Modificar um nome geraria evento de "entidade reconstituída"? Não faz sentido!
```

**Consequências observadas**:
1. **Side-effects duplicados**: Eventos, logs e métricas aparecem em contextos inadequados
2. **Dificuldade de manutenção**: Mudanças em um método afetam outros sem intenção
3. **Bugs sutis**: Rastrear por que um evento foi disparado se torna complexo
4. **Violação de SRP**: Cada método público perde sua responsabilidade única

## Como Normalmente é Feito

### Abordagem Tradicional

A maioria dos projetos permite que métodos públicos chamem livremente outros métodos públicos, justificando com "reutilização de código":

```csharp
// ⚠️ Padrão comum mas problemático
public class Person
{
    // Método público 1
    public static Person CreateFromDatabase(DbPersonDto dto)
    {
        var person = new Person();
        person.Id = dto.Id;
        person.FirstName = dto.FirstName;
        person.LastName = dto.LastName;

        // Logging e eventos
        Logger.Log($"Person {dto.Id} loaded from database");
        EventBus.Publish(new PersonLoadedEvent(person));

        return person;
    }

    // Método público 2 chamando o público 1 para "reutilizar código"
    public Person Clone()
    {
        var dto = new DbPersonDto
        {
            Id = this.Id,
            FirstName = this.FirstName,
            LastName = this.LastName
        };

        // Parece conveniente, mas traz side-effects!
        return CreateFromDatabase(dto);
        // Agora Clone() registra log de "loaded from database"
        // e publica evento PersonLoadedEvent - incorreto!
    }
}
```

### Por Que Não Funciona Bem

1. **Acoplamento entre operações públicas**: `Clone()` agora depende de implementação interna de `CreateFromDatabase()`. Qualquer mudança em side-effects de um afeta o outro.

2. **Side-effects inesperados se acumulam**:

```csharp
// CreateFromDatabase faz:
// - Validação de dados do banco
// - Logging de "entity loaded"
// - Evento EntityLoadedEvent
// - Incremento de métrica de cache hit

// Clone chama CreateFromDatabase, então Clone TAMBÉM faz tudo isso!
// Mas Clone é chamado em TODA modificação de entidade.
// Resultado: logs poluídos, eventos duplicados, métricas incorretas
```

3. **Caminhos de execução imprevisíveis**:

```csharp
// Desenvolvedor A adiciona cache em CreateFromDatabase
public static Person CreateFromDatabase(DbPersonDto dto)
{
    _cache.Set($"person:{dto.Id}", dto);  // Adiciona ao cache
    // ...resto da lógica
}

// Agora Clone() TAMBÉM adiciona ao cache!
// Bug sutil: modificações de entidade poluem o cache com versões intermediárias
```

4. **Dificuldade de debugging**:

```csharp
// Stack trace mostra:
// Clone() -> CreateFromDatabase() -> RaiseEvent(EntityLoadedEvent)
// Por que Clone está disparando EntityLoadedEvent?
// Precisa ler implementação de ambos os métodos para entender
```

## A Decisão

### Nossa Abordagem

**Métodos públicos nunca chamam outros métodos públicos**. Se precisam da mesma lógica, extraem para:
- **Construtor privado** (quando é apenas construção de estado)
- **Método *Internal compartilhado** (quando há lógica de negócio reutilizável)

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // -----------------------------------------------------------------------
    // MÉTODOS PÚBLICOS - Cada um com caminho isolado
    // -----------------------------------------------------------------------

    // Método público 1: Reconstitution
    public static SimpleAggregateRoot CreateFromExistingInfo(
        CreateFromExistingInfoInput input
    )
    {
        // Usa construtor privado DIRETAMENTE
        // Sem side-effects além da criação
        return new SimpleAggregateRoot(
            input.EntityInfo,
            input.FirstName,
            input.LastName,
            input.FullName,
            input.BirthDate
        );
    }

    // Método público 2: Clone (para imutabilidade)
    public override SimpleAggregateRoot Clone()
    {
        // ✅ Usa o MESMO construtor privado
        // NÃO chama CreateFromExistingInfo()
        // Sem side-effects adicionais
        return new SimpleAggregateRoot(
            EntityInfo,
            FirstName,
            LastName,
            FullName,
            BirthDate
        );
    }

    // -----------------------------------------------------------------------
    // CONSTRUTOR PRIVADO - Compartilhado sem side-effects
    // -----------------------------------------------------------------------

    private SimpleAggregateRoot(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        string fullName,
        BirthDate birthDate
    ) : base(entityInfo)
    {
        // Apenas atribuições - zero side-effects
        FirstName = firstName;
        LastName = lastName;
        FullName = fullName;
        BirthDate = birthDate;
    }
}
```

### Por Que Funciona Melhor

1. **Side-effects controlados**: Cada método público tem apenas seus próprios side-effects

```csharp
// Se CreateFromExistingInfo adicionar logging no futuro...
public static SimpleAggregateRoot CreateFromExistingInfo(CreateFromExistingInfoInput input)
{
    var instance = new SimpleAggregateRoot(...);  // Construtor SEM side-effects

    // Side-effect APENAS para CreateFromExistingInfo
    _logger.LogInformation("Entity {Id} reconstituted", instance.EntityInfo.Id);

    return instance;
}

// ...Clone() NÃO é afetado!
public override SimpleAggregateRoot Clone()
{
    // Usa construtor diretamente - sem logging
    return new SimpleAggregateRoot(...);
}
```

2. **Manutenção previsível**: Mudanças em um método não afetam outros

3. **Debugging simplificado**: Stack trace mostra o caminho real de execução

4. **Testabilidade**: Cada método pode ser testado isoladamente

### Exemplo com Métodos *Internal Compartilhados

Se dois métodos públicos precisam da mesma **lógica de negócio**, não apenas construção:

```csharp
public sealed class SimpleAggregateRoot
{
    // Método público 1: Criação
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext ctx,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(ctx, input, entityFactory: (c, i) => new SimpleAggregateRoot(),
            handler: (c, i, instance) =>
            {
                // Chama método *Internal para lógica de validação
                return instance.ChangeNameInternal(c, i.FirstName, i.LastName);
            }
        );
    }

    // Método público 2: Modificação
    public SimpleAggregateRoot? ChangeName(
        ExecutionContext ctx,
        ChangeNameInput input
    )
    {
        return RegisterChangeInternal(ctx, this, input,
            handler: (c, i, newInstance) =>
            {
                // ✅ Reutiliza o MESMO método *Internal
                // NÃO chama RegisterNew() que tem side-effects diferentes
                return newInstance.ChangeNameInternal(c, i.FirstName, i.LastName);
            }
        );
    }

    // Método *Internal compartilhado - SEM side-effects
    private bool ChangeNameInternal(
        ExecutionContext ctx,
        string firstName,
        string lastName
    )
    {
        // Apenas lógica de validação e atribuição
        return SetFirstName(ctx, firstName)
            & SetLastName(ctx, lastName)
            & SetFullName(ctx, $"{firstName} {lastName}");
    }
}
```

### Comparação

| Aspecto | Método Público Chama Público | Nossa Abordagem |
|---------|------------------------------|-----------------|
| **Side-effects** | Acumulados, imprevisíveis | Isolados, previsíveis |
| **Manutenção** | Mudança em A afeta B | Cada método independente |
| **Debugging** | Stack traces confusos | Caminho direto |
| **Testabilidade** | Difícil isolar comportamento | Cada método testável isoladamente |
| **Reutilização** | Via métodos públicos (perigoso) | Via construtor privado ou *Internal |
| **SRP** | Violado (múltiplas responsabilidades) | Preservado (uma responsabilidade) |

## Benefícios

1. **Isolamento de responsabilidades**: Cada método público tem caminho de execução único e previsível
2. **Side-effects explícitos**: Apenas os side-effects intencionais do método chamado
3. **Manutenção segura**: Mudanças em um método não afetam outros inadvertidamente
4. **Debugging facilitado**: Stack trace mostra exatamente o que está acontecendo
5. **Testabilidade**: Cada método pode ser testado isoladamente sem mock de outros métodos públicos
6. **Reutilização correta**: Lógica compartilhada em métodos privados, não públicos

## Trade-offs (Com Perspectiva)

- **Lógica duplicada aparente**: Pode parecer que há duplicação se múltiplos métodos públicos usam o mesmo construtor privado
  - **Mitigação**: A "duplicação" é apenas a chamada ao construtor, não a lógica em si. É explicitação de intenção, não duplicação real.

- **Não é padrão tradicional OOP**: Muitos desenvolvedores estão acostumados a métodos públicos chamarem outros
  - **Mitigação**: Documentação clara e exemplos consistentes no codebase facilitam o entendimento

## Trade-offs Frequentemente Superestimados

**"Métodos públicos chamando outros públicos é reutilização de código"**

A "reutilização" traz mais problemas do que benefícios:

```csharp
// "Reutilização" problemática
public Person Clone()
{
    return CreateFromDatabase(ConvertToDto()); // Parece conveniente
    // Mas agora Clone tem todos os side-effects de CreateFromDatabase!
}

// Reutilização correta
public Person Clone()
{
    return new Person(...); // Usa construtor privado
}

public static Person CreateFromDatabase(DbPersonDto dto)
{
    var person = new Person(...); // Mesmo construtor
    // Side-effects APENAS aqui
    return person;
}
```

A verdadeira reutilização está no **construtor privado** e **métodos *Internal**, não em métodos públicos.

**"Vai criar muitos métodos privados"**

Métodos privados são reutilização saudável. O problema é reutilização via métodos públicos que carregam side-effects.

```csharp
// ✅ Reutilização saudável - método privado sem side-effects
private bool ChangeNameInternal(...)
{
    // Lógica pura de validação e atribuição
}

// Usado por RegisterNew E ChangeName sem problemas
```

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre interfaces claras:

> "AGGREGATES [...] should have narrow interfaces. [...] Keep the Aggregate Roots and their methods focused on expressing the domain logic."
>
> *AGGREGATES [...] devem ter interfaces estreitas. [...] Mantenha os Aggregate Roots e seus métodos focados em expressar a lógica de domínio.*

Métodos públicos que chamam outros métodos públicos criam interfaces largas e confusas. Cada método público deve expressar uma operação de domínio específica, sem lateralidade.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre coesão:

> "Cohesion is all about keeping related things together and unrelated things separate."
>
> *Coesão é sobre manter coisas relacionadas juntas e coisas não relacionadas separadas.*

Side-effects pertencem ao método que os necessita. `Clone()` não deve ter side-effects de `CreateFromExistingInfo()` porque são operações não relacionadas.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre funções:

> "Functions should do one thing. They should do it well. They should do it only."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso.*

Método público que chama outro método público faz "duas coisas": sua própria operação + os side-effects do método chamado.

Sobre side effects:

> "Side effects are lies. Your function promises to do one thing, but it also does other hidden things."
>
> *Efeitos colaterais são mentiras. Sua função promete fazer uma coisa, mas também faz outras coisas escondidas.*

`Clone()` que chama `CreateFromExistingInfo()` promete clonar, mas secretamente também dispara eventos de reconstitution.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre o Single Responsibility Principle:

> "The Single Responsibility Principle (SRP) states that a module should have one, and only one, reason to change."
>
> *O Princípio de Responsabilidade Única (SRP) afirma que um módulo deve ter uma, e apenas uma, razão para mudar.*

Se `Clone()` chama `CreateFromExistingInfo()`, então `Clone()` muda por duas razões:
1. Quando a lógica de clonagem muda
2. Quando a lógica de reconstitution muda (incluindo seus side-effects)

Isso viola SRP diretamente.

### Command Pattern

Gang of Four em "Design Patterns" (1994) sobre Commands:

> "Encapsulate a request as an object, thereby letting you parameterize clients with different requests, queue or log requests, and support undoable operations."
>
> *Encapsule uma requisição como um objeto, permitindo parametrizar clientes com diferentes requisições, enfileirar ou logar requisições, e suportar operações reversíveis.*

Cada método público de entidade é como um Command - encapsula uma operação completa com seus side-effects. Commands não devem chamar outros Commands, pois isso cria dependências e side-effects compostos.

## Antipadrões Documentados

### Antipadrão 1: Clone Chamando CreateFromExistingInfo

```csharp
// ❌ Side-effects de reconstitution vazam para clone
public override SimpleAggregateRoot Clone()
{
    return CreateFromExistingInfo(new CreateFromExistingInfoInput(
        EntityInfo, FirstName, LastName, FullName, BirthDate
    ));
}

// Se CreateFromExistingInfo registrar evento...
public static SimpleAggregateRoot CreateFromExistingInfo(...)
{
    var instance = new SimpleAggregateRoot(...);
    RaiseEvent(new EntityReconstitutedEvent(...)); // Evento
    return instance;
}

// Toda modificação de entidade agora dispara EntityReconstitutedEvent!
// ChangeName -> Clone -> CreateFromExistingInfo -> EntityReconstitutedEvent 😱
```

### Antipadrão 2: Método Save Chamando Validate

```csharp
// ❌ Save chama Validate (ambos públicos)
public bool Save()
{
    if (!Validate())  // Método público chamando outro público
        return false;

    // Problema: Se Validate tiver side-effects (logging, eventos)
    // Save também os terá
    _repository.Save(this);
    return true;
}

public bool Validate()
{
    _logger.Log("Validating entity");  // Side-effect
    return /* validação */;
}

// Save agora faz logging duplicado!
```

### Antipadrão 3: Factory Method Chamando Outro Factory

```csharp
// ❌ RegisterNew chamando CreateDefault
public static Person? RegisterNew(ExecutionContext ctx, string name)
{
    var person = CreateDefault();  // Método público
    person.Name = name;
    // Validar e retornar
}

public static Person CreateDefault()
{
    _metrics.RecordDefaultPersonCreated();  // Side-effect
    return new Person();
}

// Toda criação de pessoa agora incrementa métrica de "default created"
// Mesmo quando não é default!
```

### Antipadrão 4: Método de Modificação Chamando RegisterNew

```csharp
// ❌ Reset chamando RegisterNew
public Person? Reset()
{
    // "Reutiliza" RegisterNew para resetar estado
    return RegisterNew(_context, _defaultValues);
    // Problema: RegisterNew tem side-effects de CRIAÇÃO
    // Reset deveria ter side-effects de MODIFICAÇÃO
}
```

## Decisões Relacionadas

- [DE-002](./DE-002-construtores-privados-com-factory-methods.md) - Construtores privados permitem compartilhamento sem side-effects
- [DE-003](./DE-003-imutabilidade-controlada-clone-modify-return.md) - Clone deve ser puro, sem side-effects extras
- [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) - RegisterNew e CreateFromExistingInfo são métodos distintos que não devem se chamar
- [DE-021](./DE-021-metodos-publicos-vs-metodos-internos.md) - Métodos *Internal são a forma correta de reutilização

## Leitura Recomendada

- [Clean Code - Robert C. Martin](https://blog.cleancoder.com/) - Capítulo sobre Functions e Side Effects
- [Single Responsibility Principle](https://blog.cleancoder.com/uncle-bob/2014/05/08/SingleReponsibilityPrinciple.html)
- [Command Pattern - GoF](https://refactoring.guru/design-patterns/command)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/) - Capítulo sobre Aggregates

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Implementa o padrão onde métodos públicos delegam para métodos *Internal, nunca chamando outros métodos públicos |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Método Público NUNCA Chama Outro Público
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Clone Usa Construtor Privado
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Clone - implementação correta
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - CreateFromExistingInfo - implementação correta
