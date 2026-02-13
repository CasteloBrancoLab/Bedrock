namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-001: Valida que as dependencias entre camadas de um bounded context
/// seguem o grafo canonico definido na ADR IN-001.
///
/// Opera no nivel de projeto (.csproj ProjectReferences), diferente das
/// regras DE (tipos) e CS (code style).
/// </summary>
public sealed class IN001_CanonicalLayerDependenciesRule : ProjectRule
{
    private const string BuildingBlocksPrefix = "Bedrock.BuildingBlocks.";

    public override string Name => "IN001_CanonicalLayerDependencies";

    public override string Description =>
        "Dependencias entre camadas de um bounded context devem seguir o grafo canonico da ADR IN-001.";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-001-camadas-canonicas-bounded-context.md";

    /// <summary>
    /// Camadas canonicas de um bounded context.
    /// </summary>
    public enum BoundedContextLayer
    {
        DomainEntities,
        Domain,
        Application,
        InfraData,
        InfraDataTech,
        Configuration,
        Bootstrapper,
        Api,
        Unknown
    }

    /// <summary>
    /// Matriz de dependencias permitidas intra-BC.
    /// Chave: camada de origem. Valor: camadas que a origem pode referenciar.
    /// </summary>
    private static readonly Dictionary<BoundedContextLayer, BoundedContextLayer[]> AllowedDependencies = new()
    {
        [BoundedContextLayer.DomainEntities] = [],
        [BoundedContextLayer.Configuration] = [],
        [BoundedContextLayer.Domain] = [BoundedContextLayer.DomainEntities, BoundedContextLayer.Configuration],
        [BoundedContextLayer.Application] = [BoundedContextLayer.Domain, BoundedContextLayer.Configuration],
        [BoundedContextLayer.InfraData] = [BoundedContextLayer.Domain, BoundedContextLayer.DomainEntities, BoundedContextLayer.InfraDataTech, BoundedContextLayer.Configuration],
        [BoundedContextLayer.InfraDataTech] = [BoundedContextLayer.DomainEntities, BoundedContextLayer.Configuration],
        [BoundedContextLayer.Bootstrapper] = [BoundedContextLayer.Application, BoundedContextLayer.InfraData, BoundedContextLayer.InfraDataTech, BoundedContextLayer.Configuration],
        [BoundedContextLayer.Api] = [BoundedContextLayer.Application, BoundedContextLayer.Bootstrapper, BoundedContextLayer.Configuration],
    };

    /// <summary>
    /// Sufixos reconhecidos para classificar a camada do projeto.
    /// Ordenados do mais especifico para o mais generico para evitar ambiguidade.
    /// </summary>
    private static readonly (string Suffix, BoundedContextLayer Layer)[] LayerSuffixes =
    [
        (".Domain.Entities", BoundedContextLayer.DomainEntities),
        (".Infra.CrossCutting.Configuration", BoundedContextLayer.Configuration),
        (".Infra.CrossCutting.Bootstrapper", BoundedContextLayer.Bootstrapper),
        (".Infra.Data.", BoundedContextLayer.InfraDataTech), // ex: .Infra.Data.PostgreSql
        (".Infra.Data", BoundedContextLayer.InfraData),
        (".Domain", BoundedContextLayer.Domain),
        (".Application", BoundedContextLayer.Application),
        (".Api", BoundedContextLayer.Api),
    ];

    protected override IReadOnlyList<Violation> AnalyzeProjectReferences(ProjectRuleContext context)
    {
        var violations = new List<Violation>();

        var sourceLayer = ClassifyLayer(context.ProjectName);
        if (sourceLayer == BoundedContextLayer.Unknown)
            return violations;

        var sourceBcPrefix = ExtractBcPrefix(context.ProjectName, sourceLayer);
        if (string.IsNullOrEmpty(sourceBcPrefix))
            return violations;

        foreach (var reference in context.DirectProjectReferences)
        {
            // Ignorar referencias a BuildingBlocks (framework)
            if (reference.StartsWith(BuildingBlocksPrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            var targetLayer = ClassifyLayer(reference);
            if (targetLayer == BoundedContextLayer.Unknown)
                continue;

            var targetBcPrefix = ExtractBcPrefix(reference, targetLayer);

            // Ignorar referencias cross-BC (prefixo diferente)
            if (!string.Equals(sourceBcPrefix, targetBcPrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            // Verificar se a dependencia é permitida
            if (!AllowedDependencies.TryGetValue(sourceLayer, out var allowed) ||
                !allowed.Contains(targetLayer))
            {
                var allowedNames = AllowedDependencies.TryGetValue(sourceLayer, out var a) && a.Length > 0
                    ? string.Join(", ", a)
                    : "(nenhuma camada intra-BC)";

                violations.Add(new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.CsprojRelativePath,
                    Line = 1,
                    Message = $"{context.ProjectName} ({sourceLayer}) referencia {reference} ({targetLayer}), " +
                              $"mas {sourceLayer} so pode referenciar: {allowedNames}.",
                    LlmHint = $"Remover a ProjectReference de '{reference}' no .csproj de '{context.ProjectName}'. " +
                              $"A camada {sourceLayer} nao deve depender diretamente de {targetLayer}. " +
                              $"Consulte a ADR IN-001 para o grafo de dependencias correto."
                });
            }
        }

        return violations;
    }

    /// <summary>
    /// Classifica a camada de um projeto pelo seu nome.
    /// </summary>
    public static BoundedContextLayer ClassifyLayer(string projectName)
    {
        foreach (var (suffix, layer) in LayerSuffixes)
        {
            // Para InfraDataTech, o sufixo ".Infra.Data." precisa aparecer no meio
            // (ex: ShopDemo.Auth.Infra.Data.PostgreSql contém ".Infra.Data." seguido de algo)
            if (layer == BoundedContextLayer.InfraDataTech)
            {
                var idx = projectName.IndexOf(suffix, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var afterSuffix = projectName[(idx + suffix.Length)..];
                    // Se há algo depois de ".Infra.Data.", é InfraDataTech
                    if (afterSuffix.Length > 0)
                        return BoundedContextLayer.InfraDataTech;
                }
                continue;
            }

            if (projectName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return layer;
        }

        return BoundedContextLayer.Unknown;
    }

    /// <summary>
    /// Extrai o prefixo do bounded context a partir do nome do projeto e sua camada.
    /// Ex: "ShopDemo.Auth.Domain.Entities" com camada DomainEntities -> "ShopDemo.Auth"
    /// </summary>
    public static string ExtractBcPrefix(string projectName, BoundedContextLayer layer)
    {
        foreach (var (suffix, l) in LayerSuffixes)
        {
            if (l != layer)
                continue;

            if (layer == BoundedContextLayer.InfraDataTech)
            {
                // Para InfraDataTech, encontrar ".Infra.Data." e pegar tudo antes
                var idx = projectName.IndexOf(suffix, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                    return projectName[..idx];
            }
            else if (projectName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return projectName[..^suffix.Length];
            }
        }

        return string.Empty;
    }
}
