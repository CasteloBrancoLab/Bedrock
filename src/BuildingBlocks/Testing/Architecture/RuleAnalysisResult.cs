namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Resultado consolidado da análise de uma regra sobre um projeto.
/// Contém metadados da regra e o resultado individual de cada tipo analisado.
/// </summary>
public sealed class RuleAnalysisResult
{
    /// <summary>
    /// Nome identificador da regra (ex: DE001_SealedClass).
    /// </summary>
    public required string RuleName { get; init; }

    /// <summary>
    /// Descrição da regra.
    /// </summary>
    public required string RuleDescription { get; init; }

    /// <summary>
    /// Severidade padrão das violações geradas por esta regra.
    /// </summary>
    public required Severity DefaultSeverity { get; init; }

    /// <summary>
    /// Caminho relativo para a ADR relacionada.
    /// </summary>
    public required string AdrPath { get; init; }

    /// <summary>
    /// Nome do projeto analisado.
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    /// Resultados individuais de cada tipo analisado.
    /// </summary>
    public required IReadOnlyList<TypeAnalysisResult> TypeResults { get; init; }

    /// <summary>
    /// Quantidade de tipos que passaram na regra.
    /// </summary>
    public int PassedCount => TypeResults.Count(r => r.Status == TypeAnalysisStatus.Passed);

    /// <summary>
    /// Quantidade de tipos que falharam na regra.
    /// </summary>
    public int FailedCount => TypeResults.Count(r => r.Status == TypeAnalysisStatus.Failed);
}
