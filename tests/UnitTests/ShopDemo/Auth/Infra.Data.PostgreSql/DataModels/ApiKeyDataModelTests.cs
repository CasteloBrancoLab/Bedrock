using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class ApiKeyDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public ApiKeyDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new ApiKeyDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetServiceClientId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ApiKeyDataModel { ServiceClientId = expectedValue };
        LogAssert("Verifying value");
        dataModel.ServiceClientId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetKeyPrefix()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ApiKeyDataModel { KeyPrefix = expectedValue };
        LogAssert("Verifying value");
        dataModel.KeyPrefix.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetKeyHash()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ApiKeyDataModel { KeyHash = expectedValue };
        LogAssert("Verifying value");
        dataModel.KeyHash.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatus()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new ApiKeyDataModel { Status = expectedValue };
        LogAssert("Verifying value");
        dataModel.Status.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetExpiresAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new ApiKeyDataModel { ExpiresAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.ExpiresAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetLastUsedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new ApiKeyDataModel { LastUsedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.LastUsedAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRevokedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new ApiKeyDataModel { RevokedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.RevokedAt.ShouldBe(expectedValue);
    }
}
