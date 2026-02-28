# MS-001: Envelope Encapsulado em MessageMetadata (Não Flat)

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você envia uma carta pelo correio. O envelope contém: remetente, destinatário, carimbo de data, código de rastreio. O conteúdo da carta (payload) fica dentro. Quando o carteiro precisa rotear a carta, ele lê APENAS o envelope — não precisa abrir a carta.

Agora imagine que remetente, destinatário e carimbo estivessem escritos diretamente no papel da carta, misturados com o texto. O carteiro teria que ler a carta inteira para encontrar as informações de roteamento. E se o formato da carta mudasse, o carteiro quebraria.

### O Problema Técnico

Mensagens (Commands, Events, Queries) que cruzam fronteiras de processo precisam de metadata para roteamento, tracing e multi-tenancy. Se esses campos ficam flat na interface (`Guid MessageId`, `DateTimeOffset Timestamp`, `string SchemaName`, `Guid CorrelationId`, ...), surgem problemas:

- **Construtores poluídos**: Tipos concretos precisam passar 7+ parâmetros de envelope antes do payload
- **Deserialização acoplada**: O consumer precisa conhecer o tipo completo para ler o envelope
- **Extensibilidade frágil**: Adicionar um campo ao envelope quebra a assinatura de todos os tipos concretos

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos frameworks de mensageria coloca os campos de metadata diretamente na interface ou classe base:

```csharp
// Flat — campos misturados na interface
public interface IMessage
{
    Guid MessageId { get; }
    DateTimeOffset Timestamp { get; }
    string SchemaName { get; }
    Guid CorrelationId { get; }
    Guid TenantCode { get; }
    string ExecutionUser { get; }
    string ExecutionOrigin { get; }
    string BusinessOperationCode { get; }
}

// Tipo concreto — 7 parâmetros de envelope + payload
public sealed record UserRegisteredEvent(
    Guid MessageId,
    DateTimeOffset Timestamp,
    string SchemaName,
    Guid CorrelationId,
    Guid TenantCode,
    string ExecutionUser,
    string ExecutionOrigin,
    string BusinessOperationCode,
    Guid UserId,         // payload começa aqui
    string Email
) : EventBase(MessageId, Timestamp, SchemaName, CorrelationId, ...);
```

### Por Que Não Funciona Bem

- **Construtores ilegíveis**: 7 parâmetros de envelope + N de payload. Fácil trocar a ordem
- **Deserialização impossível sem tipo concreto**: Para ler o envelope, o consumer precisaria conhecer o tipo completo
- **Quebra em cascata**: Adicionar `string SourceRegion` ao envelope exige alterar TODAS as mensagens

## A Decisão

### Nossa Abordagem

O envelope é encapsulado em um record separado (`MessageMetadata`). A interface `IMessage` expõe apenas `MessageMetadata Metadata`:

```csharp
// Envelope como objeto
public sealed record MessageMetadata(
    Guid MessageId,
    DateTimeOffset Timestamp,
    string SchemaName,
    Guid CorrelationId,
    Guid TenantCode,
    string ExecutionUser,
    string ExecutionOrigin,
    string BusinessOperationCode
);

// Interface limpa
public interface IMessage
{
    MessageMetadata Metadata { get; }
}

// Tipo concreto — 1 parâmetro de envelope + payload
public sealed record UserRegisteredEvent(
    MessageMetadata Metadata,
    Guid UserId,
    string Email
) : EventBase(Metadata);
```

### Por Que Funciona Melhor

1. **Construtor limpo**: 1 parâmetro de envelope (`Metadata`) em vez de 7
2. **Deserialização independente**: `Deserialize<MessageEnvelope>(raw)` sem conhecer o tipo concreto
3. **Extensibilidade**: Adicionar campo ao `MessageMetadata` não quebra nenhum tipo concreto
4. **Campos derivados do ExecutionContext**: CorrelationId, TenantCode, ExecutionUser, ExecutionOrigin e BusinessOperationCode espelham o ExecutionContext para que consumidores reconstruam contexto sem acesso ao original

## Consequências

### Benefícios

- **Separação clara**: Envelope (metadata) vs. conteúdo (payload)
- **Roteamento eficiente**: Consumer lê apenas o envelope para decidir o handler
- **Evolução segura**: Novos campos de metadata não quebram tipos existentes
- **Reuso**: O mesmo `MessageMetadata` serve para Commands, Events e Queries

### Trade-offs (Com Perspectiva)

- **Nível extra de indireção**: `message.Metadata.CorrelationId` em vez de `message.CorrelationId`
  - Na prática, isso é negligenciável — o acesso é direto via propriedade, sem alocação extra
- **Objeto adicional na serialização**: `MessageMetadata` é um nó a mais no JSON/Protobuf
  - O custo é desprezível comparado ao payload e ao IO de rede

## Fundamentação Teórica

### Padrões de Design Relacionados

**Envelope Pattern** (Enterprise Integration Patterns, Hohpe & Woolf): Separar metadata de roteamento do conteúdo da mensagem é exatamente o Envelope Pattern. O envelope viaja com a mensagem mas pode ser inspecionado independentemente.

> "The Envelope Wrapper packages the message inside an envelope that is compliant with the messaging infrastructure."
>
> *O Envelope Wrapper empacota a mensagem dentro de um envelope que é compatível com a infraestrutura de mensageria.*

### O Que o DDD Diz

Em contextos de DDD, mensagens cruzam Bounded Contexts. O envelope deve ser portátil — sem dependências de domain models — para que consumidores em outros BCs possam processar a metadata sem referenciar o domínio do produtor.

### O Que o Clean Architecture Diz

Clean Architecture enfatiza que camadas externas não devem forçar mudanças em camadas internas. Encapsular metadata em um objeto separado protege os tipos concretos (camada interna) de mudanças na infraestrutura de mensageria (camada externa).

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como o Envelope Pattern se aplica a sistemas de mensageria distribuídos?"
- "Quais são as vantagens de metadata como objeto vs. campos flat em mensagens?"
- "Como MessageMetadata facilita deserialização em dois estágios?"

### Leitura Recomendada

- [Enterprise Integration Patterns - Envelope Wrapper](https://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html)
- [Message Envelope Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/publisher-subscriber)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | MessageMetadata, IMessage, MessageBase implementam este padrão |

## Referências no Código

- [MessageMetadata.cs](../../../src/BuildingBlocks/Messages/MessageMetadata.cs) - record selado do envelope
- [IMessage.cs](../../../src/BuildingBlocks/Messages/Interfaces/IMessage.cs) - interface com `MessageMetadata Metadata`
- [MessageBase.cs](../../../src/BuildingBlocks/Messages/MessageBase.cs) - abstract record que recebe Metadata
