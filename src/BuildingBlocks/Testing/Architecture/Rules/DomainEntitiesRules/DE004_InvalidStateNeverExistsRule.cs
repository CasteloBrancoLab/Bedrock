using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-004: Estado inválido nunca existe na memória.
/// Classes concretas que herdam de EntityBase devem ter um factory method estático
/// <c>RegisterNew</c> que retorna <c>T?</c> (nullable do tipo da entidade).
/// Exceções: classes abstratas, estáticas, records, enums, interfaces, structs.
/// </summary>
public sealed class DE004_InvalidStateNeverExistsRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE004_InvalidStateNeverExists";
    public override string Description => "Entidades concretas devem ter factory method RegisterNew retornando T? (DE-004)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/domain-entities/DE-004-estado-invalido-nunca-existe-na-memoria.md";

    /// <summary>
    /// Nome do factory method obrigatório.
    /// </summary>
    private const string RegisterNewMethodName = "RegisterNew";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Procurar método estático público RegisterNew
        var registerNewMethod = FindRegisterNewMethod(type);

        if (registerNewMethod is null)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"Classe '{type.Name}' herda de EntityBase mas não possui factory method estático 'RegisterNew'. Estado inválido nunca deve existir na memória",
                LlmHint = $"Adicionar factory method 'public static {type.Name}? RegisterNew(ExecutionContext executionContext, RegisterNewInput input)' na classe '{type.Name}' que valida todos os campos e retorna null se alguma validação falhar"
            };
        }

        // Verificar se o retorno é T? (nullable do tipo da entidade)
        if (!ReturnsNullableOfContainingType(registerNewMethod, type))
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"Factory method 'RegisterNew' da classe '{type.Name}' deve retornar '{type.Name}?' (nullable) ao invés de '{registerNewMethod.ReturnType.ToDisplayString()}'. Retorno nullable garante que estado inválido nunca existe",
                LlmHint = $"Alterar o retorno do factory method 'RegisterNew' da classe '{type.Name}' para '{type.Name}?' e retornar null quando a validação falhar"
            };
        }

        return null;
    }

    /// <summary>
    /// Procura o método estático público RegisterNew no tipo.
    /// </summary>
    private static IMethodSymbol? FindRegisterNewMethod(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.Name == RegisterNewMethodName &&
                method.IsStatic &&
                method.DeclaredAccessibility == Accessibility.Public &&
                method.MethodKind == MethodKind.Ordinary)
            {
                return method;
            }
        }

        return null;
    }
}
