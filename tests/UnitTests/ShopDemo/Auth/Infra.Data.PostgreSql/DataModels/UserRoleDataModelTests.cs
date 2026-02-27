using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class UserRoleDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public UserRoleDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new UserRoleDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new UserRoleDataModel { UserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRoleId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new UserRoleDataModel { RoleId = expectedValue };
        LogAssert("Verifying value");
        dataModel.RoleId.ShouldBe(expectedValue);
    }
}
