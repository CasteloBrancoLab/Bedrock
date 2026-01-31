namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Status da análise de um tipo por uma regra.
/// </summary>
public enum TypeAnalysisStatus
{
    /// <summary>O tipo está em conformidade com a regra.</summary>
    Passed,

    /// <summary>O tipo viola a regra.</summary>
    Failed
}

/// <summary>
/// Resultado da análise de um tipo individual por uma regra de arquitetura.
/// Captura tanto tipos que passaram quanto os que falharam.
/// </summary>
public sealed class TypeAnalysisResult
{
    /// <summary>
    /// Nome simples do tipo (ex: OrderEntity).
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Nome fully qualified do tipo (ex: global::Bedrock.Templates.Domain.Entities.OrderEntity).
    /// </summary>
    public required string TypeFullName { get; init; }

    /// <summary>
    /// Caminho relativo do arquivo fonte.
    /// </summary>
    public required string File { get; init; }

    /// <summary>
    /// Número da linha onde o tipo é declarado.
    /// </summary>
    public required int Line { get; init; }

    /// <summary>
    /// Status da análise (Passed ou Failed).
    /// </summary>
    public required TypeAnalysisStatus Status { get; init; }

    /// <summary>
    /// Violação encontrada. Não-nulo apenas quando <see cref="Status"/> é <see cref="TypeAnalysisStatus.Failed"/>.
    /// </summary>
    public Violation? Violation { get; init; }
}
