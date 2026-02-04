using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-018: CreateFromExistingInfo NÃO deve chamar métodos Validate* nem IsValid.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ReconstitutionDoesNotValidateRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ReconstitutionDoesNotValidateRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void CreateFromExistingInfo_nao_deve_chamar_Validate()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE018_ReconstitutionDoesNotValidateRule());
    }
}
