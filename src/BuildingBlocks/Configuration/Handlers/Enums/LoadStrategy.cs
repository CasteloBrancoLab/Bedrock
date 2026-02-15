namespace Bedrock.BuildingBlocks.Configuration.Handlers.Enums;

/// <summary>
/// Define quando um handler carrega ou atualiza seus dados.
/// </summary>
public enum LoadStrategy
{
    /// <summary>Executa uma vez durante Initialize(). Cache permanente. Falha = fail-fast.</summary>
    StartupOnly = 0,

    /// <summary>Executa no primeiro acesso. Cache permanente. Falha NAO e cacheada (retry).</summary>
    LazyStartupOnly = 1,

    /// <summary>Executa a cada acesso. Sem cache.</summary>
    AllTime = 2
}
