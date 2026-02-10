using Bedrock.BuildingBlocks.Testing.Architecture;
using Xunit;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;

/// <summary>
/// Fixture que carrega a compilação dos projetos para análise de regras de code style.
/// Inclui BuildingBlocks e samples que possuem interfaces.
/// </summary>
public sealed class CodeStyleArchFixture
    : RuleFixture
{
    protected override IReadOnlyList<string> GetProjectPaths(string rootDir)
    {
        return
        [
            Path.Combine(rootDir, "src", "BuildingBlocks", "Security", "Bedrock.BuildingBlocks.Security.csproj"),
            Path.Combine(rootDir, "src", "BuildingBlocks", "Domain.Entities", "Bedrock.BuildingBlocks.Domain.Entities.csproj"),
            Path.Combine(rootDir, "src", "BuildingBlocks", "Domain", "Bedrock.BuildingBlocks.Domain.csproj"),
            Path.Combine(rootDir, "src", "BuildingBlocks", "Persistence.Abstractions", "Bedrock.BuildingBlocks.Persistence.Abstractions.csproj"),
            Path.Combine(rootDir, "samples", "ShopDemo", "Auth", "Domain.Entities", "ShopDemo.Auth.Domain.Entities.csproj"),
            Path.Combine(rootDir, "samples", "ShopDemo", "Auth", "Domain", "ShopDemo.Auth.Domain.csproj")
        ];
    }
}

[CollectionDefinition("CodeStyleArch")]
public sealed class CodeStyleArchCollection : ICollectionFixture<CodeStyleArchFixture>;
