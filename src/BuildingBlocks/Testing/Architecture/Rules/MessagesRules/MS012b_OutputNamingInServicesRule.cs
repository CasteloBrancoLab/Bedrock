using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-012b: Tipos em *.Domain.Services.* nao devem ter sufixo *Result
/// nem namespace *.Results. Usar *Output e *.Outputs.
/// </summary>
public sealed class MS012b_OutputNamingInServicesRule : MessageGeneralRuleBase
{
    public override string Name => "MS012b_OutputNamingInServices";

    public override string Description =>
        "Tipos em Domain.Services nao devem usar sufixo '*Result' nem namespace '*.Results'. Usar '*Output' / '*.Outputs' (MS-012)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-012-nomenclatura-input-output.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Filtro: apenas tipos em namespace contendo ".Domain.Services."
        var ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (!ns.Contains(".Domain.Services.", StringComparison.Ordinal) &&
            !ns.EndsWith(".Domain.Services", StringComparison.Ordinal))
            return null;

        // Detecta: namespace contendo ".Results"
        if (ns.Contains(".Results", StringComparison.Ordinal))
        {
            var suggestedNs = ns.Replace(".Results", ".Outputs", StringComparison.Ordinal);

            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"O tipo '{type.Name}' esta no namespace '{ns}' que usa '.Results'. " +
                          $"Usar '.Outputs' para consistencia com a convencao Input/Output",
                LlmHint = $"Mover '{type.Name}' para namespace '{suggestedNs}'"
            };
        }

        // Detecta: nome terminando em "Result" (exceto se termina em "OutputResult" para evitar falso positivo)
        if (type.Name.EndsWith("Result", StringComparison.Ordinal) &&
            !type.Name.EndsWith("OutputResult", StringComparison.Ordinal))
        {
            var suggestedName = type.Name[..^"Result".Length] + "Output";

            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"O tipo '{type.Name}' em Domain.Services usa sufixo '*Result'. " +
                          $"Usar '*Output' para consistencia com a convencao Input/Output",
                LlmHint = $"Renomear de '{type.Name}' para '{suggestedName}' e mover para namespace *.Outputs"
            };
        }

        return null;
    }
}
