using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Inputs;
using ShopDemo.Auth.Domain.Entities.UserConsents;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;
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

    [Fact]
    public async Task RecordConsentAsync_WhenRegisterNewReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with IP address exceeding max length to trigger RegisterNew failure");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var consentTermId = Id.GenerateNewId();

        _consentTermRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userConsentRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndConsentTermIdAsync(executionContext, userId, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserConsent?)null);

        // Act
        LogAct("Recording consent with invalid IP address (exceeds max length 45)");
        var result = await _sut.RecordConsentAsync(executionContext, userId, consentTermId, new string('x', 46), CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned when RegisterNew fails validation");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task RecordConsentAsync_WhenRegistrationFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repositories where registration fails");
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
            .ReturnsAsync(false);

        // Act
        LogAct("Recording consent when registration fails");
        var result = await _sut.RecordConsentAsync(executionContext, userId, consentTermId, "127.0.0.1", CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
    }

    #endregion

    #region CheckPendingConsentsAsync Tests (with terms)

    [Fact]
    public async Task CheckPendingConsentsAsync_WhenTermsExistAndNoConsent_ShouldReturnPendingTerms()
    {
        // Arrange
        LogArrange("Setting up with existing terms but no user consents");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var tousTerm = CreateTestConsentTerm(ConsentTermType.TermsOfUse, "1.0");

        _userConsentRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserConsent>());

        _consentTermRepositoryMock
            .Setup(x => x.GetLatestByTypeAsync(executionContext, ConsentTermType.TermsOfUse, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tousTerm);

        _consentTermRepositoryMock
            .Setup(x => x.GetLatestByTypeAsync(executionContext, ConsentTermType.PrivacyPolicy, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsentTerm?)null);

        _consentTermRepositoryMock
            .Setup(x => x.GetLatestByTypeAsync(executionContext, ConsentTermType.Marketing, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsentTerm?)null);

        // Act
        LogAct("Checking pending consents");
        var result = await _sut.CheckPendingConsentsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying pending term returned");
        result.Count.ShouldBe(1);
        result[0].ShouldBeSameAs(tousTerm);
    }

    [Fact]
    public async Task CheckPendingConsentsAsync_WhenUserHasActiveConsent_ShouldExcludeFromPending()
    {
        // Arrange
        LogArrange("Setting up with terms and active user consent");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var tousTerm = CreateTestConsentTerm(ConsentTermType.TermsOfUse, "1.0");

        var activeConsent = UserConsent.RegisterNew(
            executionContext,
            new ShopDemo.Auth.Domain.Entities.UserConsents.Inputs.RegisterNewUserConsentInput(
                userId, tousTerm.EntityInfo.Id, "127.0.0.1"));

        _userConsentRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserConsent> { activeConsent! });

        _consentTermRepositoryMock
            .Setup(x => x.GetLatestByTypeAsync(executionContext, ConsentTermType.TermsOfUse, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tousTerm);

        _consentTermRepositoryMock
            .Setup(x => x.GetLatestByTypeAsync(executionContext, ConsentTermType.PrivacyPolicy, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsentTerm?)null);

        _consentTermRepositoryMock
            .Setup(x => x.GetLatestByTypeAsync(executionContext, ConsentTermType.Marketing, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsentTerm?)null);

        // Act
        LogAct("Checking pending consents for user with active consent");
        var result = await _sut.CheckPendingConsentsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list (consent already given)");
        result.ShouldBeEmpty();
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

    [Fact]
    public async Task RevokeConsentAsync_WhenValid_ShouldReturnRevokedConsent()
    {
        // Arrange
        LogArrange("Setting up with active consent that can be revoked");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var consentTermId = Id.GenerateNewId();

        var existingConsent = UserConsent.RegisterNew(
            executionContext,
            new ShopDemo.Auth.Domain.Entities.UserConsents.Inputs.RegisterNewUserConsentInput(userId, consentTermId, "127.0.0.1"));

        _userConsentRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndConsentTermIdAsync(executionContext, userId, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConsent);

        _userConsentRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<UserConsent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Revoking active consent");
        var result = await _sut.RevokeConsentAsync(executionContext, userId, consentTermId, CancellationToken.None);

        // Assert
        LogAssert("Verifying revoked consent returned");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(UserConsentStatus.Revoked);
    }

    [Fact]
    public async Task RevokeConsentAsync_WhenRevokeReturnsNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with consent from different tenant (Revoke returns null)");
        var executionContext = CreateTestExecutionContext();
        var differentContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var consentTermId = Id.GenerateNewId();

        var existingConsent = UserConsent.RegisterNew(
            differentContext,
            new ShopDemo.Auth.Domain.Entities.UserConsents.Inputs.RegisterNewUserConsentInput(userId, consentTermId, "127.0.0.1"));

        _userConsentRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndConsentTermIdAsync(executionContext, userId, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConsent);

        // Act
        LogAct("Revoking consent when Revoke returns null due to tenant mismatch");
        var result = await _sut.RevokeConsentAsync(executionContext, userId, consentTermId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
        _userConsentRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, It.IsAny<UserConsent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeConsentAsync_WhenUpdateFails_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up with active consent where update fails");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var consentTermId = Id.GenerateNewId();

        var existingConsent = UserConsent.RegisterNew(
            executionContext,
            new ShopDemo.Auth.Domain.Entities.UserConsents.Inputs.RegisterNewUserConsentInput(userId, consentTermId, "127.0.0.1"));

        _userConsentRepositoryMock
            .Setup(x => x.GetActiveByUserIdAndConsentTermIdAsync(executionContext, userId, consentTermId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConsent);

        _userConsentRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, It.IsAny<UserConsent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Revoking consent when update fails");
        var result = await _sut.RevokeConsentAsync(executionContext, userId, consentTermId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned");
        result.ShouldBeNull();
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

    private static ConsentTerm CreateTestConsentTerm(ConsentTermType type, string version)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow, createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(), createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null, lastChangedBy: null,
                lastChangedCorrelationId: null, lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return ConsentTerm.CreateFromExistingInfo(new CreateFromExistingInfoConsentTermInput(
            entityInfo, type, version, "Test consent content", DateTimeOffset.UtcNow));
    }

    #endregion
}
