using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.Claims.Inputs;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Domain.Entities.RoleClaims.Inputs;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies.Inputs;
using ShopDemo.Auth.Domain.Entities.UserRoles;
using ShopDemo.Auth.Domain.Entities.UserRoles.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Resolvers;
using ShopDemo.Auth.Domain.Resolvers.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Resolvers;

public class ClaimResolverTests : TestBase
{
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly Mock<IRoleClaimRepository> _roleClaimRepositoryMock;
    private readonly Mock<IRoleHierarchyRepository> _roleHierarchyRepositoryMock;
    private readonly Mock<IClaimRepository> _claimRepositoryMock;
    private readonly ClaimResolver _sut;

    public ClaimResolverTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
        _roleClaimRepositoryMock = new Mock<IRoleClaimRepository>();
        _roleHierarchyRepositoryMock = new Mock<IRoleHierarchyRepository>();
        _claimRepositoryMock = new Mock<IClaimRepository>();
        _sut = new ClaimResolver(
            _userRoleRepositoryMock.Object,
            _roleClaimRepositoryMock.Object,
            _roleHierarchyRepositoryMock.Object,
            _claimRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUserRoleRepository_ShouldThrow()
    {
        LogAct("Creating with null user role repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new ClaimResolver(
            null!, _roleClaimRepositoryMock.Object, _roleHierarchyRepositoryMock.Object, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullRoleClaimRepository_ShouldThrow()
    {
        LogAct("Creating with null role claim repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new ClaimResolver(
            _userRoleRepositoryMock.Object, null!, _roleHierarchyRepositoryMock.Object, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullRoleHierarchyRepository_ShouldThrow()
    {
        LogAct("Creating with null role hierarchy repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new ClaimResolver(
            _userRoleRepositoryMock.Object, _roleClaimRepositoryMock.Object, null!, _claimRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WithNullClaimRepository_ShouldThrow()
    {
        LogAct("Creating with null claim repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new ClaimResolver(
            _userRoleRepositoryMock.Object, _roleClaimRepositoryMock.Object, _roleHierarchyRepositoryMock.Object, null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIClaimResolver()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IClaimResolver>();
    }

    #endregion

    #region ResolveUserClaimsAsync Tests

    [Fact]
    public async Task ResolveUserClaimsAsync_WhenUserHasNoRoles_ShouldReturnAllClaimsDenied()
    {
        // Arrange
        LogArrange("Setting up user with no roles and two claims in the system");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        var claimReadId = Id.GenerateNewId();
        var claimWriteId = Id.GenerateNewId();
        var claimRead = CreateTestClaim(claimReadId, "read:users");
        var claimWrite = CreateTestClaim(claimWriteId, "write:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claimRead, claimWrite });

        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole>());

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy>());

        // Act
        LogAct("Resolving claims for user with no roles");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying all claims are denied");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result["read:users"].IsDenied.ShouldBeTrue();
        result["write:users"].IsDenied.ShouldBeTrue();
    }

    [Fact]
    public async Task ResolveUserClaimsAsync_WhenUserHasRole_ShouldResolveDirectClaims()
    {
        // Arrange
        LogArrange("Setting up user with one role that has granted claims");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var roleId = Id.GenerateNewId();
        var claimReadId = Id.GenerateNewId();
        var claimWriteId = Id.GenerateNewId();

        var claimRead = CreateTestClaim(claimReadId, "read:users");
        var claimWrite = CreateTestClaim(claimWriteId, "write:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claimRead, claimWrite });

        var userRole = CreateTestUserRole(userId, roleId);
        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole> { userRole });

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy>());

        var roleClaims = new List<RoleClaim>
        {
            CreateTestRoleClaim(roleId, claimReadId, ClaimValue.Granted),
            CreateTestRoleClaim(roleId, claimWriteId, ClaimValue.Denied)
        };

        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleClaims);

        // Act
        LogAct("Resolving claims for user with direct role claims");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying claims are resolved from role");
        result.ShouldNotBeNull();
        result["read:users"].IsGranted.ShouldBeTrue();
        result["write:users"].IsDenied.ShouldBeTrue();
    }

    [Fact]
    public async Task ResolveUserClaimsAsync_WhenMultipleRoles_ShouldTakeMostRestrictive()
    {
        // Arrange
        LogArrange("Setting up user with two roles: one grants, other denies");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var roleAdminId = Id.GenerateNewId();
        var roleReaderId = Id.GenerateNewId();
        var claimWriteId = Id.GenerateNewId();

        var claimWrite = CreateTestClaim(claimWriteId, "write:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claimWrite });

        var userRoles = new List<UserRole>
        {
            CreateTestUserRole(userId, roleAdminId),
            CreateTestUserRole(userId, roleReaderId)
        };

        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRoles);

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy>());

        // Admin grants write, Reader denies write
        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleAdminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(roleAdminId, claimWriteId, ClaimValue.Granted) });

        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleReaderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(roleReaderId, claimWriteId, ClaimValue.Denied) });

        // Act
        LogAct("Resolving claims with conflicting roles (Min semantics)");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the most restrictive claim wins (Denied < Granted)");
        result.ShouldNotBeNull();
        result["write:users"].IsDenied.ShouldBeTrue();
    }

