using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-028: ExecutionContext deve ser o primeiro parâmetro em métodos que o recebem.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class ExecutionContextFirstParameterRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public ExecutionContextFirstParameterRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void ExecutionContext_deve_ser_primeiro_parametro()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE028_ExecutionContextFirstParameterRule());
    }
}
