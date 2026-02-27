using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class ImpersonationSessionDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public ImpersonationSessionDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new ImpersonationSessionDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetOperatorUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ImpersonationSessionDataModel { OperatorUserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.OperatorUserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetTargetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new ImpersonationSessionDataModel { TargetUserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.TargetUserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetExpiresAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new ImpersonationSessionDataModel { ExpiresAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.ExpiresAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatus()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new ImpersonationSessionDataModel { Status = expectedValue };
        LogAssert("Verifying value");
        dataModel.Status.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetEndedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new ImpersonationSessionDataModel { EndedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.EndedAt.ShouldBe(expectedValue);
    }
}
