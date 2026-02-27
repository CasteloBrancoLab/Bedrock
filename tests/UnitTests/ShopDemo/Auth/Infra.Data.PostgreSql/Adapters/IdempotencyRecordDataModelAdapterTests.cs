using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class IdempotencyRecordDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public IdempotencyRecordDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateIdempotencyKeyFromEntity()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel and IdempotencyRecord with different IdempotencyKeys");
        var dataModel = CreateTestDataModel();
        string expectedKey = Faker.Random.Guid().ToString();
        var entity = CreateTestEntity(expectedKey, "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Adapting data model from entity");
        IdempotencyRecordDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IdempotencyKey was updated");
        dataModel.IdempotencyKey.ShouldBe(expectedKey);
    }

    [Fact]
    public void Adapt_ShouldUpdateRequestHashFromEntity()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel and IdempotencyRecord with different RequestHashes");
        var dataModel = CreateTestDataModel();
        string expectedHash = Faker.Random.String2(64);
        var entity = CreateTestEntity("key-123", expectedHash, null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Adapting data model from entity");
        IdempotencyRecordDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying RequestHash was updated");
        dataModel.RequestHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Adapt_ShouldUpdateResponseBodyFromEntity()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel and IdempotencyRecord with different ResponseBodies");
        var dataModel = CreateTestDataModel();
        string expectedResponseBody = "{\"updated\":true}";
        var entity = CreateTestEntity("key-123", "hash-abc", expectedResponseBody, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Adapting data model from entity");
        IdempotencyRecordDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ResponseBody was updated");
        dataModel.ResponseBody.ShouldBe(expectedResponseBody);
    }

    [Fact]
    public void Adapt_ShouldUpdateStatusCodeFromEntity()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel and IdempotencyRecord with different StatusCodes");
        var dataModel = CreateTestDataModel();
        int expectedStatusCode = 409;
        var entity = CreateTestEntity("key-123", "hash-abc", null, expectedStatusCode, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Adapting data model from entity");
        IdempotencyRecordDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying StatusCode was updated");
        dataModel.StatusCode.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel and IdempotencyRecord with different ExpiresAt values");
        var dataModel = CreateTestDataModel();
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var entity = CreateTestEntity("key-123", "hash-abc", null, 200, expectedExpiresAt);

        // Act
        LogAct("Adapting data model from entity");
        IdempotencyRecordDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating IdempotencyRecordDataModel and IdempotencyRecord with different EntityInfo values");
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

        var entity = IdempotencyRecord.CreateFromExistingInfo(
            new CreateFromExistingInfoIdempotencyRecordInput(
                entityInfo,
                "key-123",
                "hash-abc",
                null,
                200,
                DateTimeOffset.UtcNow.AddMinutes(30)));

        // Act
        LogAct("Adapting data model from entity");
        IdempotencyRecordDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating IdempotencyRecordDataModel and IdempotencyRecord");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity("key-123", "hash-abc", null, 200, DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        LogAct("Adapting data model from entity");
        var result = IdempotencyRecordDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static IdempotencyRecordDataModel CreateTestDataModel()
    {
        return new IdempotencyRecordDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            IdempotencyKey = "initial-key",
            RequestHash = "initial-hash",
            ResponseBody = null,
            StatusCode = 200,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
        };
    }

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
