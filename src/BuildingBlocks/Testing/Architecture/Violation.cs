namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Representa uma violação de regra arquitetural detectada pela análise Roslyn.
/// </summary>
public sealed class Violation
{
    /// <summary>
    /// Nome da regra violada (ex: SealedClass).
    /// </summary>
    public required string Rule { get; init; }

    /// <summary>
    /// Severidade da violação.
    /// </summary>
    public required Severity Severity { get; init; }

    /// <summary>
    /// Caminho relativo para a ADR relacionada.
    /// </summary>
    public required string Adr { get; init; }

    /// <summary>
    /// Nome do projeto onde a violação foi encontrada.
    /// </summary>
    public required string Project { get; init; }

    /// <summary>
    /// Caminho do arquivo fonte.
    /// </summary>
    public required string File { get; init; }

    /// <summary>
    /// Número da linha onde a violação ocorre.
    /// </summary>
    public required int Line { get; init; }

    /// <summary>
    /// Mensagem descritiva da violação.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Dica para a LLM saber como corrigir a violação.
    /// </summary>
    public required string LlmHint { get; init; }
}

/// <summary>
/// Severidade de uma violação arquitetural.
/// </summary>
public enum Severity
{
    /// <summary>Violação bloqueante - a pipeline deve falhar.</summary>
    Error,

    /// <summary>Aviso - reportado mas não bloqueia a pipeline.</summary>
    Warning,

    /// <summary>Informação - sugestão de melhoria.</summary>
    Info
}
