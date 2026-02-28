# MS-010: Eventos Self-Contained: Input + OldState + NewState

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um sistema de controle de versão como o Git. Quando você faz um commit, o Git armazena: o que você mudou (diff), como estava antes (old), e como ficou depois (new). Com essas três informações, você pode fazer replay, reverter, comparar, e auditar — sem precisar de mais nada.

Agora imagine se o Git guardasse apenas "arquivo X foi modificado" — sem o diff, sem o antes, sem o depois. Para saber o que mudou, você teria que consultar o histórico inteiro.

### O Problema Técnico

Em sistemas de Event Sourcing ou mensageria, eventos precisam suportar:
- **Replay**: Reprocessar eventos do zero para reconstruir estado
- **Auditoria**: "O que pediu? Como estava? Como ficou?"
- **Diff**: Comparar estado anterior e posterior
- **Compensação**: Reverter uma mudança usando o estado anterior

Se o evento carrega apenas o `NewState`, falta o contexto completo. Se carrega apenas propriedades alteradas individualmente (`OldName`, `NewName`), fica ineficiente para agregados com múltiplas propriedades e impossibilita comparações genéricas.

Sem o `Input` no evento, o produtor precisaria manter um command store separado para replay — complexidade desnecessária.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos usa uma das abordagens:

**Abordagem 1: Apenas campos alterados**
```csharp
// Propriedades individuais — ineficiente para comparação
public record NameChangedEvent(
    Guid UserId,
    string OldFirstName, string NewFirstName,
    string OldLastName, string NewLastName,
    string OldFullName, string NewFullName
);
```

**Abordagem 2: Apenas estado novo**
```csharp
// Sem estado anterior — não suporta diff nem compensação
public record NameChangedEvent(Guid UserId, string NewName);
```

**Abordagem 3: Command store separado**
```csharp
// Dois stores para correlacionar — complexidade desnecessária
CommandStore: ChangeNameCommand(UserId, NewName)
EventStore:   NameChangedEvent(UserId, OldName, NewName)
```

### Por Que Não Funciona Bem

- **Abordagem 1**: Escala O(n) com o número de propriedades. Adicionar um campo ao agregado exige alterar todos os eventos. Impossibilita factories e adapters genéricos
- **Abordagem 2**: Impossibilita auditoria ("como estava antes?"), compensação, e diff
- **Abordagem 3**: Correlacionar command + event é frágil (IDs, timestamps, race conditions). Dobra a complexidade de infraestrutura

## A Decisão

### Nossa Abordagem

Cada evento carrega três peças:

**Evento de criação** (entidade não existia antes):
```csharp
public sealed record SimpleAggregateRootRegisteredEvent(
    MessageMetadata Metadata,
    RegisterSimpleAggregateRootInputModel Input,    // o que pediu
    SimpleAggregateRootModel NewState               // como ficou
) : EventBase(Metadata);
```

**Evento de mudança** (entidade já existia):
```csharp
public sealed record SimpleAggregateRootNameChangedEvent(
    MessageMetadata Metadata,
    ChangeSimpleAggregateRootNameInputModel Input,  // o que pediu
    SimpleAggregateRootModel OldState,              // como estava
    SimpleAggregateRootModel NewState               // como ficou
) : EventBase(Metadata);
```

Capacidades:
- **Replay**: `Input` + `Metadata` é suficiente para reprocessar o comando
- **Auditoria**: "Usuário X pediu (Input) alterar de (OldState) para (NewState)"
- **Diff**: `OldState` vs `NewState` — genérico, funciona com qualquer adapter
- **Compensação**: `OldState` é o estado para reverter

### Por Que Funciona Melhor

1. **Replay sem command store**: O `Input` no evento elimina a necessidade de armazenar commands separadamente
2. **Diff genérico**: Comparar `OldState` e `NewState` como objetos, não campo a campo
3. **Auditoria completa**: Quem pediu (Metadata), o quê pediu (Input), como estava (OldState), como ficou (NewState)
4. **Factories e adapters genéricos**: Um único adapter pode processar qualquer par `OldState/NewState` sem conhecer campos individuais
5. **Escala O(1)**: Adicionar propriedade ao agregado atualiza apenas o Model — eventos não mudam

