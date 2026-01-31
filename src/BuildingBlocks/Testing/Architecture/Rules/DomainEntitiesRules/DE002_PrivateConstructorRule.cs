using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-002: Classes concretas devem ter apenas construtores privados.
/// A criação de instâncias deve ser feita via factory methods estáticos.
/// Exceções: classes abstratas, estáticas, records, enums, interfaces, structs.
/// </summary>
public sealed class DE002_PrivateConstructorRule : Rule
{
    // Properties
    public override string Name => "DE002_PrivateConstructor";
    public override string Description => "Classes concretas devem ter apenas construtores privados (DE-002)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/domain-entities/DE-002-construtores-privados-com-factory-methods.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Ignorar: abstratos, estáticos, records, enums, interfaces, structs
        if (type.IsAbstract || type.IsStatic ||
            type.IsRecord || type.TypeKind != TypeKind.Class)
            return null;

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
