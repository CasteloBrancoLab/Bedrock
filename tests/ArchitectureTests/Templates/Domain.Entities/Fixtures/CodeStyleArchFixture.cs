using Bedrock.BuildingBlocks.Testing.Architecture;
using Xunit;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;

/// <summary>
/// Fixture que carrega a compilação dos projetos para análise de regras de code style.
/// Descobre automaticamente todos os .csproj de BuildingBlocks (exceto Testing),
/// Templates e ShopDemo.
/// </summary>
public sealed class CodeStyleArchFixture
    : RuleFixture
{
    private static readonly string _separator = Path.DirectorySeparatorChar.ToString();

    protected override IReadOnlyList<string> GetProjectPaths(string rootDir)
    {
        var searchDirs = new[]
        {
            Path.Combine(rootDir, "src", "BuildingBlocks"),
            Path.Combine(rootDir, "src", "Templates"),
            Path.Combine(rootDir, "src", "ShopDemo"),
        };

        return searchDirs
            .Where(Directory.Exists)
            .SelectMany(static dir => Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories))
            .Where(static path => !path.Contains($"{_separator}Testing{_separator}", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

[CollectionDefinition("CodeStyleArch")]
public sealed class CodeStyleArchCollection : ICollectionFixture<CodeStyleArchFixture>;
