using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class ApiTokenExpirationManagerTests : TestBase
{
    private readonly ApiTokenExpirationManager _sut;

    public ApiTokenExpirationManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _sut = new ApiTokenExpirationManager();
    }

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIApiTokenExpirationManager()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IApiTokenExpirationManager>();
    }

    #endregion

    #region CalculateExpiration Tests

    [Fact]
    public void CalculateExpiration_WithNullTtl_ShouldUseDefault90Days()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Calculating expiration with null TTL");
        var result = _sut.CalculateExpiration(executionContext, null);

        // Assert
        LogAssert("Verifying expiration is 90 days from now");
        var expected = executionContext.Timestamp.AddDays(ApiTokenExpirationManager.DefaultTtlDays);
        result.ShouldBe(expected);
    }

    [Fact]
    public void CalculateExpiration_WithValidTtl_ShouldUseRequestedDays()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Calculating expiration with 30 days TTL");
        var result = _sut.CalculateExpiration(executionContext, 30);

        // Assert
        LogAssert("Verifying expiration is 30 days from now");
        var expected = executionContext.Timestamp.AddDays(30);
        result.ShouldBe(expected);
    }

    [Fact]
    public void CalculateExpiration_WithTtlLessThan1_ShouldFallbackToDefault()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Calculating expiration with TTL of 0");
        var result = _sut.CalculateExpiration(executionContext, 0);

        // Assert
        LogAssert("Verifying error message added and default TTL used");
        executionContext.HasErrorMessages.ShouldBeTrue();
        var expected = executionContext.Timestamp.AddDays(ApiTokenExpirationManager.DefaultTtlDays);
        result.ShouldBe(expected);
    }

    [Fact]
    public void CalculateExpiration_WithTtlExceedingMax_ShouldCapAtMax()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Calculating expiration with TTL exceeding maximum");
        var result = _sut.CalculateExpiration(executionContext, 500);

        // Assert
        LogAssert("Verifying error message added and max TTL used");
        executionContext.HasErrorMessages.ShouldBeTrue();
        var expected = executionContext.Timestamp.AddDays(ApiTokenExpirationManager.MaxTtlDays);
        result.ShouldBe(expected);
    }

    #endregion

    #region IsExpired Tests

    [Fact]
    public void IsExpired_WithNoExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating service client with no expiration");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(expiresAt: null);

        // Act
        LogAct("Checking if client is expired");
        var result = _sut.IsExpired(executionContext, serviceClient);

        // Assert
        LogAssert("Verifying returns false");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpired_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating service client that expired yesterday");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(expiresAt: executionContext.Timestamp.AddDays(-1));

        // Act
        LogAct("Checking if expired client is expired");
        var result = _sut.IsExpired(executionContext, serviceClient);

        // Assert
        LogAssert("Verifying returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating service client that expires tomorrow");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(expiresAt: executionContext.Timestamp.AddDays(1));

        // Act
        LogAct("Checking if non-expired client is expired");
        var result = _sut.IsExpired(executionContext, serviceClient);

        // Assert
        LogAssert("Verifying returns false");
        result.ShouldBeFalse();
    }

    #endregion

    #region IsNearExpiration Tests

    [Fact]
    public void IsNearExpiration_WithNoExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating service client with no expiration");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(expiresAt: null);

        // Act
        LogAct("Checking if client is near expiration");
        var result = _sut.IsNearExpiration(executionContext, serviceClient);

        // Assert
        LogAssert("Verifying returns false");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsNearExpiration_WhenWithinNotificationWindow_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating service client expiring in 3 days (within 7-day window)");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(expiresAt: executionContext.Timestamp.AddDays(3));

        // Act
        LogAct("Checking if client is near expiration");
        var result = _sut.IsNearExpiration(executionContext, serviceClient);

        // Assert
        LogAssert("Verifying returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsNearExpiration_WhenAlreadyExpired_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating service client that already expired");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(expiresAt: executionContext.Timestamp.AddDays(-1));

        // Act
        LogAct("Checking if expired client is near expiration");
        var result = _sut.IsNearExpiration(executionContext, serviceClient);

        // Assert
        LogAssert("Verifying returns false (already expired)");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsNearExpiration_WhenFarFromExpiration_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating service client expiring in 30 days");
        var executionContext = CreateTestExecutionContext();
        var serviceClient = CreateTestServiceClient(expiresAt: executionContext.Timestamp.AddDays(30));

        // Act
        LogAct("Checking if client is near expiration");
        var result = _sut.IsNearExpiration(executionContext, serviceClient);

        // Assert
        LogAssert("Verifying returns false");
        result.ShouldBeFalse();
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    private static ServiceClient CreateTestServiceClient(DateTimeOffset? expiresAt)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow,
                createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(),
                createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null, lastChangedBy: null,
                lastChangedCorrelationId: null, lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return ServiceClient.CreateFromExistingInfo(new CreateFromExistingInfoServiceClientInput(
            entityInfo, "test-client", new byte[32], "Test Client",
            ServiceClientStatus.Active, Id.GenerateNewId(), expiresAt, null));
    }

    #endregion
}
