using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Resolvers.Interfaces;
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
