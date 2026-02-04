using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-056: Classes abstratas de dominio nao devem ter metodo CreateFromExistingInfo.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Classes abstratas que herdam de EntityBase nao devem declarar um metodo
///         <c>CreateFromExistingInfo</c>. Reconstitution e responsabilidade da classe concreta.</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes concretas (devem ter CreateFromExistingInfo)</item>
///   <item>Classes que nao herdam de EntityBase</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE056_NoCreateFromExistingInfoInAbstractClassesRule : Rule
{
    // Properties
    public override string Name => "DE056_NoCreateFromExistingInfoInAbstractClasses";

    public override string Description =>
        "Classes abstratas nao devem ter CreateFromExistingInfo. " +
        "Reconstitution e responsabilidade da classe concreta (DE-056)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-056-classe-abstrata-nao-tem-createfromexistinginfo.md";

    /// <summary>
    /// Nome do metodo proibido em classes abstratas.
    /// </summary>
    private const string CreateFromExistingInfoMethodName = "CreateFromExistingInfo";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // So se aplica a classes abstratas
        if (!type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        // So se aplica a classes que herdam de EntityBase
        if (!DomainEntityRuleBase.InheritsFromEntityBase(type))
            return null;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.Name != CreateFromExistingInfoMethodName)
                continue;

            // Ignorar metodos herdados
            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, type))
                continue;

            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = GetMethodLineNumber(method, context.LineNumber),
                Message = $"A classe abstrata '{type.Name}' declara o metodo " +
                          $"'{CreateFromExistingInfoMethodName}'. Classes abstratas " +
                          $"nao podem ser instanciadas, portanto reconstitution " +
                          $"(CreateFromExistingInfo) deve ser implementado apenas " +
                          $"nas classes concretas (sealed)",
                LlmHint = $"Remover '{CreateFromExistingInfoMethodName}' da classe " +
                          $"abstrata '{type.Name}'. Cada classe filha concreta deve " +
                          $"implementar seu proprio CreateFromExistingInfo com input " +
                          $"completo (propriedades da pai + proprias). " +
                          $"Consultar ADR DE-056 para exemplos"
            };
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
