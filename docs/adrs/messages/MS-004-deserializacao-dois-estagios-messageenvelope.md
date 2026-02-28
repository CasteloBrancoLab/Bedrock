# MS-004: Deserialização em Dois Estágios via MessageEnvelope

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um centro de distribuição de encomendas. Quando um pacote chega, o funcionário não precisa abrir a caixa para decidir para qual esteira enviar — ele lê a etiqueta externa. Só depois, no destino final, a caixa é aberta.

Se cada tipo de produto exigisse que a caixa fosse aberta para ler o destino, o centro de distribuição seria absurdamente lento e precisaria conhecer todos os produtos do mundo.

### O Problema Técnico

O consumer recebe bytes brutos de um broker (Kafka, RabbitMQ, etc.). Ele precisa:
1. Descobrir o tipo da mensagem para rotear ao handler correto
2. Deserializar o payload completo para o tipo concreto

Se o consumer precisa conhecer o tipo concreto ANTES de deserializar, temos um ciclo: preciso deserializar para saber o tipo, mas preciso do tipo para deserializar.

Além disso, a solução não pode depender de recursos específicos de um formato de serialização (ex: ler nós JSON com `JsonDocument`), senão acopla ao formato.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos frameworks usam headers do broker ou leem o JSON diretamente:

```csharp
// Acoplado ao formato JSON
var doc = JsonDocument.Parse(raw);
var schemaName = doc.RootElement.GetProperty("Metadata").GetProperty("SchemaName").GetString();
var type = TypeResolver.Resolve(schemaName);
var message = JsonSerializer.Deserialize(raw, type);
```

Ou dependem de headers do broker:

```csharp
// Acoplado ao broker
var typeName = kafkaMessage.Headers["message-type"];
```

### Por Que Não Funciona Bem

- **Acoplamento ao formato**: `JsonDocument.Parse` não funciona com Protobuf, Avro ou MessagePack
- **Acoplamento ao broker**: Headers são feature do Kafka/RabbitMQ — não portátil
- **`MessageBase` é abstract**: `Deserialize<MessageBase>(raw)` falha — abstract não pode ser instanciado

## A Decisão

### Nossa Abordagem

Um tipo concreto (`MessageEnvelope`) contém apenas o `MessageMetadata`. Qualquer serializer consegue deserializá-lo:

```csharp
// Tipo concreto — qualquer serializer consegue hidratar
public sealed record MessageEnvelope(MessageMetadata Metadata);
```

Fluxo do consumer:
```csharp
// Estágio 1: Deserializa apenas o envelope (format-agnostic)
var envelope = serializer.Deserialize<MessageEnvelope>(raw);
var schemaName = envelope.Metadata.SchemaName;

// Estágio 2: Resolve tipo concreto e deserializa payload completo
var concreteType = typeResolver.Resolve(schemaName);
var message = serializer.Deserialize(raw, concreteType);

// Handler recebe o tipo concreto
handler.Handle(message);
```

### Por Que Funciona Melhor

1. **Format-agnostic**: Funciona com JSON, Protobuf, Avro, MessagePack — qualquer serializer
2. **Broker-agnostic**: Não depende de headers ou features do broker
3. **Tipo concreto**: `MessageEnvelope` é `sealed record` — qualquer serializer instancia
4. **Payload não deserializado**: No estágio 1, o payload é ignorado — custo mínimo para roteamento

## Consequências

### Benefícios

- **Desacoplamento total**: Consumer não depende de formato de serialização nem de broker
- **Performance**: Roteamento eficiente — deserializa o mínimo necessário
- **Extensibilidade**: Trocar JSON por Protobuf não exige mudança no fluxo de roteamento

### Trade-offs (Com Perspectiva)

- **Dupla deserialização**: O raw é deserializado duas vezes (envelope + tipo concreto)
  - Na prática, o estágio 1 é extremamente leve (apenas `MessageMetadata` — 8 campos primitivos). O custo é negligenciável comparado ao IO de rede e processamento de negócio
- **MessageEnvelope é "quase vazio"**: Existe apenas para resolver o problema de `MessageBase` ser abstract
  - Isso é intencional — é um tipo de infraestrutura, não de domínio. Sua simplicidade é uma virtude

## Fundamentação Teórica

### Padrões de Design Relacionados

**Content-Based Router (EIP)**: O consumer inspeciona o conteúdo da mensagem (SchemaName no envelope) para decidir o roteamento. MessageEnvelope permite essa inspeção sem deserializar o payload.

> "A Content-Based Router examines the message content and routes the message to a different channel based on data contained in the message."
>
> *Um Content-Based Router examina o conteúdo da mensagem e roteia a mensagem para um canal diferente baseado nos dados contidos na mensagem.*

**Two-Phase Deserialization**: Padrão emergente em sistemas de event sourcing e mensageria, onde o envelope é deserializado separadamente do payload para permitir roteamento e filtering sem custo de deserialização completa.

### O Que o Clean Architecture Diz

A infraestrutura de deserialização (JSON, Protobuf) é um detalhe de implementação. O fluxo de roteamento (`envelope → schemaName → tipo → handler`) deve funcionar independente do formato. `MessageEnvelope` como tipo concreto e simples garante essa independência.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como implementar content-based routing desacoplado do formato de serialização?"
- "Quais são os padrões de deserialização em sistemas de Event Sourcing?"
- "Como o Apache Kafka Schema Registry resolve a descoberta de tipo?"

### Leitura Recomendada

- [Enterprise Integration Patterns - Content-Based Router](https://www.enterpriseintegrationpatterns.com/patterns/messaging/ContentBasedRouter.html)
- [Event Store - Reading Events](https://developers.eventstore.com/clients/grpc/reading-events.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | MessageEnvelope e MessageMetadata implementam os dois estágios |

## Referências no Código

- [MessageEnvelope.cs](../../../src/BuildingBlocks/Messages/MessageEnvelope.cs) - tipo concreto para primeiro estágio
- [MessageMetadata.cs](../../../src/BuildingBlocks/Messages/MessageMetadata.cs) - envelope deserializável com SchemaName
