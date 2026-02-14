using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// Classe base abstrata para regras de infraestrutura que operam no nivel de tipo (Roslyn).
/// Diferente de <see cref="InfrastructureRuleBase"/> que opera no nivel de projeto (.csproj),
/// esta base permite inspecionar interfaces, classes, heranca e modificadores via Roslyn.
/// </summary>
public abstract class InfrastructureTypeRuleBase : Rule
{
    public override string Category => "Infrastructure";

    /// <summary>
    /// Verifica se um projeto e do tipo Infra.Data.{Tech} (ex: ShopDemo.Auth.Infra.Data.PostgreSql).
    /// </summary>
    protected static bool IsInfraDataTechProject(string projectName)
    {
        const string marker = ".Infra.Data.";
        var idx = projectName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return false;

        // Deve ter algo apos ".Infra.Data." (o nome da tecnologia)
        var afterMarker = projectName[(idx + marker.Length)..];
        return afterMarker.Length > 0;
    }

    /// <summary>
    /// Verifica se um tipo (interface ou classe) implementa transitivamente uma interface especifica,
    /// verificando pelo nome simples da interface.
    /// </summary>
    protected static bool ImplementsInterfaceTransitively(INamedTypeSymbol type, string interfaceSimpleName)
    {
        // Para interfaces, verificar AllInterfaces (interfaces herdadas transitivamente)
        foreach (var iface in type.AllInterfaces)
        {
            if (iface.Name == interfaceSimpleName)
                return true;
        }

        // Se o proprio tipo e a interface procurada
        if (type.TypeKind == TypeKind.Interface && type.Name == interfaceSimpleName)
            return true;

        // Para interfaces, verificar tambem as interfaces diretas e suas AllInterfaces
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
    /// Membros herdados nao contam.
    /// </summary>
    protected static bool IsMarkerInterface(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Interface)
            return false;

        // GetMembers() retorna membros declarados diretamente na interface
        // Filtrar membros implicitos (como .ctor)
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

            // Verificar nome generico original
            if (current.IsGenericType && current.ConstructedFrom.Name == baseClassSimpleName)
                return true;

            current = current.BaseType;
        }

        return false;
    }
}
