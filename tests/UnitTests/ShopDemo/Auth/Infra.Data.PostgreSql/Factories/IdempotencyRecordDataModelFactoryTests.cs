using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class IdempotencyRecordDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public IdempotencyRecordDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapIdempotencyKeyCorrectly()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord entity with known IdempotencyKey");
        string expectedKey = Faker.Random.Guid().ToString();
        var entity = CreateTestEntity(expectedKey, "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecordDataModel from IdempotencyRecord entity");
        var dataModel = IdempotencyRecordDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying IdempotencyKey mapping");
        dataModel.IdempotencyKey.ShouldBe(expectedKey);
    }

    [Fact]
    public void Create_ShouldMapRequestHashCorrectly()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord entity with known RequestHash");
        string expectedHash = Faker.Random.String2(64);
        var entity = CreateTestEntity("key-123", expectedHash, null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecordDataModel from IdempotencyRecord entity");
        var dataModel = IdempotencyRecordDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying RequestHash mapping");
        dataModel.RequestHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Create_ShouldMapResponseBodyCorrectly()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord entity with known ResponseBody");
        string expectedResponseBody = "{\"result\":\"ok\"}";
        var entity = CreateTestEntity("key-123", "hash-abc", expectedResponseBody, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecordDataModel from IdempotencyRecord entity");
        var dataModel = IdempotencyRecordDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ResponseBody mapping");
        dataModel.ResponseBody.ShouldBe(expectedResponseBody);
    }

    [Fact]
    public void Create_ShouldMapNullResponseBodyCorrectly()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord entity with null ResponseBody");
        var entity = CreateTestEntity("key-123", "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecordDataModel from IdempotencyRecord entity");
        var dataModel = IdempotencyRecordDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null ResponseBody mapping");
        dataModel.ResponseBody.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapStatusCodeCorrectly()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord entity with known StatusCode");
        int expectedStatusCode = 201;
        var entity = CreateTestEntity("key-123", "hash-abc", null, expectedStatusCode, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Creating IdempotencyRecordDataModel from IdempotencyRecord entity");
        var dataModel = IdempotencyRecordDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying StatusCode mapping");
        dataModel.StatusCode.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord entity with known ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var entity = CreateTestEntity("key-123", "hash-abc", null, 200, expectedExpiresAt);

        // Act
        LogAct("Creating IdempotencyRecordDataModel from IdempotencyRecord entity");
        var dataModel = IdempotencyRecordDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecord entity with specific EntityInfo values");
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

        var entity = IdempotencyRecord.CreateFromExistingInfo(
            new CreateFromExistingInfoIdempotencyRecordInput(
                entityInfo,
                "key-123",
                "hash-abc",
                null,
                200,
                DateTimeOffset.UtcNow.AddMinutes(30)));

        // Act
        LogAct("Creating IdempotencyRecordDataModel from IdempotencyRecord entity");
        var dataModel = IdempotencyRecordDataModelFactory.Create(entity);

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

    private static IdempotencyRecord CreateTestEntity(
        string idempotencyKey,
        string requestHash,
        string? responseBody,
        int statusCode,
        DateTimeOffset expiresAt)
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

        return IdempotencyRecord.CreateFromExistingInfo(
            new CreateFromExistingInfoIdempotencyRecordInput(
                entityInfo,
                idempotencyKey,
                requestHash,
                responseBody,
                statusCode,
                expiresAt));
    }

    #endregion
}
