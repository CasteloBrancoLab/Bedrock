using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-013: Membros de classes de metadados devem seguir a convenção
/// <c>{PropertyName}{ConstraintType}</c>.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Classe aninhada <c>{EntityName}Metadata</c> deve existir</item>
///   <item>Membros públicos (exceto métodos <c>Change*Metadata</c>) devem seguir o padrão
///         <c>{PropertyName}{ConstraintType}</c>, onde <c>PropertyName</c> corresponde a uma
///         propriedade existente na entidade</item>
/// </list>
/// </para>
/// <para>
/// Tipos de constraints suportados:
/// <list type="bullet">
///   <item><c>PropertyName</c> — Nome da propriedade via <c>nameof()</c></item>
///   <item><c>IsRequired</c>, <c>IsUnique</c>, <c>IsReadOnly</c> — Booleanos</item>
///   <item><c>MinLength</c>, <c>MaxLength</c> — Comprimento (strings)</item>
///   <item><c>MinAgeInYears</c>, <c>MaxAgeInYears</c>, <c>MinAgeInDays</c>, <c>MaxAgeInDays</c> — Idade/Tempo</item>
///   <item><c>MinValue</c>, <c>MaxValue</c> — Valores numéricos</item>
///   <item><c>Pattern</c>, <c>Format</c> — Formato (strings)</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Membros privados da classe de metadados (ex: <c>_lockObject</c>)</item>
///   <item>Métodos <c>Change*Metadata</c> (métodos de customização no startup)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE013_MetadataNamingConventionRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE013_MetadataNamingConvention";

    public override string Description =>
        "Membros de metadados devem seguir a convenção {PropertyName}{ConstraintType} (DE-013)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-013-nomenclatura-de-metadados.md";

    /// <summary>
    /// Sufixo obrigatório da classe de metadados aninhada.
    /// </summary>
    private const string MetadataSuffix = "Metadata";

    /// <summary>
    /// Prefixo de métodos de customização de metadados (Change*Metadata).
    /// </summary>
    private const string ChangeMethodPrefix = "Change";

    /// <summary>
    /// Sufixo de métodos de customização de metadados (Change*Metadata).
    /// </summary>
    private const string ChangeMethodSuffix = "Metadata";

    /// <summary>
    /// Suffixes de constraint types suportados, ordenados do mais longo para o mais curto
    /// para evitar match parcial (ex: "MinAgeInYears" antes de "Min").
    /// </summary>
    private static readonly string[] SupportedConstraintSuffixes =
    [
        // Referência
        "PropertyName",

        // Idade/Tempo (antes de Value para não conflitar)
        "MinAgeInYears",
        "MaxAgeInYears",
        "MinAgeInDays",
        "MaxAgeInDays",

        // Comprimento
        "MinLength",
        "MaxLength",

        // Valores numéricos
        "MinValue",
        "MaxValue",

        // Booleanos
        "IsRequired",
        "IsUnique",
        "IsReadOnly",

        // Formato
        "Pattern",
        "Format"
    ];

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Buscar a classe de metadados aninhada: {EntityName}Metadata
        var expectedMetadataName = type.Name + MetadataSuffix;
        INamedTypeSymbol? metadataClass = null;

        foreach (var member in type.GetTypeMembers())
        {
            if (member.Name == expectedMetadataName && member.IsStatic)
            {
                metadataClass = member;
                break;
            }
        }

        // Se não há classe de metadados, não há o que verificar nesta regra
        if (metadataClass is null)
            return null;

        // Coletar nomes de propriedades da entidade (incluindo herdadas)
        var entityPropertyNames = CollectEntityPropertyNames(type);

        // Verificar cada membro público da classe de metadados
        foreach (var member in metadataClass.GetMembers())
        {
            // Ignorar membros não-públicos (ex: _lockObject, backing fields)
            if (member.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Ignorar métodos Change*Metadata
            if (member is IMethodSymbol method && IsChangeMetadataMethod(method))
                continue;

            // Ignorar property accessors e construtores
            if (member is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
                continue;

            // Verificar apenas propriedades e campos públicos
            if (member is not IPropertySymbol and not IFieldSymbol)
                continue;

            var memberName = member.Name;

            // Verificar se o nome segue {PropertyName}{ConstraintType}
            var violation = ValidateMemberName(memberName, entityPropertyNames, type, metadataClass, member, context);
            if (violation is not null)
                return violation;
        }

        return null;
    }

    /// <summary>
    /// Coleta os nomes de todas as propriedades públicas da entidade (incluindo herdadas via base).
    /// </summary>
    private static HashSet<string> CollectEntityPropertyNames(INamedTypeSymbol type)
    {
        var propertyNames = new HashSet<string>(StringComparer.Ordinal);

        var current = type;
        while (current is not null)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol property && !property.IsStatic && !property.IsImplicitlyDeclared)
                {
                    propertyNames.Add(property.Name);
                }
            }

            current = current.BaseType;
        }

        return propertyNames;
    }

    /// <summary>
    /// Verifica se um método é um Change*Metadata (método de customização no startup).
    /// </summary>
    private static bool IsChangeMetadataMethod(IMethodSymbol method)
    {
        return method.Name.StartsWith(ChangeMethodPrefix, StringComparison.Ordinal) &&
               method.Name.EndsWith(ChangeMethodSuffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Valida se o nome do membro segue o formato {PropertyName}{ConstraintType}.
    /// </summary>
    private Violation? ValidateMemberName(
        string memberName,
        HashSet<string> entityPropertyNames,
        INamedTypeSymbol entityType,
        INamedTypeSymbol metadataClass,
        ISymbol member,
        TypeContext context)
    {
        // Tentar extrair PropertyName e ConstraintType do nome do membro
        foreach (var suffix in SupportedConstraintSuffixes)
        {
            if (!memberName.EndsWith(suffix, StringComparison.Ordinal))
                continue;

            var propertyName = memberName[..^suffix.Length];

            // Verificar se a propertyName corresponde a uma propriedade da entidade
            if (entityPropertyNames.Contains(propertyName))
                return null; // Nome válido

            // PropertyName não corresponde a nenhuma propriedade da entidade
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = GetMemberLineNumber(member, context.LineNumber),
                Message = $"O membro '{memberName}' de '{metadataClass.Name}' usa o sufixo '{suffix}' " +
                          $"mas o prefixo '{propertyName}' não corresponde a nenhuma propriedade de '{entityType.Name}'. " +
                          $"O formato obrigatório é {{PropertyName}}{{ConstraintType}}",
                LlmHint = $"Verificar se '{propertyName}' é o nome correto da propriedade em '{entityType.Name}'. " +
                          $"Se a propriedade não existe, o membro '{memberName}' pode estar com nome incorreto. " +
                          $"O formato obrigatório é {{PropertyName}}{{ConstraintType}} conforme ADR DE-013"
            };
        }

        // Nenhum sufixo de constraint reconhecido
        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = GetMemberLineNumber(member, context.LineNumber),
            Message = $"O membro '{memberName}' de '{metadataClass.Name}' não segue a convenção " +
                      $"{{PropertyName}}{{ConstraintType}}. Sufixos válidos: " +
                      $"{string.Join(", ", SupportedConstraintSuffixes)}",
            LlmHint = $"Renomear o membro '{memberName}' de '{metadataClass.Name}' para seguir " +
                      $"o padrão {{PropertyName}}{{ConstraintType}}. " +
                      $"Exemplo: FirstNameMaxLength, BirthDateIsRequired. " +
                      $"Consultar ADR DE-013 para a lista completa de constraint types suportados"
        };
    }

    /// <summary>
    /// Obtém o número da linha onde o membro é declarado, ou fallback para a linha da classe.
    /// </summary>
    private static int GetMemberLineNumber(ISymbol member, int fallbackLineNumber)
    {
        var location = member.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
