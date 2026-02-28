# MS-007: Concretos Herdam da Base Tipada

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um formulário governamental. Existem três tipos: requerimento (pedido), certidão (documento emitido), e consulta (pergunta). Todos compartilham um cabeçalho padrão, mas cada um tem seu próprio corpo. Se alguém entregar um requerimento usando o formulário de certidão, o protocolo rejeita.

### O Problema Técnico

Tipos concretos de mensagens precisam:
- Herdar o envelope (MessageMetadata) da hierarquia de bases
- Ser distinguíveis por tipo (Command vs Event vs Query) para roteamento e constraints genéricos
- Ser `sealed` para prevenir herança não intencional

Sem herança obrigatória, um desenvolvedor poderia criar um evento que implementa apenas `IMessage` (sem `IEvent`), ou pior, implementar `ICommand` sem herdar o envelope.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos frameworks usam marker interfaces sem bases obrigatórias:

```csharp
// Só interface — sem envelope obrigatório
public interface IEvent { }

// Tipo concreto "solto"
public record UserRegisteredEvent(Guid UserId, string Email) : IEvent;
// Cadê o envelope? Cadê MessageId, CorrelationId, SchemaName?
```

### Por Que Não Funciona Bem

- **Envelope esquecido**: Sem base obrigatória, o desenvolvedor pode criar mensagens sem metadata
- **Serialização inconsistente**: Mensagens sem SchemaName não podem ser roteadas
- **Type safety perdido**: `IEvent` sem envelope não garante nada sobre a estrutura

## A Decisão

### Nossa Abordagem

Todo tipo concreto DEVE herdar da base tipada correspondente:

```csharp
// Command → CommandBase
public sealed record RegisterUserCommand(
    MessageMetadata Metadata,
    string Email, string FullName
) : CommandBase(Metadata);

// Event → EventBase
public sealed record UserRegisteredEvent(
    MessageMetadata Metadata,
    Guid UserId, string Email
) : EventBase(Metadata);

// Query → QueryBase
public sealed record GetUserByIdQuery(
    MessageMetadata Metadata,
    Guid UserId
) : QueryBase(Metadata);
```

A cadeia de herança garante que todo tipo concreto:
1. Recebe `MessageMetadata` como primeiro parâmetro
2. Tem `SchemaName` auto-computado (via `MessageBase`)
3. Implementa a marker interface correta (`ICommand`, `IEvent`, ou `IQuery`)
4. Pode ser roteado e deserializado uniformemente

### Por Que Funciona Melhor

1. **Envelope obrigatório**: Impossível criar mensagem sem metadata
2. **SchemaName automático**: Herdado de `MessageBase` — zero boilerplate
3. **Type safety**: Constraints genéricos (`where T : ICommand`) funcionam
4. **Padrão consistente**: `Metadata` é sempre o primeiro parâmetro, payload depois

## Consequências

### Benefícios

- **Uniformidade**: Todas as mensagens têm exatamente a mesma estrutura de envelope
- **Discoverability**: Navegar a hierarquia revela todas as mensagens do sistema
- **Compilador enforces**: Esquecer de passar `Metadata` é erro de compilação
- **Roteamento genérico**: Um handler `IEventHandler<T> where T : IEvent` funciona para qualquer evento

### Trade-offs (Com Perspectiva)

- **Herança obrigatória**: Não é possível criar mensagem "leve" sem envelope
  - Mensagens sem envelope não servem para comunicação entre processos. Se é intra-processo, use um domain event simples (readonly record struct)
- **Tipo sealed**: Concretos não podem ser herdados
  - Mensagens representam contratos versionados. Herança de mensagens concretas quebraria compatibilidade

## Fundamentação Teórica

### Padrões de Design Relacionados

**Template Method (GoF)**: A base define o envelope e o mecanismo (SchemaName auto-computado). O tipo concreto define apenas o payload. A estrutura é fixa; o conteúdo varia.

### O Que o DDD Diz

No DDD tático, Commands e Events são first-class citizens. Forçar que todos herdem de uma base tipada garante que o contrato de comunicação entre Bounded Contexts é sempre respeitado — não há mensagem "incompleta" transitando no sistema.

### O Que o Clean Code Diz

Robert C. Martin enfatiza o **Principle of Least Surprise**: o código deve fazer o que o leitor espera. Se um tipo termina com `Event` e herda de `EventBase`, o leitor sabe imediatamente que carrega envelope, SchemaName, e pode ser roteado. Sem essa herança, a expectativa é quebrada.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como herança obrigatória de bases garante contratos de comunicação?"
- "Quais são os benefícios de sealed records para mensagens versionadas?"
- "Como constraints genéricos melhoram type safety em handlers de mensagens?"

### Leitura Recomendada

- [C# Records with Inheritance](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [Generic Constraints in C#](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | CommandBase, EventBase, QueryBase são as bases tipadas obrigatórias |

## Referências no Código

- [CommandBase.cs](../../../src/BuildingBlocks/Messages/Commands/CommandBase.cs) - `abstract record CommandBase(MessageMetadata Metadata) : MessageBase(Metadata), ICommand`
- [EventBase.cs](../../../src/BuildingBlocks/Messages/Events/EventBase.cs) - `abstract record EventBase(MessageMetadata Metadata) : MessageBase(Metadata), IEvent`
- [QueryBase.cs](../../../src/BuildingBlocks/Messages/Queries/QueryBase.cs) - `abstract record QueryBase(MessageMetadata Metadata) : MessageBase(Metadata), IQuery`
- [SimpleAggregateRootRegisteredEvent.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Events/SimpleAggregateRootRegisteredEvent.cs) - exemplo concreto herdando de EventBase
