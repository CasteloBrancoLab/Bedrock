using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Core.Entities.Users.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class UserFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public UserFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUsernameFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserDataModel with specific username");
        string expectedUsername = Faker.Internet.UserName();
        var dataModel = CreateTestDataModel(expectedUsername, "test@example.com", [1, 2, 3], (short)UserStatus.Active);

        // Act
        LogAct("Creating User from UserDataModel");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Username mapping");
        user.Username.ShouldBe(expectedUsername);
    }

    [Fact]
    public void Create_ShouldMapEmailFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserDataModel with specific email");
        string expectedEmail = Faker.Internet.Email();
        var dataModel = CreateTestDataModel("testuser", expectedEmail, [1, 2, 3], (short)UserStatus.Active);

        // Act
        LogAct("Creating User from UserDataModel");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Email mapping");
        user.Email.Value.ShouldBe(expectedEmail);
    }

    [Fact]
    public void Create_ShouldMapPasswordHashFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserDataModel with specific password hash");
        byte[] expectedHash = [10, 20, 30, 40, 50];
        var dataModel = CreateTestDataModel("testuser", "test@example.com", expectedHash, (short)UserStatus.Active);

        // Act
        LogAct("Creating User from UserDataModel");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying PasswordHash mapping");
        user.PasswordHash.Value.ToArray().ShouldBe(expectedHash);
    }

    [Theory]
    [InlineData((short)1, UserStatus.Active)]
    [InlineData((short)2, UserStatus.Suspended)]
    [InlineData((short)3, UserStatus.Blocked)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, UserStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating UserDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel("testuser", "test@example.com", [1, 2, 3], statusValue);

        // Act
        LogAct("Creating User from UserDataModel");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        user.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserDataModel with specific base fields");
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-5);
        long expectedVersion = Faker.Random.Long(1);
        string? expectedLastChangedBy = Faker.Person.FullName;
        var expectedLastChangedAt = DateTimeOffset.UtcNow;
        var expectedLastChangedCorrelationId = Guid.NewGuid();
        string expectedLastChangedExecutionOrigin = "TestOrigin";
        string expectedLastChangedBusinessOperationCode = "TEST_OP";

        var dataModel = new UserDataModel
        {
            Id = expectedId,
            TenantCode = expectedTenantCode,
            CreatedBy = expectedCreatedBy,
            CreatedAt = expectedCreatedAt,
            LastChangedBy = expectedLastChangedBy,
            LastChangedAt = expectedLastChangedAt,
            LastChangedCorrelationId = expectedLastChangedCorrelationId,
            LastChangedExecutionOrigin = expectedLastChangedExecutionOrigin,
            LastChangedBusinessOperationCode = expectedLastChangedBusinessOperationCode,
            EntityVersion = expectedVersion,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = [1, 2, 3],
            Status = (short)UserStatus.Active
        };

        // Act
        LogAct("Creating User from UserDataModel");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EntityInfo fields");
        user.EntityInfo.Id.Value.ShouldBe(expectedId);
        user.EntityInfo.TenantInfo.Code.ShouldBe(expectedTenantCode);
        user.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe(expectedCreatedBy);
        user.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(expectedCreatedAt);
        user.EntityInfo.EntityVersion.Value.ShouldBe(expectedVersion);
        user.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBe(expectedLastChangedBy);
        user.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(expectedLastChangedAt);
        user.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(expectedLastChangedCorrelationId);
        user.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe(expectedLastChangedExecutionOrigin);
        user.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe(expectedLastChangedBusinessOperationCode);
    }

    [Fact]
    public void Create_WithNullLastChangedFields_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating UserDataModel with null last-changed fields");
        var dataModel = new UserDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "creator",
            CreatedAt = DateTimeOffset.UtcNow,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = [1, 2, 3],
            Status = (short)UserStatus.Active
        };

        // Act
        LogAct("Creating User from UserDataModel with nulls");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying nullable fields are null");
        user.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        user.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        user.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBeNull();
        user.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        user.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapCreatedCorrelationIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel("testuser", "test@example.com", [1, 2, 3], (short)UserStatus.Active);

        // Act
        LogAct("Creating User from UserDataModel");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        user.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel("testuser", "test@example.com", [1, 2, 3], (short)UserStatus.Active);

        // Act
        LogAct("Creating User from UserDataModel");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        user.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating UserDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel("testuser", "test@example.com", [1, 2, 3], (short)UserStatus.Active);

        // Act
        LogAct("Creating User from UserDataModel");
        var user = UserFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        user.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static UserDataModel CreateTestDataModel(
        string username,
        string email,
        byte[] passwordHash,
        short status)
    {
        return new UserDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_USER",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Status = status
        };
    }

    #endregion
}
