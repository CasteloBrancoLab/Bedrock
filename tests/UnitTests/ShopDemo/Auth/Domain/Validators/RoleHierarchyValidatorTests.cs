using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Validators;
using ShopDemo.Auth.Domain.Validators.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Validators;

public class RoleHierarchyValidatorTests : TestBase
{
    private readonly Mock<IRoleHierarchyRepository> _roleHierarchyRepositoryMock;
    private readonly RoleHierarchyValidator _sut;

    public RoleHierarchyValidatorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _roleHierarchyRepositoryMock = new Mock<IRoleHierarchyRepository>();
        _sut = new RoleHierarchyValidator(_roleHierarchyRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null role hierarchy repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new RoleHierarchyValidator(null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIRoleHierarchyValidator()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IRoleHierarchyValidator>();
    }

    #endregion

    #region ValidateNoCircularDependencyAsync Tests

    [Fact]
    public async Task ValidateNoCircularDependencyAsync_WhenSameRoleAndParent_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up with roleId equal to parentRoleId");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();

        // Act
        LogAct("Validating circular dependency (same role)");
        var result = await _sut.ValidateNoCircularDependencyAsync(executionContext, roleId, roleId, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false (direct circular reference)");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateNoCircularDependencyAsync_WhenNoCircularDependency_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up with no circular dependencies");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.GenerateNewId();
        var parentRoleId = Id.GenerateNewId();

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, parentRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy>());

        // Act
        LogAct("Validating no circular dependency");
        var result = await _sut.ValidateNoCircularDependencyAsync(executionContext, roleId, parentRoleId, CancellationToken.None);

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
