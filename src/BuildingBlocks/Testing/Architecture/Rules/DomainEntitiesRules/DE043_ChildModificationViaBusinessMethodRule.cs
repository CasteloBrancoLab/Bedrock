using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-043: Modificacao de entidade filha deve ser feita via metodo de negocio
/// da propria entidade filha, que segue o padrao Clone-Modify-Return.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Entidades filhas (tipo T usado em <c>List&lt;T&gt;</c> fields da Aggregate Root
///         onde T herda de EntityBase) devem ter pelo menos um metodo publico de instancia
///         que retorna <c>T?</c> (Clone-Modify-Return), alem de factory methods</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Entidades sem colecoes de entidades filhas</item>
///   <item>Factory methods (RegisterNew, CreateFromExistingInfo) — sao estaticos</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE043_ChildModificationViaBusinessMethodRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE043_ChildModificationViaBusinessMethod";

    public override string Description =>
        "Modificacao de entidade filha deve ser via metodo de negocio dela (DE-043)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-043-modificacao-entidade-filha-via-metodo-negocio.md";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Encontrar tipos de entidades filhas usadas em colecoes
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

            if (namedType.TypeArguments.Length == 0)
                continue;

            var childType = namedType.TypeArguments[0] as INamedTypeSymbol;
            if (childType is null || !InheritsFromEntityBase(childType))
                continue;

            // Verificar se a entidade filha tem metodos de negocio (Clone-Modify-Return)
            // que retornam T? (nullable do proprio tipo)
            if (!HasBusinessMethodReturningNullableSelf(childType))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = context.LineNumber,
                    Message = $"A entidade filha '{childType.Name}' usada na colecao " +
                              $"'{field.Name}' da classe '{type.Name}' nao possui " +
                              $"metodos publicos de instancia retornando '{childType.Name}?' " +
                              $"(Clone-Modify-Return). Modificacoes devem ser feitas " +
                              $"via metodos de negocio da propria entidade filha",
                    LlmHint = $"Adicionar metodos de negocio publicos a '{childType.Name}' " +
                              $"que retornem '{childType.Name}?' usando " +
                              $"RegisterChangeInternal. Consultar ADR DE-043 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o tipo tem pelo menos um metodo publico de instancia
    /// que retorna o proprio tipo com nullable (T?).
    /// </summary>
    private static bool HasBusinessMethodReturningNullableSelf(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Apenas metodos publicos de instancia
            if (method.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (method.IsStatic)
                continue;

            // Verificar se retorna T?
            if (ReturnsNullableOfContainingType(method, type))
                return true;
        }

        return false;
    }
}
