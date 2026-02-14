using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;

/// <summary>
/// RL-002: O metodo ConfigureInternal de cada Mapper (que herda DataModelMapperBase)
/// deve chamar MapTable no MapperOptions para definir schema e nome da tabela.
/// Sem MapTable, o mapper nao sabe para qual tabela gerar SQL.
/// </summary>
public sealed class RL002_MapperConfigureInternalCallsMapTableRule : InfrastructureTypeRuleBase
{
    private const string MappersNamespaceSegment = ".Mappers";
    private const string MapperBaseName = "DataModelMapperBase";
    private const string ConfigureInternalMethodName = "ConfigureInternal";
    private const string MapTableMethodName = "MapTable";

    public override string Category => "Relational";

    public override string Name => "RL002_MapperConfigureInternalCallsMapTable";

    public override string Description =>
        "ConfigureInternal do Mapper deve chamar MapTable para definir schema e tabela (RL-002).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/relational/RL-002-mapper-configurar-maptable.md";

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

                var callsMapTable = ConfigureInternalCallsMapTable(mapper, compilation);

                if (callsMapTable)
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
                            Message = $"Mapper '{mapper.Name}' em '{projectName}': ConfigureInternal nao chama MapTable.",
                            LlmHint = $"No mapper '{mapper.Name}', adicione uma chamada a " +
                                      $"mapperOptions.MapTable(schema, name) dentro de ConfigureInternal. " +
                                      $"Consulte a ADR RL-002."
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

    private static bool ConfigureInternalCallsMapTable(INamedTypeSymbol mapper, Compilation compilation)
    {
        var configureMethod = mapper.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Name == ConfigureInternalMethodName && m.IsOverride);

        if (configureMethod is null)
            return false;

        var syntaxRef = configureMethod.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef is null)
            return false;

        var methodSyntax = syntaxRef.GetSyntax() as MethodDeclarationSyntax;
        if (methodSyntax?.Body is null && methodSyntax?.ExpressionBody is null)
            return false;

        var invocations = (methodSyntax.Body is not null
            ? methodSyntax.Body.DescendantNodes()
            : methodSyntax.ExpressionBody!.DescendantNodes())
            .OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var methodName = ExtractMethodName(invocation);
            if (methodName == MapTableMethodName)
                return true;
        }

        return false;
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

        var after = namespaceName[(idx + segment.Length)..];
        return after.Length == 0;
    }
}
