using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-006: Projetos Infra.Data.{Tech} devem declarar pelo menos uma marker interface
/// de conexao no namespace *.Connections.Interfaces que herde (direta ou indiretamente)
/// de IConnection.
/// </summary>
public sealed class IN006_ConnectionMarkerInterfaceRule : InfrastructureTypeRuleBase
{
    private const string ConnectionInterfaceName = "IConnection";
    private const string RequiredNamespaceSuffix = ".Connections.Interfaces";

    public override string Name => "IN006_ConnectionMarkerInterface";

    public override string Description =>
        "Projetos Infra.Data.{Tech} devem declarar marker interface de conexao que herde de IConnection (IN-006).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-006-conexao-marker-interface-herda-iconnection.md";

    /// <summary>
    /// Nao usamos AnalyzeType individual — a regra precisa verificar a existencia de
    /// pelo menos uma interface por projeto, o que e uma asserção de nivel projeto.
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

            // Encontrar interfaces no namespace *.Connections.Interfaces
            var connectionInterfaces = ownTypes
                .Where(t => t.TypeKind == TypeKind.Interface &&
                            t.ContainingNamespace.ToDisplayString().EndsWith(RequiredNamespaceSuffix, StringComparison.Ordinal))
                .ToList();

            if (connectionInterfaces.Count == 0)
            {
                // Projeto Infra.Data.{Tech} sem marker interface de conexao
                typeResults.Add(new TypeAnalysisResult
                {
                    TypeName = projectName,
                    TypeFullName = projectName,
                    File = $"src/{projectName.Replace('.', '/')}",
                    Line = 1,
                    Status = TypeAnalysisStatus.Failed,
                    Violation = new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = projectName,
                        File = $"src/{projectName.Replace('.', '/')}",
                        Line = 1,
                        Message = $"Projeto '{projectName}' nao declara nenhuma interface no namespace " +
                                  $"'*.Connections.Interfaces'. Cada Infra.Data.{{Tech}} deve ter uma marker " +
                                  $"interface de conexao.",
                        LlmHint = $"Criar uma interface no namespace '{projectName}.Connections.Interfaces' " +
                                  $"que herde (direta ou indiretamente) de IConnection. " +
                                  $"A interface deve ser um marker (corpo vazio). Consulte a ADR IN-006."
                    }
                });
            }
            else
            {
                // Validar cada interface encontrada
                foreach (var iface in connectionInterfaces)
                {
                    var location = iface.Locations.FirstOrDefault(l => l.IsInSource);
                    var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                    var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                    var inheritsIConnection = ImplementsInterfaceTransitively(iface, ConnectionInterfaceName);
                    var isMarker = IsMarkerInterface(iface);

                    if (inheritsIConnection && isMarker)
                    {
                        typeResults.Add(new TypeAnalysisResult
                        {
                            TypeName = iface.Name,
                            TypeFullName = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            File = filePath,
                            Line = lineNumber,
                            Status = TypeAnalysisStatus.Passed,
                            Violation = null
                        });
                    }
                    else
                    {
                        var issues = new List<string>();
                        if (!inheritsIConnection)
                            issues.Add("nao herda de IConnection");
                        if (!isMarker)
                            issues.Add("nao e um marker (tem membros proprios)");

                        typeResults.Add(new TypeAnalysisResult
                        {
                            TypeName = iface.Name,
                            TypeFullName = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
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
                                Message = $"Interface '{iface.Name}' em '{projectName}' {string.Join(" e ", issues)}.",
                                LlmHint = !inheritsIConnection
                                    ? $"A interface '{iface.Name}' deve herdar (direta ou indiretamente) de " +
                                      $"IConnection. Adicione a heranca via interface tecnologica " +
                                      $"(ex: IPostgreSqlConnection). Consulte a ADR IN-006."
                                    : $"A interface '{iface.Name}' deve ser um marker (corpo vazio, sem membros " +
                                      $"adicionais). Remova os membros declarados. Consulte a ADR IN-006."
                            }
                        });
                    }
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
}
