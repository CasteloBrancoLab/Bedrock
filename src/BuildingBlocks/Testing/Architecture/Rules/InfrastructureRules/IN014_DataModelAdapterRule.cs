using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-014: Para cada DataModel em *.DataModels de projetos Infra.Data.{Tech},
/// deve existir uma static class no namespace *.Adapters com nome {Entity}DataModelAdapter,
/// possuindo metodo publico Adapt.
/// </summary>
public sealed class IN014_DataModelAdapterRule : InfrastructureTypeRuleBase
{
    private const string DataModelsNamespaceSegment = ".DataModels";
    private const string AdaptersNamespaceSegment = ".Adapters";
    private const string DataModelSuffix = "DataModel";
    private const string DataModelBaseTypeName = "DataModelBase";
    private const string AdaptMethodName = "Adapt";

    public override string Name => "IN014_DataModelAdapter";

    public override string Description =>
        "Para cada DataModel deve existir {Entity}DataModelAdapter static com metodo Adapt (IN-014).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-014-adapter-atualizacao-datamodel-existente.md";

    /// <summary>
    /// Nao usamos AnalyzeType individual — a regra precisa correlacionar
    /// DataModels com Adapters, o que e uma asserção de nivel projeto.
    /// </summary>
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

            // Encontrar DataModels no namespace *.DataModels
            var dataModels = ownTypes
                .Where(t => t.TypeKind == TypeKind.Class && !t.IsAbstract && !t.IsStatic)
                .Where(t => t.Name != DataModelBaseTypeName)
                .Where(t => IsInExactNamespaceSegment(t.ContainingNamespace.ToDisplayString(), DataModelsNamespaceSegment))
                .Where(t => t.Name.EndsWith(DataModelSuffix, StringComparison.Ordinal))
                .ToList();

            // Encontrar classes no namespace *.Adapters
            var adapterTypes = ownTypes
                .Where(t => t.TypeKind == TypeKind.Class)
                .Where(t => IsInExactNamespaceSegment(t.ContainingNamespace.ToDisplayString(), AdaptersNamespaceSegment))
                .ToDictionary(t => t.Name, t => t);

            foreach (var dataModel in dataModels)
            {
                var location = dataModel.Locations.FirstOrDefault(l => l.IsInSource);
                var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                var entityName = DeriveEntityName(dataModel.Name);
                var adapterName = $"{entityName}{DataModelSuffix}Adapter";

                var issues = new List<string>();
                ValidateAdapter(adapterTypes, adapterName, issues);

                if (issues.Count == 0)
                {
                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = dataModel.Name,
                        TypeFullName = dataModel.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
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
                        TypeName = dataModel.Name,
                        TypeFullName = dataModel.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
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
                            Message = $"DataModel '{dataModel.Name}' em '{projectName}': {string.Join("; ", issues)}.",
                            LlmHint = $"Para o DataModel '{dataModel.Name}', crie uma static class " +
                                      $"'{adapterName}' no namespace *.Adapters com metodo publico " +
                                      $"'Adapt' que recebe o DataModel existente e a entidade de dominio. " +
                                      $"Consulte a ADR IN-014."
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

    private static void ValidateAdapter(
        Dictionary<string, INamedTypeSymbol> adapterTypes,
        string expectedName,
        List<string> issues)
    {
        if (!adapterTypes.TryGetValue(expectedName, out var adapter))
        {
            issues.Add($"adapter '{expectedName}' nao encontrado no namespace *.Adapters");
            return;
        }

        if (!adapter.IsStatic)
            issues.Add($"'{expectedName}' nao e static class");

        var hasAdaptMethod = adapter.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m.Name == AdaptMethodName &&
                      m.DeclaredAccessibility == Accessibility.Public &&
                      m.IsStatic);

        if (!hasAdaptMethod)
            issues.Add($"'{expectedName}' nao tem metodo publico static 'Adapt'");
    }

    private static string DeriveEntityName(string dataModelName)
    {
        if (dataModelName.EndsWith(DataModelSuffix, StringComparison.Ordinal))
            return dataModelName[..^DataModelSuffix.Length];

        return dataModelName;
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
