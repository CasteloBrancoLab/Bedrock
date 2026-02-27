using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class ClaimDependencyRepositoryTests : TestBase
{
    private readonly Mock<ILogger<ClaimDependencyRepository>> _loggerMock;
    private readonly Mock<IClaimDependencyPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly ClaimDependencyRepository _sut;

    public ClaimDependencyRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<ClaimDependencyRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IClaimDependencyPostgreSqlRepository>();
        _sut = new ClaimDependencyRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating ClaimDependencyRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new ClaimDependencyRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating ClaimDependencyRepository with valid parameters");
        var repository = new ClaimDependencyRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByClaimIdAsync Tests

    [Fact]
    public async Task GetByClaimIdAsync_WhenFound_ShouldReturnClaimDependencies()
    {
        // Arrange
        LogArrange("Setting up mock to return claim dependencies for claim ID lookup");
        var executionContext = CreateTestExecutionContext();
        var claimId = Id.GenerateNewId();
        var expectedDependency = CreateTestClaimDependency(executionContext);
        IReadOnlyList<ClaimDependency> expectedList = [expectedDependency];

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByClaimIdAsync(executionContext, claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        // Act
        LogAct("Calling GetByClaimIdAsync with existing claim ID");
        var result = await _sut.GetByClaimIdAsync(executionContext, claimId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the claim dependencies list was returned");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByClaimIdAsync(executionContext, claimId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByClaimIdAsync_WhenNotFound_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list for claim ID lookup");
        var executionContext = CreateTestExecutionContext();
        var claimId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByClaimIdAsync(executionContext, claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Calling GetByClaimIdAsync with non-existing claim ID");
        var result = await _sut.GetByClaimIdAsync(executionContext, claimId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByClaimIdAsync_WhenException_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on claim ID lookup");
        var executionContext = CreateTestExecutionContext();
        var claimId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByClaimIdAsync(executionContext, claimId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByClaimIdAsync when repository throws");
        var result = await _sut.GetByClaimIdAsync(executionContext, claimId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned after exception and error was logged");
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

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenFound_ShouldReturnClaimDependencies()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of claim dependencies");
        var executionContext = CreateTestExecutionContext();
        var expectedDependency = CreateTestClaimDependency(executionContext);
        IReadOnlyList<ClaimDependency> expectedList = [expectedDependency];

        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        // Act
        LogAct("Calling GetAllAsync");
        var result = await _sut.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying the claim dependencies list was returned");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list");
        var executionContext = CreateTestExecutionContext();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Calling GetAllAsync when no dependencies exist");
        var result = await _sut.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenException_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on GetAllAsync");
        var executionContext = CreateTestExecutionContext();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetAllAsync(executionContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetAllAsync when repository throws");
        var result = await _sut.GetAllAsync(executionContext, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned after exception and error was logged");
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

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for DeleteAsync");
        var executionContext = CreateTestExecutionContext();
        var dependency = CreateTestClaimDependency(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteAsync(executionContext, dependency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling DeleteAsync with valid claim dependency");
        var result = await _sut.DeleteAsync(executionContext, dependency, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.DeleteAsync(executionContext, dependency, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for DeleteAsync");
        var executionContext = CreateTestExecutionContext();
        var dependency = CreateTestClaimDependency(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteAsync(executionContext, dependency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling DeleteAsync when delete fails");
        var result = await _sut.DeleteAsync(executionContext, dependency, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenException_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on DeleteAsync");
        var executionContext = CreateTestExecutionContext();
        var dependency = CreateTestClaimDependency(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteAsync(executionContext, dependency, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling DeleteAsync when repository throws");
        var result = await _sut.DeleteAsync(executionContext, dependency, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned after exception and error was logged");
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
        LogArrange("Setting up mock to return true for ExistsAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling ExistsAsync through RepositoryBase public API");
        var result = await _sut.ExistsAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.ExistsAsync(executionContext, id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenNotFound_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for ExistsAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var id = Id.GenerateNewId();

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
    public async Task GetByIdAsync_WhenFound_ShouldReturnClaimDependency()
    {
        // Arrange
        LogArrange("Setting up mock to return a claim dependency for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var expectedDependency = CreateTestClaimDependency(executionContext);
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDependency);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the claim dependency was returned and repository was called");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedDependency);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimDependency?)null);

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
        LogArrange("Setting up mock to return true for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var dependency = CreateTestClaimDependency(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, dependency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, dependency, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, dependency, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var dependency = CreateTestClaimDependency(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, dependency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, dependency, CancellationToken.None);

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
        var itemsReceived = new List<ClaimDependency>();

        EnumerateAllItemHandler<ClaimDependency> handler = (ctx, item, pagination, ct) =>
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
        var itemsReceived = new List<ClaimDependency>();

        EnumerateModifiedSinceItemHandler<ClaimDependency> handler = (ctx, item, tp, s, ct) =>
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

    private static ClaimDependency CreateTestClaimDependency(ExecutionContext executionContext)
    {
        var input = new RegisterNewClaimDependencyInput(
            ClaimId: Id.GenerateNewId(),
            DependsOnClaimId: Id.GenerateNewId());
        return ClaimDependency.RegisterNew(executionContext, input)!;
    }

    #endregion
}
