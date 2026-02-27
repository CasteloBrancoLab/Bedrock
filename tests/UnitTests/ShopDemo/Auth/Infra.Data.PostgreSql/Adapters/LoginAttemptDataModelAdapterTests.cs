using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Bogus;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;
using ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.Adapters;

public class LoginAttemptDataModelAdapterTests : TestBase
{
    private static readonly Faker Faker = new();

    public LoginAttemptDataModelAdapterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Adapt_ShouldUpdateUsernameFromEntity()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel and LoginAttempt with different usernames");
        var dataModel = CreateTestDataModel();
        string expectedUsername = Faker.Internet.UserName();
        var entity = CreateTestEntity(expectedUsername, "192.168.1.1", DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Adapting data model from entity");
        LoginAttemptDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying Username was updated");
        dataModel.Username.ShouldBe(expectedUsername);
    }

    [Fact]
    public void Adapt_ShouldUpdateIpAddressFromEntity()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel and LoginAttempt with different IpAddresses");
        var dataModel = CreateTestDataModel();
        string expectedIpAddress = Faker.Internet.Ip();
        var entity = CreateTestEntity("testuser", expectedIpAddress, DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Adapting data model from entity");
        LoginAttemptDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IpAddress was updated");
        dataModel.IpAddress.ShouldBe(expectedIpAddress);
    }

    [Fact]
    public void Adapt_ShouldUpdateNullIpAddressFromEntity()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel and LoginAttempt with null IpAddress");
        var dataModel = CreateTestDataModel();
        dataModel.IpAddress = "192.168.0.1";
        var entity = CreateTestEntity("testuser", null, DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Adapting data model from entity");
        LoginAttemptDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IpAddress was updated to null");
        dataModel.IpAddress.ShouldBeNull();
    }

    [Fact]
    public void Adapt_ShouldUpdateAttemptedAtFromEntity()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel and LoginAttempt with different AttemptedAt");
        var dataModel = CreateTestDataModel();
        var expectedAttemptedAt = DateTimeOffset.UtcNow.AddHours(-3);
        var entity = CreateTestEntity("testuser", "192.168.1.1", expectedAttemptedAt, true, null);

        // Act
        LogAct("Adapting data model from entity");
        LoginAttemptDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying AttemptedAt was updated");
        dataModel.AttemptedAt.ShouldBe(expectedAttemptedAt);
    }

    [Fact]
    public void Adapt_ShouldUpdateIsSuccessfulFromEntity()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel and LoginAttempt with different IsSuccessful values");
        var dataModel = CreateTestDataModel();
        dataModel.IsSuccessful = true;
        var entity = CreateTestEntity("testuser", "192.168.1.1", DateTimeOffset.UtcNow, false, "Invalid password");

        // Act
        LogAct("Adapting data model from entity");
        LoginAttemptDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying IsSuccessful was updated");
        dataModel.IsSuccessful.ShouldBeFalse();
    }

    [Fact]
    public void Adapt_ShouldUpdateFailureReasonFromEntity()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel and LoginAttempt with different FailureReasons");
        var dataModel = CreateTestDataModel();
        string expectedFailureReason = "Account locked";
        var entity = CreateTestEntity("testuser", "192.168.1.1", DateTimeOffset.UtcNow, false, expectedFailureReason);

        // Act
        LogAct("Adapting data model from entity");
        LoginAttemptDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying FailureReason was updated");
        dataModel.FailureReason.ShouldBe(expectedFailureReason);
    }

    [Fact]
    public void Adapt_ShouldUpdateBaseFieldsFromEntityInfo()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel and LoginAttempt with different EntityInfo values");
        var dataModel = CreateTestDataModel();
        var expectedId = Guid.NewGuid();
        var expectedTenantCode = Guid.NewGuid();
        string expectedCreatedBy = Faker.Person.FullName;
        var expectedCreatedAt = DateTimeOffset.UtcNow.AddDays(-2);
        long expectedVersion = Faker.Random.Long(1);

        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(expectedId),
            tenantInfo: TenantInfo.Create(expectedTenantCode),
            createdAt: expectedCreatedAt,
            createdBy: expectedCreatedBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "UnitTest",
            createdBusinessOperationCode: "TEST_OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(expectedVersion));

        var entity = LoginAttempt.CreateFromExistingInfo(
            new CreateFromExistingInfoLoginAttemptInput(
                entityInfo,
                "testuser",
                "192.168.1.1",
                DateTimeOffset.UtcNow,
                true,
                null));

        // Act
        LogAct("Adapting data model from entity");
        LoginAttemptDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying base fields were updated from EntityInfo");
        dataModel.Id.ShouldBe(expectedId);
        dataModel.TenantCode.ShouldBe(expectedTenantCode);
        dataModel.CreatedBy.ShouldBe(expectedCreatedBy);
        dataModel.CreatedAt.ShouldBe(expectedCreatedAt);
        dataModel.EntityVersion.ShouldBe(expectedVersion);
    }

    [Fact]
    public void Adapt_ShouldReturnTheSameDataModelInstance()
    {
        // Arrange
        LogArrange("Creating LoginAttemptDataModel and LoginAttempt");
        var dataModel = CreateTestDataModel();
        var entity = CreateTestEntity("testuser", "192.168.1.1", DateTimeOffset.UtcNow, true, null);

        // Act
        LogAct("Adapting data model from entity");
        var result = LoginAttemptDataModelAdapter.Adapt(dataModel, entity);

        // Assert
        LogAssert("Verifying the same instance is returned");
        result.ShouldBeSameAs(dataModel);
    }

    #region Helper Methods

    private static LoginAttemptDataModel CreateTestDataModel()
    {
        return new LoginAttemptDataModel
        {
            Id = Guid.NewGuid(),
            TenantCode = Guid.NewGuid(),
            CreatedBy = "test-creator",
            CreatedAt = DateTimeOffset.UtcNow,
            EntityVersion = 1,
            Username = "initial-user",
            IpAddress = "10.0.0.1",
            AttemptedAt = DateTimeOffset.UtcNow.AddHours(-1),
            IsSuccessful = false,
            FailureReason = "Initial reason"
        };
    }

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
