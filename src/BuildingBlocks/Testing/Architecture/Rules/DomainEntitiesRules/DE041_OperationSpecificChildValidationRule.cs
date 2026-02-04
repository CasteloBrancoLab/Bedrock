using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-041: Validacao de entidades filhas deve ser especifica por operacao,
/// seguindo o padrao <c>Validate[NomeDaEntidadeFilha]For[Operacao]Internal</c>.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Entidades que possuem metodos <c>Process*Internal</c> (DE-040) tambem
///         devem ter metodos <c>Validate*For*Internal</c> correspondentes</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Entidades sem colecoes de entidades filhas</item>
///   <item>Entidades sem metodos Process*Internal</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE041_OperationSpecificChildValidationRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE041_OperationSpecificChildValidation";

    public override string Description =>
        "Validacao de entidades filhas deve ser especifica por operacao via Validate*For*Internal (DE-041)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-041-validacao-entidade-filha-especifica-operacao.md";

    /// <summary>
    /// Prefixo esperado para metodos de validacao especifica.
    /// </summary>
    private const string ValidatePrefix = "Validate";

    /// <summary>
    /// Sufixo esperado.
    /// </summary>
    private const string InternalSuffix = "Internal";

    /// <summary>
    /// Palavra-chave que separa entidade da operacao.
    /// </summary>
    private const string ForKeyword = "For";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Verificar se a entidade tem colecoes de entidades filhas
        if (!HasChildEntityCollectionField(type))
            return null;

        // Verificar se tem metodos Process*Internal (se nao, DE-040 ja flagra)
        if (!HasProcessInternalMethods(type))
            return null;

        // Verificar se tem pelo menos um metodo Validate*For*Internal
        var hasValidateForMethod = false;
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            if (IsValidateForInternalMethod(method.Name))
            {
                hasValidateForMethod = true;
                break;
            }
        }

        if (!hasValidateForMethod)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe '{type.Name}' possui metodos Process*Internal " +
                          $"mas nao tem nenhum metodo de validacao especifica por operacao " +
                          $"(Validate*For*Internal). Cada operacao sobre entidades filhas " +
                          $"deve ter seu proprio metodo de validacao contextual",
                LlmHint = $"Adicionar metodos Validate*For*Internal para validar " +
                          $"entidades filhas no contexto de cada operacao. " +
                          $"Consultar ADR DE-041 para exemplos"
            };
        }

        return null;
    }

    /// <summary>
    /// Verifica se o nome do metodo segue o padrao Validate*For*Internal.
    /// </summary>
    private static bool IsValidateForInternalMethod(string methodName)
    {
        if (!methodName.StartsWith(ValidatePrefix, StringComparison.Ordinal))
            return false;

        if (!methodName.EndsWith(InternalSuffix, StringComparison.Ordinal))
            return false;

        // Verificar se contem "For" entre o prefixo e o sufixo
        var middle = methodName.Substring(
            ValidatePrefix.Length,
            methodName.Length - ValidatePrefix.Length - InternalSuffix.Length);

        return middle.Contains(ForKeyword, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifica se a entidade tem colecoes de entidades filhas (List&lt;T&gt; onde T herda EntityBase).
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
    /// Verifica se a entidade tem metodos Process*Internal.
    /// </summary>
    private static bool HasProcessInternalMethods(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            if (method.Name.StartsWith("Process", StringComparison.Ordinal) &&
                method.Name.EndsWith("Internal", StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
