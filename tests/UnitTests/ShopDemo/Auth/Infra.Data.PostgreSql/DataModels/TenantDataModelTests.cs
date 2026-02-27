using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class TenantDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public TenantDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new TenantDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetName()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new TenantDataModel { Name = expectedValue };
        LogAssert("Verifying value");
        dataModel.Name.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetDomain()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new TenantDataModel { Domain = expectedValue };
        LogAssert("Verifying value");
        dataModel.Domain.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetSchemaName()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new TenantDataModel { SchemaName = expectedValue };
        LogAssert("Verifying value");
        dataModel.SchemaName.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatus()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new TenantDataModel { Status = expectedValue };
        LogAssert("Verifying value");
        dataModel.Status.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetTier()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new TenantDataModel { Tier = expectedValue };
        LogAssert("Verifying value");
        dataModel.Tier.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetDbVersion()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new TenantDataModel { DbVersion = expectedValue };
        LogAssert("Verifying value");
        dataModel.DbVersion.ShouldBe(expectedValue);
    }
}
