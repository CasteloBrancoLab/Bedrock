using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Validators;
using ShopDemo.Auth.Domain.Validators.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Validators;

public class ClaimDependencyValidatorTests : TestBase
{
    private readonly Mock<IClaimDependencyRepository> _claimDependencyRepositoryMock;
    private readonly Mock<IRoleClaimRepository> _roleClaimRepositoryMock;
    private readonly ClaimDependencyValidator _sut;

    public ClaimDependencyValidatorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _claimDependencyRepositoryMock = new Mock<IClaimDependencyRepository>();
        _roleClaimRepositoryMock = new Mock<IRoleClaimRepository>();
        _sut = new ClaimDependencyValidator(_claimDependencyRepositoryMock.Object, _roleClaimRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullClaimDependencyRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null claim dependency repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new ClaimDependencyValidator(null!, _roleClaimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullRoleClaimRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null role claim repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new ClaimDependencyValidator(_claimDependencyRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIClaimDependencyValidator()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IClaimDependencyValidator>();
    }

    #endregion

    #region ValidateClaimDependenciesAsync Tests

    [Fact]
    public async Task ValidateClaimDependenciesAsync_WhenValueNotGranted_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up with denied claim value");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();

        // Act
        LogAct("Validating dependencies for non-granted claim");
        var result = await _sut.ValidateClaimDependenciesAsync(executionContext, roleId, claimId, ClaimValue.Denied, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true immediately (no deps check needed)");
        result.ShouldBeTrue();
        _claimDependencyRepositoryMock.Verify(
            x => x.GetByClaimIdAsync(It.IsAny<ExecutionContext>(), It.IsAny<Id>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateClaimDependenciesAsync_WhenGrantedAndNoDependencies_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up with granted claim and no dependencies");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var claimId = Id.GenerateNewId();

        _claimDependencyRepositoryMock
            .Setup(x => x.GetByClaimIdAsync(executionContext, claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClaimDependency>());

        // Act
        LogAct("Validating dependencies for granted claim with no deps");
        var result = await _sut.ValidateClaimDependenciesAsync(executionContext, roleId, claimId, ClaimValue.Granted, CancellationToken.None);

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

    #endregion
}
