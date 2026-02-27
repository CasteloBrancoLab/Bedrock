using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ClaimFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ClaimFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapNameFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel with specific Name");
        string expectedName = Faker.Random.Word();
        var dataModel = CreateTestDataModel(expectedName, "A test claim");

        // Act
        LogAct("Creating Claim from ClaimDataModel");
        var entity = ClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Name mapping");
        entity.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void Create_ShouldMapDescriptionFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel with specific Description");
        string expectedDescription = Faker.Lorem.Sentence();
        var dataModel = CreateTestDataModel("claim-name", expectedDescription);

        // Act
        LogAct("Creating Claim from ClaimDataModel");
        var entity = ClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Description mapping");
        entity.Description.ShouldBe(expectedDescription);
    }

    [Fact]
    public void Create_ShouldMapNullDescriptionFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel with null Description");
        var dataModel = CreateTestDataModel("claim-name", null);

        // Act
        LogAct("Creating Claim from ClaimDataModel");
        var entity = ClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying null Description mapping");
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel with specific base fields");
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

        var dataModel = new ClaimDataModel
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
            Name = "claim-name",
            Description = null
        };

        // Act
        LogAct("Creating Claim from ClaimDataModel");
        var entity = ClaimFactory.Create(dataModel);

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
        LogArrange("Creating ClaimDataModel with null last-changed fields");
        var dataModel = new ClaimDataModel
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
            Name = "claim-name",
            Description = null
        };

        // Act
        LogAct("Creating Claim from ClaimDataModel with nulls");
        var entity = ClaimFactory.Create(dataModel);

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
        LogArrange("Creating ClaimDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel("claim-name", null);

        // Act
        LogAct("Creating Claim from ClaimDataModel");
        var entity = ClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel("claim-name", null);

        // Act
        LogAct("Creating Claim from ClaimDataModel");
        var entity = ClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ClaimDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel("claim-name", null);

        // Act
        LogAct("Creating Claim from ClaimDataModel");
        var entity = ClaimFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ClaimDataModel CreateTestDataModel(string name, string? description)
    {
        return new ClaimDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_CLAIM",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Name = name,
            Description = description
        };
    }

    #endregion
}
