using Bedrock.BuildingBlocks.Messages.Queries.Interfaces;

namespace Bedrock.BuildingBlocks.Messages.Queries;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Abstract Record Com Metadata Encapsulado
───────────────────────────────────────────────────────────────────────────────

QueryBase recebe MessageMetadata e repassa para MessageBase.
Tipos concretos herdam e adicionam critérios de busca:

public sealed record GetUserByIdQuery(
    MessageMetadata Metadata,
    Guid UserId
) : QueryBase(Metadata);

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: SchemaName Auto-Computado — Nunca Passado Manualmente
───────────────────────────────────────────────────────────────────────────────

SchemaName é computado por MessageBase via GetType().FullName e injetado
no Metadata automaticamente. Tipos concretos NÃO preenchem SchemaName.

───────────────────────────────────────────────────────────────────────────────
*/

/// <summary>
/// Abstract base record for queries, providing the standard message envelope.
/// </summary>
public abstract record QueryBase(MessageMetadata Metadata)
    : MessageBase(Metadata), IQuery;
