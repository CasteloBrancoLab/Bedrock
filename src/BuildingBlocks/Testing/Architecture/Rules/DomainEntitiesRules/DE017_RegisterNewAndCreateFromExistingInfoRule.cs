using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-017: Entidades devem ter separação entre <c>RegisterNew</c> (criação com validação)
/// e <c>CreateFromExistingInfo</c> (reconstitution sem validação).
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Entidades concretas devem ter método estático público <c>CreateFromExistingInfo</c></item>
///   <item><c>CreateFromExistingInfo</c> deve retornar o tipo da entidade (não nullable)</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>A existência e retorno de <c>RegisterNew</c> é verificada pela regra DE-004</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE017_RegisterNewAndCreateFromExistingInfoRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE017_RegisterNewAndCreateFromExistingInfo";

    public override string Description =>
        "Entidades devem ter CreateFromExistingInfo retornando T (non-nullable) para reconstitution (DE-017)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-017-separacao-registernew-vs-createfromexistinginfo.md";

    /// <summary>
    /// Nome do factory method de reconstitution obrigatório.
    /// </summary>
    private const string CreateFromExistingInfoMethodName = "CreateFromExistingInfo";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Procurar método estático público CreateFromExistingInfo
        var createFromExistingInfoMethod = FindCreateFromExistingInfoMethod(type);

        if (createFromExistingInfoMethod is null)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"Classe '{type.Name}' herda de EntityBase mas não possui factory method estático " +
                          $"'CreateFromExistingInfo'. Entidades devem ter separação entre RegisterNew " +
                          $"(criação com validação) e CreateFromExistingInfo (reconstitution sem validação)",
                LlmHint = $"Adicionar factory method 'public static {type.Name} CreateFromExistingInfo" +
                          $"(CreateFromExistingInfoInput input)' na classe '{type.Name}' que reconstitui " +
                          $"a entidade a partir de dados persistidos SEM validação. " +
                          $"Consultar ADR DE-017 para exemplos de uso correto"
            };
        }

        // Verificar se o retorno é T (non-nullable, tipo da entidade)
        if (!ReturnsNonNullableOfContainingType(createFromExistingInfoMethod, type))
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = GetMethodLineNumber(createFromExistingInfoMethod, context.LineNumber),
                Message = $"Factory method 'CreateFromExistingInfo' da classe '{type.Name}' deve retornar " +
                          $"'{type.Name}' (non-nullable) ao invés de " +
                          $"'{createFromExistingInfoMethod.ReturnType.ToDisplayString()}'. " +
                          $"CreateFromExistingInfo nunca falha pois reconstitui dados já validados",
                LlmHint = $"Alterar o retorno do factory method 'CreateFromExistingInfo' da classe '{type.Name}' " +
                          $"para '{type.Name}' (non-nullable). CreateFromExistingInfo NUNCA retorna null " +
                          $"pois dados já foram validados quando foram persistidos originalmente"
            };
        }

        return null;
    }

    /// <summary>
    /// Procura o método estático público CreateFromExistingInfo no tipo.
    /// </summary>
    private static IMethodSymbol? FindCreateFromExistingInfoMethod(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.Name == CreateFromExistingInfoMethodName &&
                method.IsStatic &&
                method.DeclaredAccessibility == Accessibility.Public &&
                method.MethodKind == MethodKind.Ordinary)
            {
                return method;
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o método retorna o tipo da entidade (non-nullable).
    /// </summary>
    private static bool ReturnsNonNullableOfContainingType(IMethodSymbol method, INamedTypeSymbol containingType)
    {
        var returnType = method.ReturnType;

        // O retorno deve ser o tipo exato da entidade, não nullable
        if (returnType.NullableAnnotation == NullableAnnotation.Annotated)
            return false;

        if (returnType is INamedTypeSymbol namedReturn)
        {
            return SymbolEqualityComparer.Default.Equals(namedReturn, containingType);
        }

        return false;
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
