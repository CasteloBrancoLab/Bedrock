using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-012: Interfaces no namespace *.Repositories.Interfaces de projetos Infra.Data.{Tech}
/// devem herdar de IRepository&lt;TAggregateRoot&gt; (via IPostgreSqlRepository&lt;T&gt;).
/// Classes concretas no namespace *.Repositories devem ser sealed.
/// </summary>
public sealed class IN012_TechRepositoryImplementsIRepositoryRule : InfrastructureTypeRuleBase
{
    private const string IRepositoryInterfaceName = "IRepository";
    private const string RepositoriesSegment = ".Repositories";
    private const string DataModelsRepositoriesSegment = ".DataModelsRepositories";
    private const string InterfacesSuffix = ".Interfaces";

    public override string Name => "IN012_TechRepositoryImplementsIRepository";

    public override string Description =>
        "Repositorios tecnologicos devem implementar IRepository e ser sealed (IN-012).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-012-repositorio-tech-implementa-irepository.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (!IsInfraDataTechProject(context.ProjectName))
            return null;

        var namespaceName = type.ContainingNamespace.ToDisplayString();

        if (!IsInRepositoriesNamespace(namespaceName))
            return null;

        var isInInterfacesNamespace = namespaceName.EndsWith(
            RepositoriesSegment + InterfacesSuffix, StringComparison.Ordinal);

        // Interfaces no namespace *.Repositories.Interfaces
        if (isInInterfacesNamespace && type.TypeKind == TypeKind.Interface)
            return AnalyzeInterface(type, context);

        // Classes concretas no namespace *.Repositories (nao .Interfaces)
        if (!isInInterfacesNamespace && type.TypeKind == TypeKind.Class && !type.IsAbstract && !type.IsStatic)
            return AnalyzeClass(type, context);

        return null;
    }

    private Violation? AnalyzeInterface(INamedTypeSymbol type, TypeContext context)
    {
        if (ImplementsInterfaceTransitively(type, IRepositoryInterfaceName))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Interface '{type.Name}' em '{context.ProjectName}': nao herda de IRepository.",
            LlmHint = $"A interface '{type.Name}' deve herdar de IRepository<TAggregateRoot> " +
                      $"(direta ou indiretamente via IPostgreSqlRepository). " +
                      $"Consulte a ADR IN-012."
        };
    }

    private Violation? AnalyzeClass(INamedTypeSymbol type, TypeContext context)
    {
        if (type.IsSealed)
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Repositorio '{type.Name}' em '{context.ProjectName}': nao e sealed.",
            LlmHint = $"A classe '{type.Name}' deve ser sealed. " +
                      $"Consulte a ADR IN-012."
        };
    }

    /// <summary>
    /// Verifica se o namespace pertence a *.Repositories ou *.Repositories.Interfaces.
    /// Exclui *.DataModelsRepositories (que pertence a IN-011).
    /// </summary>
    private static bool IsInRepositoriesNamespace(string namespaceName)
    {
        // Se contem .DataModelsRepositories, e territorio da IN-011
        if (namespaceName.Contains(DataModelsRepositoriesSegment, StringComparison.Ordinal))
            return false;

        var idx = namespaceName.IndexOf(RepositoriesSegment, StringComparison.Ordinal);
        if (idx < 0)
            return false;

        var after = namespaceName[(idx + RepositoriesSegment.Length)..];
        return after.Length == 0 || after.StartsWith('.');
    }
}
