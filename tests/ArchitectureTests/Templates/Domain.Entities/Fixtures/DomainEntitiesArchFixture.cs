using Bedrock.BuildingBlocks.Testing.Architecture;
using Xunit;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;

/// <summary>
/// Fixture que carrega a compilação dos projetos Domain.Entities para análise arquitetural.
/// Descobre automaticamente todos os .csproj que terminam com Domain.Entities em
/// BuildingBlocks, templates e samples.
/// </summary>
public sealed class DomainEntitiesArchFixture
    : RuleFixture
{
    protected override IReadOnlyList<string> GetProjectPaths(string rootDir)
    {
        var searchDirs = new[]
        {
            Path.Combine(rootDir, "src", "BuildingBlocks"),
            Path.Combine(rootDir, "src", "templates"),
            Path.Combine(rootDir, "samples"),
        };

        return searchDirs
            .Where(Directory.Exists)
            .SelectMany(static dir => Directory.GetFiles(dir, "*.Domain.Entities.csproj", SearchOption.AllDirectories))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

[CollectionDefinition("DomainEntitiesArch")]
public sealed class DomainEntitiesArchCollection : ICollectionFixture<DomainEntitiesArchFixture>;
