using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class ExternalLoginFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public ExternalLoginFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel with specific UserId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedUserId, "google", "user-123", "user@gmail.com");

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapProviderFromDataModel()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel with specific Provider");
        string expectedProvider = "github";
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedProvider, "user-456", null);

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Provider mapping");
        entity.Provider.Value.ShouldBe(expectedProvider);
    }

    [Fact]
    public void Create_ShouldMapProviderUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel with specific ProviderUserId");
        string expectedProviderUserId = Faker.Random.AlphaNumeric(20);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "google", expectedProviderUserId, null);

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ProviderUserId mapping");
        entity.ProviderUserId.ShouldBe(expectedProviderUserId);
    }

    [Fact]
    public void Create_ShouldMapEmailFromDataModel()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel with specific Email");
        string expectedEmail = Faker.Internet.Email();
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "google", "user-123", expectedEmail);

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Email mapping");
        entity.Email.ShouldBe(expectedEmail);
    }

    [Fact]
    public void Create_ShouldMapNullEmailFromDataModel()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel with null Email");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "google", "user-123", null);

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying null Email mapping");
        entity.Email.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel with specific base fields");
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

        var dataModel = new ExternalLoginDataModel
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
            Provider = "google",
            ProviderUserId = "user-123",
            Email = null
        };

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

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
        LogArrange("Creating ExternalLoginDataModel with null last-changed fields");
        var dataModel = new ExternalLoginDataModel
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
            Provider = "google",
            ProviderUserId = "user-123",
            Email = null
        };

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel with nulls");
        var entity = ExternalLoginFactory.Create(dataModel);

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
        LogArrange("Creating ExternalLoginDataModel to verify CreatedCorrelationId is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "google", "user-123", null);

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel to verify CreatedExecutionOrigin is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "google", "user-123", null);

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating ExternalLoginDataModel to verify CreatedBusinessOperationCode is mapped");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "google", "user-123", null);

        // Act
        LogAct("Creating ExternalLogin from ExternalLoginDataModel");
        var entity = ExternalLoginFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static ExternalLoginDataModel CreateTestDataModel(
        Guid userId,
        string provider,
        string providerUserId,
        string? email)
    {
        return new ExternalLoginDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_EXTERNAL_LOGIN",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            Email = email
        };
    }

    #endregion
}
