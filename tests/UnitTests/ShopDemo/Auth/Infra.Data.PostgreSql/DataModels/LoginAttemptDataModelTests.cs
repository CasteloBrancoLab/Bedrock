using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class LoginAttemptDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public LoginAttemptDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new LoginAttemptDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUsername()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new LoginAttemptDataModel { Username = expectedValue };
        LogAssert("Verifying value");
        dataModel.Username.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIpAddress()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new LoginAttemptDataModel { IpAddress = expectedValue };
        LogAssert("Verifying value");
        dataModel.IpAddress.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetAttemptedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new LoginAttemptDataModel { AttemptedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.AttemptedAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIsSuccessful()
    {
        LogArrange("Setting up value");
        LogAct("Setting property");
        var dataModel = new LoginAttemptDataModel { IsSuccessful = true };
        LogAssert("Verifying value");
        dataModel.IsSuccessful.ShouldBe(true);
    }

    [Fact]
    public void Properties_ShouldSetAndGetFailureReason()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new LoginAttemptDataModel { FailureReason = expectedValue };
        LogAssert("Verifying value");
        dataModel.FailureReason.ShouldBe(expectedValue);
    }
}
