using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-009: Classes concretas de UnitOfWork em Infra.Data.{Tech} devem ser sealed,
/// herdar da base class tecnologica (que implementa IUnitOfWork) e implementar
/// a marker interface do BC.
/// </summary>
public sealed class IN009_UnitOfWorkImplementationSealedRule : InfrastructureTypeRuleBase
{
    private const string UnitOfWorkInterfaceName = "IUnitOfWork";
    private const string UnitOfWorkNamespaceSegment = ".UnitOfWork";
    private const string InterfacesNamespaceSegment = ".UnitOfWork.Interfaces";

    public override string Name => "IN009_UnitOfWorkImplementationSealed";

    public override string Description =>
        "Implementacoes de UnitOfWork devem ser sealed e herdar da base class tecnologica (IN-009).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-009-unitofwork-implementacao-sealed-herda-base.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (!IsInfraDataTechProject(context.ProjectName))
            return null;

        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
            return null;

        var namespaceName = type.ContainingNamespace.ToDisplayString();

        // Apenas classes no namespace *.UnitOfWork (mas nao *.UnitOfWork.Interfaces)
        if (!namespaceName.Contains(UnitOfWorkNamespaceSegment, StringComparison.Ordinal))
            return null;
        if (namespaceName.Contains(InterfacesNamespaceSegment, StringComparison.Ordinal))
            return null;

        // Verificar se a classe implementa IUnitOfWork (transitivamente)
        var implementsIUnitOfWork = ImplementsInterfaceTransitively(type, UnitOfWorkInterfaceName);
        if (!implementsIUnitOfWork)
            return null; // Nao e uma classe de UnitOfWork — ignorar

        var issues = new List<string>();

        if (!type.IsSealed)
            issues.Add("nao e sealed");

        var hasBaseClass = type.BaseType is not null &&
                           type.BaseType.SpecialType != SpecialType.System_Object;
        if (!hasBaseClass)
            issues.Add("nao herda de uma base class tecnologica");

        var implementsBcMarker = type.Interfaces.Any(i =>
            i.ContainingNamespace.ToDisplayString().EndsWith(InterfacesNamespaceSegment, StringComparison.Ordinal) &&
            ImplementsInterfaceTransitively(i, UnitOfWorkInterfaceName));

        if (!implementsBcMarker)
            issues.Add("nao implementa a marker interface de UnitOfWork do BC");

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
            Message = $"Classe de UnitOfWork '{type.Name}' em '{context.ProjectName}': {string.Join(", ", issues)}.",
            LlmHint = $"A classe '{type.Name}' deve ser sealed, herdar da base class da tecnologia " +
                      $"(ex: PostgreSqlUnitOfWorkBase) e implementar a marker interface do BC " +
                      $"(ex: IAuthPostgreSqlUnitOfWork). Consulte a ADR IN-009."
        };
    }
}
