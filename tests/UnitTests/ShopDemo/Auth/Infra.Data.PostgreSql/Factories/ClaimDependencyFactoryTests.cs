using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ClaimDependencyFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ClaimDependencyFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapClaimIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModel with specific ClaimId");
        var expectedClaimId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedClaimId, Guid.NewGuid());

        // Act
        LogAct("Creating ClaimDependency from ClaimDependencyDataModel");
        var entity = ClaimDependencyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ClaimId mapping");
        entity.ClaimId.Value.ShouldBe(expectedClaimId);
    }

    [Fact]
    public void Create_ShouldMapDependsOnClaimIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModel with specific DependsOnClaimId");
        var expectedDependsOnClaimId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedDependsOnClaimId);

        // Act
        LogAct("Creating ClaimDependency from ClaimDependencyDataModel");
        var entity = ClaimDependencyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying DependsOnClaimId mapping");
        entity.DependsOnClaimId.Value.ShouldBe(expectedDependsOnClaimId);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModel with specific base fields");
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

        var dataModel = new ClaimDependencyDataModel
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
            ClaimId = Guid.NewGuid(),
            DependsOnClaimId = Guid.NewGuid()
        };

        // Act
        LogAct("Creating ClaimDependency from ClaimDependencyDataModel");
        var entity = ClaimDependencyFactory.Create(dataModel);

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
        LogArrange("Creating ClaimDependencyDataModel with null last-changed fields");
        var dataModel = new ClaimDependencyDataModel
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
            ClaimId = Guid.NewGuid(),
            DependsOnClaimId = Guid.NewGuid()
        };

        // Act
        LogAct("Creating ClaimDependency from ClaimDependencyDataModel with nulls");
        var entity = ClaimDependencyFactory.Create(dataModel);

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
        LogArrange("Creating ClaimDependencyDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid());

        // Act
        LogAct("Creating ClaimDependency from ClaimDependencyDataModel");
        var entity = ClaimDependencyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid());

        // Act
        LogAct("Creating ClaimDependency from ClaimDependencyDataModel");
        var entity = ClaimDependencyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDependencyDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), Guid.NewGuid());

        // Act
        LogAct("Creating ClaimDependency from ClaimDependencyDataModel");
        var entity = ClaimDependencyFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ClaimDependencyDataModel CreateTestDataModel(Guid claimId, Guid dependsOnClaimId)
    {
        return new ClaimDependencyDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_CLAIM_DEPENDENCY",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            ClaimId = claimId,
            DependsOnClaimId = dependsOnClaimId
        };
    }

    #endregion
}
