using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class KeyChainRepositoryTests : TestBase
{
    private readonly Mock<ILogger<KeyChainRepository>> _loggerMock;
    private readonly Mock<IKeyChainPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly KeyChainRepository _sut;

    public KeyChainRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<KeyChainRepository>>();
        _loggerMock.Setup(static x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _postgreSqlRepositoryMock = new Mock<IKeyChainPostgreSqlRepository>();
        _sut = new KeyChainRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating KeyChainRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new KeyChainRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating KeyChainRepository with valid parameters");
        var repository = new KeyChainRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenFound_ShouldReturnKeyChains()
    {
        // Arrange
        LogArrange("Setting up mock to return a list of key chains for user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var expectedKeyChain = CreateTestKeyChain(executionContext);
        IReadOnlyList<KeyChain> expectedList = [expectedKeyChain];

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        // Act
        LogAct("Calling GetByUserIdAsync with existing user ID");
        var result = await _sut.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the key chains list was returned");
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(1);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to return empty list for user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        LogAct("Calling GetByUserIdAsync when no key chains exist for user");
        var result = await _sut.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

        // Assert
        LogAssert("Verifying empty list was returned");
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenException_ShouldReturnEmpty()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on user ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAsync(executionContext, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByUserIdAsync when repository throws");
        var result = await _sut.GetByUserIdAsync(executionContext, userId, CancellationToken.None);

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

    #region GetByUserIdAndKeyIdAsync Tests

    [Fact]
    public async Task GetByUserIdAndKeyIdAsync_WhenFound_ShouldReturnKeyChain()
    {
        // Arrange
        LogArrange("Setting up mock to return a key chain for user ID and key ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var keyId = KeyId.CreateNew("test-key-id");
        var expectedKeyChain = CreateTestKeyChain(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedKeyChain);

        // Act
        LogAct("Calling GetByUserIdAndKeyIdAsync with existing user ID and key ID");
        var result = await _sut.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, CancellationToken.None);

        // Assert
        LogAssert("Verifying the key chain was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedKeyChain);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAndKeyIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for user ID and key ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var keyId = KeyId.CreateNew("nonexistent-key-id");

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((KeyChain?)null);

        // Act
        LogAct("Calling GetByUserIdAndKeyIdAsync with non-existing user ID and key ID");
        var result = await _sut.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUserIdAndKeyIdAsync_WhenException_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on user ID and key ID lookup");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();
        var keyId = KeyId.CreateNew("error-key-id");

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByUserIdAndKeyIdAsync when repository throws");
        var result = await _sut.GetByUserIdAndKeyIdAsync(executionContext, userId, keyId, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned after exception and error was logged");
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
        var keyChain = CreateTestKeyChain(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, keyChain, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling UpdateAsync with a key chain");
        var result = await _sut.UpdateAsync(executionContext, keyChain, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.UpdateAsync(executionContext, keyChain, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var keyChain = CreateTestKeyChain(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, keyChain, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling UpdateAsync when update fails");
        var result = await _sut.UpdateAsync(executionContext, keyChain, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WhenException_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on UpdateAsync");
        var executionContext = CreateTestExecutionContext();
        var keyChain = CreateTestKeyChain(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.UpdateAsync(executionContext, keyChain, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling UpdateAsync when repository throws");
        var result = await _sut.UpdateAsync(executionContext, keyChain, CancellationToken.None);

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

    #region DeleteExpiredAsync Tests

    [Fact]
    public async Task DeleteExpiredAsync_WhenSuccessful_ShouldReturnDeletedCount()
    {
        // Arrange
        LogArrange("Setting up mock to return deleted count for DeleteExpiredAsync");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;
        int expectedCount = 3;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        // Act
        LogAct("Calling DeleteExpiredAsync with reference date");
        var result = await _sut.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verifying the deleted count was returned and repository was called");
        result.ShouldBe(expectedCount);
        _postgreSqlRepositoryMock.Verify(
            x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WhenNothingToDelete_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Setting up mock to return zero for DeleteExpiredAsync when no expired key chains");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        LogAct("Calling DeleteExpiredAsync when no expired key chains exist");
        var result = await _sut.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero was returned");
        result.ShouldBe(0);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WhenException_ShouldReturnZero()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on DeleteExpiredAsync");
        var executionContext = CreateTestExecutionContext();
        var referenceDate = DateTimeOffset.UtcNow;

        _postgreSqlRepositoryMock
            .Setup(x => x.DeleteExpiredAsync(executionContext, referenceDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling DeleteExpiredAsync when repository throws");
        var result = await _sut.DeleteExpiredAsync(executionContext, referenceDate, CancellationToken.None);

        // Assert
        LogAssert("Verifying zero was returned after exception and error was logged");
        result.ShouldBe(0);
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
    public async Task GetByIdAsync_WhenFound_ShouldReturnKeyChain()
    {
        // Arrange
        LogArrange("Setting up mock to return a key chain for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var expectedKeyChain = CreateTestKeyChain(executionContext);
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedKeyChain);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the key chain was returned and repository was called");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedKeyChain);
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
            .ReturnsAsync((KeyChain?)null);

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
        var keyChain = CreateTestKeyChain(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, keyChain, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, keyChain, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, keyChain, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var keyChain = CreateTestKeyChain(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, keyChain, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, keyChain, CancellationToken.None);

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
        var itemsReceived = new List<KeyChain>();

        EnumerateAllItemHandler<KeyChain> handler = (ctx, item, pagination, ct) =>
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
        var itemsReceived = new List<KeyChain>();

        EnumerateModifiedSinceItemHandler<KeyChain> handler = (ctx, item, tp, s, ct) =>
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

    private static KeyChain CreateTestKeyChain(ExecutionContext executionContext)
    {
        var input = new RegisterNewKeyChainInput(
            UserId: Id.GenerateNewId(),
            KeyId: KeyId.CreateNew("test-key-id"),
            PublicKey: "test-public-key",
            EncryptedSharedSecret: "test-encrypted-shared-secret",
            ExpiresAt: DateTimeOffset.UtcNow.AddDays(30));
        return KeyChain.RegisterNew(executionContext, input)!;
    }

    #endregion
}
