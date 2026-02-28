using Bedrock.BuildingBlocks.Messages.Commands.Interfaces;

namespace Bedrock.BuildingBlocks.Messages.Commands;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Abstract Record Com Metadata Encapsulado
───────────────────────────────────────────────────────────────────────────────

CommandBase recebe MessageMetadata e repassa para MessageBase.
Tipos concretos herdam e adicionam payload:

public sealed record RegisterUserCommand(
    MessageMetadata Metadata,
    string Email, string FullName
) : CommandBase(Metadata);

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: SchemaName Auto-Computado — Nunca Passado Manualmente
───────────────────────────────────────────────────────────────────────────────

SchemaName é computado por MessageBase via GetType().FullName e injetado
no Metadata automaticamente. Tipos concretos NÃO preenchem SchemaName.

───────────────────────────────────────────────────────────────────────────────
*/

/// <summary>
/// Abstract base record for commands, providing the standard message envelope.
/// </summary>
public abstract record CommandBase(MessageMetadata Metadata)
    : MessageBase(Metadata), ICommand;
