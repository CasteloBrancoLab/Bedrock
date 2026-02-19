using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.UserConsents;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class ConsentManagerTests : TestBase
{
    private readonly Mock<IConsentTermRepository> _consentTermRepositoryMock;
    private readonly Mock<IUserConsentRepository> _userConsentRepositoryMock;
    private readonly ConsentManager _sut;

    public ConsentManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _consentTermRepositoryMock = new Mock<IConsentTermRepository>();
        _userConsentRepositoryMock = new Mock<IUserConsentRepository>();
        _sut = new ConsentManager(_consentTermRepositoryMock.Object, _userConsentRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullConsentTermRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating ConsentManager with null consent term repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new ConsentManager(null!, _userConsentRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullUserConsentRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating ConsentManager with null user consent repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new ConsentManager(_consentTermRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIConsentManager()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IConsentManager>();
    }

    #endregion

    #region CheckPendingConsentsAsync Tests

    [Fact]
    public async Task CheckPendingConsentsAsync_WhenNoTermsExist_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up repositories with no consent terms");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _userConsentRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserConsent>());

        _consentTermRepositoryMock
            .Setup(x => x.GetLatestByTypeAsync(executionContext, It.IsAny<ConsentTermType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsentTerm?)null);

        // Act
        LogAct("Checking pending consents");
        var result = await _sut.CheckPendingConsentsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list returned");
        result.ShouldBeEmpty();
    }

    #endregion

    #region RecordConsentAsync Tests

    [Fact]
    public async Task RecordConsentAsync_WhenConsentTermNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository with no consent term");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var consentTermId = Id.GenerateNewId();

        _consentTermRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Recording consent for non-existent term");
        var result = await _sut.RecordConsentAsync(executionContext, userId, consentTermId, "127.0.0.1", CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with error message");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task RecordConsentAsync_WhenConsentAlreadyActive_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository with existing active consent");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var consentTermId = Id.GenerateNewId();

        _consentTermRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var existingConsent = UserConsent.RegisterNew(
            executionContext,
            new ShopDemo.Auth.Domain.Entities.UserConsents.Inputs.RegisterNewUserConsentInput(userId, consentTermId, "127.0.0.1"));

        _userConsentRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndConsentTermIdAsync(executionContext, userId, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConsent);

        // Act
        LogAct("Recording consent that already exists");
        var result = await _sut.RecordConsentAsync(executionContext, userId, consentTermId, "127.0.0.1", CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with error message");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RecordConsentAsync_WhenValid_ShouldReturnUserConsent()
    {
        // Arrange
        LogArrange("Setting up repositories for valid consent recording");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var consentTermId = Id.GenerateNewId();

        _consentTermRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userConsentRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndConsentTermIdAsync(executionContext, userId, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserConsent?)null);

        _userConsentRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<UserConsent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Recording valid consent");
        var result = await _sut.RecordConsentAsync(executionContext, userId, consentTermId, "127.0.0.1", CancellationToken.None);

        // Assert
        LogAssert("Verifying consent was recorded");
        result.ShouldNotBeNull();
    }

    #endregion

    #region RevokeConsentAsync Tests

    [Fact]
    public async Task RevokeConsentAsync_WhenNoActiveConsent_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository with no active consent");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var consentTermId = Id.GenerateNewId();

        _userConsentRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndConsentTermIdAsync(executionContext, userId, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserConsent?)null);

        // Act
        LogAct("Revoking non-existent consent");
        var result = await _sut.RevokeConsentAsync(executionContext, userId, consentTermId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with error message");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
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
