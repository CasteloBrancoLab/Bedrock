using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using Templates.Infra.CrossCutting.Messages.V1.Models;

namespace Templates.Infra.CrossCutting.Messages.V1.Events;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Evento de Criação — Input + NewState
───────────────────────────────────────────────────────────────────────────────

Eventos de criação carregam:
- Input: o que foi solicitado (replay do comando sem command store)
- NewState: snapshot do aggregate root após a criação

NÃO há OldState em eventos de criação — a entidade não existia antes.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Models Serializáveis — Sem Tipos de Domínio
───────────────────────────────────────────────────────────────────────────────

Input e State usam Message Models (primitivos apenas), NÃO domain models.
Isso isola o schema da mensagem do schema de domínio — consumidores em
outros BCs/linguagens não dependem dos value objects do produtor.

✅ RegisterSimpleAggregateRootInputModel (primitivos)
❌ RegisterNewInput (BirthDate — tipo de domínio)

───────────────────────────────────────────────────────────────────────────────
*/

/// <summary>
/// Event raised when a new SimpleAggregateRoot is registered.
/// Self-contained: carries the original input and the resulting state.
/// </summary>
public sealed record SimpleAggregateRootRegisteredEvent(
    MessageMetadata Metadata,
    RegisterSimpleAggregateRootInputModel Input,
    SimpleAggregateRootModel NewState
) : EventBase(Metadata);
