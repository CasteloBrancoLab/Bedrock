using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-011: Parâmetros de valor em métodos Validate* devem ser nullable por design.
/// A obrigatoriedade é decidida em runtime via metadata, não em compile-time.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ValidateParametersNullableRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ValidateParametersNullableRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Parametros_validate_devem_ser_nullable()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE011_ValidateParametersNullableRule());
    }
}
