using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies.Inputs;
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

    [Fact]
    public async Task ValidateNoCircularDependencyAsync_WhenIndirectCircularDependency_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up chain: A -> B -> A (circular through B)");
        var executionContext = CreateTestExecutionContext();
        var roleA = Id.GenerateNewId();
        var roleB = Id.GenerateNewId();

        // B has parent A
        var hierarchyBA = RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(CreateTestEntityInfo(), roleB, roleA));

        // When checking B's parents, return A
        _roleHierarchyRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchyBA });

        // Act — trying to add A -> B (which would create A -> B -> A cycle)
        LogAct("Validating circular dependency through chain");
        var result = await _sut.ValidateNoCircularDependencyAsync(executionContext, roleA, roleB, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns false (circular dependency detected)");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateNoCircularDependencyAsync_WhenDeepChainWithNoCycle_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up chain: A -> B -> C (no cycle)");
        var executionContext = CreateTestExecutionContext();
        var roleA = Id.GenerateNewId();
        var roleB = Id.GenerateNewId();
        var roleC = Id.GenerateNewId();

        // B has parent C
        var hierarchyBC = RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(CreateTestEntityInfo(), roleB, roleC));

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchyBC });

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleC, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy>());

        // Act — trying to add A -> B (no cycle since C doesn't lead back to A)
        LogAct("Validating no circular dependency in deep chain");
        var result = await _sut.ValidateNoCircularDependencyAsync(executionContext, roleA, roleB, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true (no cycle)");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateNoCircularDependencyAsync_WhenDiamondGraph_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up diamond graph: A -> B, B has parents C and D, both C and D have parent E");
        var executionContext = CreateTestExecutionContext();
        var roleA = Id.GenerateNewId();
        var roleB = Id.GenerateNewId();
        var roleC = Id.GenerateNewId();
        var roleD = Id.GenerateNewId();
        var roleE = Id.GenerateNewId();

        // B has parents C and D
        var hierarchyBC = RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(CreateTestEntityInfo(), roleB, roleC));
        var hierarchyBD = RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(CreateTestEntityInfo(), roleB, roleD));

        // C has parent E
        var hierarchyCE = RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(CreateTestEntityInfo(), roleC, roleE));

        // D has parent E (same E, causing visited.Add to return false on second visit)
        var hierarchyDE = RoleHierarchy.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleHierarchyInput(CreateTestEntityInfo(), roleD, roleE));

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchyBC, hierarchyBD });

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleC, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchyCE });

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleD, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchyDE });

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleE, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy>());

        // Act — adding A -> B, traversing B -> C -> E, then B -> D -> E (E already visited)
        LogAct("Validating diamond graph where E is visited through two paths");
        var result = await _sut.ValidateNoCircularDependencyAsync(executionContext, roleA, roleB, CancellationToken.None);

        // Assert
        LogAssert("Verifying returns true (no cycle, just diamond convergence)");
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

    private static EntityInfo CreateTestEntityInfo()
    {
        return EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow,
                createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(),
                createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null,
                lastChangedBy: null,
                lastChangedCorrelationId: null,
                lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));
    }

    #endregion
}
