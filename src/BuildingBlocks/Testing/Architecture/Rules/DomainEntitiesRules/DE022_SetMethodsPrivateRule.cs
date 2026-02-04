using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-022: Métodos <c>Set*</c> devem ser privados. Eles validam e atribuem
/// uma única propriedade, sendo chamados exclusivamente por métodos <c>*Internal</c>.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos cujo nome comece com <c>Set</c> devem ter acessibilidade <c>private</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Métodos herdados de Object</item>
///   <item>Property accessors (set_*)</item>
///   <item><c>SetEntityInfo</c> (gerenciado por EntityBase)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE022_SetMethodsPrivateRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE022_SetMethodsPrivate";

    public override string Description =>
        "Métodos Set* devem ser privados para garantir encapsulamento (DE-022)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-022-metodos-set-privados.md";

    /// <summary>
    /// Prefixo dos métodos de atribuição.
    /// </summary>
    private const string SetPrefix = "Set";

    /// <summary>
    /// Método gerenciado por EntityBase que é exceção.
    /// </summary>
    private const string SetEntityInfoMethodName = "SetEntityInfo";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Apenas métodos ordinários (ignora property accessors, operators, etc.)
            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Verificar apenas métodos cujo nome comece com "Set"
            if (!method.Name.StartsWith(SetPrefix, StringComparison.Ordinal))
                continue;

            // Ignorar métodos herdados de Object
            if (IsObjectMethod(method))
                continue;

            // Ignorar SetEntityInfo (gerenciado por EntityBase)
            if (method.Name == SetEntityInfoMethodName)
                continue;

            // Ignorar métodos abstratos ou extern
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Verificar que é privado
            if (method.DeclaredAccessibility != Accessibility.Private)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O método '{method.Name}' da classe '{type.Name}' tem acessibilidade " +
                              $"'{method.DeclaredAccessibility}', mas deveria ser 'Private'. " +
                              $"Métodos Set* validam e atribuem uma propriedade e devem ser privados",
                    LlmHint = $"Alterar o método '{method.Name}' da classe '{type.Name}' para 'private'. " +
                              $"Métodos Set* são chamados exclusivamente por métodos *Internal " +
                              $"e não devem ser acessíveis externamente. " +
                              $"Consultar ADR DE-022 para fundamentação"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Obtém o número da linha onde o método é declarado, ou fallback para a linha da classe.
    /// </summary>
    private static int GetMethodLineNumber(IMethodSymbol method, int fallbackLineNumber)
    {
        var location = method.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
