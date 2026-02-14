using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;

/// <summary>
/// RL-003: Literais SQL (strings contendo 2+ keywords SQL) sao proibidos
/// fora do namespace *.Mappers em projetos Infra.Data.{Tech}.
/// SQL deve ser gerado exclusivamente via DataModelMapperBase.
/// </summary>
public sealed class RL003_NoSqlLiteralsOutsideMappersRule : InfrastructureTypeRuleBase
{
    private const string MappersNamespaceSegment = ".Mappers";

    private static readonly string[] SqlKeywords =
    [
        "SELECT", "INSERT", "UPDATE", "DELETE",
        "FROM", "WHERE", "JOIN", "INTO",
        "SET", "VALUES", "ORDER BY", "GROUP BY",
        "HAVING", "UNION", "CREATE TABLE", "ALTER TABLE",
        "DROP TABLE", "TRUNCATE"
    ];

    private const int MinKeywordCount = 2;

    public override string Category => "Relational";

    public override string Name => "RL003_NoSqlLiteralsOutsideMappers";

    public override string Description =>
        "Literais SQL sao proibidos fora do namespace *.Mappers (RL-003).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/relational/RL-003-proibir-sql-fora-de-mapper.md";

    protected override Violation? AnalyzeType(TypeContext context) => null;

    public override IReadOnlyList<RuleAnalysisResult> Analyze(
        IReadOnlyDictionary<string, Compilation> compilations,
        string rootDir)
    {
        var results = new List<RuleAnalysisResult>();

        foreach (var (projectName, compilation) in compilations)
        {
            if (!IsInfraDataTechProject(projectName))
                continue;

            var typeResults = new List<TypeAnalysisResult>();
            var assemblySymbol = compilation.Assembly;
            var allTypes = GetAllNamedTypes(compilation.GlobalNamespace);

            var ownTypes = allTypes
                .Where(t => SymbolEqualityComparer.Default.Equals(t.ContainingAssembly, assemblySymbol))
                .ToList();

            // Tipos FORA do namespace *.Mappers
            var nonMapperTypes = ownTypes
                .Where(t => t.TypeKind == TypeKind.Class && !t.IsAbstract && !t.IsStatic)
                .Where(t => !IsInMappersNamespace(t.ContainingNamespace.ToDisplayString()))
                .ToList();

            foreach (var type in nonMapperTypes)
            {
                var location = type.Locations.FirstOrDefault(l => l.IsInSource);
                if (location is null)
                    continue;

                var filePath = GetRelativePath(location.GetLineSpan().Path, rootDir);
                var lineNumber = location.GetLineSpan().StartLinePosition.Line + 1;

                var sqlLiterals = FindSqlLiterals(type);

                if (sqlLiterals.Count == 0)
                {
                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = type.Name,
                        TypeFullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        File = filePath,
                        Line = lineNumber,
                        Status = TypeAnalysisStatus.Passed,
                        Violation = null
                    });
                }
                else
                {
                    var firstLiteral = sqlLiterals[0];
                    var truncated = firstLiteral.Length > 60
                        ? firstLiteral[..60] + "..."
                        : firstLiteral;

                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = type.Name,
                        TypeFullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        File = filePath,
                        Line = lineNumber,
                        Status = TypeAnalysisStatus.Failed,
                        Violation = new Violation
                        {
                            Rule = Name,
                            Severity = DefaultSeverity,
                            Adr = AdrPath,
                            Project = projectName,
                            File = filePath,
                            Line = lineNumber,
                            Message = $"Tipo '{type.Name}' em '{projectName}': contem literal SQL fora de *.Mappers: \"{truncated}\".",
                            LlmHint = $"Mova a geracao de SQL para o Mapper correspondente usando " +
                                      $"a API type-safe do DataModelMapperBase (Where, OrderBy, GenerateSelectCommand). " +
                                      $"Consulte a ADR RL-003."
                        }
                    });
                }
            }

            results.Add(new RuleAnalysisResult
            {
                RuleCategory = Category,
                RuleName = Name,
                RuleDescription = Description,
                DefaultSeverity = DefaultSeverity,
                AdrPath = AdrPath,
                ProjectName = projectName,
                TypeResults = typeResults
            });
        }

        return results;
    }

    private static List<string> FindSqlLiterals(INamedTypeSymbol type)
    {
        var sqlLiterals = new List<string>();

        foreach (var syntaxRef in type.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();

            var stringLiterals = syntax.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression))
                .Select(l => l.Token.ValueText);

            foreach (var literal in stringLiterals)
            {
                if (IsSqlLiteral(literal))
                    sqlLiterals.Add(literal);
            }

            // Tambem verificar interpolated strings
            var interpolatedStrings = syntax.DescendantNodes()
                .OfType<InterpolatedStringExpressionSyntax>();

            foreach (var interpolated in interpolatedStrings)
            {
                var fullText = interpolated.ToString();
                if (IsSqlLiteral(fullText))
                    sqlLiterals.Add(fullText);
            }
        }

        return sqlLiterals;
    }

    private static bool IsSqlLiteral(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var count = 0;
        foreach (var keyword in SqlKeywords)
        {
            if (value.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                count++;
                if (count >= MinKeywordCount)
                    return true;
            }
        }

        return false;
    }

    private static bool IsInMappersNamespace(string namespaceName)
    {
        if (namespaceName.EndsWith(MappersNamespaceSegment, StringComparison.Ordinal))
            return true;

        var idx = namespaceName.LastIndexOf(MappersNamespaceSegment, StringComparison.Ordinal);
        if (idx < 0)
            return false;

        return namespaceName[(idx + MappersNamespaceSegment.Length)..].Length == 0;
    }
}
