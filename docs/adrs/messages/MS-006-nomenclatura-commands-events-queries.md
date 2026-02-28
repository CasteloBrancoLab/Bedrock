# MS-006: Nomenclatura de Commands, Events e Queries

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma central de rádio militar. Se o operador diz "Ataque confirmado", ninguém sabe se é uma ORDEM para atacar ou um RELATÓRIO de que o ataque já aconteceu. A diferença entre "Atacar a posição" (comando) e "Posição atacada" (relato) é a diferença entre ação futura e fato passado.

### O Problema Técnico

Commands, Events e Queries têm semânticas fundamentalmente diferentes:
- **Commands**: Intenção de executar uma ação (pode ser rejeitada)
- **Events**: Fato que já aconteceu (imutável, não rejeitável)
- **Queries**: Solicitação de leitura (sem side-effects)

Se a nomenclatura não reflete essas semânticas, o desenvolvedor precisa abrir a classe para entender se `UserRegistration` é um comando, um evento, ou uma query. Em um sistema com centenas de mensagens, isso é inviável.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos usam nomes ambíguos ou inconsistentes:

```csharp
// Ambíguo — é comando ou evento?
public record UserRegistration(Guid UserId, string Email);

// Inconsistente — mistura estilos
public record CreateUser(...);          // imperativo
public record UserWasCreated(...);      // passivo com "Was"
public record FetchUserData(...);       // "Fetch" em vez de "Get"
```

### Por Que Não Funciona Bem

- **Ambiguidade**: `UserRegistration` é intenção (command) ou fato (event)?
- **Inconsistência**: Sem padrão, cada desenvolvedor inventa um estilo
- **Confusão em code review**: Reviewer precisa verificar a base class para entender a semântica

## A Decisão

### Nossa Abordagem

Cada tipo de mensagem segue uma convenção de nomenclatura que reflete sua semântica:

**Commands — Verbo Imperativo** (expressam intenção):
```csharp
✅ RegisterUserCommand
✅ CancelOrderCommand
✅ ChangeNameCommand

❌ UserRegisteredCommand    // passado = evento, não comando
❌ UserRegistrationCommand  // substantivo = ambíguo
```

**Events — Passado Simples** (expressam fatos):
```csharp
✅ UserRegisteredEvent
✅ OrderCancelledEvent
✅ NameChangedEvent

❌ RegisterUserEvent    // imperativo = comando, não evento
❌ UserRegistrationEvent  // substantivo = ambíguo
```

**Queries — Substantivo Descritivo + Query** (expressam o que se quer obter):
```csharp
✅ GetUserByIdQuery
✅ ListActiveOrdersQuery
✅ SearchProductsQuery

❌ FetchUser     // sem sufixo Query
❌ UserQuery     // ambíguo — qual user? por qual critério?
```

### Por Que Funciona Melhor

1. **Autodocumentação**: O nome revela a semântica sem abrir a classe
2. **Consistência**: Todo desenvolvedor segue o mesmo padrão
3. **Busca eficiente**: `*Command` lista todos os commands, `*Event` todos os events
4. **Code review**: Reviewer identifica imediatamente se a nomenclatura está correta

## Consequências

### Benefícios

- **Comunicação clara**: Time inteiro usa o mesmo vocabulário
- **Discoverability**: IntelliSense/autocomplete filtra por sufixo
- **Alinhamento com DDD**: Nomenclatura reflete a linguagem ubíqua do domínio

### Trade-offs (Com Perspectiva)

- **Nomes mais longos**: `SimpleAggregateRootNameChangedEvent` vs `NameChanged`
  - Clareza vale mais que brevidade. Em um sistema com centenas de mensagens, o sufixo é essencial
- **Rigidez**: Forçar passado para events pode parecer artificial em alguns casos
  - Na prática, se não soa natural no passado, pode ser que o conceito não seja realmente um evento

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans e Vaughn Vernon enfatizam que a **linguagem ubíqua** deve permeiar o código:

> "Use the model as the backbone of a language. [...] Commit the team to exercising that language relentlessly in all communication within the team and in the code."
>
> *Use o modelo como a espinha dorsal de uma linguagem. [...] Comprometa o time a exercitar essa linguagem incansavelmente em toda comunicação dentro do time e no código.*

Commands no imperativo e Events no passado refletem a semântica real do domínio: "Registrar Usuário" (intenção) vs "Usuário Registrado" (fato).

### Outros Fundamentos

**CQRS (Command Query Responsibility Segregation)**: A separação entre Commands e Queries é fundamental no CQRS. A nomenclatura reforça essa separação no nível do tipo — impossível confundir um Command com uma Query quando os nomes são explícitos.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como a linguagem ubíqua do DDD se aplica à nomenclatura de mensagens?"
- "Quais são as convenções de nomenclatura em Event Sourcing?"
- "Como CQRS influencia a nomenclatura de Commands e Queries?"

### Leitura Recomendada

- [Domain-Driven Design - Ubiquitous Language](https://martinfowler.com/bliki/UbiquitousLanguage.html)
- [CQRS - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | ICommand, IEvent, IQuery marker interfaces reforçam a separação semântica |

## Referências no Código

- [ICommand.cs](../../../src/BuildingBlocks/Messages/Commands/Interfaces/ICommand.cs) - LLM_RULE sobre nomenclatura imperativa
- [IEvent.cs](../../../src/BuildingBlocks/Messages/Events/Interfaces/IEvent.cs) - LLM_RULE sobre nomenclatura no passado
- [IQuery.cs](../../../src/BuildingBlocks/Messages/Queries/Interfaces/IQuery.cs) - LLM_RULE sobre nomenclatura descritiva
