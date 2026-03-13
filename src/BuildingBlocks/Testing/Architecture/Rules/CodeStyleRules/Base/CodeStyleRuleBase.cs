using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

/// <summary>
/// Classe base abstrata para regras de code style.
/// Define a categoria "Code Style" para todas as regras CS.
/// </summary>
public abstract class CodeStyleRuleBase : Rule
{
    public override string Category => "Code Style";

    /// <summary>
    /// Verifica se o tipo e uma factory (static class em *.Factories.* com sufixo Factory).
    /// </summary>
    protected static bool IsFactory(INamedTypeSymbol type)
    {
        if (!type.IsStatic || type.TypeKind != TypeKind.Class)
            return false;

        var ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return ns.Contains(".Factories", StringComparison.Ordinal) &&
               type.Name.EndsWith("Factory", StringComparison.Ordinal);
    }
}
