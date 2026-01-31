using Microsoft.CodeAnalysis;
using Xunit;

namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Fixture base para testes de arquitetura.
/// Carrega compilações Roslyn dos projetos configurados via <see cref="GetProjectPaths"/>.
/// Garante que a compilação é feita uma única vez para todos os testes.
/// </summary>
/// <remarks>
/// Para usar, herde esta classe no projeto de teste e implemente <see cref="GetProjectPaths"/>
/// retornando os caminhos absolutos dos .csproj a serem analisados.
/// </remarks>
public abstract class RuleFixture : IAsyncLifetime
{
    private readonly Dictionary<string, Compilation> _compilations = [];

    /// <summary>
    /// Diretório raiz do repositório.
    /// </summary>
    public string RootDir { get; private set; } = string.Empty;

    /// <summary>
    /// Compilações dos projetos carregados.
    /// </summary>
    public IReadOnlyDictionary<string, Compilation> Compilations => _compilations;

    /// <summary>
    /// Gerenciador de violações arquiteturais.
    /// </summary>
    public ViolationManager Manager { get; } = new();

    /// <summary>
    /// Retorna os caminhos absolutos dos .csproj que devem ser compilados para análise.
    /// </summary>
    /// <param name="rootDir">Diretório raiz do repositório (contém Bedrock.sln).</param>
    /// <returns>Lista de caminhos absolutos de .csproj.</returns>
    protected abstract IReadOnlyList<string> GetProjectPaths(string rootDir);

    public async Task InitializeAsync()
    {
        // Descobrir o root dir navegando para cima até encontrar Bedrock.sln
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Bedrock.sln")))
            dir = Path.GetDirectoryName(dir);

        RootDir = dir ?? throw new InvalidOperationException(
            "Não foi possível encontrar o diretório raiz do repositório (Bedrock.sln).");

        WorkspaceFactory.EnsureMSBuildRegistered();

        var projectPaths = GetProjectPaths(RootDir);

        foreach (var csproj in projectPaths)
        {
            var projectName = Path.GetFileNameWithoutExtension(csproj);
            Console.WriteLine($"  Compilando via Roslyn: {projectName}");

            try
            {
                var compilation = await WorkspaceFactory.GetCompilationAsync(csproj);
                _compilations[projectName] = compilation;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  AVISO: Falha ao compilar {projectName}: {ex.Message}");
            }
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
