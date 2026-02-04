using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-004: Entidades concretas devem ter factory method RegisterNew retornando T?.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class InvalidStateNeverExistsRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public InvalidStateNeverExistsRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_concretas_devem_ter_factory_method_RegisterNew_retornando_nullable()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE004_InvalidStateNeverExistsRule());
    }
}
