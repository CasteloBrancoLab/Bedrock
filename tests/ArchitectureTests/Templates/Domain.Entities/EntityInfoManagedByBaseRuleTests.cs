using Bedrock.ArchitectureTests.Templates.Domain.Entities.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture;
using Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.Templates.Domain.Entities;

/// <summary>
/// DE-031: EntityInfo (Id, CreatedAt, Version, etc.) deve ser gerenciado pela classe base.
/// </summary>
[Collection("DomainEntitiesArch")]
public sealed class EntityInfoManagedByBaseRuleTests : RuleTestBase<DomainEntitiesArchFixture>
{
    public EntityInfoManagedByBaseRuleTests(DomainEntitiesArchFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public void Metadados_de_infraestrutura_devem_vir_de_EntityInfo()
    {
        // Arrange / Act / Assert (via AssertNoViolations)
        AssertNoViolations(new DE031_EntityInfoManagedByBaseRule());
    }
}
