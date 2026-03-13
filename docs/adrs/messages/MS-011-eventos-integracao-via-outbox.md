# MS-011: Eventos de Integração Exclusivamente via Outbox

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um cartório que precisa notificar outros cartórios sobre mudanças em registros. Se o funcionário simplesmente gritasse pela janela ("Fulano mudou de endereço!"), apenas quem estivesse passando naquele momento ouviria. Se ele enviasse a notificação por carta registrada com comprovante de entrega, todos os destinatários receberiam — mesmo que estivessem fechados no momento do envio. O Outbox é a carta registrada: persiste a mensagem antes de enviá-la, garantindo entrega mesmo com falhas temporárias.

### O Problema Técnico

Em sistemas distribuídos, eventos de integração cruzam fronteiras de processo — entre bounded contexts, microserviços e sistemas externos. Se usarmos o mecanismo de `event` do C# (delegates/multicast), temos problemas fundamentais:

- **Volatilidade**: Eventos em memória são perdidos se o processo reiniciar antes do handler processar
- **Acoplamento temporal**: Produtor e consumidor devem estar ativos simultaneamente
- **Sem garantia de entrega**: Não há retry, dead-letter queue ou persistência
- **Sem auditoria**: Eventos in-memory não deixam rastro — impossível replay ou debugging
- **Violação de fronteira**: `event` do C# é um mecanismo intra-processo, não inter-processo

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos usam o mecanismo de `event` do C# ou domain events in-memory para notificar outros bounded contexts:

```csharp
// Evento via delegate do C# — intra-processo
public class UserService
{
    public event EventHandler<UserDeactivatedArgs>? UserDeactivated;

    public void DeactivateUser(Guid userId)
    {
        // ... lógica
        UserDeactivated?.Invoke(this, new UserDeactivatedArgs(userId));
    }
}
```

### Por Que Não Funciona Bem

- **Perda de eventos**: Se nenhum handler estiver registrado, o evento é descartado silenciosamente
- **Sem transacionalidade**: O evento pode ser disparado mas a operação de banco falhar (ou vice-versa)
- **Sem replay**: Impossível re-processar eventos passados para debugging ou rebuild de projeções
- **Acoplamento de processo**: Consumidor precisa estar no mesmo processo e registrado antes do evento

## A Decisão

### Nossa Abordagem

Eventos de integração são `sealed record` que herdam de `EventBase` e são publicados exclusivamente via Outbox Pattern. Nenhum tipo no sistema deve declarar membros `event` (delegates do C#):

```csharp
// Correto: evento como record via Outbox
public sealed record UserDeactivatedEvent(
    MessageMetadata Metadata,
    DeactivateUserInputModel Input,
    UserModel OldState,
    UserModel NewState,
    DeactivationOutputModel Output
) : EventBase(Metadata);

// Publicação via Outbox (transacional com a operação)
await outbox.PublishAsync(userDeactivatedEvent);
```

O que é proibido:

```csharp
// PROIBIDO: declaração de 'event' (delegate do C#)
public class UserService
{
    public event EventHandler<UserArgs>? UserChanged; // ← violação MS-011
}
```

### Por Que Funciona Melhor

1. **Transacionalidade**: Evento é salvo na mesma transação da operação — atomicidade garantida
2. **Durabilidade**: Evento persiste no banco antes de ser enviado ao broker
3. **Replay**: Eventos podem ser re-processados a qualquer momento
4. **Desacoplamento temporal**: Consumidor pode estar offline; receberá quando voltar
5. **Auditoria**: Trail completo de todos os eventos com timestamps e metadata

## Consequências

### Benefícios

- **Garantia de entrega**: At-least-once delivery via Outbox + relay
- **Consistência eventual**: Operação e evento são atômicos (mesma transação)
- **Observabilidade**: Todos os eventos persistidos e rastreáveis
- **Desacoplamento**: Produtor não conhece consumidores — publica no Outbox e pronto

### Trade-offs (Com Perspectiva)

- **Complexidade adicional**: Outbox requer relay, tabela de outbox, e idempotência no consumidor
  - Essa complexidade é encapsulada no building block `Bedrock.BuildingBlocks.Outbox` — o desenvolvedor apenas chama `PublishAsync`
- **Latência**: Evento não é instantâneo — passa por persistência e relay
  - Para integração entre BCs, latência de milissegundos é aceitável. Consistência eventual é o trade-off correto
- **Sem eventos in-memory**: Mesmo notificações internas usam Outbox
  - Consistência é mais importante que performance para eventos de integração. Eventos internos que precisam de latência zero são outra categoria (domain events síncronos)

## Fundamentação Teórica

### Padrões de Design Relacionados

**Outbox Pattern**: Persiste o evento na mesma transação da operação de negócio. Um processo separado (relay/poller) lê os eventos pendentes e os publica no message broker. Garante at-least-once delivery sem two-phase commit.

**Event Sourcing Lite**: Mesmo sem event sourcing completo, o Outbox cria um log de eventos que pode ser usado para replay, auditoria e debugging.

> "The outbox pattern ensures that a message is sent if and only if the database transaction commits."
>
> *O padrão Outbox garante que uma mensagem é enviada se e somente se a transação de banco for commitada.*

### O Que o Clean Architecture Diz

Clean Architecture separa mecanismos de entrega (frameworks, drivers) das regras de negócio. O `event` do C# é um mecanismo de framework (.NET runtime) — usá-lo para integração entre BCs viola a separação de camadas. O Outbox abstrai o mecanismo de entrega, mantendo o domínio limpo.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre domain events in-memory e integration events via Outbox?"
- "Como o Outbox Pattern garante at-least-once delivery?"
- "Por que delegates do C# não servem para integração entre bounded contexts?"

### Leitura Recomendada

- [Outbox Pattern - microservices.io](https://microservices.io/patterns/data/transactional-outbox.html)
- [Domain Events vs Integration Events - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | Define EventBase, CommandBase, QueryBase |
| Bedrock.BuildingBlocks.Outbox | Implementa o Outbox Pattern para publicação transacional |

## Referências no Código

- [UserDeactivatedEvent.cs](../../../src/ShopDemo/Auth/Infra.CrossCutting.Messages/V1/Events/UserDeactivatedEvent.cs) - evento via record + Outbox
- [AuthEventFactory.cs](../../../src/ShopDemo/Auth/Application/Factories/AuthEventFactory.cs) - factory que cria eventos como records
