using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-014: Metadados devem ser inicializados inline, não em construtores estáticos.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class InlineMetadataInitializationRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public InlineMetadataInitializationRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metadados_devem_ser_inicializados_inline()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE014_InlineMetadataInitializationRule());
    }
}
