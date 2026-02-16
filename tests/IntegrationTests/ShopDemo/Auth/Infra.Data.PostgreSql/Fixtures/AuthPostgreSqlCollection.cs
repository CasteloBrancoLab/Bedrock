using Xunit;

namespace ShopDemo.IntegrationTests.Auth.Infra.Data.PostgreSql.Fixtures;

[CollectionDefinition("AuthPostgreSql")]
public class AuthPostgreSqlCollection : ICollectionFixture<AuthPostgreSqlFixture>
{
}
