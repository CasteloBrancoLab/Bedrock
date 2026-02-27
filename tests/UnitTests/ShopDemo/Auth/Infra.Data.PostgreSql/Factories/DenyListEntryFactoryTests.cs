using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class DenyListEntryFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public DenyListEntryFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [InlineData((short)1, DenyListEntryType.Jti)]
    [InlineData((short)2, DenyListEntryType.UserId)]
    public void Create_ShouldMapTypeFromDataModel(short typeValue, DenyListEntryType expectedType)
    {
        // Arrange
        LogArrange($"Creating DenyListEntryDataModel with type value {typeValue}");
        var dataModel = CreateTestDataModel(typeValue, "some-value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Type mapped to {expectedType}");
        entity.Type.ShouldBe(expectedType);
    }

    [Fact]
    public void Create_ShouldMapValueFromDataModel()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel with specific Value");
        string expectedValue = Faker.Random.Guid().ToString();
        var dataModel = CreateTestDataModel((short)DenyListEntryType.Jti, expectedValue, DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Value mapping");
        entity.Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel with specific ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(2);
        var dataModel = CreateTestDataModel((short)DenyListEntryType.Jti, "value", expectedExpiresAt, null);

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapReasonFromDataModel()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel with specific Reason");
        string expectedReason = "Revoked by security policy";
        var dataModel = CreateTestDataModel((short)DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), expectedReason);

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Reason mapping");
        entity.Reason.ShouldBe(expectedReason);
    }

    [Fact]
    public void Create_ShouldMapNullReasonFromDataModel()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel with null Reason");
        var dataModel = CreateTestDataModel((short)DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying null Reason mapping");
        entity.Reason.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel with specific base fields");
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

        var dataModel = new DenyListEntryDataModel
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
            Type = (short)DenyListEntryType.Jti,
            Value = "some-value",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Reason = null
        };

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

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
        LogArrange("Creating DenyListEntryDataModel with null last-changed fields");
        var dataModel = new DenyListEntryDataModel
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
            Type = (short)DenyListEntryType.Jti,
            Value = "some-value",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Reason = null
        };

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel with nulls");
        var entity = DenyListEntryFactory.Create(dataModel);

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
        LogArrange("Creating DenyListEntryDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel((short)DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel((short)DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating DenyListEntryDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel((short)DenyListEntryType.Jti, "value", DateTimeOffset.UtcNow.AddHours(1), null);

        // Act
        LogAct("Creating DenyListEntry from DenyListEntryDataModel");
        var entity = DenyListEntryFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static DenyListEntryDataModel CreateTestDataModel(
        short type,
        string value,
        DateTimeOffset expiresAt,
        string? reason)
    {
        return new DenyListEntryDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_DENY_LIST_ENTRY",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Type = type,
            Value = value,
            ExpiresAt = expiresAt,
            Reason = reason
        };
    }

    #endregion
}
