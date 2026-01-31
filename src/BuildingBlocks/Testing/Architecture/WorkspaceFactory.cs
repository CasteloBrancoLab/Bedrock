using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Factory para criação de MSBuildWorkspace com registro do MSBuild.
/// Garante que o MSBuildLocator é registrado apenas uma vez.
/// </summary>
public static class WorkspaceFactory
{
    private static readonly object Lock = new();
    private static bool _registered;

    /// <summary>
    /// Registra a instância do MSBuild se ainda não foi registrada.
    /// Deve ser chamado antes de criar qualquer workspace.
    /// </summary>
    public static void EnsureMSBuildRegistered()
    {
        if (_registered)
            return;

        lock (Lock)
        {
            if (_registered)
                return;

            MSBuildLocator.RegisterDefaults();
            _registered = true;
        }
    }

    /// <summary>
    /// Abre um projeto .csproj e retorna sua compilação Roslyn.
    /// </summary>
    /// <param name="csprojPath">Caminho absoluto para o .csproj.</param>
    /// <returns>A compilação do projeto.</returns>
    public static async Task<Compilation> GetCompilationAsync(string csprojPath)
    {
        EnsureMSBuildRegistered();

        using var workspace = MSBuildWorkspace.Create();

        // Suprimir warnings do workspace (pacotes não encontrados etc.)
        workspace.RegisterWorkspaceFailedHandler(e =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                Console.WriteLine($"  Workspace warning: {e.Diagnostic.Message}");
        });

        var project = await workspace.OpenProjectAsync(csprojPath);
        var compilation = await project.GetCompilationAsync()
            ?? throw new InvalidOperationException(
                $"Não foi possível obter a compilação para '{csprojPath}'.");

        return compilation;
    }
}
