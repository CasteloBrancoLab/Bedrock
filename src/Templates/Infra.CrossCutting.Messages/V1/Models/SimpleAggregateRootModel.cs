using System.Diagnostics.CodeAnalysis;

namespace Templates.Infra.CrossCutting.Messages.V1.Models;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Message Model - Snapshot do Aggregate Root
═══════════════════════════════════════════════════════════════════════════════

Message Models são representações serializáveis do estado de um aggregate
root para uso em mensagens (events, commands, queries). NÃO são entidades
de domínio — são DTOs de fronteira.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Primitivos Apenas — Sem Tipos de Domínio
───────────────────────────────────────────────────────────────────────────────

Message Models usam APENAS tipos primitivos serializáveis:
✅ Guid, string, DateTimeOffset, int, decimal
❌ Id, BirthDate, EntityInfo — acoplam consumidores ao domínio

RAZÃO: Mensagens cruzam fronteiras de processo. Consumidores em outras
linguagens/BCs não têm acesso aos value objects do domínio.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Readonly Record Struct — Zero Heap Allocation
───────────────────────────────────────────────────────────────────────────────

✅ public readonly record struct SimpleAggregateRootModel(...)
❌ public class SimpleAggregateRootModel { ... }

RAZÃO: Imutável, igualdade por valor, stack-allocated.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Snapshot Completo — Todas as Propriedades do Aggregate Root
───────────────────────────────────────────────────────────────────────────────

O model deve representar o estado COMPLETO do aggregate root para que
eventos sejam self-contained e suportem replay sem round-trips.

═══════════════════════════════════════════════════════════════════════════════
*/

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
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
