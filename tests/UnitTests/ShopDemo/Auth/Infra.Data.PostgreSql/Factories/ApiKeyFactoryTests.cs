using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ApiKeyFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ApiKeyFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapServiceClientIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel with specific ServiceClientId");
        var expectedServiceClientId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedServiceClientId, "pfx", "hash123", (short)ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ServiceClientId mapping");
        entity.ServiceClientId.Value.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Create_ShouldMapKeyPrefixFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel with specific KeyPrefix");
        string expectedKeyPrefix = Faker.Random.String2(8);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedKeyPrefix, "hash123", (short)ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying KeyPrefix mapping");
        entity.KeyPrefix.ShouldBe(expectedKeyPrefix);
    }

    [Fact]
    public void Create_ShouldMapKeyHashFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel with specific KeyHash");
        string expectedKeyHash = Faker.Random.String2(64);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "pfx", expectedKeyHash, (short)ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying KeyHash mapping");
        entity.KeyHash.ShouldBe(expectedKeyHash);
    }

    [Theory]
    [InlineData((short)1, ApiKeyStatus.Active)]
    [InlineData((short)2, ApiKeyStatus.Revoked)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, ApiKeyStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating ApiKeyDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "pfx", "hash123", statusValue);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel with specific ExpiresAt");
        DateTimeOffset? expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "pfx", "hash123", (short)ApiKeyStatus.Active, expiresAt: expectedExpiresAt);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapLastUsedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel with specific LastUsedAt");
        DateTimeOffset? expectedLastUsedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "pfx", "hash123", (short)ApiKeyStatus.Active, lastUsedAt: expectedLastUsedAt);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying LastUsedAt mapping");
        entity.LastUsedAt.ShouldBe(expectedLastUsedAt);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel with specific RevokedAt");
        DateTimeOffset? expectedRevokedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "pfx", "hash123", (short)ApiKeyStatus.Revoked, revokedAt: expectedRevokedAt);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        entity.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel with specific base fields");
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

        var dataModel = new ApiKeyDataModel
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
            ServiceClientId = Guid.NewGuid(),
            KeyPrefix = "pfx",
            KeyHash = "hash123",
            Status = (short)ApiKeyStatus.Active
        };

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

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
        LogArrange("Creating ApiKeyDataModel with null last-changed fields");
        var dataModel = new ApiKeyDataModel
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
            ServiceClientId = Guid.NewGuid(),
            KeyPrefix = "pfx",
            KeyHash = "hash123",
            Status = (short)ApiKeyStatus.Active
        };

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel with nulls");
        var entity = ApiKeyFactory.Create(dataModel);

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
        LogArrange("Creating ApiKeyDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "pfx", "hash123", (short)ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "pfx", "hash123", (short)ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ApiKeyDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "pfx", "hash123", (short)ApiKeyStatus.Active);

        // Act
        LogAct("Creating ApiKey from ApiKeyDataModel");
        var entity = ApiKeyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ApiKeyDataModel CreateTestDataModel(
        Guid serviceClientId,
        string keyPrefix,
        string keyHash,
        short status,
        DateTimeOffset? expiresAt = null,
        DateTimeOffset? lastUsedAt = null,
        DateTimeOffset? revokedAt = null)
    {
        return new ApiKeyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_API_KEY",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            ServiceClientId = serviceClientId,
            KeyPrefix = keyPrefix,
            KeyHash = keyHash,
            Status = status,
            ExpiresAt = expiresAt,
            LastUsedAt = lastUsedAt,
            RevokedAt = revokedAt
        };
    }

    #endregion
}
