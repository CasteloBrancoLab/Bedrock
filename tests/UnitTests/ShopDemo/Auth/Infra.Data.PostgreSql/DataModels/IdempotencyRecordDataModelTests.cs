using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class IdempotencyRecordDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public IdempotencyRecordDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new IdempotencyRecordDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetIdempotencyKey()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new IdempotencyRecordDataModel { IdempotencyKey = expectedValue };
        LogAssert("Verifying value");
        dataModel.IdempotencyKey.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRequestHash()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new IdempotencyRecordDataModel { RequestHash = expectedValue };
        LogAssert("Verifying value");
        dataModel.RequestHash.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetResponseBody()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new IdempotencyRecordDataModel { ResponseBody = expectedValue };
        LogAssert("Verifying value");
        dataModel.ResponseBody.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatusCode()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Random.Int(1, 100);
        LogAct("Setting property");
        var dataModel = new IdempotencyRecordDataModel { StatusCode = expectedValue };
        LogAssert("Verifying value");
        dataModel.StatusCode.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetExpiresAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new IdempotencyRecordDataModel { ExpiresAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.ExpiresAt.ShouldBe(expectedValue);
    }
}
