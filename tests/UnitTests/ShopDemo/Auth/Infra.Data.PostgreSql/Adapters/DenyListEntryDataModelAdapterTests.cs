using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class DenyListEntryDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public DenyListEntryDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateTypeFromEntity()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel and DenyListEntry with different Types");
        var dataModel = CreateTestDataModel();
        dataModel.Type = (short)DenyListEntryType.Jti;
        var entity = CreateTestEntity(DenyListEntryType.UserId, "value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Adapting data model from entity");
        DenyListEntryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Type was updated");
        dataModel.Type.ShouldBe((short)DenyListEntryType.UserId);
    }

    [Fact]
    public void Adapt_ShouldUpdateValueFromEntity()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel and DenyListEntry with different Values");
        var dataModel = CreateTestDataModel();
        string expectedValue = Faker.Random.Guid().ToString();
        var entity = CreateTestEntity(DenyListEntryType.Jti, expectedValue, DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Adapting data model from entity");
        DenyListEntryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Value was updated");
        dataModel.Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Adapt_ShouldUpdateExpiresAtFromEntity()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel and DenyListEntry with different ExpiresAt values");
        var dataModel = CreateTestDataModel();
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var entity = CreateTestEntity(DenyListEntryType.Jti, "value", expectedExpiresAt, null);

        // Act
        LogAct("Adapting data model from entity");
        DenyListEntryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying ExpiresAt was updated");
        dataModel.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateReasonFromEntity()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel and DenyListEntry with different Reasons");
        var dataModel = CreateTestDataModel();
        string expectedReason = "Suspicious activity detected";
        var entity = CreateTestEntity(DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), expectedReason);

        // Act
        LogAct("Adapting data model from entity");
        DenyListEntryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Reason was updated");
        dataModel.Reason.ShouldBe(expectedReason);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel and DenyListEntry with different EntityInfo values");
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

        var entity = DenyListEntry.CreateFromExistingInfo(
            new CreateFromExistingInfoDenyListEntryInput(
                entityInfo,
                DenyListEntryType.Jti,
                "some-value",
                DateTimeOffset.UtcNow.AddHours(1),
                null));

        // Act
        LogAct("Adapting data model from entity");
        DenyListEntryDataModelAdapter.Adapt(dataModel, entity);

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
        LogArrange("Creating DenyListEntryDataModel and DenyListEntry");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity(DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Adapting data model from entity");
        var result = DenyListEntryDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static DenyListEntryDataModel CreateTestDataModel()
    {
        return new DenyListEntryDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            Type = (short)DenyListEntryType.Jti,
            Value = "initial-value",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            Reason = null
        };
    }

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
