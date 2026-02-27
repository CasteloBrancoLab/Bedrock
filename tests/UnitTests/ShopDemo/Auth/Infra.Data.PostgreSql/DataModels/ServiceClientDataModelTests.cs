using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class ServiceClientDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public ServiceClientDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new ServiceClientDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetClientId()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ServiceClientDataModel { ClientId = expectedValue };
        LogAssert("Verifying value");
        dataModel.ClientId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetClientSecretHash()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Random.Bytes(32);
        LogAct("Setting property");
        var dataModel = new ServiceClientDataModel { ClientSecretHash = expectedValue };
        LogAssert("Verifying value");
        dataModel.ClientSecretHash.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetName()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ServiceClientDataModel { Name = expectedValue };
        LogAssert("Verifying value");
        dataModel.Name.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatus()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new ServiceClientDataModel { Status = expectedValue };
        LogAssert("Verifying value");
        dataModel.Status.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetCreatedByUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ServiceClientDataModel { CreatedByUserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.CreatedByUserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetExpiresAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new ServiceClientDataModel { ExpiresAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.ExpiresAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRevokedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new ServiceClientDataModel { RevokedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.RevokedAt.ShouldBe(expectedValue);
    }
}
