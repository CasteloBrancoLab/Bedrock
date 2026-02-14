namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Contexto para analise de regras de nivel projeto (ProjectReferences).
/// Fornece informacoes sobre o projeto sendo analisado e suas dependencias.
/// </summary>
public sealed class ProjectRuleContext
{
    /// <summary>
    /// Nome do projeto (ex: ShopDemo.Auth.Application).
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    /// Caminho relativo do .csproj em relacao ao rootDir.
    /// </summary>
    public required string CsprojRelativePath { get; init; }

    /// <summary>
    /// Nomes dos projetos referenciados diretamente via ProjectReference.
    /// </summary>
    public required IReadOnlyList<string> DirectProjectReferences { get; init; }

    /// <summary>
    /// Diretorio raiz do repositorio.
    /// </summary>
    public required string RootDir { get; init; }

    /// <summary>
    /// Todos os nomes de projetos no escopo da fixture.
    /// </summary>
    public required IReadOnlySet<string> AllProjectNames { get; init; }

    /// <summary>
    /// Nomes dos pacotes NuGet referenciados diretamente via PackageReference.
    /// </summary>
    public IReadOnlyList<string> DirectPackageReferences { get; init; } = [];
}
