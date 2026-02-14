using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Core.Entities.Users.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class UserDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public UserDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUsernameFromEntity()
    {
        // Arrange
        LogArrange("Creating UserDataModel and User with different usernames");
        var dataModel = CreateTestDataModel();
        string expectedUsername = Faker.Internet.UserName();
        var user = CreateTestUser(expectedUsername, "new@example.com", [10, 20, 30], UserStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        UserDataModelAdapter.Adapt(dataModel, user);

        // Assert
        LogAssert("Verifying Username was updated");
        dataModel.Username.ShouldBe(expectedUsername);
    }

    [Fact]
    public void Adapt_ShouldUpdateEmailFromEntity()
    {
        // Arrange
        LogArrange("Creating UserDataModel and User with different emails");
        var dataModel = CreateTestDataModel();
        string expectedEmail = Faker.Internet.Email();
        var user = CreateTestUser("testuser", expectedEmail, [10, 20, 30], UserStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        UserDataModelAdapter.Adapt(dataModel, user);

        // Assert
        LogAssert("Verifying Email was updated");
        dataModel.Email.ShouldBe(expectedEmail);
    }

    [Fact]
    public void Adapt_ShouldUpdatePasswordHashFromEntity()
    {
        // Arrange
        LogArrange("Creating UserDataModel and User with different password hashes");
        var dataModel = CreateTestDataModel();
        byte[] expectedHash = [99, 88, 77, 66, 55];
        var user = CreateTestUser("testuser", "test@example.com", expectedHash, UserStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        UserDataModelAdapter.Adapt(dataModel, user);

        // Assert
        LogAssert("Verifying PasswordHash was updated");
        dataModel.PasswordHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusFromEntity()
    {
        // Arrange
        LogArrange("Creating UserDataModel and User with different statuses");
        var dataModel = CreateTestDataModel();
        dataModel.Status = (short)UserStatus.Active;
        var user = CreateTestUser("testuser", "test@example.com", [1, 2, 3], UserStatus.Suspended);

        // Act
        LogAct("Adapting data model from entity");
        UserDataModelAdapter.Adapt(dataModel, user);

        // Assert
        LogAssert("Verifying Status was updated");
        dataModel.Status.ShouldBe((short)UserStatus.Suspended);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating UserDataModel and User with different EntityInfo values");
        var dataModel = CreateTestDataModel();
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-2);
        long expectedVersion = Faker.Random.Long(1);

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(expectedId),
            tenantInfo: TenantInfo.Create(expectedTenantCode),
            createdAt: expectedCreatedAt,
            createdBy: expectedCreatedBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(expectedVersion));

        var user = User.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo,
                "testuser",
                EmailAddress.CreateNew("test@example.com"),
                PasswordHash.CreateNew([1, 2, 3]),
                UserStatus.Active));

        // Act
        LogAct("Adapting data model from entity");
        UserDataModelAdapter.Adapt(dataModel, user);

        // Assert
        LogAssert("Verifying base fields were updated from EntityInfo");
        dataModel.Id.ShouldBe(expectedId);
        dataModel.TenantCode.ShouldBe(expectedTenantCode);
        dataModel.CreatedBy.ShouldBe(expectedCreatedBy);
        dataModel.CreatedAt.ShouldBe(expectedCreatedAt);
        dataModel.EntityVersion.ShouldBe(expectedVersion);
    }

    [Fact]
    public void Adapt_ShouldReturnTheSameDataModelInstance()
    {
        // Arrange
        LogArrange("Creating UserDataModel and User");
        var dataModel = CreateTestDataModel();
        var user = CreateTestUser("testuser", "test@example.com", [1, 2, 3], UserStatus.Active);

        // Act
        LogAct("Adapting data model from entity");
        var result = UserDataModelAdapter.Adapt(dataModel, user);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    [Fact]
    public void Adapt_ShouldUpdateAllFieldsSimultaneously()
    {
        // Arrange
        LogArrange("Creating UserDataModel with initial values and User with different values");
        var dataModel = new UserDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "old-creator",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            EntityVersion = 1,
            Username = "olduser",
            Email = "old@example.com",
            PasswordHash = [1, 1, 1],
            Status = (short)UserStatus.Active
        };

        string newUsername = Faker.Internet.UserName();
        string newEmail = Faker.Internet.Email();
        byte[] newHash = Faker.Random.Bytes(32);
        var user = CreateTestUser(newUsername, newEmail, newHash, UserStatus.Blocked);

        // Act
        LogAct("Adapting data model from entity");
        UserDataModelAdapter.Adapt(dataModel, user);

        // Assert
        LogAssert("Verifying all fields updated simultaneously");
        dataModel.Username.ShouldBe(newUsername);
        dataModel.Email.ShouldBe(newEmail);
        dataModel.PasswordHash.ShouldBe(newHash);
        dataModel.Status.ShouldBe((short)UserStatus.Blocked);
    }

    #region Helper Methods

    private static UserDataModel CreateTestDataModel()
    {
        return new UserDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            Username = "initialuser",
            Email = "initial@example.com",
            PasswordHash = [1, 2, 3],
            Status = (short)UserStatus.Active
        };
    }

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
