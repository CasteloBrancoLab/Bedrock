using Bedrock.BuildingBlocks.Testing.Architecture;
using Xunit;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;

/// <summary>
/// Fixture que carrega a compilação do projeto Templates.Domain.Entities para análise arquitetural.
/// </summary>
public sealed class DomainEntitiesArchFixture
    : RuleFixture
{
    protected override IReadOnlyList<string> GetProjectPaths(string rootDir)
    {
        return
        [
            Path.Combine(rootDir, "src", "templates", "Domain.Entities", "Templates.Domain.Entities.csproj")
        ];
    }
}

[CollectionDefinition("DomainEntitiesArch")]
public sealed class DomainEntitiesArchCollection : ICollectionFixture<DomainEntitiesArchFixture>;
