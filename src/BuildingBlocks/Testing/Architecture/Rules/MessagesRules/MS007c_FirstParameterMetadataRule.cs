using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-007c: O primeiro parametro posicional de uma mensagem concreta
/// deve ser <c>MessageMetadata Metadata</c>.
/// </summary>
public sealed class MS007c_FirstParameterMetadataRule : MessageRuleBase
{
    public override string Name => "MS007c_FirstParameterMetadata";

    public override string Description =>
        "O primeiro parametro posicional deve ser 'MessageMetadata Metadata' (MS-007)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-007-concretos-herdam-base-tipada.md";

    protected override Violation? AnalyzeMessageType(TypeContext context)
    {
        var type = context.Type;

        // Records posicionais geram um construtor primario cujos parametros
        // correspondem aos parametros posicionais do record.
        var primaryCtor = type.InstanceConstructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        // Se nao tem construtor com parametros, pode ser um copy constructor;
        // verificar tambem o construtor implicito gerado pelo record
        primaryCtor ??= type.InstanceConstructors
            .Where(c => c.Parameters.Length > 0)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (primaryCtor is null || primaryCtor.Parameters.Length == 0)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A mensagem '{type.Name}' nao tem parametros posicionais. O primeiro deve ser 'MessageMetadata Metadata'",
                LlmHint = $"Adicionar 'MessageMetadata Metadata' como primeiro parametro posicional de '{type.Name}'"
            };
        }

        var firstParam = primaryCtor.Parameters[0];
        var paramTypeName = firstParam.Type.Name;
        var paramName = firstParam.Name;

        if (paramTypeName != "MessageMetadata" || paramName != "Metadata")
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"O primeiro parametro de '{type.Name}' e '{paramTypeName} {paramName}', " +
                          $"mas deve ser 'MessageMetadata Metadata'",
                LlmHint = $"Alterar o primeiro parametro posicional de '{type.Name}' para 'MessageMetadata Metadata'"
            };
        }

        return null;
    }
}
