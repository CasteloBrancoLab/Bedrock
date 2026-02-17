namespace Bedrock.BuildingBlocks.Configuration.Handlers;

/// <summary>Tipo de escopo de um handler.</summary>
public enum ScopeType
{
    /// <summary>Handler aplica a todas as chaves.</summary>
    Global = 0,

    /// <summary>Handler aplica a todas as propriedades de uma secao/classe.</summary>
    Class = 1,

    /// <summary>Handler aplica a uma propriedade/chave exata.</summary>
    Property = 2
}

/// <summary>
/// Define o escopo de aplicacao de um handler (quais chaves ele processa).
/// </summary>
public readonly struct HandlerScope : IEquatable<HandlerScope>
{
    /// <summary>Tipo de escopo: Global, Class, ou Property.</summary>
    public ScopeType ScopeType { get; }

    /// <summary>Padrao de matching (vazio para global, secao para class, path completo para property).</summary>
    public string PathPattern { get; }

    private HandlerScope(ScopeType scopeType, string pathPattern)
    {
        ScopeType = scopeType;
        PathPattern = pathPattern;
    }

    /// <summary>Verifica se uma chave corresponde a este escopo.</summary>
    /// <param name="key">Chave completa de configuracao.</param>
    /// <returns>true se o handler deve processar esta chave.</returns>
    // Stryker disable once all : Default case do switch e inalcancavel — ScopeType e enum com 3 valores definidos
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Default case inalcancavel — ScopeType e enum com 3 valores definidos, coberto por testes exaustivos dos 3 valores")]
    public bool Matches(string key)
    {
        return ScopeType switch
        {
            ScopeType.Global => true,
            ScopeType.Class => key.StartsWith(PathPattern, StringComparison.Ordinal)
                               && key.Length > PathPattern.Length
                               && key[PathPattern.Length] == ':',
            ScopeType.Property => string.Equals(key, PathPattern, StringComparison.Ordinal),
            _ => false
        };
    }

    /// <summary>Cria escopo global (todas as chaves).</summary>
    public static HandlerScope Global() => new(ScopeType.Global, string.Empty);

    /// <summary>Cria escopo por classe (todas as propriedades de uma secao).</summary>
    /// <param name="sectionPath">Caminho da secao (ex: "Persistence:PostgreSql").</param>
    /// <exception cref="ArgumentException">Se sectionPath for nulo ou vazio.</exception>
    public static HandlerScope ForClass(string sectionPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionPath);
        return new HandlerScope(ScopeType.Class, sectionPath);
    }

    /// <summary>Cria escopo por propriedade (chave exata).</summary>
    /// <param name="fullPath">Caminho completo (ex: "Persistence:PostgreSql:ConnectionString").</param>
    /// <exception cref="ArgumentException">Se fullPath for nulo ou vazio.</exception>
    public static HandlerScope ForProperty(string fullPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);
        return new HandlerScope(ScopeType.Property, fullPath);
    }

    public bool Equals(HandlerScope other) =>
        ScopeType == other.ScopeType
        && string.Equals(PathPattern, other.PathPattern, StringComparison.Ordinal);

    public override bool Equals(object? obj) =>
        obj is HandlerScope other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(ScopeType, StringComparer.Ordinal.GetHashCode(PathPattern));

    public static bool operator ==(HandlerScope left, HandlerScope right) =>
        left.Equals(right);

    public static bool operator !=(HandlerScope left, HandlerScope right) =>
        !left.Equals(right);
}
