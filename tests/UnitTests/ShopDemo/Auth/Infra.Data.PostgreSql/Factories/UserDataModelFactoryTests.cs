using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using ShopDemo.Core.Entities.Users.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class UserDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public UserDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUsernameCorrectly()
    {
        // Arrange
        LogArrange("Creating User entity with known username");
        string expectedUsername = Faker.Internet.UserName();
        var user = CreateTestUser(expectedUsername, "test@example.com", [1, 2, 3], UserStatus.Active);

        // Act
        LogAct("Creating UserDataModel from User entity");
        var dataModel = UserDataModelFactory.Create(user);

        // Assert
        LogAssert("Verifying Username mapping");
        dataModel.Username.ShouldBe(expectedUsername);
    }

    [Fact]
    public void Create_ShouldMapEmailValueCorrectly()
    {
        // Arrange
        LogArrange("Creating User entity with known email");
        string expectedEmail = Faker.Internet.Email();
        var user = CreateTestUser("testuser", expectedEmail, [1, 2, 3], UserStatus.Active);

        // Act
        LogAct("Creating UserDataModel from User entity");
        var dataModel = UserDataModelFactory.Create(user);

        // Assert
        LogAssert("Verifying Email mapping");
        dataModel.Email.ShouldBe(expectedEmail);
    }

    [Fact]
    public void Create_ShouldMapPasswordHashValueCorrectly()
    {
        // Arrange
        LogArrange("Creating User entity with known password hash bytes");
        byte[] expectedHash = [1, 2, 3, 4, 5];
        var user = CreateTestUser("testuser", "test@example.com", expectedHash, UserStatus.Active);

        // Act
        LogAct("Creating UserDataModel from User entity");
        var dataModel = UserDataModelFactory.Create(user);

        // Assert
        LogAssert("Verifying PasswordHash mapping");
        dataModel.PasswordHash.ShouldBe(expectedHash);
    }

    [Theory]
    [InlineData(UserStatus.Active, 1)]
    [InlineData(UserStatus.Suspended, 2)]
    [InlineData(UserStatus.Blocked, 3)]
    public void Create_ShouldMapStatusAsShortCorrectly(UserStatus status, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating User entity with status {status}");
        var user = CreateTestUser("testuser", "test@example.com", [1, 2, 3], status);

        // Act
        LogAct("Creating UserDataModel from User entity");
        var dataModel = UserDataModelFactory.Create(user);

        // Assert
        LogAssert($"Verifying Status mapped to short value {expectedShortValue}");
        dataModel.Status.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating User entity with specific EntityInfo values");
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        string createdBy = Faker.Person.FullName;
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        long entityVersion = Faker.Random.Long(1);
        string? lastChangedBy = Faker.Person.FullName;
        var lastChangedAt = DateTimeOffset.UtcNow;
        var lastChangedCorrelationId = Guid.NewGuid();
        string lastChangedExecutionOrigin = "TestOrigin";
        string lastChangedBusinessOperationCode = "TEST_OP";

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId),
            tenantInfo: TenantInfo.Create(tenantCode),
            createdAt: createdAt,
            createdBy: createdBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_CREATE",
            lastChangedAt: lastChangedAt,
            lastChangedBy: lastChangedBy,
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: lastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: lastChangedBusinessOperationCode,
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion));

        var user = User.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo,
                "testuser",
                EmailAddress.CreateNew("test@example.com"),
                PasswordHash.CreateNew([1, 2, 3]),
                UserStatus.Active));

        // Act
        LogAct("Creating UserDataModel from User entity");
        var dataModel = UserDataModelFactory.Create(user);

        // Assert
        LogAssert("Verifying base fields from EntityInfo");
        dataModel.Id.ShouldBe(entityId);
        dataModel.TenantCode.ShouldBe(tenantCode);
        dataModel.CreatedBy.ShouldBe(createdBy);
        dataModel.CreatedAt.ShouldBe(createdAt);
        dataModel.EntityVersion.ShouldBe(entityVersion);
        dataModel.LastChangedBy.ShouldBe(lastChangedBy);
        dataModel.LastChangedAt.ShouldBe(lastChangedAt);
        dataModel.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        dataModel.LastChangedExecutionOrigin.ShouldBe(lastChangedExecutionOrigin);
        dataModel.LastChangedBusinessOperationCode.ShouldBe(lastChangedBusinessOperationCode);
    }

    #region Helper Methods

    private static User CreateTestUser(
        string username,
        string email,
        byte[] passwordHashBytes,
        UserStatus status)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return User.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo,
                username,
                EmailAddress.CreateNew(email),
                PasswordHash.CreateNew(passwordHashBytes),
                status));
    }

    #endregion
}
