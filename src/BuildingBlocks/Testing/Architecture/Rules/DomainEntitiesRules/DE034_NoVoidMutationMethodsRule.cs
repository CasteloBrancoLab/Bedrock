using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-034: Entidades de dominio NAO devem ter metodos publicos de instancia
/// que retornam <c>void</c> (exceto overrides de Object).
/// Metodos void indicam mutabilidade direta, violando o padrao Clone-Modify-Return.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Metodos publicos de instancia NAO devem retornar void</item>
///   <item>Metodos de mudanca de estado devem retornar <c>T?</c> (nullable do tipo da entidade)</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Metodos estaticos (factory methods, validacao)</item>
///   <item>Metodos herdados/override de Object (ToString, Equals, GetHashCode)</item>
///   <item>Construtores</item>
///   <item>Metodos gerados pelo compilador</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE034_NoVoidMutationMethodsRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE034_NoVoidMutationMethods";

    public override string Description =>
        "Metodos publicos de instancia nao devem retornar void - usar Clone-Modify-Return (DE-034)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-034-antipadrao-mutabilidade-direta.md";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Apenas metodos comuns (nao construtores, operadores, etc.)
            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Apenas metodos publicos
            if (method.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Ignorar metodos estaticos (factory methods, validacao, etc.)
            if (method.IsStatic)
                continue;

            // Ignorar metodos gerados pelo compilador
            if (method.IsImplicitlyDeclared)
                continue;

            // Ignorar overrides de Object (ToString, Equals, GetHashCode)
            if (IsObjectMethod(method))
                continue;

            // Verificar se retorna void
            if (method.ReturnsVoid)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O metodo publico '{method.Name}' da classe '{type.Name}' retorna " +
                              $"void. Metodos publicos de instancia devem usar o padrao " +
                              $"Clone-Modify-Return, retornando '{type.Name}?' em vez de void " +
                              $"para garantir atomicidade e preservar o estado original em caso " +
                              $"de falha",
                    LlmHint = $"Alterar o metodo '{method.Name}' para retornar '{type.Name}?' " +
                              $"usando RegisterChangeInternal. Consultar ADR DE-034 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Obtem o numero da linha onde o metodo e declarado.
    /// </summary>
    private static int GetMethodLineNumber(IMethodSymbol method, int fallbackLineNumber)
    {
        var location = method.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
