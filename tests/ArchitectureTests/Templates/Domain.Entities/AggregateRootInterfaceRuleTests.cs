using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-005: Aggregate Roots devem implementar IAggregateRoot.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class AggregateRootInterfaceRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public AggregateRootInterfaceRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Aggregate_roots_devem_implementar_IAggregateRoot()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE005_AggregateRootInterfaceRule());
    }
}
