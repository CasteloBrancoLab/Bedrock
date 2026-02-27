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
using ShopDemo.Auth.Domain.Entities.Roles;
using ShopDemo.Auth.Domain.Entities.Roles.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class RoleRepositoryTests : TestBase
{
    private readonly Mock<ILogger<RoleRepository>> _loggerMock;
    private readonly Mock<IRolePostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly RoleRepository _sut;

    public RoleRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<RoleRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IRolePostgreSqlRepository>();
        _sut = new RoleRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating RoleRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new RoleRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating RoleRepository with valid parameters");
        var repository = new RoleRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_WhenFound_ShouldReturnRole()
    {
        // Arrange
        LogArrange("Setting up mock to return a role for name lookup");
        var executionContext = CreateTestExecutionContext();
        string name = "Admin";
        var expectedRole = CreateTestRole(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByNameAsync(executionContext, name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRole);

        // Act
        LogAct("Calling GetByNameAsync with existing name");
        var result = await _sut.GetByNameAsync(executionContext, name, CancellationToken.None);

        // Assert
        LogAssert("Verifying the role was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedRole);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByNameAsync(executionContext, name, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByNameAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for name lookup");
        var executionContext = CreateTestExecutionContext();
        string name = "NonExistentRole";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByNameAsync(executionContext, name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        LogAct("Calling GetByNameAsync with non-existing name");
        var result = await _sut.GetByNameAsync(executionContext, name, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WhenException_ShouldReturnNullAndLogError()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on name lookup");
        var executionContext = CreateTestExecutionContext();
        string name = "ErrorRole";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByNameAsync(executionContext, name, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByNameAsync when repository throws");
        var result = await _sut.GetByNameAsync(executionContext, name, CancellationToken.None);

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

    #region ExistsByNameAsync Tests

    [Fact]
    public async Task ExistsByNameAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for name existence check");
        var executionContext = CreateTestExecutionContext();
        string name = "Admin";

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByNameAsync(executionContext, name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling ExistsByNameAsync with existing name");
        var result = await _sut.ExistsByNameAsync(executionContext, name, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.ExistsByNameAsync(executionContext, name, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for name existence check");
        var executionContext = CreateTestExecutionContext();
        string name = "NonExistentRole";

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByNameAsync(executionContext, name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling ExistsByNameAsync with non-existing name");
        var result = await _sut.ExistsByNameAsync(executionContext, name, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenException_ShouldReturnFalseAndLogError()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on name existence check");
        var executionContext = CreateTestExecutionContext();
        string name = "ErrorRole";

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByNameAsync(executionContext, name, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling ExistsByNameAsync when repository throws");
        var result = await _sut.ExistsByNameAsync(executionContext, name, CancellationToken.None);

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

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRole(executionContext);

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
        var entity = CreateTestRole(executionContext);

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
        var entity = CreateTestRole(executionContext);

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
        var entity = CreateTestRole(executionContext);

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
        var entity = CreateTestRole(executionContext);

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
        var entity = CreateTestRole(executionContext);

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
    public async Task GetByIdAsync_WhenFound_ShouldReturnRole()
    {
        // Arrange
        LogArrange("Setting up mock to return a role for GetByIdAsync");
        var executionContext = CreateTestExecutionContext();
        var entity = CreateTestRole(executionContext);
        var id = entity.EntityInfo.Id;

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the role was returned");
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
            .ReturnsAsync((Role?)null);

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
        var entity = CreateTestRole(executionContext);

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
        var entity = CreateTestRole(executionContext);

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
        var itemsReceived = new List<Role>();

        EnumerateAllItemHandler<Role> handler = (ctx, item, pagination, ct) =>
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
        var itemsReceived = new List<Role>();

        EnumerateModifiedSinceItemHandler<Role> handler = (ctx, item, tp, s, ct) =>
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

    private static Role CreateTestRole(ExecutionContext executionContext)
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

        return Role.CreateFromExistingInfo(
            new CreateFromExistingInfoRoleInput(
                entityInfo,
                "TestRole",
                null));
    }

    #endregion
}
