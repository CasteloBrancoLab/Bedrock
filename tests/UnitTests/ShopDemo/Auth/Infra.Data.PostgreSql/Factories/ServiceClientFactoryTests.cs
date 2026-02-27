using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ServiceClientFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapClientIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel with specific clientId");
        string expectedClientId = Faker.Random.AlphaNumeric(20);
        var dataModel = CreateTestDataModel(expectedClientId, [1, 2, 3], "Test Client", (short)ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ClientId mapping");
        entity.ClientId.ShouldBe(expectedClientId);
    }

    [Fact]
    public void Create_ShouldMapClientSecretHashFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel with specific client secret hash");
        byte[] expectedHash = [10, 20, 30, 40, 50];
        var dataModel = CreateTestDataModel("client-id", expectedHash, "Test Client", (short)ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ClientSecretHash mapping");
        entity.ClientSecretHash.ShouldBe(expectedHash);
    }

    [Fact]
    public void Create_ShouldMapNameFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel with specific name");
        string expectedName = Faker.Company.CompanyName();
        var dataModel = CreateTestDataModel("client-id", [1, 2, 3], expectedName, (short)ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Name mapping");
        entity.Name.ShouldBe(expectedName);
    }

    [Theory]
    [InlineData((short)1, ServiceClientStatus.Active)]
    [InlineData((short)2, ServiceClientStatus.Revoked)]
    public void Create_ShouldMapStatusFromDataModel(short statusValue, ServiceClientStatus expectedStatus)
    {
        // Arrange
        LogArrange($"Creating ServiceClientDataModel with status value {statusValue}");
        var dataModel = CreateTestDataModel("client-id", [1, 2, 3], "Test Client", statusValue);

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying Status mapped to {expectedStatus}");
        entity.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Create_ShouldMapCreatedByUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel with specific createdByUserId");
        var expectedCreatedByUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel("client-id", [1, 2, 3], "Test Client", (short)ServiceClientStatus.Active);
        dataModel.CreatedByUserId = expectedCreatedByUserId;

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedByUserId mapping");
        entity.CreatedByUserId.Value.ShouldBe(expectedCreatedByUserId);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel with specific expiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        var dataModel = CreateTestDataModel("client-id", [1, 2, 3], "Test Client", (short)ServiceClientStatus.Active);
        dataModel.ExpiresAt = expectedExpiresAt;

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Fact]
    public void Create_ShouldMapRevokedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel with specific revokedAt");
        var expectedRevokedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var dataModel = CreateTestDataModel("client-id", [1, 2, 3], "Test Client", (short)ServiceClientStatus.Active);
        dataModel.RevokedAt = expectedRevokedAt;

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying RevokedAt mapping");
        entity.RevokedAt.ShouldBe(expectedRevokedAt);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel with specific base fields");
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

        var dataModel = new ServiceClientDataModel
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
            ClientId = "client-id",
            ClientSecretHash = [1, 2, 3],
            Name = "Test Client",
            Status = (short)ServiceClientStatus.Active,
            CreatedByUserId = Guid.NewGuid(),
            ExpiresAt = null,
            RevokedAt = null
        };

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

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
        LogArrange("Creating ServiceClientDataModel with null last-changed fields");
        var dataModel = new ServiceClientDataModel
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
            ClientId = "client-id",
            ClientSecretHash = [1, 2, 3],
            Name = "Test Client",
            Status = (short)ServiceClientStatus.Active,
            CreatedByUserId = Guid.NewGuid(),
            ExpiresAt = null,
            RevokedAt = null
        };

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel with nulls");
        var entity = ServiceClientFactory.Create(dataModel);

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
        LogArrange("Creating ServiceClientDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel("client-id", [1, 2, 3], "Test Client", (short)ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel("client-id", [1, 2, 3], "Test Client", (short)ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel("client-id", [1, 2, 3], "Test Client", (short)ServiceClientStatus.Active);

        // Act
        LogAct("Creating ServiceClient from ServiceClientDataModel");
        var entity = ServiceClientFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ServiceClientDataModel CreateTestDataModel(
        string clientId,
        byte[] clientSecretHash,
        string name,
        short status)
    {
        return new ServiceClientDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_SERVICE_CLIENT",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            ClientId = clientId,
            ClientSecretHash = clientSecretHash,
            Name = name,
            Status = status,
            CreatedByUserId = Guid.NewGuid(),
            ExpiresAt = null,
            RevokedAt = null
        };
    }

    #endregion
}
