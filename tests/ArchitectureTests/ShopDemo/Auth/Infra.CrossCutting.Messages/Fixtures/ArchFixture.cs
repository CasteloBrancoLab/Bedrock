using Bedrock.BuildingBlocks.Testing.Architecture;
using Xunit;

namespace Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.CrossCutting.Messages.Fixtures;

/// <summary>
/// Fixture que carrega as compilacoes dos projetos Messages (BuildingBlocks + Auth)
/// para analise arquitetural.
/// </summary>
public sealed class ArchFixture : RuleFixture
{
    protected override IReadOnlyList<string> GetProjectPaths(string rootDir)
    {
        return
        [
            Path.Combine(rootDir, "src", "BuildingBlocks", "Messages", "Bedrock.BuildingBlocks.Messages.csproj"),
            Path.Combine(rootDir, "src", "ShopDemo", "Auth", "Infra.CrossCutting.Messages", "ShopDemo.Auth.Infra.CrossCutting.Messages.csproj")
        ];
    }
}

[CollectionDefinition("Arch")]
public sealed class ArchCollection : ICollectionFixture<ArchFixture>;
