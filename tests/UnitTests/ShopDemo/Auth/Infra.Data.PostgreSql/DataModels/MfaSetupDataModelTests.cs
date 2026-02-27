using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class MfaSetupDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public MfaSetupDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new MfaSetupDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new MfaSetupDataModel { UserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetEncryptedSharedSecret()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new MfaSetupDataModel { EncryptedSharedSecret = expectedValue };
        LogAssert("Verifying value");
        dataModel.EncryptedSharedSecret.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIsEnabled()
    {
        LogArrange("Setting up value");
        LogAct("Setting property");
        var dataModel = new MfaSetupDataModel { IsEnabled = true };
        LogAssert("Verifying value");
        dataModel.IsEnabled.ShouldBe(true);
    }

    [Fact]
    public void Properties_ShouldSetAndGetEnabledAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new MfaSetupDataModel { EnabledAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.EnabledAt.ShouldBe(expectedValue);
    }
}
