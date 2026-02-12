using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

/// <summary>
/// Regra CS-003: Todo log emitido em código que possui <c>ExecutionContext</c>
/// disponível como parâmetro DEVE usar as variantes <c>ForDistributedTracing</c>
/// em vez da API padrão do <c>ILogger</c>.
/// </summary>
public sealed class CS003_LoggingWithDistributedTracingRule : Rule
{
    private static readonly HashSet<string> ForbiddenLogMethods = new(StringComparer.Ordinal)
    {
        "Log",
        "LogTrace",
        "LogDebug",
        "LogInformation",
        "LogWarning",
        "LogError",
        "LogCritical"
    };

    public override string Name => "CS003_LoggingWithDistributedTracing";
    public override string Description =>
        "Logging deve usar variantes ForDistributedTracing quando ExecutionContext está disponível (CS-003)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath =>
        "docs/adrs/code-style/CS-003-logging-sempre-com-distributed-tracing.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;
        var compilation = context.Compilation;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (!HasExecutionContextParameter(method))
                continue;

            foreach (var syntaxRef in method.DeclaringSyntaxReferences)
            {
                var syntaxNode = syntaxRef.GetSyntax();
                var sourceText = syntaxNode.SyntaxTree.GetText();

                foreach (var invocation in syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                        continue;

                    var methodName = memberAccess.Name.Identifier.Text;

                    if (!ForbiddenLogMethods.Contains(methodName))
                        continue;

                    var invocationLineSpan = invocation.GetLocation().GetLineSpan();
                    var invocationLine = invocationLineSpan.StartLinePosition.Line;

                    if (IsSuppressed(sourceText, invocationLine))
                        continue;

                    var lineNumber = invocationLine + 1;
                    var filePath = GetRelativePath(
                        invocationLineSpan.Path, context.RootDir);

                    var suggestedMethod = GetSuggestedMethod(methodName);

                    return new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = string.IsNullOrEmpty(filePath) ? context.RelativeFilePath : filePath,
                        Line = lineNumber,
                        Message = $"Método '{method.Name}' no tipo '{type.Name}' usa '{methodName}' " +
                                  $"em vez da variante ForDistributedTracing. " +
                                  $"ExecutionContext está disponível como parâmetro.",
                        LlmHint = $"Substituir '{methodName}' por '{suggestedMethod}' " +
                                  $"passando executionContext como primeiro argumento, " +
                                  $"ou suprimir com '// CS003 disable once : razão'"
                    };
                }
            }
        }

        return null;
    }

    private static bool HasExecutionContextParameter(IMethodSymbol method)
    {
        foreach (var parameter in method.Parameters)
        {
            if (parameter.Type.Name == "ExecutionContext" &&
                parameter.Type.ContainingNamespace?.ToDisplayString() != "System.Threading")
                return true;
        }
        return false;
    }

    private static string GetSuggestedMethod(string forbiddenMethodName)
    {
        return forbiddenMethodName switch
        {
            "Log" => "LogForDistributedTracing",
            "LogError" => "LogErrorForDistributedTracing ou LogExceptionForDistributedTracing (se houver Exception)",
            _ => $"{forbiddenMethodName}ForDistributedTracing"
        };
    }

    private static bool IsSuppressed(Microsoft.CodeAnalysis.Text.SourceText sourceText, int invocationLine)
    {
        if (invocationLine > 0)
        {
            var prevLineText = GetLineText(sourceText, invocationLine - 1);
            if (prevLineText.Contains("CS003 disable once", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var inDisableBlock = false;
        for (var i = 0; i <= invocationLine; i++)
        {
            var lineText = GetLineText(sourceText, i);

            if (lineText.Contains("CS003 disable", StringComparison.OrdinalIgnoreCase) &&
                !lineText.Contains("CS003 disable once", StringComparison.OrdinalIgnoreCase))
            {
                inDisableBlock = true;
            }
            else if (lineText.Contains("CS003 restore", StringComparison.OrdinalIgnoreCase))
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
