using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-033: Entidades de dominio nao devem ser readonly struct ou record struct.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class NotReadonlyStructRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public NotReadonlyStructRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_nao_devem_ser_readonly_struct()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE033_NotReadonlyStructRule());
    }
}
