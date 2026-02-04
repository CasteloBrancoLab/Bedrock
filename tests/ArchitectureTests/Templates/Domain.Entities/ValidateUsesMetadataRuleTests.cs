using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-016: Métodos Validate* devem referenciar metadados como Single Source of Truth.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ValidateUsesMetadataRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ValidateUsesMetadataRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_validate_devem_referenciar_metadata()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE016_ValidateUsesMetadataRule());
    }
}
