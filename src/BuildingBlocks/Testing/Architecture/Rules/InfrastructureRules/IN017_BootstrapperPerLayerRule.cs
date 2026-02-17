using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-017: Projetos de infraestrutura de bounded context (Infra.Data.{Tech},
/// Infra.CrossCutting.Configuration) devem ter uma classe publica estatica
/// Bootstrapper no namespace raiz do projeto com metodo ConfigureServices.
/// </summary>
public sealed class IN017_BootstrapperPerLayerRule : InfrastructureTypeRuleBase
{
    private const string BootstrapperClassName = "Bootstrapper";
    private const string ConfigureServicesMethodName = "ConfigureServices";
    private const string ServiceCollectionInterfaceName = "IServiceCollection";
    private const string InfraCrossCuttingConfigSuffix = ".Infra.CrossCutting.Configuration";

    public override string Name => "IN017_BootstrapperPerLayer";

    public override string Description =>
        "Projetos de infraestrutura devem ter classe Bootstrapper estatica com ConfigureServices (IN-017).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-017-bootstrapper-por-camada-para-ioc.md";

    /// <summary>
    /// Nao usamos AnalyzeType individual — a regra usa o Analyze customizado
    /// que verifica tanto a existencia de Bootstrapper quanto tipos nao-Bootstrapper
    /// que recebem IServiceCollection.
    /// </summary>
    protected override Violation? AnalyzeType(TypeContext context) => null;

    public override IReadOnlyList<RuleAnalysisResult> Analyze(
        IReadOnlyDictionary<string, Compilation> compilations,
        string rootDir)
    {
        var results = new List<RuleAnalysisResult>();

        foreach (var (projectName, compilation) in compilations)
        {
            if (!RequiresBootstrapper(projectName))
                continue;

            var typeResults = new List<TypeAnalysisResult>();
            var assemblySymbol = compilation.Assembly;
            var allTypes = GetAllNamedTypes(compilation.GlobalNamespace);

            var ownTypes = allTypes
                .Where(t => SymbolEqualityComparer.Default.Equals(t.ContainingAssembly, assemblySymbol))
                .ToList();

            // Procurar classe Bootstrapper
            var bootstrapper = ownTypes
                .FirstOrDefault(t => t.TypeKind == TypeKind.Class
                    && t.Name == BootstrapperClassName);

            if (bootstrapper is null)
            {
                typeResults.Add(CreateFailedResult(
                    projectName, projectName, "", 1,
                    $"Projeto '{projectName}' nao possui classe '{BootstrapperClassName}'. " +
                    $"Cada projeto de infraestrutura deve ter um Bootstrapper.cs na raiz.",
                    $"Criar arquivo 'Bootstrapper.cs' na raiz do projeto '{projectName}' com " +
                    $"'public static class Bootstrapper' e metodo " +
                    $"'public static IServiceCollection ConfigureServices(IServiceCollection services)'. " +
                    $"Consulte a ADR IN-017."));
            }
            else
            {
                var location = bootstrapper.Locations.FirstOrDefault(l => l.IsInSource);
                var filePath = location is not null ? GetRelativePath(location.GetLineSpan().Path, rootDir) : "";
                var lineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : 1;

                var issues = new List<string>();

                if (!bootstrapper.IsStatic)
                    issues.Add("nao e static");

                if (bootstrapper.DeclaredAccessibility != Accessibility.Public)
                    issues.Add("nao e public");

                // Verificar que esta no namespace raiz do projeto
                var expectedRootNamespace = GetExpectedRootNamespace(projectName);
                var actualNamespace = bootstrapper.ContainingNamespace.ToDisplayString();
                if (!string.Equals(actualNamespace, expectedRootNamespace, StringComparison.Ordinal))
                    issues.Add($"esta no namespace '{actualNamespace}' em vez de '{expectedRootNamespace}'");

                // Verificar metodo ConfigureServices
                var hasConfigureServices = bootstrapper.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == ConfigureServicesMethodName
                        && m.Parameters.Length >= 1
                        && m.Parameters[0].Type.Name == ServiceCollectionInterfaceName);

                if (!hasConfigureServices)
                    issues.Add($"nao possui metodo '{ConfigureServicesMethodName}(IServiceCollection)'");

                if (issues.Count == 0)
                {
                    typeResults.Add(new TypeAnalysisResult
                    {
                        TypeName = bootstrapper.Name,
                        TypeFullName = bootstrapper.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        File = filePath,
                        Line = lineNumber,
                        Status = TypeAnalysisStatus.Passed,
                        Violation = null
                    });
                }
                else
                {
                    typeResults.Add(CreateFailedResult(
                        bootstrapper.Name,
                        bootstrapper.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        filePath, lineNumber,
                        $"Bootstrapper em '{projectName}' {string.Join(" e ", issues)}.",
                        $"A classe Bootstrapper deve ser 'public static class Bootstrapper' " +
                        $"no namespace raiz do projeto, com metodo " +
                        $"'public static IServiceCollection ConfigureServices(IServiceCollection services)'. " +
                        $"Consulte a ADR IN-017."));
                }
            }

            // Verificar tipos nao-Bootstrapper que recebem IServiceCollection
            foreach (var type in ownTypes)
            {
                if (type.Name == BootstrapperClassName)
                    continue;

                var hasServiceCollectionParam = type.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Parameters
                        .Any(p => p.Type.Name == ServiceCollectionInterfaceName));

                if (!hasServiceCollectionParam)
                    continue;

                var typeLoc = type.Locations.FirstOrDefault(l => l.IsInSource);
                var typeFile = typeLoc is not null
                    ? GetRelativePath(typeLoc.GetLineSpan().Path, rootDir) : "";
                var typeLine = typeLoc is not null
                    ? typeLoc.GetLineSpan().StartLinePosition.Line + 1 : 1;

                typeResults.Add(CreateFailedResult(
                    type.Name,
                    type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    typeFile, typeLine,
                    $"Classe '{type.Name}' em '{projectName}' recebe IServiceCollection como parametro. " +
                    $"Apenas a classe Bootstrapper pode registrar servicos no IoC.",
                    $"Mover o registro de servicos para a classe 'Bootstrapper.cs' na raiz do projeto. " +
                    $"Consulte a ADR IN-017."));
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

    /// <summary>
    /// Determina se um projeto deve ter Bootstrapper.
    /// Aplica-se a: Infra.Data.{Tech}, Infra.Data.{Tech}.Migrations e Infra.CrossCutting.Configuration.
    /// </summary>
    private static bool RequiresBootstrapper(string projectName)
    {
        return IsInfraDataTechProject(projectName)
            || IsMigrationProject(projectName)
            || projectName.EndsWith(InfraCrossCuttingConfigSuffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifica se um projeto e do tipo Infra.Data.{Tech}.Migrations.
    /// </summary>
    private static bool IsMigrationProject(string projectName)
    {
        const string marker = ".Infra.Data.";
        var idx = projectName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return false;

        var afterMarker = projectName[(idx + marker.Length)..];
        return afterMarker.EndsWith(".Migrations", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Deriva o namespace raiz esperado a partir do nome do projeto.
    /// Ex: "ShopDemo.Auth.Infra.Data.PostgreSql" → "ShopDemo.Auth.Infra.Data.PostgreSql"
    /// </summary>
    private static string GetExpectedRootNamespace(string projectName)
    {
        return projectName;
    }

    private TypeAnalysisResult CreateFailedResult(
        string typeName, string typeFullName, string file, int line,
        string message, string llmHint)
    {
        return new TypeAnalysisResult
        {
            TypeName = typeName,
            TypeFullName = typeFullName,
            File = file,
            Line = line,
            Status = TypeAnalysisStatus.Failed,
            Violation = new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = typeName,
                File = file,
                Line = line,
                Message = message,
                LlmHint = llmHint
            }
        };
    }
}
