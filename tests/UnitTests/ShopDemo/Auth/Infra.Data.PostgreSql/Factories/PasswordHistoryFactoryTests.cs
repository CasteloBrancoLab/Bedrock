using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class PasswordHistoryFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public PasswordHistoryFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel with specific UserId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedUserId, "hashed-password", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating PasswordHistory from PasswordHistoryDataModel");
        var entity = PasswordHistoryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapPasswordHashFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel with specific PasswordHash");
        string expectedPasswordHash = Faker.Random.AlphaNumeric(128);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedPasswordHash, DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating PasswordHistory from PasswordHistoryDataModel");
        var entity = PasswordHistoryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying PasswordHash mapping");
        entity.PasswordHash.ShouldBe(expectedPasswordHash);
    }

    [Fact]
    public void Create_ShouldMapChangedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel with specific ChangedAt");
        var expectedChangedAt = DateTimeOffset.UtcNow.AddDays(-10);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "hashed-password", expectedChangedAt);

        // Act
        LogAct("Creating PasswordHistory from PasswordHistoryDataModel");
        var entity = PasswordHistoryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ChangedAt mapping");
        entity.ChangedAt.ShouldBe(expectedChangedAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel with specific base fields");
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

        var dataModel = new PasswordHistoryDataModel
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
            UserId = Guid.NewGuid(),
            PasswordHash = "hashed-password",
            ChangedAt = DateTimeOffset.UtcNow
        };

        // Act
        LogAct("Creating PasswordHistory from PasswordHistoryDataModel");
        var entity = PasswordHistoryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EntityInfo fields");
        entity.EntityInfo.Id.Value.ShouldBe(expectedId);
        entity.EntityInfo.TenantInfo.Code.ShouldBe(expectedTenantCode);
        entity.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe(expectedCreatedBy);
        entity.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(expectedCreatedAt);
        entity.EntityInfo.EntityVersion.Value.ShouldBe(expectedVersion);
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBe(expectedLastChangedBy);
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(expectedLastChangedAt);
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBe(expectedLastChangedCorrelationId);
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBe(expectedLastChangedExecutionOrigin);
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBe(expectedLastChangedBusinessOperationCode);
    }

    [Fact]
    public void Create_WithNullLastChangedFields_ShouldMapCorrectly()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel with null last-changed fields");
        var dataModel = new PasswordHistoryDataModel
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
            UserId = Guid.NewGuid(),
            PasswordHash = "hashed-password",
            ChangedAt = DateTimeOffset.UtcNow
        };

        // Act
        LogAct("Creating PasswordHistory from PasswordHistoryDataModel with nulls");
        var entity = PasswordHistoryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying nullable fields are null");
        entity.EntityInfo.EntityChangeInfo.LastChangedBy.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin.ShouldBeNull();
        entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapCreatedCorrelationIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel to verify CreatedCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "hashed-password", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating PasswordHistory from PasswordHistoryDataModel");
        var entity = PasswordHistoryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel to verify CreatedExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "hashed-password", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating PasswordHistory from PasswordHistoryDataModel");
        var entity = PasswordHistoryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordHistoryDataModel to verify CreatedBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "hashed-password", DateTimeOffset.UtcNow);

        // Act
        LogAct("Creating PasswordHistory from PasswordHistoryDataModel");
        var entity = PasswordHistoryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static PasswordHistoryDataModel CreateTestDataModel(
        Guid userId,
        string passwordHash,
        DateTimeOffset changedAt)
    {
        return new PasswordHistoryDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_PASSWORD_HISTORY",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = userId,
            PasswordHash = passwordHash,
            ChangedAt = changedAt
        };
    }

    #endregion
}
