using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-048: Metodos Validate* em classes abstratas devem ser publicos e estaticos.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ValidateMethodPublicInAbstractClassesRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ValidateMethodPublicInAbstractClassesRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Validate_devem_ser_publicos_e_estaticos_em_classes_abstratas()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE048_ValidateMethodPublicInAbstractClassesRule());
    }
}
