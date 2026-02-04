using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-054: Hierarquia de heranca nao deve exceder 2 niveis alem de EntityBase.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class MaxInheritanceDepthRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public MaxInheritanceDepthRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Hierarquia_de_heranca_nao_deve_ser_profunda()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE054_MaxInheritanceDepthRule());
    }
}
