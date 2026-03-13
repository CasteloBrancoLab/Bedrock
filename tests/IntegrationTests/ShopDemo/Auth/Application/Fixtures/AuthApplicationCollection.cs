using Xunit;

namespace ShopDemo.IntegrationTests.Auth.Application.Fixtures;

[CollectionDefinition("AuthApplication")]
public class AuthApplicationCollection : ICollectionFixture<AuthApplicationFixture>;
