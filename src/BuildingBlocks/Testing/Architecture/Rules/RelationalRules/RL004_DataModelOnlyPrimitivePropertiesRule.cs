using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.RelationalRules;

/// <summary>
/// RL-004: Classes DataModel (em *.DataModels) devem ter apenas propriedades
/// de tipos primitivos. Value objects, entidades e outros tipos complexos
/// sao proibidos — o DataModel e um DTO plano para o banco relacional.
/// </summary>
public sealed class RL004_DataModelOnlyPrimitivePropertiesRule : InfrastructureTypeRuleBase
{
    private const string DataModelsNamespaceSegment = ".DataModels";
    private const string DataModelSuffix = "DataModel";
    private const string DataModelBaseTypeName = "DataModelBase";

    private static readonly HashSet<string> AllowedTypeNames = new(StringComparer.Ordinal)
    {
        // Inteiros
        "Byte", "SByte", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64",
        "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong",
        // Ponto flutuante
        "Single", "Double", "Decimal",
        "float", "double", "decimal",
        // Booleano
        "Boolean", "bool",
        // Caractere
        "Char", "char",
        // String
        "String", "string",
        // Identificadores
        "Guid",
        // Datas
        "DateTime", "DateTimeOffset", "DateOnly", "TimeOnly", "TimeSpan",
        // Binario
        "Byte[]", "byte[]"
    };

    public override string Category => "Relational";

    public override string Name => "RL004_DataModelOnlyPrimitiveProperties";

    public override string Description =>
        "DataModel deve ter apenas propriedades de tipos primitivos (RL-004).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/relational/RL-004-datamodel-propriedades-primitivas.md";

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

            var dataModels = ownTypes
                .Where(t => t.TypeKind == TypeKind.Class && !t.IsAbstract && !t.IsStatic)
                .Where(t => t.Name != DataModelBaseTypeName)
                .Where(t => IsInExactNamespaceSegment(t.ContainingNamespace.ToDisplayString(), DataModelsNamespaceSegment))
                .Where(t => t.Name.EndsWith(DataModelSuffix, StringComparison.Ordinal))
                .ToList();

            foreach (var dataModel in dataModels)
            {
                var location = dataModel.Locations.FirstOrDefault(l => l.IsInSource);
                var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                var nonPrimitiveProperties = FindNonPrimitiveProperties(dataModel);

                if (nonPrimitiveProperties.Count == 0)
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
                    var propList = string.Join(", ", nonPrimitiveProperties.Select(p => $"'{p.name}' ({p.typeName})"));

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
                            Message = $"DataModel '{dataModel.Name}' em '{projectName}': propriedades nao-primitivas: {propList}.",
                            LlmHint = $"No DataModel '{dataModel.Name}', substitua propriedades complexas por " +
                                      $"tipos primitivos (string, int, long, Guid, DateTimeOffset, bool, byte[], etc.). " +
                                      $"DataModels sao DTOs planos para o banco relacional. " +
                                      $"Consulte a ADR RL-004."
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

    private static List<(string name, string typeName)> FindNonPrimitiveProperties(INamedTypeSymbol dataModel)
    {
        var nonPrimitive = new List<(string name, string typeName)>();

        // Somente propriedades declaradas diretamente (nao herdadas)
        var declaredProperties = dataModel.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsImplicitlyDeclared && !p.IsStatic);

        foreach (var prop in declaredProperties)
        {
            if (!IsAllowedType(prop.Type))
            {
                nonPrimitive.Add((prop.Name, prop.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
            }
        }

        return nonPrimitive;
    }

    private static bool IsAllowedType(ITypeSymbol type)
    {
        // Nullable<T>: verificar o tipo interno
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            namedType.TypeArguments.Length == 1)
        {
            return IsAllowedType(namedType.TypeArguments[0]);
        }

        // Arrays: verificar tipo do elemento
        if (type is IArrayTypeSymbol arrayType)
        {
            var elementName = arrayType.ElementType.Name;
            return elementName is "Byte" or "byte";
        }

        // Enums sao permitidos (mapeados como int/short no banco)
        if (type.TypeKind == TypeKind.Enum)
            return true;

        return AllowedTypeNames.Contains(type.Name);
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
