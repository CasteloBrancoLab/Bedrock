using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-013: Para cada DataModel em *.DataModels de projetos Infra.Data.{Tech},
/// devem existir duas static classes no namespace *.Factories:
/// - {Entity}Factory (DataModel → Entity) com metodo Create
/// - {Entity}DataModelFactory (Entity → DataModel) com metodo Create
/// </summary>
public sealed class IN013_BidirectionalFactoriesRule : InfrastructureTypeRuleBase
{
    private const string DataModelsNamespaceSegment = ".DataModels";
    private const string FactoriesNamespaceSegment = ".Factories";
    private const string DataModelSuffix = "DataModel";
    private const string DataModelBaseTypeName = "DataModelBase";
    private const string CreateMethodName = "Create";

    public override string Name => "IN013_BidirectionalFactories";

    public override string Description =>
        "Para cada DataModel devem existir {Entity}Factory e {Entity}DataModelFactory static com metodo Create (IN-013).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-013-factories-bidirecionais-datamodel-entidade.md";

    /// <summary>
    /// Nao usamos AnalyzeType individual — a regra precisa correlacionar
    /// DataModels com Factories, o que e uma assertcao de nivel projeto.
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

            // Encontrar classes no namespace *.Factories
            var factoryTypes = ownTypes
                .Where(t => t.TypeKind == TypeKind.Class)
                .Where(t => IsInExactNamespaceSegment(t.ContainingNamespace.ToDisplayString(), FactoriesNamespaceSegment))
                .ToDictionary(t => t.Name, t => t);

            foreach (var dataModel in dataModels)
            {
                var location = dataModel.Locations.FirstOrDefault(l => l.IsInSource);
                var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                var entityName = DeriveEntityName(dataModel.Name);
                var entityFactoryName = $"{entityName}Factory";
                var dataModelFactoryName = $"{entityName}{DataModelSuffix}Factory";

                var issues = new List<string>();

                ValidateFactory(factoryTypes, entityFactoryName, issues);
                ValidateFactory(factoryTypes, dataModelFactoryName, issues);

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
                            LlmHint = $"Para o DataModel '{dataModel.Name}', crie duas static classes " +
                                      $"no namespace *.Factories: '{entityFactoryName}' (DataModel → Entity) " +
                                      $"e '{dataModelFactoryName}' (Entity → DataModel). Ambas devem ter " +
                                      $"metodo publico 'Create'. Consulte a ADR IN-013."
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

    /// <summary>
    /// Valida se uma factory existe, e static e tem metodo Create.
    /// </summary>
    private static void ValidateFactory(
        Dictionary<string, INamedTypeSymbol> factoryTypes,
        string expectedName,
        List<string> issues)
    {
        if (!factoryTypes.TryGetValue(expectedName, out var factory))
        {
            issues.Add($"factory '{expectedName}' nao encontrada no namespace *.Factories");
            return;
        }

        if (!factory.IsStatic)
            issues.Add($"'{expectedName}' nao e static class");

        var hasCreateMethod = factory.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m.Name == CreateMethodName &&
                      m.DeclaredAccessibility == Accessibility.Public &&
                      m.IsStatic);

        if (!hasCreateMethod)
            issues.Add($"'{expectedName}' nao tem metodo publico static 'Create'");
    }

    /// <summary>
    /// Deriva o nome da entidade a partir do nome do DataModel.
    /// Ex: "UserDataModel" → "User".
    /// </summary>
    private static string DeriveEntityName(string dataModelName)
    {
        if (dataModelName.EndsWith(DataModelSuffix, StringComparison.Ordinal))
            return dataModelName[..^DataModelSuffix.Length];

        return dataModelName;
    }

    /// <summary>
    /// Verifica se o namespace contem um segmento exato (ex: ".DataModels" nao casa com ".DataModelsRepositories").
    /// </summary>
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
