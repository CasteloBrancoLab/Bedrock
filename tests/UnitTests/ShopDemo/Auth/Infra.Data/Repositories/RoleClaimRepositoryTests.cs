using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Domain.Entities.RoleClaims.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class RoleClaimRepositoryTests : TestBase
{
    private readonly Mock<ILogger<RoleClaimRepository>> _loggerMock;
    private readonly Mock<IRoleClaimPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly RoleClaimRepository _sut;

    public RoleClaimRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<RoleClaimRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IRoleClaimPostgreSqlRepository>();
        _sut = new RoleClaimRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating RoleClaimRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new RoleClaimRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating RoleClaimRepository with valid parameters");
        var repository = new RoleClaimRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByRoleIdAsync Tests

    [Fact]
    public async Task GetByRoleIdAsync_WhenFound_ShouldReturnList()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of role claims");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expectedList = new List<RoleClaim> { CreateTestRoleClaim(executionContext) };

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        // Act
        LogAct("Calling GetByRoleIdAsync with existing roleId");
        var result = await _sut.GetByRoleIdAsync(executionContext, roleId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the list was returned");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByRoleIdAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleClaim>());

        // Act
        LogAct("Calling GetByRoleIdAsync with roleId having no claims");
        var result = await _sut.GetByRoleIdAsync(executionContext, roleId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByRoleIdAsync_WhenException_ShouldReturnEmptyAndLogError()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAsync(executionContext, roleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByRoleIdAsync when repository throws");
        var result = await _sut.GetByRoleIdAsync(executionContext, roleId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned and error was logged");
        result.ShouldBeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetByRoleIdAndClaimIdAsync Tests

    [Fact]
    public async Task GetByRoleIdAndClaimIdAsync_WhenFound_ShouldReturnRoleClaim()
    {
        // Arrange
        LogArrange("Setting up mock to return a role claim");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var expectedEntity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAndClaimIdAsync(executionContext, roleId, claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEntity);

        // Act
        LogAct("Calling GetByRoleIdAndClaimIdAsync with existing ids");
        var result = await _sut.GetByRoleIdAndClaimIdAsync(executionContext, roleId, claimId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the role claim was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedEntity);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByRoleIdAndClaimIdAsync(executionContext, roleId, claimId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByRoleIdAndClaimIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAndClaimIdAsync(executionContext, roleId, claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleClaim?)null);

        // Act
        LogAct("Calling GetByRoleIdAndClaimIdAsync with non-existing ids");
        var result = await _sut.GetByRoleIdAndClaimIdAsync(executionContext, roleId, claimId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByRoleIdAndClaimIdAsync_WhenException_ShouldReturnNullAndLogError()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception");
        var executionContext = CreateTestExecutionContext();
        var roleId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var claimId = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByRoleIdAndClaimIdAsync(executionContext, roleId, claimId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByRoleIdAndClaimIdAsync when repository throws");
        var result = await _sut.GetByRoleIdAndClaimIdAsync(executionContext, roleId, claimId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned and error was logged");
        result.ShouldBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync");
        var result = await _sut.UpdateAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, entity, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling UpdateAsync when persistence fails");
        var result = await _sut.UpdateAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WhenException_ShouldReturnFalseAndLogError()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception for UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling UpdateAsync when repository throws");
        var result = await _sut.UpdateAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned and error was logged");
        result.ShouldBeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for DeleteAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling DeleteAsync");
        var result = await _sut.DeleteAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.DeleteAsync(executionContext, entity, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for DeleteAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling DeleteAsync when entity not found");
        var result = await _sut.DeleteAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenException_ShouldReturnFalseAndLogError()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception for DeleteAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling DeleteAsync when repository throws");
        var result = await _sut.DeleteAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned and error was logged");
        result.ShouldBeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ExistsAsync (via RepositoryBase) Tests

    [Fact]
    public async Task ExistsAsync_WhenFound_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for ExistsAsync");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling ExistsAsync through RepositoryBase public API");
        var result = await _sut.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenNotFound_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for ExistsAsync");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling ExistsAsync with non-existing ID");
        var result = await _sut.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    #endregion

    #region GetByIdAsync (via RepositoryBase) Tests

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnRoleClaim()
    {
        // Arrange
        LogArrange("Setting up mock to return a role claim for GetByIdAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);
        var id = entity.EntityInfo.Id;

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the role claim was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(entity);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for GetByIdAsync");
        var executionContext = CreateTestExecutionContext();
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleClaim?)null);

        // Act
        LogAct("Calling GetByIdAsync with non-existing ID");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    #endregion

    #region RegisterNewAsync (via RepositoryBase) Tests

    [Fact]
    public async Task RegisterNewAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for RegisterNewAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, entity, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRoleClaim(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, entity, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    #endregion

    #region EnumerateAllAsync (via RepositoryBase) Tests

    [Fact]
    public async Task EnumerateAllAsync_ShouldReturnTrueWithNoItems()
    {
        // Arrange
        LogArrange("Setting up EnumerateAllAsync test - GetAllInternalAsync does yield break");
        var executionContext = CreateTestExecutionContext();
        var paginationInfo = PaginationInfo.All;
        var itemsReceived = new List<RoleClaim>();

        EnumerateAllItemHandler<RoleClaim> handler = (ctx, item, pagination, ct) =>
        {
            itemsReceived.Add(item);
            return Task.FromResult(true);
        };

        // Act
        LogAct("Calling EnumerateAllAsync through RepositoryBase public API");
        var result = await _sut.EnumerateAllAsync(
            executionContext,
            paginationInfo,
            handler,
            CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and no items were yielded");
        result.ShouldBeTrue();
        itemsReceived.ShouldBeEmpty();
    }

    #endregion

    #region EnumerateModifiedSinceAsync (via RepositoryBase) Tests

    [Fact]
    public async Task EnumerateModifiedSinceAsync_ShouldReturnTrueWithNoItems()
    {
        // Arrange
        LogArrange("Setting up EnumerateModifiedSinceAsync test - GetModifiedSinceInternalAsync does yield break");
        var executionContext = CreateTestExecutionContext();
        var timeProvider = TimeProvider.System;
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var itemsReceived = new List<RoleClaim>();

        EnumerateModifiedSinceItemHandler<RoleClaim> handler = (ctx, item, tp, s, ct) =>
        {
            itemsReceived.Add(item);
            return Task.FromResult(true);
        };

        // Act
        LogAct("Calling EnumerateModifiedSinceAsync through RepositoryBase public API");
        var result = await _sut.EnumerateModifiedSinceAsync(
            executionContext,
            timeProvider,
            since,
            handler,
            CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and no items were yielded");
        result.ShouldBeTrue();
        itemsReceived.ShouldBeEmpty();
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid());
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    private static RoleClaim CreateTestRoleClaim(ExecutionContext executionContext)
    {
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: executionContext.TenantInfo,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: executionContext.ExecutionUser,
            createdCorrelationId: executionContext.CorrelationId,
            createdExecutionOrigin: executionContext.ExecutionOrigin,
            createdBusinessOperationCode: executionContext.BusinessOperationCode,
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(1));

        return RoleClaim.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleClaimInput(
                entityInfo,
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                Id.CreateFromExistingInfo(Guid.NewGuid()),
                ClaimValue.Granted));
    }

    #endregion
}
