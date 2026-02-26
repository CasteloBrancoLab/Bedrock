using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using Moq;
using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;
using ShopDemo.Auth.Domain.Entities.Tenants.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class TenantResolverTests : TestBase
{
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly TenantResolver _sut;

    public TenantResolverTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _sut = new TenantResolver(_tenantRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act & Assert
        LogAct("Creating with null tenant repository");
        LogAssert("Verifying ArgumentNullException");
        Should.Throw<ArgumentNullException>(() => new TenantResolver(null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ShouldImplementITenantResolver()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<ITenantResolver>();
    }

    #endregion

    #region ResolveByDomainAsync Tests

    [Fact]
    public async Task ResolveByDomainAsync_WhenTenantNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository to return null for unknown domain");
        var executionContext = CreateTestExecutionContext();

        _tenantRepositoryMock
            .Setup(x => x.GetByDomainAsync(executionContext, "unknown.example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        LogAct("Resolving tenant by unknown domain");
        var result = await _sut.ResolveByDomainAsync(executionContext, "unknown.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveByDomainAsync_WhenTenantIsSuspended_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository to return a suspended tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(TenantStatus.Suspended);

        _tenantRepositoryMock
            .Setup(x => x.GetByDomainAsync(executionContext, "suspended.example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        LogAct("Resolving tenant by domain with suspended status");
        var result = await _sut.ResolveByDomainAsync(executionContext, "suspended.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned for non-active tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveByDomainAsync_WhenTenantIsInMaintenance_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up repository to return a tenant in maintenance");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(TenantStatus.Maintenance);

        _tenantRepositoryMock
            .Setup(x => x.GetByDomainAsync(executionContext, "maintenance.example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        LogAct("Resolving tenant by domain with maintenance status");
        var result = await _sut.ResolveByDomainAsync(executionContext, "maintenance.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying null is returned for non-active tenant");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ResolveByDomainAsync_WhenTenantIsActive_ShouldReturnTenant()
    {
        // Arrange
        LogArrange("Setting up repository to return an active tenant");
        var executionContext = CreateTestExecutionContext();
        var tenant = CreateTestTenant(TenantStatus.Active);

        _tenantRepositoryMock
            .Setup(x => x.GetByDomainAsync(executionContext, "active.example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        LogAct("Resolving tenant by domain with active status");
        var result = await _sut.ResolveByDomainAsync(executionContext, "active.example.com", CancellationToken.None);

        // Assert
        LogAssert("Verifying the active tenant is returned");
        result.ShouldNotBeNull();
        result.ShouldBeSameAs(tenant);
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

    private static Tenant CreateTestTenant(TenantStatus status)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
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

        return Tenant.CreateFromExistingInfo(new CreateFromExistingInfoTenantInput(
            entityInfo,
            "Test Corp",
            "test.example.com",
            "test_schema",
            status,
            TenantTier.Professional,
            null));
    }

    #endregion
}
