using Xunit;

namespace ShopDemo.IntegrationTests.Auth.Domain.Fixtures;

[CollectionDefinition("AuthDomain")]
public class AuthDomainCollection : ICollectionFixture<AuthDomainFixture>
{
}
