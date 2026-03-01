using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.OutboxRules;

/// <summary>
/// Classe base abstrata para regras de arquitetura do Outbox.
/// Fornece helpers para inspecionar repositorios, writers e marker interfaces do outbox.
/// </summary>
public abstract class OutboxRuleBase : Rule
{
    public override string Category => "Outbox";

    protected const string OutboxRepositoryInterfaceName = "IOutboxRepository";
    protected const string OutboxReaderInterfaceName = "IOutboxReader";
    protected const string OutboxWriterInterfaceName = "IOutboxWriter";
    protected const string OutboxRepositoryBaseName = "OutboxPostgreSqlRepositoryBase";
    protected const string MessageOutboxWriterName = "MessageOutboxWriter";

    /// <summary>
    /// Verifica se o projeto pertence aos BuildingBlocks (nao e um BC).
    /// </summary>
    protected static bool IsBuildingBlockProject(string projectName)
        => projectName.StartsWith("Bedrock.BuildingBlocks.", StringComparison.Ordinal);

    /// <summary>
    /// Verifica se um tipo (interface ou classe) implementa transitivamente uma interface especifica,
    /// verificando pelo nome simples da interface.
    /// </summary>
    protected static bool ImplementsInterfaceTransitively(INamedTypeSymbol type, string interfaceSimpleName)
    {
        foreach (var iface in type.AllInterfaces)
        {
            if (iface.Name == interfaceSimpleName)
                return true;
        }

        if (type.TypeKind == TypeKind.Interface && type.Name == interfaceSimpleName)
            return true;

        if (type.TypeKind == TypeKind.Interface)
        {
            foreach (var directInterface in type.Interfaces)
            {
                if (directInterface.Name == interfaceSimpleName)
                    return true;

                if (ImplementsInterfaceTransitively(directInterface, interfaceSimpleName))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica se uma interface e um marker (corpo vazio — sem membros adicionais proprios).
    /// </summary>
    protected static bool IsMarkerInterface(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Interface)
            return false;

        var ownMembers = type.GetMembers()
            .Where(m => !m.IsImplicitlyDeclared)
            .ToList();

        return ownMembers.Count == 0;
    }

    /// <summary>
    /// Verifica se uma classe herda (direta ou indiretamente) de uma base class
    /// cujo nome simples corresponde ao fornecido.
    /// </summary>
    protected static bool InheritsFromBaseClass(INamedTypeSymbol type, string baseClassSimpleName)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.Name == baseClassSimpleName)
                return true;

            if (current.IsGenericType && current.ConstructedFrom.Name == baseClassSimpleName)
                return true;

            current = current.BaseType;
        }

        return false;
    }
}
