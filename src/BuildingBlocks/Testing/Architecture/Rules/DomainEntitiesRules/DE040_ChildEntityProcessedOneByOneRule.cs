using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-040: Entidades filhas devem ser processadas uma a uma atraves de
/// metodo especifico <c>Process[NomeDaEntidadeFilha]For[Operacao]Internal</c>.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Entidades que possuem colecoes de entidades filhas (<c>List&lt;T&gt;</c>
///         onde T herda de EntityBase) devem ter pelo menos um metodo
///         <c>Process*Internal</c> para processar essas entidades</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Entidades sem colecoes de entidades filhas</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE040_ChildEntityProcessedOneByOneRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE040_ChildEntityProcessedOneByOne";

    public override string Description =>
        "Entidades filhas devem ser processadas uma a uma via Process*Internal (DE-040)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-040-processamento-entidades-filhas-uma-a-uma.md";

    /// <summary>
    /// Prefixo esperado para metodos de processamento de entidades filhas.
    /// </summary>
    private const string ProcessPrefix = "Process";

    /// <summary>
    /// Sufixo esperado para metodos de processamento de entidades filhas.
    /// </summary>
    private const string InternalSuffix = "Internal";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Encontrar fields de colecao de entidades filhas
        var childCollectionFieldName = FindChildEntityCollectionField(type);
        if (childCollectionFieldName is null)
            return null; // Sem colecao de entidades filhas, regra nao se aplica

        // Verificar se existe pelo menos um metodo Process*Internal
        var hasProcessMethod = false;
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            if (method.Name.StartsWith(ProcessPrefix, StringComparison.Ordinal) &&
                method.Name.EndsWith(InternalSuffix, StringComparison.Ordinal))
            {
                hasProcessMethod = true;
                break;
            }
        }

        if (!hasProcessMethod)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe '{type.Name}' possui colecao de entidades filhas " +
                          $"(field '{childCollectionFieldName}') mas nao tem nenhum metodo " +
                          $"'Process*Internal' para processar entidades filhas " +
                          $"individualmente. Cada operacao deve ter um metodo " +
                          $"Process[NomeDaEntidadeFilha]For[Operacao]Internal",
                LlmHint = $"Adicionar metodos Process*For*Internal para processar " +
                          $"entidades filhas da colecao '{childCollectionFieldName}'. " +
                          $"Consultar ADR DE-040 para exemplos"
            };
        }

        return null;
    }

    /// <summary>
    /// Encontra o nome de um field de colecao cujo tipo generico herda de EntityBase.
    /// </summary>
    /// <returns>O nome do field ou null se nao houver.</returns>
    private static string? FindChildEntityCollectionField(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            if (field.IsStatic || field.IsImplicitlyDeclared)
                continue;

            if (field.Type is not INamedTypeSymbol namedType)
                continue;

            if (!namedType.IsGenericType || namedType.Name != "List")
                continue;

            // Verificar se o tipo generico herda de EntityBase
            if (namedType.TypeArguments.Length > 0)
            {
                var typeArg = namedType.TypeArguments[0];
                if (typeArg is INamedTypeSymbol childType && InheritsFromEntityBase(childType))
                    return field.Name;
            }
        }

        return null;
    }
}
