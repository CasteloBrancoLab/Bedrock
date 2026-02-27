using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class IdempotencyRecordFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public IdempotencyRecordFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapIdempotencyKeyFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel with specific IdempotencyKey");
        string expectedKey = Faker.Random.Guid().ToString();
        var dataModel = CreateTestDataModel(expectedKey, "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying IdempotencyKey mapping");
        entity.IdempotencyKey.ShouldBe(expectedKey);
    }

    [Fact]
    public void Create_ShouldMapRequestHashFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel with specific RequestHash");
        string expectedHash = Faker.Random.String2(64);
        var dataModel = CreateTestDataModel("key-123", expectedHash, null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RequestHash mapping");
        entity.RequestHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Create_ShouldMapResponseBodyFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel with specific ResponseBody");
        string expectedResponseBody = "{\"data\":\"result\"}";
        var dataModel = CreateTestDataModel("key-123", "hash-abc", expectedResponseBody, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ResponseBody mapping");
        entity.ResponseBody.ShouldBe(expectedResponseBody);
    }

    [Fact]
    public void Create_ShouldMapNullResponseBodyFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel with null ResponseBody");
        var dataModel = CreateTestDataModel("key-123", "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying null ResponseBody mapping");
        entity.ResponseBody.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapStatusCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel with specific StatusCode");
        int expectedStatusCode = 422;
        var dataModel = CreateTestDataModel("key-123", "hash-abc", null, expectedStatusCode, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying StatusCode mapping");
        entity.StatusCode.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel with specific ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(6);
        var dataModel = CreateTestDataModel("key-123", "hash-abc", null, 200, expectedExpiresAt);

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel with specific base fields");
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

        var dataModel = new IdempotencyRecordDataModel
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
            IdempotencyKey = "key-123",
            RequestHash = "hash-abc",
            ResponseBody = null,
            StatusCode = 200,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

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
        LogArrange("Creating IdempotencyRecordDataModel with null last-changed fields");
        var dataModel = new IdempotencyRecordDataModel
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
            IdempotencyKey = "key-123",
            RequestHash = "hash-abc",
            ResponseBody = null,
            StatusCode = 200,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel with nulls");
        var entity = IdempotencyRecordFactory.Create(dataModel);

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
        LogArrange("Creating IdempotencyRecordDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel("key-123", "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel("key-123", "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel("key-123", "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecord from IdempotencyRecordDataModel");
        var entity = IdempotencyRecordFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static IdempotencyRecordDataModel CreateTestDataModel(
        string idempotencyKey,
        string requestHash,
        string? responseBody,
        int statusCode,
        DateTimeOffset expiresAt)
    {
        return new IdempotencyRecordDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_IDEMPOTENCY_RECORD",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            IdempotencyKey = idempotencyKey,
            RequestHash = requestHash,
            ResponseBody = responseBody,
            StatusCode = statusCode,
            ExpiresAt = expiresAt
        };
    }

    #endregion
}
