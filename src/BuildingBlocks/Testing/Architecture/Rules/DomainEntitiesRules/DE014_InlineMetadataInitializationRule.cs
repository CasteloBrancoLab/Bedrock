using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-014: Metadados de entidades devem ser inicializados inline (não em construtores estáticos).
/// Inicialização inline mantém declaração e valor juntos, melhorando legibilidade e code review.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>A classe <c>{EntityName}Metadata</c> NÃO deve ter construtor estático (cctor)</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Entidades sem classe de metadados aninhada</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE014_InlineMetadataInitializationRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE014_InlineMetadataInitialization";

    public override string Description =>
        "Metadados devem ser inicializados inline, não em construtores estáticos (DE-014)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-014-inicializacao-inline-de-metadados.md";

    /// <summary>
    /// Sufixo obrigatório da classe de metadados aninhada.
    /// </summary>
    private const string MetadataSuffix = "Metadata";

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

        // Verificar se a classe de metadados possui construtor estático EXPLÍCITO (cctor)
        // Ignorar construtores estáticos gerados implicitamente pelo compilador
        // (ex: quando há static readonly Lock _lockObject = new())
        foreach (var member in metadataClass.GetMembers())
        {
            if (member is IMethodSymbol { MethodKind: MethodKind.StaticConstructor, IsImplicitlyDeclared: false } cctor)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMemberLineNumber(cctor, context.LineNumber),
                    Message = $"A classe '{metadataClass.Name}' de '{type.Name}' possui construtor estático. " +
                              $"Metadados devem ser inicializados inline para manter declaração e valor juntos. " +
                              $"Remover o construtor estático e mover inicializações para inline",
                    LlmHint = $"Remover o construtor estático de '{metadataClass.Name}' e mover todas as " +
                              $"inicializações para inline (ex: public static int MaxLength {{ get; private set; }} = 255). " +
                              $"Valores derivados podem usar expressões inline " +
                              $"(ex: FullNameMaxLength = FirstNameMaxLength + LastNameMaxLength + 1). " +
                              $"Consultar ADR DE-014 para exemplos de uso correto"
                };
            }
        }

        return null;
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
