using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Messages.Events;
using Templates.Infra.CrossCutting.Messages.V1.Models;

namespace Templates.Infra.CrossCutting.Messages.V1.Events;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Evento de Mudança — Input + OldState + NewState
───────────────────────────────────────────────────────────────────────────────

Eventos de mudança carregam:
- Input: o que foi solicitado (replay do comando sem command store)
- OldState: snapshot do aggregate root ANTES da mudança
- NewState: snapshot do aggregate root APÓS a mudança

Isso permite:
- Replay completo do evento sem command store
- Diff entre estados sem consultar repositório
- Audit trail completo (quem pediu o quê → o que mudou)

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Models Serializáveis — Sem Tipos de Domínio
───────────────────────────────────────────────────────────────────────────────

Input e State usam Message Models (primitivos apenas), NÃO domain models.
Isso isola o schema da mensagem do schema de domínio — consumidores em
outros BCs/linguagens não dependem dos value objects do produtor.

───────────────────────────────────────────────────────────────────────────────
*/

/// <summary>
/// Event raised when a SimpleAggregateRoot's name is changed.
/// Self-contained: carries the original input, previous state, and resulting state.
/// </summary>
public sealed record SimpleAggregateRootNameChangedEvent(
    MessageMetadata Metadata,
    ChangeSimpleAggregateRootNameInputModel Input,
    SimpleAggregateRootModel OldState,
    SimpleAggregateRootModel NewState
) : EventBase(Metadata);
