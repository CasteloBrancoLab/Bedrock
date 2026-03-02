using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-013: Tipos em *.Application.* (exceto *.Factories) nao devem instanciar
/// EventBase-derived diretamente. A criacao de eventos deve ser feita via factory.
/// </summary>
public sealed class MS013_EventCreationViaFactoryRule : MessageGeneralRuleBase
{
    public override string Name => "MS013_EventCreationViaFactory";

    public override string Description =>
        "Tipos em Application (exceto Factories) nao devem instanciar eventos diretamente. Usar factory (MS-013)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-013-criacao-eventos-via-factory.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Filtro: apenas tipos em namespace contendo ".Application."
        var ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (!ns.Contains(".Application.", StringComparison.Ordinal) &&
            !ns.EndsWith(".Application", StringComparison.Ordinal))
            return null;

        // Excluir: tipos em namespace contendo ".Factories"
        if (ns.Contains(".Factories", StringComparison.Ordinal))
            return null;

        // Analisar syntax tree para encontrar instanciacao de EventBase-derived
        foreach (var syntaxRef in type.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();
            var tree = syntaxNode.SyntaxTree;
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                ITypeSymbol? createdType = null;

                if (descendant is ObjectCreationExpressionSyntax objectCreation)
                {
                    var typeInfo = semanticModel.GetTypeInfo(objectCreation);
                    createdType = typeInfo.Type;
                }
                else if (descendant is ImplicitObjectCreationExpressionSyntax implicitCreation)
                {
                    var typeInfo = semanticModel.GetTypeInfo(implicitCreation);
                    createdType = typeInfo.Type;
                }

                if (createdType is INamedTypeSymbol namedCreatedType &&
                    MessageRuleBase.InheritsFromMessageBase(namedCreatedType))
                {
                    // Verificar se herda especificamente de EventBase
                    if (InheritsFromEventBase(namedCreatedType))
                    {
                        var lineSpan = descendant.GetLocation().GetLineSpan();
                        var line = lineSpan.StartLinePosition.Line + 1;

                        return new Violation
                        {
                            Rule = Name,
                            Severity = DefaultSeverity,
                            Adr = AdrPath,
                            Project = context.ProjectName,
                            File = context.RelativeFilePath,
                            Line = line,
                            Message = $"O tipo '{type.Name}' instancia '{namedCreatedType.Name}' " +
                                      $"(EventBase-derived) diretamente. A criacao de eventos deve " +
                                      $"ser feita via factory em *.Factories",
                            LlmHint = $"Mover criacao de '{namedCreatedType.Name}' para uma factory " +
                                      $"em *.Factories. Use case deve fazer UMA chamada a factory"
                        };
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o tipo herda de EventBase (direta ou indiretamente).
    /// </summary>
    private static bool InheritsFromEventBase(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.Name == "EventBase")
                return true;

            current = current.BaseType;
        }

        return false;
    }
}