## Consequências

### Benefícios

- **Event store é suficiente**: Replay completo sem command store separado
- **Self-contained**: Cada evento carrega contexto completo para qualquer operação
- **Genérico**: Adapters, projections, e audit trails funcionam com qualquer evento
- **Evolução segura**: Novo campo no model não exige alterar a estrutura do evento

### Trade-offs (Com Perspectiva)

- **Payload maior**: Dois snapshots completos (OldState + NewState) em eventos de mudança
  - Na prática, um aggregate root tem ~10 propriedades primitivas. Dois snapshots = ~500 bytes. O custo de serializar isso é negligenciável comparado ao IO economizado (zero round-trips em todos os consumers). Pela experiência empírica, o trade-off compensa amplamente
- **Redundância**: `OldState` + `NewState` contêm campos que não mudaram
  - Essa "redundância" é o que permite diff genérico. Sem ela, cada evento precisaria declarar quais campos mudaram — voltando à Abordagem 1
- **Input duplica informação**: `Input.FirstName` está contido em `NewState.FirstName`
  - O `Input` representa a INTENÇÃO (o que o usuário pediu), o `NewState` representa o RESULTADO (o que o sistema produziu). Podem divergir — ex: o sistema normaliza o nome (trim, capitalize). Ambos são necessários para replay e auditoria

## Fundamentação Teórica

### Padrões de Design Relacionados

**Event Sourcing (CQRS)**: Em Event Sourcing, eventos são a fonte de verdade. Carregar Input + OldState + NewState torna cada evento completamente autônomo — o event store é suficiente para reconstruir qualquer estado passado.

**Memento Pattern (GoF)**: OldState e NewState são mementos — capturas do estado do objeto em momentos específicos. O evento funciona como um "commit" com before/after.

> "Without violating encapsulation, capture and externalize an object's internal state so that the object can be restored to this state later."
>
> *Sem violar encapsulamento, capture e externalize o estado interno de um objeto para que o objeto possa ser restaurado a este estado posteriormente.*

### O Que o DDD Diz

Vernon em "Implementing Domain-Driven Design" argumenta que Domain Events devem ser self-describing:

> "Domain Events should be enriched with enough data for consumers to perform their tasks without needing to call back to the originating Bounded Context."
>
> *Domain Events devem ser enriquecidos com dados suficientes para que consumidores executem suas tarefas sem precisar consultar o Bounded Context de origem.*

Input + OldState + NewState é a materialização máxima desse princípio.

### Outros Fundamentos

**ACID em Event Stores**: Um evento com Input + OldState + NewState é atomicamente consistente. Se o event store persiste o evento, TODAS as informações para replay existem. Não há dependência de outro store (command store) que poderia estar inconsistente.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como Event Sourcing elimina a necessidade de command store com eventos enriched?"
- "Quais são os trade-offs de eventos fat vs skinny em sistemas distribuídos?"
- "Como o Memento Pattern se aplica a Event Sourcing?"

### Leitura Recomendada

- [Event Sourcing - Martin Fowler](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Event-Carried State Transfer](https://martinfowler.com/articles/201701-event-driven.html)
- [Implementing Domain-Driven Design - Chapter 8: Domain Events](https://www.oreilly.com/library/view/implementing-domain-driven-design/9780133039900/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | EventBase fornece o envelope; templates demonstram o padrão Input + OldState + NewState |

## Referências no Código

- [SimpleAggregateRootRegisteredEvent.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Events/SimpleAggregateRootRegisteredEvent.cs) - criação: `Input` + `NewState`
- [SimpleAggregateRootNameChangedEvent.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Events/SimpleAggregateRootNameChangedEvent.cs) - mudança: `Input` + `OldState` + `NewState`
- [RegisterSimpleAggregateRootInputModel.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Models/RegisterSimpleAggregateRootInputModel.cs) - input model com primitivos
- [SimpleAggregateRootModel.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Models/SimpleAggregateRootModel.cs) - snapshot model para OldState/NewState
