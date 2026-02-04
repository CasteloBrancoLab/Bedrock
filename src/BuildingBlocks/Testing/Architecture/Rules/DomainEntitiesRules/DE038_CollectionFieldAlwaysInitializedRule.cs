using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-038: Fields de colecao devem ser sempre inicializados (nao nullable).
/// Usar <c>= []</c> para inicializar listas vazias, nunca deixar como null.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Fields de colecao (<c>List&lt;T&gt;</c>) NAO devem ser nullable</item>
///   <item>Fields de colecao devem ter inicializador (ex: <c>= []</c>)</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Fields estaticos</item>
///   <item>Fields gerados pelo compilador</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE038_CollectionFieldAlwaysInitializedRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE038_CollectionFieldAlwaysInitialized";

    public override string Description =>
        "Fields de colecao devem ser sempre inicializados como lista vazia, nunca nullable (DE-038)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-038-field-colecao-sempre-inicializado.md";

    /// <summary>
    /// Nomes de tipos de colecao que devem ser inicializados.
    /// </summary>
    private static readonly HashSet<string> CollectionTypeNames = new(StringComparer.Ordinal)
    {
        "List"
    };

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            // Ignorar estaticos
            if (field.IsStatic)
                continue;

            // Ignorar gerados pelo compilador
            if (field.IsImplicitlyDeclared)
                continue;

            // Verificar se e um tipo de colecao
            if (field.Type is not INamedTypeSymbol namedType ||
                !namedType.IsGenericType ||
                !CollectionTypeNames.Contains(namedType.Name))
                continue;

            // Verificar 1: field nullable (ex: List<T>?)
            if (field.Type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return CreateViolation(field, type, context,
                    $"O field '{field.Name}' da classe '{type.Name}' e uma colecao " +
                    $"nullable. Fields de colecao devem ser sempre inicializados " +
                    $"como lista vazia (= []), nunca nullable",
                    $"Remover o '?' de '{field.Name}' e inicializar com '= []'. " +
                    $"Consultar ADR DE-038 para exemplos");
            }

            // Verificar 2: field sem inicializador
            if (!HasFieldInitializer(field))
            {
                return CreateViolation(field, type, context,
                    $"O field '{field.Name}' da classe '{type.Name}' e uma colecao " +
                    $"sem inicializador. Fields de colecao devem ser inicializados " +
                    $"como lista vazia (= []) na declaracao",
                    $"Adicionar inicializador '= []' ao field '{field.Name}'. " +
                    $"Consultar ADR DE-038 para exemplos");
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o field possui inicializador na declaracao.
    /// </summary>
    private static bool HasFieldInitializer(IFieldSymbol field)
    {
        foreach (var syntaxRef in field.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            if (syntaxNode is VariableDeclaratorSyntax variableDeclarator)
            {
                // Se tem EqualsValueClause, tem inicializador
                if (variableDeclarator.Initializer is not null)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Cria uma violacao com informacoes do field.
    /// </summary>
    private Violation CreateViolation(
        IFieldSymbol field,
        INamedTypeSymbol type,
        TypeContext context,
        string message,
        string llmHint)
    {
        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = GetFieldLineNumber(field, context.LineNumber),
            Message = message,
            LlmHint = llmHint
        };
    }

    /// <summary>
    /// Obtem o numero da linha onde o field e declarado.
    /// </summary>
    private static int GetFieldLineNumber(IFieldSymbol field, int fallbackLineNumber)
    {
        var location = field.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
