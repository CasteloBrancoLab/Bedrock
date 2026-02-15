namespace Bedrock.BuildingBlocks.Configuration;

/// <summary>
/// Encapsula um caminho de configuracao derivado de classe + propriedade.
/// </summary>
public readonly struct ConfigurationPath : IEquatable<ConfigurationPath>
{
    /// <summary>Secao de configuracao (ex: "Persistence:PostgreSql").</summary>
    public string Section { get; }

    /// <summary>Nome da propriedade (ex: "ConnectionString").</summary>
    public string Property { get; }

    /// <summary>Caminho completo (ex: "Persistence:PostgreSql:ConnectionString").</summary>
    public string FullPath { get; }

    private ConfigurationPath(string section, string property, string fullPath)
    {
        Section = section;
        Property = property;
        FullPath = fullPath;
    }

    /// <summary>Cria um ConfigurationPath a partir de secao e propriedade.</summary>
    /// <param name="section">Secao de configuracao (ex: "Persistence:PostgreSql").</param>
    /// <param name="property">Nome da propriedade (ex: "ConnectionString").</param>
    /// <returns>ConfigurationPath com caminho completo derivado.</returns>
    /// <exception cref="ArgumentException">Se section ou property forem nulos ou vazios.</exception>
    public static ConfigurationPath Create(string section, string property)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(section);
        ArgumentException.ThrowIfNullOrWhiteSpace(property);

        var fullPath = string.Create(section.Length + 1 + property.Length, (section, property), static (span, state) =>
        {
            state.section.AsSpan().CopyTo(span);
            span[state.section.Length] = ':';
            state.property.AsSpan().CopyTo(span[(state.section.Length + 1)..]);
        });

        return new ConfigurationPath(section, property, fullPath);
    }

    public bool Equals(ConfigurationPath other) =>
        string.Equals(FullPath, other.FullPath, StringComparison.Ordinal);

    public override bool Equals(object? obj) =>
        obj is ConfigurationPath other && Equals(other);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(FullPath);

    public static bool operator ==(ConfigurationPath left, ConfigurationPath right) =>
        left.Equals(right);

    public static bool operator !=(ConfigurationPath left, ConfigurationPath right) =>
        !left.Equals(right);

    public override string ToString() => FullPath;
}
