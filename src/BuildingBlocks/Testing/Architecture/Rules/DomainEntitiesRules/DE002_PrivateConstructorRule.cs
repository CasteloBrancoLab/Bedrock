using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-002: Classes concretas devem ter apenas construtores privados.
/// A criação de instâncias deve ser feita via factory methods estáticos.
/// Exceções: classes abstratas, estáticas, records, enums, interfaces, structs.
/// </summary>
public sealed class DE002_PrivateConstructorRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE002_PrivateConstructor";
    public override string Description => "Classes concretas devem ter apenas construtores privados (DE-002)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/domain-entities/DE-002-construtores-privados-com-factory-methods.md";

    /// <summary>
    /// Aplica-se a todas as classes concretas, não apenas EntityBase.
    /// </summary>
    protected override bool RequiresEntityBaseInheritance => false;

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Verificar construtores de instância (ignorar estáticos)
        var instanceConstructors = type.InstanceConstructors;

        foreach (var ctor in instanceConstructors)
        {
            // Ignorar construtores implícitos (gerados pelo compilador)
            if (ctor.IsImplicitlyDeclared)
                continue;

            if (ctor.DeclaredAccessibility != Accessibility.Private)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = context.LineNumber,
                    Message = $"Classe '{type.Name}' possui construtor {ctor.DeclaredAccessibility.ToString().ToLowerInvariant()}. Construtores devem ser privados",
                    LlmHint = $"Alterar o(s) construtor(es) da classe '{type.Name}' para private e usar factory methods estáticos para criação"
                };
            }
        }

        return null;
    }
}
