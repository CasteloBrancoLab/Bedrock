using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class PasswordPolicyServiceTests : TestBase
{
    private readonly Mock<IPasswordHistoryRepository> _passwordHistoryRepositoryMock;
    private readonly Mock<IPasswordBreachChecker> _passwordBreachCheckerMock;
    private readonly PasswordPolicyService _sut;

    public PasswordPolicyServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _passwordHistoryRepositoryMock = new Mock<IPasswordHistoryRepository>();
        _passwordBreachCheckerMock = new Mock<IPasswordBreachChecker>();
        _sut = new PasswordPolicyService(_passwordHistoryRepositoryMock.Object, _passwordBreachCheckerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPasswordHistoryRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating PasswordPolicyService with null history repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new PasswordPolicyService(null!, _passwordBreachCheckerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullPasswordBreachChecker_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating PasswordPolicyService with null breach checker");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new PasswordPolicyService(_passwordHistoryRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIPasswordPolicyService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IPasswordPolicyService>();
    }

    #endregion

    #region ValidatePasswordAsync Tests

    [Fact]
    public async Task ValidatePasswordAsync_WithTooShortPassword_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Preparing password shorter than 12 characters");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating too short password");
        var result = await _sut.ValidatePasswordAsync(executionContext, "short", null, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithTooLongPassword_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Preparing password longer than 128 characters");
        var executionContext = CreateTestExecutionContext();
        var longPassword = new string('a', 129);

        // Act
        LogAct("Validating too long password");
        var result = await _sut.ValidatePasswordAsync(executionContext, longPassword, null, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithBreachedPassword_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up breach checker to report password as breached");
        var executionContext = CreateTestExecutionContext();
        var password = "ValidLength12345";
        _passwordBreachCheckerMock
            .Setup(x => x.IsBreachedAsync(password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Validating breached password");
        var result = await _sut.ValidatePasswordAsync(executionContext, password, null, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with breach error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidatePasswordAsync_WithValidPassword_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up breach checker to report password as safe");
        var executionContext = CreateTestExecutionContext();
        var password = "ValidPassword12345!";
        _passwordBreachCheckerMock
            .Setup(x => x.IsBreachedAsync(password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Validating valid password");
        var result = await _sut.ValidatePasswordAsync(executionContext, password, null, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true");
        result.ShouldBeTrue();
    }

    #endregion

    #region RecordPasswordChangeAsync Tests

    [Fact]
    public async Task RecordPasswordChangeAsync_WhenRepositorySucceeds_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up repository for successful registration");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        _passwordHistoryRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Recording password change");
        var result = await _sut.RecordPasswordChangeAsync(executionContext, userId, "hashed-password", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task RecordPasswordChangeAsync_WhenRepositoryFails_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up repository to fail");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        _passwordHistoryRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Recording password change with repository failure");
        var result = await _sut.RecordPasswordChangeAsync(executionContext, userId, "hashed-password", CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task RecordPasswordChangeAsync_WhenEntityCreationFails_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up with null passwordHash to trigger entity creation failure");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        // Act
        LogAct("Recording password change with null hash");
        var result = await _sut.RecordPasswordChangeAsync(executionContext, userId, null!, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false when PasswordHistory.RegisterNew returns null");
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

    #endregion
}
