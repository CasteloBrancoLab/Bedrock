# MS-009: Snapshot Completo do Aggregate Root nos Models

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você é um jornalista cobrindo uma votação no parlamento. Se seu editor pede um relatório, você não escreve "O deputado X votou" — você escreve quem votou, em qual projeto, a que horas, o resultado, e o placar. O editor não precisa ligar para o parlamento para completar a informação. A matéria é self-contained.

### O Problema Técnico

Quando um consumer recebe um evento, ele precisa agir (atualizar uma read model, enviar notificação, disparar workflow). Se o evento carrega apenas o `Id` do aggregate root, o consumer precisa:
1. Chamar o repositório do produtor para buscar os dados
2. Conhecer a interface do repositório (acoplamento)
3. Fazer IO adicional (latência, falha potencial)
4. Lidar com race conditions (o estado pode ter mudado entre o evento e a query)

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos frameworks enviam eventos "magros" — apenas o Id e os campos alterados:

```csharp
// Evento magro — consumer precisa buscar o resto
public record UserRegisteredEvent(Guid UserId);

// Ou só os campos alterados
public record UserNameChangedEvent(Guid UserId, string NewName);
```

O consumer faz:
```csharp
void Handle(UserRegisteredEvent evt)
{
    // Round-trip ao banco do produtor!
    var user = await _userRepository.GetByIdAsync(evt.UserId);
    await _readModel.UpdateAsync(user.Id, user.Name, user.Email, ...);
}
```

### Por Que Não Funciona Bem

- **Round-trip desnecessário**: O produtor JÁ tinha os dados — por que forçar o consumer a buscar?
- **Acoplamento**: Consumer precisa de acesso ao repositório do produtor
- **Race condition**: Estado pode ter mudado entre a publicação do evento e a query
- **Indisponibilidade**: Se o serviço do produtor estiver fora, o consumer não consegue processar
- **Processamento obrigatoriamente sequencial**: Events sem estado completo forçam ordem temporal

### Exemplo Concreto: Consumo Sequencial Forçado

Considere um sistema de e-commerce com dois eventos publicados em sequência rápida:

```
Evento 1: OrderCreatedEvent(OrderId: 42)
Evento 2: OrderItemAddedEvent(OrderId: 42, ProductId: 99)
```

Um consumer precisa montar uma read model de "pedidos com itens". Com eventos magros:

```csharp
// Consumer A — precisa buscar dados extras
async Task Handle(OrderItemAddedEvent evt)
{
    // Preciso do nome do produto e do endereço do cliente
    // Mas esses dados não estão no evento!

    var order = await _orderApi.GetByIdAsync(evt.OrderId);   // round-trip 1
    var product = await _productApi.GetByIdAsync(evt.ProductId); // round-trip 2

    await _readModel.AddItemAsync(
        evt.OrderId,
        order.CustomerName,      // veio do round-trip
        product.Name,            // veio do round-trip
        product.Price
    );
}
```

Problemas que surgem:

**1. Ordenação temporal obrigatória**: Se o Evento 2 chega ANTES do Evento 1 ser processado
(partições diferentes, retry, rebalanceamento), o `_orderApi.GetByIdAsync(42)` retorna `null`
porque o pedido ainda não foi persistido. O consumer é FORÇADO a processar na ordem exata
de publicação — perdendo paralelismo e resiliência.

**2. Cascata de indisponibilidade**: Se `_productApi` está fora do ar, o consumer para.
Um serviço que deveria ser independente agora depende da disponibilidade de outro.
Em um sistema com 10 consumers, cada um buscando dados extras, a indisponibilidade
de UM serviço trava TODOS.

**3. Inconsistência temporal**: Entre o momento que o evento foi publicado e o momento
que o consumer faz o round-trip, o produto pode ter mudado de preço. O consumer
registra o preço ATUAL, não o preço no momento do pedido — dado incorreto.

**4. Impossibilidade de replay**: Se o consumer precisa reprocessar eventos de 6 meses
atrás, os dados que ele buscaria via API podem não existir mais (produto descontinuado,
cliente excluído). O replay falha silenciosamente.

Com snapshot completo, nenhum desses problemas existe:

```csharp
// Consumer B — tudo no evento
async Task Handle(OrderItemAddedEvent evt)
{
    // Zero round-trips, zero dependências, ordem irrelevante
    await _readModel.AddItemAsync(
        evt.NewState.OrderId,
        evt.NewState.CustomerName,    // veio no snapshot
        evt.NewState.Items.Last().ProductName,  // veio no snapshot
        evt.NewState.Items.Last().Price         // preço no momento do evento
    );
}
```

O consumer processa em qualquer ordem, sem dependências externas, com dados
consistentes do momento exato do evento.

## A Decisão

### Nossa Abordagem

