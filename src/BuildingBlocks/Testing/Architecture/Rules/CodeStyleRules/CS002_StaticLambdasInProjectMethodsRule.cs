using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

/// <summary>
/// Regra CS-002: Toda lambda inline passada como argumento a um método cujo tipo
/// pertence ao namespace raiz do projeto DEVE ser <c>static</c>.
/// Previne closures e alocações no heap.
/// </summary>
public sealed class CS002_StaticLambdasInProjectMethodsRule : Rule
{
    public override string Name => "CS002_StaticLambdasInProjectMethods";
    public override string Description =>
        "Lambdas inline em métodos do projeto devem ser static (CS-002)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath =>
        "docs/adrs/code-style/CS-002-lambdas-inline-devem-ser-static.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;
        var compilation = context.Compilation;
        var rootNamespace = GetRootNamespace(compilation);

        if (string.IsNullOrEmpty(rootNamespace))
            return null;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol and not IPropertySymbol)
                continue;

            foreach (var syntaxRef in member.DeclaringSyntaxReferences)
            {
                var syntaxNode = syntaxRef.GetSyntax();
                var semanticModel = compilation.GetSemanticModel(syntaxNode.SyntaxTree);
                var sourceText = syntaxNode.SyntaxTree.GetText();

                foreach (var invocation in syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                    if (symbolInfo.Symbol is not IMethodSymbol invokedMethod)
                        continue;

                    if (!IsProjectMethod(invokedMethod, rootNamespace))
                        continue;

                    foreach (var argument in invocation.ArgumentList.Arguments)
                    {
                        if (argument.Expression is not AnonymousFunctionExpressionSyntax lambda)
                            continue;

                        if (IsStaticLambda(lambda))
                            continue;

                        var lambdaLineSpan = lambda.GetLocation().GetLineSpan();
                        var lambdaLine = lambdaLineSpan.StartLinePosition.Line;

                        if (IsSuppressed(sourceText, lambdaLine))
                            continue;

                        var lineNumber = lambdaLine + 1;
                        var filePath = GetRelativePath(
                            lambdaLineSpan.Path, context.RootDir);

                        return new Violation
                        {
                            Rule = Name,
                            Severity = DefaultSeverity,
                            Adr = AdrPath,
                            Project = context.ProjectName,
                            File = string.IsNullOrEmpty(filePath) ? context.RelativeFilePath : filePath,
                            Line = lineNumber,
                            Message = $"Lambda não-static passada como argumento para " +
                                      $"'{invokedMethod.ContainingType.Name}.{invokedMethod.Name}' " +
                                      $"no tipo '{type.Name}'. Adicione o modificador 'static'.",
                            LlmHint = $"Adicionar modificador 'static' à lambda no argumento de " +
                                      $"'{invokedMethod.Name}' ou suprimir com '// CS002 disable once : razão'"
                        };
                    }
                }
            }
        }

        return null;
    }

    private static string GetRootNamespace(Compilation compilation)
    {
        var assemblyName = compilation.AssemblyName;
        if (string.IsNullOrEmpty(assemblyName))
            return string.Empty;

        var dotIndex = assemblyName.IndexOf('.');
        return dotIndex > 0 ? assemblyName[..dotIndex] : assemblyName;
    }

    private static bool IsProjectMethod(IMethodSymbol method, string rootNamespace)
    {
        var containingType = method.ContainingType;
        if (containingType is null)
            return false;

        var ns = containingType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return ns.StartsWith(rootNamespace, StringComparison.Ordinal) &&
               (ns.Length == rootNamespace.Length ||
                ns[rootNamespace.Length] == '.');
    }

    private static bool IsStaticLambda(AnonymousFunctionExpressionSyntax lambda)
    {
        return lambda.Modifiers.Any(SyntaxKind.StaticKeyword);
    }

    private static bool IsSuppressed(Microsoft.CodeAnalysis.Text.SourceText sourceText, int lambdaLine)
    {
        // Check for "CS002 disable once" on the line immediately before the lambda
        if (lambdaLine > 0)
        {
            var prevLineText = GetLineText(sourceText, lambdaLine - 1);
            if (prevLineText.Contains("CS002 disable once", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Check for block-level "CS002 disable" / "CS002 restore"
        // Scan from the start of the file up to the lambda line
        var inDisableBlock = false;
        for (var i = 0; i <= lambdaLine; i++)
        {
            var lineText = GetLineText(sourceText, i);

            if (lineText.Contains("CS002 disable", StringComparison.OrdinalIgnoreCase) &&
                !lineText.Contains("CS002 disable once", StringComparison.OrdinalIgnoreCase))
            {
                inDisableBlock = true;
            }
            else if (lineText.Contains("CS002 restore", StringComparison.OrdinalIgnoreCase))
            {
                inDisableBlock = false;
            }
        }

        return inDisableBlock;
    }

    private static string GetLineText(Microsoft.CodeAnalysis.Text.SourceText sourceText, int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= sourceText.Lines.Count)
            return string.Empty;

        var line = sourceText.Lines[lineIndex];
        return sourceText.ToString(line.Span).Trim();
    }
}
