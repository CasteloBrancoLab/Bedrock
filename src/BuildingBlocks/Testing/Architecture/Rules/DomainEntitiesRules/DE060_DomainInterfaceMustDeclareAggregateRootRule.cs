using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-060: Interfaces de domínio de entidades que são Aggregate Roots
/// devem herdar de <c>IAggregateRoot</c>, não apenas de <c>IEntity</c>.
/// <para>
/// A classificação DDD (Aggregate Root vs entidade interna) DEVE estar na
/// interface de domínio, não apenas na classe concreta. Isso previne assimetria
/// entre contrato e implementação e garante que repositórios e serviços de
/// domínio funcionem naturalmente com a interface.
/// </para>
/// </summary>
public sealed class DE060_DomainInterfaceMustDeclareAggregateRootRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE060_DomainInterfaceMustDeclareAggregateRoot";
    public override string Description =>
        "Interface de domínio de Aggregate Root deve herdar IAggregateRoot (DE-060)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-060-interface-dominio-deve-declarar-iaggregateroot.md";

    /// <summary>
    /// Nome da interface marker de Aggregate Root.
    /// </summary>
    private const string AggregateRootInterfaceName = "IAggregateRoot";

    /// <summary>
    /// Nome da interface base de entidade.
    /// </summary>
    private const string EntityInterfaceName = "IEntity";

    /// <summary>
    /// Nomes de interfaces de infraestrutura que não são consideradas
    /// "interfaces de domínio" para efeitos desta regra.
    /// </summary>
    private static readonly HashSet<string> InfrastructureInterfaceNames =
    [
        "IEntity",
        "IAggregateRoot"
    ];

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Regra aplica-se apenas a Aggregate Roots
        if (!ImplementsInterface(type, AggregateRootInterfaceName))
            return null;

        // Coletar interfaces de domínio implementadas pela classe
        var violatingInterface = FindViolatingDomainInterface(type);

        if (violatingInterface is null)
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Interface de domínio '{violatingInterface.Name}' implementada pelo " +
                      $"Aggregate Root '{type.Name}' herda de IEntity mas não de IAggregateRoot. " +
                      $"A interface de domínio deve declarar IAggregateRoot",
            LlmHint = $"Alterar a declaração de '{violatingInterface.Name}' para herdar de " +
                      $"IAggregateRoot em vez de IEntity: " +
                      $"'public interface {violatingInterface.Name} : IAggregateRoot'"
        };
    }

    /// <summary>
    /// Procura uma interface de domínio que herda de IEntity mas não de IAggregateRoot.
    /// Retorna a primeira interface violadora encontrada, ou null se todas estão corretas.
    /// </summary>
    private static INamedTypeSymbol? FindViolatingDomainInterface(INamedTypeSymbol type)
    {
        foreach (var iface in type.Interfaces)
        {
            // Ignorar interfaces de infraestrutura
            if (IsInfrastructureInterface(iface))
                continue;

            // Verificar se é uma interface de domínio (herda de IEntity)
            if (!InheritsFromInterface(iface, EntityInterfaceName))
                continue;

            // É uma interface de domínio. Verificar se herda de IAggregateRoot.
            if (!InheritsFromInterface(iface, AggregateRootInterfaceName))
                return iface;
        }

        return null;
    }

    /// <summary>
    /// Verifica se a interface é uma das interfaces de infraestrutura do framework.
    /// </summary>
    private static bool IsInfrastructureInterface(INamedTypeSymbol iface)
    {
        return InfrastructureInterfaceNames.Contains(iface.Name);
    }

    /// <summary>
    /// Verifica se o tipo implementa uma interface específica (direta ou indiretamente).
    /// </summary>
    private static bool ImplementsInterface(INamedTypeSymbol type, string interfaceName)
    {
        foreach (var iface in type.AllInterfaces)
        {
            if (iface.Name == interfaceName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Verifica se uma interface herda de outra interface específica (direta ou indiretamente).
    /// </summary>
    private static bool InheritsFromInterface(INamedTypeSymbol iface, string targetInterfaceName)
    {
        foreach (var inherited in iface.AllInterfaces)
        {
            if (inherited.Name == targetInterfaceName)
                return true;
        }

        return false;
    }
}
