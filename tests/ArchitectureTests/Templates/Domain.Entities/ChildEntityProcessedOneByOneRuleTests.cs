using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-040: Entidades filhas devem ser processadas uma a uma via Process*Internal.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ChildEntityProcessedOneByOneRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ChildEntityProcessedOneByOneRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_filhas_devem_ser_processadas_uma_a_uma()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE040_ChildEntityProcessedOneByOneRule());
    }
}
