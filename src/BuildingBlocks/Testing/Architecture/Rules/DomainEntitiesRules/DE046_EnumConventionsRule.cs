using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-046: Enumeracoes de dominio devem seguir convencoes padronizadas.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Nome NAO deve ter sufixo "Enum" ou "Enumeration"</item>
///   <item>Tipo subjacente deve ser explicito (<c>byte</c> ou <c>short</c>)</item>
///   <item>Todos os membros devem ter valores explicitos</item>
///   <item>Primeiro valor deve ser >= 1 (zero reservado para "Unknown"/"NotSpecified")</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Tipos que nao sao enums</item>
///   <item>Membros com valor 0 cujo nome indica ausencia (ex: "None", "Unknown", "NotSpecified")</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE046_EnumConventionsRule : DomainEntitiesGeneralRuleBase
{
    // Properties
    public override string Name => "DE046_EnumConventions";

    public override string Description =>
        "Enumeracoes de dominio devem seguir convencoes de nomenclatura, tipo subjacente, valores explicitos e valor inicial (DE-046)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-046-convencoes-enumeracoes-dominio.md";

    /// <summary>
    /// Sufixos proibidos para nomes de enum.
    /// </summary>
    private static readonly string[] ForbiddenSuffixes = ["Enum", "Enumeration"];

    /// <summary>
    /// Nomes de membros que podem ter valor zero (indicam ausencia intencional).
    /// </summary>
    private static readonly string[] AllowedZeroNames =
        ["None", "Unknown", "NotSpecified", "Undefined", "Default"];

    /// <summary>
    /// Tipos subjacentes aceitos para enums (byte e short).
    /// </summary>
    private static readonly SpecialType[] AcceptedUnderlyingTypes =
        [SpecialType.System_Byte, SpecialType.System_Int16];

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // So analisa enums
        if (type.TypeKind != TypeKind.Enum)
            return null;

        // Regra 1: Nome nao deve ter sufixo proibido
        foreach (var suffix in ForbiddenSuffixes)
        {
            if (type.Name.EndsWith(suffix, StringComparison.Ordinal) &&
                type.Name.Length > suffix.Length)
            {
                return CreateViolation(
                    context,
                    $"O enum '{type.Name}' tem sufixo '{suffix}' no nome. " +
                    $"Nomes de enum devem ser simples e diretos, sem sufixo redundante " +
                    $"(ex: '{type.Name[..^suffix.Length]}' em vez de '{type.Name}')",
                    $"Remover o sufixo '{suffix}' do nome do enum. " +
                    $"Consultar ADR DE-046 para exemplos");
            }
        }

        // Regra 2: Tipo subjacente deve ser explicito (byte ou short, nao int)
        var underlyingType = type.EnumUnderlyingType;
        if (underlyingType is not null)
        {
            var isAccepted = false;
            foreach (var accepted in AcceptedUnderlyingTypes)
            {
                if (underlyingType.SpecialType == accepted)
                {
                    isAccepted = true;
                    break;
                }
            }

            if (!isAccepted)
            {
                return CreateViolation(
                    context,
                    $"O enum '{type.Name}' usa tipo subjacente '{underlyingType.ToDisplayString()}'. " +
                    $"Enums devem usar 'byte' (ate 255 valores) ou 'short' (quando byte nao e suficiente)",
                    $"Alterar o tipo subjacente do enum '{type.Name}' para 'byte' " +
                    $"(ex: 'public enum {type.Name} : byte'). " +
                    $"Consultar ADR DE-046 para detalhes");
            }
        }

        // Regra 3 e 4: Verificar membros
        foreach (var member in type.GetMembers())
        {
            if (member is not IFieldSymbol field)
                continue;

            // Ignorar membros especiais (value__)
            if (field.IsImplicitlyDeclared)
                continue;

            if (!field.HasConstantValue)
            {
                return CreateViolation(
                    context,
                    $"O membro '{field.Name}' do enum '{type.Name}' nao tem valor explicito. " +
                    $"Todos os membros de enum devem ter valores definidos explicitamente " +
                    $"para evitar quebra de compatibilidade",
                    $"Definir valor explicito para '{field.Name}' " +
                    $"(ex: '{field.Name} = <valor>'). " +
                    $"Consultar ADR DE-046 para exemplos");
            }

            // Regra 4: Primeiro valor >= 1 (zero reservado)
            var value = Convert.ToInt64(field.ConstantValue);
            if (value == 0)
            {
                var isAllowedZero = false;
                foreach (var name in AllowedZeroNames)
                {
                    if (string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        isAllowedZero = true;
                        break;
                    }
                }

                if (!isAllowedZero)
                {
                    return CreateViolation(
                        context,
                        $"O membro '{field.Name}' do enum '{type.Name}' tem valor 0. " +
                        $"Zero e reservado para valores que indicam ausencia " +
                        $"('None', 'Unknown', 'NotSpecified'). " +
                        $"Valores de negocio devem comecar em 1",
                        $"Alterar o valor de '{field.Name}' para >= 1, " +
                        $"ou renomear para um nome que indique ausencia. " +
                        $"Consultar ADR DE-046 para exemplos");
                }
            }
        }

        return null;
    }

    private Violation CreateViolation(TypeContext context, string message, string llmHint)
    {
        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = message,
            LlmHint = llmHint
        };
    }
}
