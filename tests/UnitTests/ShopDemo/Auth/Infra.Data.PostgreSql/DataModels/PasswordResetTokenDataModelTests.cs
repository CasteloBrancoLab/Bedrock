using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class PasswordResetTokenDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public PasswordResetTokenDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new PasswordResetTokenDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new PasswordResetTokenDataModel { UserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetTokenHash()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new PasswordResetTokenDataModel { TokenHash = expectedValue };
        LogAssert("Verifying value");
        dataModel.TokenHash.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetExpiresAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new PasswordResetTokenDataModel { ExpiresAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.ExpiresAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIsUsed()
    {
        LogArrange("Setting up value");
        LogAct("Setting property");
        var dataModel = new PasswordResetTokenDataModel { IsUsed = true };
        LogAssert("Verifying value");
        dataModel.IsUsed.ShouldBe(true);
    }

    [Fact]
    public void Properties_ShouldSetAndGetUsedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new PasswordResetTokenDataModel { UsedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.UsedAt.ShouldBe(expectedValue);
    }
}
