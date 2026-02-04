using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-042: Operacoes de modificacao em entidades filhas devem localiza-las
/// por Id (Guid). Metodos <c>Process*ForChange*Internal</c> ou similares devem
/// receber um parametro <c>Guid</c> para identificar a entidade filha.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Metodos <c>Process*Internal</c> que NAO sao <c>Process*ForRegisterNew*</c>
///         devem ter pelo menos um parametro do tipo <c>Guid</c> para localizar
///         a entidade filha</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Metodos <c>Process*ForRegisterNew*Internal</c> (criacao, nao precisa de Id)</item>
///   <item>Entidades sem metodos Process*Internal</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE042_ChildEntityLookupByIdRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE042_ChildEntityLookupById";

    public override string Description =>
        "Operacoes de modificacao em entidades filhas devem localiza-las por Id (DE-042)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-042-localizacao-entidade-filha-por-id.md";

    /// <summary>
    /// Substring que identifica metodos de criacao (nao precisam de Id).
    /// </summary>
    private const string RegisterNewKeyword = "RegisterNew";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Esta regra so se aplica a entidades com colecoes de entidades filhas (1:N)
        // Entidades com relacionamentos 1:1 (associated aggregate roots) nao precisam
        // de Guid para localizar — recebem a entidade diretamente
        if (!HasChildEntityCollectionField(type))
            return null;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Apenas metodos Process*Internal
            if (!method.Name.StartsWith("Process", StringComparison.Ordinal) ||
                !method.Name.EndsWith("Internal", StringComparison.Ordinal))
                continue;

            // Ignorar Process*ForRegisterNew*Internal (criacao nao precisa de Id)
            if (method.Name.Contains(RegisterNewKeyword, StringComparison.Ordinal))
                continue;

            // Verificar se tem parametro Guid
            var hasGuidParam = false;
            foreach (var param in method.Parameters)
            {
                if (param.Type.Name == "Guid")
                {
                    hasGuidParam = true;
                    break;
                }
            }

            if (!hasGuidParam)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O metodo '{method.Name}' da classe '{type.Name}' " +
                              $"e uma operacao de modificacao de entidade filha mas " +
                              $"nao recebe um parametro Guid para localizar a entidade. " +
                              $"Operacoes de modificacao devem localizar a entidade " +
                              $"filha por Id antes de modifica-la",
                    LlmHint = $"Adicionar parametro Guid ao metodo '{method.Name}' " +
                              $"para localizar a entidade filha por Id. " +
                              $"Consultar ADR DE-042 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se a entidade tem colecoes de entidades filhas (List&lt;T&gt; onde T herda EntityBase).
    /// Apenas entidades com colecoes (1:N) precisam de lookup por Id.
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
