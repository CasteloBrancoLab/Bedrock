using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class SessionDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public SessionDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new SessionDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new SessionDataModel { UserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRefreshTokenId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new SessionDataModel { RefreshTokenId = expectedValue };
        LogAssert("Verifying value");
        dataModel.RefreshTokenId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetDeviceInfo()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new SessionDataModel { DeviceInfo = expectedValue };
        LogAssert("Verifying value");
        dataModel.DeviceInfo.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIpAddress()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new SessionDataModel { IpAddress = expectedValue };
        LogAssert("Verifying value");
        dataModel.IpAddress.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserAgent()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new SessionDataModel { UserAgent = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserAgent.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetExpiresAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new SessionDataModel { ExpiresAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.ExpiresAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatus()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new SessionDataModel { Status = expectedValue };
        LogAssert("Verifying value");
        dataModel.Status.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetLastActivityAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new SessionDataModel { LastActivityAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.LastActivityAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRevokedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new SessionDataModel { RevokedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.RevokedAt.ShouldBe(expectedValue);
    }
}
