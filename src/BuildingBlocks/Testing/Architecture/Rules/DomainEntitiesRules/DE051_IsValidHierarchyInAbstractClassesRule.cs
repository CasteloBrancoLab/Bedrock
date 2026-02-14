using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-051: Classes abstratas devem implementar hierarquia de IsValid com tres metodos.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Classe abstrata deve ter <c>IsValidConcreteInternal</c> como
///         <c>protected abstract</c></item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes concretas (verificadas por outras regras)</item>
///   <item>Classes que nao herdam de EntityBase</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE051_IsValidHierarchyInAbstractClassesRule : DomainEntitiesGeneralRuleBase
{
    // Properties
    public override string Name => "DE051_IsValidHierarchyInAbstractClasses";

    public override string Description =>
        "Classes abstratas devem ter IsValidConcreteInternal como protected abstract (DE-051)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-051-hierarquia-isvalid-em-classes-abstratas.md";

    /// <summary>
    /// Nome do metodo que a classe abstrata deve declarar como abstract.
    /// </summary>
    private const string IsValidConcreteInternalMethodName = "IsValidConcreteInternal";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // So se aplica a classes abstratas
        if (!type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        // So se aplica a classes que herdam de EntityBase
        if (!DomainEntityRuleBase.InheritsFromEntityBase(type))
            return null;

        // Verificar se tem IsValidConcreteInternal como protected abstract
        var hasIsValidConcreteInternal = false;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.Name != IsValidConcreteInternalMethodName)
                continue;

            hasIsValidConcreteInternal = true;

            // Verificar se e protected
            if (method.DeclaredAccessibility != Accessibility.Protected)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O metodo '{IsValidConcreteInternalMethodName}' da classe " +
                              $"abstrata '{type.Name}' tem acessibilidade " +
                              $"'{method.DeclaredAccessibility}' mas deveria ser 'protected'",
                    LlmHint = $"Alterar '{IsValidConcreteInternalMethodName}' para " +
                              $"'protected abstract'. " +
                              $"Consultar ADR DE-051 para exemplos"
                };
            }

            // Verificar se e abstract
            if (!method.IsAbstract)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O metodo '{IsValidConcreteInternalMethodName}' da classe " +
                              $"abstrata '{type.Name}' nao e abstract. " +
                              $"Deve ser 'protected abstract' para forcar classes filhas " +
                              $"a implementar validacoes especificas",
                    LlmHint = $"Alterar '{IsValidConcreteInternalMethodName}' para " +
                              $"'protected abstract bool {IsValidConcreteInternalMethodName}" +
                              $"(ExecutionContext executionContext)'. " +
                              $"Consultar ADR DE-051 para exemplos"
                };
            }

            break;
        }

        if (!hasIsValidConcreteInternal)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe abstrata '{type.Name}' nao declara o metodo " +
                          $"'{IsValidConcreteInternalMethodName}'. Classes abstratas " +
                          $"devem declarar este metodo como 'protected abstract' " +
                          $"para forcar classes filhas a implementar validacoes especificas",
                LlmHint = $"Adicionar 'protected abstract bool " +
                          $"{IsValidConcreteInternalMethodName}" +
                          $"(ExecutionContext executionContext);' a classe '{type.Name}'. " +
                          $"Consultar ADR DE-051 para exemplos"
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
