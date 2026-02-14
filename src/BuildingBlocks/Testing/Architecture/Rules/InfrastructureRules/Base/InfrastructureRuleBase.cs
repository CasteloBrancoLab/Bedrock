namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// Classe base abstrata para regras de infraestrutura.
/// Define a categoria "Infrastructure" para todas as regras IN.
/// </summary>
public abstract class InfrastructureRuleBase : ProjectRule
{
    public override string Category => "Infrastructure";
}
