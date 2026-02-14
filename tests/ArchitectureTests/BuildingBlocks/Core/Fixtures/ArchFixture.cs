using Bedrock.BuildingBlocks.Testing.Architecture;
using Xunit;

namespace Bedrock.ArchitectureTests.BuildingBlocks.Core.Fixtures;

/// <summary>
/// Fixture que carrega a compilacao do projeto alvo para analise arquitetural.
/// </summary>
public sealed class ArchFixture : RuleFixture
{
    protected override IReadOnlyList<string> GetProjectPaths(string rootDir)
    {
        return [Path.Combine(rootDir, "src", "BuildingBlocks", "Core", "Bedrock.BuildingBlocks.Core.csproj")];
    }
}

[CollectionDefinition("Arch")]
public sealed class ArchCollection : ICollectionFixture<ArchFixture>;