Message Models representam o snapshot COMPLETO do aggregate root — todas as propriedades, incluindo metadata de auditoria:

```csharp
public readonly record struct SimpleAggregateRootModel(
    Guid Id,
    Guid TenantCode,
    string FirstName,
    string LastName,
    string FullName,
    DateTimeOffset BirthDate,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? LastModifiedAt,
    string? LastModifiedBy
);
```

O evento carrega o snapshot completo:
```csharp
public sealed record UserRegisteredEvent(
    MessageMetadata Metadata,
    RegisterUserInputModel Input,
    SimpleAggregateRootModel NewState   // snapshot completo
) : EventBase(Metadata);
```

O consumer tem tudo que precisa:
```csharp
void Handle(UserRegisteredEvent evt)
{
    // Zero round-trips — tudo no evento
    await _readModel.UpdateAsync(
        evt.NewState.Id,
        evt.NewState.FirstName,
        evt.NewState.LastName,
        evt.NewState.FullName,
        evt.NewState.BirthDate,
        evt.NewState.CreatedAt,
        evt.NewState.CreatedBy
    );
}
```

### Por Que Funciona Melhor

1. **Zero round-trips**: Consumer tem todos os dados no evento
2. **Zero acoplamento**: Consumer não precisa do repositório do produtor
3. **Sem race conditions**: O snapshot é do momento exato do evento
4. **Alta disponibilidade**: Consumer processa mesmo com o produtor offline
5. **Dados suficientes para o consumidor agir**: Princípio fundamental — o produtor já tinha os dados

## Consequências

### Benefícios

- **Self-contained**: Evento carrega tudo que qualquer consumer precisa
- **Desacoplamento**: Consumer não precisa de acesso ao banco do produtor
- **Consistência**: Snapshot reflete o estado exato no momento do evento
- **Projections eficientes**: Read models atualizadas com um único evento, sem queries extras

### Trade-offs (Com Perspectiva)

- **Payload maior**: Snapshot completo ocupa mais bytes que apenas o Id
  - Na prática, um aggregate root típico tem 10-20 campos primitivos — poucos bytes comparados ao custo de uma query ao banco. O IO economizado nos consumers supera em ordens de magnitude o custo de serializar bytes extras
- **Dados potencialmente desnecessários**: Nem todo consumer usa todos os campos
  - Melhor ter dados sobrando do que faltar. O custo de carregar campos extras é desprezível; o custo de um round-trip ao banco é significativo
- **Snapshot pode ficar grande com agregados complexos**: Aggregate roots com muitas entidades filhas geram snapshots grandes
  - Agregados grandes são um code smell em si (vide DDD — agregados devem ser pequenos). O tamanho do snapshot é um sinal de que o agregado precisa ser refatorado

## Fundamentação Teórica

### Padrões de Design Relacionados

**Event-Carried State Transfer (Martin Fowler)**: Eventos carregam o estado completo para que consumidores mantenham cópias locais sem consultar o produtor.

> "The idea is that the event carries all the data that the receiver needs to process the event. This eliminates the need for a callback to the sender."
>
> *A ideia é que o evento carrega todos os dados que o receptor precisa para processar o evento. Isso elimina a necessidade de callback ao remetente.*

### O Que o DDD Diz

Vernon em "Implementing Domain-Driven Design" enfatiza que eventos entre Bounded Contexts devem ser auto-suficientes:

> "A Domain Event published by one Bounded Context [...] should carry all necessary information for the consuming Bounded Context to take action."
>
> *Um Domain Event publicado por um Bounded Context [...] deve carregar toda informação necessária para que o Bounded Context consumidor possa agir.*

### O Que o Clean Architecture Diz

Dependências devem apontar para dentro (Dependency Rule). Se o consumer precisa chamar o repositório do produtor para complementar dados do evento, a dependência aponta para fora — violando Clean Architecture. Eventos self-contained eliminam essa dependência.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "O que é Event-Carried State Transfer e quando usar?"
- "Como snapshots em eventos melhoram a resiliência de microsserviços?"
- "Quais são os trade-offs de eventos fat vs skinny?"

### Leitura Recomendada

- [Event-Carried State Transfer - Martin Fowler](https://martinfowler.com/articles/201701-event-driven.html)
- [Implementing Domain-Driven Design - Chapter 8: Domain Events](https://www.oreilly.com/library/view/implementing-domain-driven-design/9780133039900/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | EventBase fornece o envelope; models no template demonstram o snapshot |

## Referências no Código

- [SimpleAggregateRootModel.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Models/SimpleAggregateRootModel.cs) - snapshot completo com todas as propriedades
- [SimpleAggregateRootRegisteredEvent.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Events/SimpleAggregateRootRegisteredEvent.cs) - evento com `NewState` snapshot
