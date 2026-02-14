using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;

/// <summary>
/// RL-001: Para cada DataModel em *.DataModels de projetos Infra.Data.{Tech},
/// deve existir uma sealed class no namespace *.Mappers que herda de
/// DataModelMapperBase&lt;TDataModel&gt;.
/// A classe deve sobrescrever ConfigureInternal e MapBinaryImporter.
/// </summary>
public sealed class RL001_MapperInheritsDataModelMapperBaseRule : InfrastructureTypeRuleBase
{
    private const string DataModelsNamespaceSegment = ".DataModels";
    private const string MappersNamespaceSegment = ".Mappers";
    private const string DataModelSuffix = "DataModel";
    private const string DataModelBaseTypeName = "DataModelBase";
    private const string MapperBaseName = "DataModelMapperBase";
    private const string ConfigureInternalMethodName = "ConfigureInternal";
    private const string MapBinaryImporterMethodName = "MapBinaryImporter";

    public override string Category => "Relational";

    public override string Name => "RL001_MapperInheritsDataModelMapperBase";

    public override string Description =>
        "Para cada DataModel deve existir {Entity}DataModelMapper sealed herdando DataModelMapperBase com ConfigureInternal e MapBinaryImporter (RL-001).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/relational/RL-001-mapper-herda-datamodelmapperbase.md";

    /// <summary>
    /// Nao usamos AnalyzeType individual — a regra precisa correlacionar
    /// DataModels com Mappers, o que e uma asserção de nivel projeto.
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

            // Encontrar classes no namespace *.Mappers
            var mapperTypes = ownTypes
                .Where(t => t.TypeKind == TypeKind.Class)
                .Where(t => IsInExactNamespaceSegment(t.ContainingNamespace.ToDisplayString(), MappersNamespaceSegment))
                .ToDictionary(t => t.Name, t => t);

            foreach (var dataModel in dataModels)
            {
                var location = dataModel.Locations.FirstOrDefault(l => l.IsInSource);
                var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                var entityName = DeriveEntityName(dataModel.Name);
                var mapperName = $"{entityName}{DataModelSuffix}Mapper";

                var issues = new List<string>();
                ValidateMapper(mapperTypes, mapperName, issues);

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
                            LlmHint = $"Para o DataModel '{dataModel.Name}', crie uma sealed class " +
                                      $"'{mapperName}' no namespace *.Mappers herdando " +
                                      $"DataModelMapperBase<{dataModel.Name}>. " +
                                      $"Sobrescreva 'ConfigureInternal' e 'MapBinaryImporter'. " +
                                      $"Consulte a ADR RL-001."
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

    private static void ValidateMapper(
        Dictionary<string, INamedTypeSymbol> mapperTypes,
        string expectedName,
        List<string> issues)
    {
        if (!mapperTypes.TryGetValue(expectedName, out var mapper))
        {
            issues.Add($"mapper '{expectedName}' nao encontrado no namespace *.Mappers");
            return;
        }

        if (!mapper.IsSealed)
            issues.Add($"'{expectedName}' nao e sealed");

        if (!InheritsFromDataModelMapperBase(mapper))
            issues.Add($"'{expectedName}' nao herda de DataModelMapperBase<T>");

        var hasConfigureInternal = HasOverriddenMethod(mapper, ConfigureInternalMethodName);
        if (!hasConfigureInternal)
            issues.Add($"'{expectedName}' nao sobrescreve '{ConfigureInternalMethodName}'");

        var hasMapBinaryImporter = HasOverriddenMethod(mapper, MapBinaryImporterMethodName);
        if (!hasMapBinaryImporter)
            issues.Add($"'{expectedName}' nao sobrescreve '{MapBinaryImporterMethodName}'");
    }

    private static bool InheritsFromDataModelMapperBase(INamedTypeSymbol type)
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

    private static bool HasOverriddenMethod(INamedTypeSymbol type, string methodName)
    {
        return type.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m.Name == methodName && m.IsOverride);
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
