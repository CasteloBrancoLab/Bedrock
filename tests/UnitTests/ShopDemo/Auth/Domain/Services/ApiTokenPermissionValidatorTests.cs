using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.Claims.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class ApiTokenPermissionValidatorTests : TestBase
{
    private readonly Mock<IClaimResolver> _claimResolverMock;
    private readonly Mock<IClaimRepository> _claimRepositoryMock;
    private readonly ApiTokenPermissionValidator _sut;

    public ApiTokenPermissionValidatorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _claimResolverMock = new Mock<IClaimResolver>();
        _claimRepositoryMock = new Mock<IClaimRepository>();
        _sut = new ApiTokenPermissionValidator(_claimResolverMock.Object, _claimRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullClaimResolver_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating ApiTokenPermissionValidator with null claim resolver");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new ApiTokenPermissionValidator(null!, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullClaimRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating ApiTokenPermissionValidator with null claim repository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(() => new ApiTokenPermissionValidator(_claimResolverMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIApiTokenPermissionValidator()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IApiTokenPermissionValidator>();
    }

    #endregion

    #region ValidatePermissionCeilingAsync Tests

    [Fact]
    public async Task ValidatePermissionCeilingAsync_WithEmptyRequestedClaims_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up with empty requested claims");
        var executionContext = CreateTestExecutionContext();
        var creatorUserId = Id.GenerateNewId();
        var requestedClaims = new Dictionary<Id, ClaimValue>();

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, creatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());

        // Act
        LogAct("Validating with no requested claims");
        var result = await _sut.ValidatePermissionCeilingAsync(executionContext, creatorUserId, requestedClaims, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidatePermissionCeilingAsync_WhenClaimNotFound_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up with unknown claim ID");
        var executionContext = CreateTestExecutionContext();
        var creatorUserId = Id.GenerateNewId();
        var unknownClaimId = Id.GenerateNewId();
        var requestedClaims = new Dictionary<Id, ClaimValue> { [unknownClaimId] = ClaimValue.Granted };

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, creatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim>());

        // Act
        LogAct("Validating with unknown claim");
        var result = await _sut.ValidatePermissionCeilingAsync(executionContext, creatorUserId, requestedClaims, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidatePermissionCeilingAsync_WhenCreatorLacksPermission_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up creator without the requested claim");
        var executionContext = CreateTestExecutionContext();
        var creatorUserId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var requestedClaims = new Dictionary<Id, ClaimValue> { [claimId] = ClaimValue.Granted };

        var claim = CreateTestClaim(claimId, "admin_access");

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, creatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue>());

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claim });

        // Act
        LogAct("Validating when creator lacks the requested claim");
        var result = await _sut.ValidatePermissionCeilingAsync(executionContext, creatorUserId, requestedClaims, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidatePermissionCeilingAsync_WhenRequestedExceedsCreatorPermission_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up creator with lower permission than requested");
        var executionContext = CreateTestExecutionContext();
        var creatorUserId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var requestedClaims = new Dictionary<Id, ClaimValue> { [claimId] = ClaimValue.Granted };

        var claim = CreateTestClaim(claimId, "admin_access");

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, creatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue> { ["admin_access"] = ClaimValue.Denied });

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claim });

        // Act
        LogAct("Validating when requested exceeds creator permission");
        var result = await _sut.ValidatePermissionCeilingAsync(executionContext, creatorUserId, requestedClaims, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false with error");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidatePermissionCeilingAsync_WhenValid_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up creator with sufficient permissions");
        var executionContext = CreateTestExecutionContext();
        var creatorUserId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();
        var requestedClaims = new Dictionary<Id, ClaimValue> { [claimId] = ClaimValue.Granted };

        var claim = CreateTestClaim(claimId, "admin_access");

        _claimResolverMock
            .Setup(x => x.ResolveUserClaimsAsync(executionContext, creatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, ClaimValue> { ["admin_access"] = ClaimValue.Granted });

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claim });

        // Act
        LogAct("Validating with sufficient permissions");
        var result = await _sut.ValidatePermissionCeilingAsync(executionContext, creatorUserId, requestedClaims, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true");
        result.ShouldBeTrue();
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

    private static Claim CreateTestClaim(Id claimId, string name)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(claimId.Value),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow, createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(), createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null, lastChangedBy: null,
                lastChangedCorrelationId: null, lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        return Claim.CreateFromExistingInfo(new CreateFromExistingInfoClaimInput(entityInfo, name, null));
    }

    #endregion
}
