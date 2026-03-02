using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-012a: Readonly record structs em namespaces *.Messages.* nao devem
/// ter sufixo *ResultModel. Usar *OutputModel.
/// </summary>
public sealed class MS012a_OutputModelNamingRule : MessageGeneralRuleBase
{
    public override string Name => "MS012a_OutputModelNaming";

    public override string Description =>
        "Readonly record structs em Messages nao devem usar sufixo '*ResultModel'. Usar '*OutputModel' (MS-012)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-012-nomenclatura-input-output.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Filtro: apenas tipos em namespace contendo ".Messages."
        var ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (!ns.Contains(".Messages.", StringComparison.Ordinal))
            return null;

        // Filtro: apenas readonly record struct
        if (!type.IsReadOnly || !type.IsRecord || !type.IsValueType)
            return null;

        // Detecta: nome terminando em "ResultModel"
        if (type.Name.EndsWith("ResultModel", StringComparison.Ordinal))
        {
            var suggestedName = type.Name[..^"ResultModel".Length] + "OutputModel";

            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"O tipo '{type.Name}' em Messages usa sufixo '*ResultModel'. " +
                          $"Usar '*OutputModel' para consistencia com a convencao Input/Output",
                LlmHint = $"Renomear de '{type.Name}' para '{suggestedName}'"
            };
        }

        return null;
    }
}
