using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-015: Projetos Infra.Data.{Tech} com pelo menos um DataModel devem
/// conter namespaces correspondentes as pastas canonicas:
/// *.Connections.Interfaces, *.UnitOfWork.Interfaces, *.DataModels,
/// *.DataModelsRepositories.Interfaces, *.Factories,
/// *.Adapters, *.Repositories.Interfaces.
/// Nota: *.Mappers e validado pela regra RL-001 (categoria Relational).
/// </summary>
public sealed class IN015_CanonicalFolderStructureRule : InfrastructureTypeRuleBase
{
    private const string DataModelsNamespaceSegment = ".DataModels";
    private const string DataModelSuffix = "DataModel";
    private const string DataModelBaseTypeName = "DataModelBase";

    private static readonly string[] RequiredNamespaceSegments =
    [
        ".Connections.Interfaces",
        ".UnitOfWork.Interfaces",
        ".DataModels",
        ".DataModelsRepositories.Interfaces",
        ".Factories",
        ".Adapters",
        ".Repositories.Interfaces"
    ];

    public override string Name => "IN015_CanonicalFolderStructure";

    public override string Description =>
        "Projetos Infra.Data.{Tech} com DataModels devem conter todas as pastas canonicas (IN-015).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-015-estrutura-pastas-canonica-infra-data-tech.md";

    /// <summary>
    /// Nao usamos AnalyzeType individual — a regra verifica presenca de
    /// namespaces no nivel do projeto.
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

            // Verificar se existe pelo menos um DataModel
            var hasDataModel = ownTypes
                .Any(t => t.TypeKind == TypeKind.Class && !t.IsAbstract && !t.IsStatic
                    && t.Name != DataModelBaseTypeName
                    && IsInExactNamespaceSegment(t.ContainingNamespace.ToDisplayString(), DataModelsNamespaceSegment)
                    && t.Name.EndsWith(DataModelSuffix, StringComparison.Ordinal));

            if (!hasDataModel)
            {
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
                continue;
            }

            // Coletar todos os namespaces que contem tipos proprios
            var namespacesWithTypes = ownTypes
                .Select(t => t.ContainingNamespace.ToDisplayString())
                .Where(ns => !string.IsNullOrEmpty(ns))
                .ToHashSet(StringComparer.Ordinal);

            var missingSegments = new List<string>();

            foreach (var segment in RequiredNamespaceSegments)
            {
                var found = namespacesWithTypes.Any(ns => IsInExactNamespaceSegment(ns, segment));
                if (!found)
                    missingSegments.Add(segment);
            }

            if (missingSegments.Count == 0)
            {
                typeResults.Add(new TypeAnalysisResult
                {
                    TypeName = projectName,
                    TypeFullName = projectName,
                    File = "",
                    Line = 1,
                    Status = TypeAnalysisStatus.Passed,
                    Violation = null
                });
            }
            else
            {
                typeResults.Add(new TypeAnalysisResult
                {
                    TypeName = projectName,
                    TypeFullName = projectName,
                    File = "",
                    Line = 1,
                    Status = TypeAnalysisStatus.Failed,
                    Violation = new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = projectName,
                        File = "",
                        Line = 1,
                        Message = $"Projeto '{projectName}' possui DataModels mas faltam namespaces canonicos: {string.Join(", ", missingSegments)}.",
                        LlmHint = $"O projeto '{projectName}' tem DataModels, mas faltam as pastas " +
                                  $"canonicas: {string.Join(", ", missingSegments)}. " +
                                  $"Crie as pastas e seus artefatos conforme a ADR IN-015."
                    }
                });
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
