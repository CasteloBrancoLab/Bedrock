using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.DataModels;

/// <summary>
/// Test data model for integration tests extending DataModelBase.
/// Maps to the test_entities table in the PostgreSQL test database.
/// </summary>
public class TestEntityDataModel : DataModelBase
{
    /// <summary>
    /// Gets or sets the name of the test entity.
    /// </summary>
    public string Name { get; set; } = null!;
}
