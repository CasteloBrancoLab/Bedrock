namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Resultado da verificacao de uma ProjectReference por uma regra de projeto.
/// Permite que <see cref="ProjectRule"/> registre tanto passes quanto falhas,
/// dando visibilidade real ao que foi validado.
/// </summary>
public sealed class ProjectReferenceCheckResult
{
    /// <summary>
    /// Nome do projeto referenciado (ex: ShopDemo.Auth.Domain.Entities).
    /// </summary>
    public required string TargetReference { get; init; }

    /// <summary>
    /// Descricao legivel da verificacao (ex: "Domain -> DomainEntities (permitido)").
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Se a referencia e valida (<c>true</c>) ou viola a regra (<c>false</c>).
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Violacao associada. Nao-nulo apenas quando <see cref="IsValid"/> e <c>false</c>.
    /// </summary>
    public Violation? Violation { get; init; }
}
