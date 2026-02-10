using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-059: A classe de metadados deve ser aninhada dentro da entidade.
/// Metadados pertencem à entidade - aninhamento expressa essa relação de coesão.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Se existe uma classe <c>{EntityName}Metadata</c> como tipo top-level no mesmo namespace,
///         ela deve ser aninhada dentro da entidade</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Entidades que não possuem classe de metadados (nem aninhada nem separada)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE059_NestedMetadataClassRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE059_NestedMetadataClass";

    public override string Description =>
        "A classe de metadados deve ser aninhada dentro da entidade, não em arquivo separado (DE-059)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-059-metadata-deve-ser-classe-aninhada.md";

    /// <summary>
    /// Sufixo obrigatório da classe de metadados.
    /// </summary>
    private const string MetadataSuffix = "Metadata";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;
        var expectedMetadataName = type.Name + MetadataSuffix;

        // Buscar tipo top-level no mesmo namespace com nome {EntityName}Metadata
        var nonNestedMetadata = type.ContainingNamespace.GetTypeMembers(expectedMetadataName);

        if (nonNestedMetadata.Length == 0)
            return null;

        // Encontrou classe de metadados como tipo top-level - violação
        var metadataType = nonNestedMetadata[0];
        var metadataLocation = metadataType.Locations.FirstOrDefault(l => l.IsInSource);
        int metadataLine = metadataLocation is not null
            ? metadataLocation.GetLineSpan().StartLinePosition.Line + 1
            : context.LineNumber;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = metadataLine,
            Message = $"A classe '{expectedMetadataName}' está definida como tipo separado no namespace " +
                      $"'{type.ContainingNamespace.ToDisplayString()}'. " +
                      $"Metadados devem ser uma classe estática aninhada dentro de '{type.Name}'. " +
                      $"Mover '{expectedMetadataName}' para dentro de '{type.Name}' como nested class",
            LlmHint = $"Mover a classe '{expectedMetadataName}' para dentro de '{type.Name}' como " +
                      $"'public static class {expectedMetadataName}' aninhada. " +
                      $"Deletar o arquivo separado '{expectedMetadataName}.cs'. " +
                      $"Nos testes, adicionar 'using {expectedMetadataName} = " +
                      $"{type.ContainingNamespace.ToDisplayString()}.{type.Name}.{expectedMetadataName};' " +
                      $"para manter compatibilidade. " +
                      $"Consultar ADR DE-059 para exemplos de uso correto"
        };
    }
}
