using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-029: Entidades NÃO devem usar <c>DateTime.Now</c>, <c>DateTime.UtcNow</c>,
/// <c>DateTimeOffset.Now</c> ou <c>DateTimeOffset.UtcNow</c> diretamente.
/// O tempo deve vir do <c>TimeProvider</c> encapsulado no <c>ExecutionContext</c>.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Corpo dos métodos NÃO deve conter acesso a membros estáticos de tempo do sistema</item>
///   <item>Deve usar <c>ctx.TimeProvider.GetUtcNow()</c> ou <c>ctx.Timestamp</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Métodos abstratos ou extern (sem corpo)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE029_TimeProviderViaExecutionContextRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE029_TimeProviderViaExecutionContext";

    public override string Description =>
        "Entidades devem usar TimeProvider do ExecutionContext, não DateTime.Now/UtcNow (DE-029)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-029-timeprovider-encapsulado-no-executioncontext.md";

    /// <summary>
    /// Nomes de membros proibidos e seus tipos correspondentes.
    /// </summary>
    private static readonly HashSet<string> ForbiddenMemberNames = new(StringComparer.Ordinal)
    {
        "Now",
        "UtcNow"
    };

    /// <summary>
    /// Tipos que contêm os membros proibidos.
    /// </summary>
    private static readonly HashSet<string> ForbiddenTypeNames = new(StringComparer.Ordinal)
    {
        "DateTime",
        "DateTimeOffset"
    };

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Ignorar métodos abstratos ou extern
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Ignorar gerados pelo compilador
            if (method.IsImplicitlyDeclared)
                continue;

            var forbiddenUsage = FindForbiddenTimeUsage(method);
            if (forbiddenUsage is not null)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O método '{method.Name}' da classe '{type.Name}' usa " +
                              $"'{forbiddenUsage}' diretamente. Usar " +
                              $"'executionContext.TimeProvider.GetUtcNow()' ou " +
                              $"'executionContext.Timestamp' para garantir testabilidade " +
                              $"e consistência temporal",
                    LlmHint = $"Substituir '{forbiddenUsage}' por " +
                              $"'executionContext.TimeProvider.GetUtcNow()' no método " +
                              $"'{method.Name}'. Consultar ADR DE-029 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Procura no corpo do método por uso direto de DateTime.Now, DateTime.UtcNow,
    /// DateTimeOffset.Now ou DateTimeOffset.UtcNow.
    /// </summary>
    /// <returns>O uso proibido encontrado (ex: "DateTime.Now"), ou null.</returns>
    private static string? FindForbiddenTimeUsage(IMethodSymbol method)
    {
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is not MemberAccessExpressionSyntax memberAccess)
                    continue;

                var memberName = memberAccess.Name.Identifier.ValueText;

                if (!ForbiddenMemberNames.Contains(memberName))
                    continue;

                // Verificar se o tipo é DateTime ou DateTimeOffset
                if (memberAccess.Expression is IdentifierNameSyntax identifier &&
                    ForbiddenTypeNames.Contains(identifier.Identifier.ValueText))
                {
                    return $"{identifier.Identifier.ValueText}.{memberName}";
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Obtém o número da linha onde o método é declarado.
    /// </summary>
    private static int GetMethodLineNumber(IMethodSymbol method, int fallbackLineNumber)
    {
        var location = method.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
