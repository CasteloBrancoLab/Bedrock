using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-001: Classes concretas que não são herdadas por nenhuma outra classe
/// devem ser sealed. Exceções: classes abstratas e classes de teste.
/// </summary>
public sealed class DE001_SealedClassRule : Rule
{
    // Properties
    public override string Name => "DE001_SealedClass";
    public override string Description => "Classes concretas sem herdeiros devem ser sealed (DE-001)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/domain-entities/DE-001-entidades-devem-ser-sealed.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Ignorar: abstratos, sealed, estáticos, records, enums, interfaces, structs
        if (type.IsAbstract || type.IsSealed || type.IsStatic ||
            type.IsRecord || type.TypeKind != TypeKind.Class)
            return null;

        // Verificar se o tipo é herdado (cross-project via Roslyn ou via source grep)
        var typeFullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (context.GlobalInheritedTypes.Contains(typeFullName) ||
            context.GlobalInheritedTypes.Contains(type.Name))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Classe '{type.Name}' não tem herdeiros e deveria ser sealed",
            LlmHint = $"Adicionar modificador 'sealed' na declaração da classe '{type.Name}'"
        };
    }
}
