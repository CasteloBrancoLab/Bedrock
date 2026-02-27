using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class TokenExchangeDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public TokenExchangeDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new TokenExchangeDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetUserId()
    {
        LogArrange("Setting up value");
        var expectedValue = Guid.NewGuid();
        LogAct("Setting property");
        var dataModel = new TokenExchangeDataModel { UserId = expectedValue };
        LogAssert("Verifying value");
        dataModel.UserId.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetSubjectTokenJti()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new TokenExchangeDataModel { SubjectTokenJti = expectedValue };
        LogAssert("Verifying value");
        dataModel.SubjectTokenJti.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRequestedAudience()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new TokenExchangeDataModel { RequestedAudience = expectedValue };
        LogAssert("Verifying value");
        dataModel.RequestedAudience.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIssuedTokenJti()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new TokenExchangeDataModel { IssuedTokenJti = expectedValue };
        LogAssert("Verifying value");
        dataModel.IssuedTokenJti.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetIssuedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new TokenExchangeDataModel { IssuedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.IssuedAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetExpiresAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new TokenExchangeDataModel { ExpiresAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.ExpiresAt.ShouldBe(expectedValue);
    }
}
