using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-029: Entidades devem usar TimeProvider via ExecutionContext, não DateTime.Now.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class TimeProviderViaExecutionContextRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public TimeProviderViaExecutionContextRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Entidades_nao_devem_usar_DateTime_Now_diretamente()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE029_TimeProviderViaExecutionContextRule());
    }
}
