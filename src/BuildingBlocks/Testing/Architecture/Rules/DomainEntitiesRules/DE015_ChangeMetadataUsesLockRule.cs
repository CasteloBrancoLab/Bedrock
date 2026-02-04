using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-015: Métodos <c>Change*Metadata()</c> devem usar <c>lock</c> para garantir
/// atomicidade das alterações de metadados durante o startup.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos <c>Change*Metadata</c> na classe <c>{EntityName}Metadata</c> devem conter
///         um <c>lock</c> statement no corpo</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Entidades sem classe de metadados aninhada</item>
///   <item>Métodos que não seguem o padrão <c>Change*Metadata</c></item>
/// </list>
/// </para>
/// </summary>
public sealed class DE015_ChangeMetadataUsesLockRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE015_ChangeMetadataUsesLock";

    public override string Description =>
        "Métodos Change*Metadata() devem usar lock para atomicidade (DE-015)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-015-customizacao-de-metadados-apenas-no-startup.md";

    /// <summary>
    /// Sufixo obrigatório da classe de metadados aninhada.
    /// </summary>
    private const string MetadataSuffix = "Metadata";

    /// <summary>
    /// Prefixo de métodos de customização de metadados.
    /// </summary>
    private const string ChangeMethodPrefix = "Change";

    /// <summary>
    /// Sufixo de métodos de customização de metadados.
    /// </summary>
    private const string ChangeMethodSuffix = "Metadata";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Buscar a classe de metadados aninhada: {EntityName}Metadata
        var expectedMetadataName = type.Name + MetadataSuffix;
        INamedTypeSymbol? metadataClass = null;

        foreach (var member in type.GetTypeMembers())
        {
            if (member.Name == expectedMetadataName && member.IsStatic)
            {
                metadataClass = member;
                break;
            }
        }

        // Se não há classe de metadados, não há o que verificar nesta regra
        if (metadataClass is null)
            return null;

        // Verificar cada método Change*Metadata
        foreach (var member in metadataClass.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Verificar se é um método Change*Metadata
            if (!IsChangeMetadataMethod(method))
                continue;

            // Verificar se o corpo do método contém lock statement
            if (!ContainsLockStatement(method))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMemberLineNumber(method, context.LineNumber),
                    Message = $"O método '{method.Name}' de '{metadataClass.Name}' não usa lock. " +
                              $"Métodos Change*Metadata() devem usar lock para garantir atomicidade " +
                              $"das alterações de metadados",
                    LlmHint = $"Adicionar lock(_lockObject) ao método '{method.Name}' de '{metadataClass.Name}'. " +
                              $"O lock garante que todas as propriedades são alteradas juntas - " +
                              $"não há estado intermediário inconsistente. " +
                              $"Consultar ADR DE-015 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se um método é um Change*Metadata (método de customização no startup).
    /// </summary>
    private static bool IsChangeMetadataMethod(IMethodSymbol method)
    {
        return method.Name.StartsWith(ChangeMethodPrefix, StringComparison.Ordinal) &&
               method.Name.EndsWith(ChangeMethodSuffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifica se o corpo do método contém um lock statement.
    /// </summary>
    private static bool ContainsLockStatement(IMethodSymbol method)
    {
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is LockStatementSyntax)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Obtém o número da linha onde o membro é declarado, ou fallback para a linha da classe.
    /// </summary>
    private static int GetMemberLineNumber(ISymbol member, int fallbackLineNumber)
    {
        var location = member.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
