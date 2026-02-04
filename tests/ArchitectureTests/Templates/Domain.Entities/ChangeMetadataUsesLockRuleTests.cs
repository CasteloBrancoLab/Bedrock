using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-015: Métodos Change*Metadata() devem usar lock para atomicidade.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ChangeMetadataUsesLockRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ChangeMetadataUsesLockRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metodos_ChangeMetadata_devem_usar_lock()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE015_ChangeMetadataUsesLockRule());
    }
}
