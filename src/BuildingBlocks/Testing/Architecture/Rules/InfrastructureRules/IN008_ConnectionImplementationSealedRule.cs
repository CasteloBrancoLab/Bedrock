using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-008: Classes concretas de conexao em Infra.Data.{Tech} devem ser sealed,
/// herdar da base class tecnologica (que implementa IConnection) e implementar
/// a marker interface do BC.
/// </summary>
public sealed class IN008_ConnectionImplementationSealedRule : InfrastructureTypeRuleBase
{
    private const string ConnectionInterfaceName = "IConnection";
    private const string ConnectionsNamespaceSegment = ".Connections";
    private const string InterfacesNamespaceSegment = ".Connections.Interfaces";

    public override string Name => "IN008_ConnectionImplementationSealed";

    public override string Description =>
        "Implementacoes de conexao devem ser sealed e herdar da base class tecnologica (IN-008).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-008-conexao-implementacao-sealed-herda-base.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Apenas projetos Infra.Data.{Tech}
        if (!IsInfraDataTechProject(context.ProjectName))
            return null;

        // Apenas classes concretas (nao abstratas, nao interfaces, nao enums)
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
            return null;

        var namespaceName = type.ContainingNamespace.ToDisplayString();

        // Apenas classes no namespace *.Connections (mas nao *.Connections.Interfaces)
        if (!namespaceName.Contains(ConnectionsNamespaceSegment, StringComparison.Ordinal))
            return null;
        if (namespaceName.Contains(InterfacesNamespaceSegment, StringComparison.Ordinal))
            return null;

        // Verificar se a classe implementa IConnection (transitivamente)
        var implementsIConnection = ImplementsInterfaceTransitively(type, ConnectionInterfaceName);
        if (!implementsIConnection)
            return null; // Nao e uma classe de conexao — ignorar

        var issues = new List<string>();

        // Verificar sealed
        if (!type.IsSealed)
            issues.Add("nao e sealed");

        // Verificar se herda de alguma base class (nao diretamente de object)
        var hasBaseClass = type.BaseType is not null &&
                           type.BaseType.SpecialType != SpecialType.System_Object;
        if (!hasBaseClass)
            issues.Add("nao herda de uma base class tecnologica");

        // Verificar se implementa uma marker interface do BC
        // (interface em *.Connections.Interfaces que herda de IConnection)
        var implementsBcMarker = type.Interfaces.Any(i =>
            i.ContainingNamespace.ToDisplayString().EndsWith(InterfacesNamespaceSegment, StringComparison.Ordinal) &&
            ImplementsInterfaceTransitively(i, ConnectionInterfaceName));

        if (!implementsBcMarker)
            issues.Add("nao implementa a marker interface de conexao do BC");

        if (issues.Count == 0)
            return null; // Tudo OK

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Classe de conexao '{type.Name}' em '{context.ProjectName}': {string.Join(", ", issues)}.",
            LlmHint = $"A classe '{type.Name}' deve ser sealed, herdar da base class da tecnologia " +
                      $"(ex: PostgreSqlConnectionBase) e implementar a marker interface do BC " +
                      $"(ex: IAuthPostgreSqlConnection). Consulte a ADR IN-008."
        };
    }
}
