using Bedrock.BuildingBlocks.Messages.Interfaces;

namespace Bedrock.BuildingBlocks.Messages.Queries.Interfaces;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Queries - Solicitação de Leitura
═══════════════════════════════════════════════════════════════════════════════

Queries representam solicitações de leitura de dados. Diferente de Commands,
Queries NÃO alteram estado — apenas consultam.

Tipos concretos ficam em V1/Queries/, V2/Queries/, etc.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Nomenclatura - Substantivo + Query
───────────────────────────────────────────────────────────────────────────────

Queries descrevem o que se quer obter:
✅ GetUserByIdQuery, ListActiveOrdersQuery, SearchProductsQuery
❌ FetchUser, UserQuery // ambíguo sem verbo descritivo

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Query Herda de QueryBase
───────────────────────────────────────────────────────────────────────────────

Todo query concreto herda de QueryBase (que herda de MessageBase).
O envelope (Metadata) é herdado. O tipo concreto adiciona apenas critérios:

✅ public sealed record GetUserByIdQuery(
       MessageMetadata Metadata,
       Guid UserId
   ) : QueryBase(Metadata);

❌ public record GetUserByIdQuery(Guid UserId) // sem envelope

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Marker interface for all queries in this bounded context.
/// </summary>
public interface IQuery : IMessage;
