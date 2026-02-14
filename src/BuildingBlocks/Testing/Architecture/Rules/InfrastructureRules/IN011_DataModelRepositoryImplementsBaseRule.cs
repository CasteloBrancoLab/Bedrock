using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-011: Interfaces no namespace *.DataModelsRepositories.Interfaces de projetos Infra.Data.{Tech}
/// devem herdar de IDataModelRepository&lt;TDataModel&gt;.
/// Classes concretas no namespace *.DataModelsRepositories devem ser sealed
/// e herdar de DataModelRepositoryBase&lt;TDataModel&gt;.
/// </summary>
public sealed class IN011_DataModelRepositoryImplementsBaseRule : InfrastructureTypeRuleBase
{
    private const string DataModelRepositoryBaseTypeName = "DataModelRepositoryBase";
    private const string IDataModelRepositoryInterfaceName = "IDataModelRepository";
    private const string DataModelsRepositoriesSegment = ".DataModelsRepositories";
    private const string InterfacesSuffix = ".Interfaces";

    public override string Name => "IN011_DataModelRepositoryImplementsBase";

    public override string Description =>
        "DataModelRepositories devem implementar IDataModelRepository e herdar de DataModelRepositoryBase (IN-011).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-011-datamodel-repository-implementa-idatamodelrepository.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (!IsInfraDataTechProject(context.ProjectName))
            return null;

        var namespaceName = type.ContainingNamespace.ToDisplayString();

        if (!IsInDataModelsRepositoriesNamespace(namespaceName))
            return null;

        var isInInterfacesNamespace = namespaceName.EndsWith(
            DataModelsRepositoriesSegment + InterfacesSuffix, StringComparison.Ordinal);

        // Interfaces no namespace *.DataModelsRepositories.Interfaces
        if (isInInterfacesNamespace && type.TypeKind == TypeKind.Interface)
            return AnalyzeInterface(type, context);

        // Classes concretas no namespace *.DataModelsRepositories (nao .Interfaces)
        if (!isInInterfacesNamespace && type.TypeKind == TypeKind.Class && !type.IsAbstract && !type.IsStatic)
            return AnalyzeClass(type, context);

        return null;
    }

    private Violation? AnalyzeInterface(INamedTypeSymbol type, TypeContext context)
    {
        if (ImplementsInterfaceTransitively(type, IDataModelRepositoryInterfaceName))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Interface '{type.Name}' em '{context.ProjectName}': nao herda de IDataModelRepository.",
            LlmHint = $"A interface '{type.Name}' deve herdar de IDataModelRepository<TDataModel> " +
                      $"(direta ou indiretamente via IPostgreSqlDataModelRepository). " +
                      $"Consulte a ADR IN-011."
        };
    }

    private Violation? AnalyzeClass(INamedTypeSymbol type, TypeContext context)
    {
        var issues = new List<string>();

        if (!type.IsSealed)
            issues.Add("nao e sealed");

        if (!InheritsFromBaseClass(type, DataModelRepositoryBaseTypeName))
            issues.Add("nao herda de DataModelRepositoryBase");

        if (issues.Count == 0)
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"DataModelRepository '{type.Name}' em '{context.ProjectName}': {string.Join(", ", issues)}.",
            LlmHint = $"A classe '{type.Name}' deve ser sealed e herdar de " +
                      $"DataModelRepositoryBase<TDataModel>. Consulte a ADR IN-011."
        };
    }

    /// <summary>
    /// Verifica se o namespace pertence a *.DataModelsRepositories ou sub-namespace.
    /// Nao confundir com *.DataModels (IN-010).
    /// </summary>
    private static bool IsInDataModelsRepositoriesNamespace(string namespaceName)
    {
        var idx = namespaceName.IndexOf(DataModelsRepositoriesSegment, StringComparison.Ordinal);
        if (idx < 0)
            return false;

        var after = namespaceName[(idx + DataModelsRepositoriesSegment.Length)..];
        // Valido: vazio (exato) ou comeca com "." (sub-namespace como .Interfaces)
        return after.Length == 0 || after.StartsWith('.'); // Use char overload (no StringComparison needed)
    }
}
