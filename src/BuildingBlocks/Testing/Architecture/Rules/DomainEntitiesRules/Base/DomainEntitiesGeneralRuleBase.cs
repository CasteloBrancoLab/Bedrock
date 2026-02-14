namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Classe base para regras Domain Entities que analisam TODOS os tipos
/// (nao apenas entidades). Usada por regras de enums, classes abstratas, etc.
/// Diferente de <see cref="DomainEntityRuleBase"/> que filtra apenas entidades concretas.
/// </summary>
public abstract class DomainEntitiesGeneralRuleBase : Rule
{
    public override string Category => "Domain Entities";
}
