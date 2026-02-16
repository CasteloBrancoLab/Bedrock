using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.PostgreSqlRules;

/// <summary>
/// PG-001: O metodo MapBinaryImporter de cada Mapper deve chamar importer.Write()
/// exatamente para todas as colunas mapeadas: 13 colunas base (DataModelBase)
/// + N colunas especificas (MapColumn calls em ConfigureInternal).
/// A quantidade de Write() deve ser igual a 13 + MapColumn count.
/// </summary>
public sealed class PG001_MapBinaryImporterWritesAllColumnsRule : InfrastructureTypeRuleBase
{
    private const string MappersNamespaceSegment = ".Mappers";
    private const string MapperBaseName = "DataModelMapperBase";
    private const string MapBinaryImporterMethodName = "MapBinaryImporter";
    private const string ConfigureInternalMethodName = "ConfigureInternal";
    private const string WriteMethodName = "Write";
    private const string MapColumnMethodName = "MapColumn";
    private const string AutoMapColumnsMethodName = "AutoMapColumns";
    private const int BaseColumnCount = 13;

    public override string Category => "PostgreSQL";

    public override string Name => "PG001_MapBinaryImporterWritesAllColumns";

    public override string Description =>
        "MapBinaryImporter deve escrever todas as colunas mapeadas: 13 base + N especificas (PG-001).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/postgresql/PG-001-binary-importer-todas-colunas.md";

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

            var mappers = ownTypes
                .Where(t => t.TypeKind == TypeKind.Class && !t.IsAbstract && !t.IsStatic)
                .Where(t => IsInExactNamespaceSegment(t.ContainingNamespace.ToDisplayString(), MappersNamespaceSegment))
                .Where(t => InheritsFromMapperBase(t))
                .ToList();

            foreach (var mapper in mappers)
            {
                var location = mapper.Locations.FirstOrDefault(l => l.IsInSource);
                var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                var writeCount = CountWriteCalls(mapper, MapBinaryImporterMethodName);
                var mapColumnCount = CountMethodCalls(mapper, ConfigureInternalMethodName, MapColumnMethodName);
                var usesAutoMap = HasMethodCall(mapper, ConfigureInternalMethodName, AutoMapColumnsMethodName);

                // Se usa AutoMapColumns, nao podemos contar as colunas estaticamente
                if (usesAutoMap)
                {
                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = mapper.Name,
                        TypeFullName = mapper.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        File = filePath,
                        Line = lineNumber,
                        Status = TypeAnalysisStatus.Passed,
                        Violation = null
                    });
                    continue;
                }

                var expectedCount = BaseColumnCount + mapColumnCount;

                if (writeCount == expectedCount)
                {
                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = mapper.Name,
                        TypeFullName = mapper.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        File = filePath,
                        Line = lineNumber,
                        Status = TypeAnalysisStatus.Passed,
                        Violation = null
                    });
                }
                else
                {
                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = mapper.Name,
                        TypeFullName = mapper.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
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
                            Message = $"Mapper '{mapper.Name}' em '{projectName}': " +
                                      $"MapBinaryImporter tem {writeCount} Write() mas esperado {expectedCount} " +
                                      $"({BaseColumnCount} base + {mapColumnCount} MapColumn).",
                            LlmHint = $"No mapper '{mapper.Name}', ajuste MapBinaryImporter para ter " +
                                      $"exatamente {expectedCount} chamadas a importer.Write() " +
                                      $"({BaseColumnCount} colunas base + {mapColumnCount} colunas especificas). " +
                                      $"Consulte a ADR PG-001."
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

    private static bool InheritsFromMapperBase(INamedTypeSymbol type)
    {
        var baseType = type.BaseType;
        while (baseType is not null)
        {
            if (baseType.Name == MapperBaseName)
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static int CountWriteCalls(INamedTypeSymbol type, string containingMethodName)
    {
        var method = type.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Name == containingMethodName && m.IsOverride);

        if (method is null)
            return 0;

        var syntaxRef = method.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef is null)
            return 0;

        var methodSyntax = syntaxRef.GetSyntax() as MethodDeclarationSyntax;
        if (methodSyntax is null)
            return 0;

        var bodyNodes = methodSyntax.Body is not null
            ? methodSyntax.Body.DescendantNodes()
            : methodSyntax.ExpressionBody?.DescendantNodes() ?? [];

        return bodyNodes
            .OfType<InvocationExpressionSyntax>()
            .Count(inv => ExtractMethodName(inv) == WriteMethodName);
    }

    private static int CountMethodCalls(INamedTypeSymbol type, string containingMethodName, string targetMethodName)
    {
        var method = type.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Name == containingMethodName && m.IsOverride);

        if (method is null)
            return 0;

        var syntaxRef = method.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef is null)
            return 0;

        var methodSyntax = syntaxRef.GetSyntax() as MethodDeclarationSyntax;
        if (methodSyntax is null)
            return 0;

        var bodyNodes = methodSyntax.Body is not null
            ? methodSyntax.Body.DescendantNodes()
            : methodSyntax.ExpressionBody?.DescendantNodes() ?? [];

        return bodyNodes
            .OfType<InvocationExpressionSyntax>()
            .Count(inv => ExtractMethodName(inv) == targetMethodName);
    }

    private static bool HasMethodCall(INamedTypeSymbol type, string containingMethodName, string targetMethodName)
    {
        return CountMethodCalls(type, containingMethodName, targetMethodName) > 0;
    }

    private static string? ExtractMethodName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => null
        };
    }

    private static bool IsInExactNamespaceSegment(string namespaceName, string segment)
    {
        if (namespaceName.EndsWith(segment, StringComparison.Ordinal))
            return true;

        var idx = namespaceName.LastIndexOf(segment, StringComparison.Ordinal);
        if (idx < 0)
            return false;

        return namespaceName[(idx + segment.Length)..].Length == 0;
    }
}
