using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.DataModels;

public class SigningKeyDataModelTests : TestBase
{
    private static readonly Faker Faker = new();
    public SigningKeyDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void ShouldInheritFromDataModelBase()
    {
        LogArrange("Creating instance");
        LogAct("Checking inheritance");
        var dataModel = new SigningKeyDataModel();
        LogAssert("Verifying inheritance");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldSetAndGetKid()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new SigningKeyDataModel { Kid = expectedValue };
        LogAssert("Verifying value");
        dataModel.Kid.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetAlgorithm()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new SigningKeyDataModel { Algorithm = expectedValue };
        LogAssert("Verifying value");
        dataModel.Algorithm.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetPublicKey()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new SigningKeyDataModel { PublicKey = expectedValue };
        LogAssert("Verifying value");
        dataModel.PublicKey.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetEncryptedPrivateKey()
    {
        LogArrange("Setting up value");
        var expectedValue = Faker.Lorem.Word();
        LogAct("Setting property");
        var dataModel = new SigningKeyDataModel { EncryptedPrivateKey = expectedValue };
        LogAssert("Verifying value");
        dataModel.EncryptedPrivateKey.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatus()
    {
        LogArrange("Setting up value");
        var expectedValue = (short)1;
        LogAct("Setting property");
        var dataModel = new SigningKeyDataModel { Status = expectedValue };
        LogAssert("Verifying value");
        dataModel.Status.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetRotatedAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new SigningKeyDataModel { RotatedAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.RotatedAt.ShouldBe(expectedValue);
    }

    [Fact]
    public void Properties_ShouldSetAndGetExpiresAt()
    {
        LogArrange("Setting up value");
        var expectedValue = DateTimeOffset.UtcNow;
        LogAct("Setting property");
        var dataModel = new SigningKeyDataModel { ExpiresAt = expectedValue };
        LogAssert("Verifying value");
        dataModel.ExpiresAt.ShouldBe(expectedValue);
    }
}
