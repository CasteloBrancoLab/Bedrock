using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Infra.Data.Repositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Infra.Data.Repositories;

public class UserRepositoryTests : TestBase
{
    private readonly Mock<ILogger<UserRepository>> _loggerMock;
    private readonly Mock<IUserPostgreSqlRepository> _postgreSqlRepositoryMock;
    private readonly UserRepository _sut;

    public UserRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _loggerMock = new Mock<ILogger<UserRepository>>();
        _postgreSqlRepositoryMock = new Mock<IUserPostgreSqlRepository>();
        _sut = new UserRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullPostgreSqlRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        LogArrange("Preparing null PostgreSQL repository");

        // Act & Assert
        LogAct("Creating UserRepository with null postgreSqlRepository");
        LogAssert("Verifying ArgumentNullException is thrown");
        Should.Throw<ArgumentNullException>(
            () => new UserRepository(_loggerMock.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing valid logger and PostgreSQL repository mocks");

        // Act
        LogAct("Creating UserRepository with valid parameters");
        var repository = new UserRepository(_loggerMock.Object, _postgreSqlRepositoryMock.Object);

        // Assert
        LogAssert("Verifying no exception was thrown");
        repository.ShouldNotBeNull();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_WhenFound_ShouldReturnUser()
    {
        // Arrange
        LogArrange("Setting up mock to return a user for email lookup");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("found@example.com");
        var expectedUser = CreateTestUser(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByEmailAsync(executionContext, email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        LogAct("Calling GetByEmailAsync with existing email");
        var result = await _sut.GetByEmailAsync(executionContext, email, CancellationToken.None);

        // Assert
        LogAssert("Verifying the user was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedUser);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByEmailAsync(executionContext, email, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for email lookup");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("notfound@example.com");

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByEmailAsync(executionContext, email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        LogAct("Calling GetByEmailAsync with non-existing email");
        var result = await _sut.GetByEmailAsync(executionContext, email, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenException_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on email lookup");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("error@example.com");

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByEmailAsync(executionContext, email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByEmailAsync when repository throws");
        var result = await _sut.GetByEmailAsync(executionContext, email, CancellationToken.None);

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

    #region GetByUsernameAsync Tests

    [Fact]
    public async Task GetByUsernameAsync_WhenFound_ShouldReturnUser()
    {
        // Arrange
        LogArrange("Setting up mock to return a user for username lookup");
        var executionContext = CreateTestExecutionContext();
        string username = "existinguser";
        var expectedUser = CreateTestUser(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUsernameAsync(executionContext, username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        LogAct("Calling GetByUsernameAsync with existing username");
        var result = await _sut.GetByUsernameAsync(executionContext, username, CancellationToken.None);

        // Assert
        LogAssert("Verifying the user was returned");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedUser);
        _postgreSqlRepositoryMock.Verify(
            x => x.GetByUsernameAsync(executionContext, username, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to return null for username lookup");
        var executionContext = CreateTestExecutionContext();
        string username = "nonexistentuser";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUsernameAsync(executionContext, username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        LogAct("Calling GetByUsernameAsync with non-existing username");
        var result = await _sut.GetByUsernameAsync(executionContext, username, CancellationToken.None);

        // Assert
        LogAssert("Verifying null was returned");
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenException_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on username lookup");
        var executionContext = CreateTestExecutionContext();
        string username = "erroruser";

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByUsernameAsync(executionContext, username, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling GetByUsernameAsync when repository throws");
        var result = await _sut.GetByUsernameAsync(executionContext, username, CancellationToken.None);

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

    #region ExistsByEmailAsync Tests

    [Fact]
    public async Task ExistsByEmailAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for email existence check");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("exists@example.com");

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(executionContext, email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling ExistsByEmailAsync with existing email");
        var result = await _sut.ExistsByEmailAsync(executionContext, email, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.ExistsByEmailAsync(executionContext, email, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for email existence check");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("notexists@example.com");

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(executionContext, email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling ExistsByEmailAsync with non-existing email");
        var result = await _sut.ExistsByEmailAsync(executionContext, email, CancellationToken.None);

        // Assert
        LogAssert("Verifying false was returned");
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenException_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on email existence check");
        var executionContext = CreateTestExecutionContext();
        var email = EmailAddress.CreateNew("error@example.com");

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(executionContext, email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling ExistsByEmailAsync when repository throws");
        var result = await _sut.ExistsByEmailAsync(executionContext, email, CancellationToken.None);

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

    #region ExistsByUsernameAsync Tests

    [Fact]
    public async Task ExistsByUsernameAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Setting up mock to return true for username existence check");
        var executionContext = CreateTestExecutionContext();
        string username = "existinguser";

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByUsernameAsync(executionContext, username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling ExistsByUsernameAsync with existing username");
        var result = await _sut.ExistsByUsernameAsync(executionContext, username, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.ExistsByUsernameAsync(executionContext, username, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsByUsernameAsync_WhenException_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to throw exception on username existence check");
        var executionContext = CreateTestExecutionContext();
        string username = "erroruser";

        _postgreSqlRepositoryMock
            .Setup(x => x.ExistsByUsernameAsync(executionContext, username, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        LogAct("Calling ExistsByUsernameAsync when repository throws");
        var result = await _sut.ExistsByUsernameAsync(executionContext, username, CancellationToken.None);

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
    public async Task GetByIdAsync_WhenFound_ShouldReturnUser()
    {
        // Arrange
        LogArrange("Setting up mock to return a user for GetByIdAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var expectedUser = CreateTestUser(executionContext);
        var id = Id.GenerateNewId();

        _postgreSqlRepositoryMock
            .Setup(x => x.GetByIdAsync(executionContext, id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        LogAct("Calling GetByIdAsync through RepositoryBase public API");
        var result = await _sut.GetByIdAsync(executionContext, id, CancellationToken.None);

        // Assert
        LogAssert("Verifying the user was returned and repository was called");
        result.ShouldNotBeNull();
        result.ShouldBe(expectedUser);
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
            .ReturnsAsync((User?)null);

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
        var user = CreateTestUser(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        LogAct("Calling RegisterNewAsync through RepositoryBase public API");
        var result = await _sut.RegisterNewAsync(executionContext, user, CancellationToken.None);

        // Assert
        LogAssert("Verifying true was returned and repository was called");
        result.ShouldBeTrue();
        _postgreSqlRepositoryMock.Verify(
            x => x.RegisterNewAsync(executionContext, user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterNewAsync_WhenFailed_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Setting up mock to return false for RegisterNewAsync via base class");
        var executionContext = CreateTestExecutionContext();
        var user = CreateTestUser(executionContext);

        _postgreSqlRepositoryMock
            .Setup(x => x.RegisterNewAsync(executionContext, user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        LogAct("Calling RegisterNewAsync when persistence fails");
        var result = await _sut.RegisterNewAsync(executionContext, user, CancellationToken.None);

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
        var itemsReceived = new List<User>();

        EnumerateAllItemHandler<User> handler = (ctx, item, pagination, ct) =>
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
        var itemsReceived = new List<User>();

        EnumerateModifiedSinceItemHandler<User> handler = (ctx, item, tp, s, ct) =>
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

    private static User CreateTestUser(ExecutionContext executionContext)
    {
        var email = EmailAddress.CreateNew("test@example.com");
        var passwordHash = PasswordHash.CreateNew(CreateValidHashBytes());
        var input = new RegisterNewInput(email, passwordHash);
        return User.RegisterNew(executionContext, input)!;
    }

    private static byte[] CreateValidHashBytes()
    {
        byte[] bytes = new byte[49];
        bytes[0] = 1;
        for (int i = 1; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i % 256);
        }
        return bytes;
    }

    #endregion
}
