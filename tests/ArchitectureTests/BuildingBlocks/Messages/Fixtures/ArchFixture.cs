using Bedrock.BuildingBlocks.Testing.Architecture;
using Xunit;

namespace Bedrock.ArchitectureTests.BuildingBlocks.Messages.Fixtures;

/// <summary>
/// Fixture que carrega as compilacoes dos projetos Messages e Templates.Messages
/// para analise arquitetural.
/// </summary>
public sealed class ArchFixture : RuleFixture
{
    protected override IReadOnlyList<string> GetProjectPaths(string rootDir)
    {
        return
        [
            Path.Combine(rootDir, "src", "BuildingBlocks", "Messages", "Bedrock.BuildingBlocks.Messages.csproj"),
            Path.Combine(rootDir, "src", "Templates", "Infra.CrossCutting.Messages", "Templates.Infra.CrossCutting.Messages.csproj")
        ];
    }
}

[CollectionDefinition("Arch")]
public sealed class ArchCollection : ICollectionFixture<ArchFixture>;
