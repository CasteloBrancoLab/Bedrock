using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class PasswordResetTokenFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public PasswordResetTokenFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUserIdFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel with specific UserId");
        var expectedUserId = Guid.NewGuid();
        var dataModel = CreateTestDataModel(expectedUserId, "token-hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UserId mapping");
        entity.UserId.Value.ShouldBe(expectedUserId);
    }

    [Fact]
    public void Create_ShouldMapTokenHashFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel with specific TokenHash");
        string expectedTokenHash = Faker.Random.AlphaNumeric(64);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), expectedTokenHash, DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying TokenHash mapping");
        entity.TokenHash.ShouldBe(expectedTokenHash);
    }

    [Fact]
    public void Create_ShouldMapExpiresAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel with specific ExpiresAt");
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddHours(48);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "token-hash", expectedExpiresAt, false, null);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying ExpiresAt mapping");
        entity.ExpiresAt.ShouldBe(expectedExpiresAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldMapIsUsedFromDataModel(bool expectedIsUsed)
    {
        // Arrange
        LogArrange($"Creating PasswordResetTokenDataModel with IsUsed={expectedIsUsed}");
        var usedAt = expectedIsUsed ? DateTimeOffset.UtcNow.AddMinutes(-5) : (DateTimeOffset?)null;
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), expectedIsUsed, usedAt);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying IsUsed mapped to {expectedIsUsed}");
        entity.IsUsed.ShouldBe(expectedIsUsed);
    }

    [Fact]
    public void Create_ShouldMapUsedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel with specific UsedAt");
        var expectedUsedAt = DateTimeOffset.UtcNow.AddMinutes(-15);
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), true, expectedUsedAt);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying UsedAt mapping");
        entity.UsedAt.ShouldBe(expectedUsedAt);
    }

    [Fact]
    public void Create_ShouldMapNullUsedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel with null UsedAt");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying null UsedAt mapping");
        entity.UsedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel with specific base fields");
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

        var dataModel = new PasswordResetTokenDataModel
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
            TokenHash = "token-hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            IsUsed = false,
            UsedAt = null
        };

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

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
        LogArrange("Creating PasswordResetTokenDataModel with null last-changed fields");
        var dataModel = new PasswordResetTokenDataModel
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
            TokenHash = "token-hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            IsUsed = false,
            UsedAt = null
        };

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel with nulls");
        var entity = PasswordResetTokenFactory.Create(dataModel);

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
        LogArrange("Creating PasswordResetTokenDataModel to verify CreatedCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel to verify CreatedExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating PasswordResetTokenDataModel to verify CreatedBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel(Guid.NewGuid(), "token-hash", DateTimeOffset.UtcNow.AddHours(1), false, null);

        // Act
        LogAct("Creating PasswordResetToken from PasswordResetTokenDataModel");
        var entity = PasswordResetTokenFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static PasswordResetTokenDataModel CreateTestDataModel(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        bool isUsed,
        DateTimeOffset? usedAt)
    {
        return new PasswordResetTokenDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_PASSWORD_RESET_TOKEN",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            IsUsed = isUsed,
            UsedAt = usedAt
        };
    }

    #endregion
}
