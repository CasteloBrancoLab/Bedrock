using Bedrock.BuildingBlocks.Testing.Architecture;
using Xunit;

namespace Bedrock.ArchitectureTests.BuildingBlocks.Serialization.Avro.Fixtures;

/// <summary>
/// Fixture que carrega a compilacao do projeto alvo para analise arquitetural.
/// </summary>
public sealed class ArchFixture : RuleFixture
{
    protected override IReadOnlyList<string> GetProjectPaths(string rootDir)
    {
        return [Path.Combine(rootDir, "src", "BuildingBlocks", "Serialization.Avro", "Bedrock.BuildingBlocks.Serialization.Avro.csproj")];
    }
}

[CollectionDefinition("Arch")]
public sealed class ArchCollection : ICollectionFixture<ArchFixture>;
