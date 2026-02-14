using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-007: Projetos Infra.Data.{Tech} devem declarar pelo menos uma marker interface
/// de UnitOfWork no namespace *.UnitOfWork.Interfaces que herde (direta ou indiretamente)
/// de IUnitOfWork.
/// </summary>
public sealed class IN007_UnitOfWorkMarkerInterfaceRule : InfrastructureTypeRuleBase
{
    private const string UnitOfWorkInterfaceName = "IUnitOfWork";
    private const string RequiredNamespaceSuffix = ".UnitOfWork.Interfaces";

    public override string Name => "IN007_UnitOfWorkMarkerInterface";

    public override string Description =>
        "Projetos Infra.Data.{Tech} devem declarar marker interface de UnitOfWork que herde de IUnitOfWork (IN-007).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-007-unitofwork-marker-interface-herda-iunitofwork.md";

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

            var uowInterfaces = ownTypes
                .Where(t => t.TypeKind == TypeKind.Interface &&
                            t.ContainingNamespace.ToDisplayString().EndsWith(RequiredNamespaceSuffix, StringComparison.Ordinal))
                .ToList();

            if (uowInterfaces.Count == 0)
            {
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
                                  $"'*.UnitOfWork.Interfaces'. Cada Infra.Data.{{Tech}} deve ter uma marker " +
                                  $"interface de UnitOfWork.",
                        LlmHint = $"Criar uma interface no namespace '{projectName}.UnitOfWork.Interfaces' " +
                                  $"que herde (direta ou indiretamente) de IUnitOfWork. " +
                                  $"A interface deve ser um marker (corpo vazio). Consulte a ADR IN-007."
                    }
                });
            }
            else
            {
                foreach (var iface in uowInterfaces)
                {
                    var location = iface.Locations.FirstOrDefault(l => l.IsInSource);
                    var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                    var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                    var inheritsIUnitOfWork = ImplementsInterfaceTransitively(iface, UnitOfWorkInterfaceName);
                    var isMarker = IsMarkerInterface(iface);

                    if (inheritsIUnitOfWork && isMarker)
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
                        if (!inheritsIUnitOfWork)
                            issues.Add("nao herda de IUnitOfWork");
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
                                LlmHint = !inheritsIUnitOfWork
                                    ? $"A interface '{iface.Name}' deve herdar (direta ou indiretamente) de " +
                                      $"IUnitOfWork. Adicione a heranca via interface tecnologica " +
                                      $"(ex: IPostgreSqlUnitOfWork). Consulte a ADR IN-007."
                                    : $"A interface '{iface.Name}' deve ser um marker (corpo vazio, sem membros " +
                                      $"adicionais). Remova os membros declarados. Consulte a ADR IN-007."
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
