using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Persistence.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Persistence.DataModels;

public class UserDataModelTests : TestBase
{
    private static readonly Faker Faker = new();

    public UserDataModelTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void UserDataModel_ShouldInheritFromDataModelBase()
    {
        // Arrange
        LogArrange("Creating UserDataModel instance");

        // Act
        LogAct("Checking inheritance");
        var dataModel = new UserDataModel();

        // Assert
        LogAssert("Verifying UserDataModel inherits from DataModelBase");
        dataModel.ShouldBeAssignableTo<DataModelBase>();
    }

    [Fact]
    public void Properties_ShouldHaveCorrectDefaultValues()
    {
        // Arrange
        LogArrange("Creating UserDataModel with default values");

        // Act
        LogAct("Instantiating UserDataModel");
        var dataModel = new UserDataModel();

        // Assert
        LogAssert("Verifying default property values");
        dataModel.Username.ShouldBeNull();
        dataModel.Email.ShouldBeNull();
        dataModel.PasswordHash.ShouldBeNull();
        dataModel.Status.ShouldBe((short)0);
    }

    [Fact]
    public void Properties_ShouldSetAndGetUsername()
    {
        // Arrange
        LogArrange("Creating UserDataModel with username");
        string expectedUsername = Faker.Internet.UserName();

        // Act
        LogAct("Setting Username property");
        var dataModel = new UserDataModel
        {
            Username = expectedUsername
        };

        // Assert
        LogAssert("Verifying Username was set correctly");
        dataModel.Username.ShouldBe(expectedUsername);
    }

    [Fact]
    public void Properties_ShouldSetAndGetEmail()
    {
        // Arrange
        LogArrange("Creating UserDataModel with email");
        string expectedEmail = Faker.Internet.Email();

        // Act
        LogAct("Setting Email property");
        var dataModel = new UserDataModel
        {
            Email = expectedEmail
        };

        // Assert
        LogAssert("Verifying Email was set correctly");
        dataModel.Email.ShouldBe(expectedEmail);
    }

    [Fact]
    public void Properties_ShouldSetAndGetPasswordHash()
    {
        // Arrange
        LogArrange("Creating UserDataModel with password hash");
        byte[] expectedHash = Faker.Random.Bytes(64);

        // Act
        LogAct("Setting PasswordHash property");
        var dataModel = new UserDataModel
        {
            PasswordHash = expectedHash
        };

        // Assert
        LogAssert("Verifying PasswordHash was set correctly");
        dataModel.PasswordHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Properties_ShouldSetAndGetStatus()
    {
        // Arrange
        LogArrange("Creating UserDataModel with status");
        short expectedStatus = 1;

        // Act
        LogAct("Setting Status property");
        var dataModel = new UserDataModel
        {
            Status = expectedStatus
        };

        // Assert
        LogAssert("Verifying Status was set correctly");
        dataModel.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Properties_ShouldSetAndGetAllPropertiesSimultaneously()
    {
        // Arrange
        LogArrange("Creating UserDataModel with all properties set");
        string expectedUsername = Faker.Internet.UserName();
        string expectedEmail = Faker.Internet.Email();
        byte[] expectedHash = Faker.Random.Bytes(64);
        short expectedStatus = 2;

        // Act
        LogAct("Setting all properties");
        var dataModel = new UserDataModel
        {
            Username = expectedUsername,
            Email = expectedEmail,
            PasswordHash = expectedHash,
            Status = expectedStatus
        };

        // Assert
        LogAssert("Verifying all properties were set correctly");
        dataModel.Username.ShouldBe(expectedUsername);
        dataModel.Email.ShouldBe(expectedEmail);
        dataModel.PasswordHash.ShouldBe(expectedHash);
        dataModel.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void BaseProperties_ShouldBeAccessible()
    {
        // Arrange
        LogArrange("Creating UserDataModel to test base properties");
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow;
        long expectedVersion = Faker.Random.Long(1);

        // Act
        LogAct("Setting base properties from DataModelBase");
        var dataModel = new UserDataModel
        {
            Id = expectedId,
            TenantCode = expectedTenantCode,
            CreatedBy = expectedCreatedBy,
            CreatedAt = expectedCreatedAt,
            EntityVersion = expectedVersion
        };

        // Assert
        LogAssert("Verifying base properties are accessible");
        dataModel.Id.ShouldBe(expectedId);
        dataModel.TenantCode.ShouldBe(expectedTenantCode);
        dataModel.CreatedBy.ShouldBe(expectedCreatedBy);
        dataModel.CreatedAt.ShouldBe(expectedCreatedAt);
        dataModel.EntityVersion.ShouldBe(expectedVersion);
    }
}
