using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;
using ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Factories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Factories;

public class LoginAttemptDataModelFactoryTests : TestBase
{
    private static readonly Faker Faker = new();

    public LoginAttemptDataModelFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Create_ShouldMapUsernameCorrectly()
    {
        // Arrange
        LogArrange("Creating LoginAttempt entity with known username");
        string expectedUsername = Faker.Internet.UserName();
        var entity = CreateTestEntity(expectedUsername, "192.168.1.1", DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttemptDataModel from LoginAttempt entity");
        var dataModel = LoginAttemptDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying Username mapping");
        dataModel.Username.ShouldBe(expectedUsername);
    }

    [Fact]
    public void Create_ShouldMapIpAddressCorrectly()
    {
        // Arrange
        LogArrange("Creating LoginAttempt entity with known IpAddress");
        string expectedIpAddress = Faker.Internet.Ip();
        var entity = CreateTestEntity("testuser", expectedIpAddress, DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttemptDataModel from LoginAttempt entity");
        var dataModel = LoginAttemptDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying IpAddress mapping");
        dataModel.IpAddress.ShouldBe(expectedIpAddress);
    }

    [Fact]
    public void Create_ShouldMapNullIpAddressCorrectly()
    {
        // Arrange
        LogArrange("Creating LoginAttempt entity with null IpAddress");
        var entity = CreateTestEntity("testuser", null, DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttemptDataModel from LoginAttempt entity");
        var dataModel = LoginAttemptDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null IpAddress mapping");
        dataModel.IpAddress.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapAttemptedAtCorrectly()
    {
        // Arrange
        LogArrange("Creating LoginAttempt entity with known AttemptedAt");
        var expectedAttemptedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var entity = CreateTestEntity("testuser", "192.168.1.1", expectedAttemptedAt, true, null);

        // Act
        LogAct("Creating LoginAttemptDataModel from LoginAttempt entity");
        var dataModel = LoginAttemptDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying AttemptedAt mapping");
        dataModel.AttemptedAt.ShouldBe(expectedAttemptedAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldMapIsSuccessfulCorrectly(bool expectedIsSuccessful)
    {
        // Arrange
        LogArrange($"Creating LoginAttempt entity with IsSuccessful={expectedIsSuccessful}");
        var entity = CreateTestEntity("testuser", "192.168.1.1", DateTimeOffset.UtcNow, expectedIsSuccessful, null);

        // Act
        LogAct("Creating LoginAttemptDataModel from LoginAttempt entity");
        var dataModel = LoginAttemptDataModelFactory.Create(entity);

        // Assert
        LogAssert($"Verifying IsSuccessful mapping to {expectedIsSuccessful}");
        dataModel.IsSuccessful.ShouldBe(expectedIsSuccessful);
    }

    [Fact]
    public void Create_ShouldMapFailureReasonCorrectly()
    {
        // Arrange
        LogArrange("Creating LoginAttempt entity with known FailureReason");
        string expectedFailureReason = "Invalid password";
        var entity = CreateTestEntity("testuser", "192.168.1.1", DateTimeOffset.UtcNow, false, expectedFailureReason);

        // Act
        LogAct("Creating LoginAttemptDataModel from LoginAttempt entity");
        var dataModel = LoginAttemptDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying FailureReason mapping");
        dataModel.FailureReason.ShouldBe(expectedFailureReason);
    }

    [Fact]
    public void Create_ShouldMapNullFailureReasonCorrectly()
    {
        // Arrange
        LogArrange("Creating LoginAttempt entity with null FailureReason");
        var entity = CreateTestEntity("testuser", "192.168.1.1", DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Creating LoginAttemptDataModel from LoginAttempt entity");
        var dataModel = LoginAttemptDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying null FailureReason mapping");
        dataModel.FailureReason.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldMapBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating LoginAttempt entity with specific EntityInfo values");
        var entityId = Guid.NewGuid();
        var tenantCode = Guid.NewGuid();
        string createdBy = Faker.Person.FullName;
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        long entityVersion = Faker.Random.Long(1);
        string? lastChangedBy = Faker.Person.FullName;
        var lastChangedAt = DateTimeOffset.UtcNow;
        var lastChangedCorrelationId = Guid.NewGuid();
        string lastChangedExecutionOrigin = "TestOrigin";
        string lastChangedBusinessOperationCode = "TEST_OP";

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(entityId),
            tenantInfo: TenantInfo.Create(tenantCode),
            createdAt: createdAt,
            createdBy: createdBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_CREATE",
            lastChangedAt: lastChangedAt,
            lastChangedBy: lastChangedBy,
            lastChangedCorrelationId: lastChangedCorrelationId,
            lastChangedExecutionOrigin: lastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: lastChangedBusinessOperationCode,
            entityVersion: RegistryVersion.CreateFromExistingInfo(entityVersion));

        var entity = LoginAttempt.CreateFromExistingInfo(
            new CreateFromExistingInfoLoginAttemptInput(
                entityInfo,
                "testuser",
                "192.168.1.1",
                DateTimeOffset.UtcNow,
                true,
                null));

        // Act
        LogAct("Creating LoginAttemptDataModel from LoginAttempt entity");
        var dataModel = LoginAttemptDataModelFactory.Create(entity);

        // Assert
        LogAssert("Verifying base fields from EntityInfo");
        dataModel.Id.ShouldBe(entityId);
        dataModel.TenantCode.ShouldBe(tenantCode);
        dataModel.CreatedBy.ShouldBe(createdBy);
        dataModel.CreatedAt.ShouldBe(createdAt);
        dataModel.EntityVersion.ShouldBe(entityVersion);
        dataModel.LastChangedBy.ShouldBe(lastChangedBy);
        dataModel.LastChangedAt.ShouldBe(lastChangedAt);
        dataModel.LastChangedCorrelationId.ShouldBe(lastChangedCorrelationId);
        dataModel.LastChangedExecutionOrigin.ShouldBe(lastChangedExecutionOrigin);
        dataModel.LastChangedBusinessOperationCode.ShouldBe(lastChangedBusinessOperationCode);
    }

    #region Helper Methods

    private static LoginAttempt CreateTestEntity(
        string username,
        string? ipAddress,
        DateTimeOffset attemptedAt,
        bool isSuccessful,
        string? failureReason)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid()),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "test-creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return LoginAttempt.CreateFromExistingInfo(
            new CreateFromExistingInfoLoginAttemptInput(
                entityInfo,
                username,
                ipAddress,
                attemptedAt,
                isSuccessful,
                failureReason));
    }

    #endregion
}
