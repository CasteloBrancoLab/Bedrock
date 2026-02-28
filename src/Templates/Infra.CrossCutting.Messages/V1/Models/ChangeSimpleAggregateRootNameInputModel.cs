using System.Diagnostics.CodeAnalysis;

namespace Templates.Infra.CrossCutting.Messages.V1.Models;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Input Model — Representa o Comando Que Originou o Evento
───────────────────────────────────────────────────────────────────────────────

Input Models capturam os dados do request original que produziu o evento.
Junto com OldState e NewState, permitem replay completo sem command store.

✅ Primitivos apenas (Guid, string, DateTimeOffset)
❌ Tipos de domínio (Id, BirthDate)

───────────────────────────────────────────────────────────────────────────────
*/

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct ChangeSimpleAggregateRootNameInputModel(
    string FirstName,
    string LastName
);
