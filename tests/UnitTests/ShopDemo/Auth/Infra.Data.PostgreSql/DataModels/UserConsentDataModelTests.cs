using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class UserConsentDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public UserConsentDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new UserConsentDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new UserConsentDataModel { UserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetConsentTermId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new UserConsentDataModel { ConsentTermId = expectedValue };
        LogAssert("Verifying value");
        dataModel.ConsentTermId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetAcceptedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new UserConsentDataModel { AcceptedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.AcceptedAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatus()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new UserConsentDataModel { Status = expectedValue };
        LogAssert("Verifying value");
        dataModel.Status.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRevokedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new UserConsentDataModel { RevokedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.RevokedAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIpAddress()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new UserConsentDataModel { IpAddress = expectedValue };
        LogAssert("Verifying value");
        dataModel.IpAddress.ShouldBe(expectedValue);
    }
}
