using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-032: O construtor completo (com parâmetros) deve receber <c>EntityInfo</c>
/// e passá-lo para a classe base, garantindo que optimistic locking via
/// <c>EntityVersion</c> seja gerenciado automaticamente.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>O construtor com parâmetros deve ter um parâmetro do tipo <c>EntityInfo</c></item>
///   <item>Isso garante que EntityVersion (optimistic locking) é gerenciado pela classe base</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Construtor sem parâmetros (usado por RegisterNew via entityFactory)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE032_OptimisticLockingViaEntityInfoRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE032_OptimisticLockingViaEntityInfo";

    public override string Description =>
        "Construtor com parâmetros deve receber EntityInfo para optimistic locking automático (DE-032)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-032-optimistic-locking-com-entityversion.md";

    /// <summary>
    /// Nome do tipo EntityInfo.
    /// </summary>
    private const string EntityInfoTypeName = "EntityInfo";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Constructor)
                continue;

            // Ignorar construtor sem parâmetros (usado por entityFactory no RegisterNew)
            if (method.Parameters.Length == 0)
                continue;

            // Ignorar construtores gerados pelo compilador
            if (method.IsImplicitlyDeclared)
                continue;

            // Verificar se tem parâmetro EntityInfo
            var hasEntityInfoParam = false;
            foreach (var param in method.Parameters)
            {
                if (param.Type.Name == EntityInfoTypeName)
                {
                    hasEntityInfoParam = true;
                    break;
                }
            }

            if (!hasEntityInfoParam)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O construtor com parâmetros da classe '{type.Name}' não recebe " +
                              $"EntityInfo. O construtor completo deve receber EntityInfo e " +
                              $"passá-lo para a classe base via ':base(entityInfo)' para garantir " +
                              $"optimistic locking automático via EntityVersion",
                    LlmHint = $"Adicionar parâmetro 'EntityInfo entityInfo' ao construtor da " +
                              $"classe '{type.Name}' e chamá-lo com ': base(entityInfo)'. " +
                              $"Consultar ADR DE-032 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Obtém o número da linha onde o método é declarado.
    /// </summary>
    private static int GetMethodLineNumber(IMethodSymbol method, int fallbackLineNumber)
    {
        var location = method.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
