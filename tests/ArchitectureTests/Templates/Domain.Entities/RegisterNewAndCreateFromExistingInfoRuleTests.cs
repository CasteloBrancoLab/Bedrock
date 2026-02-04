using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-017: Entidades devem ter CreateFromExistingInfo para reconstitution sem validação.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class RegisterNewAndCreateFromExistingInfoRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public RegisterNewAndCreateFromExistingInfoRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_devem_ter_CreateFromExistingInfo_retornando_non_nullable()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE017_RegisterNewAndCreateFromExistingInfoRule());
    }
}
