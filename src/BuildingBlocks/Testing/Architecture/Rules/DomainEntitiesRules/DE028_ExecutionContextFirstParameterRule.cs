using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-028: <c>ExecutionContext</c> deve ser sempre o <b>primeiro parâmetro</b>
/// em métodos que o recebem.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos que possuem parâmetro do tipo <c>ExecutionContext</c> devem tê-lo
///         como primeiro parâmetro</item>
///   <item>Convenção garante consistência, visibilidade e auto-documentação</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Métodos que não recebem ExecutionContext (não aplicável)</item>
///   <item>Métodos gerados pelo compilador</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE028_ExecutionContextFirstParameterRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE028_ExecutionContextFirstParameter";

    public override string Description =>
        "ExecutionContext deve ser o primeiro parâmetro em métodos que o recebem (DE-028)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-028-executioncontext-explicito.md";

    /// <summary>
    /// Nome do tipo ExecutionContext.
    /// </summary>
    private const string ExecutionContextTypeName = "ExecutionContext";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Ignorar construtores, propriedades, etc.
            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Ignorar métodos gerados pelo compilador
            if (method.IsImplicitlyDeclared)
                continue;

            // Ignorar métodos abstratos ou extern
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Verificar se algum parâmetro é ExecutionContext
            var executionContextIndex = FindExecutionContextParameterIndex(method);

            // Se não tem ExecutionContext, regra não se aplica
            if (executionContextIndex < 0)
                continue;

            // Se tem ExecutionContext mas não é o primeiro, é violação
            if (executionContextIndex > 0)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O método '{method.Name}' da classe '{type.Name}' recebe " +
                              $"ExecutionContext na posição {executionContextIndex}, mas deveria " +
                              $"ser o primeiro parâmetro (posição 0). Convenção garante " +
                              $"consistência e auto-documentação",
                    LlmHint = $"Mover o parâmetro ExecutionContext para a primeira posição " +
                              $"no método '{method.Name}'. Atualizar todos os call sites. " +
                              $"Consultar ADR DE-028 para convenção de posicionamento"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Encontra o índice do parâmetro ExecutionContext no método.
    /// Retorna -1 se não encontrado.
    /// </summary>
    private static int FindExecutionContextParameterIndex(IMethodSymbol method)
    {
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            if (method.Parameters[i].Type.Name == ExecutionContextTypeName)
                return i;
        }

        return -1;
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
