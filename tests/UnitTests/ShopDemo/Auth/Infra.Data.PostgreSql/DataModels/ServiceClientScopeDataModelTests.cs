using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class ServiceClientScopeDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public ServiceClientScopeDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new ServiceClientScopeDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetServiceClientId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ServiceClientScopeDataModel { ServiceClientId = expectedValue };
        LogAssert("Verifying value");
        dataModel.ServiceClientId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetScope()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ServiceClientScopeDataModel { Scope = expectedValue };
        LogAssert("Verifying value");
        dataModel.Scope.ShouldBe(expectedValue);
    }
}
