using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
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

    #endregion
}
