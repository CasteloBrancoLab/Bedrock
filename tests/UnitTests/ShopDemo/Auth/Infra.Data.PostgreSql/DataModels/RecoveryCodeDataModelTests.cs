using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class RecoveryCodeDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public RecoveryCodeDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new RecoveryCodeDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new RecoveryCodeDataModel { UserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetCodeHash()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new RecoveryCodeDataModel { CodeHash = expectedValue };
        LogAssert("Verifying value");
        dataModel.CodeHash.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIsUsed()
    {
        LogArrange("Setting up value");
        LogAct("Setting property");
        var dataModel = new RecoveryCodeDataModel { IsUsed = true };
        LogAssert("Verifying value");
        dataModel.IsUsed.ShouldBe(true);
    }

    [Fact]
    public void Properties_ShouldSetAndGetUsedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new RecoveryCodeDataModel { UsedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.UsedAt.ShouldBe(expectedValue);
    }
}
