using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

[Collection("InfrastructureArch")]
public sealed class InfrastructureRuleTests : RuleTestBase<InfrastructureArchFixture>
{
    public InfrastructureRuleTests(InfrastructureArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void IN001_Camadas_de_bounded_context_devem_seguir_grafo_de_dependencias()
    {
        AssertNoViolations(new IN001_CanonicalLayerDependenciesRule());
    }
}
