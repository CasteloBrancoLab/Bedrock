using Xunit;

namespace Bedrock.IntegrationTests.BuildingBlocks.Outbox.PostgreSql.Fixtures;

[CollectionDefinition("OutboxPostgreSql")]
public class OutboxPostgreSqlCollection : ICollectionFixture<OutboxPostgreSqlFixture>;
