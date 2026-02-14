using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-010: Classes no namespace *.DataModels de projetos Infra.Data.{Tech}
/// devem herdar (direta ou indiretamente) de DataModelBase.
/// DataModels nao devem conter metodos com logica de negocio (apenas propriedades get/set).
/// </summary>
public sealed class IN010_DataModelInheritsDataModelBaseRule : InfrastructureTypeRuleBase
{
    private const string DataModelBaseTypeName = "DataModelBase";
    private const string DataModelsNamespaceSegment = ".DataModels";

    public override string Name => "IN010_DataModelInheritsDataModelBase";

    public override string Description =>
        "DataModels devem herdar de DataModelBase e conter apenas propriedades get/set (IN-010).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-010-datamodel-herda-datamodelbase.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (!IsInfraDataTechProject(context.ProjectName))
            return null;

        // Apenas classes concretas (nao abstratas, nao interfaces, nao enums, nao structs)
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
            return null;

        var namespaceName = type.ContainingNamespace.ToDisplayString();

        // Apenas classes no namespace *.DataModels (nao sub-namespaces como *.DataModelsRepositories)
        if (!IsInDataModelsNamespace(namespaceName))
            return null;

        // DataModelBase (a propria base class) nao precisa herdar de si mesma
        if (type.Name == DataModelBaseTypeName)
            return null;

        var issues = new List<string>();

        // Verificar heranca de DataModelBase
        if (!InheritsFromBaseClass(type, DataModelBaseTypeName))
            issues.Add("nao herda de DataModelBase");

        // Verificar se tem metodos de negocio (alem de propriedades, construtores e object overrides)
        var businessMethods = GetBusinessMethods(type);
        if (businessMethods.Count > 0)
        {
            var methodNames = string.Join(", ", businessMethods.Select(m => m.Name));
            issues.Add($"contem metodos de negocio ({methodNames})");
        }

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
            Message = $"DataModel '{type.Name}' em '{context.ProjectName}': {string.Join(", ", issues)}.",
            LlmHint = $"A classe '{type.Name}' deve herdar de DataModelBase e conter apenas " +
                      $"propriedades get/set (sem metodos de negocio). " +
                      $"Consulte a ADR IN-010."
        };
    }

    /// <summary>
    /// Verifica se o namespace e exatamente *.DataModels (nao *.DataModelsRepositories, etc).
    /// </summary>
    private static bool IsInDataModelsNamespace(string namespaceName)
    {
        // Deve terminar com ".DataModels" ou conter ".DataModels" sem ser seguido de mais segmentos
        if (namespaceName.EndsWith(DataModelsNamespaceSegment, StringComparison.Ordinal))
            return true;

        // Verificar se e exatamente o segmento (nao ".DataModelsRepositories")
        var idx = namespaceName.LastIndexOf(DataModelsNamespaceSegment, StringComparison.Ordinal);
        if (idx < 0)
            return false;

        var afterSegment = namespaceName[(idx + DataModelsNamespaceSegment.Length)..];
        // Se nao ha nada apos ".DataModels" ou ha um ponto (sub-namespace), ok
        // Mas se ha letras sem ponto (ex: ".DataModelsRepositories"), nao e o namespace correto
        return afterSegment.Length == 0;
    }

    /// <summary>
    /// Retorna metodos que representam logica de negocio (nao sao propriedades, construtores,
    /// property accessors ou object overrides).
    /// </summary>
    private static List<IMethodSymbol> GetBusinessMethods(INamedTypeSymbol type)
    {
        return type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => !m.IsImplicitlyDeclared)
            .Where(m => m.MethodKind == MethodKind.Ordinary) // Exclui construtores, property accessors, operators
            .Where(m => !IsObjectOverride(m))
            .ToList();
    }

    private static bool IsObjectOverride(IMethodSymbol method)
    {
        if (!method.IsOverride)
            return false;

        var overridden = method.OverriddenMethod;
        while (overridden is not null)
        {
            if (overridden.ContainingType.SpecialType == SpecialType.System_Object)
                return true;
            overridden = overridden.OverriddenMethod;
        }

        return false;
    }
}
