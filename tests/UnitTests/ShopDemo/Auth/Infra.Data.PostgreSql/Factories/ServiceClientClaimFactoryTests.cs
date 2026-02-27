using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ServiceClientClaimFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ServiceClientClaimFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapServiceClientIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel with specific serviceClientId");
        var expectedServiceClientId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedServiceClientId, Guid.NewGuid(), (short)1);

        // Act
        LogAct("Creating ServiceClientClaim from ServiceClientClaimDataModel");
        var entity = ServiceClientClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ServiceClientId mapping");
        entity.ServiceClientId.Value.ShouldBe(expectedServiceClientId);
    }

    [Fact]
    public void Create_ShouldMapClaimIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel with specific claimId");
        var expectedClaimId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedClaimId, (short)1);

        // Act
        LogAct("Creating ServiceClientClaim from ServiceClientClaimDataModel");
        var entity = ServiceClientClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ClaimId mapping");
        entity.ClaimId.Value.ShouldBe(expectedClaimId);
    }

    [Theory]
    [InlineData((short)1)]
    [InlineData((short)-1)]
    [InlineData((short)0)]
    public void Create_ShouldMapValueFromDataModel(short expectedValue)
    {
        // Arrange
        LogArrange($"Creating ServiceClientClaimDataModel with value {expectedValue}");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), expectedValue);

        // Act
        LogAct("Creating ServiceClientClaim from ServiceClientClaimDataModel");
        var entity = ServiceClientClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Value mapping");
        entity.Value.Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel with specific base fields");
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

        var dataModel = new ServiceClientClaimDataModel
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
            ClaimId = Guid.NewGuid(),
            Value = (short)1
        };

        // Act
        LogAct("Creating ServiceClientClaim from ServiceClientClaimDataModel");
        var entity = ServiceClientClaimFactory.Create(dataModel);

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
        LogArrange("Creating ServiceClientClaimDataModel with null last-changed fields");
        var dataModel = new ServiceClientClaimDataModel
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
            ClaimId = Guid.NewGuid(),
            Value = (short)1
        };

        // Act
        LogAct("Creating ServiceClientClaim from ServiceClientClaimDataModel with nulls");
        var entity = ServiceClientClaimFactory.Create(dataModel);

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
        LogArrange("Creating ServiceClientClaimDataModel to verify createdCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), (short)1);

        // Act
        LogAct("Creating ServiceClientClaim from ServiceClientClaimDataModel");
        var entity = ServiceClientClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel to verify createdExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), (short)1);

        // Act
        LogAct("Creating ServiceClientClaim from ServiceClientClaimDataModel");
        var entity = ServiceClientClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ServiceClientClaimDataModel to verify createdBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid(), (short)1);

        // Act
        LogAct("Creating ServiceClientClaim from ServiceClientClaimDataModel");
        var entity = ServiceClientClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ServiceClientClaimDataModel CreateTestDataModel(
        Guid serviceClientId,
        Guid claimId,
        short value)
    {
        return new ServiceClientClaimDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_SERVICE_CLIENT_CLAIM",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            ServiceClientId = serviceClientId,
            ClaimId = claimId,
            Value = value
        };
    }

    #endregion
}
