using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-019: Factory methods devem receber Input Objects (readonly record struct).
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class InputObjectsPatternRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public InputObjectsPatternRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Factory_methods_devem_receber_input_objects()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE019_InputObjectsPatternRule());
    }
}
