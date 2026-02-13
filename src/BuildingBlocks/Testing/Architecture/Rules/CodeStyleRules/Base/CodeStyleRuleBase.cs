namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

/// <summary>
/// Classe base abstrata para regras de code style.
/// Define a categoria "Code Style" para todas as regras CS.
/// </summary>
public abstract class CodeStyleRuleBase : Rule
{
    public override string Category => "Code Style";
}
