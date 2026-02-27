using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class DenyListEntryDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public DenyListEntryDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [InlineData(DenyListEntryType.Jti, 1)]
    [InlineData(DenyListEntryType.UserId, 2)]
    public void Create_ShouldMapTypeAsShortCorrectly(DenyListEntryType type, short expectedShortValue)
    {
        // Arrange
        LogArrange($"Creating DenyListEntry entity with type {type}");
        var entity = CreateTestEntity(type, "some-value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntryDataModel from DenyListEntry entity");
        var dataModel = DenyListEntryDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying Type mapped to short value {expectedShortValue}");
        dataModel.Type.ShouldBe(expectedShortValue);
    }

    [Fact]
    public void Create_ShouldMapValueCorrectly()
    {
        // Arrange
        LogArrange("Creating DenyListEntry entity with known Value");
        string expectedValue = Faker.Random.Guid().ToString();
        var entity = CreateTestEntity(DenyListEntryType.Jti, expectedValue, DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntryDataModel from DenyListEntry entity");
        var dataModel = DenyListEntryDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Value mapping");
        dataModel.Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtCorrectly()
    {
        // Arrange
        LogArrange("Creating DenyListEntry entity with known ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(1);
        var entity = CreateTestEntity(DenyListEntryType.Jti, "value", expectedExpiresAt, null);

        // Act
        LogAct("Creating DenyListEntryDataModel from DenyListEntry entity");
        var dataModel = DenyListEntryDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapReasonCorrectly()
    {
        // Arrange
        LogArrange("Creating DenyListEntry entity with known Reason");
        string expectedReason = "Token revoked by admin";
        var entity = CreateTestEntity(DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), expectedReason);

        // Act
        LogAct("Creating DenyListEntryDataModel from DenyListEntry entity");
        var dataModel = DenyListEntryDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Reason mapping");
        dataModel.Reason.ShouldBe(expectedReason);
    }

    [Fact]
    public void Create_ShouldMapNullReasonCorrectly()
    {
        // Arrange
        LogArrange("Creating DenyListEntry entity with null Reason");
        var entity = CreateTestEntity(DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntryDataModel from DenyListEntry entity");
        var dataModel = DenyListEntryDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null Reason mapping");
        dataModel.Reason.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating DenyListEntry entity with specific EntityInfo values");
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

        var entity = DenyListEntry.CreateFromExistingInfo(
            new CreateFromExistingInfoDenyListEntryInput(
                entityInfo,
                DenyListEntryType.Jti,
                "some-value",
                DateTimeOffset.UtcNow.AddHours(1),
                null));

        // Act
        LogAct("Creating DenyListEntryDataModel from DenyListEntry entity");
        var dataModel = DenyListEntryDataModelFactory.Create(entity);

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

    private static DenyListEntry CreateTestEntity(
        DenyListEntryType type,
        string value,
        DateTimeOffset expiresAt,
        string? reason)
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

        return DenyListEntry.CreateFromExistingInfo(
            new CreateFromExistingInfoDenyListEntryInput(entityInfo, type, value, expiresAt, reason));
    }

    #endregion
}
