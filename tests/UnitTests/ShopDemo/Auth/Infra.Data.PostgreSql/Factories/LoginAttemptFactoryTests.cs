using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class LoginAttemptFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public LoginAttemptFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUsernameFromDataModel()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel with specific username");
        string expectedUsername = Faker.Internet.UserName();
        var dataModel = CreateTestDataModel(expectedUsername, "192.168.1.1", DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying Username mapping");
        entity.Username.ShouldBe(expectedUsername);
    }

    [Fact]
    public void Create_ShouldMapIpAddressFromDataModel()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel with specific IpAddress");
        string expectedIpAddress = Faker.Internet.Ip();
        var dataModel = CreateTestDataModel("testuser", expectedIpAddress, DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying IpAddress mapping");
        entity.IpAddress.ShouldBe(expectedIpAddress);
    }

    [Fact]
    public void Create_ShouldMapNullIpAddressFromDataModel()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel with null IpAddress");
        var dataModel = CreateTestDataModel("testuser", null, DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying null IpAddress mapping");
        entity.IpAddress.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapAttemptedAtFromDataModel()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel with specific AttemptedAt");
        var expectedAttemptedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var dataModel = CreateTestDataModel("testuser", "192.168.1.1", expectedAttemptedAt, true, null);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying AttemptedAt mapping");
        entity.AttemptedAt.ShouldBe(expectedAttemptedAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldMapIsSuccessfulFromDataModel(bool expectedIsSuccessful)
    {
        // Arrange
        LogArrange($"Creating LoginAttemptDataModel with IsSuccessful={expectedIsSuccessful}");
        var dataModel = CreateTestDataModel("testuser", "192.168.1.1", DateTimeOffset.UtcNow, expectedIsSuccessful, null);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert($"Verifying IsSuccessful mapped to {expectedIsSuccessful}");
        entity.IsSuccessful.ShouldBe(expectedIsSuccessful);
    }

    [Fact]
    public void Create_ShouldMapFailureReasonFromDataModel()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel with specific FailureReason");
        string expectedFailureReason = "Invalid credentials";
        var dataModel = CreateTestDataModel("testuser", "192.168.1.1", DateTimeOffset.UtcNow, false, expectedFailureReason);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying FailureReason mapping");
        entity.FailureReason.ShouldBe(expectedFailureReason);
    }

    [Fact]
    public void Create_ShouldMapEntityInfoFieldsFromDataModel()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel with specific base fields");
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

        var dataModel = new LoginAttemptDataModel
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
            Username = "testuser",
            IpAddress = "192.168.1.1",
            AttemptedAt = DateTimeOffset.UtcNow,
            IsSuccessful = true,
            FailureReason = null
        };

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

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
        LogArrange("Creating LoginAttemptDataModel with null last-changed fields");
        var dataModel = new LoginAttemptDataModel
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
            Username = "testuser",
            IpAddress = null,
            AttemptedAt = DateTimeOffset.UtcNow,
            IsSuccessful = true,
            FailureReason = null
        };

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel with nulls");
        var entity = LoginAttemptFactory.Create(dataModel);

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
        LogArrange("Creating LoginAttemptDataModel to verify CreatedCorrelationId is mapped from data model");
        var dataModel = CreateTestDataModel("testuser", "192.168.1.1", DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedCorrelationId matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(dataModel.CreatedCorrelationId);
    }

    [Fact]
    public void Create_ShouldMapCreatedExecutionOriginFromDataModel()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel to verify CreatedExecutionOrigin is mapped from data model");
        var dataModel = CreateTestDataModel("testuser", "192.168.1.1", DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedExecutionOrigin matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedExecutionOrigin.ShouldBe(dataModel.CreatedExecutionOrigin);
    }

    [Fact]
    public void Create_ShouldMapCreatedBusinessOperationCodeFromDataModel()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel to verify CreatedBusinessOperationCode is mapped from data model");
        var dataModel = CreateTestDataModel("testuser", "192.168.1.1", DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttempt from LoginAttemptDataModel");
        var entity = LoginAttemptFactory.Create(dataModel);

        // Assert
        LogAssert("Verifying CreatedBusinessOperationCode matches data model");
        entity.EntityInfo.EntityChangeInfo.CreatedBusinessOperationCode.ShouldBe(dataModel.CreatedBusinessOperationCode);
    }

    #region Helper Methods

    private static LoginAttemptDataModel CreateTestDataModel(
        string username,
        string? ipAddress,
        DateTimeOffset attemptedAt,
        bool isSuccessful,
        string? failureReason)
    {
        return new LoginAttemptDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedCorrelationId = Guid.NewGuid(),
            CreatedExecutionOrigin = "UnitTest",
            CreatedBusinessOperationCode = "CREATE_LOGIN_ATTEMPT",
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedCorrelationId = null,
            LastChangedExecutionOrigin = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = 1,
            Username = username,
            IpAddress = ipAddress,
            AttemptedAt = attemptedAt,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason
        };
    }

    #endregion
}
