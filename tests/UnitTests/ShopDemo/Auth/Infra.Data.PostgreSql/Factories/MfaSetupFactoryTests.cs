using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class MfaSetupFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public MfaSetupFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel with specific UserId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedUserId, "encrypted-secret", false, null);

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapEncryptedSharedSecretFromDataModel()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel with specific EncryptedSharedSecret");
        string expectedSecret = Faker.Random.AlphaNumeric(256);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedSecret, false, null);

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EncryptedSharedSecret mapping");
        entity.EncryptedSharedSecret.ShouldBe(expectedSecret);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldMapIsEnabledFromDataModel(bool expectedIsEnabled)
    {
        // Arrange
        LogArrange($"Creating MfaSetupDataModel with IsEnabled={expectedIsEnabled}");
        var enabledAt = expectedIsEnabled ? DateTimeOffset.UtcNow.AddDays(-1) : (DateTimeOffset?)null;
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "encrypted-secret", expectedIsEnabled, enabledAt);

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying IsEnabled mapped to {expectedIsEnabled}");
        entity.IsEnabled.ShouldBe(expectedIsEnabled);
    }

    [Fact]
    public void Create_ShouldMapEnabledAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel with specific EnabledAt");
        var expectedEnabledAt = DateTimeOffset.UtcNow.AddDays(-3);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "encrypted-secret", true, expectedEnabledAt);

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying EnabledAt mapping");
        entity.EnabledAt.ShouldBe(expectedEnabledAt);
    }

    [Fact]
    public void Create_ShouldMapNullEnabledAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel with null EnabledAt");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "encrypted-secret", false, null);

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying null EnabledAt mapping");
        entity.EnabledAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel with specific base fields");
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

        var dataModel = new MfaSetupDataModel
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
            UserId = Guid.NewGuid(),
            EncryptedSharedSecret = "encrypted-secret",
            IsEnabled = false,
            EnabledAt = null
        };

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

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
        LogArrange("Creating MfaSetupDataModel with null last-changed fields");
        var dataModel = new MfaSetupDataModel
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
            UserId = Guid.NewGuid(),
            EncryptedSharedSecret = "encrypted-secret",
            IsEnabled = false,
            EnabledAt = null
        };

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel with nulls");
        var entity = MfaSetupFactory.Create(dataModel);

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
        LogArrange("Creating MfaSetupDataModel to verify CreatedCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "encrypted-secret", false, null);

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel to verify CreatedExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "encrypted-secret", false, null);

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating MfaSetupDataModel to verify CreatedBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "encrypted-secret", false, null);

        // Act
        LogAct("Creating MfaSetup from MfaSetupDataModel");
        var entity = MfaSetupFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static MfaSetupDataModel CreateTestDataModel(
        Guid userId,
        string encryptedSharedSecret,
        bool isEnabled,
        DateTimeOffset? enabledAt)
    {
        return new MfaSetupDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_MFA_SETUP",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = userId,
            EncryptedSharedSecret = encryptedSharedSecret,
            IsEnabled = isEnabled,
            EnabledAt = enabledAt
        };
    }

    #endregion
}
