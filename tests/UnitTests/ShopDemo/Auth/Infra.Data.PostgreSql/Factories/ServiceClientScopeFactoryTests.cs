using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ServiceClientScopeFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientScopeFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapServiceClientIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModel with specific serviceClientId");
        var expectedServiceClientId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedServiceClientId, "openid");

        // Act
        LogAct("Creating ServiceClientScope from ServiceClientScopeDataModel");
        var entity = ServiceClientScopeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ServiceClientId mapping");
        entity.ServiceClientId.Value.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Create_ShouldMapScopeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModel with specific scope");
        string expectedScope = "profile";
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedScope);

        // Act
        LogAct("Creating ServiceClientScope from ServiceClientScopeDataModel");
        var entity = ServiceClientScopeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Scope mapping");
        entity.Scope.ShouldBe(expectedScope);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModel with specific base fields");
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

        var dataModel = new ServiceClientScopeDataModel
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
            Scope = "openid"
        };

        // Act
        LogAct("Creating ServiceClientScope from ServiceClientScopeDataModel");
        var entity = ServiceClientScopeFactory.Create(dataModel);

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
        LogArrange("Creating ServiceClientScopeDataModel with null last-changed fields");
        var dataModel = new ServiceClientScopeDataModel
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
            Scope = "openid"
        };

        // Act
        LogAct("Creating ServiceClientScope from ServiceClientScopeDataModel with nulls");
        var entity = ServiceClientScopeFactory.Create(dataModel);

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
        LogArrange("Creating ServiceClientScopeDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "openid");

        // Act
        LogAct("Creating ServiceClientScope from ServiceClientScopeDataModel");
        var entity = ServiceClientScopeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "openid");

        // Act
        LogAct("Creating ServiceClientScope from ServiceClientScopeDataModel");
        var entity = ServiceClientScopeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientScopeDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "openid");

        // Act
        LogAct("Creating ServiceClientScope from ServiceClientScopeDataModel");
        var entity = ServiceClientScopeFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ServiceClientScopeDataModel CreateTestDataModel(Guid serviceClientId, string scope)
    {
        return new ServiceClientScopeDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_SERVICE_CLIENT_SCOPE",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            ServiceClientId = serviceClientId,
            Scope = scope
        };
    }

    #endregion
}
