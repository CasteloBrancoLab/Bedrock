namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-002: Valida que projetos Domain.Entities possuem zero dependencias externas.
///
/// Projetos *.Domain.Entities so podem referenciar:
/// - ProjectReference: Bedrock.BuildingBlocks.* (framework) ou outros *.Entities (shared kernel)
/// - PackageReference: nenhum (zero NuGet externo)
///
/// Projetos que NAO terminam com .Domain.Entities sao ignorados.
/// </summary>
public sealed class IN002_DomainEntitiesZeroExternalDependenciesRule : InfrastructureRuleBase
{
    private const string DomainEntitiesSuffix = ".Domain.Entities";
    private const string BuildingBlocksPrefix = "Bedrock.BuildingBlocks.";
    private const string EntitiesSuffix = ".Entities";

    public override string Name => "IN002_DomainEntitiesZeroExternalDependencies";

    public override string Description =>
        "Projetos Domain.Entities devem ter zero dependencias externas (apenas Bedrock.BuildingBlocks.* e outros *.Entities).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-002-domain-entities-projeto-separado.md";

    protected override IReadOnlyList<ProjectReferenceCheckResult> AnalyzeProjectReferences(ProjectRuleContext context)
    {
        // Ignorar projetos que nao sao Domain.Entities
        if (!context.ProjectName.EndsWith(DomainEntitiesSuffix, StringComparison.OrdinalIgnoreCase))
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
                    Description = $"ProjectReference '{reference}' proibida em Domain.Entities",
                    IsValid = false,
                    Violation = new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = context.CsprojRelativePath,
                        Line = 1,
                        Message = $"{context.ProjectName} referencia '{reference}', mas projetos Domain.Entities " +
                                  $"so podem referenciar Bedrock.BuildingBlocks.* ou outros *.Entities (shared kernel).",
                        LlmHint = $"Remover a ProjectReference de '{reference}' no .csproj de '{context.ProjectName}'. " +
                                  $"Projetos Domain.Entities devem ter zero dependencias externas. " +
                                  $"Consulte a ADR IN-002 para as regras de isolamento."
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
                Description = $"PackageReference '{package}' proibida em Domain.Entities",
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
                              $"mas projetos Domain.Entities nao podem ter pacotes NuGet externos.",
                    LlmHint = $"Remover a PackageReference de '{package}' no .csproj de '{context.ProjectName}'. " +
                              $"Projetos Domain.Entities devem ter zero pacotes NuGet. " +
                              $"Se a funcionalidade for necessaria, mova para a camada Domain ou superior. " +
                              $"Consulte a ADR IN-002."
                }
            });
        }

        return results;
    }

    /// <summary>
    /// Verifica se uma ProjectReference e permitida para projetos Domain.Entities.
    /// Permitidos: Bedrock.BuildingBlocks.* (framework) e outros *.Entities (shared kernel).
    /// </summary>
    public static bool IsAllowedProjectReference(string reference)
    {
        // Bedrock.BuildingBlocks.* (framework)
        if (reference.StartsWith(BuildingBlocksPrefix, StringComparison.OrdinalIgnoreCase))
            return true;

        // Outros projetos *.Entities (shared kernel)
        if (reference.EndsWith(EntitiesSuffix, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
