using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-005: Classes concretas que herdam de EntityBase e cujo nome contém
/// "AggregateRoot" devem implementar a interface <c>IAggregateRoot</c>.
/// A interface marker garante type safety em tempo de compilação, permitindo
/// que repositórios e serviços exijam Aggregate Roots via constraints genéricos.
/// Exceções: classes abstratas, estáticas, records, enums, interfaces, structs.
/// </summary>
public sealed class DE005_AggregateRootInterfaceRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE005_AggregateRootInterface";
    public override string Description => "Aggregate Roots devem implementar IAggregateRoot (DE-005)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/domain-entities/DE-005-aggregateroot-deve-implementar-iaggregateroot.md";

    /// <summary>
    /// Nome da interface marker de Aggregate Root.
    /// </summary>
    private const string AggregateRootInterfaceName = "IAggregateRoot";

    /// <summary>
    /// Sufixo convencional para classes que são Aggregate Roots.
    /// </summary>
    private const string AggregateRootSuffix = "AggregateRoot";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Verificar se o nome indica que é um Aggregate Root
        if (!IsNamedAsAggregateRoot(type))
            return null;

        // Verificar se implementa IAggregateRoot
        if (ImplementsAggregateRootInterface(type))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Classe '{type.Name}' é um Aggregate Root mas não implementa IAggregateRoot. " +
                      $"A interface marker é necessária para type safety em repositórios e serviços de domínio",
            LlmHint = $"Adicionar implementação de IAggregateRoot na declaração da classe '{type.Name}': " +
                       $"'public sealed class {type.Name} : EntityBase<{type.Name}>, IAggregateRoot'"
        };
    }

    /// <summary>
    /// Verifica se o nome do tipo indica que é um Aggregate Root.
    /// Usa o sufixo convencional "AggregateRoot" para identificação.
    /// </summary>
    private static bool IsNamedAsAggregateRoot(INamedTypeSymbol type)
    {
        return type.Name.Contains(AggregateRootSuffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifica se o tipo implementa a interface IAggregateRoot (direta ou indiretamente).
    /// </summary>
    private static bool ImplementsAggregateRootInterface(INamedTypeSymbol type)
    {
        foreach (var iface in type.AllInterfaces)
        {
            if (iface.Name == AggregateRootInterfaceName)
                return true;
        }

        return false;
    }
}
