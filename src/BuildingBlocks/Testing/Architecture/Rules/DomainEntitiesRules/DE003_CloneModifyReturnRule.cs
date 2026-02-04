using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-003: Métodos públicos de instância que modificam estado devem seguir
/// o padrão Clone-Modify-Return, retornando T? (nullable do tipo da entidade).
/// Exceções: Clone(), métodos estáticos, métodos herdados de object, propriedades,
/// e métodos de validação (Validate*, IsValid).
/// </summary>
public sealed class DE003_CloneModifyReturnRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE003_CloneModifyReturn";
    public override string Description => "Métodos públicos de instância em entidades devem seguir o padrão Clone-Modify-Return retornando T? (DE-003)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/domain-entities/DE-003-imutabilidade-controlada-clone-modify-return.md";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Analisar cada método público de instância
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Ignorar: estáticos, não-públicos, propriedade accessors, construtores, operators
            if (method.IsStatic ||
                method.DeclaredAccessibility != Accessibility.Public ||
                method.MethodKind != MethodKind.Ordinary)
                continue;

            // Ignorar métodos herdados (definidos em tipo base, não overridden aqui)
            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, type) &&
                method.IsImplicitlyDeclared)
                continue;

            // Ignorar Clone() - é exceção documentada (retorna T, não T?)
            if (method.Name == "Clone")
                continue;

            // Ignorar métodos de validação (Validate*, IsValid) - retornam bool por design
            if (IsValidationMethod(method))
                continue;

            // Ignorar métodos herdados de object (ToString, Equals, GetHashCode, etc.)
            if (IsObjectMethod(method))
                continue;

            // Este é um método público de instância ordinário em uma entidade concreta.
            // Deve retornar T? (nullable do tipo da entidade) conforme Clone-Modify-Return.
            if (!ReturnsNullableOfContainingType(method, type))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = context.LineNumber,
                    Message = $"Método público '{method.Name}' da classe '{type.Name}' não segue o padrão Clone-Modify-Return. Deve retornar '{type.Name}?' ao invés de '{method.ReturnType.ToDisplayString()}'",
                    LlmHint = $"Alterar o método público '{method.Name}' da classe '{type.Name}' para seguir o padrão Clone-Modify-Return: retornar '{type.Name}?' e usar RegisterChangeInternal<{type.Name}, TInput>() para clonar, modificar e retornar nova instância"
                };
            }
        }

        return null;
    }
}
