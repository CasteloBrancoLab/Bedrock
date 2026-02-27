using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class ExternalLoginDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public ExternalLoginDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new ExternalLoginDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ExternalLoginDataModel { UserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetProvider()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ExternalLoginDataModel { Provider = expectedValue };
        LogAssert("Verifying value");
        dataModel.Provider.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetProviderUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ExternalLoginDataModel { ProviderUserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.ProviderUserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetEmail()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ExternalLoginDataModel { Email = expectedValue };
        LogAssert("Verifying value");
        dataModel.Email.ShouldBe(expectedValue);
    }
}
