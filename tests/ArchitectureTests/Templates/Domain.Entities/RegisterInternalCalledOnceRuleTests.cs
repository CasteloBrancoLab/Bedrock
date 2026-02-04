using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-023: Register*Internal deve ser chamado no máximo uma vez por método público.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class RegisterInternalCalledOnceRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public RegisterInternalCalledOnceRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void RegisterInternal_deve_ser_chamado_uma_unica_vez()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE023_RegisterInternalCalledOnceRule());
    }
}
