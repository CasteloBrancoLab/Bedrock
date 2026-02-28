using Bedrock.BuildingBlocks.Messages.Interfaces;

namespace Bedrock.BuildingBlocks.Messages.Commands.Interfaces;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Commands - Intenção de Mudança de Estado
═══════════════════════════════════════════════════════════════════════════════

Commands representam a INTENÇÃO de executar uma ação no domínio.
Diferente de Events (fatos passados), Commands podem ser rejeitados.

Tipos concretos ficam em V1/Commands/, V2/Commands/, etc.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Nomenclatura - Verbo Imperativo
───────────────────────────────────────────────────────────────────────────────

Commands expressam intenção. Use imperativo:
✅ RegisterUserCommand, CancelOrderCommand, ChangeNameCommand
❌ UserRegisteredCommand, OrderCancelledCommand // passado = evento, não comando

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Command Herda de CommandBase
───────────────────────────────────────────────────────────────────────────────

Todo command concreto herda de CommandBase (que herda de MessageBase).
O envelope (Metadata) é herdado. O tipo concreto adiciona apenas payload:

✅ public sealed record RegisterUserCommand(
       MessageMetadata Metadata,
       string Email, string FullName
   ) : CommandBase(Metadata);

❌ public record RegisterUserCommand(string Email) // sem envelope

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Marker interface for all commands in this bounded context.
/// </summary>
public interface ICommand : IMessage;
