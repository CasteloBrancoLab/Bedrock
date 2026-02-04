using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-045: Metodos de validacao para operacoes de alteracao
/// (<c>Validate*ForChange*Internal</c>) devem receber um parametro <c>int</c>
/// (currentIndex) para ignorar a propria entidade na verificacao de duplicidade.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Metodos <c>Validate*ForChange*Internal</c> devem ter pelo menos um
///         parametro do tipo <c>int</c> (indice da entidade sendo alterada)</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Metodos <c>Validate*ForRegisterNew*Internal</c> (criacao, nao precisa de indice)</item>
///   <item>Entidades sem colecoes de entidades filhas</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE045_DuplicateValidationIgnoresSelfRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE045_DuplicateValidationIgnoresSelf";

    public override string Description =>
        "Validacao de duplicidade em operacoes de alteracao deve ignorar a propria entidade via indice (DE-045)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-045-validacao-duplicidade-ignora-propria-entidade.md";

    /// <summary>
    /// Palavra-chave que identifica operacoes de alteracao.
    /// </summary>
    private const string ChangeKeyword = "Change";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // So se aplica a entidades com colecoes de entidades filhas
        if (!HasChildEntityCollectionField(type))
            return null;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Apenas metodos Validate*For*Change*Internal
            if (!method.Name.StartsWith("Validate", StringComparison.Ordinal) ||
                !method.Name.EndsWith("Internal", StringComparison.Ordinal))
                continue;

            if (!method.Name.Contains(ChangeKeyword, StringComparison.Ordinal))
                continue;

            // Verificar se tem parametro int (currentIndex)
            var hasIntParam = false;
            foreach (var param in method.Parameters)
            {
                if (param.Type.SpecialType == SpecialType.System_Int32)
                {
                    hasIntParam = true;
                    break;
                }
            }

            if (!hasIntParam)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O metodo '{method.Name}' da classe '{type.Name}' e uma " +
                              $"validacao de operacao de alteracao mas nao recebe um " +
                              $"parametro 'int' (currentIndex). Em operacoes de alteracao, " +
                              $"a validacao de duplicidade deve ignorar a propria entidade " +
                              $"usando o indice do item sendo alterado",
                    LlmHint = $"Adicionar parametro 'int currentIndex' ao metodo " +
                              $"'{method.Name}' e usar 'if (i == currentIndex) continue;' " +
                              $"no loop de verificacao de duplicidade. " +
                              $"Consultar ADR DE-045 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se a entidade tem colecoes de entidades filhas.
    /// </summary>
    private static bool HasChildEntityCollectionField(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            if (field.IsStatic || field.IsImplicitlyDeclared)
                continue;

            if (field.Type is not INamedTypeSymbol namedType)
                continue;

            if (!namedType.IsGenericType || namedType.Name != "List")
                continue;

            if (namedType.TypeArguments.Length > 0 &&
                namedType.TypeArguments[0] is INamedTypeSymbol childType &&
                InheritsFromEntityBase(childType))
                return true;
        }

        return false;
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
