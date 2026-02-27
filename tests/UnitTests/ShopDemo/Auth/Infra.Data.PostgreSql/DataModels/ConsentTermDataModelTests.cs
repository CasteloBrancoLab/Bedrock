using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class ConsentTermDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public ConsentTermDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new ConsentTermDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetType()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new ConsentTermDataModel { Type = expectedValue };
        LogAssert("Verifying value");
        dataModel.Type.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetVersion()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ConsentTermDataModel { Version = expectedValue };
        LogAssert("Verifying value");
        dataModel.Version.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetContent()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new ConsentTermDataModel { Content = expectedValue };
        LogAssert("Verifying value");
        dataModel.Content.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetPublishedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new ConsentTermDataModel { PublishedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.PublishedAt.ShouldBe(expectedValue);
    }
}