    [Fact]
    public async Task ResolveUserClaimsAsync_WithHierarchy_ShouldInheritFromParentRole()
    {
        // Arrange
        LogArrange("Setting up role hierarchy: childRole inherits from parentRole");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var childRoleId = Id.GenerateNewId();
        var parentRoleId = Id.GenerateNewId();
        var claimReadId = Id.GenerateNewId();

        var claimRead = CreateTestClaim(claimReadId, "read:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claimRead });

        var userRole = CreateTestUserRole(userId, childRoleId);
        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole> { userRole });

        // Child role has parent
        var hierarchy = CreateTestRoleHierarchy(childRoleId, parentRoleId);
        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchy });

        // Child role has inherited claim, parent role has granted claim
        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, childRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(childRoleId, claimReadId, ClaimValue.Inherited) });

        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, parentRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(parentRoleId, claimReadId, ClaimValue.Granted) });

        // Act
        LogAct("Resolving claims with hierarchy (child inherits from parent)");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying inherited claim is resolved from parent");
        result.ShouldNotBeNull();
        result["read:users"].IsGranted.ShouldBeTrue();
    }

    [Fact]
    public async Task ResolveUserClaimsAsync_WithInheritedAndNoParent_ShouldDeny()
    {
        // Arrange
        LogArrange("Setting up role with inherited claim but no parent role");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var roleId = Id.GenerateNewId();
        var claimReadId = Id.GenerateNewId();

        var claimRead = CreateTestClaim(claimReadId, "read:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claimRead });

        var userRole = CreateTestUserRole(userId, roleId);
        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole> { userRole });

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy>());

        // Role has inherited claim but no parent to inherit from
        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(roleId, claimReadId, ClaimValue.Inherited) });

        // Act
        LogAct("Resolving inherited claim with no parent (should deny)");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying inherited claim without parent falls back to Denied");
        result.ShouldNotBeNull();
        result["read:users"].IsDenied.ShouldBeTrue();
    }

    [Fact]
    public async Task ResolveUserClaimsAsync_WhenClaimNotInDictionary_ShouldSkip()
    {
        // Arrange
        LogArrange("Setting up role with a claim that doesn't exist in global claim list");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var roleId = Id.GenerateNewId();
        var knownClaimId = Id.GenerateNewId();
        var unknownClaimId = Id.GenerateNewId();

        var knownClaim = CreateTestClaim(knownClaimId, "read:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { knownClaim });

        var userRole = CreateTestUserRole(userId, roleId);
        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole> { userRole });

        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy>());

        // Role has claims: one known, one unknown
        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim>
            {
                CreateTestRoleClaim(roleId, knownClaimId, ClaimValue.Granted),
                CreateTestRoleClaim(roleId, unknownClaimId, ClaimValue.Granted)
            });

        // Act
        LogAct("Resolving claims where one role claim references non-existent global claim");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying unknown claim is skipped, known claim is resolved");
        result.ShouldNotBeNull();
        result["read:users"].IsGranted.ShouldBeTrue();
        result.ShouldNotContainKey("unknown-claim");
    }

    [Fact]
    public async Task ResolveUserClaimsAsync_WithMultipleParents_ShouldMergeInheritedClaims()
    {
        // Arrange
        LogArrange("Setting up role with two parent roles (diamond inheritance)");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var childRoleId = Id.GenerateNewId();
        var parentRole1Id = Id.GenerateNewId();
        var parentRole2Id = Id.GenerateNewId();
        var claimReadId = Id.GenerateNewId();

        var claimRead = CreateTestClaim(claimReadId, "read:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claimRead });

        var userRole = CreateTestUserRole(userId, childRoleId);
        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole> { userRole });

        // Child role has two parents
        var hierarchy1 = CreateTestRoleHierarchy(childRoleId, parentRole1Id);
        var hierarchy2 = CreateTestRoleHierarchy(childRoleId, parentRole2Id);
        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchy1, hierarchy2 });

        // Child has inherited claim
        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, childRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(childRoleId, claimReadId, ClaimValue.Inherited) });

        // Parent1 grants, Parent2 denies → Min should be Denied
        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, parentRole1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(parentRole1Id, claimReadId, ClaimValue.Granted) });

        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, parentRole2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(parentRole2Id, claimReadId, ClaimValue.Denied) });

        // Act
        LogAct("Resolving claims with multiple parents (most restrictive wins)");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the most restrictive inherited value is applied (Denied)");
        result.ShouldNotBeNull();
        result["read:users"].IsDenied.ShouldBeTrue();
    }

    [Fact]
    public async Task ResolveUserClaimsAsync_DirectClaimOverridesInherited()
    {
        // Arrange
        LogArrange("Setting up child role with direct (non-inherited) claim and parent with different value");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var childRoleId = Id.GenerateNewId();
        var parentRoleId = Id.GenerateNewId();
        var claimReadId = Id.GenerateNewId();

        var claimRead = CreateTestClaim(claimReadId, "read:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claimRead });

        var userRole = CreateTestUserRole(userId, childRoleId);
        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole> { userRole });

        var hierarchy = CreateTestRoleHierarchy(childRoleId, parentRoleId);
        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchy });

        // Child has direct Granted claim (not inherited), Parent has Denied
        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, childRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(childRoleId, claimReadId, ClaimValue.Granted) });

        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, parentRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(parentRoleId, claimReadId, ClaimValue.Denied) });

        // Act
        LogAct("Resolving claims where direct claim should override inherited");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying direct claim overrides inherited (Granted wins over parent's Denied)");
        result.ShouldNotBeNull();
        result["read:users"].IsGranted.ShouldBeTrue();
    }

    [Fact]
    public async Task ResolveUserClaimsAsync_ParentClaimNotInChild_ShouldInherit()
    {
        // Arrange
        LogArrange("Setting up parent role with a claim that child doesn't have");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var childRoleId = Id.GenerateNewId();
        var parentRoleId = Id.GenerateNewId();
        var claimReadId = Id.GenerateNewId();

        var claimRead = CreateTestClaim(claimReadId, "read:users");

        _claimRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Claim> { claimRead });

        var userRole = CreateTestUserRole(userId, childRoleId);
        _userRoleRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole> { userRole });

        var hierarchy = CreateTestRoleHierarchy(childRoleId, parentRoleId);
        _roleHierarchyRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchy> { hierarchy });

        // Child has no claims, parent has Granted
        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, childRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim>());

        _roleClaimRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, parentRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim> { CreateTestRoleClaim(parentRoleId, claimReadId, ClaimValue.Granted) });

        // Act
        LogAct("Resolving claims where parent has a claim child doesn't reference");
        var result = await _sut.ResolveUserClaimsAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying claim is inherited from parent when child has none");
        result.ShouldNotBeNull();
        result["read:users"].IsGranted.ShouldBeTrue();
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

    private static EntityInfo CreateTestEntityInfoWithId(Id id)
    {
        return EntityInfo.CreateFromExistingInfo(
            id: id,
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

    private static Claim CreateTestClaim(Id claimId, string name)
    {
        return Claim.CreateFromExistingInfo(new CreateFromExistingInfoClaimInput(
            CreateTestEntityInfoWithId(claimId), name, null));
    }

    private static UserRole CreateTestUserRole(Id userId, Id roleId)
    {
        return UserRole.CreateFromExistingInfo(new CreateFromExistingInfoUserRoleInput(
            CreateTestEntityInfo(), userId, roleId));
    }

    private static RoleClaim CreateTestRoleClaim(Id roleId, Id claimId, ClaimValue value)
    {
        return RoleClaim.CreateFromExistingInfo(new CreateFromExistingInfoRoleClaimInput(
            CreateTestEntityInfo(), roleId, claimId, value));
    }

    private static RoleHierarchy CreateTestRoleHierarchy(Id roleId, Id parentRoleId)
    {
        return RoleHierarchy.CreateFromExistingInfo(new CreateFromExistingInfoRoleHierarchyInput(
            CreateTestEntityInfo(), roleId, parentRoleId));
    }

    #endregion
}
