namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-003: Valida que projetos Domain possuem zero dependencias externas proibidas.
///
/// Projetos *.Domain (mas NAO *.Domain.Entities) so podem referenciar:
/// - ProjectReference: Bedrock.BuildingBlocks.* (framework), *.Domain.Entities ou *.Entities (shared kernel), *.Configuration
/// - PackageReference: nenhum (zero NuGet externo)
///
/// Projetos que NAO terminam com .Domain ou que terminam com .Domain.Entities sao ignorados.
/// </summary>
public sealed class IN003_DomainZeroExternalDependenciesRule : InfrastructureRuleBase
{
    private const string DomainSuffix = ".Domain";
    private const string DomainEntitiesSuffix = ".Domain.Entities";
    private const string BuildingBlocksPrefix = "Bedrock.BuildingBlocks.";
    private const string EntitiesSuffix = ".Entities";
    private const string ConfigurationSuffix = ".Configuration";

    public override string Name => "IN003_DomainZeroExternalDependencies";

    public override string Description =>
        "Projetos Domain devem depender apenas de Domain.Entities, Configuration e framework Bedrock.";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-003-domain-projeto-separado.md";

    protected override IReadOnlyList<ProjectReferenceCheckResult> AnalyzeProjectReferences(ProjectRuleContext context)
    {
        // Ignorar projetos que nao sao Domain (ou que sao Domain.Entities)
        if (!IsDomainProject(context.ProjectName))
            return [];

        var results = new List<ProjectReferenceCheckResult>();

        // Verificar cada ProjectReference
        foreach (var reference in context.DirectProjectReferences)
        {
            if (IsAllowedProjectReference(reference))
            {
                results.Add(new ProjectReferenceCheckResult
                {
                    TargetReference = reference,
                    Description = $"ProjectReference '{reference}' permitida",
                    IsValid = true
                });
            }
            else
            {
                results.Add(new ProjectReferenceCheckResult
                {
                    TargetReference = reference,
                    Description = $"ProjectReference '{reference}' proibida em Domain",
                    IsValid = false,
                    Violation = new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = context.CsprojRelativePath,
                        Line = 1,
                        Message = $"{context.ProjectName} referencia '{reference}', mas projetos Domain " +
                                  $"so podem referenciar Bedrock.BuildingBlocks.*, *.Domain.Entities, *.Entities (shared kernel) ou *.Configuration.",
                        LlmHint = $"Remover a ProjectReference de '{reference}' no .csproj de '{context.ProjectName}'. " +
                                  $"Projetos Domain devem depender apenas de Domain.Entities, Configuration e framework Bedrock. " +
                                  $"Consulte a ADR IN-003 para as regras de isolamento."
                    }
                });
            }
        }

        // Verificar cada PackageReference (nenhum permitido)
        foreach (var package in context.DirectPackageReferences)
        {
            results.Add(new ProjectReferenceCheckResult
            {
                TargetReference = package,
                Description = $"PackageReference '{package}' proibida em Domain",
                IsValid = false,
                Violation = new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.CsprojRelativePath,
                    Line = 1,
                    Message = $"{context.ProjectName} possui PackageReference '{package}', " +
                              $"mas projetos Domain nao podem ter pacotes NuGet externos.",
                    LlmHint = $"Remover a PackageReference de '{package}' no .csproj de '{context.ProjectName}'. " +
                              $"Projetos Domain devem ter zero pacotes NuGet. " +
                              $"Se a funcionalidade for necessaria, mova para a camada Application ou superior. " +
                              $"Consulte a ADR IN-003."
                }
            });
        }

        return results;
    }

    /// <summary>
    /// Verifica se o projeto e um projeto Domain (termina com .Domain, mas NAO com .Domain.Entities).
    /// </summary>
    public static bool IsDomainProject(string projectName)
    {
        return projectName.EndsWith(DomainSuffix, StringComparison.OrdinalIgnoreCase)
            && !projectName.EndsWith(DomainEntitiesSuffix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica se uma ProjectReference e permitida para projetos Domain.
    /// Permitidos: Bedrock.BuildingBlocks.* (framework), *.Domain.Entities ou *.Entities (shared kernel), *.Configuration.
    /// </summary>
    public static bool IsAllowedProjectReference(string reference)
    {
        // Bedrock.BuildingBlocks.* (framework)
        if (reference.StartsWith(BuildingBlocksPrefix, StringComparison.OrdinalIgnoreCase))
            return true;

        // *.Domain.Entities (proprio BC)
        if (reference.EndsWith(DomainEntitiesSuffix, StringComparison.OrdinalIgnoreCase))
            return true;

        // *.Entities (shared kernel generico)
        if (reference.EndsWith(EntitiesSuffix, StringComparison.OrdinalIgnoreCase))
            return true;

        // *.Configuration
        if (reference.EndsWith(ConfigurationSuffix, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
