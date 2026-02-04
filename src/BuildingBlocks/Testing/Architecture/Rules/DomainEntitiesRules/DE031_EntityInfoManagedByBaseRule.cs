using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-031: Metadados de infraestrutura (<c>EntityInfo</c>) devem ser gerenciados
/// pela classe base (<c>EntityBase</c>), não pela entidade concreta.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Entidades NÃO devem declarar propriedades com nomes de infraestrutura
///         (Id, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy, Version, etc.)</item>
///   <item>Esses metadados vêm de EntityInfo gerenciado pela classe base</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Propriedades herdadas (definidas em EntityBase)</item>
///   <item>Propriedades estáticas</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE031_EntityInfoManagedByBaseRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE031_EntityInfoManagedByBase";

    public override string Description =>
        "Metadados de infraestrutura (Id, CreatedAt, Version, etc.) devem vir de EntityInfo, " +
        "não declarados na entidade (DE-031)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-031-entityinfo-gerenciado-pela-classe-base.md";

    /// <summary>
    /// Nomes de propriedades de infraestrutura que NÃO devem ser declarados
    /// diretamente na entidade (são gerenciados por EntityInfo via EntityBase).
    /// </summary>
    private static readonly HashSet<string> ForbiddenPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id",
        "CreatedAt",
        "CreatedBy",
        "ModifiedAt",
        "ModifiedBy",
        "LastChangedAt",
        "LastChangedBy",
        "Version",
        "EntityVersion"
    };

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Ignorar propriedades estáticas (metadados)
            if (property.IsStatic)
                continue;

            // Ignorar propriedades herdadas (definidas na base)
            if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, type))
                continue;

            // Ignorar overrides
            if (property.IsOverride)
                continue;

            // Verificar se o nome é de propriedade de infraestrutura
            if (ForbiddenPropertyNames.Contains(property.Name))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetPropertyLineNumber(property, context.LineNumber),
                    Message = $"A classe '{type.Name}' declara propriedade de infraestrutura " +
                              $"'{property.Name}' diretamente. Metadados como Id, CreatedAt, " +
                              $"Version devem vir de EntityInfo gerenciado pela classe base " +
                              $"(EntityBase). Acessar via 'EntityInfo.{property.Name}'",
                    LlmHint = $"Remover a propriedade '{property.Name}' da classe '{type.Name}'. " +
                              $"Acessar via 'EntityInfo' herdado de EntityBase. " +
                              $"Consultar ADR DE-031 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Obtém o número da linha onde a propriedade é declarada.
    /// </summary>
    private static int GetPropertyLineNumber(IPropertySymbol property, int fallbackLineNumber)
    {
        var location = property.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
