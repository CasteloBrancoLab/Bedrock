using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class ServiceClientClaimDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public ServiceClientClaimDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new ServiceClientClaimDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetServiceClientId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ServiceClientClaimDataModel { ServiceClientId = expectedValue };
        LogAssert("Verifying value");
        dataModel.ServiceClientId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetClaimId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ServiceClientClaimDataModel { ClaimId = expectedValue };
        LogAssert("Verifying value");
        dataModel.ClaimId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetValue()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new ServiceClientClaimDataModel { Value = expectedValue };
        LogAssert("Verifying value");
        dataModel.Value.ShouldBe(expectedValue);
    }
}
