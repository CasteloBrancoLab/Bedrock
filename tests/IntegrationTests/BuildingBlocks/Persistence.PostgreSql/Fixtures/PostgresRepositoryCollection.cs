using Xunit;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;

/// <summary>
/// Collection definition for PostgreSQL repository integration tests.
/// All test classes using this collection will share the same fixture instance.
/// </summary>
[CollectionDefinition("PostgresRepository")]
public class PostgresRepositoryCollection : ICollectionFixture<PostgresRepositoryFixture>
{
    // This class has no code, and is never created.
    // Its purpose is to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
