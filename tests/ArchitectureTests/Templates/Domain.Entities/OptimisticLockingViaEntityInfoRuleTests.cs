using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-032: Construtor com parametros deve receber EntityInfo para optimistic locking automatico.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class OptimisticLockingViaEntityInfoRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public OptimisticLockingViaEntityInfoRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Construtores_com_parametros_devem_receber_EntityInfo()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE032_OptimisticLockingViaEntityInfoRule());
    }
}
