using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-055: Classes abstratas de dominio devem ter metodo RegisterNewBase
/// para encapsular o processo de registro e garantir que propriedades da classe pai
/// sejam sempre validadas e inicializadas.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Classes abstratas que herdam de EntityBase devem ter um metodo
///         <c>RegisterNewBase</c> publico e estatico</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes concretas (usam RegisterNew, nao RegisterNewBase)</item>
///   <item>Classes que nao herdam de EntityBase</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE055_RegisterNewBaseInAbstractClassesRule : Rule
{
    // Properties
    public override string Name => "DE055_RegisterNewBaseInAbstractClasses";

    public override string Description =>
        "Classes abstratas devem ter metodo RegisterNewBase para controlar registro (DE-055)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-055-registernewbase-em-classes-abstratas.md";

    /// <summary>
    /// Nome do metodo esperado.
    /// </summary>
    private const string RegisterNewBaseMethodName = "RegisterNewBase";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // So se aplica a classes abstratas
        if (!type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        // So se aplica a classes que herdam de EntityBase
        if (!DomainEntityRuleBase.InheritsFromEntityBase(type))
            return null;

        // Verificar se tem RegisterNewBase
        var hasRegisterNewBase = false;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.Name != RegisterNewBaseMethodName)
                continue;

            if (!method.IsStatic)
                continue;

            if (method.DeclaredAccessibility != Accessibility.Public)
                continue;

            hasRegisterNewBase = true;
            break;
        }

        if (!hasRegisterNewBase)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe abstrata '{type.Name}' nao tem o metodo " +
                          $"'{RegisterNewBaseMethodName}'. Classes abstratas devem " +
                          $"encapsular seu processo de registro via RegisterNewBase " +
                          $"para garantir que suas propriedades sejam sempre validadas " +
                          $"e inicializadas pelas classes filhas",
                LlmHint = $"Adicionar 'public static TConcreteType? " +
                          $"{RegisterNewBaseMethodName}<TConcreteType, TInput>(" +
                          $"ExecutionContext, TInput, ...)' a classe '{type.Name}'. " +
                          $"Consultar ADR DE-055 para exemplos"
            };
        }

        return null;
    }
}
