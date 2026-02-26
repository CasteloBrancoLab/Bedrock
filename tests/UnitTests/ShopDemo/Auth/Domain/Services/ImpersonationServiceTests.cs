using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Resolvers.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class ImpersonationServiceTests : TestBase
{
    private readonly Mock<IImpersonationSessionRepository> _impersonationSessionRepositoryMock;
    private readonly Mock<IClaimResolver> _claimResolverMock;
    private readonly ImpersonationService _sut;

    public ImpersonationServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _impersonationSessionRepositoryMock = new Mock<IImpersonationSessionRepository>();
        _claimResolverMock = new Mock<IClaimResolver>();
        _sut = new ImpersonationService(_impersonationSessionRepositoryMock.Object, _claimResolverMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullSessionRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating ImpersonationService with null session repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new ImpersonationService(null!, _claimResolverMock.Object));
    }

    [Fact]
    public void Constructor_WithNullClaimResolver_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating ImpersonationService with null claim resolver");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new ImpersonationService(_impersonationSessionRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIImpersonationService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IImpersonationService>();
    }

    #endregion

    #region ValidateAndCreateAsync Tests

    [Fact]
    public async Task ValidateAndCreateAsync_WhenOperatorLacksPermission_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up claim resolver with no can_impersonate claim");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.GenerateNewId();
        var targetUserId = Id.GenerateNewId();

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        // Act
        LogAct("Validating impersonation without permission");
        var result = await _sut.ValidateAndCreateAsync(executionContext, operatorUserId, targetUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with error");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateAndCreateAsync_WhenTargetExplicitlyDenied_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up claims where operator can impersonate but target is denied");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.GenerateNewId();
        var targetUserId = Id.GenerateNewId();

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue> { ["can_impersonate"] = ClaimValue.Granted });

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue> { ["is_impersonatable"] = ClaimValue.Denied });

        // Act
        LogAct("Validating impersonation of denied target");
        var result = await _sut.ValidateAndCreateAsync(executionContext, operatorUserId, targetUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with error");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateAndCreateAsync_WhenChainImpersonation_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up claims and active session for chain impersonation");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.GenerateNewId();
        var targetUserId = Id.GenerateNewId();

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue> { ["can_impersonate"] = ClaimValue.Granted });

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        var activeSession = ImpersonationSession.RegisterNew(
            executionContext,
            new ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Inputs.RegisterNewImpersonationSessionInput(
                Id.GenerateNewId(), operatorUserId, executionContext.Timestamp.AddMinutes(30)));

        _impersonationSessionRepositoryMock
            .Setup(x => x.GetActiveByTargetUserIdAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeSession);

        // Act
        LogAct("Validating chain impersonation");
        var result = await _sut.ValidateAndCreateAsync(executionContext, operatorUserId, targetUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null returned with chain impersonation error");
        result.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateAndCreateAsync_WhenValid_ShouldReturnSession()
    {
        // Arrange
        LogArrange("Setting up valid impersonation scenario");
        var executionContext = CreateTestExecutionContext();
        var operatorUserId = Id.GenerateNewId();
        var targetUserId = Id.GenerateNewId();

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue> { ["can_impersonate"] = ClaimValue.Granted });

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        _impersonationSessionRepositoryMock
            .Setup(x => x.GetActiveByTargetUserIdAsync(executionContext, operatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImpersonationSession?)null);

        _impersonationSessionRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, It.IsAny<ImpersonationSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Validating and creating impersonation session");
        var result = await _sut.ValidateAndCreateAsync(executionContext, operatorUserId, targetUserId, CancellationToken.None);

        // Assert
        LogAssert("Verifying session was created");
        result.ShouldNotBeNull();
    }

    #endregion

    #region EndSessionAsync Tests

    [Fact]
    public async Task EndSessionAsync_WhenSessionNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository with no session");
        var executionContext = CreateTestExecutionContext();
        var sessionId = Id.GenerateNewId();

        _impersonationSessionRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImpersonationSession?)null);

        // Act
        LogAct("Ending non-existent session");
        var result = await _sut.EndSessionAsync(executionContext, sessionId, CancellationToken.None);

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

    #endregion
}
