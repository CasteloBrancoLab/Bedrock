using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class ClaimDependencyDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public ClaimDependencyDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new ClaimDependencyDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetClaimId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ClaimDependencyDataModel { ClaimId = expectedValue };
        LogAssert("Verifying value");
        dataModel.ClaimId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetDependsOnClaimId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ClaimDependencyDataModel { DependsOnClaimId = expectedValue };
        LogAssert("Verifying value");
        dataModel.DependsOnClaimId.ShouldBe(expectedValue);
    }
}
