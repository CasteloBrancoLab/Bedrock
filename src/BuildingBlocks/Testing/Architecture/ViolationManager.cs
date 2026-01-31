using System.Text;
using System.Text.Json;

namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Gerencia a coleta de resultados de análise e geração de artefatos para consumo pela pipeline.
/// Gera arquivos individuais em artifacts/pending/architecture_*.txt e um JSON consolidado.
/// </summary>
public sealed class ViolationManager
{
    private readonly List<Violation> _violations = [];
    private readonly List<RuleAnalysisResult> _ruleResults = [];

    /// <summary>
    /// Todas as violações coletadas.
    /// </summary>
    public IReadOnlyList<Violation> Violations => _violations;

    /// <summary>
    /// Todos os resultados de análise por regra/projeto.
    /// </summary>
    public IReadOnlyList<RuleAnalysisResult> RuleResults => _ruleResults;

    /// <summary>
    /// Indica se há violações com severidade Error.
    /// </summary>
    public bool HasErrors => _violations.Exists(v => v.Severity == Severity.Error);

    /// <summary>
    /// Adiciona resultados de análise de regras.
    /// Extrai automaticamente as violações para a lista de violations.
    /// </summary>
    public void AddRuleResults(IEnumerable<RuleAnalysisResult> results)
    {
        foreach (var result in results)
        {
            _ruleResults.Add(result);

            foreach (var typeResult in result.TypeResults)
            {
                if (typeResult.Violation is not null)
                    _violations.Add(typeResult.Violation);
            }
        }
    }

    /// <summary>
    /// Gera os artefatos de pending no diretório especificado.
    /// </summary>
    /// <param name="pendingDir">Diretório de pendências (ex: artifacts/pending).</param>
    public void WritePendingFiles(string pendingDir)
    {
        Directory.CreateDirectory(pendingDir);

        for (var i = 0; i < _violations.Count; i++)
        {
            var violation = _violations[i];
            var fileName = $"architecture_{violation.Rule.ToLowerInvariant()}_{(i + 1):D3}.txt";
            var filePath = Path.Combine(pendingDir, fileName);

            var sb = new StringBuilder();
            sb.AppendLine($"RULE: {violation.Rule}");
            sb.AppendLine($"SEVERITY: {violation.Severity}");
            sb.AppendLine($"ADR: {violation.Adr}");
            sb.AppendLine($"PROJECT: {violation.Project}");
            sb.AppendLine($"FILE: {violation.File}");
            sb.AppendLine($"LINE: {violation.Line}");
            sb.AppendLine($"MESSAGE: {violation.Message}");
            sb.AppendLine($"LLM_HINT: {violation.LlmHint}");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }

    /// <summary>
    /// Gera o relatório JSON consolidado para consumo pelo report generator.
    /// Inclui tanto o resumo de violações quanto os resultados detalhados por regra/tipo.
    /// </summary>
    /// <param name="outputPath">Caminho do arquivo JSON de saída.</param>
    public void WriteJsonReport(string outputPath)
    {
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var totalTypesAnalyzed = _ruleResults.Sum(r => r.TypeResults.Count);
        var totalPassed = _ruleResults.Sum(r => r.PassedCount);

        var report = new
        {
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            totalTypesAnalyzed,
            totalPassed,
            totalViolations = _violations.Count,
            errors = _violations.Count(v => v.Severity == Severity.Error),
            warnings = _violations.Count(v => v.Severity == Severity.Warning),
            infos = _violations.Count(v => v.Severity == Severity.Info),
            ruleResults = _ruleResults
                .OrderBy(r => r.ProjectName, StringComparer.Ordinal)
                .ThenBy(r => r.RuleName, StringComparer.Ordinal)
                .Select(r => new
            {
                ruleName = r.RuleName,
                ruleDescription = r.RuleDescription,
                defaultSeverity = r.DefaultSeverity.ToString(),
                adrPath = r.AdrPath,
                projectName = r.ProjectName,
                types = r.TypeResults.Select(t => new
                {
                    typeName = t.TypeName,
                    typeFullName = t.TypeFullName,
                    file = t.File,
                    line = t.Line,
                    status = t.Status.ToString(),
                    violation = t.Violation is null ? null : new
                    {
                        rule = t.Violation.Rule,
                        severity = t.Violation.Severity.ToString(),
                        message = t.Violation.Message,
                        llmHint = t.Violation.LlmHint
                    }
                })
            })
        };

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        File.WriteAllText(outputPath, json, Encoding.UTF8);
    }
}
